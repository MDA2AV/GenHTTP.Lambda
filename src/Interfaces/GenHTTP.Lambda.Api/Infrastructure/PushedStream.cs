using GenHTTP.Api.Content;

using GenHTTP.Modules.ServerSentEvents;

namespace GenHTTP.Lambda.Api.Infrastructure;

/// <summary>
/// What a page follows a run by: each state of it pushed as a server-sent
/// event the moment it changes, rather than asked for on a timer.
/// </summary>
/// <remarks>
/// Every event is the whole of what the page draws, the same object the
/// plain read answers with - so a page that loses the stream and reads once
/// is where it would have been, and the browser's EventSource reconnecting
/// on its own is given the current state first. While nothing changes a
/// comment keeps the connection, so a proxy in front does not take it for
/// idle. The stream ends when the run does, or when the page goes.
/// </remarks>
internal static class PushedStream
{

    public static IHandlerBuilder Of<T>(string name, Func<CancellationToken, IAsyncEnumerable<T?>> follow) where T : class
        => EventSource.Create().Generator(async connection =>
        {
            using var gone = new CancellationTokenSource();

            try
            {
                await foreach (var state in follow(gone.Token))
                {
                    var sent = state != null ? await connection.DataAsync(state, name) : await connection.CommentAsync("still here");

                    if (!sent)
                    {
                        await gone.CancelAsync();
                        break;
                    }
                }
            }
            catch (OperationCanceledException) when (gone.IsCancellationRequested)
            {
                // the page went
            }
        });

}
