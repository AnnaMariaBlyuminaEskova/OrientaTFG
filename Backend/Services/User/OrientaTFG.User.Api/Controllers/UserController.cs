using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrientaTFG.Shared.Infrastructure.Enums;
using OrientaTFG.User.Core;
using OrientaTFG.User.Core.DTOs;

namespace OrientaTFG.User.Api.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    /// <summary>
    /// The user manager
    /// </summary>
    private readonly IUserManager userManager;

    /// <summary>
    /// The logger
    /// </summary>
    private readonly ILogger<UserController> logger;

    /// <summary>
    /// The class name
    /// </summary>
    private readonly string className = "OrientaTFG.User.Api.Controllers.UserController";

    /// <summary>
    /// Initializes a new instance of the <see cref="UserController"/> class
    /// </summary>
    /// <param name="userManager">The user manager</param>
    /// <param name="logger">The logger</param>
    public UserController(IUserManager userManager, ILogger<UserController> logger) 
    {
        this.userManager = userManager;
        this.logger = logger;
    }

    /// <summary>
    /// Login method for all users
    /// </summary>
    /// <param name="logInDTO">The user's email and password</param>
    /// <returns>Token if the login was successful, error message otherwise</returns>
    [HttpPost]
    [Route("/login")]
    [AllowAnonymous]
    public async Task<IActionResult> LogIn([FromBody] LogInDTO logInDTO)
    {
        this.logger.LogInformation($"Starting execution of LogIn in {this.className}");

        LogInResponseDTO logInResponseDTO = await this.userManager.LogIn(logInDTO);

        this.logger.LogInformation($"Ended execution of LogIn in {this.className}");

        if (logInResponseDTO.ErrorMessage != null) 
        {
            return Ok(new { Error = logInResponseDTO.ErrorMessage });
        }
        else
        {
            return Ok(logInResponseDTO);
        }
    }

    /// <summary>
    /// Gets the students list
    /// </summary>
    /// <returns>The students list</returns>
    [HttpGet]
    [Route("/students")]
    [Authorize(Policy = nameof(RoleEnum.Tutor))]
    public async Task<IActionResult> GetAllStudents()
    {
        this.logger.LogInformation($"Starting execution of GetAllStudents in {this.className}");

        List<StudentDTO> students = await this.userManager.GetStudents();

        this.logger.LogInformation($"Ended execution of GetAllStudents in {this.className}");

        return Ok(students);

    }

    /// <summary>
    /// Gets the tutors list
    /// </summary>
    /// <returns>The tutors list</returns>
    [HttpGet]
    [Route("/tutors")]
    [Authorize(Policy = nameof(RoleEnum.Administrator))]
    public async Task<IActionResult> GetAllTutors()
    {
        this.logger.LogInformation($"Starting execution of GetAllTutors in {this.className}");

        List<TutorDTO> tutors = await this.userManager.GetTutors();

        this.logger.LogInformation($"Ended execution of GetAllTutors in {this.className}");

        return Ok(tutors);
    }
}
