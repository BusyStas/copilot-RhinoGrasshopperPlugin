using Newtonsoft.Json.Linq;
using RhinoCopilot.Core.Claude;

namespace RhinoCopilot.Core.Tests.Support
{
    /// <summary>Builders for model responses used in tests.</summary>
    public static class Responses
    {
        public static ClaudeResponse Text(string text)
        {
            return new ClaudeResponse("end_turn", new JArray(
                new JObject { ["type"] = "text", ["text"] = text }));
        }

        public static ClaudeResponse ToolUse(string id, string name, JObject input = null)
        {
            return new ClaudeResponse("tool_use", new JArray(
                new JObject
                {
                    ["type"] = "tool_use",
                    ["id"] = id,
                    ["name"] = name,
                    ["input"] = input ?? new JObject()
                }));
        }
    }
}
