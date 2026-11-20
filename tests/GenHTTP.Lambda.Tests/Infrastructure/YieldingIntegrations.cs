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
using Microsoft.EntityFrameworkCore.Diagnostics;

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
/// counts what it was asked, so a test can tell it was. The platform's own
/// records yield before every statement as well, as they do on a database
/// server: the turns a change takes are held across a real wait, and the
/// caches serving reads are loaded by tasks that have not completed.
/// </remarks>
internal static class YieldingIntegrations
{

    public static IIntegrationBuilder<IDatabaseProvider> Database(Counter counter)
        => new Builder<IDatabaseProvider>(context => new YieldingDatabase(SqliteDatabase.Create().Build(context), counter));

    public static IIntegrationBuilder<IFileProvider> Files(Counter counter)
        => new Builder<IFileProvider>(context => new YieldingFiles(LocalFiles.Create().Build(context), counter));

    public static IIntegrationBuilder<ISecretProvider> Secrets(Counter counter)
        => new Builder<ISecretProvider>(_ => new YieldingSecrets(counter, true));

    /// <summary>
    /// Secrets in a vault that does not answer: preparing them fails, as it
    /// does where the server is down or the address wrong.
    /// </summary>
    public static IIntegrationBuilder<ISecretProvider> UnreachableSecrets(Counter counter)
        => new Builder<ISecretProvider>(_ => new YieldingSecrets(counter, false));

    /// <summary>
    /// How often each kind was asked something it had to wait for.
    /// </summary>
    internal sealed class Counter
    {
        private int _database, _files, _secrets, _records;

        private readonly ConcurrentQueue<string> _lifecycle = [];

        /// <summary>
        /// What was prepared and let go of, in order: <c>prepared database</c>,
        /// <c>disposed secrets</c> and so on.
        /// </summary>
        public IReadOnlyList<string> Lifecycle => [.. _lifecycle];

        internal void Happened(string what) => _lifecycle.Enqueue(what);

        public int Database => Volatile.Read(ref _database);

        /// <summary>
        /// How many statements the platform sent to its own records.
        /// </summary>
        public int Records => Volatile.Read(ref _records);

        public int Files => Volatile.Read(ref _files);

        public int Secrets => Volatile.Read(ref _secrets);

        internal async ValueTask DatabaseAsync()
        {
            Interlocked.Increment(ref _database);
            await Task.Yield();
        }

        internal async ValueTask RecordsAsync()
        {
            Interlocked.Increment(ref _records);
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

    private sealed class Builder<T>(Func<IntegrationContext, T> build) : IIntegrationBuilder<T> where T : IIntegration
    {

        public T Build(IntegrationContext context) => build(context);

    }

    private sealed class YieldingDatabase(IDatabaseProvider inner, Counter counter) : IDatabaseProvider
    {

        public DatabaseDialect Dialect => inner.Dialect;

        public DatabaseBinding Binding => inner.Binding;

        public void ConfigureStore(DbContextOptionsBuilder options, DatabaseStore store)
        {
            inner.ConfigureStore(options, store);

            if (store == DatabaseStore.Platform)
            {
                options.AddInterceptors(new YieldingRecords(counter));
            }
        }

        public void ConfigureContext(DbContextOptionsBuilder options, DbConnection connection) => inner.ConfigureContext(options, connection);

        public string Quote(string name) => inner.Quote(name);

        public IntegrationInfo Describe() => inner.Describe();

        public string? Guide(GuideContext context) => inner.Guide(context);

        public async ValueTask PrepareAsync(CancellationToken cancellation = default)
        {
            await Task.Yield();
            await inner.PrepareAsync(cancellation);

            counter.Happened("prepared database");
        }

        public async ValueTask DisposeAsync()
        {
            await inner.DisposeAsync();

            counter.Happened("disposed database");
        }

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

        public async ValueTask<DatabaseOptimization> OptimizeAsync(DatabaseStore store, CancellationToken cancellation = default)
        {
            await counter.DatabaseAsync();
            return await inner.OptimizeAsync(store, cancellation);
        }

        public async ValueTask<DatabaseOptimization> OptimizeAsync(DataScope scope, CancellationToken cancellation = default)
        {
            await counter.DatabaseAsync();
            return await inner.OptimizeAsync(scope, cancellation);
        }

    }

    /// <summary>
    /// Yields before every statement the platform sends to its own records.
    /// </summary>
    private sealed class YieldingRecords(Counter counter) : DbCommandInterceptor
    {

        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
                                                                                               CancellationToken cancellationToken = default)
        {
            await counter.RecordsAsync();
            return result;
        }

        public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result,
                                                                                        CancellationToken cancellationToken = default)
        {
            await counter.RecordsAsync();
            return result;
        }

        public override async ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<object> result,
                                                                                         CancellationToken cancellationToken = default)
        {
            await counter.RecordsAsync();
            return result;
        }

    }

    private sealed class YieldingFiles(IFileProvider inner, Counter counter) : IFileProvider
    {

        public IntegrationInfo Describe() => inner.Describe();

        public string? Guide(GuideContext context) => inner.Guide(context);

        public async ValueTask PrepareAsync(CancellationToken cancellation = default)
        {
            await Task.Yield();
            await inner.PrepareAsync(cancellation);

            counter.Happened("prepared files");
        }

        public async ValueTask DisposeAsync()
        {
            await inner.DisposeAsync();

            counter.Happened("disposed files");
        }

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
    private sealed class YieldingSecrets(Counter counter, bool reachable) : ISecretProvider
    {
        private readonly ConcurrentDictionary<DataScope, ConcurrentDictionary<string, SecretEntry>> _entries = [];

        private readonly ConcurrentDictionary<(DataScope Scope, string Name), string> _values = [];

        public IntegrationInfo Describe() => new(IntegrationKind.Secrets, "Yielding", []);

        public string? Guide(GuideContext context) => null;

        public async ValueTask PrepareAsync(CancellationToken cancellation = default)
        {
            await Task.Yield();

            if (!reachable)
            {
                throw new HttpRequestException("The vault did not answer.");
            }

            counter.Happened("prepared secrets");
        }

        public ValueTask DisposeAsync()
        {
            counter.Happened("disposed secrets");
            return ValueTask.CompletedTask;
        }

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
