using System.Security.Cryptography;
using System.Text;

using GenHTTP.Lambda.Configuration;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Secrets;

/// <summary>
/// Encrypts the values of secrets, and reads them back.
/// </summary>
/// <remarks>
/// AES-256-GCM, which also says whether what it opens is what it sealed. The
/// key of a lambda is made from two things that are kept in different places:
/// the secret of the installation (<see cref="LambdaOptions.SecretsKey" />, or a
/// file beside the database), and the random salt the lambda has in its row.
/// The database alone therefore decrypts nothing, and neither does the secret
/// alone; moving to another server or restoring a backup takes both, which is
/// the database and whatever that secret was.
///
/// The name of a secret is bound to its value as associated data. A row whose
/// value was copied under another name - by somebody with the database, say -
/// does not open, so a secret cannot be made to answer for another.
///
/// What is stored is <c>version | nonce | tag | ciphertext</c>. The version is
/// there so that a later scheme can be told from this one without a migration
/// of everything that was written with it.
///
/// There is no rotation of the secret of the installation. Changing it makes
/// every secret unreadable, and a lambda that reads one is told so where it
/// asks. Rotating would be a job for the operator: read each, write each.
/// </remarks>
public sealed class SecretCipher
{

    /// <summary>
    /// How long the secret of the installation has to be, when it is chosen
    /// rather than made.
    /// </summary>
    public const int MinimumKeyLength = 32;

    private const byte Scheme = 1;

    private const int NonceSize = 12;

    private const int TagSize = 16;

    private const int KeySize = 32;

    private const int SaltSize = 32;

    private static readonly byte[] Purpose = "GenHTTP Lambda secrets, scheme 1"u8.ToArray();

    private readonly byte[] _master;

    #region Initialization

    /// <summary>
    /// Reads the secret of the installation, and makes one where there is none.
    /// </summary>
    public SecretCipher(LambdaOptions options, ILogger<SecretCipher> logger)
    {
        _master = Master(options, logger);
    }

    private static byte[] Master(LambdaOptions options, ILogger logger)
    {
        if (!string.IsNullOrWhiteSpace(options.SecretsKey))
        {
            var chosen = options.SecretsKey.Trim();

            if (chosen.Length < MinimumKeyLength)
            {
                throw new InvalidOperationException($"LAMBDA_SECRETS_KEY must be at least {MinimumKeyLength} characters long (openssl rand -base64 32 makes a good one).");
            }

            return Encoding.UTF8.GetBytes(chosen);
        }

        var file = options.SecretsKeyFile;

        if (File.Exists(file))
        {
            var kept = File.ReadAllText(file).Trim();

            if (kept.Length >= MinimumKeyLength)
            {
                return Encoding.UTF8.GetBytes(kept);
            }

            throw new InvalidOperationException($"The secret of this installation in {file} is too short to be one. Restore it, or set LAMBDA_SECRETS_KEY.");
        }

        var made = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));

        Directory.CreateDirectory(Path.GetDirectoryName(file)!);

        // created for nobody but the server, so it is never readable before it is restricted
        var mode = new FileStreamOptions
        {
            Mode = FileMode.CreateNew,
            Access = FileAccess.Write,
            Share = FileShare.None
        };

        if (!OperatingSystem.IsWindows())
        {
            mode.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
        }

        using (var stream = new FileStream(file, mode))
        {
            stream.Write(Encoding.UTF8.GetBytes(made));
        }

        logger.LogWarning("LAMBDA_SECRETS_KEY is not set, so a secret for this installation was made and kept in {File}. Secrets are encrypted with it and with a salt " +
                          "of their own in the database: back the two up together, or set LAMBDA_SECRETS_KEY and keep it apart from the database", file);

        return Encoding.UTF8.GetBytes(made);
    }

    #endregion

    #region Functionality

    /// <summary>
    /// A salt for a lambda that has none yet.
    /// </summary>
    public static byte[] NewSalt() => RandomNumberGenerator.GetBytes(SaltSize);

    /// <summary>
    /// The key a lambda's secrets are encrypted with.
    /// </summary>
    /// <param name="salt">The salt in the lambda's row</param>
    public byte[] KeyFor(byte[] salt) => HKDF.DeriveKey(HashAlgorithmName.SHA256, _master, KeySize, salt, Purpose);

    /// <summary>
    /// Seals a value.
    /// </summary>
    public static byte[] Seal(byte[] key, string name, string value)
    {
        var plain = Encoding.UTF8.GetBytes(value);

        var sealedValue = new byte[1 + NonceSize + TagSize + plain.Length];

        var nonce = sealedValue.AsSpan(1, NonceSize);
        var tag = sealedValue.AsSpan(1 + NonceSize, TagSize);
        var cipher = sealedValue.AsSpan(1 + NonceSize + TagSize);

        sealedValue[0] = Scheme;

        RandomNumberGenerator.Fill(nonce);

        using var aes = new AesGcm(key, TagSize);

        aes.Encrypt(nonce, plain, cipher, tag, Encoding.UTF8.GetBytes(name));

        CryptographicOperations.ZeroMemory(plain);

        return sealedValue;
    }

    /// <summary>
    /// Opens a value, or says that it cannot be opened.
    /// </summary>
    /// <returns>The value, or nothing if the key or the name is not the one it was sealed with</returns>
    public static string? Open(byte[] key, string name, byte[] sealedValue)
    {
        if (sealedValue.Length < 1 + NonceSize + TagSize || sealedValue[0] != Scheme)
        {
            return null;
        }

        var nonce = sealedValue.AsSpan(1, NonceSize);
        var tag = sealedValue.AsSpan(1 + NonceSize, TagSize);
        var cipher = sealedValue.AsSpan(1 + NonceSize + TagSize);

        var plain = new byte[cipher.Length];

        try
        {
            using var aes = new AesGcm(key, TagSize);

            aes.Decrypt(nonce, cipher, tag, plain, Encoding.UTF8.GetBytes(name));

            return Encoding.UTF8.GetString(plain);
        }
        catch (CryptographicException)
        {
            return null;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plain);
        }
    }

    #endregion

}
