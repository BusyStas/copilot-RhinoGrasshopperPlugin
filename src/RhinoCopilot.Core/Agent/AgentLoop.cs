using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RhinoCopilot.Core.Claude;

namespace RhinoCopilot.Core.Agent
{
    /// <summary>
    /// Runs one user request to completion: ask the model, run the tools it calls on the main
    /// thread, send the results back, and repeat. The whole request is one undo record.
    /// </summary>
    public sealed class AgentLoop
    {
        private readonly IClaudeClient _client;
        private readonly ToolRegistry _registry;
        private readonly IHostContext _host;
        private readonly AgentOptions _options;
        private readonly IConfirmationHandler _confirmation;
        private readonly IAgentObserver _observer;

        public AgentLoop(
            IClaudeClient client,
            ToolRegistry registry,
            IHostContext host,
            AgentOptions options = null,
            IConfirmationHandler confirmation = null,
            IAgentObserver observer = null)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (registry == null) throw new ArgumentNullException("registry");
            if (host == null) throw new ArgumentNullException("host");

            _client = client;
            _registry = registry;
            _host = host;
            _options = options ?? new AgentOptions();
            _confirmation = confirmation;
            _observer = observer ?? NullAgentObserver.Instance;
        }

        public async Task<AgentRunResult> RunAsync(Conversation conversation, string userText, CancellationToken cancellationToken)
        {
            if (conversation == null) throw new ArgumentNullException("conversation");
            if (string.IsNullOrWhiteSpace(userText)) throw new ArgumentException("The request is empty.", "userText");

            int checkpoint = conversation.Messages.Count;
            int steps = 0;
            bool undoOpen = false;
            conversation.AddUserText(userText);
            JArray toolDefinitions = _registry.GetDefinitions();

            try
            {
                await _host.BeginUndoRecordAsync("Stas Copilot: " + Shorten(userText, 40)).ConfigureAwait(false);
                undoOpen = true;

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (steps >= _options.MaxSteps)
                    {
                        return AgentRunResult.MaxStepsReached(steps);
                    }

                    steps++;
                    var request = new ClaudeRequest
                    {
                        Model = _options.Model,
                        System = _options.SystemPrompt,
                        MaxTokens = _options.MaxTokens,
                        Messages = (JArray)conversation.Messages.DeepClone(),
                        Tools = toolDefinitions
                    };

                    ClaudeResponse response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
                    conversation.AddAssistant(response.Content);

                    string text = response.GetText();
                    if (text.Length > 0)
                    {
                        _observer.OnAssistantText(text);
                    }

                    IReadOnlyList<ToolUseBlock> toolUses = response.GetToolUses();
                    if (toolUses.Count == 0)
                    {
                        return AgentRunResult.Finished(steps);
                    }

                    var resultBlocks = new List<JObject>();
                    foreach (ToolUseBlock use in toolUses)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        ToolResult result = await ExecuteToolAsync(use, cancellationToken).ConfigureAwait(false);
                        resultBlocks.Add(BuildToolResultBlock(use.Id, result));
                    }

                    conversation.AddToolResults(resultBlocks);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                conversation.TruncateTo(checkpoint);
                return AgentRunResult.Cancelled(steps);
            }
            catch (OperationCanceledException)
            {
                // Cancellation that the user did not request, for example an HTTP timeout.
                conversation.TruncateTo(checkpoint);
                return AgentRunResult.Failed(steps, "The request timed out.");
            }
            catch (Exception ex)
            {
                conversation.TruncateTo(checkpoint);
                return AgentRunResult.Failed(steps, ex.Message);
            }
            finally
            {
                if (undoOpen)
                {
                    try
                    {
                        await _host.EndUndoRecordAsync().ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                        // Nothing sensible can be done if closing the undo record fails.
                    }
                }
            }
        }

        private async Task<ToolResult> ExecuteToolAsync(ToolUseBlock use, CancellationToken cancellationToken)
        {
            ITool tool;
            if (!_registry.TryGet(use.Name, out tool))
            {
                ToolResult unknown = ToolResult.Error("Unknown tool: " + use.Name);
                _observer.OnToolStarted(use.Name, use.Input);
                _observer.OnToolFinished(use.Name, unknown);
                return unknown;
            }

            _observer.OnToolStarted(use.Name, use.Input);

            if (tool.RequiresConfirmation && !_options.AutoApprove)
            {
                bool approved = _confirmation != null &&
                    await _confirmation.ConfirmAsync(use.Name, use.Input, cancellationToken).ConfigureAwait(false);
                if (!approved)
                {
                    ToolResult declined = ToolResult.Error("The user declined to run this tool.");
                    _observer.OnToolFinished(use.Name, declined);
                    return declined;
                }
            }

            ToolResult result;
            try
            {
                JObject input = use.Input;
                result = await _host.RunOnMainThreadAsync(() => tool.Execute(input)).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                result = ToolResult.Error(ex.GetType().Name + ": " + ex.Message);
            }

            _observer.OnToolFinished(use.Name, result);
            return result;
        }

        private JObject BuildToolResultBlock(string toolUseId, ToolResult result)
        {
            string content = result.Content;
            if (content.Length > _options.MaxToolResultChars)
            {
                int dropped = content.Length - _options.MaxToolResultChars;
                content = content.Substring(0, _options.MaxToolResultChars) +
                    "\n[truncated " + dropped + " characters; ask a narrower question]";
            }

            var block = new JObject
            {
                ["type"] = "tool_result",
                ["tool_use_id"] = toolUseId,
                ["content"] = content
            };

            if (result.IsError)
            {
                block["is_error"] = true;
            }

            return block;
        }

        private static string Shorten(string text, int maxLength)
        {
            string single = text.Replace('\r', ' ').Replace('\n', ' ').Trim();
            return single.Length <= maxLength ? single : single.Substring(0, maxLength) + "...";
        }
    }
}
