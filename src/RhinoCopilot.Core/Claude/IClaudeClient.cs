using System.Threading;
using System.Threading.Tasks;

namespace RhinoCopilot.Core.Claude
{
    /// <summary>Sends one request to the Claude Messages API. Fakes implement this in tests.</summary>
    public interface IClaudeClient
    {
        Task<ClaudeResponse> SendAsync(ClaudeRequest request, CancellationToken cancellationToken);
    }
}
