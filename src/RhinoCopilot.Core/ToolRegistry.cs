using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace RhinoCopilot.Core
{
    /// <summary>
    /// The single place tools are registered, so the schema sent to the model and the code
    /// that runs it cannot drift apart.
    /// </summary>
    public sealed class ToolRegistry
    {
        private readonly Dictionary<string, ITool> _tools = new Dictionary<string, ITool>(StringComparer.Ordinal);
        private readonly List<ITool> _ordered = new List<ITool>();

        public IReadOnlyList<ITool> Tools
        {
            get { return _ordered; }
        }

        public void Register(ITool tool)
        {
            if (tool == null)
            {
                throw new ArgumentNullException("tool");
            }

            if (_tools.ContainsKey(tool.Name))
            {
                throw new InvalidOperationException("A tool named '" + tool.Name + "' is already registered.");
            }

            _tools.Add(tool.Name, tool);
            _ordered.Add(tool);
        }

        public bool TryGet(string name, out ITool tool)
        {
            if (name == null)
            {
                tool = null;
                return false;
            }

            return _tools.TryGetValue(name, out tool);
        }

        /// <summary>Tool definitions in the shape the Claude Messages API expects.</summary>
        public JArray GetDefinitions()
        {
            var definitions = new JArray();
            foreach (ITool tool in _ordered)
            {
                definitions.Add(new JObject
                {
                    ["name"] = tool.Name,
                    ["description"] = tool.Description,
                    ["input_schema"] = tool.InputSchema.DeepClone()
                });
            }

            return definitions;
        }
    }
}
