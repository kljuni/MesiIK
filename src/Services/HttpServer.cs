using System;
using System.Net;
using System.Threading.Tasks;
using System.Text.Json;

namespace MesiIK.Services
{
    public class HttpServer
    {
        private HttpListener listener;

        public HttpServer()
        {
            listener = new HttpListener();
        }

        public void Start(string address, int port)
        {
            listener.Prefixes.Add($"http://{address}:{port}/");
            listener.Start();

            Task.Run(async () =>
            {
                while (listener.IsListening)
                {
                    var context = await listener.GetContextAsync();
                    ProcessRequest(context);
                }
            });
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            using (var reader = new System.IO.StreamReader(context.Request.InputStream))
            {
                var body = reader.ReadToEnd();
                var response = context.Response;

                if (context.Request.ContentType?.ToLower().Contains("application/json") == true)
                {
                    var jsonDocument = JsonDocument.Parse(body);

                    var responseObject = new
                    {
                        timestamp = timestamp,
                        receivedData = jsonDocument.RootElement
                    };

                    var jsonResponse = JsonSerializer.Serialize(responseObject);
                    var buffer = System.Text.Encoding.UTF8.GetBytes(jsonResponse);

                    response.ContentType = "application/json";
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                }
                else
                {
                    var buffer = System.Text.Encoding.UTF8.GetBytes($"[{timestamp}] Received: {body}");
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                }

                response.Close();
            }
        }

        public void Stop()
        {
            listener?.Stop();
        }
    }
}
