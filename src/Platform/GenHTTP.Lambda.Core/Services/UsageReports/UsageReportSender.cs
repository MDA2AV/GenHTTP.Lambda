using System.Net.Http.Headers;

namespace GenHTTP.Lambda.Services.UsageReports;

/// <summary>
/// Hands a report to whoever collects them: one POST of the JSON, with a
/// time limit, and whether it arrived.
/// </summary>
/// <remarks>
/// Nothing is retried within a run. A report that does not arrive is
/// replaced by the next day's, which says the same with newer figures - and
/// an installation behind a firewall that drops the connection should cost
/// the collector nothing and itself one attempt a day.
/// </remarks>
public sealed class UsageReportSender : IDisposable
{

    /// <summary>
    /// How long a report may take to arrive, the name lookup and the
    /// connection included, before it is given up on.
    /// </summary>
    public static readonly TimeSpan Patience = TimeSpan.FromSeconds(30);

    private readonly HttpClient _client = new() { Timeout = Patience };

    #region Functionality

    /// <summary>
    /// Sends a report.
    /// </summary>
    /// <returns>Nothing if it arrived, else why it did not, in a few words</returns>
    public async ValueTask<string?> SendAsync(string endpoint, byte[] report, CancellationToken cancellation)
    {
        try
        {
            using var content = new ByteArrayContent(report);

            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };

            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("GenHTTP-Lambda", UsageSurvey.PlatformVersion));

            using var response = await _client.SendAsync(request, cancellation);

            return response.IsSuccessStatusCode ? null : $"answered with {(int)response.StatusCode} {response.ReasonPhrase}";
        }
        catch (OperationCanceledException) when (!cancellation.IsCancellationRequested)
        {
            return $"no answer within {Patience.TotalSeconds:N0} seconds";
        }
        catch (HttpRequestException e)
        {
            return e.InnerException?.Message ?? e.Message;
        }
    }

    public void Dispose() => _client.Dispose();

    #endregion

}
