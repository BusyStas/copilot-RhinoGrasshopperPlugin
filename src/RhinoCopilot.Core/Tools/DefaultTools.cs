using RhinoCopilot.Core.Model;

namespace RhinoCopilot.Core.Tools
{
    public static class DefaultTools
    {
        /// <summary>Builds a registry with every built-in tool bound to the given model adapter.</summary>
        public static ToolRegistry CreateRegistry(IModelAdapter model)
        {
            var registry = new ToolRegistry();
            registry.Register(new GetDocumentInfoTool(model));
            registry.Register(new GetSelectionTool(model));
            registry.Register(new ListLayersTool(model));
            registry.Register(new CreateBoxTool(model));
            return registry;
        }
    }
}
