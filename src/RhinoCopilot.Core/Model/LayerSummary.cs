namespace RhinoCopilot.Core.Model
{
    public sealed class LayerSummary
    {
        /// <summary>Full layer path, with nested layers separated by "::".</summary>
        public string Path { get; set; }

        public bool Visible { get; set; }

        public bool Locked { get; set; }

        /// <summary>Layer color as #RRGGBB.</summary>
        public string Color { get; set; }
    }
}
