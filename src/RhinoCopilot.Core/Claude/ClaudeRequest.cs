using Newtonsoft.Json.Linq;

namespace RhinoCopilot.Core.Claude
{
    public sealed class ClaudeRequest
    {
        public string Model { get; set; }

        public string System { get; set; }

        public int MaxTokens { get; set; }

        /// <summary>Conversation so far, in Messages API format.</summary>
        public JArray Messages { get; set; }

        /// <summary>Tool definitions, in Messages API format. May be empty.</summary>
        public JArray Tools { get; set; }
    }
}
