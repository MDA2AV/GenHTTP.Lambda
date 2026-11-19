using System.Collections.Concurrent;
using System.Data.Common;

using GenHTTP.Api.Content.IO;

using GenHTTP.Lambda.Integrations;
using GenHTTP.Lambda.Integrations.Databases;
using GenHTTP.Lambda.Integrations.Files;
using GenHTTP.Lambda.Integrations.Local;
using GenHTTP.Lambda.Integrations.Secrets;
using GenHTTP.Lambda.Integrations.Sqlite;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Tests.Infrastructure;

/// <summary>
/// Integrations that answer the way one on another machine does: nothing a
/// call reaches has completed when it returns, as with a database server,
/// object storage or a vault.
/// </summary>
/// <remarks>
/// The platform's own integrations answer at once, so the platform and its
/// tests never see a task that has not completed. These yield before every
/// call reaches what keeps the data - the platform's SQLite and local files
/// underneath, the secrets in memory - so whatever awaits them is resumed
/// later, elsewhere, and the plain calls a lambda kept wait for them. Each
/// counts what it was asked, so a test can tell it was.
/// </remarks>
internal static class YieldingIntegrations
{

    public static IIntegrationBuilder<IDatabaseProvider> Database(Counter counter)
        => new Builder<IDatabaseProvider>(async (context, cancellation) => new YieldingDatabase(await SqliteDatabase.Create().BuildAsync(context, cancellation), counter));

    public static IIntegrationBuilder<IFileProvider> Files(Counter counter)
        => new Builder<IFileProvider>(async (context, cancellation) => new YieldingFiles(await LocalFiles.Create().BuildAsync(context, cancellation), counter));

    public static IIntegrationBuilder<ISecretProvider> Secrets(Counter counter)
        => new Builder<ISecretProvider>((_, _) => ValueTask.FromResult<ISecretProvider>(new YieldingSecrets(counter)));

    /// <summary>
    /// How often each kind was asked something it had to wait for.
    /// </summary>
    internal sealed class Counter
    {
        private int _database, _files, _secrets;

        public int Database => Volatile.Read(ref _database);

        public int Files => Volatile.Read(ref _files);

        public int Secrets => Volatile.Read(ref _secrets);

        internal async ValueTask DatabaseAsync()
        {
            Interlocked.Increment(ref _database);
            await Task.Yield();
        }

        internal async ValueTask FilesAsync()
        {
            Interlocked.Increment(ref _files);
            await Task.Yield();
        }

        internal async ValueTask SecretsAsync()
        {
            Interlocked.Increment(ref _secrets);
            await Task.Yield();
        }
    }

    private sealed class Builder<T>(Func<IntegrationContext, CancellationToken, ValueTask<T>> build) : IIntegrationBuilder<T> where T : IIntegration
    {

        public ValueTask<T> BuildAsync(IntegrationContext context, CancellationToken cancellation = default) => build(context, cancellation);

    }

    private sealed class YieldingDatabase(IDatabaseProvider inner, Counter counter) : IDatabaseProvider
    {

        public DatabaseDialect Dialect => inner.Dialect;

        public DatabaseBinding Binding => inner.Binding;

        public void ConfigureStore(DbContextOptionsBuilder options, DatabaseStore store) => inner.ConfigureStore(options, store);

        public void ConfigureContext(DbContextOptionsBuilder options, DbConnection connection) => inner.ConfigureContext(options, connection);

        public string Quote(string name) => inner.Quote(name);

        public IntegrationInfo Describe() => inner.Describe();

        public string? Guide(GuideContext context) => inner.Guide(context);

        public async ValueTask<DbConnection> OpenAsync(DatabaseStore store, CancellationToken cancellation = default)
        {
            await counter.DatabaseAsync();
            return await inner.OpenAsync(store, cancellation);
        }

        public async ValueTask CreateAsync(DataScope scope, CancellationToken cancellation = default)
        {
            await counter.DatabaseAsync();
            await inner.CreateAsync(scope, cancellation);
        }

        public async ValueTask<bool> ExistsAsync(DataScope scope, CancellationToken cancellation = default)
        {
            await counter.DatabaseAsync();
            return await inner.ExistsAsync(scope, cancellation);
        }

        public async ValueTask<DbConnection> ConnectAsync(DataScope scope, long room, CancellationToken cancellation = default)
        {
            await counter.DatabaseAsync();
            return await inner.ConnectAsync(scope, room, cancellation);
        }

        public async ValueTask<DbConnection?> ReadAsync(DataScope scope, TimeSpan patience, CancellationToken cancellation = default)
        {
            await counter.DatabaseAsync();
            return await inner.ReadAsync(scope, patience, cancellation);
        }

        public async ValueTask<long> SizeOfAsync(DataScope scope, CancellationToken cancellation = default)
        {
            await counter.DatabaseAsync();
            return await inner.SizeOfAsync(scope, cancellation);
        }

        public async ValueTask CopyAsync(DataScope source, DataScope target, CancellationToken cancellation = default)
        {
            await counter.DatabaseAsync();
            await inner.CopyAsync(source, target, cancellation);
        }

        public async ValueTask DeleteAsync(DataScope scope, CancellationToken cancellation = default)
        {
            await counter.DatabaseAsync();
            await inner.DeleteAsync(scope, cancellation);
        }

        public async ValueTask DeleteAllAsync(long lambdaId, CancellationToken cancellation = default)
        {
            await counter.DatabaseAsync();
            await inner.DeleteAllAsync(lambdaId, cancellation);
        }

        public async ValueTask<string?> ExportAsync(DataScope scope, CancellationToken cancellation = default)
        {
            await counter.DatabaseAsync();
            return await inner.ExportAsync(scope, cancellation);
        }

        public async ValueTask<IReadOnlyList<DatabaseTableInfo>> TablesAsync(DbConnection connection, CancellationToken cancellation = default)
        {
            await counter.DatabaseAsync();
            return await inner.TablesAsync(connection, cancellation);
        }

        public async ValueTask<IReadOnlyList<DatabaseColumnInfo>> ColumnsAsync(DbConnection connection, string table, CancellationToken cancellation = default)
        {
            await counter.DatabaseAsync();
            return await inner.ColumnsAsync(connection, table, cancellation);
        }

        public async ValueTask<string?> NaturalOrderAsync(DbConnection connection, string table, CancellationToken cancellation = default)
        {
            await counter.DatabaseAsync();
            return await inner.NaturalOrderAsync(connection, table, cancellation);
        }

    }

    private sealed class YieldingFiles(IFileProvider inner, Counter counter) : IFileProvider
    {

        public IntegrationInfo Describe() => inner.Describe();

        public string? Guide(GuideContext context) => inner.Guide(context);

        public IResourceTree Tree(DataScope scope, string? folder) => inner.Tree(scope, folder);

        public async ValueTask<byte[]?> ReadAsync(FileStore store, string name, CancellationToken cancellation = default)
        {
            await counter.FilesAsync();
            return await inner.ReadAsync(store, name, cancellation);
        }

        public async ValueTask WriteAsync(FileStore store, string name, ReadOnlyMemory<byte> content, CancellationToken cancellation = default)
        {
            await counter.FilesAsync();
            await inner.WriteAsync(store, name, content, cancellation);
        }

        public async ValueTask DeleteAsync(FileStore store, string name, CancellationToken cancellation = default)
        {
            await counter.FilesAsync();
            await inner.DeleteAsync(store, name, cancellation);
        }

        public async ValueTask<IReadOnlyList<FileEntry>> ListAsync(DataScope scope, CancellationToken cancellation = default)
        {
            await counter.FilesAsync();
            return await inner.ListAsync(scope, cancellation);
        }

        public async ValueTask<FileEntry?> FindAsync(DataScope scope, string path, CancellationToken cancellation = default)
        {
            await counter.FilesAsync();
            return await inner.FindAsync(scope, path, cancellation);
        }

        public async ValueTask<byte[]?> ReadAsync(DataScope scope, string path, CancellationToken cancellation = default)
        {
            await counter.FilesAsync();
            return await inner.ReadAsync(scope, path, cancellation);
        }

        public async ValueTask<IResource?> ResourceAsync(DataScope scope, string path, CancellationToken cancellation = default)
        {
            await counter.FilesAsync();
            return await inner.ResourceAsync(scope, path, cancellation);
        }

        public async ValueTask WriteAsync(DataScope scope, string path, ReadOnlyMemory<byte> content, CancellationToken cancellation = default)
        {
            await counter.FilesAsync();
            await inner.WriteAsync(scope, path, content, cancellation);
        }

        public async ValueTask WriteAsync(DataScope scope, string path, Stream content, CancellationToken cancellation = default)
        {
            await counter.FilesAsync();
            await inner.WriteAsync(scope, path, content, cancellation);
        }

        public async ValueTask CreateFolderAsync(DataScope scope, string path, CancellationToken cancellation = default)
        {
            await counter.FilesAsync();
            await inner.CreateFolderAsync(scope, path, cancellation);
        }

        public async ValueTask DeleteAsync(DataScope scope, string path, CancellationToken cancellation = default)
        {
            await counter.FilesAsync();
            await inner.DeleteAsync(scope, path, cancellation);
        }

        public async ValueTask ClearAsync(DataScope scope, CancellationToken cancellation = default)
        {
            await counter.FilesAsync();
            await inner.ClearAsync(scope, cancellation);
        }

        public async ValueTask CopyAsync(DataScope source, DataScope target, CancellationToken cancellation = default)
        {
            await counter.FilesAsync();
            await inner.CopyAsync(source, target, cancellation);
        }

        public async ValueTask DeleteAsync(DataScope scope, CancellationToken cancellation = default)
        {
            await counter.FilesAsync();
            await inner.DeleteAsync(scope, cancellation);
        }

        public async ValueTask DeleteAllAsync(long lambdaId, CancellationToken cancellation = default)
        {
            await counter.FilesAsync();
            await inner.DeleteAllAsync(lambdaId, cancellation);
        }

    }

    /// <summary>
    /// Secrets kept in memory, in the clear - for a test, a stand-in for a
    /// vault on another machine.
    /// </summary>
    private sealed class YieldingSecrets(Counter counter) : ISecretProvider
    {
        private readonly ConcurrentDictionary<DataScope, ConcurrentDictionary<string, SecretEntry>> _entries = [];

        private readonly ConcurrentDictionary<(DataScope Scope, string Name), string> _values = [];

        public IntegrationInfo Describe() => new(IntegrationKind.Secrets, "Yielding", []);

        public string? Guide(GuideContext context) => null;

        public async ValueTask<ISecretSet> LoadAsync(DataScope scope, CancellationToken cancellation = default)
        {
            await counter.SecretsAsync();

            var names = Of(scope).Keys.ToList();

            return new Set(names.ToDictionary(n => n, n => _values[(scope, n)], StringComparer.Ordinal));
        }

        public async ValueTask<IReadOnlyList<SecretEntry>> ListAsync(DataScope scope, CancellationToken cancellation = default)
        {
            await counter.SecretsAsync();
            return [.. Of(scope).Values.OrderBy(e => e.Name, StringComparer.Ordinal)];
        }

        public async ValueTask<SecretEntry> StoreAsync(DataScope scope, string name, string value, CancellationToken cancellation = default)
        {
            await counter.SecretsAsync();

            var now = DateTime.UtcNow;

            _values[(scope, name)] = value;

            return Of(scope).AddOrUpdate(name, new SecretEntry(name, now, now), (_, was) => was with { Changed = now });
        }

        public async ValueTask<bool> DeleteAsync(DataScope scope, string name, CancellationToken cancellation = default)
        {
            await counter.SecretsAsync();

            _values.TryRemove((scope, name), out _);

            return Of(scope).TryRemove(name, out _);
        }

        public async ValueTask CopyAsync(DataScope source, DataScope target, CancellationToken cancellation = default)
        {
            await counter.SecretsAsync();

            Clear(target);

            foreach (var entry in Of(source).Values)
            {
                Of(target)[entry.Name] = entry;
                _values[(target, entry.Name)] = _values[(source, entry.Name)];
            }
        }

        public async ValueTask DeleteAsync(DataScope scope, CancellationToken cancellation = default)
        {
            await counter.SecretsAsync();
            Clear(scope);
        }

        public async ValueTask DeleteAllAsync(long lambdaId, CancellationToken cancellation = default)
        {
            await counter.SecretsAsync();

            foreach (var scope in _entries.Keys.Where(s => s.LambdaId == lambdaId).ToList())
            {
                Clear(scope);
            }
        }

        private ConcurrentDictionary<string, SecretEntry> Of(DataScope scope) => _entries.GetOrAdd(scope, _ => new(StringComparer.Ordinal));

        private void Clear(DataScope scope)
        {
            foreach (var name in Of(scope).Keys.ToList())
            {
                _values.TryRemove((scope, name), out _);
            }

            _entries.TryRemove(scope, out _);
        }

        private sealed class Set(Dictionary<string, string> values) : ISecretSet
        {

            public int Count => values.Count;

            public string? Read(string name) => values.GetValueOrDefault(name);

        }

    }

}
