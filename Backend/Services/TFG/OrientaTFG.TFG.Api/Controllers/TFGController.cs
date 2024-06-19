using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrientaTFG.Shared.Infrastructure.Enums;
using OrientaTFG.TFG.Core;
using OrientaTFG.TFG.Core.DTOs;
using IAuthorizationService = OrientaTFG.TFG.Core.Utils.AuthorizationService.IAuthorizationService;

namespace OrientaTFG.TFG.Api.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("[controller]")]
[Authorize]
public class TFGController : ControllerBase
{
    /// <summary>
    /// The tfg manager
    /// </summary>
    private readonly ITFGManager tfgManager;

    /// <summary>
    /// The authorization service
    /// </summary>
    private readonly IAuthorizationService authorizationService;

    /// <summary>
    /// The logger
    /// </summary>
    private readonly ILogger<TFGController> logger;

    /// <summary>
    /// The class name
    /// </summary>
    private readonly string className = "OrientaTFG.TFG.Api.Controllers.TFGController";

    /// <summary>
    /// Initializes a new instance of the <see cref="TFGController"/> class
    /// </summary>
    /// <param name="tfgManager">The tfg manager</param>
    /// <param name="authorizationService">The authorization service</param>
    /// <param name="logger">The logger</param>
    public TFGController(ITFGManager tfgManager, IAuthorizationService authorizationService, ILogger<TFGController> logger) 
    {
        this.tfgManager = tfgManager;
        this.authorizationService = authorizationService;
        this.logger = logger;
    }

    /// <summary>
    /// Assigns a new TFG
    /// </summary>
    /// <param name="tfgAssignmentDTO">The object containing the tutor's id, the student's id and the tfg's name</param>
    /// <returns>ErrorMessage if the student already has an assigned tfg</returns>
    [HttpPost]
    [Authorize(Policy = nameof(RoleEnum.Tutor))]
    public async Task<IActionResult> AssignTFG([FromBody] TFGAssignmentDTO tfgAssignmentDTO)
    {
        this.logger.LogInformation($"Starting execution of AssignTFG in {this.className}");

        ErrorMessageDTO errorMesaggeDTO = await this.tfgManager.AssignTFG(tfgAssignmentDTO);

        this.logger.LogInformation($"Ended execution of AssignTFG in {this.className}");

        if (errorMesaggeDTO != null)
        {
            return Ok(new { Error = errorMesaggeDTO });
        }
        else
        {
            return Ok();
        }
    }

    /// <summary>
    /// Creates a new main task
    /// </summary>
    /// <param name="createMainTaskDTO">The object containing the information to create a new task</param>
    /// <returns>ErrorMessage if the tfg already has a main task with the same name</returns>
    [HttpPost]
    [Route("/main-task")]
    [Authorize(Policy = nameof(RoleEnum.Tutor))]
    [Authorize(Policy = nameof(RoleEnum.Estudiante))]
    public async Task<IActionResult> CreateMainTask([FromBody] CreateMainTaskDTO createMainTaskDTO)
    {
        this.logger.LogInformation($"Starting execution of CreateMainTask in {this.className}");

        if (!await this.authorizationService.IsAllowed(User, createMainTaskDTO.TFGId))
        {
            return Forbid();
        }

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

        this.logger.LogInformation($"Ended execution of CreateMainTask in {this.className}");

        if (errorMesaggeDTO != null)
        {
            return Ok(new { Error = errorMesaggeDTO });
        }
        else
        {
            return Ok();
        }
    }

    /// <summary>
    /// Gets the tfg's summary
    /// </summary>
    /// <param name="tfgId">The tfg's id</param>
    /// <returns>List of the tfg's main tasks summaries</returns>
    [HttpGet]
    [Authorize(Policy = nameof(RoleEnum.Tutor))]
    [Authorize(Policy = nameof(RoleEnum.Estudiante))]
    public async Task<IActionResult> GetTFGSummary([FromRoute] int tfgId)
    {
        this.logger.LogInformation($"Starting execution of GetTFGSummary in {this.className}");

        if (!await this.authorizationService.IsAllowed(User, tfgId))
        {
            return Forbid();
        }

        List<MainTaskSummaryDTO> tfgSummary = await this.tfgManager.GetTFGSummary(tfgId);

        this.logger.LogInformation($"Ended execution of GetTFGSummary in {this.className}");

        return Ok(tfgSummary);
    }

    /// <summary>
    /// Gets the tutor's list of assigned TFGs
    /// </summary>
    /// <param name="tutorId">The tutor's id</param>
    /// <returns>List of assigned TFGs</returns>
    [HttpGet]
    [Route("/all")]
    [Authorize(Policy = nameof(RoleEnum.Tutor))]
    public async Task<IActionResult> GetTFGs([FromRoute] int tutorId)
    {
        this.logger.LogInformation($"Starting execution of GetTFGs in {this.className}");

        List<TFGDTO> tfgs = await this.tfgManager.GetTFGs(tutorId);

        this.logger.LogInformation($"Ended execution of GetTFGs in {this.className}");

        return Ok(tfgs);
    }

    /// <summary>
    /// Gets a main task 
    /// </summary>
    /// <param name="mainTaskId">The main task's id</param>
    /// <returns>The main task</returns>
    [HttpGet]
    [Route("/main-task/{mainTaskId}")]
    [Authorize(Policy = nameof(RoleEnum.Tutor))]
    [Authorize(Policy = nameof(RoleEnum.Estudiante))]
    public async Task<IActionResult> GetMainTask([FromRoute] int mainTaskId)
    {
        this.logger.LogInformation($"Starting execution of GetMainTask in {this.className}");

        MainTaskDTO mainTask = await this.tfgManager.GetMainTask(mainTaskId);

        this.logger.LogInformation($"Ended execution of GetMainTask in {this.className}");

        return Ok(mainTask);
    }
}
