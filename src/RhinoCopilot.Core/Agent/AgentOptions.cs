namespace RhinoCopilot.Core.Agent
{
    public sealed class AgentOptions
    {
        public string Model { get; set; } = "claude-sonnet-5-5";

        public int MaxTokens { get; set; } = 4096;

        /// <summary>Upper bound on model calls per user request, so a confused model cannot loop forever.</summary>
        public int MaxSteps { get; set; } = 20;

        /// <summary>Longest tool result sent to the model. Longer results are cut with a note.</summary>
        public int MaxToolResultChars { get; set; } = 20000;

        /// <summary>When true, tools that require confirmation run without asking.</summary>
        public bool AutoApprove { get; set; }

        public string SystemPrompt { get; set; } = SystemPrompts.Default;
    }
}
