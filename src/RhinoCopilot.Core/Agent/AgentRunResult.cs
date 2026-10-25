namespace RhinoCopilot.Core.Agent
{
    public sealed class AgentRunResult
    {
        private AgentRunResult(AgentStopReason stopReason, int steps, string error)
        {
            StopReason = stopReason;
            Steps = steps;
            Error = error;
        }

        public AgentStopReason StopReason { get; }

        /// <summary>Number of model calls made.</summary>
        public int Steps { get; }

        /// <summary>Error message when StopReason is Failed, otherwise null.</summary>
        public string Error { get; }

        public static AgentRunResult Finished(int steps)
        {
            return new AgentRunResult(AgentStopReason.Finished, steps, null);
        }

        public static AgentRunResult MaxStepsReached(int steps)
        {
            return new AgentRunResult(AgentStopReason.MaxStepsReached, steps, null);
        }

        public static AgentRunResult Cancelled(int steps)
        {
            return new AgentRunResult(AgentStopReason.Cancelled, steps, null);
        }

        public static AgentRunResult Failed(int steps, string error)
        {
            return new AgentRunResult(AgentStopReason.Failed, steps, error);
        }
    }
}
