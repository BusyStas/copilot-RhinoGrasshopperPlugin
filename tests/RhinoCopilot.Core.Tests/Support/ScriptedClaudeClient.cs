using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RhinoCopilot.Core.Claude;

namespace RhinoCopilot.Core.Tests.Support
{
    /// <summary>
    /// Returns pre-scripted responses in order and records every request. When the script runs
    /// out it repeats the last response, which is handy for step-limit tests.
    /// </summary>
    public sealed class ScriptedClaudeClient : IClaudeClient
    {
        private readonly Queue<ClaudeResponse> _script;
        private ClaudeResponse _last;

        public ScriptedClaudeClient(params ClaudeResponse[] responses)
        {
            _script = new Queue<ClaudeResponse>(responses);
        }

        public List<ClaudeRequest> Requests { get; } = new List<ClaudeRequest>();

        public Exception ThrowOnSend { get; set; }

        public Task<ClaudeResponse> SendAsync(ClaudeRequest request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            if (ThrowOnSend != null)
            {
                throw ThrowOnSend;
            }

            if (_script.Count > 0)
            {
                _last = _script.Dequeue();
            }

            if (_last == null)
            {
                throw new InvalidOperationException("The scripted client has no responses.");
            }

            return Task.FromResult(_last);
        }
    }
}
