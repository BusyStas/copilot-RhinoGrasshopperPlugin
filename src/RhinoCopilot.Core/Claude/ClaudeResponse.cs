using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace RhinoCopilot.Core.Claude
{
    public sealed class ClaudeResponse
    {
        public ClaudeResponse(string stopReason, JArray content)
        {
            StopReason = stopReason;
            Content = content ?? new JArray();
        }

        public string StopReason { get; }

        /// <summary>Raw content blocks, kept as-is so they can be sent back in the next request.</summary>
        public JArray Content { get; }

        /// <summary>All text blocks joined together.</summary>
        public string GetText()
        {
            var builder = new StringBuilder();
            foreach (JToken block in Content)
            {
                if ((string)block["type"] == "text")
                {
                    string text = (string)block["text"];
                    if (!string.IsNullOrEmpty(text))
                    {
                        if (builder.Length > 0)
                        {
                            builder.Append('\n');
                        }

                        builder.Append(text);
                    }
                }
            }

            return builder.ToString();
        }

        public IReadOnlyList<ToolUseBlock> GetToolUses()
        {
            var uses = new List<ToolUseBlock>();
            foreach (JToken block in Content)
            {
                if ((string)block["type"] == "tool_use")
                {
                    uses.Add(new ToolUseBlock(
                        (string)block["id"],
                        (string)block["name"],
                        block["input"] as JObject));
                }
            }

            return uses;
        }
    }
}
