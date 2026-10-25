namespace RhinoCopilot.Core.Model
{
    /// <summary>Compact description of one document object. Never contains the geometry itself.</summary>
    public sealed class ObjectSummary
    {
        public string Id { get; set; }

        public string Type { get; set; }

        public string Layer { get; set; }

        public string Name { get; set; }

        /// <summary>Bounding box minimum corner as [x, y, z], or null if unavailable.</summary>
        public double[] BoundsMin { get; set; }

        /// <summary>Bounding box maximum corner as [x, y, z], or null if unavailable.</summary>
        public double[] BoundsMax { get; set; }
    }
}
