using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RhinoCopilot.Core
{
    /// <summary>Asks the user whether a tool that requires confirmation may run.</summary>
    public interface IConfirmationHandler
    {
        Task<bool> ConfirmAsync(string toolName, JObject input, CancellationToken cancellationToken);
    }
}
