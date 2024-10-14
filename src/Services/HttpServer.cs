using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MesiIK.Services
{
    public class HttpServer
    {
        private HttpListener? listener;
        private CancellationTokenSource? cts;
        public event Action<string>? RequestReceived;

        public void Start(string address, int port)
        {
            if (listener != null)
            {
                throw new InvalidOperationException("Server is already running.");
            }

            try
            {
                listener = new HttpListener();
                cts = new CancellationTokenSource();

                listener.Prefixes.Add($"http://{address}:{port}/");
                listener.Start();

                Task.Run(async () =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            var context = await listener.GetContextAsync().WithCancellation(cts.Token);
                            _ = ProcessRequestAsync(context);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error accepting request: {ex.Message}");
                        }
                    }
                }, cts.Token);
            }
            catch (HttpListenerException ex) when (ex.ErrorCode == 48 || ex.ErrorCode == 183)
            {
                listener = null;
                cts = null;
                throw new InvalidOperationException($"Port {port} is already in use.", ex);
            }
            catch (Exception)
            {
                listener = null;
                cts = null;
                throw;
            }
        }

        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string requestInfo = $"[{timestamp}] Received {context.Request.HttpMethod} request.";
            RequestReceived?.Invoke(requestInfo);

            var response = context.Response;

            try
            {
                using var requestCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                string requestBody = await ReadRequestBodyAsync(context.Request, requestCts.Token);
                await WriteResponseAsync(response, $"[{timestamp}] Received: {requestBody}", requestCts.Token);
            }
            catch (OperationCanceledException)
            {
                response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                await WriteResponseAsync(response, "Request processing timed out", CancellationToken.None);
            }
            catch (Exception ex)
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await WriteResponseAsync(response, $"An error occurred: {ex.Message}", CancellationToken.None);
            }
            finally
            {
                response.Close();
            }
        }

        private async Task<string> ReadRequestBodyAsync(HttpListenerRequest request, CancellationToken token)
        {
            using var reader = new System.IO.StreamReader(request.InputStream);
            return await reader.ReadToEndAsync().WithCancellation(token);
        }

        private async Task WriteResponseAsync(HttpListenerResponse response, string content, CancellationToken token)
        {
            if (response.OutputStream == null)
            {
                Console.WriteLine("Warning: Response OutputStream is null");
                return;
            }

            var buffer = System.Text.Encoding.UTF8.GetBytes(content);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length, token);
        }

        public void Stop()
        {
            cts?.Cancel();
            listener?.Stop();
            listener?.Close();
            listener = null;
            cts = null;
        }
    }

    public static class TaskExtensions
    {
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using var registration = cancellationToken.Register(
                state => ((TaskCompletionSource<bool>?)state)?.TrySetResult(true),
                tcs
            );

            if (task != await Task.WhenAny(task, tcs.Task))
            {
                throw new OperationCanceledException(cancellationToken);
            }

            return await task;
        }
    }
}
