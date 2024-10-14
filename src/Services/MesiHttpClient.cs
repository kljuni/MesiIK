using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MesiIK.Services
{
    public class MesiHttpClient
    {
        private string? baseUrl;

        public void Configure(string address, int port)
        {
            baseUrl = $"http://{address}:{port}/";
        }

        public async Task<string> SendRequest(string body, string headers)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException("MesiHttpClient must be configured before sending requests.");
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                throw new ArgumentException("Request body must not be empty.");
            }

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(5);
                var request = new HttpRequestMessage(HttpMethod.Post, baseUrl);
                var content = new StringContent(body);

                if (!string.IsNullOrWhiteSpace(headers))
                {
                    foreach (var header in headers.Split('\n'))
                    {
                        if (string.IsNullOrWhiteSpace(header))
                        {
                            continue;
                        }

                        var parts = header.Split(':', 2);
                        if (parts.Length == 2)
                        {
                            var headerKey = parts[0].Trim();
                            var headerValue = parts[1].Trim();

                            if (!IsValidHeaderName(headerKey))
                            {
                                throw new FormatException($"Invalid header name: {headerKey}");
                            }

                            if (headerKey.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                            {
                                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(headerValue);
                            }
                            else if (headerKey.StartsWith("Content-", StringComparison.OrdinalIgnoreCase))
                            {
                                content.Headers.TryAddWithoutValidation(headerKey, headerValue);
                            }
                            else
                            {
                                request.Headers.TryAddWithoutValidation(headerKey, headerValue);
                            }
                        }
                        else
                        {
                            throw new FormatException("Headers should be in 'Key: Value' format.");
                        }
                    }
                }

                request.Content = content;

                var response = await client.SendAsync(request);
                return await response.Content.ReadAsStringAsync();
            }
        }

        private bool IsValidHeaderName(string headerName)
        {
            if (string.IsNullOrWhiteSpace(headerName))
            {
                return false;
            }

            foreach (char c in headerName)
            {
                if (!IsValidHeaderChar(c))
                {
                    return false;
                }
            }

            if (char.IsDigit(headerName[0]))
            {
                return false;
            }

            return true;
        }


        private bool IsValidHeaderChar(char c)
        {
            return (c >= 33 && c <= 126) && c != ':';
        }

    }
}
