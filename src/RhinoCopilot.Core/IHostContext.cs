using System;
using System.Threading.Tasks;

namespace RhinoCopilot.Core
{
    /// <summary>
    /// What the agent loop needs from the host application: a way to run work on the
    /// main (UI) thread and a way to group a whole request into one undo step.
    /// </summary>
    public interface IHostContext
    {
        Task<T> RunOnMainThreadAsync<T>(Func<T> work);

        Task BeginUndoRecordAsync(string description);

        Task EndUndoRecordAsync();
    }
}
