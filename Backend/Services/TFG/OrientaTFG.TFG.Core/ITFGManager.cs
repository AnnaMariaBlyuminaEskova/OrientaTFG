using OrientaTFG.TFG.Core.DTOs;

namespace OrientaTFG.TFG.Core;

public interface ITFGManager
{
    /// <summary>
    /// Assigns a new TFG to a student.
    /// </summary>
    /// <param name="tfgAssignmentDTO">The object containing the tutor's id, the student's id, and the TFG's name.</param>
    /// <returns>A DTO containing the error message, if any.</returns>
    Task<ErrorMessageDTO> AssignTFG(TFGAssignmentDTO tfgAssignmentDTO);

    /// <summary>
    /// Creates a new main task for a TFG.
    /// </summary>
    /// <param name="createMainTaskDTO">The object containing the information to create a new main task.</param>
    /// <param name="createdBy">The user who created the main task, either tutor or student.</param>
    /// <returns>A DTO containing the error message, if any.</returns>
    Task<ErrorMessageDTO> CreateMainTask(CreateMainTaskDTO createMainTaskDTO, string createdBy);

    /// <summary>
    /// Updates an existing main task for a TFG.
    /// </summary>
    /// <param name="createMainTaskDTO">The object containing the information to update the main task.</param>
    /// <returns>A DTO containing the error message, if any.</returns>
    Task<ErrorMessageDTO> UpdateMainTask(CreateMainTaskDTO createMainTaskDTO);

    /// <summary>
    /// Updates the task panel, handling sub-tasks.
    /// </summary>
    /// <param name="updateTaskPanel">The object containing information about the task panel update, including sub-tasks.</param>
    /// <param name="createdBy">The user who performed the update, either tutor or student.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task UpdateTaskPanel(UpdateTaskPanel updateTaskPanel, string createdBy);

    /// <summary>
    /// Checks and sends task alerts based on the student's alert configuration.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task CheckAndSendTaskAlerts();

    /// <summary>
    /// Gets the list of TFGs assigned to a tutor.
    /// </summary>
    /// <param name="tutorId">The ID of the tutor.</param>
    /// <returns>A list of TFGDTO objects representing the TFGs assigned to the tutor.</returns>
    Task<List<TFGDTO>> GetTFGs(int tutorId);

    /// <summary>
    /// Gets the summary of a TFG.
    /// </summary>
    /// <param name="tfgId">The ID of the TFG.</param>
    /// <param name="userId">The ID of the user requesting the summary.</param>
    /// <param name="createdBy">The role of the user requesting the summary (either tutor or student).</param>
    /// <returns>A DTO containing the summary of the TFG, including main tasks.</returns>
    Task<TFGSummaryDTO> GetTFGSummary(int tfgId, int userId, string createdBy);

    /// <summary>
    /// Gets a main task by its ID.
    /// </summary>
    /// <param name="mainTaskId">The ID of the main task.</param>
    /// <param name="userId">The ID of the user requesting the main task.</param>
    /// <param name="createdBy">The role of the user requesting the main task (either tutor or student).</param>
    /// <returns>A DTO containing the main task information.</returns>
    Task<MainTaskDTO> GetMainTask(int mainTaskId, int userId, string createdBy);

    /// <summary>
    /// Gets the TFG ID for a given student.
    /// </summary>
    /// <param name="studentId">The ID of the student.</param>
    /// <returns>The ID of the TFG assigned to the student.</returns>
    Task<int> GetStudentTFGId(int studentId);

    /// <summary>
    /// Creates a comment on a task (main or sub-task).
    /// </summary>
    /// <param name="createCommentDTO">The object containing the comment's information.</param>
    /// <param name="createdBy">The user who created the comment, either tutor or student.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task CreateComment(CreateCommentDTO createCommentDTO, string createdBy);

    /// <summary>
    /// Gets the comment history for a specific task.
    /// </summary>
    /// <param name="id">The ID of the task.</param>
    /// <param name="mainTask">True if the task is a main task, false if it is a sub-task.</param>
    /// <param name="userId">The ID of the user requesting the comment history.</param>
    /// <param name="createdBy">The role of the user requesting the comment history (either tutor or student).</param>
    /// <returns>A DTO containing the history of comments for the specified task.</returns>
    Task<CommentHistoryDTO> GetCommentHistory(int id, bool mainTask, int userId, string createdBy);

    /// <summary>
    /// Gets the messages exchanged between the student and the tutor, with pagination and timestamp support.
    /// </summary>
    /// <param name="tfgId">The ID of the TFG.</param>
    /// <param name="userId">The ID of the user requesting the messages.</param>
    /// <param name="createdBy">The role of the user requesting the messages (either tutor or student).</param>
    /// <param name="limit">The maximum number of messages to return.</param>
    /// <param name="beforeTimestamp">Only messages created before this timestamp will be returned.</param>
    /// <returns>A list of MessageDTO objects representing the messages exchanged between the student and the tutor.</returns>
    Task<List<MessageDTO>> GetMessages(int tfgId, int userId, string createdBy, int limit = 20, DateTime? beforeTimestamp = null);

    /// <summary>
    /// Updates the name of a TFG.
    /// </summary>
    /// <param name="tfgId">The ID of the TFG.</param>
    /// <param name="name">The new name for the TFG.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task UpdateTFG(int tfgId, string name);

    /// <summary>
    /// Sends a message between the student and the tutor.
    /// </summary>
    /// <param name="tfgId">The ID of the TFG.</param>
    /// <param name="messageDTO">The object containing the message information.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task SendMessage(int tfgId, MessageDTO messageDTO);

    /// <summary>
    /// Deletes a main task, along with its sub-tasks and comments.
    /// </summary>
    /// <param name="mainTaskId">The ID of the main task to delete.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task DeleteMainTask(int mainTaskId);

    /// <summary>
    /// Deletes a TFG and all its associated tasks, subtasks and comments.
    /// </summary>
    /// <param name="tfgId">The ID of the TFG to delete.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task DeleteTFG(int tfgId);

    /// <summary>
    /// Gets the main task statuses from the master squema.
    /// </summary>
    /// <returns>A list of StatusDTO objects representing the statuses.</returns>
    Task<List<StatusDTO>> GetMainTaskStatuses();

    /// <summary>
    /// Gets the sub task statuses from the master squema.
    /// </summary>
    /// <returns>A list of StatusDTO objects representing the statuses.</returns>
    Task<List<StatusDTO>> GetSubTaskStatuses();

}
