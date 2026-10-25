namespace RhinoCopilot.Core.Agent
{
    public enum AgentStopReason
    {
        /// <summary>The model finished without asking for more tools.</summary>
        Finished,

        /// <summary>The step limit was reached before the model finished.</summary>
        MaxStepsReached,

        /// <summary>The user cancelled the request.</summary>
        Cancelled,

        /// <summary>An error stopped the request, for example a failed API call.</summary>
        Failed
    }
}
