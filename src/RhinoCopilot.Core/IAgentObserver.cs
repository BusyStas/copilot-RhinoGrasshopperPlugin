using Newtonsoft.Json.Linq;

namespace RhinoCopilot.Core
{
    /// <summary>
    /// Receives progress events from the agent loop. Methods may be called from a background
    /// thread, so UI implementations must marshal to the UI thread themselves.
    /// </summary>
    public interface IAgentObserver
    {
        void OnAssistantText(string text);

        void OnToolStarted(string toolName, JObject input);

        void OnToolFinished(string toolName, ToolResult result);
    }
}
