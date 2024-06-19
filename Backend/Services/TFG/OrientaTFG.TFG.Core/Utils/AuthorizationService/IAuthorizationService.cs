using System.Security.Claims;

namespace OrientaTFG.TFG.Core.Utils.AuthorizationService;

public interface IAuthorizationService
{
    /// <summary>
    /// Cheks if a student or a tutor is allowed to make changes to a TFG
    /// </summary>
    /// <param name="user">The student or tutor</param>
    /// <param name="tfgId">The tfg's id</param>
    /// <returns>True if the user is allowed to make changes to a TFG, false otherwise</returns>
    Task<bool> IsAllowed(ClaimsPrincipal user, int tfgId);
}
