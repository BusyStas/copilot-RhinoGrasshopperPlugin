using System;

namespace RhinoCopilot.Core.Claude
{
    /// <summary>The Claude API returned a non-success status.</summary>
    public sealed class ClaudeApiException : Exception
    {
        public ClaudeApiException(int statusCode, string responseBody)
            : base(BuildMessage(statusCode, responseBody))
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }

        public int StatusCode { get; }

        public string ResponseBody { get; }

        private static string BuildMessage(int statusCode, string body)
        {
            string text = body ?? string.Empty;
            if (text.Length > 500)
            {
                text = text.Substring(0, 500) + "...";
            }

            return "Claude API returned HTTP " + statusCode + ": " + text;
        }
    }
}
