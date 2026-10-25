using System;
using System.Collections.Generic;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using RhinoCopilot.Core.Model;

namespace RhinoCopilot.Plugin
{
    /// <summary>
    /// Implements the model adapter with RhinoCommon. Every method must run on Rhino's UI thread,
    /// which the agent loop guarantees by executing tools through the host context.
    /// </summary>
    public sealed class RhinoModelAdapter : IModelAdapter
    {
        public DocumentInfo GetDocumentInfo()
        {
            RhinoDoc doc = RequireDoc();
            return new DocumentInfo
            {
                Name = string.IsNullOrEmpty(doc.Name) ? "Untitled" : doc.Name,
                Units = doc.ModelUnitSystem.ToString(),
                ObjectCount = doc.Objects.Count,
                LayerCount = doc.Layers.ActiveCount
            };
        }

        public IReadOnlyList<ObjectSummary> GetSelection()
        {
            RhinoDoc doc = RequireDoc();
            var list = new List<ObjectSummary>();
            foreach (RhinoObject obj in doc.Objects.GetSelectedObjects(false, false))
            {
                Layer layer = doc.Layers[obj.Attributes.LayerIndex];
                var summary = new ObjectSummary
                {
                    Id = obj.Id.ToString(),
                    Type = obj.ObjectType.ToString(),
                    Layer = layer != null ? layer.FullPath : string.Empty,
                    Name = obj.Attributes.Name ?? string.Empty
                };

                BoundingBox box = obj.Geometry.GetBoundingBox(true);
                if (box.IsValid)
                {
                    summary.BoundsMin = new[] { box.Min.X, box.Min.Y, box.Min.Z };
                    summary.BoundsMax = new[] { box.Max.X, box.Max.Y, box.Max.Z };
                }

                list.Add(summary);
            }

            return list;
        }

        public IReadOnlyList<LayerSummary> ListLayers()
        {
            RhinoDoc doc = RequireDoc();
            var list = new List<LayerSummary>();
            foreach (Layer layer in doc.Layers)
            {
                if (layer.IsDeleted)
                {
                    continue;
                }

                System.Drawing.Color color = layer.Color;
                list.Add(new LayerSummary
                {
                    Path = layer.FullPath,
                    Visible = layer.IsVisible,
                    Locked = layer.IsLocked,
                    Color = string.Format("#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B)
                });
            }

            return list;
        }

        public string AddBox(double x, double y, double z, double sizeX, double sizeY, double sizeZ, string layer)
        {
            RhinoDoc doc = RequireDoc();

            Plane plane = Plane.WorldXY;
            plane.Origin = new Point3d(x, y, z);
            var box = new Box(plane, new Interval(0, sizeX), new Interval(0, sizeY), new Interval(0, sizeZ));
            Brep brep = box.ToBrep();
            if (brep == null)
            {
                throw new InvalidOperationException("Rhino could not create the box.");
            }

            var attributes = new ObjectAttributes();
            if (!string.IsNullOrWhiteSpace(layer))
            {
                attributes.LayerIndex = ResolveLayer(doc, layer.Trim());
            }

            Guid id = doc.Objects.AddBrep(brep, attributes);
            if (id == Guid.Empty)
            {
                throw new InvalidOperationException("Rhino did not add the box to the document.");
            }

            doc.Views.Redraw();
            return id.ToString();
        }

        private static RhinoDoc RequireDoc()
        {
            RhinoDoc doc = RhinoDoc.ActiveDoc;
            if (doc == null)
            {
                throw new InvalidOperationException("No active Rhino document.");
            }

            return doc;
        }

        private static int ResolveLayer(RhinoDoc doc, string path)
        {
            int index = doc.Layers.FindByFullPath(path, -1);
            if (index >= 0)
            {
                return index;
            }

            if (path.Contains("::"))
            {
                throw new InvalidOperationException(
                    "Layer '" + path + "' does not exist, and nested layers cannot be created yet. Use an existing path or a top-level name.");
            }

            index = doc.Layers.Add(path, System.Drawing.Color.Black);
            if (index < 0)
            {
                throw new InvalidOperationException("Rhino could not create layer '" + path + "'.");
            }

            return index;
        }
    }
}
