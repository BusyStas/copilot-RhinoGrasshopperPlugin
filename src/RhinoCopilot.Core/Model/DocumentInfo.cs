namespace RhinoCopilot.Core.Model
{
    public sealed class DocumentInfo
    {
        public string Name { get; set; }

        /// <summary>Model unit system, for example Millimeters or Feet.</summary>
        public string Units { get; set; }

        public int ObjectCount { get; set; }

        public int LayerCount { get; set; }
    }
}
