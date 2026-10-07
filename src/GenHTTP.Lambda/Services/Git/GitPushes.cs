using GenHTTP.Modules.Git;

using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Settings;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Git;

/// <summary>
/// What a <c>git push</c> to a lambda does: commits pushed to main become its
/// versions, a branch pushed becomes a feature, and a branch deleted takes
/// its feature with it.
/// </summary>
/// <remarks>
/// Everything goes through the services every other door goes through - a
/// version is saved, a feature started, saved, previewed and merged the way
/// the editor or an agent does it - so no rule of theirs is skipped, and a
/// refusal says what they say. What is git's own is decided here: main moves
/// on one version at a time and is never rewritten, and the files the
/// platform puts around a lambda are not the lambda's to change (see
/// <see cref="ProjectTree"/>).
///
/// What happened is told back as the push goes, in the lines git shows after
/// <c>remote:</c> - never with the editor key, which whoever pushed has, and
/// whoever reads a build log of theirs should not. Options a push carries
/// (<c>git push -o deploy</c>, <c>-o merge</c>) put what was pushed online.
/// </remarks>
public sealed class GitPushes(GitStore store, GitHistory history, IMetaService meta, IFeatureService features, ILimitsService limits,
                                ILogger<GitPushes> logger)
{
    private const string Deploy = "deploy";

    private const string Merge = "merge";

    /// <summary>
    /// How many problems the compiler found are told before the rest is counted.
    /// </summary>
    private const int MostProblems = 15;

    #region Pushing

    /// <summary>
    /// Applies a push, in the lambda's turn to be written.
    /// </summary>
    /// <param name="index">Which commit is what, as it stands at the start of the turn - kept up to date as it goes</param>
    public async ValueTask PushAsync(GitLambda lambda, GitIndex index, GitPush push)
    {
        var said = new Said(push);

        foreach (var option in push.Options.Where(o => o is not (Deploy or Merge)))
        {
            await said.LineAsync($"There is no option '{option}' here: -o deploy and -o merge are.");
        }

        var commits = new Commits(store, history, lambda.Id, push);

        foreach (var update in push.Updates)
        {
            try
            {
                if (update.IsTag)
                {
                    Refuse(lambda, update, "Tags are the versions, and the platform sets them: v1, v2, and so on. Push a branch instead.");
                }
                else if (!update.IsBranch)
                {
                    Refuse(lambda, update, "Only branches are pushed here: main for versions, any other for a feature.");
                }
                else if (update.ShortName == FeatureBranches.Main)
                {
                    await MainAsync(lambda, index, update, commits, said, push.Options.Contains(Deploy));
                }
                else
                {
                    await FeatureAsync(lambda, index, update, commits, said, push.Options.Contains(Deploy), push.Options.Contains(Merge));
                }
            }
            catch (LambdaException e)
            {
                // what the services refuse is what the push is told, in their words
                if (update.Status == GitUpdateStatus.Pending)
                {
                    Refuse(lambda, update, e.Message);
                }
                else
                {
                    await said.LineAsync(e.Message);
                }
            }
        }
    }

    /// <summary>
    /// Refuses every update of a push to a lambda nobody may change.
    /// </summary>
    public void RefuseAll(GitLambda lambda, GitPush push, string reason)
    {
        foreach (var update in push.Updates)
        {
            Refuse(lambda, update, reason);
        }
    }

    #endregion

    #region Versions

    /// <summary>
    /// Commits pushed to main: a version each, the newest compiled first.
    /// </summary>
    private async ValueTask MainAsync(GitLambda lambda, GitIndex index, GitReferenceUpdate update, Commits commits, Said said, bool deploy)
    {
        if (update.IsDelete)
        {
            Refuse(lambda, update, "main holds the versions of the lambda and cannot be deleted.");
            return;
        }

        if (index.Main is not { } main || update.OldId != main)
        {
            Refuse(lambda, update, "A version was saved since you fetched - in the editor, or by an agent. git pull --rebase, and push again.");
            return;
        }

        if (!update.IsFastForward)
        {
            Refuse(lambda, update, "A version never changes, so main is never rewritten. git pull --rebase, and push again without --force.");
            return;
        }

        var allowed = limits.Of(Enum.Parse<LambdaTier>(lambda.Tier)).Versions;

        // from the old tip to the new one, one commit after another
        var chain = new List<GitObjectId>();

        for (var at = update.NewId; at != update.OldId;)
        {
            var commit = commits.Find(at);

            if (commit == null || index.VersionOf(at) != null)
            {
                Refuse(lambda, update, "This is not based on main as it is. git pull --rebase, and push again.");
                return;
            }

            if (commit.Parents.Count != 1)
            {
                Refuse(lambda, update, $"{Short(at)} is a merge commit, and main is one version after another. "
                                                       + "Rebase instead of merging (git pull --rebase, or git rebase origin/main), and push again.");
                return;
            }

            chain.Insert(0, at);

            if (chain.Count > allowed)
            {
                Refuse(lambda, update, $"Every commit pushed to main becomes a version, and a lambda keeps {allowed}: this push holds more. "
                                                       + "Squash them into fewer (git rebase -i origin/main), and push again.");
                return;
            }

            at = commit.Parents[0];
        }

        // each commit read as the lambda it holds, against the one before it
        var steps = new List<(GitObjectId Id, GitCommit Commit, IReadOnlyList<GitFile> Tree, ReadTree Read)>();

        var parent = store.ReadCommit(lambda.Id, update.OldId);

        foreach (var id in chain)
        {
            var tree = commits.TreeOf(id);

            var read = await ProjectTree.ReadAsync(tree, parent != null ? [parent] : [], blob => store.ReadBlob(lambda.Id, blob));

            if (read.Files == null)
            {
                Refuse(lambda, update, chain.Count > 1 ? $"{Short(id)}: {read.Complaint}" : read.Complaint!);
                return;
            }

            steps.Add((id, commits.Find(id)!, tree, read));

            parent = new StoredCommit(string.Empty, [.. tree.Select(f => new StoredEntry(f.Path, f.Id!.Value.ToString()))], read.Stored, Layout: GitLayouts.Current);
        }

        // a version is only made of what compiles, as when a feature is merged
        var tip = steps[^1];

        var check = await meta.CheckAsync(lambda.PrivateKey, LambdaSource.Serialize(tip.Read.Files!));

        if (!check.Success)
        {
            await ProblemsAsync(said, check.Diagnostics, tip.Read);
            Refuse(lambda, update, "It does not compile, so it is no version. Fix what the lines above say, and push again.");
            return;
        }

        var saved = new List<LambdaVersionInfo>();

        var after = index.Versions.Keys.Max();

        foreach (var step in steps)
        {
            var (change, specification) = Message(step.Commit);

            LambdaVersionInfo version;

            try
            {
                version = meta.Save(lambda.PrivateKey, LambdaSource.Serialize(step.Read.Files!), new VersionNote(specification, change, VersionOrigins.Git), after);
            }
            catch (LambdaException e) when (saved.Count > 0)
            {
                // the versions before it are saved, and main is where they took it
                await said.LineAsync($"Saved {Versions(saved)}, and stopped at {Short(step.Id)}: {e.Message}");
                Refuse(lambda, update, "Not all of it was saved. git pull --rebase, and push again.");
                return;
            }

            await history.KeepAsync(lambda.Id, step.Commit, step.Tree, step.Read, version.Version);

            index.Versions[version.Version] = step.Id.ToString();

            store.WriteIndex(lambda.Id, index);

            saved.Add(version);

            after = version.Version;
        }

        update.Accept();

        logger.LogInformation("Pushed lambda {Lambda} versions {From} to {To}", lambda.PublicKey, saved[0].Version, saved[^1].Version);

        await said.LineAsync(saved.Count == 1
            ? $"Saved version {saved[0].Version}: {saved[0].Change ?? "(no message)"}"
            : $"Saved {Versions(saved)}, the newest: {saved[^1].Change ?? "(no message)"}");

        await MergedAsync(lambda, index, chain, saved[^1].Version, said);

        if (deploy)
        {
            await DeployAsync(lambda, saved[^1].Version, said);
        }
        else
        {
            await NotOnlineAsync(lambda, saved[^1].Version, said);
        }
    }

    /// <summary>
    /// Removes the features main now holds: their branch was pushed to it, so
    /// they are merged, as merging them would have done.
    /// </summary>
    private async ValueTask MergedAsync(GitLambda lambda, GitIndex index, List<GitObjectId> chain, int version, Said said)
    {
        var made = chain.Select(c => c.ToString()).ToHashSet(StringComparer.Ordinal);

        foreach (var (key, tip) in index.Features.Where(f => made.Contains(f.Value.Commit)).ToList())
        {
            var name = features.Delete(lambda.PrivateKey, key);

            index.Features.Remove(key);

            store.WriteIndex(lambda.Id, index);

            logger.LogInformation("Merged feature '{Feature}' of lambda {Lambda} by push version {Version}", name, lambda.PublicKey, version);

            await said.LineAsync($"The feature '{name}' is in main now, so it is merged: its preview and its copy of the data are gone, the lambda keeps its own.");
        }
    }

    #endregion

    #region Features

    /// <summary>
    /// A branch pushed: its feature started, saved or deleted.
    /// </summary>
    private async ValueTask FeatureAsync(GitLambda lambda, GitIndex index, GitReferenceUpdate update, Commits commits, Said said, bool deploy, bool merge)
    {
        var branch = update.ShortName;

        var feature = features.List(lambda.PrivateKey).FirstOrDefault(f => f.Branch == branch);

        if (update.IsDelete)
        {
            if (feature == null)
            {
                Refuse(lambda, update, $"There is no feature at '{branch}'.");
                return;
            }

            var name = features.Delete(lambda.PrivateKey, feature.Key);

            index.Features.Remove(feature.Key);

            store.WriteIndex(lambda.Id, index);

            update.Accept();

            logger.LogInformation("Deleted feature '{Feature}' of lambda {Lambda} by push", name, lambda.PublicKey);

            await said.LineAsync($"Deleted the feature '{name}', with its preview and its copy of the data.");
            return;
        }

        var tipBefore = feature != null ? index.Features.GetValueOrDefault(feature.Key) : null;

        if (feature != null && (tipBefore == null || GitObjectId.Parse(tipBefore.Commit) != update.OldId))
        {
            Refuse(lambda, update, $"The feature '{feature.Name}' was changed since you fetched - in the editor, or by an agent. git pull --rebase, and push again.");
            return;
        }

        if (Base(index, update.NewId, commits) is not { } based)
        {
            Refuse(lambda, update, "A branch starts from main or one of its versions, and this one does not. "
                                                   + $"git switch -c {branch} origin/main, bring your commits over, and push again.");
            return;
        }

        // what its commits make of the lambda, the new ones parents first
        ReadTree? read = null;

        var kept = new List<(GitCommit Commit, IReadOnlyList<GitFile> Tree, ReadTree Read)>();

        foreach (var revision in update.Revisions)
        {
            var tree = commits.TreeOf(revision.Commit.Id);

            var parents = revision.Commit.Parents.Select(p => commits.Stored(p)).OfType<StoredCommit>().ToList();

            var mine = await ProjectTree.ReadAsync(tree, parents, blob => store.ReadBlob(lambda.Id, blob));

            commits.Provide(revision.Commit.Id, tree, mine.Files != null ? mine.Stored : null);

            kept.Add((revision.Commit, tree, mine));

            if (revision.Commit.Id == update.NewId)
            {
                read = mine;
            }
        }

        if (read == null)
        {
            // a commit the lambda has already: a version, or another feature's
            var known = commits.Stored(update.NewId);

            read = known?.Files != null
                ? await ProjectTree.ReadAsync(commits.TreeOf(update.NewId), [known], blob => store.ReadBlob(lambda.Id, blob))
                : ReadTree.Refused("This commit holds no state of the lambda.");
        }

        if (read.Files == null)
        {
            Refuse(lambda, update, read.Complaint!);
            return;
        }

        var code = LambdaSource.Serialize(read.Files);

        var (change, _) = Message(commits.Find(update.NewId)!);

        var started = feature == null;

        if (feature == null)
        {
            feature = await features.CreateAsync(lambda.PrivateKey, new FeatureDraft(Name(branch), Base: based, Origin: VersionOrigins.Git, Branch: branch));
        }
        else if (feature.Base != based)
        {
            feature = features.Update(lambda.PrivateKey, feature.Key, new FeatureUpdate(Base: based));
        }

        try
        {
            feature = features.Save(lambda.PrivateKey, feature.Key, code, new VersionNote(Change: feature.Change == null ? change : null, Origin: VersionOrigins.Git),
                                    after: feature.Revision);
        }
        catch (LambdaException) when (started)
        {
            // refused, so the branch is not there: neither is the feature started for it
            features.Delete(lambda.PrivateKey, feature.Key);
            throw;
        }

        if (started)
        {
            logger.LogInformation("Created feature '{Feature}' of lambda {Lambda} by push base {Version}", feature.Name, lambda.PublicKey, based);

            await said.LineAsync($"Started the feature '{feature.Name}' from version {based}, with a copy of the lambda's data to try it on.");
        }

        foreach (var (commit, tree, mine) in kept)
        {
            await history.KeepAsync(lambda.Id, commit, tree, mine.Files != null ? mine : null, null);
        }

        index.Features[feature.Key] = new FeatureTip(update.NewId.ToString(), feature.Revision, based);

        store.WriteIndex(lambda.Id, index);

        update.Accept();

        logger.LogInformation("Pushed feature '{Feature}' of lambda {Lambda} base {Version}", feature.Name, lambda.PublicKey, based);

        if (!started)
        {
            await said.LineAsync($"Saved the feature '{feature.Name}', based on version {based}{(feature.Mergeable ? string.Empty : $" - the newest is {feature.Newest}")}.");
        }

        if (merge)
        {
            await MergeAsync(lambda, index, feature, read, said, deploy);
        }
        else
        {
            // a preview is no visitor's business, so every push puts it online
            await PreviewAsync(lambda, feature, read, said);
        }
    }

    /// <summary>
    /// The newest version a commit holds - the one a feature at it is based
    /// on - or nothing where it holds none.
    /// </summary>
    private static int? Base(GitIndex index, GitObjectId tip, Commits commits)
    {
        int? found = null;

        var seen = new HashSet<GitObjectId>();

        var pending = new Stack<GitObjectId>([tip]);

        while (pending.TryPop(out var id) && seen.Count < 100_000)
        {
            if (!seen.Add(id))
            {
                continue;
            }

            if (index.VersionOf(id) is { } version)
            {
                // the versions below it are older
                found = Math.Max(found ?? 0, version);
                continue;
            }

            foreach (var parent in commits.Find(id)?.Parents ?? [])
            {
                pending.Push(parent);
            }
        }

        return found;
    }

    private async ValueTask PreviewAsync(GitLambda lambda, FeatureInfo feature, ReadTree read, Said said)
    {
        var result = await features.DeployAsync(lambda.PrivateKey, feature.Key);

        logger.LogInformation(result.Success ? "Deployed feature '{Feature}' of lambda {Lambda}" : "Failed to deploy feature '{Feature}' of lambda {Lambda}",
                              feature.Name, lambda.PublicKey);

        if (!result.Success)
        {
            await ProblemsAsync(said, result.Diagnostics, read);
            await said.LineAsync("It does not compile yet, so its preview stays as it was. Fix what the lines above say, and push again.");
            return;
        }

        await said.LineAsync($"Its preview is online at {Preview(lambda, feature)}");

        await said.LineAsync(feature.Mergeable
            ? $"To make it the next version: git push origin {feature.Branch}:main (a version for each of its commits), -o merge with its next push (one version), or merge it in the editor."
            : $"Before it can become a version, bring main in (git rebase origin/main) - version {feature.Newest} was saved since it began.");
    }

    private async ValueTask MergeAsync(GitLambda lambda, GitIndex index, FeatureInfo feature, ReadTree read, Said said, bool deploy)
    {
        if (!feature.Mergeable)
        {
            await said.LineAsync($"It cannot be merged yet: it holds version {feature.Base}, and the newest is {feature.Newest}. "
                               + "Bring main in (git rebase origin/main, or git merge origin/main), and push it with -o merge again.");
            return;
        }

        var merged = await features.MergeAsync(lambda.PrivateKey, feature.Key, new VersionNote(Origin: VersionOrigins.Git), deploy);

        if (!merged.Merged)
        {
            await ProblemsAsync(said, merged.Diagnostics, read);
            await said.LineAsync("It does not compile, so it was not merged. Fix what the lines above say, and push it with -o merge again.");
            return;
        }

        index.Features.Remove(feature.Key);

        store.WriteIndex(lambda.Id, index);

        logger.LogInformation("Merged feature '{Feature}' of lambda {Lambda} by push version {Version}", merged.Name, lambda.PublicKey, merged.Version!.Version);

        await said.LineAsync($"Merged it as version {merged.Version.Version}: the feature, its preview and its copy of the data are gone, the lambda keeps its own.");

        if (merged.Deployment is { } deployment)
        {
            await DeployedAsync(lambda, deployment, merged.Version.Version, said);
        }
        else
        {
            await NotOnlineAsync(lambda, merged.Version.Version, said);
        }

        await said.LineAsync("git switch main, git pull and git fetch --prune bring your clone up to date.");
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Says what is online instead of a version just made, and how it goes online.
    /// </summary>
    private async ValueTask NotOnlineAsync(GitLambda lambda, int version, Said said)
    {
        var online = meta.Get(lambda.PrivateKey)?.ActiveVersion;

        await said.LineAsync($"{(online == null ? "Nothing is online yet" : $"Version {online} is online")}. To put version {version} online, deploy it in the editor "
                           + $"or POST {lambda.Project.Home}/api/v1/lambdas/<editor key>/deployment/start; -o deploy does it with a push.");
    }

    private async ValueTask DeployAsync(GitLambda lambda, int version, Said said)
    {
        var result = await meta.DeployAsync(lambda.PrivateKey, version, VersionOrigins.Git);

        await DeployedAsync(lambda, result, version, said);
    }

    private async ValueTask DeployedAsync(GitLambda lambda, DeploymentResult result, int version, Said said)
    {
        logger.LogInformation(result.Success ? "Deployed lambda {Lambda} version {Version}" : "Failed to deploy lambda {Lambda} version {Version}",
                              lambda.PublicKey, version);

        if (result.Success)
        {
            await said.LineAsync($"Version {version} is online at {lambda.Project.Address}");
        }
        else
        {
            await ProblemsAsync(said, result.Diagnostics, null);
            await said.LineAsync($"Version {version} could not go online; what was online stays.");
        }
    }

    /// <summary>
    /// What the compiler found, where it is in the repository.
    /// </summary>
    private static async ValueTask ProblemsAsync(Said said, IReadOnlyList<CompilationDiagnostic> diagnostics, ReadTree? read)
    {
        var errors = diagnostics.Where(d => d.Severity == "Error").ToList();

        var unwrapped = read?.Project != null ? ProjectSnippet.Unwrap(read.Project) : null;

        foreach (var diagnostic in errors.Take(MostProblems))
        {
            await said.LineAsync($"  {Where(diagnostic, read, unwrapped)}: error {diagnostic.Id}: {diagnostic.Message}");
        }

        if (errors.Count > MostProblems)
        {
            await said.LineAsync($"  and {errors.Count - MostProblems} more");
        }
    }

    private static string Where(CompilationDiagnostic diagnostic, ReadTree? read, SnippetUnwrapped? unwrapped)
    {
        var file = diagnostic.File ?? LambdaSource.EntryName;

        if (diagnostic.Line <= 0)
        {
            return file == LambdaSource.EntryName ? ProjectPaths.Snippet : ProjectPaths.Of(file);
        }

        if (file == LambdaSource.EntryName)
        {
            var (line, column) = unwrapped?.Locate(diagnostic.Line, diagnostic.Column) ?? (diagnostic.Line, diagnostic.Column);

            return $"{ProjectPaths.Snippet}({line},{column})";
        }

        var path = read?.Stored.FirstOrDefault(f => f.Name == file)?.Path ?? ProjectPaths.Of(file);

        return $"{path}({diagnostic.Line},{diagnostic.Column})";
    }

    /// <remarks>
    /// Said once, by git, in the line it prints for the branch that was refused.
    /// </remarks>
    private void Refuse(GitLambda lambda, GitReferenceUpdate update, string reason)
    {
        update.Reject(OneLine(reason));

        logger.LogInformation("Refused push of {Reference} to lambda {Lambda}", update.ShortName, lambda.PublicKey);
    }

    /// <summary>
    /// What a commit says it changes, and the rest of its message: what the
    /// user wanted, kept with the version as its specification.
    /// </summary>
    private static (string? Change, string? Specification) Message(GitCommit commit)
    {
        var subject = commit.Subject.Trim();

        var newline = commit.Message.IndexOf('\n');

        var rest = newline < 0 ? null : commit.Message[(newline + 1)..].Trim();

        return (subject.Length == 0 ? null : subject, string.IsNullOrEmpty(rest) ? null : rest);
    }

    private static string Name(string branch) => branch.Length <= FeatureService.MaxName ? branch : branch[..FeatureService.MaxName];

    private static string Preview(GitLambda lambda, FeatureInfo feature) => $"{lambda.Origin}{feature.Path}";

    private static string Short(GitObjectId id) => id.ToString()[..7];

    private static string Versions(List<LambdaVersionInfo> saved) => saved.Count == 1 ? $"version {saved[0].Version}" : $"versions {saved[0].Version} to {saved[^1].Version}";

    private static string OneLine(string text) => string.Join(' ', text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    #endregion

    #region Types

    /// <summary>
    /// What is told back to whoever pushes, a line at a time.
    /// </summary>
    private sealed class Said(GitPush push)
    {
        private bool _started;

        public async ValueTask LineAsync(string line)
        {
            if (!_started)
            {
                _started = true;

                await push.MessageAsync(string.Empty);
            }

            await push.MessageAsync(line);
        }
    }

    /// <summary>
    /// The commits a push reaches: those it brings, and those the lambda has.
    /// </summary>
    private sealed class Commits(GitStore store, GitHistory history, long lambdaId, GitPush push)
    {
        private readonly Dictionary<GitObjectId, GitRevision> _pushed = push.Updates.SelectMany(u => u.Revisions)
                                                                            .DistinctBy(r => r.Commit.Id)
                                                                            .ToDictionary(r => r.Commit.Id);

        private readonly Dictionary<GitObjectId, StoredCommit> _provided = [];

        public GitCommit? Find(GitObjectId id)
            => _pushed.TryGetValue(id, out var revision) ? revision.Commit : store.ReadCommit(lambdaId, id)?.Parse();

        public IReadOnlyList<GitFile> TreeOf(GitObjectId id)
        {
            if (_pushed.TryGetValue(id, out var revision))
            {
                return revision.Tree.Files;
            }

            var stored = store.ReadCommit(lambdaId, id) ?? throw new InvalidOperationException($"Commit {id} is not kept.");

            return history.TreeOf(lambdaId, stored.Tree).Files;
        }

        /// <summary>
        /// A commit as it is kept - or as it will be, for one this push brings.
        /// </summary>
        public StoredCommit? Stored(GitObjectId id) => _provided.GetValueOrDefault(id) ?? store.ReadCommit(lambdaId, id);

        /// <summary>
        /// Remembers what the lambda makes of a commit this push brings, for the commits on top of it.
        /// </summary>
        public void Provide(GitObjectId id, IReadOnlyList<GitFile> tree, IReadOnlyList<StoredFile>? files)
            => _provided[id] = new StoredCommit(string.Empty, [.. tree.Select(f => new StoredEntry(f.Path, f.Id!.Value.ToString()))], files, Layout: GitLayouts.Current);
    }

    #endregion

}
