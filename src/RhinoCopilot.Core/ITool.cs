using Newtonsoft.Json.Linq;

namespace RhinoCopilot.Core
{
    /// <summary>
    /// A capability the model can call. Execute is always invoked on the host's main thread,
    /// so implementations may touch the Rhino document directly (through an adapter).
    /// </summary>
    public interface ITool
    {
        /// <summary>Unique name, letters, digits, underscores and hyphens only.</summary>
        string Name { get; }

        /// <summary>Tells the model what the tool does and when to use it.</summary>
        string Description { get; }

        /// <summary>JSON schema (type "object") describing the input.</summary>
        JObject InputSchema { get; }

        /// <summary>True for operations the user must approve (deleting, running scripts, overwriting files).</summary>
        bool RequiresConfirmation { get; }

        ToolResult Execute(JObject input);
    }
}
