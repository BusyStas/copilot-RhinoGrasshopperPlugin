using Newtonsoft.Json.Linq;

namespace RhinoCopilot.Core.Claude
{
    /// <summary>A tool call requested by the model.</summary>
    public sealed class ToolUseBlock
    {
        public ToolUseBlock(string id, string name, JObject input)
        {
            Id = id;
            Name = name;
            Input = input ?? new JObject();
        }

        public string Id { get; }

        public string Name { get; }

        public JObject Input { get; }
    }
}
