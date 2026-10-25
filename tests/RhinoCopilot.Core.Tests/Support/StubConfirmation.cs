using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RhinoCopilot.Core.Tests.Support
{
    public sealed class StubConfirmation : IConfirmationHandler
    {
        private readonly bool _answer;

        public StubConfirmation(bool answer)
        {
            _answer = answer;
        }

        public int AskedCount { get; private set; }

        public Task<bool> ConfirmAsync(string toolName, JObject input, CancellationToken cancellationToken)
        {
            AskedCount++;
            return Task.FromResult(_answer);
        }
    }
}
