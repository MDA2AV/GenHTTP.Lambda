using System.Security.Cryptography;
using System.Text;

using GenHTTP.Lambda.Configuration;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Secrets;

/// <summary>
/// Seals the values of secrets before they are stored, and opens them again
/// for the lambda that reads them.
/// </summary>
/// <remarks>
/// AES-256-GCM under a key made of two halves. One belongs to the
/// installation and lives outside the database (<see cref="LambdaOptions.SecretsKey"/>
/// or the key file beside it); the other belongs to the lambda and lives in
/// its row. HKDF makes one key of the two, so every lambda seals with a key of
/// its own, and neither half opens anything alone: the database without the
/// installation's key is a list of names, and the installation's key without
/// the database has nothing to open.
///
/// The name of a secret is bound to its value as associated data, so a value
/// copied under another name - or into another lambda, whose key differs - no
/// longer opens. A feature's copy keeps the lambda's key, which is what lets
/// it be copied without being opened.
///
/// None of this keeps a secret from the lambda it belongs to, which reads it
/// in the process this runs in; see the README on what the code guard is and
/// is not.
/// </remarks>
public sealed class SecretCipher
{

    /// <summary>
    /// The first byte of a sealed value: how it was sealed, so another way can
    /// be added later without guessing at what is stored.
    /// </summary>
    private const byte Format = 1;

    private const int KeySize = 32;

    private const int NonceSize = 12;

    private const int TagSize = 16;

    private static readonly byte[] Purpose = "genhttp-lambda secrets v1"u8.ToArray();

    private static readonly byte[] FingerprintPurpose = "genhttp-lambda secrets fingerprint"u8.ToArray();

    private readonly Lazy<byte[]> _master;

    #region Initialization

    public SecretCipher(LambdaOptions options, ILogger<SecretCipher> logger)
    {
        _master = new Lazy<byte[]>(() => LoadMaster(options, logger), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    #endregion

    #region Functionality

    /// <summary>
    /// A new half of a key, for a lambda that stores its first secret.
    /// </summary>
    public static byte[] NewSalt() => RandomNumberGenerator.GetBytes(KeySize);

    /// <summary>
    /// The key of one lambda, made of its half and the installation's.
    /// </summary>
    public byte[] KeyOf(byte[] salt) => HKDF.DeriveKey(HashAlgorithmName.SHA256, _master.Value, KeySize, salt, Purpose);

    /// <summary>
    /// Seals a value under the key of a lambda, bound to the name it is stored by.
    /// </summary>
    public byte[] Seal(byte[] key, string name, string value)
    {
        var plain = Encoding.UTF8.GetBytes(value);

        var sealedValue = new byte[1 + NonceSize + TagSize + plain.Length];

        sealedValue[0] = Format;

        var nonce = sealedValue.AsSpan(1, NonceSize);
        var tag = sealedValue.AsSpan(1 + NonceSize, TagSize);
        var cipher = sealedValue.AsSpan(1 + NonceSize + TagSize);

        RandomNumberGenerator.Fill(nonce);

        using var aes = new AesGcm(key, TagSize);

        aes.Encrypt(nonce, plain, cipher, tag, Encoding.UTF8.GetBytes(name));

        CryptographicOperations.ZeroMemory(plain);

        return sealedValue;
    }

    /// <summary>
    /// Opens a value sealed by <see cref="Seal"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// When it does not open: sealed with another key, which is what a server
    /// started with a key other than the one its database was written with sees.
    /// </exception>
    public string Open(byte[] key, string name, byte[] sealedValue)
    {
        if (sealedValue.Length < 1 + NonceSize + TagSize || sealedValue[0] != Format)
        {
            throw new InvalidOperationException($"The secret '{name}' is stored in a form this server does not know.");
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
        catch (AuthenticationTagMismatchException)
        {
            throw new InvalidOperationException($"The secret '{name}' cannot be opened with the key of this server. It was stored under another one: "
                                              + "start the server with the LAMBDA_SECRETS_KEY (or the secrets.key file) the database was written with, or store the secret again.");
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plain);
        }
    }

    /// <summary>
    /// What tells one installation key from another without saying anything
    /// about it, so a server can notice it was started with the wrong one.
    /// </summary>
    public string Fingerprint() => Convert.ToHexStringLower(HMACSHA256.HashData(_master.Value, FingerprintPurpose).AsSpan(0, 8));

    #endregion

    #region Master key

    private static byte[] LoadMaster(LambdaOptions options, ILogger logger)
    {
        if (options.SecretsKey is { } configured)
        {
            return FromSetting(configured);
        }

        var file = options.SecretsKeyFile;

        if (File.Exists(file))
        {
            return FromFile(file);
        }

        var key = RandomNumberGenerator.GetBytes(KeySize);

        Directory.CreateDirectory(Path.GetDirectoryName(file)!);

        try
        {
            // readable by the server alone, from the moment it exists
            var creation = new FileStreamOptions { Mode = FileMode.CreateNew, Access = FileAccess.Write, Share = FileShare.None };

            if (!OperatingSystem.IsWindows())
            {
                creation.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
            }

            using (var stream = new FileStream(file, creation))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(Convert.ToBase64String(key));
            }

            logger.LogWarning("Made a new key for the secrets of the lambdas at {File}. Keep a copy of it apart from the database: "
                            + "a backup of the database opens no secret without it. Or set LAMBDA_SECRETS_KEY instead.", file);

            return key;
        }
        catch (IOException) when (File.Exists(file))
        {
            // made by somebody else a moment ago
            return FromFile(file);
        }
    }

    private static byte[] FromFile(string file)
    {
        var text = File.ReadAllText(file).Trim();

        try
        {
            var key = Convert.FromBase64String(text);

            if (key.Length == KeySize)
            {
                return key;
            }
        }
        catch (FormatException)
        {
            // said below
        }

        throw new InvalidOperationException($"The key for secrets in '{file}' is not {KeySize} bytes in base64. Restore the file it was, or set LAMBDA_SECRETS_KEY.");
    }

    /// <summary>
    /// A key given as 32 bytes in base64, or as a passphrase long enough to
    /// be one - which is stretched into the 32 bytes rather than used as is.
    /// </summary>
    private static byte[] FromSetting(string configured)
    {
        var text = configured.Trim();

        try
        {
            var key = Convert.FromBase64String(text);

            if (key.Length == KeySize)
            {
                return key;
            }
        }
        catch (FormatException)
        {
            // a passphrase, then
        }

        if (text.Length < KeySize)
        {
            throw new InvalidOperationException($"LAMBDA_SECRETS_KEY is too short: give {KeySize} random bytes in base64 (openssl rand -base64 32), or a passphrase of at least {KeySize} characters.");
        }

        return HKDF.Extract(HashAlgorithmName.SHA256, Encoding.UTF8.GetBytes(text), Purpose);
    }

    #endregion

}
