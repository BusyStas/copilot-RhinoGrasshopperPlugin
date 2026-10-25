using Newtonsoft.Json.Linq;
using RhinoCopilot.Core.Model;

namespace RhinoCopilot.Core.Tools
{
    public sealed class GetDocumentInfoTool : ToolBase
    {
        private readonly IModelAdapter _model;

        public GetDocumentInfoTool(IModelAdapter model)
        {
            _model = model;
        }

        public override string Name
        {
            get { return "get_document_info"; }
        }

        public override string Description
        {
            get
            {
                return "Returns the active document's name, model units, object count and layer count. " +
                       "Call this first so you know the units before creating geometry.";
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
            return ToolResult.Json(_model.GetDocumentInfo());
        }
    }
}
