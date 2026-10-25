using System;
using System.Threading.Tasks;

namespace RhinoCopilot.Core.Tests.Support
{
    /// <summary>Runs work inline and counts undo records so tests can check they are balanced.</summary>
    public sealed class FakeHostContext : IHostContext
    {
        public int UndoBegun { get; private set; }

        public int UndoEnded { get; private set; }

        public string LastUndoDescription { get; private set; }

        public Task<T> RunOnMainThreadAsync<T>(Func<T> work)
        {
            try
            {
                return Task.FromResult(work());
            }
            catch (Exception ex)
            {
                var completion = new TaskCompletionSource<T>();
                completion.SetException(ex);
                return completion.Task;
            }
        }

        public Task BeginUndoRecordAsync(string description)
        {
            UndoBegun++;
            LastUndoDescription = description;
            return Task.CompletedTask;
        }

        public Task EndUndoRecordAsync()
        {
            UndoEnded++;
            return Task.CompletedTask;
        }
    }
}
