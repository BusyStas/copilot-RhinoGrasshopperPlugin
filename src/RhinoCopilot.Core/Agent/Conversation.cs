using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace RhinoCopilot.Core.Agent
{
    /// <summary>The message history of one chat, kept in Messages API format.</summary>
    public sealed class Conversation
    {
        public JArray Messages { get; } = new JArray();

        public void AddUserText(string text)
        {
            Messages.Add(new JObject
            {
                ["role"] = "user",
                ["content"] = new JArray
                {
                    new JObject { ["type"] = "text", ["text"] = text }
                }
            });
        }

        public void AddAssistant(JArray content)
        {
            Messages.Add(new JObject
            {
                ["role"] = "assistant",
                ["content"] = content.DeepClone()
            });
        }

        public void AddToolResults(IEnumerable<JObject> results)
        {
            var content = new JArray();
            foreach (JObject result in results)
            {
                content.Add(result);
            }

            Messages.Add(new JObject
            {
                ["role"] = "user",
                ["content"] = content
            });
        }

        /// <summary>Drops everything after the first <paramref name="count"/> messages.</summary>
        public void TruncateTo(int count)
        {
            while (Messages.Count > count)
            {
                Messages.RemoveAt(Messages.Count - 1);
            }
        }

        public void Clear()
        {
            Messages.Clear();
        }
    }
}
