using Kirby.Core.Components;

namespace Kirby.Extensions
{
    public static class GroundCheckExtensions
    {
        public static bool IsInAir(this KirbyGroundCheck groundCheck) =>
            !groundCheck?.IsGrounded ?? false;
    }
}
