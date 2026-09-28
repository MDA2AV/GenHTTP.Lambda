/*
 * Every test builds a fixture of its own - its own directory, database,
 * server and port - so they run side by side rather than one after the
 * other. What they share is process wide by nature and fine to share: the
 * console tee, the lambdas already compiled (cached by the workspace path,
 * which is per fixture) and the port counter of the test host.
 *
 * A test that ever has to have the process to itself says so with
 * [DoNotParallelize] and runs after the others.
 */
[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]
