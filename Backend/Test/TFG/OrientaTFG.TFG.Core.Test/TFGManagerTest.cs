using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Newtonsoft.Json;
using OrientaTFG.Shared.Infrastructure.DBContext;
using OrientaTFG.Shared.Infrastructure.Enums;
using OrientaTFG.Shared.Infrastructure.Model;
using OrientaTFG.Shared.Infrastructure.Repository;
using OrientaTFG.Shared.Infrastructure.Utils.StorageClient;
using OrientaTFG.TFG.Core.DTOs;
using OrientaTFG.TFG.Core.Utils.AutoMapper;
using OrientaTFG.TFG.Core.Utils.QueueMessageSender;
using System.Linq.Expressions;
using Xunit;
using Message = OrientaTFG.Shared.Infrastructure.Model.Message;
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
    /// The main task's status repository mock
    /// </summary>
    private readonly Mock<IRepository<MainTaskStatus>> mainTaskStatusRepositoryMock;

    /// <summary>
    /// The sub task's repository mock
    /// </summary>
    private readonly Mock<IRepository<SubTask>> subTaskRepositoryMock;

    /// <summary>
    /// The sub task's repository mock
    /// </summary>
    private readonly Mock<IRepository<SubTaskStatus>> subTaskStatusRepositoryMock;

    /// <summary>
    /// The comment's repository mock
    /// </summary>
    private readonly Mock<IRepository<Comment>> commentRepositoryMock;

    /// <summary>
    /// The message's repository mock
    /// </summary>
    private readonly Mock<IRepository<Message>> messageRepositoryMock;

    /// <summary>
    /// The mapper
    /// </summary>
    private readonly IMapper mapper;

    /// <summary>
    /// The storage client mock
    /// </summary>
    private readonly Mock<IStorageClient> storageClientMock;

    /// <summary>
    /// The queue message sender mock
    /// </summary>
    private readonly Mock<IQueueMessageSender> queueMessageSenderMock;

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
        this.mainTaskStatusRepositoryMock = new Mock<IRepository<MainTaskStatus>>();
        this.subTaskRepositoryMock = new Mock<IRepository<SubTask>>();
        this.subTaskStatusRepositoryMock = new Mock<IRepository<SubTaskStatus>>();
        this.commentRepositoryMock = new Mock<IRepository<Comment>>();
        this.messageRepositoryMock = new Mock<IRepository<Message>>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new MapperProfile());
        });

        this.mapper = new Mapper(mapperConfig);
        this.storageClientMock = new Mock<IStorageClient>();
        this.queueMessageSenderMock = new Mock<IQueueMessageSender>();

        this.tfgManager = new TFGManager(this.tfgRepositoryMock.Object, this.mainTaskRepositoryMock.Object, this.mainTaskStatusRepositoryMock.Object, this.subTaskRepositoryMock.Object, this.subTaskStatusRepositoryMock.Object, this.commentRepositoryMock.Object, this.messageRepositoryMock.Object, this.mapper, this.storageClientMock.Object, this.queueMessageSenderMock.Object);
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
        this.tfgRepositoryMock.Setup(x => x.AddAsync(It.IsAny<TFGModel>()));

        // Act
        var result = await this.tfgManager.AssignTFG(tfgAssignmentDTO);

        // Assert
        this.tfgRepositoryMock.Verify(m => m.AddAsync(It.Is<TFGModel>(tfg => tfg.Name == tfgAssignmentDTO.Name && tfg.StudentId == tfgAssignmentDTO.StudentId && tfg.TutorId == tfgAssignmentDTO.TutorId)), Times.Once());
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
    public async Task CreateMainTaskShouldSaveTheMainTaskSuccessfully()
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
        this.mainTaskRepositoryMock.Setup(x => x.AddAsync(It.IsAny<MainTask>()));

        // Act
        var result = await this.tfgManager.CreateMainTask(createMainTaskDTO, nameof(RoleEnum.Estudiante));

        // Assert
        this.mainTaskRepositoryMock.Verify(m => m.AddAsync(It.Is<MainTask>(mainTask =>
            mainTask.TFGId == createMainTaskDTO.TFGId &&
            mainTask.Name == createMainTaskDTO.Name &&
            mainTask.Deadline == createMainTaskDTO.Deadline &&
            mainTask.MaximumPoints == createMainTaskDTO.MaximumPoints &&
            mainTask.Description == createMainTaskDTO.Description &&
            mainTask.CreatedBy == nameof(RoleEnum.Estudiante) &&
            mainTask.Order == 2)
        ), Times.Once());
    }

    #endregion

    #region CheckAndSendTaskAlerts tests

    /// <summary>
    /// Tests that no alert is sent if the student has disabled alert emails
    /// </summary>
    [Fact]
    public async Task CheckAndSendTaskAlertsShouldNotSendAlertWhenAlertEmailIsDisabled()
    {
        // Arrange
        MainTask mainTask = new()
        {
            TFG = new TFGModel
            {
                Student = new Student
                {
                    AlertConfiguration = new StudentAlertConfiguration
                    {
                        AlertEmail = false
                    }
                }
            },
            Deadline = DateTime.UtcNow.AddDays(5),
            SubTasks = new List<SubTask>
        {
            new SubTask { Name = "SubTask 1", EstimatedHours = 2 },
            new SubTask { Name = "SubTask 2", EstimatedHours = 3 }
        }
        };

        IQueryable<MainTask> mainTasks = new List<MainTask> { mainTask }.AsQueryable();
        Mock<DbSet<MainTask>> mockSetMainTask = mainTasks.BuildMockDbSet();
        this.mainTaskRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetMainTask.Object);

        // Act
        await this.tfgManager.CheckAndSendTaskAlerts();

        // Assert
        this.queueMessageSenderMock.Verify(x => x.SendMessageToQueueAsync(It.IsAny<MainTaskAlertMessage>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Tests that an alert is sent if the total estimated hours is less than or equal to TotalTaskHours and the due date matches AnticipationDaysForFewerThanTotalTaskHoursTasks
    /// </summary>
    [Fact]
    public async Task CheckAndSendTaskAlertsShouldSendAlertForFewerThanTotalTaskHoursTasks()
    {
        // Arrange
        MainTask mainTask = new()
        {
            Name = "Main Task 1",
            Deadline = DateTime.UtcNow.AddDays(3), // Matching 3 days of anticipation
            SubTasks = new List<SubTask>
            {
                new SubTask { Name = "SubTask 1", EstimatedHours = 2 },
                new SubTask { Name = "SubTask 2", EstimatedHours = 3 }
            },
            TFG = new TFGModel
            {
                Student = new Student
                {
                    Email = "student@example.com",
                    AlertConfiguration = new StudentAlertConfiguration
                    {
                        TotalTaskHours = 10,
                        AnticipationDaysForFewerThanTotalTaskHoursTasks = 3,
                        AlertEmail = true
                    }
                }
            }
        };

        IQueryable<MainTask> mainTasks = new List<MainTask> { mainTask }.AsQueryable();
        Mock<DbSet<MainTask>> mockSetMainTask = mainTasks.BuildMockDbSet();
        this.mainTaskRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetMainTask.Object);

        // Act
        await this.tfgManager.CheckAndSendTaskAlerts();

        // Assert
        this.queueMessageSenderMock.Verify(x => x.SendMessageToQueueAsync(It.Is<MainTaskAlertMessage>(m =>
            m.StudentEmail == mainTask.TFG.Student.Email &&
            m.MainTaskName == mainTask.Name &&
            m.Deadline == mainTask.Deadline &&
            m.SubTasksToDo.Count == 2
        ), "task-alert-queue"), Times.Once);
    }

    /// <summary>
    /// Tests that an alert is sent if the total estimated hours is more than TotalTaskHours and the due date matches AnticipationDaysForMoreThanTotalTaskHoursTasks.
    /// </summary>
    [Fact]
    public async Task CheckAndSendTaskAlertsShouldSendAlertForMoreThanTotalTaskHoursTasks()
    {
        // Arrange
        MainTask mainTask = new()
        {
            Name = "Main Task 2",
            Deadline = DateTime.UtcNow.AddDays(5),
            SubTasks = new List<SubTask>
            {
                new SubTask { Name = "SubTask 1", EstimatedHours = 6 },
                new SubTask { Name = "SubTask 2", EstimatedHours = 5 }
            },
            TFG = new TFGModel
            {
                Student = new Student
                {
                    Email = "student@example.com",
                    AlertConfiguration = new StudentAlertConfiguration
                    {
                        TotalTaskHours = 10,
                        AnticipationDaysForMoreThanTotalTaskHoursTasks = 5,
                        AlertEmail = true
                    }
                }
            }
        };

        IQueryable<MainTask> mainTasks = new List<MainTask> { mainTask }.AsQueryable();
        Mock<DbSet<MainTask>> mockSetMainTask = mainTasks.BuildMockDbSet();
        this.mainTaskRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetMainTask.Object);

        // Act
        await this.tfgManager.CheckAndSendTaskAlerts();

        // Assert
        this.queueMessageSenderMock.Verify(x => x.SendMessageToQueueAsync(It.Is<MainTaskAlertMessage>(m =>
            m.StudentEmail == mainTask.TFG.Student.Email &&
            m.MainTaskName == mainTask.Name &&
            m.Deadline == mainTask.Deadline &&
            m.SubTasksToDo.Count == 2
        ), "task-alert-queue"), Times.Once);
    }

    /// <summary>
    /// Tests that no alert is sent if the due date does not match the anticipation days configured
    /// </summary>
    [Fact]
    public async Task CheckAndSendTaskAlertsShouldNotSendAlertWhenDueDateDoesNotMatchAnticipationDays()
    {
        // Arrange
        MainTask mainTask = new()
        {
            Name = "Main Task 3",
            Deadline = DateTime.UtcNow.AddDays(2),
            SubTasks = new List<SubTask>
            {
                new SubTask { Name = "SubTask 1", EstimatedHours = 6 },
                new SubTask { Name = "SubTask 2", EstimatedHours = 5 }
            },
            TFG = new TFGModel
            {
                Student = new Student
                {
                    Email = "student@example.com",
                    AlertConfiguration = new StudentAlertConfiguration
                    {
                        TotalTaskHours = 10,
                        AnticipationDaysForFewerThanTotalTaskHoursTasks = 3,
                        AnticipationDaysForMoreThanTotalTaskHoursTasks = 5,
                        AlertEmail = true
                    }
                }
            }
        };

        IQueryable<MainTask> mainTasks = new List<MainTask> { mainTask }.AsQueryable();
        Mock<DbSet<MainTask>> mockSetMainTask = mainTasks.BuildMockDbSet();
        this.mainTaskRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetMainTask.Object);

        // Act
        await this.tfgManager.CheckAndSendTaskAlerts();

        // Assert
        this.queueMessageSenderMock.Verify(x => x.SendMessageToQueueAsync(It.IsAny<MainTaskAlertMessage>(), It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region UpdateTaskPanel tests

    [Fact]
    public async Task UpdateTaskPanelShouldDeleteSubTasksWithFiles()
    {
        // Arrange
        UpdateTaskPanel updateTaskPanel = new UpdateTaskPanel
        {
            MainTaskId = 1, 
            DeletedSubTasksIds = new List<int> { 1, 2 }
        };

        SubTask subTask1 = new SubTask
        {
            Id = 1,
            Comments = new List<Comment>
        {
            new Comment
            {
                Id = 1,
                Files = JsonConvert.SerializeObject(new List<FileCommentHistoryDTO>
                {
                    new FileCommentHistoryDTO { Name = "file1.txt", URL = "http://example.com/file1.txt" }
                })
            }
        }
        };

        SubTask subTask2 = new SubTask
        {
            Id = 2,
            Comments = new List<Comment>
        {
            new Comment
            {
                Id = 2,
                Files = JsonConvert.SerializeObject(new List<FileCommentHistoryDTO>
                {
                    new FileCommentHistoryDTO { Name = "file2.txt", URL = "http://example.com/file2.txt" }
                })
            }
        }
        };

        MainTask mainTask = new MainTask
        {
            Id = updateTaskPanel.MainTaskId,
            Name = "Main Task",
            StatusId = 1  
        };

        // Configurar los mocks
        var subTasks = new List<SubTask>() { subTask1, subTask2 }.AsQueryable().BuildMockDbSet();
        this.subTaskRepositoryMock.Setup(x => x.AsQueryable()).Returns(subTasks.Object);

        this.mainTaskRepositoryMock.Setup(repo => repo.GetByIdAsync(updateTaskPanel.MainTaskId))
            .ReturnsAsync(mainTask);

        this.storageClientMock.Setup(client => client.DeleteCommentFiles(It.IsAny<string>())).Returns(Task.CompletedTask);
        this.subTaskRepositoryMock.Setup(repo => repo.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

        // Act
        await this.tfgManager.UpdateTaskPanel(updateTaskPanel, "User1");

        // Assert
        this.storageClientMock.Verify(client => client.DeleteCommentFiles("SubTask1"), Times.Once);
        this.storageClientMock.Verify(client => client.DeleteCommentFiles("SubTask2"), Times.Once);
        this.subTaskRepositoryMock.Verify(repo => repo.DeleteAsync(1), Times.Once);
        this.subTaskRepositoryMock.Verify(repo => repo.DeleteAsync(2), Times.Once);
    }

    [Fact]
    public async Task UpdateTaskPanelShouldCreateNewSubTasks()
    {
        // Arrange
        UpdateTaskPanel updateTaskPanel = new UpdateTaskPanel
        {
            MainTaskId = 1,
            NewSubTasks = new List<NewSubTaskDTO>
        {
            new() { Name = "New SubTask 1", TotalHours = 3, StatusId = (int)SubTaskStatusEnum.Pendiente, Order = 1 },
            new() { Name = "New SubTask 2", TotalHours = 4, StatusId = (int)SubTaskStatusEnum.Realizado, Order = 2 }
        }
        };

        MainTask mainTask = new MainTask
        {
            Id = updateTaskPanel.MainTaskId,
            Name = "Main Task",
            StatusId = 1
        };

        // Mock de mainTaskRepository para GetByIdAsync
        this.mainTaskRepositoryMock.Setup(repo => repo.GetByIdAsync(updateTaskPanel.MainTaskId))
            .ReturnsAsync(mainTask);

        // Mock de subTaskRepository para AddAsync
        this.subTaskRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<SubTask>()))
            .Returns(Task.CompletedTask);

        // Act
        await this.tfgManager.UpdateTaskPanel(updateTaskPanel, "User1");

        // Assert
        this.subTaskRepositoryMock.Verify(repo => repo.AddAsync(It.Is<SubTask>(st =>
            st.Name == "New SubTask 1" && st.TotalHours == 3 && st.StatusId == (int)SubTaskStatusEnum.Pendiente && st.Order == 1 && st.CreatedBy == "User1"
        )), Times.Once);

        this.subTaskRepositoryMock.Verify(repo => repo.AddAsync(It.Is<SubTask>(st =>
            st.Name == "New SubTask 2" && st.TotalHours == 4 && st.StatusId == (int)SubTaskStatusEnum.Realizado && st.Order == 2 && st.CreatedBy == "User1"
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateTaskPanelShouldUpdateSubTasks()
    {
        // Arrange
        UpdateTaskPanel updateTaskPanel = new UpdateTaskPanel
        {
            MainTaskId = 1,
            UpdatedSubTasks = new List<UpdateSubTaskDTO>
            {
                new UpdateSubTaskDTO { Id = 1, TotalHours = 5, StatusId = (int)SubTaskStatusEnum.Pendiente, Order = 1 },
                new UpdateSubTaskDTO { Id = 2, TotalHours = 8, StatusId = (int)SubTaskStatusEnum.Realizado, Order = 2 }
            }
        };

        MainTask mainTask = new()
        {
            Id = 1,
            Name = "Main Task",
            Deadline = DateTime.UtcNow.AddDays(2)
        };

        SubTask subTask1 = new SubTask { Id = 1, MainTaskId = 1 };
        SubTask subTask2 = new SubTask { Id = 2, MainTaskId = 1 };

        this.mainTaskRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(mainTask);

        this.subTaskRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(subTask1);
        this.subTaskRepositoryMock.Setup(repo => repo.GetByIdAsync(2)).ReturnsAsync(subTask2);

        this.subTaskRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<SubTask>())).Returns(Task.CompletedTask);

        // Act
        await this.tfgManager.UpdateTaskPanel(updateTaskPanel, nameof(RoleEnum.Tutor));

        // Assert
        Assert.Equal(5, subTask1.TotalHours);
        Assert.Equal((int)SubTaskStatusEnum.Pendiente, subTask1.StatusId);
        Assert.Equal(1, subTask1.Order);

        Assert.Equal(8, subTask2.TotalHours);
        Assert.Equal((int)SubTaskStatusEnum.Realizado, subTask2.StatusId);
        Assert.Equal(2, subTask2.Order);

        this.subTaskRepositoryMock.Verify(repo => repo.UpdateAsync(subTask1), Times.Once);
        this.subTaskRepositoryMock.Verify(repo => repo.UpdateAsync(subTask2), Times.Once);
    }

    [Fact]
    public async Task UpdateTaskPanelShouldUpdateMainTaskStatus()
    {
        // Arrange
        UpdateTaskPanel updateTaskPanel = new UpdateTaskPanel
        {
            MainTaskId = 1
        };

        MainTask mainTask = new MainTask { Id = 1, StatusId = (int)MainTaskStatusEnum.Pendiente };

        List<SubTask> subTasks = new List<SubTask>
        {
            new SubTask { Id = 1, MainTaskId = 1, StatusId = (int)SubTaskStatusEnum.Realizado },
            new SubTask { Id = 2, MainTaskId = 1, StatusId = (int)SubTaskStatusEnum.Realizado }
        };

        this.mainTaskRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(mainTask);
        var subTasksMock = subTasks.AsQueryable().BuildMockDbSet();
        this.subTaskRepositoryMock.Setup(x => x.AsQueryable()).Returns(subTasksMock.Object);

        this.mainTaskRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<MainTask>())).Returns(Task.CompletedTask);

        // Act
        await this.tfgManager.UpdateTaskPanel(updateTaskPanel, "User1");

        // Assert
        Assert.Equal((int)MainTaskStatusEnum.Realizado, mainTask.StatusId);

        this.mainTaskRepositoryMock.Verify(repo => repo.UpdateAsync(mainTask), Times.Once);
    }

    [Fact]
    public async Task UpdateTaskPanelShouldUpdateMainTaskCalificationAndSendMessage()
    {
        // Arrange
        UpdateTaskPanel updateTaskPanel = new UpdateTaskPanel
        {
            MainTaskId = 1,
            ObtainedPoints = 85
        };

        MainTask mainTask = new MainTask
        {
            Id = 1,
            Name = "Main Task 1",
            MaximumPoints = 100,
            StatusId = (int)MainTaskStatusEnum.Pendiente,
            TFG = new TFGModel
            {
                Student = new Student
                {
                    Email = "student@example.com",
                    AlertConfiguration = new StudentAlertConfiguration
                    {
                        CalificationEmail = true
                    }
                }
            }
        };

        List<SubTask> subTasks = new List<SubTask>
    {
        new SubTask { Id = 1, MainTaskId = 1, StatusId = (int)SubTaskStatusEnum.Realizado },
        new SubTask { Id = 2, MainTaskId = 1, StatusId = (int)SubTaskStatusEnum.Realizado }
    };

        this.mainTaskRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(mainTask);

        var subTasksMock = subTasks.AsQueryable().BuildMockDbSet();
        this.subTaskRepositoryMock.Setup(x => x.AsQueryable()).Returns(subTasksMock.Object);

        // This is the key part: Ensure that the AsQueryable() for MainTask is also mocked correctly
        var mainTaskMock = new List<MainTask> { mainTask }.AsQueryable().BuildMockDbSet();
        this.mainTaskRepositoryMock.Setup(x => x.AsQueryable()).Returns(mainTaskMock.Object);

        this.mainTaskRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<MainTask>())).Returns(Task.CompletedTask);
        this.queueMessageSenderMock.Setup(sender => sender.SendMessageToQueueAsync(It.IsAny<CalificationMessage>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act
        await this.tfgManager.UpdateTaskPanel(updateTaskPanel, "User1");

        // Assert
        Assert.Equal(85, mainTask.ObtainedPoints);

        this.mainTaskRepositoryMock.Verify(repo => repo.UpdateAsync(mainTask), Times.Once);

        this.queueMessageSenderMock.Verify(sender => sender.SendMessageToQueueAsync(
            It.Is<CalificationMessage>(msg =>
                msg.StudentEmail == "student@example.com" &&
                msg.MainTaskName == "Main Task 1" &&
                msg.MaximumPoints == 100 &&
                msg.ObtainedPoints == 85
            ),
            It.IsAny<string>()
        ), Times.Once);
    }


    [Fact]
    public async Task UpdateTaskPanelShouldNotSendMessageIfCalificationEmailIsDisabled()
    {
        // Arrange
        UpdateTaskPanel updateTaskPanel = new UpdateTaskPanel
        {
            MainTaskId = 1,
            ObtainedPoints = 85
        };

        MainTask mainTask = new MainTask
        {
            Id = 1,
            Name = "Main Task 1",
            MaximumPoints = 100,
            StatusId = (int)MainTaskStatusEnum.Pendiente,
            TFG = new TFGModel
            {
                Student = new Student
                {
                    Email = "student@example.com",
                    AlertConfiguration = new StudentAlertConfiguration
                    {
                        CalificationEmail = false
                    }
                }
            }
        };

        List<SubTask> subTasks = new List<SubTask>
    {
        new SubTask { Id = 1, MainTaskId = 1, StatusId = (int)SubTaskStatusEnum.Realizado },
        new SubTask { Id = 2, MainTaskId = 1, StatusId = (int)SubTaskStatusEnum.Realizado }
    };

        this.mainTaskRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(mainTask);
        var subTasksMock = subTasks.AsQueryable().BuildMockDbSet();
        this.subTaskRepositoryMock.Setup(x => x.AsQueryable()).Returns(subTasksMock.Object);
        this.mainTaskRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<MainTask>())).Returns(Task.CompletedTask);

        // Act
        await this.tfgManager.UpdateTaskPanel(updateTaskPanel, "User1");

        // Assert
        Assert.Equal(85, mainTask.ObtainedPoints);

        this.mainTaskRepositoryMock.Verify(repo => repo.UpdateAsync(mainTask), Times.Once);

        this.queueMessageSenderMock.Verify(sender => sender.SendMessageToQueueAsync(It.IsAny<CalificationMessage>(), It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region UpdateMainTask tests

    [Fact]
    public async Task UpdateMainTaskShouldReturnErrorMessageIfTaskNameExists()
    {
        // Arrange
        CreateMainTaskDTO createMainTaskDTO = new CreateMainTaskDTO
        {
            Id = 2,
            TFGId = 1,
            Name = "Existing Task",
            Deadline = DateTime.UtcNow.AddDays(5),
            MaximumPoints = 100,
            Description = "Test description"
        };

        MainTask existingMainTask = new MainTask
        {
            Id = 1,
            TFGId = 1,
            Name = "Existing Task",
            Deadline = DateTime.UtcNow.AddDays(10),
            MaximumPoints = 100,
            Description = "Existing task description"
        };

        var mainTasksMock = new List<MainTask> { existingMainTask }.AsQueryable().BuildMockDbSet();

        this.mainTaskRepositoryMock.Setup(repo => repo.AsQueryable()).Returns(mainTasksMock.Object);

        // Act
        var result = await this.tfgManager.UpdateMainTask(createMainTaskDTO);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("El TFG ya cuenta con una tarea con el mismo nombre.", result.ErrorMessage);

        this.mainTaskRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<MainTask>()), Times.Never);
    }

    [Fact]
    public async Task UpdateMainTaskShouldUpdateTaskSuccessfully()
    {
        // Arrange
        CreateMainTaskDTO createMainTaskDTO = new CreateMainTaskDTO
        {
            Id = 1,
            TFGId = 1,
            Name = "Unique Task",
            Deadline = DateTime.UtcNow.AddDays(5),
            MaximumPoints = 100,
            Description = "Updated description"
        };

        MainTask existingMainTask = new MainTask
        {
            Id = 1,
            TFGId = 1,
            Name = "Old Task Name",
            Deadline = DateTime.UtcNow.AddDays(10),
            MaximumPoints = 50,
            Description = "Old description"
        };

        var mainTasksMock = new List<MainTask> { existingMainTask }.AsQueryable().BuildMockDbSet();

        this.mainTaskRepositoryMock.Setup(repo => repo.AsQueryable()).Returns(mainTasksMock.Object);
        this.mainTaskRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(existingMainTask);
        this.mainTaskRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<MainTask>())).Returns(Task.CompletedTask);

        // Act
        var result = await this.tfgManager.UpdateMainTask(createMainTaskDTO);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.ErrorMessage);

        this.mainTaskRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<MainTask>(task =>
            task.Id == 1 &&
            task.Name == "Unique Task" &&
            task.Deadline == createMainTaskDTO.Deadline &&
            task.MaximumPoints == createMainTaskDTO.MaximumPoints &&
            task.Description == "Updated description"
        )), Times.Once);
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

        // Act
        var result = await this.tfgManager.GetTFGs(tutorId: 1);

        // Assert
        List<TFGDTO> expectedResult = new()
        {
            new()
            {
                Id = 1,
                StudentName = "First student name",
                StudentSurname = "First student surname",
                Name = "First TFG",
                MainTasksNotEvaluated = 1,
                TasksInProgress = 1,
                SubTasksToDo = 1
            },
            new()
            {
                Id = 2,
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

        context.TFGs.Add(new TFGModel
        {
            Id = 1,
            Name = "TFG Name"
        });

        context.SaveChanges();

        this.mainTaskRepositoryMock.Setup(repo => repo.AsQueryable()).Returns(context.MainTasks);
        this.tfgRepositoryMock.Setup(repo => repo.AsQueryable()).Returns(context.TFGs);

        // Act
        var result = await this.tfgManager.GetTFGSummary(1, 1, nameof(RoleEnum.Tutor));

        // Assert
        TFGSummaryDTO expectedResult = new()
        {
            Name = "TFG Name",
            MainTaskSummaryDTOList = new List<MainTaskSummaryDTO>()
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
        var result = await this.tfgManager.GetMainTask(1, 1, nameof(RoleEnum.Tutor));

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

    #region GetStudentTFGId tests

    /// <summary>
    /// Tests that get student tfg id returns data 
    /// </summary>
    [Fact]
    public async Task GetStudentTFGIdTest()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<OrientaTFGContext>()
            .UseInMemoryDatabase(databaseName: "GetStudentTFGIdTest")
            .Options;

        using var context = new OrientaTFGContext(options);
        context.TFGs.Add(new TFGModel
        {
            Id = 3,
            Name = "First main task",
            StudentId = 5
        });

        context.SaveChanges();

        this.tfgRepositoryMock.Setup(repo => repo.AsQueryable()).Returns(context.TFGs);

        // Act
        var result = await this.tfgManager.GetStudentTFGId(studentId: 5);

        // Assert
        Assert.Equal(3, result);
    }

    #endregion

    #region CreateComment tests

    [Fact]
    public async Task CreateCommentCreatesCommentWithoutUploadingFiles()
    {
        // Arrange
        CreateCommentDTO createCommentDTO = new CreateCommentDTO
        {
            Text = "This is a comment",
            Files = null,
            MainTaskId = 1,
            SubTaskId = null
        };

        string createdBy = "Student";

        this.commentRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Comment>())).Returns(Task.CompletedTask);

        // Act
        await this.tfgManager.CreateComment(createCommentDTO, createdBy);

        // Assert
        this.storageClientMock.Verify(s => s.UploadCommentFiles(It.IsAny<List<FileDTO>>(), It.IsAny<string>()), Times.Never);

        this.commentRepositoryMock.Verify(r => r.AddAsync(It.Is<Comment>(c =>
            c.Text == createCommentDTO.Text &&
            c.Files == null &&
            c.CreatedBy == createdBy &&
            c.MainTaskId == createCommentDTO.MainTaskId &&
            c.SubTaskId == createCommentDTO.SubTaskId
        )), Times.Once);
    }

    [Fact]
    public async Task CreateCommenCreatesCommentAndUploadsFiles()
    {
        // Arrange
        CreateCommentDTO createCommentDTO = new CreateCommentDTO
        {
            Text = "This is a comment",
            Files = new List<FileDTO>
            {
                new FileDTO { Name = "file1.txt", Content = "data1" },
                new FileDTO { Name = "file2.txt", Content = "data2" }
            },
            MainTaskId = 1,
            SubTaskId = null
        };

        string createdBy = "Tutor";

        this.storageClientMock.Setup(s => s.UploadCommentFiles(It.IsAny<List<FileDTO>>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        this.commentRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Comment>())).Returns(Task.CompletedTask);

        // Act
        await this.tfgManager.CreateComment(createCommentDTO, createdBy);

        // Assert
        this.storageClientMock.Verify(s => s.UploadCommentFiles(createCommentDTO.Files, "MainTask1"), Times.Once);

        this.commentRepositoryMock.Verify(r => r.AddAsync(It.Is<Comment>(c =>
            c.Text == createCommentDTO.Text &&
            c.Files == JsonConvert.SerializeObject(createCommentDTO.Files.Select(f => new
            {
                f.Name,
                f.URL
            })) &&
            c.CreatedBy == createdBy &&
            c.MainTaskId == createCommentDTO.MainTaskId &&
            c.SubTaskId == createCommentDTO.SubTaskId
        )), Times.Once);
    }

    #endregion

    #region GetCommentHistory tests

    #endregion

    #region GetMessages tests

    [Fact]
    public async Task GetMessagesTest()
    {
        // Arrange
        DbContextOptions<OrientaTFGContext> options = new DbContextOptionsBuilder<OrientaTFGContext>()
            .UseInMemoryDatabase(databaseName: "GetMessagesTest")
            .Options;

        using OrientaTFGContext context = new OrientaTFGContext(options);

        DateTime now = DateTime.UtcNow;
        List<Message> messages = new List<Message>
        {
            new Message
            {
                Id = 1,
                TFGId = 1,
                Text = "Message 1",
                CreatedBy = "User1",
                CreatedOn = now.AddMinutes(-5)
            },
            new Message
            {
                Id = 2,
                TFGId = 1,
                Text = "Message 2",
                CreatedBy = "User2",
                CreatedOn = now.AddMinutes(-10)
            },
            new Message
            {
                Id = 3,
                TFGId = 1,
                Text = "Message 3",
                CreatedBy = "User3",
                CreatedOn = now.AddMinutes(-15)
            },
            new Message
            {
                Id = 4,
                TFGId = 1,
                Text = "Message 4",
                CreatedBy = "User4",
                CreatedOn = now.AddMinutes(-20)
            }
        };

        context.Messages.AddRange(messages);
        context.SaveChanges();

        this.messageRepositoryMock.Setup(repo => repo.AsQueryable()).Returns(context.Messages);

        // Act
        List<MessageDTO> resultWithoutTimestamp = await this.tfgManager.GetMessages(tfgId: 1, userId: 1, createdBy: nameof(RoleEnum.Tutor), limit: 3);
        List<MessageDTO> resultWithTimestamp = await this.tfgManager.GetMessages(tfgId: 1, userId: 1, createdBy: nameof(RoleEnum.Tutor), limit: 3, beforeTimestamp: now.AddMinutes(-10));

        // Assert
        List<MessageDTO> expectedResultWithoutTimestamp = new List<MessageDTO>
        {
            new MessageDTO
            {
                Text = "Message 1",
                CreatedBy = "User1",
                CreatedOn = now.AddMinutes(-5)
            },
            new MessageDTO
            {
                Text = "Message 2",
                CreatedBy = "User2",
                CreatedOn = now.AddMinutes(-10)
            },
            new MessageDTO
            {
                Text = "Message 3",
                CreatedBy = "User3",
                CreatedOn = now.AddMinutes(-15)
            }
        };

        List<MessageDTO> expectedResultWithTimestamp = new List<MessageDTO>
        {
            new MessageDTO
            {
                Text = "Message 3",
                CreatedBy = "User3",
                CreatedOn = now.AddMinutes(-15)
            },
            new MessageDTO
            {
                Text = "Message 4",
                CreatedBy = "User4",
                CreatedOn = now.AddMinutes(-20)
            }
        };

        Assert.Equal(expectedResultWithoutTimestamp.Count, resultWithoutTimestamp.Count);
        for (int i = 0; i < expectedResultWithoutTimestamp.Count; i++)
        {
            Assert.Equal(expectedResultWithoutTimestamp[i].Text, resultWithoutTimestamp[i].Text);
            Assert.Equal(expectedResultWithoutTimestamp[i].CreatedBy, resultWithoutTimestamp[i].CreatedBy);
            Assert.Equal(expectedResultWithoutTimestamp[i].CreatedOn, resultWithoutTimestamp[i].CreatedOn);
        }

        Assert.Equal(expectedResultWithTimestamp.Count, resultWithTimestamp.Count);
        for (int i = 0; i < expectedResultWithTimestamp.Count; i++)
        {
            Assert.Equal(expectedResultWithTimestamp[i].Text, resultWithTimestamp[i].Text);
            Assert.Equal(expectedResultWithTimestamp[i].CreatedBy, resultWithTimestamp[i].CreatedBy);
            Assert.Equal(expectedResultWithTimestamp[i].CreatedOn, resultWithTimestamp[i].CreatedOn);
        }
    }

    #endregion

    #region UpdateTFG tests

    [Fact]
    public async Task UpdateTFGTest()
    {
        // Arrange
        DbContextOptions<OrientaTFGContext> options = new DbContextOptionsBuilder<OrientaTFGContext>()
            .UseInMemoryDatabase(databaseName: "UpdateTFGTest")
            .Options;

        using OrientaTFGContext context = new OrientaTFGContext(options);

        TFGModel initialTFG = new TFGModel
        {
            Id = 1,
            Name = "Initial TFG Name",
            StudentId = 5
        };

        context.TFGs.Add(initialTFG);
        context.SaveChanges();

        this.tfgRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int id) => context.TFGs.SingleOrDefault(t => t.Id == id));
        this.tfgRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<TFGModel>()))
            .Callback<TFGModel>(tfg =>
            {
                var existingTFG = context.TFGs.SingleOrDefault(x => x.Id == tfg.Id);
                if (existingTFG != null)
                {
                    existingTFG.Name = tfg.Name;
                }
                context.SaveChanges();
            });

        // Act
        string newName = "Updated TFG Name";
        await this.tfgManager.UpdateTFG(tfgId: 1, name: newName);

        // Assert
        TFGModel updatedTFG = context.TFGs.Single(t => t.Id == 1);
        Assert.Equal(newName, updatedTFG.Name);
    }

    #endregion

    #region DeleteTFG tests

    [Fact]
    public async Task DeleteTFGShouldRemoveAllRelatedDataAndFiles()
    {
        // Arrange
        var now = DateTime.UtcNow;

        var mainTask1 = new MainTask
        {
            Id = 1,
            TFGId = 1,
            Name = "Main Task 1",
            CreatedBy = string.Empty,
            Description = "Description",
            Comments = new List<Comment>
        {
            new Comment
            {
                Id = 1,
                Text = "Comment 1",
                Files = JsonConvert.SerializeObject(new List<FileCommentHistoryDTO>
                {
                    new FileCommentHistoryDTO { Name = "file1.txt", URL = "http://example.com/file1.txt" }
                }),
                CreatedBy = "User1",
                CreatedOn = now
            }
        },
            SubTasks = new List<SubTask>
        {
            new SubTask
            {
                Id = 1,
                Name = "SubTask 1",
                CreatedBy = string.Empty,
                Comments = new List<Comment>
                {
                    new Comment
                    {
                        Id = 2,
                        Text = "SubTask Comment 1",
                        Files = JsonConvert.SerializeObject(new List<FileCommentHistoryDTO>
                        {
                            new FileCommentHistoryDTO { Name = "file2.txt", URL = "http://example.com/file2.txt" }
                        }),
                        CreatedBy = "User2",
                        CreatedOn = now
                    }
                }
            }
        }
        };

        var tfg = new TFGModel
        {
            Id = 1,
            Name = "TFG 1",
            StudentId = 5
        };

        var mainTasksMock = new List<MainTask> { mainTask1 }.AsQueryable().BuildMockDbSet();
        var tfgsMock = new List<TFGModel> { tfg }.AsQueryable().BuildMockDbSet();

        this.mainTaskRepositoryMock.Setup(repo => repo.AsQueryable()).Returns(mainTasksMock.Object);
        this.tfgRepositoryMock.Setup(repo => repo.AsQueryable()).Returns(tfgsMock.Object);
        this.subTaskRepositoryMock.Setup(repo => repo.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);
        this.tfgRepositoryMock.Setup(repo => repo.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);
        this.storageClientMock.Setup(client => client.DeleteCommentFiles(It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act
        await this.tfgManager.DeleteTFG(tfgId: 1);

        // Assert
        this.mainTaskRepositoryMock.Verify(repo => repo.DeleteAsync(It.Is<int>(id => id == 1)), Times.Once);
        this.subTaskRepositoryMock.Verify(repo => repo.DeleteAsync(It.Is<int>(id => id == 1)), Times.Once);
        this.tfgRepositoryMock.Verify(repo => repo.DeleteAsync(It.Is<int>(id => id == 1)), Times.Once);
        this.storageClientMock.Verify(client => client.DeleteCommentFiles("MainTask1"), Times.Once);
        this.storageClientMock.Verify(client => client.DeleteCommentFiles("SubTask1"), Times.Once);
    }


    #endregion

    #region DeleteMainTask tests

    [Fact]
    public async Task DeleteMainTaskShouldRemoveAllRelatedDataAndFiles()
    {
        // Arrange
        var now = DateTime.UtcNow;

        var mainTask1 = new MainTask
        {
            Id = 1,
            TFGId = 1,
            Name = "Main Task 1",
            CreatedBy = string.Empty,
            Description = "Description",
            Comments = new List<Comment>
        {
            new Comment
            {
                Id = 1,
                Text = "Comment 1",
                Files = JsonConvert.SerializeObject(new List<FileCommentHistoryDTO>
                {
                    new FileCommentHistoryDTO { Name = "file1.txt", URL = "http://example.com/file1.txt" }
                }),
                CreatedBy = "User1",
                CreatedOn = now
            }
        },
            SubTasks = new List<SubTask>
        {
            new SubTask
            {
                Id = 1,
                Name = "SubTask 1",
                CreatedBy = string.Empty,
                Comments = new List<Comment>
                {
                    new Comment
                    {
                        Id = 2,
                        Text = "SubTask Comment 1",
                        Files = JsonConvert.SerializeObject(new List<FileCommentHistoryDTO>
                        {
                            new FileCommentHistoryDTO { Name = "file2.txt", URL = "http://example.com/file2.txt" }
                        }),
                        CreatedBy = "User2",
                        CreatedOn = now
                    }
                }
            }
        }
        };

        var mainTasksMock = new List<MainTask> { mainTask1 }.AsQueryable().BuildMockDbSet();

        this.mainTaskRepositoryMock.Setup(repo => repo.AsQueryable()).Returns(mainTasksMock.Object);
        this.subTaskRepositoryMock.Setup(repo => repo.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);
        this.storageClientMock.Setup(client => client.DeleteCommentFiles(It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act
        await this.tfgManager.DeleteTFG(tfgId: 1);

        // Assert
        this.mainTaskRepositoryMock.Verify(repo => repo.DeleteAsync(It.Is<int>(id => id == 1)), Times.Once);
        this.subTaskRepositoryMock.Verify(repo => repo.DeleteAsync(It.Is<int>(id => id == 1)), Times.Once);
        this.storageClientMock.Verify(client => client.DeleteCommentFiles("MainTask1"), Times.Once);
        this.storageClientMock.Verify(client => client.DeleteCommentFiles("SubTask1"), Times.Once);
    }

    #endregion

    #region SendMessage tests

    [Fact]
    public async Task SendMessageTest()
    {
        // Arrange
        this.messageRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Message>())).Returns(Task.CompletedTask);

        var messageDTO = new MessageDTO
        {
            Text = "Test message",
            CreatedBy = "User1",
            CreatedOn = DateTime.UtcNow
        };

        int tfgId = 1;

        // Act
        await tfgManager.SendMessage(tfgId, messageDTO);

        // Assert
        messageRepositoryMock.Verify(m => m.AddAsync(It.Is<Message>(msg =>
            msg.TFGId == tfgId &&
            msg.Text == messageDTO.Text &&
            msg.CreatedBy == messageDTO.CreatedBy &&
            msg.CreatedOn == messageDTO.CreatedOn
        )), Times.Once());
    }

    #endregion
}