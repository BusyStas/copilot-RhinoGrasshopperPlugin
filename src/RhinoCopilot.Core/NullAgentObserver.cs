using Newtonsoft.Json.Linq;

namespace RhinoCopilot.Core
{
    /// <summary>Observer that ignores every event.</summary>
    public sealed class NullAgentObserver : IAgentObserver
    {
        public static readonly NullAgentObserver Instance = new NullAgentObserver();

        public void OnAssistantText(string text)
        {
        }

        public void OnToolStarted(string toolName, JObject input)
        {
        }

        public void OnToolFinished(string toolName, ToolResult result)
        {
        }
    }
}
