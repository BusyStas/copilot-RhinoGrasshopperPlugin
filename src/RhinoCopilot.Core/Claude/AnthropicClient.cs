using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RhinoCopilot.Core.Claude
{
    /// <summary>Calls the Claude Messages API over HTTPS. The API key is never logged or put in exceptions.</summary>
    public sealed class AnthropicClient : IClaudeClient
    {
        public const string DefaultEndpoint = "https://api.anthropic.com/v1/messages";
        public const string ApiVersion = "2023-06-01";

        private static readonly HttpClient SharedHttpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };

        private readonly string _apiKey;
        private readonly HttpClient _http;
        private readonly string _endpoint;

        public AnthropicClient(string apiKey, HttpClient httpClient = null, string endpoint = DefaultEndpoint)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("An API key is required.", "apiKey");
            }

            _apiKey = apiKey.Trim();
            _http = httpClient ?? SharedHttpClient;
            _endpoint = endpoint;
        }

        public async Task<ClaudeResponse> SendAsync(ClaudeRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var body = new JObject
            {
                ["model"] = request.Model,
                ["max_tokens"] = request.MaxTokens,
                ["messages"] = request.Messages.DeepClone()
            };

            if (!string.IsNullOrEmpty(request.System))
            {
                body["system"] = request.System;
            }

            if (request.Tools != null && request.Tools.Count > 0)
            {
                body["tools"] = request.Tools.DeepClone();
            }

            using (var message = new HttpRequestMessage(HttpMethod.Post, _endpoint))
            {
                message.Headers.Add("x-api-key", _apiKey);
                message.Headers.Add("anthropic-version", ApiVersion);
                message.Content = new StringContent(body.ToString(Formatting.None), Encoding.UTF8, "application/json");

                using (HttpResponseMessage response = await _http.SendAsync(message, cancellationToken).ConfigureAwait(false))
                {
                    string text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new ClaudeApiException((int)response.StatusCode, text);
                    }

                    JObject json = JObject.Parse(text);
                    return new ClaudeResponse((string)json["stop_reason"], json["content"] as JArray);
                }
            }
        }
    }
}
