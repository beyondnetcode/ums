using System.Threading;

namespace Ums.Domain.Common
{
    /// <summary>
    /// Holds the current user identifier for the scope of a request.
    /// This is populated by middleware using <see cref="IHttpContextAccessor"/>.
    /// Domain code can use <c>CurrentUser.Value</c> to obtain the identifier.
    /// </summary>
    public static class CurrentUser
    {
        private static readonly AsyncLocal<string> _current = new();

        public static string? Value
        {
            get => _current.Value;
            set => _current.Value = value;
        }
    }
}
