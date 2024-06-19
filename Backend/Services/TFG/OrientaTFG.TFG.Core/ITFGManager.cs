using OrientaTFG.TFG.Core.DTOs;

namespace OrientaTFG.TFG.Core;

public interface ITFGManager
{
    /// <summary>
    /// Assigns a new TFG
    /// </summary>
    /// <param name="tfgAssignmentDTO">The object containing the tutor's id, the student's id and the tfg's name</param>
    /// <returns>ErrorMessage if the student already has an assigned tfg</returns>
    Task<ErrorMessageDTO> AssignTFG(TFGAssignmentDTO tfgAssignmentDTO);

    /// <summary>
    /// Creates a new main task
    /// </summary>
    /// <param name="createMainTaskDTO">The object containing the information to create a new task</param>
    /// <param name="createdBy">Tha user that created the main taks, tutor or student</param>
    /// <returns>ErrorMessage if the tfg already has a main task with the same name</returns>
    Task<ErrorMessageDTO> CreateMainTask(CreateMainTaskDTO createMainTaskDTO, string createdBy);

    /// <summary>
    /// Gets the tutor's list of assigned TFGs
    /// </summary>
    /// <param name="tutorId">The tutor's id</param>
    /// <returns>List of assigned TFGs</returns>
    Task<List<TFGDTO>> GetTFGs(int tutorId);

    /// <summary>
    /// Gets the tfg's summary
    /// </summary>
    /// <param name="tfgId">The tfg's id</param>
    /// <returns>List of the tfg's main tasks summaries</returns>
    Task<List<MainTaskSummaryDTO>> GetTFGSummary(int tfgId);

    /// <summary>
    /// Gets a main task 
    /// </summary>
    /// <param name="mainTaskId">The main task's id</param>
    /// <returns>The main task</returns>
    Task<MainTaskDTO> GetMainTask(int mainTaskId);
}
