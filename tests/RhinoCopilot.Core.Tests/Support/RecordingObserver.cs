using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace RhinoCopilot.Core.Tests.Support
{
    public sealed class RecordingObserver : IAgentObserver
    {
        public List<string> Texts { get; } = new List<string>();

        public List<string> ToolsStarted { get; } = new List<string>();

        public List<ToolResult> ToolResults { get; } = new List<ToolResult>();

        public void OnAssistantText(string text)
        {
            Texts.Add(text);
        }

        public void OnToolStarted(string toolName, JObject input)
        {
            ToolsStarted.Add(toolName);
        }

        public void OnToolFinished(string toolName, ToolResult result)
        {
            ToolResults.Add(result);
        }
    }
}
