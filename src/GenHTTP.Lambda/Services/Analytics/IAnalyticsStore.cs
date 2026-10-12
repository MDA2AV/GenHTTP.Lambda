namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// Where the analytics of the lambdas are kept, as other services reach it.
/// </summary>
public interface IAnalyticsStore
{

    /// <summary>
    /// Removes everything counted of a lambda, called when it is deleted.
    /// </summary>
    void Forget(long lambda);

}
