using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OrientaTFG.Shared.Infrastructure.Enums;
using OrientaTFG.Shared.Infrastructure.Exceptions;
using OrientaTFG.Shared.Infrastructure.Model;
using OrientaTFG.Shared.Infrastructure.Repository;
using OrientaTFG.Shared.Infrastructure.Utils.StorageClient;
using OrientaTFG.TFG.Core.DTOs;
using OrientaTFG.TFG.Core.Utils.QueueMessageSender;
using Message = OrientaTFG.Shared.Infrastructure.Model.Message;
using TFGModel = OrientaTFG.Shared.Infrastructure.Model.TFG;

namespace OrientaTFG.TFG.Core;

public class TFGManager : ITFGManager
{
    /// <summary>
    /// The TFG's repository
    /// </summary>
    private readonly IRepository<TFGModel> tfgRepository;

    /// <summary>
    /// The main task's repository
    /// </summary>
    private readonly IRepository<MainTask> mainTaskRepository;

    /// <summary>
    /// The main task's status repository
    /// </summary>
    private readonly IRepository<MainTaskStatus> mainTaskStatusRepository;

    /// <summary>
    /// The sub task's repository
    /// </summary>
    private readonly IRepository<SubTask> subTaskRepository;

    /// <summary>
    /// The sub task's status repository
    /// </summary>
    private readonly IRepository<SubTaskStatus> subTaskStatusRepository;

    /// <summary>
    /// The comment's repository
    /// </summary>
    private readonly IRepository<Comment> commentRepository;

    /// <summary>
    /// The message's repository
    /// </summary>
    private readonly IRepository<Message> messageRepository;

    /// <summary>
    /// The mapper
    /// </summary>
    private readonly IMapper mapper;

    /// <summary>
    /// The storage client
    /// </summary>
    private readonly IStorageClient storageClient;

    /// <summary>
    /// The queue message sender
    /// </summary>
    private readonly IQueueMessageSender queueMessageSender;

    /// <summary>
    /// The calification queue name 
    /// </summary>
    private readonly string calificationQueueName = "calification-queue";

    /// <summary>
    /// The task alert queue name 
    /// </summary>
    private readonly string taskAlertQueueName = "task-alert-queue";

    /// <summary>
    /// Initializes a new instance of the <see cref=TFGManager"/> class
    /// </summary>
    /// <param name="tfgRepository">The TFG's repository</param>
    /// <param name="mainTaskRepository">The main task's repository</param>
    /// <param name="mainTaskStatusRepository">The main task's status repository</param>
    /// <param name="subTaskRepository">The sub task's repository</param>
    /// <param name="subTaskRepository">The sub task's status repository</param>
    /// <param name="commentRepository">The comment's repository</param>
    /// <param name="messageRepository">The message's repository</param>
    /// <param name="mapper">The mapper</param>
    /// <param name="storageClient">The storage client</param>
    /// <param name="queueMessageSender">The queue message sender</param>
    public TFGManager(IRepository<TFGModel> tfgRepository, IRepository<MainTask> mainTaskRepository, IRepository<MainTaskStatus> mainTaskStatusRepository, IRepository<SubTask> subTaskRepository, IRepository<SubTaskStatus> subTaskStatusRepository, IRepository<Comment> commentRepository, IRepository<Message> messageRepository, IMapper mapper, IStorageClient storageClient, IQueueMessageSender queueMessageSender)
    {
        this.tfgRepository = tfgRepository;
        this.mainTaskRepository = mainTaskRepository;
        this.mainTaskStatusRepository = mainTaskStatusRepository;
        this.subTaskRepository = subTaskRepository;
        this.subTaskStatusRepository = subTaskStatusRepository;
        this.commentRepository = commentRepository;
        this.messageRepository = messageRepository;
        this.mapper = mapper;
        this.storageClient = storageClient;
        this.queueMessageSender = queueMessageSender;
    }

    /// <summary>
    /// Assigns a new TFG to a student.
    /// </summary>
    /// <param name="tfgAssignmentDTO">The object containing the tutor's id, the student's id, and the TFG's name.</param>
    /// <returns>A DTO containing the error message, if any.</returns>
    public async Task<ErrorMessageDTO> AssignTFG(TFGAssignmentDTO tfgAssignmentDTO)
    {
        ErrorMessageDTO errorMessageDTO = new();
        TFGModel tfg = await this.tfgRepository.AsQueryable().FirstOrDefaultAsync(x => x.StudentId == tfgAssignmentDTO.StudentId);

        if (tfg is not null)
        {
            errorMessageDTO.ErrorMessage = "El estudiante ya tiene un TFG asignado.";
        }
        else
        {
            tfg = this.mapper.Map<TFGModel>(tfgAssignmentDTO);
            await this.tfgRepository.AddAsync(tfg);
        }

        return errorMessageDTO;
    }

    /// <summary>
    /// Creates a new main task for a TFG.
    /// </summary>
    /// <param name="createMainTaskDTO">The object containing the information to create a new main task.</param>
    /// <param name="createdBy">The user who created the main task, either tutor or student.</param>
    /// <returns>A DTO containing the error message, if any.</returns>
    public async Task<ErrorMessageDTO> CreateMainTask(CreateMainTaskDTO createMainTaskDTO, string createdBy)
    {
        ErrorMessageDTO errorMessageDTO = new();
        MainTask mainTask = await this.mainTaskRepository.AsQueryable().FirstOrDefaultAsync(x => x.TFGId == createMainTaskDTO.TFGId && x.Name == createMainTaskDTO.Name);

        if (mainTask is not null)
        {
            errorMessageDTO.ErrorMessage = "El TFG ya cuenta con una tarea con el mismo nombre.";
        }
        else
        {
            mainTask = this.mapper.Map<MainTask>(createMainTaskDTO);
            int order = this.mainTaskRepository.AsQueryable().Where(x => x.TFGId == createMainTaskDTO.TFGId).Count();
            order++;
            mainTask.Order = order;
            mainTask.CreatedBy = createdBy;
            await this.mainTaskRepository.AddAsync(mainTask);
        }

        return errorMessageDTO;
    }

    /// <summary>
    /// Updates an existing main task for a TFG.
    /// </summary>
    /// <param name="createMainTaskDTO">The object containing the information to update the main task.</param>
    /// <returns>A DTO containing the error message, if any.</returns>
    public async Task<ErrorMessageDTO> UpdateMainTask(CreateMainTaskDTO createMainTaskDTO)
    {
        ErrorMessageDTO errorMessageDTO = new();
        MainTask mainTask = await this.mainTaskRepository.AsQueryable().FirstOrDefaultAsync(x => x.TFGId == createMainTaskDTO.TFGId && x.Name == createMainTaskDTO.Name);

        if (mainTask is not null && mainTask.Id != createMainTaskDTO.Id)
        {
            errorMessageDTO.ErrorMessage = "El TFG ya cuenta con una tarea con el mismo nombre.";
        }
        else
        {
            if (mainTask is null)
            {
                mainTask = await this.mainTaskRepository.GetByIdAsync((int)createMainTaskDTO.Id);

                if (mainTask == null)
                {
                    throw new NotFoundException($"Tarea con ID {createMainTaskDTO.Id} no encontrada.");
                }
            }

            mainTask.Name = createMainTaskDTO.Name;
            mainTask.Deadline = createMainTaskDTO.Deadline;
            mainTask.MaximumPoints = createMainTaskDTO.MaximumPoints;
            mainTask.Description = createMainTaskDTO.Description;

            await this.mainTaskRepository.UpdateAsync(mainTask);
        }

        return errorMessageDTO;

    }

    /// <summary>
    /// Updates the task panel, handling sub-tasks.
    /// </summary>
    /// <param name="updateTaskPanel">The object containing information about the task panel update, including sub-tasks.</param>
    /// <param name="createdBy">The user who performed the update, either tutor or student.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task UpdateTaskPanel(UpdateTaskPanel updateTaskPanel, string createdBy)
    {
        SubTask subTask;

        // Delete sub tasks
        if (updateTaskPanel.DeletedSubTasksIds is not null)
        {
            await this.DeleteSubTasksByIdAsync(updateTaskPanel.DeletedSubTasksIds);
        }

        // Update sub tasks
        if (updateTaskPanel.UpdatedSubTasks is not null)
        {
            foreach (UpdateSubTaskDTO updatedSubTask in updateTaskPanel.UpdatedSubTasks)
            {
                subTask = await this.subTaskRepository.GetByIdAsync(updatedSubTask.Id);
                if (subTask == null)
                {
                    throw new NotFoundException($"Subtarea con ID {updatedSubTask.Id} no encontrada.");
                }
                subTask.TotalHours = updatedSubTask.TotalHours;
                subTask.StatusId = (int)updatedSubTask.StatusId;
                subTask.Order = (int)updatedSubTask.Order;
                await this.subTaskRepository.UpdateAsync(subTask);
            }
        }

        // New sub tasks
        if (updateTaskPanel.NewSubTasks is not null)
        {
            foreach (NewSubTaskDTO newSubTask in updateTaskPanel.NewSubTasks)
            {
                subTask = this.mapper.Map<SubTask>(newSubTask);
                subTask.CreatedBy = createdBy;
                subTask.MainTaskId = updateTaskPanel.MainTaskId;
                await this.subTaskRepository.AddAsync(subTask);
            }
        }

        // Main task's status
        MainTask mainTask = await this.mainTaskRepository.GetByIdAsync(updateTaskPanel.MainTaskId);

        if (mainTask != null)
        {
            List<SubTask> subTasks = await this.subTaskRepository.AsQueryable().Where(st => st.MainTaskId == mainTask.Id).ToListAsync();

            if (subTasks.Any())
            {
                bool allPending = subTasks.All(st => st.StatusId == (int)SubTaskStatusEnum.Pendiente);
                bool allCompleted = subTasks.All(st => st.StatusId == (int)SubTaskStatusEnum.Realizado);

                if (allPending)
                {
                    mainTask.StatusId = (int)MainTaskStatusEnum.Pendiente;
                }
                else if (allCompleted)
                {
                    mainTask.StatusId = (int)MainTaskStatusEnum.Realizado;
                }
                else
                {
                    mainTask.StatusId = (int)MainTaskStatusEnum.Desarrollo;
                }
            }
        }
        else
        {
            throw new NotFoundException($"Tarea con ID {mainTask.Id} no encontrada.");
        }

        // Main task's calification
        if (updateTaskPanel.ObtainedPoints is not null)
        {
            mainTask.ObtainedPoints = updateTaskPanel.ObtainedPoints;

            IQueryable<CalificationMessage> calificationMessage = from task in mainTaskRepository.AsQueryable()
                                                                  where task.Id == updateTaskPanel.MainTaskId && task.TFG.Student.AlertConfiguration.CalificationEmail
                                                                  select new CalificationMessage
                                                                  {
                                                                      StudentEmail = task.TFG.Student.Email,
                                                                      DateTime = DateTime.Now,
                                                                      MainTaskName = task.Name,
                                                                      MaximumPoints = task.MaximumPoints,
                                                                      ObtainedPoints = (int)updateTaskPanel.ObtainedPoints
                                                                  };

            CalificationMessage? message = calificationMessage.FirstOrDefault();

            if (message != null)
            {
                this.queueMessageSender.SendMessageToQueueAsync(message, this.calificationQueueName);
            }
        }

        await this.mainTaskRepository.UpdateAsync(mainTask);
    }

    /// <summary>
    /// Checks and sends task alerts based on the student's alert configuration.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task CheckAndSendTaskAlerts()
    {
        IQueryable<MainTask> mainTasks = this.mainTaskRepository.AsQueryable().Include(x => x.TFG.Student.AlertConfiguration).Include(x => x.SubTasks);

        foreach (MainTask task in mainTasks)
        {
            if (task.TFG.Student.AlertConfiguration != null && task.TFG.Student.AlertConfiguration.AlertEmail)
            {
                StudentAlertConfiguration alertConfig = task.TFG.Student.AlertConfiguration;

                var daysUntilDue = Math.Round((task.Deadline - DateTime.UtcNow).TotalDays);

                if (task.SubTasks.Sum(x => x.EstimatedHours) <= alertConfig.TotalTaskHours)
                {
                    if (daysUntilDue == alertConfig.AnticipationDaysForFewerThanTotalTaskHoursTasks)
                    {
                        MainTaskAlertMessage alertMessage = new()
                        {
                            StudentEmail = task.TFG.Student.Email,
                            MainTaskName = task.Name,
                            Deadline = task.Deadline,
                            SubTasksToDo = task.SubTasks.Select(subTask => new SubTaskMessage
                            {
                                Name = subTask.Name,
                                EstimatedHours = subTask.EstimatedHours
                            }).ToList()
                        };

                        await this.queueMessageSender.SendMessageToQueueAsync(alertMessage, this.taskAlertQueueName);
                        await this.mainTaskRepository.UpdateAsync(task);
                    }
                }
                else
                {
                    if (daysUntilDue == alertConfig.AnticipationDaysForMoreThanTotalTaskHoursTasks)
                    {
                        MainTaskAlertMessage alertMessage = new()
                        {
                            StudentEmail = task.TFG.Student.Email,
                            MainTaskName = task.Name,
                            Deadline = task.Deadline,
                            SubTasksToDo = task.SubTasks.Select(subTask => new SubTaskMessage
                            {
                                Name = subTask.Name,
                                EstimatedHours = subTask.EstimatedHours
                            }).ToList()
                        };

                        await this.queueMessageSender.SendMessageToQueueAsync(alertMessage, this.taskAlertQueueName);
                        await this.mainTaskRepository.UpdateAsync(task);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets the list of TFGs assigned to a tutor.
    /// </summary>
    /// <param name="tutorId">The ID of the tutor.</param>
    /// <returns>A list of TFGDTO objects representing the TFGs assigned to the tutor.</returns>
    public async Task<List<TFGDTO>> GetTFGs(int tutorId)
    {
        IQueryable<TFGDTO> tfgs = from tfg in tfgRepository.AsQueryable()
                                  where tfg.TutorId == tutorId
                                  let subTasksToDo = tfg.MainTasks.SelectMany(task => task.SubTasks.Where(subtask => subtask.StatusId == (int)SubTaskStatusEnum.Pendiente)).ToList()
                                  let totalSubTasksToDo = subTasksToDo.Count
                                  select new TFGDTO
                                  {
                                      Id = tfg.Id,
                                      StudentName = tfg.Student.Name,
                                      StudentSurname = tfg.Student.Surname,
                                      StudentProfilePicture = tfg.Student.ProfilePictureName,
                                      Name = tfg.Name,
                                      DelayedMainTasks = tfg.MainTasks.Where(task => task.Deadline < DateTime.Now && task.StatusId != (int)MainTaskStatusEnum.Realizado).Count(),
                                      MainTasksNotEvaluated = tfg.MainTasks.Where(task => task.ObtainedPoints == null && task.StatusId == (int)MainTaskStatusEnum.Realizado).Count(),
                                      TasksInProgress = tfg.MainTasks.Where(task => task.StatusId == (int)MainTaskStatusEnum.Desarrollo).Count(),
                                      SubTasksToDo = totalSubTasksToDo
                                  };

        List<TFGDTO> tfgList = await tfgs.ToListAsync();

        foreach (TFGDTO tfg in tfgList)
        {
            tfg.StudentProfilePicture = await this.storageClient.GetFileContent(tfg.StudentProfilePicture);
        }

        return tfgList;
    }

    public async Task<List<StatusDTO>> GetMainTaskStatuses()
    {
        IQueryable<StatusDTO> statuses = from mainTaskStatus in mainTaskStatusRepository.AsQueryable()
                                         select new StatusDTO
                                         {
                                             Id = mainTaskStatus.Id,
                                             Name = mainTaskStatus.Name
                                         };

        return await statuses.ToListAsync();
    }

    public async Task<List<StatusDTO>> GetSubTaskStatuses()
    {
        IQueryable<StatusDTO> statuses = from subTaskStatus in subTaskStatusRepository.AsQueryable()
                                         select new StatusDTO
                                         {
                                             Id = subTaskStatus.Id,
                                             Name = subTaskStatus.Name
                                         };

        return await statuses.ToListAsync();
    }

    /// <summary>
    /// Gets the summary of a TFG.
    /// </summary>
    /// <param name="tfgId">The ID of the TFG.</param>
    /// <param name="userId">The ID of the user requesting the summary.</param>
    /// <param name="createdBy">The role of the user requesting the summary (either tutor or student).</param>
    /// <returns>A DTO containing the summary of the TFG, including main tasks.</returns>
    public async Task<TFGSummaryDTO> GetTFGSummary(int tfgId, int userId, string createdBy)
    {

        await this.CheckUserPermissionAsync(tfgId, userId, createdBy);

        string tfgName = await tfgRepository.AsQueryable()
                                    .Where(tfg => tfg.Id == tfgId)
                                    .Select(tfg => tfg.Name)
                                    .FirstOrDefaultAsync();

        if (tfgName == null)
        {
            throw new NotFoundException($"TFG con ID {tfgId} no encontrado.");
        }

        IQueryable<MainTaskSummaryDTO> tasks = from mainTask in mainTaskRepository.AsQueryable()
                                               where mainTask.TFGId == tfgId
                                               orderby mainTask.Order
                                               select new MainTaskSummaryDTO
                                               {
                                                   Id = mainTask.Id,
                                                   Name = mainTask.Name,
                                                   Order = mainTask.Order,
                                                   Description = mainTask.Description,
                                                   Deadline = mainTask.Deadline,
                                                   Status = mainTask.MainTaskStatus.Name,
                                                   MaximumPoints = mainTask.MaximumPoints,
                                                   ObtainedPoints = mainTask.ObtainedPoints ?? 0,
                                                   SubTasksToDo = mainTask.SubTasks.Where(subtask => subtask.StatusId == (int)SubTaskStatusEnum.Pendiente).Count(),
                                                   HoursSubTasksToDo = mainTask.SubTasks.Where(subtask => subtask.StatusId == (int)SubTaskStatusEnum.Pendiente).Sum(subtask => subtask.EstimatedHours),
                                                   DoneSubTasks = mainTask.SubTasks.Where(subtask => subtask.StatusId == (int)SubTaskStatusEnum.Realizado).Count(),
                                                   HoursDoneSubTasks = (int)mainTask.SubTasks.Where(subtask => subtask.StatusId == (int)SubTaskStatusEnum.Realizado).Sum(subtask => subtask.TotalHours)
                                               };

        TFGSummaryDTO tfgSummaryDTO = new TFGSummaryDTO
        {
            MainTaskSummaryDTOList = await tasks.ToListAsync(),
            Name = tfgName
        };

        return tfgSummaryDTO;
    }

    /// <summary>
    /// Gets a main task by its ID.
    /// </summary>
    /// <param name="mainTaskId">The ID of the main task.</param>
    /// <param name="userId">The ID of the user requesting the main task.</param>
    /// <param name="createdBy">The role of the user requesting the main task (either tutor or student).</param>
    /// <returns>A DTO containing the main task information.</returns>
    public async Task<MainTaskDTO> GetMainTask(int mainTaskId, int userId, string createdBy)
    {
        var task = await mainTaskRepository.AsQueryable()
                .Where(task => task.Id == mainTaskId)
                .Select(task => new
                {
                    task.TFGId,
                })
                .FirstOrDefaultAsync();

        if (task == null)
        {
            throw new NotFoundException($"La tarea con ID {mainTaskId} no fue encontrada.");
        }

        await this.CheckUserPermissionAsync(task.TFGId, userId, createdBy);

        IQueryable<MainTaskDTO> taskQuery = from mainTask in mainTaskRepository.AsQueryable()
                                            where mainTask.Id == mainTaskId
                                            select new MainTaskDTO
                                            {
                                                Id = mainTask.Id,
                                                Name = mainTask.Name,
                                                Description = mainTask.Description,
                                                EstimatedHours = mainTask.SubTasks.Sum(subtask => subtask.EstimatedHours),
                                                TotalHours = mainTask.SubTasks.Sum(subtask => subtask.TotalHours),
                                                Status = mainTask.MainTaskStatus.Name,
                                                Order = mainTask.Order,
                                                CreatedBy = mainTask.CreatedBy,
                                                Deadline = mainTask.Deadline,
                                                MaximumPoints = mainTask.MaximumPoints,
                                                ObtainedPoints = mainTask.ObtainedPoints ?? 0,
                                                CommentsCount = mainTask.Comments != null ? mainTask.Comments.Count() : 0,
                                                SubTasks = mainTask.SubTasks.Select(subtask => new SubTaskDTO
                                                {
                                                    Id = subtask.Id,
                                                    Name = subtask.Name,
                                                    EstimatedHours = subtask.EstimatedHours,
                                                    TotalHours = subtask.TotalHours,
                                                    Status = subtask.SubTaskStatus.Name,
                                                    Order = subtask.Order,
                                                    CreatedBy = subtask.CreatedBy,
                                                    CommentsCount = subtask.Comments != null ? subtask.Comments.Count() : 0
                                                })
                                                .OrderBy(subtask => subtask.Order)
                                                .ToList()
                                            };
        MainTaskDTO mainTaskDTO = await taskQuery.FirstOrDefaultAsync();

        return mainTaskDTO;

    }

    /// <summary>
    /// Gets the TFG ID for a given student.
    /// </summary>
    /// <param name="studentId">The ID of the student.</param>
    /// <returns>The ID of the TFG assigned to the student.</returns>
    public async Task<int> GetStudentTFGId(int studentId)
    {
        IQueryable<TFGModel> tfgQuery = from tfg in tfgRepository.AsQueryable()
                                        where tfg.StudentId == studentId
                                        select new TFGModel
                                        {
                                            Id = tfg.Id
                                        };

        TFGModel tfgModel = await tfgQuery.FirstOrDefaultAsync();

        if (tfgModel == null)
        {
            throw new NotFoundException($"El TFG del estudiante con ID {studentId} no fue encontrado.");
        }

        return tfgModel.Id;
    }

    /// <summary>
    /// Creates a comment on a task (main or sub-task).
    /// </summary>
    /// <param name="createCommentDTO">The object containing the comment's information.</param>
    /// <param name="createdBy">The user who created the comment, either tutor or student.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task CreateComment(CreateCommentDTO createCommentDTO, string createdBy)
    {
        if (createCommentDTO.Files != null && createCommentDTO.Files.Count != 0)
        {
            string directoryName = string.Empty;

            if (createCommentDTO.MainTaskId.HasValue)
            {
                directoryName = $"MainTask{createCommentDTO.MainTaskId.Value}";
            }
            else if (createCommentDTO.SubTaskId.HasValue)
            {
                directoryName = $"SubTask{createCommentDTO.SubTaskId.Value}";
            }

            await this.storageClient.UploadCommentFiles(createCommentDTO.Files, directoryName);
        }

        Comment comment = new()
        {
            Text = createCommentDTO.Text,
            Files = createCommentDTO.Files != null
                ? JsonConvert.SerializeObject(createCommentDTO.Files.Select(f => new
                {
                    f.Name,
                    f.URL
                }))
                : null,
            CreatedBy = createdBy,
            CreatedOn = DateTime.Now,
            MainTaskId = createCommentDTO.MainTaskId,
            SubTaskId = createCommentDTO.SubTaskId
        };

        await this.commentRepository.AddAsync(comment);
    }

    /// <summary>
    /// Gets the comment history for a specific task.
    /// </summary>
    /// <param name="id">The ID of the task.</param>
    /// <param name="mainTask">True if the task is a main task, false if it is a sub-task.</param>
    /// <param name="userId">The ID of the user requesting the comment history.</param>
    /// <param name="createdBy">The role of the user requesting the comment history (either tutor or student).</param>
    /// <returns>A DTO containing the history of comments for the specified task.</returns>
    public async Task<CommentHistoryDTO> GetCommentHistory(int id, bool mainTask, int userId, string createdBy)
    {
        CommentHistoryDTO commentHistoryDTO = new();

        IQueryable<CommentDTO> comments;
        if (mainTask)
        {
            var task = await mainTaskRepository.AsQueryable()
                .Where(task => task.Id == id)
                .Select(task => new
                {
                    task.TFGId,
                    task.Name
                })
                .FirstOrDefaultAsync();

            if (task == null)
            {
                throw new NotFoundException($"La tarea con ID {id} no fue encontrada.");
            }

            await this.CheckUserPermissionAsync(task.TFGId, userId, createdBy);

            comments = from comment in commentRepository.AsQueryable()
                       where comment.MainTaskId == id
                       orderby comment.CreatedOn descending
                       select new CommentDTO
                       {
                           Files = !string.IsNullOrWhiteSpace(comment.Files)
                           ? JsonConvert.DeserializeObject<List<FileCommentHistoryDTO>>(comment.Files)
                           : new List<FileCommentHistoryDTO>(),
                           Text = comment.Text,
                           CreatedBy = comment.CreatedBy,
                           CreatedOn = comment.CreatedOn
                       };

            commentHistoryDTO.TaskName = task.Name;
        }
        else
        {
            var subTask = await subTaskRepository.AsQueryable()
                                  .Where(task => task.Id == id)
                                    .Select(task => new
                                    {
                                        task.MainTask.TFGId,
                                        task.Name
                                    })
                                    .FirstOrDefaultAsync();

            if (subTask == null)
            {
                throw new NotFoundException($"La subtarea con ID {id} no fue encontrada.");
            }

            await this.CheckUserPermissionAsync(subTask.TFGId, userId, createdBy);

            comments = from comment in commentRepository.AsQueryable()
                       where comment.SubTaskId == id
                       orderby comment.CreatedOn descending
                       select new CommentDTO
                       {
                           Files = !string.IsNullOrWhiteSpace(comment.Files)
                           ? JsonConvert.DeserializeObject<List<FileCommentHistoryDTO>>(comment.Files)
                           : new List<FileCommentHistoryDTO>(),
                           Text = comment.Text,
                           CreatedBy = comment.CreatedBy,
                           CreatedOn = comment.CreatedOn
                       };

            commentHistoryDTO.TaskName = subTask.Name;
        }

        commentHistoryDTO.Comments = await comments.ToListAsync();
        return commentHistoryDTO;
    }

    /// <summary>
    /// Gets the messages exchanged between the student and the tutor, with pagination and timestamp support.
    /// </summary>
    /// <param name="tfgId">The ID of the TFG.</param>
    /// <param name="userId">The ID of the user requesting the messages.</param>
    /// <param name="createdBy">The role of the user requesting the messages (either tutor or student).</param>
    /// <param name="limit">The maximum number of messages to return.</param>
    /// <param name="beforeTimestamp">Only messages created before this timestamp will be returned.</param>
    /// <returns>A list of MessageDTO objects representing the messages exchanged between the student and the tutor.</returns>
    public async Task<List<MessageDTO>> GetMessages(int tfgId, int userId, string createdBy, int limit = 20, DateTime? beforeTimestamp = null)
    {
        await this.CheckUserPermissionAsync(tfgId, userId, createdBy);

        IQueryable<MessageDTO> messages = from message in messageRepository.AsQueryable()
                                          where message.TFGId == tfgId
                                          orderby message.CreatedOn descending
                                          select new MessageDTO
                                          {
                                              Text = message.Text,
                                              CreatedBy = message.CreatedBy,
                                              CreatedOn = message.CreatedOn
                                          };
        if (beforeTimestamp.HasValue)
        {
            messages = messages.Where(m => m.CreatedOn < beforeTimestamp.Value);
        }

        messages = messages.Take(limit);

        return await messages.ToListAsync();
    }

    /// <summary>
    /// Updates the name of a TFG.
    /// </summary>
    /// <param name="tfgId">The ID of the TFG.</param>
    /// <param name="name">The new name for the TFG.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task UpdateTFG(int tfgId, string name)
    {
        TFGModel tfg = await this.tfgRepository.GetByIdAsync(tfgId);
        if (tfg == null)
        {
            throw new NotFoundException($"TFG con ID {tfgId} no encontrado.");
        }
        tfg.Name = name;

        await this.tfgRepository.UpdateAsync(tfg);
    }

    /// <summary>
    /// Sends a message between the student and the tutor.
    /// </summary>
    /// <param name="tfgId">The ID of the TFG.</param>
    /// <param name="messageDTO">The object containing the message information.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task SendMessage(int tfgId, MessageDTO messageDTO)
    {
        Message message = new()
        {
            TFGId = tfgId,
            Text = messageDTO.Text,
            CreatedBy = messageDTO.CreatedBy,
            CreatedOn = messageDTO.CreatedOn
        };

        await this.messageRepository.AddAsync(message);
    }

    /// <summary>
    /// Deletes a main task, along with its sub-tasks and comments.
    /// </summary>
    /// <param name="mainTaskId">The ID of the main task to delete.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task DeleteMainTask(int mainTaskId)
    {
        MainTask mainTask = await this.mainTaskRepository.AsQueryable().Include(mt => mt.SubTasks).Include(mt => mt.Comments).Where(mt => mt.Id == mainTaskId).FirstOrDefaultAsync();

        if (mainTask != null)
        {
            await this.DeleteMainTaskAndSubTasksAsync(mainTask);
            await this.UpdateMainTasksOrder(mainTask.TFGId, mainTask.Order);
        }
    }

    /// <summary>
    /// Deletes a TFG and all its associated tasks, subtasks and comments.
    /// </summary>
    /// <param name="tfgId">The ID of the TFG to delete.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task DeleteTFG(int tfgId)
    {
        List<MainTask> mainTasks = await this.mainTaskRepository.AsQueryable().Include(mt => mt.SubTasks).Include(mt => mt.Comments).Where(mt => mt.TFGId == tfgId).ToListAsync();

        foreach (MainTask mainTask in mainTasks)
        {
            await this.DeleteMainTaskAndSubTasksAsync(mainTask);
        }

        await this.tfgRepository.DeleteAsync(tfgId);
    }

    /// <summary>
    /// Checks if the user has permission to access a specific TFG.
    /// </summary>
    /// <param name="tfgId">The ID of the TFG.</param>
    /// <param name="userId">The ID of the user requesting access.</param>
    /// <param name="userRole">The role of the user (either tutor or student).</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    private async Task CheckUserPermissionAsync(int tfgId, int userId, string userRole)
    {
        TFGModel tfg = await this.tfgRepository.GetByIdAsync(tfgId);

        if (tfg == null)
        {
            throw new NotFoundException($"TFG con ID {tfgId} no encontrado.");
        }

        bool hasPermission = (userRole == nameof(RoleEnum.Tutor) && tfg.TutorId == userId ||
                             (userRole == nameof(RoleEnum.Estudiante) && tfg.StudentId == userId));

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException("No tienes permisos para acceder a este TFG.");
        }
    }

    /// <summary>
    /// Deletes a main task and its associated sub-tasks and comments.
    /// </summary>
    /// <param name="mainTask">The main task to delete along with its sub-tasks and comments.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    private async Task DeleteMainTaskAndSubTasksAsync(MainTask mainTask)
    {
        bool hasFilesInMainTask = mainTask.Comments != null && mainTask.Comments.Any(comment => !string.IsNullOrEmpty(comment.Files));
        if (hasFilesInMainTask)
        {
            string directoryName = $"MainTask{mainTask.Id}";
            await storageClient.DeleteCommentFiles(directoryName);
        }

        await DeleteSubTasksAsync(mainTask.SubTasks);

        await this.mainTaskRepository.DeleteAsync(mainTask.Id);
    }

    /// <summary>
    /// Deletes sub-tasks and their associated comments and files.
    /// </summary>
    /// <param name="subTasks">The sub-tasks to delete along with their comments and files.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    private async Task DeleteSubTasksAsync(IEnumerable<SubTask> subTasks)
    {
        foreach (SubTask subTask in subTasks)
        {
            bool hasFilesInSubTask = subTask.Comments != null && subTask.Comments.Any(comment => !string.IsNullOrEmpty(comment.Files));
            if (hasFilesInSubTask)
            {
                string directoryName = $"SubTask{subTask.Id}";
                await storageClient.DeleteCommentFiles(directoryName);
            }
            await this.subTaskRepository.DeleteAsync(subTask.Id);
        }
    }

    /// <summary>
    /// Deletes sub-tasks by their IDs.
    /// </summary>
    /// <param name="subTaskIds">The IDs of the sub-tasks to delete.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    private async Task DeleteSubTasksByIdAsync(IEnumerable<int> subTaskIds)
    {
        List<SubTask> subTasks = await this.subTaskRepository.AsQueryable().Include(st => st.Comments).Where(st => subTaskIds.Contains(st.Id)).ToListAsync();
        await this.DeleteSubTasksAsync(subTasks);
    }

    /// <summary>
    /// Updates the order of the main tasks after a removal.
    /// </summary>
    /// <param name="tfgId">The ID of the TFG.</param>
    /// <param name="removedOrder">The order of the task that was removed.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    private async Task UpdateMainTasksOrder(int tfgId, int removedOrder)
    {
        List<MainTask> mainTasks = await this.mainTaskRepository.AsQueryable().Where(mt => mt.TFGId == tfgId).OrderBy(mt => mt.Order).ToListAsync();

        foreach (MainTask task in mainTasks.Where(task => task.Order > removedOrder))
        {
            task.Order -= 1;
            await this.mainTaskRepository.UpdateAsync(task);
        }
    }
}

