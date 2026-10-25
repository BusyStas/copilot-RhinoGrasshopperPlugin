using System;
using System.Threading.Tasks;
using Rhino;
using RhinoCopilot.Core;

namespace RhinoCopilot.Plugin
{
    /// <summary>
    /// Runs work on Rhino's UI thread and groups a request into one undo record.
    /// The document is remembered when the record opens, so a document switch mid-request
    /// cannot leave a record open on the wrong document.
    /// </summary>
    public sealed class RhinoHostContext : IHostContext
    {
        private RhinoDoc _undoDoc;
        private uint _undoSerial;

        public Task<T> RunOnMainThreadAsync<T>(Func<T> work)
        {
            var completion = new TaskCompletionSource<T>();
            RhinoApp.InvokeOnUiThread(new Action(() =>
            {
                try
                {
                    completion.SetResult(work());
                }
                catch (Exception ex)
                {
                    completion.SetException(ex);
                }
            }));
            return completion.Task;
        }

        public Task BeginUndoRecordAsync(string description)
        {
            return RunOnMainThreadAsync(() =>
            {
                RhinoDoc doc = RhinoDoc.ActiveDoc;
                if (doc == null)
                {
                    throw new InvalidOperationException("No active Rhino document.");
                }

                _undoDoc = doc;
                _undoSerial = doc.BeginUndoRecord(description);
                return true;
            });
        }

        public Task EndUndoRecordAsync()
        {
            return RunOnMainThreadAsync(() =>
            {
                if (_undoDoc != null && _undoSerial != 0)
                {
                    _undoDoc.EndUndoRecord(_undoSerial);
                    _undoDoc.Views.Redraw();
                }

                _undoDoc = null;
                _undoSerial = 0;
                return true;
            });
        }
    }
}
