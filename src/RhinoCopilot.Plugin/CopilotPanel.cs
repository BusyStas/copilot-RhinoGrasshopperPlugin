using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using Newtonsoft.Json.Linq;
using Rhino.UI;
using RhinoCopilot.Core;
using RhinoCopilot.Core.Agent;
using RhinoCopilot.Core.Claude;
using RhinoCopilot.Core.Tools;

namespace RhinoCopilot.Plugin
{
    /// <summary>The docked chat panel. Hosts the conversation UI and starts the agent loop.</summary>
    [Guid("214A0A72-9B7F-449B-80FF-8827F011D5A7")]
    public class CopilotPanel : Panel, IPanel
    {
        private readonly TextArea _transcript = new TextArea { ReadOnly = true, Wrap = true };
        private readonly TextArea _input = new TextArea { Wrap = true, Height = 72 };
        private readonly Button _send = new Button { Text = "Send" };
        private readonly Button _stop = new Button { Text = "Stop", Enabled = false };
        private readonly Button _newChat = new Button { Text = "New chat" };
        private readonly Conversation _conversation = new Conversation();

        private CancellationTokenSource _cancellation;
        private AgentLoop _loop;

        public CopilotPanel()
        {
            _send.Click += async (sender, e) => await OnSendAsync();
            _stop.Click += (sender, e) =>
            {
                CancellationTokenSource cancellation = _cancellation;
                if (cancellation != null)
                {
                    cancellation.Cancel();
                }
            };
            _newChat.Click += (sender, e) =>
            {
                if (_cancellation == null)
                {
                    _conversation.Clear();
                    _transcript.Text = string.Empty;
                }
            };

            var buttons = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Items = { _send, _stop, _newChat }
            };

            Content = new TableLayout
            {
                Padding = new Padding(6),
                Spacing = new Size(4, 4),
                Rows =
                {
                    new TableRow(_transcript) { ScaleHeight = true },
                    new TableRow(_input),
                    new TableRow(buttons)
                }
            };
        }

        public void PanelShown(uint documentSerialNumber, ShowPanelReason reason)
        {
        }

        public void PanelHidden(uint documentSerialNumber, ShowPanelReason reason)
        {
        }

        public void PanelClosing(uint documentSerialNumber, bool onCloseDocument)
        {
        }

        private async Task OnSendAsync()
        {
            string text = (_input.Text ?? string.Empty).Trim();
            if (text.Length == 0 || _cancellation != null)
            {
                return;
            }

            string apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Append("Set the ANTHROPIC_API_KEY environment variable, then restart Rhino.");
                return;
            }

            if (_loop == null)
            {
                _loop = CreateLoop(apiKey);
            }

            _input.Text = string.Empty;
            Append("You: " + text);

            _cancellation = new CancellationTokenSource();
            CancellationToken token = _cancellation.Token;
            SetBusy(true);

            try
            {
                AgentLoop loop = _loop;
                AgentRunResult result = await Task.Run(() => loop.RunAsync(_conversation, text, token));
                ReportResult(result);
            }
            catch (Exception ex)
            {
                Append("Error: " + ex.Message);
            }
            finally
            {
                _cancellation.Dispose();
                _cancellation = null;
                SetBusy(false);
            }
        }

        private AgentLoop CreateLoop(string apiKey)
        {
            var options = new AgentOptions();
            string model = Environment.GetEnvironmentVariable("RHINO_COPILOT_MODEL");
            if (!string.IsNullOrWhiteSpace(model))
            {
                options.Model = model.Trim();
            }

            ToolRegistry registry = DefaultTools.CreateRegistry(new RhinoModelAdapter());
            return new AgentLoop(
                new AnthropicClient(apiKey),
                registry,
                new RhinoHostContext(),
                options,
                new PanelConfirmation(),
                new PanelObserver(this));
        }

        private void ReportResult(AgentRunResult result)
        {
            switch (result.StopReason)
            {
                case AgentStopReason.MaxStepsReached:
                    Append("Stopped: reached the step limit. Send another message to continue.");
                    break;
                case AgentStopReason.Cancelled:
                    Append("Stopped.");
                    break;
                case AgentStopReason.Failed:
                    Append("Error: " + result.Error);
                    break;
            }
        }

        private void SetBusy(bool busy)
        {
            _send.Enabled = !busy;
            _newChat.Enabled = !busy;
            _stop.Enabled = busy;
        }

        private void Append(string line)
        {
            Application.Instance.AsyncInvoke(() => _transcript.Append(line + Environment.NewLine, true));
        }

        /// <summary>Shows agent progress in the transcript. Safe to call from any thread.</summary>
        private sealed class PanelObserver : IAgentObserver
        {
            private readonly CopilotPanel _panel;

            public PanelObserver(CopilotPanel panel)
            {
                _panel = panel;
            }

            public void OnAssistantText(string text)
            {
                _panel.Append("Copilot: " + text);
            }

            public void OnToolStarted(string toolName, JObject input)
            {
                _panel.Append("  > " + toolName + " " + input.ToString(Newtonsoft.Json.Formatting.None));
            }

            public void OnToolFinished(string toolName, ToolResult result)
            {
                if (result.IsError)
                {
                    _panel.Append("  ! " + toolName + " failed: " + result.Content);
                }
            }
        }

        /// <summary>Asks the user before a tool that requires confirmation runs.</summary>
        private sealed class PanelConfirmation : IConfirmationHandler
        {
            public Task<bool> ConfirmAsync(string toolName, JObject input, CancellationToken cancellationToken)
            {
                var completion = new TaskCompletionSource<bool>();
                Application.Instance.AsyncInvoke(() =>
                {
                    DialogResult answer = MessageBox.Show(
                        "Allow the copilot to run '" + toolName + "'?\n\n" + input.ToString(Newtonsoft.Json.Formatting.Indented),
                        "Copilot",
                        MessageBoxButtons.YesNo);
                    completion.SetResult(answer == DialogResult.Yes);
                });
                return completion.Task;
            }
        }
    }
}
