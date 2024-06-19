using Microsoft.AspNetCore.Identity;
using OrientaTFG.Shared.Infrastructure.Enums;
using OrientaTFG.Shared.Infrastructure.Repository;
using System.Security.Claims;
using TFGModel = OrientaTFG.Shared.Infrastructure.Model.TFG;

namespace OrientaTFG.TFG.Core.Utils.AuthorizationService;

public class AuthorizationService : IAuthorizationService
{
    /// <summary>
    /// The TFG's repository
    /// </summary>
    private readonly IRepository<TFGModel> tfgRepository;

    /// <summary>
    /// The user manager
    /// </summary>
    private readonly UserManager<IdentityUser> userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref=AuthorizationService"/> class
    /// </summary>
    /// <param name="tfgRepository">The TFG's repository</param>
    /// <param name="userManager">The user manager</param>
    public AuthorizationService(IRepository<TFGModel> tfgRepository, UserManager<IdentityUser> userManager)
    {
        this.tfgRepository = tfgRepository;
        this.userManager = userManager;
    }

    /// <summary>
    /// Cheks if a student or a tutor is allowed to make changes to a TFG
    /// </summary>
    /// <param name="user">The student or tutor</param>
    /// <param name="tfgId">The tfg's id</param>
    /// <returns>True if the user is allowed to make changes to a TFG, false otherwise</returns>
    public async Task<bool> IsAllowed(ClaimsPrincipal user, int tfgId)
    {
        var appUser = await this.userManager.GetUserAsync(user);
        if (appUser == null)
        {
            return false;
        }

        var tfg = this.tfgRepository.GetById(tfgId);
        if (tfg == null)
        {
            return false;
        }

        int userId = int.Parse(appUser.Id);

        if (tfg.StudentId == userId && await this.userManager.IsInRoleAsync(appUser, nameof(RoleEnum.Estudiante)))
        {
            return true;
        }

        if (tfg.TutorId == userId && await this.userManager.IsInRoleAsync(appUser, nameof(RoleEnum.Tutor)))
        {
            return true;
        }

        return false;
    }
}
