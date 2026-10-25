using Newtonsoft.Json.Linq;
using RhinoCopilot.Core.Model;

namespace RhinoCopilot.Core.Tools
{
    public sealed class ListLayersTool : ToolBase
    {
        private readonly IModelAdapter _model;

        public ListLayersTool(IModelAdapter model)
        {
            _model = model;
        }

        public override string Name
        {
            get { return "list_layers"; }
        }

        public override string Description
        {
            get
            {
                return "Lists the document's layers with full path (nested layers use '::'), visibility, lock state and color.";
            }
        }

        public override JObject InputSchema
        {
            get
            {
                return new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject()
                };
            }
        }

        public override ToolResult Execute(JObject input)
        {
            var layers = _model.ListLayers();
            return ToolResult.Json(new { count = layers.Count, layers = layers });
        }
    }
}
