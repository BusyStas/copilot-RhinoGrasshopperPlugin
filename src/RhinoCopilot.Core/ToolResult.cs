using Newtonsoft.Json;

namespace RhinoCopilot.Core
{
    /// <summary>The outcome of running a tool. Content is what the model sees.</summary>
    public sealed class ToolResult
    {
        private ToolResult(string content, bool isError)
        {
            Content = content ?? string.Empty;
            IsError = isError;
        }

        public string Content { get; }

        public bool IsError { get; }

        public static ToolResult Ok(string content)
        {
            return new ToolResult(content, false);
        }

        public static ToolResult Json(object value)
        {
            return new ToolResult(JsonConvert.SerializeObject(value, Formatting.None), false);
        }

        public static ToolResult Error(string message)
        {
            return new ToolResult(message, true);
        }
    }
}
