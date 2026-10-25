using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RhinoCopilot.Core.Claude;
using Xunit;

namespace RhinoCopilot.Core.Tests
{
    public class AnthropicClientTests
    {
        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _status;
            private readonly string _body;

            public StubHandler(HttpStatusCode status, string body)
            {
                _status = status;
                _body = body;
            }

            public HttpRequestMessage LastRequest { get; private set; }

            public string LastRequestBody { get; private set; }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequest = request;
                LastRequestBody = await request.Content.ReadAsStringAsync();
                return new HttpResponseMessage(_status)
                {
                    Content = new StringContent(_body, System.Text.Encoding.UTF8, "application/json")
                };
            }
        }

        private static ClaudeRequest SampleRequest()
        {
            return new ClaudeRequest
            {
                Model = "claude-sonnet-5-5",
                System = "be brief",
                MaxTokens = 100,
                Messages = new JArray(new JObject
                {
                    ["role"] = "user",
                    ["content"] = new JArray(new JObject { ["type"] = "text", ["text"] = "hi" })
                }),
                Tools = new JArray(new JObject { ["name"] = "t", ["description"] = "d", ["input_schema"] = new JObject { ["type"] = "object" } })
            };
        }

        [Fact]
        public async Task Sends_the_expected_headers_and_body_and_parses_the_response()
        {
            var handler = new StubHandler(
                HttpStatusCode.OK,
                "{\"stop_reason\":\"end_turn\",\"content\":[{\"type\":\"text\",\"text\":\"hello\"}]}");
            var client = new AnthropicClient("test-key", new HttpClient(handler));

            ClaudeResponse response = await client.SendAsync(SampleRequest(), CancellationToken.None);

            Assert.Equal(AnthropicClient.DefaultEndpoint, handler.LastRequest.RequestUri.ToString());
            Assert.Equal("test-key", handler.LastRequest.Headers.GetValues("x-api-key").Single());
            Assert.Equal(AnthropicClient.ApiVersion, handler.LastRequest.Headers.GetValues("anthropic-version").Single());

            JObject body = JObject.Parse(handler.LastRequestBody);
            Assert.Equal("claude-sonnet-5-5", (string)body["model"]);
            Assert.Equal(100, (int)body["max_tokens"]);
            Assert.Equal("be brief", (string)body["system"]);
            Assert.Single((JArray)body["tools"]);

            Assert.Equal("end_turn", response.StopReason);
            Assert.Equal("hello", response.GetText());
        }

        [Fact]
        public async Task Parses_tool_use_blocks()
        {
            var handler = new StubHandler(
                HttpStatusCode.OK,
                "{\"stop_reason\":\"tool_use\",\"content\":[{\"type\":\"tool_use\",\"id\":\"abc\",\"name\":\"create_box\",\"input\":{\"size_x\":2}}]}");
            var client = new AnthropicClient("test-key", new HttpClient(handler));

            ClaudeResponse response = await client.SendAsync(SampleRequest(), CancellationToken.None);

            ToolUseBlock use = response.GetToolUses().Single();
            Assert.Equal("abc", use.Id);
            Assert.Equal("create_box", use.Name);
            Assert.Equal(2, (int)use.Input["size_x"]);
        }

        [Fact]
        public async Task Throws_with_the_status_code_and_never_leaks_the_api_key()
        {
            var handler = new StubHandler(HttpStatusCode.Unauthorized, "{\"error\":{\"message\":\"invalid x-api-key\"}}");
            var client = new AnthropicClient("secret-key-123", new HttpClient(handler));

            ClaudeApiException ex = await Assert.ThrowsAsync<ClaudeApiException>(
                () => client.SendAsync(SampleRequest(), CancellationToken.None));

            Assert.Equal(401, ex.StatusCode);
            Assert.DoesNotContain("secret-key-123", ex.Message);
        }

        [Fact]
        public void Requires_an_api_key()
        {
            Assert.Throws<ArgumentException>(() => new AnthropicClient("  "));
        }
    }
}
