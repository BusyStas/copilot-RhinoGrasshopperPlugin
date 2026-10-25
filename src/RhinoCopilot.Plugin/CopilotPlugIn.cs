using System;
using Rhino.PlugIns;
using Rhino.UI;

namespace RhinoCopilot.Plugin
{
    /// <summary>Plugin entry point. Registers the docked chat panel when Rhino loads the plugin.</summary>
    public class CopilotPlugIn : PlugIn
    {
        public CopilotPlugIn()
        {
            Instance = this;
        }

        public static CopilotPlugIn Instance { get; private set; }

        protected override LoadReturnCode OnLoad(ref string errorMessage)
        {
            try
            {
                System.Drawing.Icon icon = LoadIcon();
                Panels.RegisterPanel(this, typeof(CopilotPanel), "Stas Copilot", icon);
                return LoadReturnCode.Success;
            }
            catch (Exception ex)
            {
                errorMessage = "Could not register the Stas Copilot panel: " + ex.Message;
                return LoadReturnCode.ErrorShowDialog;
            }
        }

        private static System.Drawing.Icon LoadIcon()
        {
            using (System.IO.Stream stream = typeof(CopilotPlugIn).Assembly.GetManifestResourceStream("CopilotIcon.ico"))
            {
                return new System.Drawing.Icon(stream);
            }
        }
    }
}
