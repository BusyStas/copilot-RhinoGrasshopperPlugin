using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RhinoCopilot.Core.Agent;
using RhinoCopilot.Core.Tests.Support;
using RhinoCopilot.Core.Tools;
using Xunit;

namespace RhinoCopilot.Core.Tests
{
    public class AgentLoopTests
    {
        private readonly FakeHostContext _host = new FakeHostContext();
        private readonly RecordingObserver _observer = new RecordingObserver();
        private readonly Conversation _conversation = new Conversation();
        private readonly FakeModelAdapter _model = new FakeModelAdapter();

        private AgentLoop CreateLoop(
            ScriptedClaudeClient client,
            ToolRegistry registry = null,
            AgentOptions options = null,
            IConfirmationHandler confirmation = null)
        {
            return new AgentLoop(
                client,
                registry ?? DefaultTools.CreateRegistry(_model),
                _host,
                options,
                confirmation,
                _observer);
        }

        [Fact]
        public async Task Finishes_when_the_model_returns_only_text()
        {
            var client = new ScriptedClaudeClient(Responses.Text("hello"));

            AgentRunResult result = await CreateLoop(client).RunAsync(_conversation, "hi", CancellationToken.None);

            Assert.Equal(AgentStopReason.Finished, result.StopReason);
            Assert.Equal(1, result.Steps);
            Assert.Equal(new[] { "hello" }, _observer.Texts.ToArray());
            Assert.Equal(1, _host.UndoBegun);
            Assert.Equal(1, _host.UndoEnded);
        }

        [Fact]
        public async Task Runs_the_requested_tool_and_sends_the_result_back()
        {
            var client = new ScriptedClaudeClient(
                Responses.ToolUse("t1", "get_document_info"),
                Responses.Text("done"));

            AgentRunResult result = await CreateLoop(client).RunAsync(_conversation, "what are the units?", CancellationToken.None);

            Assert.Equal(AgentStopReason.Finished, result.StopReason);
            Assert.Equal(2, result.Steps);
            Assert.Equal(2, client.Requests.Count);

            JArray secondRequestMessages = client.Requests[1].Messages;
            Assert.Equal(3, secondRequestMessages.Count);
            JToken toolResult = secondRequestMessages[2]["content"][0];
            Assert.Equal("tool_result", (string)toolResult["type"]);
            Assert.Equal("t1", (string)toolResult["tool_use_id"]);
            Assert.Contains("Millimeters", (string)toolResult["content"]);
            Assert.Null(toolResult["is_error"]);
        }

        [Fact]
        public async Task Sends_tool_definitions_and_the_system_prompt()
        {
            var client = new ScriptedClaudeClient(Responses.Text("hello"));

            await CreateLoop(client).RunAsync(_conversation, "hi", CancellationToken.None);

            Assert.Equal(4, client.Requests[0].Tools.Count);
            Assert.False(string.IsNullOrWhiteSpace(client.Requests[0].System));
        }

        [Fact]
        public async Task Reports_an_unknown_tool_to_the_model_and_keeps_going()
        {
            var client = new ScriptedClaudeClient(
                Responses.ToolUse("t1", "does_not_exist"),
                Responses.Text("sorry"));

            AgentRunResult result = await CreateLoop(client).RunAsync(_conversation, "go", CancellationToken.None);

            Assert.Equal(AgentStopReason.Finished, result.StopReason);
            JToken toolResult = client.Requests[1].Messages[2]["content"][0];
            Assert.True((bool)toolResult["is_error"]);
            Assert.Contains("Unknown tool", (string)toolResult["content"]);
        }

        [Fact]
        public async Task Reports_a_tool_exception_to_the_model_instead_of_throwing()
        {
            var client = new ScriptedClaudeClient(
                Responses.ToolUse("t1", "create_box", new JObject { ["size_x"] = -5, ["size_y"] = 1, ["size_z"] = 1 }),
                Responses.Text("fixed"));

            AgentRunResult result = await CreateLoop(client).RunAsync(_conversation, "make a box", CancellationToken.None);

            Assert.Equal(AgentStopReason.Finished, result.StopReason);
            JToken toolResult = client.Requests[1].Messages[2]["content"][0];
            Assert.True((bool)toolResult["is_error"]);
            Assert.Empty(_model.BoxCalls);
        }

        [Fact]
        public async Task Stops_at_the_step_limit()
        {
            var client = new ScriptedClaudeClient(Responses.ToolUse("t1", "get_document_info"));
            var options = new AgentOptions { MaxSteps = 3 };

            AgentRunResult result = await CreateLoop(client, options: options).RunAsync(_conversation, "loop", CancellationToken.None);

            Assert.Equal(AgentStopReason.MaxStepsReached, result.StopReason);
            Assert.Equal(3, result.Steps);
            Assert.Equal(1, _host.UndoEnded);
        }

        [Fact]
        public async Task Closes_the_undo_record_and_rolls_back_history_when_the_client_fails()
        {
            var client = new ScriptedClaudeClient { ThrowOnSend = new InvalidOperationException("network down") };

            AgentRunResult result = await CreateLoop(client).RunAsync(_conversation, "hi", CancellationToken.None);

            Assert.Equal(AgentStopReason.Failed, result.StopReason);
            Assert.Equal("network down", result.Error);
            Assert.Equal(1, _host.UndoBegun);
            Assert.Equal(1, _host.UndoEnded);
            Assert.Empty(_conversation.Messages);
        }

        [Fact]
        public async Task Returns_cancelled_and_closes_the_undo_record_when_cancelled()
        {
            var client = new ScriptedClaudeClient(Responses.Text("never sent"));
            var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            AgentRunResult result = await CreateLoop(client).RunAsync(_conversation, "hi", cancellation.Token);

            Assert.Equal(AgentStopReason.Cancelled, result.StopReason);
            Assert.Empty(client.Requests);
            Assert.Equal(_host.UndoBegun, _host.UndoEnded);
            Assert.Empty(_conversation.Messages);
        }

        [Fact]
        public async Task Does_not_run_a_confirmed_tool_when_the_user_declines()
        {
            var tool = new StubTool("danger", requiresConfirmation: true);
            var registry = new ToolRegistry();
            registry.Register(tool);
            var confirmation = new StubConfirmation(false);
            var client = new ScriptedClaudeClient(Responses.ToolUse("t1", "danger"), Responses.Text("ok"));

            await CreateLoop(client, registry, confirmation: confirmation).RunAsync(_conversation, "do it", CancellationToken.None);

            Assert.Equal(1, confirmation.AskedCount);
            Assert.Equal(0, tool.ExecuteCount);
            JToken toolResult = client.Requests[1].Messages[2]["content"][0];
            Assert.True((bool)toolResult["is_error"]);
        }

        [Fact]
        public async Task Runs_a_confirmed_tool_when_the_user_approves()
        {
            var tool = new StubTool("danger", requiresConfirmation: true);
            var registry = new ToolRegistry();
            registry.Register(tool);
            var confirmation = new StubConfirmation(true);
            var client = new ScriptedClaudeClient(Responses.ToolUse("t1", "danger"), Responses.Text("ok"));

            await CreateLoop(client, registry, confirmation: confirmation).RunAsync(_conversation, "do it", CancellationToken.None);

            Assert.Equal(1, tool.ExecuteCount);
        }

        [Fact]
        public async Task Auto_approve_skips_the_confirmation_prompt()
        {
            var tool = new StubTool("danger", requiresConfirmation: true);
            var registry = new ToolRegistry();
            registry.Register(tool);
            var confirmation = new StubConfirmation(false);
            var options = new AgentOptions { AutoApprove = true };
            var client = new ScriptedClaudeClient(Responses.ToolUse("t1", "danger"), Responses.Text("ok"));

            await CreateLoop(client, registry, options, confirmation).RunAsync(_conversation, "do it", CancellationToken.None);

            Assert.Equal(0, confirmation.AskedCount);
            Assert.Equal(1, tool.ExecuteCount);
        }

        [Fact]
        public async Task Truncates_very_long_tool_results()
        {
            var tool = new StubTool("big", output: new string('x', 500));
            var registry = new ToolRegistry();
            registry.Register(tool);
            var options = new AgentOptions { MaxToolResultChars = 100 };
            var client = new ScriptedClaudeClient(Responses.ToolUse("t1", "big"), Responses.Text("ok"));

            await CreateLoop(client, registry, options).RunAsync(_conversation, "go", CancellationToken.None);

            string content = (string)client.Requests[1].Messages[2]["content"][0]["content"];
            Assert.StartsWith(new string('x', 100), content);
            Assert.Contains("truncated 400 characters", content);
        }

        [Fact]
        public async Task Names_the_undo_record_after_the_request()
        {
            var client = new ScriptedClaudeClient(Responses.Text("hello"));

            await CreateLoop(client).RunAsync(_conversation, "make a tower", CancellationToken.None);

            Assert.Equal("Copilot: make a tower", _host.LastUndoDescription);
        }
    }
}
