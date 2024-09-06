using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrientaTFG.Shared.Infrastructure.Enums;
using OrientaTFG.User.Core;
using OrientaTFG.User.Core.DTOs;
using System.Security.Claims;

namespace OrientaTFG.User.Api.Controllers;

/// <summary>
/// Initializes a new instance of the <see cref="UserController"/> class
/// </summary>
/// <param name="userManager">The user manager</param>
[ApiVersion("1.0")]
[ApiController]
[Authorize]
public class UserController(IUserManager userManager) : ControllerBase
{
    /// <summary>
    /// The user manager
    /// </summary>
    private readonly IUserManager userManager = userManager;

    /// <summary>
    /// The server error message
    /// </summary>
    private const string ServerErrorMessage = "Ha ocurrido un error. Por favor, inténtelo de nuevo más tarde.";

    /// <summary>
    /// Authenticates the user and returns a token if successful.
    /// </summary>
    /// <param name="logInDTO">Contains the user's email and password.</param>
    /// <returns>A token if login is successful; otherwise, an error message.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> LogIn([FromBody] LogInDTO logInDTO)
    {
        try
        {
            LogInResponseDTO logInResponseDTO = await this.userManager.LogIn(logInDTO);
            return logInResponseDTO.ErrorMessage == null ? Ok(logInResponseDTO) : Ok(new { Error = logInResponseDTO.ErrorMessage });
        }
        catch (Exception) 
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }


    /// <summary>
    /// Registers a new student.
    /// </summary>
    /// <param name="registryDTO">Contains the student's details.</param>
    /// <returns>A token if registration is successful; otherwise, an error message.</returns>
    [HttpPost("register-student")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterStudent([FromBody] RegistryDTO registryDTO)
    {
        try
        {
            LogInResponseDTO logInResponseDTO = await this.userManager.RegisterStudent(registryDTO);
            return logInResponseDTO.ErrorMessage == null ? Ok(logInResponseDTO) : Ok(new { Error = logInResponseDTO.ErrorMessage });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }

    /// <summary>
    /// Registers a new tutor.
    /// </summary>
    /// <param name="tutorRegistryDTO">Contains the tutor's details.</param>
    /// <returns>A status indicating success or failure.</returns>
    [HttpPost("register-tutor")]
    [Authorize(Policy = nameof(RoleEnum.Administrator))]
    public async Task<IActionResult> RegisterTutor([FromBody] TutorRegistryDTO tutorRegistryDTO)
    {
        try
        {
            ErrorMessageDTO errorMessageDTO = await this.userManager.RegisterTutor(tutorRegistryDTO);
            return errorMessageDTO.ErrorMessage == null ? Ok() : Ok(new { Error = errorMessageDTO.ErrorMessage });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }

    /// <summary>
    /// Retrieves the list of all students.
    /// </summary>
    /// <returns>The list of all students.</returns>
    [HttpGet("students")]
    [Authorize(Policy = nameof(RoleEnum.Tutor))]
    public async Task<IActionResult> GetAllStudents()
    {
        try
        {
            return Ok(await this.userManager.GetStudents());
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }

    /// <summary>
    /// Retrieves the list of all tutors with their TFGs.
    /// </summary>
    /// <returns>The list of all tutors with their TFGs.</returns>
    [HttpGet("tutors")]
    [Authorize(Policy = nameof(RoleEnum.Administrator))]
    public async Task<IActionResult> GetAllTutorsAndTFGs()
    {
        try
        {
            return Ok(await this.userManager.GetTutorsAndTFGs());
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }

    /// <summary>
    /// Retrieves the student's tutor.
    /// </summary>
    /// <returns>The student's tutor</returns>
    [HttpGet("student-tutor/{studentId}")]
    [Authorize(Policy = nameof(RoleEnum.Estudiante))]
    public async Task<IActionResult> GetStudentTutor(int studentId)
    {
        try
        {
            return Ok(await this.userManager.GetStudentTutor(studentId));
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }

    /// <summary>
    /// Retrieves the list of all departments.
    /// </summary>
    /// <returns>The list of all departments.</returns>
    [HttpGet("departments")]
    [Authorize(Policy = nameof(RoleEnum.Administrator))]
    public async Task<IActionResult> GetAllDepartments()
    {
        try
        {
            return Ok(await this.userManager.GetDepartments());
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }

    /// <summary>
    /// Retrieves the profile of a student.
    /// </summary>
    /// <param name="studentId">The ID of the student.</param>
    /// <returns>The student's profile.</returns>
    [HttpGet("student-profile/{studentId}")]
    [Authorize(Policy = nameof(RoleEnum.Estudiante))]
    public async Task<IActionResult> GetStudentProfile([FromRoute] int studentId)
    {
        try
        {
            return Ok(await this.userManager.GetStudentProfile(studentId));
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }

    /// <summary>
    /// Retrieves the profile of a tutor.
    /// </summary>
    /// <param name="tutorId">The ID of the tutor.</param>
    /// <returns>The tutor's profile.</returns>
    [HttpGet("tutor-profile/{tutorId}")]
    [Authorize(Policy = nameof(RoleEnum.Tutor))]
    public async Task<IActionResult> GetTutorProfile([FromRoute] int tutorId)
    {
        try
        {
            int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (tutorId != currentUserId)
            {
                return Forbid(); 
            }

            var profile = await this.userManager.GetTutorProfile(tutorId);

            if (profile == null)
            {
                return NotFound();
            }

            return Ok(profile);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }

    /// <summary>
    /// Updates the profile of a student.
    /// </summary>
    /// <param name="updateStudentProfileDTO">The data to update the student's profile.</param>
    [HttpPut("student-profile")]
    [Authorize(Policy = nameof(RoleEnum.Estudiante))]
    public async Task<IActionResult> UpdateStudentProfile([FromBody] UpdateStudentProfileDTO updateStudentProfileDTO)
    {
        try
        {
            await this.userManager.UpdateStudentProfile(updateStudentProfileDTO);
            return Ok();
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }

    /// <summary>
    /// Updates the profile of a tutor.
    /// </summary>
    /// <param name="updateProfileDTO">The data to update the tutor's profile.</param>
    [HttpPut("tutor-profile")]
    [Authorize(Policy = nameof(RoleEnum.Tutor))]
    public async Task<IActionResult> UpdateTutorProfile([FromBody] UpdateProfileDTO updateProfileDTO)
    {
        try
        {
            await this.userManager.UpdateTutorProfile(updateProfileDTO);
            return Ok();
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }
}
