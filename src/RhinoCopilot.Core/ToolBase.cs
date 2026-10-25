using System;
using Newtonsoft.Json.Linq;

namespace RhinoCopilot.Core
{
    /// <summary>Convenience base class for tools, with helpers for reading typed input.</summary>
    public abstract class ToolBase : ITool
    {
        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract JObject InputSchema { get; }

        public virtual bool RequiresConfirmation
        {
            get { return false; }
        }

        public abstract ToolResult Execute(JObject input);

        protected static double RequireNumber(JObject input, string name)
        {
            JToken token = input == null ? null : input[name];
            if (token == null || (token.Type != JTokenType.Float && token.Type != JTokenType.Integer))
            {
                throw new ArgumentException("'" + name + "' is required and must be a number.");
            }

            double value = (double)token;
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentException("'" + name + "' must be a finite number.");
            }

            return value;
        }

        protected static double OptionalNumber(JObject input, string name, double defaultValue)
        {
            JToken token = input == null ? null : input[name];
            if (token == null || token.Type == JTokenType.Null)
            {
                return defaultValue;
            }

            return RequireNumber(input, name);
        }

        protected static string OptionalString(JObject input, string name)
        {
            JToken token = input == null ? null : input[name];
            if (token == null || token.Type == JTokenType.Null)
            {
                return null;
            }

            return (string)token;
        }
    }
}
