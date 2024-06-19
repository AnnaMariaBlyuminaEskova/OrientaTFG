using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OrientaTFG.Shared.Infrastructure.Enums;
using OrientaTFG.Shared.Infrastructure.Model;
using OrientaTFG.Shared.Infrastructure.Repository;
using OrientaTFG.TFG.Core.DTOs;
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
    /// The mapper
    /// </summary>
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref=TFGManager"/> class
    /// </summary>
    /// <param name="tfgRepository">The TFG's repository</param>
    /// <param name="mainTaskRepository">The main task's repository</param>
    /// <param name="mapper">The mapper</param>
    public TFGManager(IRepository<TFGModel> tfgRepository, IRepository<MainTask> mainTaskRepository, IMapper mapper)
    {
        this.tfgRepository = tfgRepository;
        this.mainTaskRepository = mainTaskRepository;
        this.mapper = mapper;
    }

    /// <summary>
    /// Assigns a new TFG
    /// </summary>
    /// <param name="tfgAssignmentDTO">The object containing the tutor's id, the student's id and the tfg's name</param>
    /// <returns>ErrorMessage if the student already has an assigned tfg</returns>
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
            this.tfgRepository.Add(tfg);
            this.tfgRepository.SaveChanges();
        }

        return errorMessageDTO;
    }

    /// <summary>
    /// Creates a new main task
    /// </summary>
    /// <param name="createMainTaskDTO">The object containing the information to create a new task</param>
    /// <param name="createdBy">Tha user that created the main taks, tutor or student</param>
    /// <returns>ErrorMessage if the tfg already has a main task with the same name</returns>
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
            this.mainTaskRepository.Add(mainTask);
            this.mainTaskRepository.SaveChanges();
        }

        return errorMessageDTO;
    }

    /// <summary>
    /// Gets the tutor's list of assigned TFGs
    /// </summary>
    /// <param name="tutorId">The tutor's id</param>
    /// <returns>List of assigned TFGs</returns>
    public async Task<List<TFGDTO>> GetTFGs(int tutorId)
    {
        IQueryable<TFGDTO> tfgs = from tfg in tfgRepository.AsQueryable()
                                  let subTasksToDo = tfg.MainTasks.SelectMany(task => task.SubTasks.Where(subtask => subtask.StatusId == (int)SubTaskStatusEnum.Pendiente)).ToList()
                                  let totalSubTasksToDo = subTasksToDo.Count
                                  select new TFGDTO
                                  {
                                      StudentName = tfg.Student.Name,
                                      StudentSurname = tfg.Student.Surname,
                                      Name = tfg.Name,
                                      MainTasksNotEvaluated = tfg.MainTasks.Where(task => task.ObtainedPoints == null && task.StatusId == (int)MainTaskStatusEnum.Realizado).Count(),
                                      TasksInProgress = tfg.MainTasks.Where(task => task.StatusId == (int)MainTaskStatusEnum.Desarrollo).Count(),
                                      SubTasksToDo = totalSubTasksToDo
                                  };

        return await tfgs.ToListAsync();
    }


    /// <summary>
    /// Gets the tfg's summary
    /// </summary>
    /// <param name="tfgId">The tfg's id</param>
    /// <returns>List of the tfg's main tasks summaries</returns>
    public async Task<List<MainTaskSummaryDTO>> GetTFGSummary(int tfgId)
    {
        IQueryable<MainTaskSummaryDTO> tasks = from mainTask in mainTaskRepository.AsQueryable()
                                               where mainTask.TFGId == tfgId
                                               select new MainTaskSummaryDTO
                                               {
                                                   Id = mainTask.Id,
                                                   Name = mainTask.Name,
                                                   Order = mainTask.Order,
                                                   Deadline = mainTask.Deadline,
                                                   Status = mainTask.MainTaskStatus.Name,
                                                   MaximumPoints = mainTask.MaximumPoints,
                                                   ObtainedPoints = mainTask.ObtainedPoints ?? 0,
                                                   SubTasksToDo = mainTask.SubTasks.Where(subtask => subtask.StatusId == (int)SubTaskStatusEnum.Pendiente).Count(),
                                                   HoursSubTasksToDo = mainTask.SubTasks.Where(subtask => subtask.StatusId == (int)SubTaskStatusEnum.Pendiente).Sum(subtask => subtask.EstimatedHours),
                                                   DoneSubTasks = mainTask.SubTasks.Where(subtask => subtask.StatusId == (int)SubTaskStatusEnum.Realizado).Count(),
                                                   HoursDoneSubTasks = (int)mainTask.SubTasks.Where(subtask => subtask.StatusId == (int)SubTaskStatusEnum.Realizado).Sum(subtask => subtask.TotalHours)
                                               };

        return await tasks.ToListAsync();
    }

    /// <summary>
    /// Gets a main task 
    /// </summary>
    /// <param name="mainTaskId">The main task's id</param>
    /// <returns>The main task</returns>
    public async Task<MainTaskDTO> GetMainTask(int mainTaskId)
    {
        IQueryable<MainTaskDTO> task = from mainTask in mainTaskRepository.AsQueryable()
                                       where mainTask.Id == mainTaskId
                                       select new MainTaskDTO
                                       {
                                           Id = mainTask.Id,
                                           Name = mainTask.Name,
                                           EstimatedHours = mainTask.SubTasks.Sum(subtask => subtask.EstimatedHours),
                                           TotalHours = mainTask.SubTasks.Sum(subtask => subtask.TotalHours),
                                           Status = mainTask.MainTaskStatus.Name,
                                           Order = mainTask.Order,
                                           CreatedBy = mainTask.CreatedBy,
                                           Deadline = mainTask.Deadline,
                                           MaximumPoints = mainTask.MaximumPoints,
                                           ObtainedPoints = mainTask.ObtainedPoints ?? 0,
                                           SubTasks = mainTask.SubTasks.Select(subtask => new SubTaskDTO
                                           {
                                               Id = subtask.Id,
                                               Name = subtask.Name,
                                               EstimatedHours = subtask.EstimatedHours,
                                               TotalHours = subtask.TotalHours,
                                               Status = subtask.SubTaskStatus.Name,
                                               Order = subtask.Order,
                                               CreatedBy = subtask.CreatedBy
                                           }).ToList()
                                       };
        return task.FirstOrDefault();

    }
}

