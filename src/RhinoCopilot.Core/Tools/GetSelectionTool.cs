using System.Linq;
using Newtonsoft.Json.Linq;
using RhinoCopilot.Core.Model;

namespace RhinoCopilot.Core.Tools
{
    public sealed class GetSelectionTool : ToolBase
    {
        public const int MaxObjects = 200;

        private readonly IModelAdapter _model;

        public GetSelectionTool(IModelAdapter model)
        {
            _model = model;
        }

        public override string Name
        {
            get { return "get_selection"; }
        }

        public override string Description
        {
            get
            {
                return "Lists the objects the user currently has selected: id, type, layer, name and bounding box. " +
                       "Returns at most " + MaxObjects + " objects and reports the total count.";
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
            var all = _model.GetSelection();
            return ToolResult.Json(new
            {
                count = all.Count,
                truncated = all.Count > MaxObjects,
                objects = all.Take(MaxObjects).ToList()
            });
        }
    }
}
