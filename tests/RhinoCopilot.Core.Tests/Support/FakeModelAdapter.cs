using System.Collections.Generic;
using RhinoCopilot.Core.Model;

namespace RhinoCopilot.Core.Tests.Support
{
    /// <summary>In-memory stand-in for the Rhino document.</summary>
    public sealed class FakeModelAdapter : IModelAdapter
    {
        public List<ObjectSummary> Selection { get; } = new List<ObjectSummary>();

        public List<LayerSummary> Layers { get; } = new List<LayerSummary>
        {
            new LayerSummary { Path = "Default", Visible = true, Locked = false, Color = "#000000" }
        };

        public List<string> BoxCalls { get; } = new List<string>();

        public DocumentInfo GetDocumentInfo()
        {
            return new DocumentInfo { Name = "Test", Units = "Millimeters", ObjectCount = Selection.Count, LayerCount = Layers.Count };
        }

        public IReadOnlyList<ObjectSummary> GetSelection()
        {
            return Selection;
        }

        public IReadOnlyList<LayerSummary> ListLayers()
        {
            return Layers;
        }

        public string AddBox(double x, double y, double z, double sizeX, double sizeY, double sizeZ, string layer)
        {
            BoxCalls.Add(string.Join(",", x, y, z, sizeX, sizeY, sizeZ, layer ?? "(none)"));
            return "box-" + BoxCalls.Count;
        }
    }
}
