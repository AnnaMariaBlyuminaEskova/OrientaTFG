using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using OrientaTFG.Shared.Infrastructure.DBContext;
using OrientaTFG.Shared.Infrastructure.Enums;
using OrientaTFG.Shared.Infrastructure.Model;
using OrientaTFG.Shared.Infrastructure.Repository;
using OrientaTFG.TFG.Core.DTOs;
using OrientaTFG.TFG.Core.Utils.AutoMapper;
using System.Linq.Expressions;
using Xunit;
using TFGModel = OrientaTFG.Shared.Infrastructure.Model.TFG;

namespace OrientaTFG.TFG.Core.Test;

public class TFGManagerTest
{
    /// <summary>
    /// The TFG's repository mock
    /// </summary>
    private readonly Mock<IRepository<TFGModel>> tfgRepositoryMock;

    /// <summary>
    /// The main task's repository mock
    /// </summary>
    private readonly Mock<IRepository<MainTask>> mainTaskRepositoryMock;

    /// <summary>
    /// The mapper
    /// </summary>
    private readonly IMapper mapper;

    /// <summary>
    /// The tfg manager
    /// </summary>
    private readonly ITFGManager tfgManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="TFGManagerTest"/> class
    /// </summary>
    public TFGManagerTest()
    {
        this.tfgRepositoryMock = new Mock<IRepository<TFGModel>>();
        this.mainTaskRepositoryMock = new Mock<IRepository<MainTask>>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new MapperProfile());
        });

        this.mapper = new Mapper(mapperConfig);
        this.tfgManager = new TFGManager(this.tfgRepositoryMock.Object, this.mainTaskRepositoryMock.Object, this.mapper);
    }

    #region AssignTFG tests

    /// <summary>
    /// Tests that assign tfg returns the expected message when the student already has a tfg
    /// </summary>
    [Fact]
    public async Task AssignTFGShouldReturnExpectedErrorMessageWhenStudentAlreadyHasAnAssignedTFG()
    {
        // Arrange
        TFGAssignmentDTO tfgAssignmentDTO = new()
        {
            Name = "TFG",
            StudentId = 1,
            TutorId = 1
        };

        TFGModel tfg = new()
        {
            Name = "Other TFG",
            StudentId = 1,
            TutorId = 4
        };

        IQueryable<TFGModel> tfgs = new List<TFGModel>() { tfg }.AsQueryable();
        Mock<DbSet<TFGModel>> mockSetTFG = tfgs.BuildMockDbSet();
        this.tfgRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetTFG.Object);

        // Act
        var result = await this.tfgManager.AssignTFG(tfgAssignmentDTO);

        // Assert
        Assert.Equal("El estudiante ya tiene un TFG asignado.", result.ErrorMessage);
    }

    /// <summary>
    /// Tests that assign tfg saves the tfg successfully
    /// </summary>
    [Fact]
    public async Task AssignTFGShouldSaveTheTFGSuccessfully()
    {
        // Arrange

        TFGAssignmentDTO tfgAssignmentDTO = new()
        {
            Name = "TFG",
            StudentId = 1,
            TutorId = 1
        };

        var tfgs = new List<TFGModel>().AsQueryable().BuildMockDbSet();
        this.tfgRepositoryMock.Setup(x => x.AsQueryable()).Returns(tfgs.Object);
        this.tfgRepositoryMock.Setup(x => x.Add(It.IsAny<TFGModel>()));
        this.tfgRepositoryMock.Setup(x => x.SaveChanges());

        // Act
        var result = await this.tfgManager.AssignTFG(tfgAssignmentDTO);

        // Assert
        this.tfgRepositoryMock.Verify(m => m.Add(It.Is<TFGModel>(tfg => tfg.Name == tfgAssignmentDTO.Name && tfg.StudentId == tfgAssignmentDTO.StudentId && tfg.TutorId == tfgAssignmentDTO.TutorId)), Times.Once());
        this.tfgRepositoryMock.Verify(m => m.SaveChanges(), Times.Once());
    }

    #endregion

    #region CreateMainTask tests

    /// <summary>
    /// Tests that create main task returns the expected message when the tfg already has a main task with the same name
    /// </summary>
    [Fact]
    public async Task CreateMainTaskShouldReturnExpectedErrorMessageWhenTheTFGAlreadyHasAMainTaskWithTheSameName()
    {
        // Arrange
        CreateMainTaskDTO createMainTaskDTO = new()
        {
            TFGId = 1,
            Name = "Test main task",
            Deadline = DateTime.UtcNow.AddDays(5),
            MaximumPoints = 100,
            Description = "Description"
        };

        MainTask mainTask = new()
        {
            Name = "Test main task",
            Description = string.Empty,
            MaximumPoints = 200,
            Deadline = DateTime.UtcNow.AddDays(2),
            CreatedBy = nameof(RoleEnum.Tutor),
            TFGId = 1,
            StatusId = (int)MainTaskStatusEnum.Pendiente,
            Order = 1
        };

        IQueryable<MainTask> mainTasks = new List<MainTask>() { mainTask }.AsQueryable();
        Mock<DbSet<MainTask>> mockSetMainTask = mainTasks.BuildMockDbSet();
        this.mainTaskRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetMainTask.Object);

        // Act
        var result = await this.tfgManager.CreateMainTask(createMainTaskDTO, nameof(RoleEnum.Estudiante));

        // Assert
        Assert.Equal("El TFG ya cuenta con una tarea con el mismo nombre.", result.ErrorMessage);
    }

    /// <summary>
    /// Tests that create main task saves the main task successfully with the correct main task order
    /// </summary>
    [Fact]
    public async Task CreateMainTaskShouldSaveTheMainTaskSuccessfullyWithCorrectOrder()
    {
        // Arrange
        CreateMainTaskDTO createMainTaskDTO = new()
        {
            TFGId = 1,
            Name = "Test main task",
            Deadline = DateTime.UtcNow.AddDays(5),
            MaximumPoints = 100,
            Description = "Description"
        };

        MainTask mainTask = new()
        {
            Name = "Test another main task",
            Description = string.Empty,
            MaximumPoints = 200,
            Deadline = DateTime.UtcNow.AddDays(2),
            CreatedBy = nameof(RoleEnum.Tutor),
            TFGId = 1,
            StatusId = (int)MainTaskStatusEnum.Pendiente,
            Order = 1
        };

        var mainTasks = new List<MainTask>() { mainTask }.AsQueryable().BuildMockDbSet();
        this.mainTaskRepositoryMock.Setup(x => x.AsQueryable()).Returns(mainTasks.Object);
        this.mainTaskRepositoryMock.Setup(x => x.Add(It.IsAny<MainTask>()));
        this.mainTaskRepositoryMock.Setup(x => x.SaveChanges());

        // Act
        var result = await this.tfgManager.CreateMainTask(createMainTaskDTO, nameof(RoleEnum.Estudiante));

        // Assert
        this.mainTaskRepositoryMock.Verify(m => m.Add(It.Is<MainTask>(mainTask =>
            mainTask.TFGId == createMainTaskDTO.TFGId &&
            mainTask.Name == createMainTaskDTO.Name &&
            mainTask.Deadline == createMainTaskDTO.Deadline &&
            mainTask.MaximumPoints == createMainTaskDTO.MaximumPoints &&
            mainTask.Description == createMainTaskDTO.Description &&
            mainTask.CreatedBy == nameof(RoleEnum.Estudiante) &&
            mainTask.Order == 2)
        ), Times.Once());

        this.mainTaskRepositoryMock.Verify(m => m.SaveChanges(), Times.Once());
    }

    #endregion

    #region GetTFGs tests

    /// <summary>
    /// Tests that get tfgs returns data 
    /// </summary>
    [Fact]
    public async Task GetTFGsTest()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<OrientaTFGContext>()
            .UseInMemoryDatabase(databaseName: "GetTFGsTest")
            .Options;

        using var context = new OrientaTFGContext(options);
        context.TFGs.AddRange(this.GetTFGsMock());
            

        List<TFGModel> tfgsList = this.GetTFGsMock();

        context.SaveChanges();

        tfgRepositoryMock.Setup(repo => repo.AsQueryable()).Returns(context.TFGs);

        // Act
        var result = await this.tfgManager.GetTFGs(tutorId: 1);

        // Assert
        List<TFGDTO> expectedResult = new()
        {
            new()
            {
                StudentName = "First student name",
                StudentSurname = "First student surname",
                Name = "First TFG",
                MainTasksNotEvaluated = 1,
                TasksInProgress = 1,
                SubTasksToDo = 1
            },
            new()
            {
                StudentName = "Second student name",
                StudentSurname = "Second student surname",
                Name = "Second TFG",
                MainTasksNotEvaluated = 0,
                TasksInProgress = 0,
                SubTasksToDo = 0
            }
        };
        Assert.Equivalent(expectedResult, result);
    }

    #endregion

    #region GetTFGSummary tests

    /// <summary>
    /// Tests that get tfg summary returns data 
    /// </summary>
    [Fact]
    public async Task GetTFGSummaryTest()
    {
        // Arrange
        DbContextOptions<OrientaTFGContext> options = new DbContextOptionsBuilder<OrientaTFGContext>()
            .UseInMemoryDatabase(databaseName: "GetTFGSummaryTest")
            .Options;

        DateTime dateTimeDeadline = DateTime.UtcNow.AddDays(-2);

        using var context = new OrientaTFGContext(options);
        context.MainTasks.Add(new MainTask
        {
            Id = 1,
            TFGId = 1,
            Name = "First main task",
            Description = "Description",
            MaximumPoints = 200,
            Deadline = dateTimeDeadline,
            CreatedBy = nameof(RoleEnum.Estudiante),
            StatusId = (int)MainTaskStatusEnum.Realizado,
            Order = 1,
            MainTaskStatus = new MainTaskStatus()
            {
                Id = (int)MainTaskStatusEnum.Realizado,
                Name = nameof(MainTaskStatusEnum.Realizado)
            },
            SubTasks = new List<SubTask>()
            {
                new()
                {
                    Id = 1,
                    Name = "First sub task in first main task",
                    EstimatedHours = 10,
                    TotalHours = 12,
                    StatusId = (int)SubTaskStatusEnum.Realizado,
                    Order = 1,
                    CreatedBy = nameof(RoleEnum.Tutor)
                },
                new()
                {
                    Id = 2,
                    Name = "Second sub task in first main task",
                    EstimatedHours = 8,
                    TotalHours = 9,
                    StatusId = (int)SubTaskStatusEnum.Realizado,
                    Order = 2,
                    CreatedBy = nameof(RoleEnum.Tutor)
                }
            }
        });

        context.SaveChanges();

        this.mainTaskRepositoryMock.Setup(repo => repo.AsQueryable()).Returns(context.MainTasks);

        // Act
        var result = await this.tfgManager.GetTFGSummary(tfgId: 1);

        // Assert
        List<MainTaskSummaryDTO> expectedResult = new()
        {
            new()
            {
                Id = 1,
                Name = "First main task",
                Order = 1,
                Deadline = dateTimeDeadline,
                Status = nameof(MainTaskStatusEnum.Realizado),
                MaximumPoints = 200,
                ObtainedPoints = 0,
                SubTasksToDo = 0,
                HoursSubTasksToDo = 0,
                DoneSubTasks = 2,
                HoursDoneSubTasks = 21
            }
        };
        Assert.Equivalent(expectedResult, result);
    }

    #endregion

    #region GetMainTask tests

    /// <summary>
    /// Tests that get main task returns data 
    /// </summary>
    [Fact]
    public async Task GetMainTaskTest()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<OrientaTFGContext>()
            .UseInMemoryDatabase(databaseName: "GetMainTaskTest")
            .Options;

        DateTime dateTimeDeadline = DateTime.UtcNow.AddDays(-2);

        SubTaskStatus subTaskStatus = new()
        {
            Id = (int)SubTaskStatusEnum.Realizado,
            Name = nameof(SubTaskStatusEnum.Realizado)
        };

        using var context = new OrientaTFGContext(options);
        context.MainTasks.Add(new MainTask
        {
            Id = 1,
            Name = "First main task",
            Description = "Description",
            MaximumPoints = 200,
            Deadline = dateTimeDeadline,
            CreatedBy = nameof(RoleEnum.Estudiante),
            Order = 1,
            MainTaskStatus = new MainTaskStatus()
            {
                Id = (int)MainTaskStatusEnum.Realizado,
                Name = nameof(MainTaskStatusEnum.Realizado)
            },
            SubTasks = new List<SubTask>()
            {
                new()
                {
                    Id = 1,
                    Name = "First sub task in first main task",
                    EstimatedHours = 10,
                    TotalHours = 12,
                    SubTaskStatus = subTaskStatus,
                    Order = 1,
                    CreatedBy = nameof(RoleEnum.Tutor)
                },
                new()
                {
                    Id = 2,
                    Name = "Second sub task in first main task",
                    EstimatedHours = 8,
                    TotalHours = 9,
                    SubTaskStatus = subTaskStatus,
                    Order = 2,
                    CreatedBy = nameof(RoleEnum.Tutor)
                }
            }
        });

        context.SaveChanges();

        this.mainTaskRepositoryMock.Setup(repo => repo.AsQueryable()).Returns(context.MainTasks);

        // Act
        var result = await this.tfgManager.GetMainTask(mainTaskId: 1);

        // Assert
        MainTaskDTO expectedResult = new()
        {
            Id = 1,
            Name = "First main task",
            EstimatedHours = 18,
            TotalHours = 21,
            Status = nameof(MainTaskStatusEnum.Realizado),
            Order = 1,
            CreatedBy = nameof(RoleEnum.Estudiante),
            Deadline = dateTimeDeadline,
            MaximumPoints = 200,
            ObtainedPoints = 0,
            SubTasks = new List<SubTaskDTO>()
            {
                new()
                {
                    Id = 1,
                    Name = "First sub task in first main task",
                    EstimatedHours = 10,
                    TotalHours = 12,
                    Status = nameof(SubTaskStatusEnum.Realizado),
                    Order = 1,
                    CreatedBy = nameof(RoleEnum.Tutor)
                },
                new()
                {
                    Id = 2,
                    Name = "Second sub task in first main task",
                    EstimatedHours = 8,
                    TotalHours = 9,
                    Status = nameof(SubTaskStatusEnum.Realizado),
                    Order = 2,
                    CreatedBy = nameof(RoleEnum.Tutor)
                }
            }
        };

        Assert.Equivalent(expectedResult, result);
    }

    #endregion

    private List<TFGModel> GetTFGsMock()
    {
        List<TFGModel> tfgsList = new()
        {
            new()
            {
                Name = "First TFG",
                Student = new()
                {
                    Id = 1,
                    Name = "First student name",
                    Surname = "First student surname",
                    Email = string.Empty,
                    Password = string.Empty,
                    ProfilePictureName = string.Empty
                },
                TutorId = 1,
                MainTasks = new List<MainTask>()
                {
                    new()
                    {
                        Name = "First main task",
                        Description = "Description",
                        MaximumPoints = 200,
                        Deadline = DateTime.UtcNow.AddDays(-2),
                        CreatedBy = nameof(RoleEnum.Tutor),
                        StatusId = (int)MainTaskStatusEnum.Realizado,
                        Order = 1,
                        SubTasks = new List<SubTask>()
                        {
                            new()
                            {
                                Name = "First sub task in first main task",
                                EstimatedHours = 10,
                                TotalHours = 12,
                                StatusId = (int)SubTaskStatusEnum.Realizado,
                                Order = 1,
                                CreatedBy = nameof(RoleEnum.Tutor)
                            }
                        }
                    },
                    new()
                    {
                        Name = "Second main task",
                        Description = "Description",
                        MaximumPoints = 100,
                        Deadline = DateTime.UtcNow.AddDays(2),
                        CreatedBy = nameof(RoleEnum.Estudiante),
                        StatusId = (int)MainTaskStatusEnum.Desarrollo,
                        Order = 2,
                        SubTasks = new List<SubTask>()
                        {
                            new()
                            {
                                Name = "First sub task in second main task",
                                EstimatedHours = 15,
                                TotalHours = 12,
                                StatusId = (int)SubTaskStatusEnum.Realizado,
                                Order = 1,
                                CreatedBy = nameof(RoleEnum.Estudiante)
                            },
                            new()
                            {
                                Name = "Second sub task in second main task",
                                EstimatedHours = 10,
                                StatusId = (int)SubTaskStatusEnum.Pendiente,
                                Order = 2,
                                CreatedBy = nameof(RoleEnum.Estudiante)
                            },
                        }
                    }
                }
            },
            new()
            {
                Name = "Second TFG",
                Student = new()
                {
                    Id = 2,
                    Name = "Second student name",
                    Surname = "Second student surname",
                    Email = string.Empty,
                    Password = string.Empty,
                    ProfilePictureName = string.Empty
                },
                TutorId = 1
            }
        };

        return tfgsList;
    }
}