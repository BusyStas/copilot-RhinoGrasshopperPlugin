using System;
using Newtonsoft.Json.Linq;
using RhinoCopilot.Core.Model;

namespace RhinoCopilot.Core.Tools
{
    public sealed class CreateBoxTool : ToolBase
    {
        private readonly IModelAdapter _model;

        public CreateBoxTool(IModelAdapter model)
        {
            _model = model;
        }

        public override string Name
        {
            get { return "create_box"; }
        }

        public override string Description
        {
            get
            {
                return "Creates a solid axis-aligned box. (x, y, z) is the corner with the smallest coordinates and " +
                       "the size_* values extend along +X, +Y and +Z. All values are in the document's model units. " +
                       "Optionally places the box on an existing layer, or creates a new top-level layer with that name.";
            }
        }

        public override JObject InputSchema
        {
            get
            {
                return new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["x"] = Number("X of the corner with the smallest coordinates. Default 0."),
                        ["y"] = Number("Y of the corner with the smallest coordinates. Default 0."),
                        ["z"] = Number("Z of the corner with the smallest coordinates. Default 0."),
                        ["size_x"] = Number("Length along X. Must be greater than 0."),
                        ["size_y"] = Number("Length along Y. Must be greater than 0."),
                        ["size_z"] = Number("Length along Z. Must be greater than 0."),
                        ["layer"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "Layer name. Optional; the current layer is used if omitted."
                        }
                    },
                    ["required"] = new JArray("size_x", "size_y", "size_z")
                };
            }
        }

        public override ToolResult Execute(JObject input)
        {
            double x = OptionalNumber(input, "x", 0);
            double y = OptionalNumber(input, "y", 0);
            double z = OptionalNumber(input, "z", 0);
            double sizeX = RequireNumber(input, "size_x");
            double sizeY = RequireNumber(input, "size_y");
            double sizeZ = RequireNumber(input, "size_z");

            if (sizeX <= 0 || sizeY <= 0 || sizeZ <= 0)
            {
                throw new ArgumentException("size_x, size_y and size_z must all be greater than 0.");
            }

            string layer = OptionalString(input, "layer");
            string id = _model.AddBox(x, y, z, sizeX, sizeY, sizeZ, layer);
            return ToolResult.Json(new { id = id });
        }

        private static JObject Number(string description)
        {
            return new JObject
            {
                ["type"] = "number",
                ["description"] = description
            };
        }
    }
}
