using System.Collections.Generic;

namespace RhinoCopilot.Core.Model
{
    /// <summary>
    /// The only surface tools use to touch the model. The plugin implements it with RhinoCommon;
    /// tests implement it with an in-memory fake. All members run on the host's main thread.
    /// </summary>
    public interface IModelAdapter
    {
        DocumentInfo GetDocumentInfo();

        IReadOnlyList<ObjectSummary> GetSelection();

        IReadOnlyList<LayerSummary> ListLayers();

        /// <summary>Adds an axis-aligned box. Returns the new object's id.</summary>
        string AddBox(double x, double y, double z, double sizeX, double sizeY, double sizeZ, string layer);
    }
}
