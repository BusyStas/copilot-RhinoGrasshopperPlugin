using Newtonsoft.Json.Linq;

namespace RhinoCopilot.Core.Tests.Support
{
    /// <summary>A configurable tool for exercising the agent loop.</summary>
    public sealed class StubTool : ToolBase
    {
        private readonly string _name;
        private readonly bool _requiresConfirmation;
        private readonly string _output;

        public StubTool(string name, bool requiresConfirmation = false, string output = "ok")
        {
            _name = name;
            _requiresConfirmation = requiresConfirmation;
            _output = output;
        }

        public int ExecuteCount { get; private set; }

        public override string Name
        {
            get { return _name; }
        }

        public override string Description
        {
            get { return "A stub tool for tests."; }
        }

        public override JObject InputSchema
        {
            get { return new JObject { ["type"] = "object", ["properties"] = new JObject() }; }
        }

        public override bool RequiresConfirmation
        {
            get { return _requiresConfirmation; }
        }

        public override ToolResult Execute(JObject input)
        {
            ExecuteCount++;
            return ToolResult.Ok(_output);
        }
    }
}
