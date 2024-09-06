using Microsoft.ApplicationInsights.WindowsServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Amqp.Framing;
using OrientaTFG.Shared.Infrastructure.Enums;
using OrientaTFG.Shared.Infrastructure.Exceptions;
using OrientaTFG.TFG.Core;
using OrientaTFG.TFG.Core.DTOs;
using System.Security.Claims;

namespace OrientaTFG.TFG.Api.Controllers;

/// <summary>
/// Initializes a new instance of the <see cref="TFGController"/> class
/// </summary>
/// <param name="tfgManager">The tfg manager</param>
[ApiVersion("1.0")]
[ApiController]
[Authorize]
public class TFGController(ITFGManager tfgManager) : ControllerBase
{
    /// <summary>
    /// The tfg manager
    /// </summary>
    private readonly ITFGManager tfgManager = tfgManager;

    /// <summary>
    /// The server error message
    /// </summary>
    private const string ServerErrorMessage = "Ha ocurrido un error. Por favor, inténtelo de nuevo más tarde.";

    /// <summary>
    /// Assigns a new TFG to a student.
    /// </summary>
    /// <param name="tfgAssignmentDTO">The object containing the tutor's ID, the student's ID, and the TFG's name.</param>
    [HttpPost("tfg")]
    [Authorize(Policy = nameof(RoleEnum.Tutor))]
    public async Task<IActionResult> AssignTFG([FromBody] TFGAssignmentDTO tfgAssignmentDTO)
    {
        try
        {
            ErrorMessageDTO errorMesaggeDTO = await this.tfgManager.AssignTFG(tfgAssignmentDTO);
            return errorMesaggeDTO.ErrorMessage == null ? Ok() : Ok(new { Error = errorMesaggeDTO.ErrorMessage });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }

    /// <summary>
    /// Retrieves the list of main task statuses.
    /// </summary>
    [HttpGet("main-task-status")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMainTaskStatuses()
    {
        return Ok(await this.tfgManager.GetMainTaskStatuses());
    }

    /// <summary>
    /// Retrieves the list of sub-task statuses.
    /// </summary>
    [HttpGet("sub-task-status")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSubTaskStatuses()
    {
        return Ok(await this.tfgManager.GetSubTaskStatuses());
    }

    /// <summary>
    /// Creates a new main task for a TFG.
    /// </summary>
    /// <param name="createMainTaskDTO">The object containing the information to create a new main task.</param>
    [HttpPost("main-task")]
    [Authorize(Policy = nameof(RoleEnum.TutorOEstudiante))]
    public async Task<IActionResult> CreateMainTask([FromBody] CreateMainTaskDTO createMainTaskDTO)
    {
        try
        {
            string createdBy = string.Empty;

            if (User.IsInRole(nameof(RoleEnum.Tutor)))
            {
                createdBy = nameof(RoleEnum.Tutor);
            }
            else if (User.IsInRole(nameof(RoleEnum.Estudiante)))
            {
                createdBy = nameof(RoleEnum.Estudiante);
            }

            ErrorMessageDTO errorMesaggeDTO = await this.tfgManager.CreateMainTask(createMainTaskDTO, createdBy);
            return errorMesaggeDTO.ErrorMessage == null ? Ok() : Ok(new { Error = errorMesaggeDTO.ErrorMessage });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }

    /// <summary>
    /// Updates an existing main task for a TFG.
    /// </summary>
    /// <param name="createMainTaskDTO">The object containing the information to update the main task.</param>
    [HttpPut("main-task")]
    [Authorize(Policy = nameof(RoleEnum.TutorOEstudiante))]
    public async Task<IActionResult> UpdateMainTask([FromBody] CreateMainTaskDTO createMainTaskDTO)
    {
        try
        {
            ErrorMessageDTO errorMesaggeDTO = await this.tfgManager.UpdateMainTask(createMainTaskDTO);
            return errorMesaggeDTO.ErrorMessage == null ? Ok() : Ok(new { Error = errorMesaggeDTO.ErrorMessage });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }

    /// <summary>
    /// Deletes a main task by its ID.
    /// </summary>
    /// <param name="mainTaskId">The ID of the main task to delete.</param>
    [HttpDelete("main-task/{mainTaskId}")]
    [Authorize(Policy = nameof(RoleEnum.TutorOEstudiante))]
    public async Task<IActionResult> DeleteMainTask([FromRoute] int mainTaskId)
    {
        try
        {
            await this.tfgManager.DeleteMainTask(mainTaskId);
            return Ok();
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }

    /// <summary>
    /// Updates the task panel.
    /// </summary>
    /// <param name="updateTaskPanel">The object containing the panel update information.</param>
    [HttpPost("panel")]
    [Authorize(Policy = nameof(RoleEnum.TutorOEstudiante))]
    public async Task<IActionResult> UpdateTaskPanel([FromBody] UpdateTaskPanel updateTaskPanel)
    {
        try
        {
            string createdBy = string.Empty;
            if (User.IsInRole(nameof(RoleEnum.Tutor)))
            {
                createdBy = nameof(RoleEnum.Tutor);
            }
            else if (User.IsInRole(nameof(RoleEnum.Estudiante)))
            {
                createdBy = nameof(RoleEnum.Estudiante);
            }
            await this.tfgManager.UpdateTaskPanel(updateTaskPanel, createdBy);
            return Ok();   
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }

    /// <summary>
    /// Gets the TFG's summary.
    /// </summary>
    /// <param name="tfgId">The TFG's ID.</param>
    [HttpGet("{tfgId}")]
    [Authorize(Policy = nameof(RoleEnum.TutorOEstudiante))]
    public async Task<IActionResult> GetTFGSummary([FromRoute] int tfgId)
    {
        try
        {
            int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            string createdBy = string.Empty;
            if (User.IsInRole(nameof(RoleEnum.Tutor)))
            {
                createdBy = nameof(RoleEnum.Tutor);
            }
            else if (User.IsInRole(nameof(RoleEnum.Estudiante)))
            {
                createdBy = nameof(RoleEnum.Estudiante);
            }
            return Ok(await this.tfgManager.GetTFGSummary(tfgId, userId, createdBy));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }

    /// <summary>
    /// Gets the student's TFG ID.
    /// </summary>
    /// <param name="studentId">The student's ID.</param>
    [HttpGet("student/{studentId}")]
    [Authorize(Policy = nameof(RoleEnum.Estudiante))]
    public async Task<IActionResult> GetStudentTFGId([FromRoute] int studentId)
    {
        try
        {
            return Ok(await this.tfgManager.GetStudentTFGId(studentId));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }

    /// <summary>
    /// Gets the tutor's list of assigned TFGs.
    /// </summary>
    /// <param name="tutorId">The tutor's ID.</param>
    [HttpGet("all/{tutorId}")]
    [Authorize(Policy = nameof(RoleEnum.Tutor))]
    public async Task<IActionResult> GetTFGs([FromRoute] int tutorId)
    {
        try
        {
            return Ok(await this.tfgManager.GetTFGs(tutorId));
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }

    /// <summary>
    /// Gets a main task by its ID.
    /// </summary>
    /// <param name="mainTaskId">The main task's ID.</param>
    [HttpGet("main-task/{mainTaskId}")]
    [Authorize(Policy = nameof(RoleEnum.TutorOEstudiante))]
    public async Task<IActionResult> GetMainTask(int mainTaskId)
    {
        try
        {
            int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            string createdBy = string.Empty;
            if (User.IsInRole(nameof(RoleEnum.Tutor)))
            {
                createdBy = nameof(RoleEnum.Tutor);
            }
            else if (User.IsInRole(nameof(RoleEnum.Estudiante)))
            {
                createdBy = nameof(RoleEnum.Estudiante);
            }
            return Ok(await this.tfgManager.GetMainTask(mainTaskId, userId, createdBy));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }

    /// <summary>
    /// Creates a comment for a task.
    /// </summary>
    /// <param name="createCommentDTO">The object containing the comment's information.</param>
    [HttpPost("comment")]
    [Authorize(Policy = nameof(RoleEnum.TutorOEstudiante))]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentDTO createCommentDTO)
    {
        try
        {
            string createdBy = string.Empty;
            if (User.IsInRole(nameof(RoleEnum.Tutor)))
            {
                createdBy = nameof(RoleEnum.Tutor);
            }
            else if (User.IsInRole(nameof(RoleEnum.Estudiante)))
            {
                createdBy = nameof(RoleEnum.Estudiante);
            }

            await this.tfgManager.CreateComment(createCommentDTO, createdBy);
            return Ok();
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }

    /// <summary>
    /// Gets the comment history for a task.
    /// </summary>
    /// <param name="id">The task's ID.</param>
    /// <param name="mainTask">True if the task is a main task, false if it is a sub-task.</param>
    [HttpGet("comment-history/{id}/{mainTask}")]
    [Authorize(Policy = nameof(RoleEnum.TutorOEstudiante))]
    public async Task<IActionResult> GetCommentHistory(int id, bool mainTask)
    {
        try
        {
            int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            string createdBy = string.Empty;
            if (User.IsInRole(nameof(RoleEnum.Tutor)))
            {
                createdBy = nameof(RoleEnum.Tutor);
            }
            else if (User.IsInRole(nameof(RoleEnum.Estudiante)))
            {
                createdBy = nameof(RoleEnum.Estudiante);
            }
            return Ok(await this.tfgManager.GetCommentHistory(id, mainTask, userId, createdBy));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }

    /// <summary>
    /// Gets the messages between the student and the tutor.
    /// </summary>
    /// <param name="tfgId">The TFG's ID.</param>
    /// <param name="limit">The limit of messages to return (default is 20).</param>
    /// <param name="beforeTimestamp">Optional timestamp to get messages before a certain date.</param>
    [HttpGet("messages/{tfgId}")]
    [Authorize(Policy = nameof(RoleEnum.TutorOEstudiante))]
    public async Task<IActionResult> GetMessages([FromRoute] int tfgId, [FromQuery] int limit = 20, [FromQuery] DateTime? beforeTimestamp = null)
    {
        try
        {
            int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            string createdBy = string.Empty;
            if (User.IsInRole(nameof(RoleEnum.Tutor)))
            {
                createdBy = nameof(RoleEnum.Tutor);
            }
            else if (User.IsInRole(nameof(RoleEnum.Estudiante)))
            {
                createdBy = nameof(RoleEnum.Estudiante);
            }
            return Ok(await this.tfgManager.GetMessages(tfgId, userId, createdBy, limit, beforeTimestamp));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }

    /// <summary>
    /// Sends a message for a TFG.
    /// </summary>
    /// <param name="tfgId">The TFG's ID.</param>
    /// <param name="message">The message to be sent.</param>
    [HttpPost("messages/{tfgId}")]
    [Authorize(Policy = nameof(RoleEnum.TutorOEstudiante))]
    public async Task<IActionResult> SendMessage([FromRoute] int tfgId, [FromBody] string message)
    {
        try
        {
            MessageDTO messageDTO = new()
            {
                Text = message,
                CreatedOn = DateTime.Now
            };

            if (User.IsInRole(nameof(RoleEnum.Tutor)))
            {
                messageDTO.CreatedBy = nameof(RoleEnum.Tutor);
            }
            else if (User.IsInRole(nameof(RoleEnum.Estudiante)))
            {
                messageDTO.CreatedBy = nameof(RoleEnum.Estudiante);
            }

            await this.tfgManager.SendMessage(tfgId, messageDTO);

            return base.Ok();
        }
        catch (Exception)
        {
            return base.StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }

    /// <summary>
    /// Updates the TFG's name.
    /// </summary>
    /// <param name="tfgId">The TFG's ID.</param>
    /// <param name="name">The new name of the TFG.</param>
    [HttpPut("{tfgId}")]
    [Authorize(Policy = nameof(RoleEnum.Tutor))]
    public async Task<IActionResult> UpdateTFG([FromRoute] int tfgId ,[FromBody] string name)
    {
        try
        {
            await this.tfgManager.UpdateTFG(tfgId, name);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }

    /// <summary>
    /// Deletes the TFG by its ID.
    /// </summary>
    /// <param name="tfgId">The TFG's ID.</param>
    [HttpDelete("{tfgId}")]
    [Authorize(Policy = nameof(RoleEnum.Tutor))]
    public async Task<IActionResult> DeleteTFG([FromRoute] int tfgId)
    {
        try
        {
            await this.tfgManager.DeleteTFG(tfgId);
            return Ok();
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ServerErrorMessage });
        }
    }
}
