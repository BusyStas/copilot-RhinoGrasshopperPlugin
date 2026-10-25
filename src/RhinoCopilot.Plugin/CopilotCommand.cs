using Rhino;
using Rhino.Commands;
using Rhino.UI;

namespace RhinoCopilot.Plugin
{
    /// <summary>The "StasCopilot" command: opens the chat panel.</summary>
    public class CopilotCommand : Command
    {
        public override string EnglishName
        {
            get { return "StasCopilot"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            Panels.OpenPanel(typeof(CopilotPanel).GUID);
            return Result.Success;
        }
    }
}
