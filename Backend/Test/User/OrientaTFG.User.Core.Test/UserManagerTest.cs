using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using OrientaTFG.Shared.Infrastructure.Model;
using OrientaTFG.Shared.Infrastructure.Repository;
using OrientaTFG.Shared.Infrastructure.Utils.StorageClient;
using OrientaTFG.User.Core.DTOs;
using OrientaTFG.User.Core.Utils.PasswordEncrypter;
using OrientaTFG.User.Core.Utils.TokenGenerator;
using Xunit;

namespace OrientaTFG.User.Core.Test;

public class UserManagerTest
{
    /// <summary>
    /// The student's repository mock
    /// </summary>
    private readonly Mock<IRepository<Student>> studentRepositoryMock;

    /// <summary>
    /// The tutor's repository mock
    /// </summary>
    private readonly Mock<IRepository<Tutor>> tutorRepositoryMock;

    /// <summary>
    /// The administrator's repository mock
    /// </summary>
    private readonly Mock<IRepository<Administrator>> administratorRepositoryMock;

    /// <summary>
    /// The department's repository mock
    /// </summary>
    private readonly Mock<IRepository<Department>> departmentRepositoryMock;

    /// <summary>
    /// The student's alert configuration repository mock
    /// </summary>
    private readonly Mock<IRepository<StudentAlertConfiguration>> studentAlertConfigurationRepository;

    /// <summary>
    /// The password encrypter mock
    /// </summary>
    private readonly Mock<IPasswordEncrypter> passwordEncrypterMock;

    /// <summary>
    /// The token generator mock
    /// </summary>
    private readonly Mock<ITokenGenerator> tokenGeneratorMock;

    /// <summary>
    /// The configuration mock
    /// </summary>
    private readonly Mock<IConfiguration> configurationMock;

    /// <summary>
    /// The storage client mock
    /// </summary>
    private readonly Mock<IStorageClient> storageClientMock;

    /// <summary>
    /// The user manager
    /// </summary>
    private readonly IUserManager userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserManagerTest"/> class
    /// </summary>
    public UserManagerTest()
    {
        this.studentRepositoryMock = new Mock<IRepository<Student>>();
        this.tutorRepositoryMock = new Mock<IRepository<Tutor>>();
        this.administratorRepositoryMock = new Mock<IRepository<Administrator>>();
        this.departmentRepositoryMock = new Mock<IRepository<Department>>();
        this.studentAlertConfigurationRepository = new Mock<IRepository<StudentAlertConfiguration>>();
        this.passwordEncrypterMock = new Mock<IPasswordEncrypter>();
        this.tokenGeneratorMock = new Mock<ITokenGenerator>();
        this.configurationMock = new Mock<IConfiguration>();
        this.storageClientMock = new Mock<IStorageClient>();

        this.userManager = new UserManager(this.studentRepositoryMock.Object, this.tutorRepositoryMock.Object, this.administratorRepositoryMock.Object, this.departmentRepositoryMock.Object, this.studentAlertConfigurationRepository.Object, this.passwordEncrypterMock.Object, this.tokenGeneratorMock.Object, this.configurationMock.Object, this.storageClientMock.Object);
    }

    #region LogIn tests

    /// <summary>
    /// Tests that the login returns the expected message when there is not any registered user with the same email.
    /// </summary>
    [Fact]
    public async Task LogInShouldReturnExpectedErrorMessageWhenUserDoesNotExistTest()
    {
        // Arrange
        LogInDTO logInDTO = new()
        {
            Email = "test@test.com",
            Password = "password"
        };

        IQueryable<Student> students = new List<Student>().AsQueryable();
        Mock<DbSet<Student>> mockSetStudent = students.BuildMockDbSet();
        this.studentRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetStudent.Object);

        IQueryable<Tutor> tutors = new List<Tutor>().AsQueryable();
        Mock<DbSet<Tutor>> mockSetTutor = tutors.BuildMockDbSet();
        this.tutorRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetTutor.Object);

        IQueryable<Administrator> administrators = new List<Administrator>().AsQueryable();
        Mock<DbSet<Administrator>> mockSetAdministrator = administrators.BuildMockDbSet();
        this.administratorRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetAdministrator.Object);

        // Act
        var result = await this.userManager.LogIn(logInDTO);

        // Assert
        Assert.Equal("No exsite ningún usuario con el email introducido.", result.ErrorMessage);
    }

    /// <summary>
    /// Tests that the login returns the expected message when the user's password is not correct.
    /// </summary>
    [Fact]
    public async Task LogInShouldReturnExpectedErrorMessageWhenUserPasswordIsIncorrectTest()
    {
        // Arrange
        LogInDTO logInDTO = new()
        {
            Email = "test@test.com",
            Password = "password"
        };

        Tutor tutor = new()
        {
            Email = "test@test.com",
            Password = "encryptedPassword"
        };

        IQueryable<Student> students = new List<Student>().AsQueryable();
        Mock<DbSet<Student>> mockSetStudent = students.BuildMockDbSet();
        this.studentRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetStudent.Object);

        IQueryable<Tutor> tutors = new List<Tutor>() { tutor }.AsQueryable();
        Mock<DbSet<Tutor>> mockSetTutor = tutors.BuildMockDbSet();
        this.tutorRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetTutor.Object);

        IQueryable<Administrator> administrators = new List<Administrator>().AsQueryable();
        Mock<DbSet<Administrator>> mockSetAdministrator = administrators.BuildMockDbSet();
        this.administratorRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetAdministrator.Object);

        this.passwordEncrypterMock.Setup(x => x.Encrypt(logInDTO.Password)).Returns("diferrentEncryptedPassword");

        // Act
        var result = await this.userManager.LogIn(logInDTO);

        // Assert
        Assert.Equal("La contraseña introducida no es correcta.", result.ErrorMessage);
    }

    /// <summary>
    /// Tests that the login is succesful.
    /// </summary>
    [Fact]
    public async Task LogInShouldReturnTokenTest()
    {
        // Arrange
        LogInDTO logInDTO = new()
        {
            Email = "test@test.com",
            Password = "password"
        };

        Student student = new()
        {
            Email = "test@test.com",
            Password = "encryptedPassword"
        };

        IQueryable<Student> students = new List<Student>() { student }.AsQueryable();
        Mock<DbSet<Student>> mockSetStudent = students.BuildMockDbSet();
        this.studentRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetStudent.Object);

        IQueryable<Tutor> tutors = new List<Tutor>().AsQueryable();
        Mock<DbSet<Tutor>> mockSetTutor = tutors.BuildMockDbSet();
        this.tutorRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetTutor.Object);

        IQueryable<Administrator> administrators = new List<Administrator>().AsQueryable();
        Mock<DbSet<Administrator>> mockSetAdministrator = administrators.BuildMockDbSet();
        this.administratorRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetAdministrator.Object);

        this.passwordEncrypterMock.Setup(x => x.Encrypt(logInDTO.Password)).Returns("encryptedPassword");
        this.tokenGeneratorMock.Setup(x => x.Generate(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns("generatedToken");

        // Act
        var result = await this.userManager.LogIn(logInDTO);

        // Assert
        Assert.Equal("generatedToken", result.Token);
    }

    #endregion

    #region RegisterStudent tests

    /// <summary>
    /// Tests that the register student returns the expected message when the email is already in use.
    /// </summary>
    [Fact]
    public async Task RegisterStudentShouldReturnExpectedErrorMessageWhenEmailIsAlreadyInUseTest()
    {
        // Arrange
        RegistryDTO registryDTO = new()
        {
            Name = "test",
            Surname = "test",
            Email = "test@test.com",
            Password = "password",
        };

        Student student = new()
        {
            Email = "test@test.com",
            Password = "encryptedPassword"
        };

        IQueryable<Student> students = new List<Student>() { student }.AsQueryable();
        Mock<DbSet<Student>> mockSetStudent = students.BuildMockDbSet();
        this.studentRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetStudent.Object);

        IQueryable<Tutor> tutors = new List<Tutor>().AsQueryable();
        Mock<DbSet<Tutor>> mockSetTutor = tutors.BuildMockDbSet();
        this.tutorRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetTutor.Object);

        IQueryable<Administrator> administrators = new List<Administrator>().AsQueryable();
        Mock<DbSet<Administrator>> mockSetAdministrator = administrators.BuildMockDbSet();
        this.administratorRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetAdministrator.Object);

        // Act
        var result = await this.userManager.RegisterStudent(registryDTO);

        // Assert
        Assert.Equal("Ya existe un usuario registrado con el mismo correo electrónico.", result.ErrorMessage);
    }

    /// <summary>
    /// Tests that the register student succesfully registers a student.
    /// </summary>
    [Fact]
    public async Task RegisterStudentShouldRegisterSuccessfullyTest()
    {
        // Arrange
        RegistryDTO registryDTO = new()
        {
            Name = "test",
            Surname = "test",
            Email = "test@test.com",
            Password = "password",
            ProfilePictureName = "test"
        };

        IQueryable<Student> students = new List<Student>().AsQueryable();
        Mock<DbSet<Student>> mockSetStudent = students.BuildMockDbSet();
        this.studentRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetStudent.Object);

        IQueryable<Tutor> tutors = new List<Tutor>().AsQueryable();
        Mock<DbSet<Tutor>> mockSetTutor = tutors.BuildMockDbSet();
        this.tutorRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetTutor.Object);

        IQueryable<Administrator> administrators = new List<Administrator>().AsQueryable();
        Mock<DbSet<Administrator>> mockSetAdministrator = administrators.BuildMockDbSet();
        this.administratorRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetAdministrator.Object);

        this.passwordEncrypterMock.Setup(x => x.Encrypt(registryDTO.Password)).Returns("encryptedPassword");
        this.tokenGeneratorMock.Setup(x => x.Generate(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns("generatedToken");
        this.storageClientMock.Setup(x => x.UploadFile(It.IsAny<string>(), It.IsAny<string>()));

        // Act
        var result = await this.userManager.RegisterStudent(registryDTO);

        // Assert
        Assert.Equal("generatedToken", result.Token);
    }

    #endregion

    #region RegisterTutor tests

    /// <summary>
    /// Tests that the register tutor returns the expected message when the email is already in use .
    /// </summary>
    [Fact]
    public async Task RegisterTutorShouldReturnExpectedErrorMessageWhenEmailIsAlreadyInUseTest()
    {
        // Arrange
        TutorRegistryDTO registryDTO = new()
        {
            Name = "test",
            Surname = "test",
            Email = "test@test.com",
            Password = "password",
        };

        Student student = new()
        {
            Email = "test@test.com",
            Password = "encryptedPassword"
        };

        IQueryable<Student> students = new List<Student>() { student }.AsQueryable();
        Mock<DbSet<Student>> mockSetStudent = students.BuildMockDbSet();
        this.studentRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetStudent.Object);

        IQueryable<Tutor> tutors = new List<Tutor>().AsQueryable();
        Mock<DbSet<Tutor>> mockSetTutor = tutors.BuildMockDbSet();
        this.tutorRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetTutor.Object);

        IQueryable<Administrator> administrators = new List<Administrator>().AsQueryable();
        Mock<DbSet<Administrator>> mockSetAdministrator = administrators.BuildMockDbSet();
        this.administratorRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetAdministrator.Object);

        // Act
        var result = await this.userManager.RegisterTutor(registryDTO);

        // Assert
        Assert.Equal("Ya existe un usuario registrado con el mismo correo electrónico.", result.ErrorMessage);
    }

    /// <summary>
    /// Tests that the register tutor succesfully registers a tutor.
    /// </summary>
    [Fact]
    public async Task RegisterTutorShouldRegisterSuccessfullyTest()
    {
        // Arrange
        TutorRegistryDTO registryDTO = new()
        {
            DepartmentId = 1,
            Name = "test",
            Surname = "test",
            Email = "test@test.com",
            Password = "password",
            ProfilePictureName = "test"
        };

        IQueryable<Student> students = new List<Student>().AsQueryable();
        Mock<DbSet<Student>> mockSetStudent = students.BuildMockDbSet();
        this.studentRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetStudent.Object);

        IQueryable<Tutor> tutors = new List<Tutor>().AsQueryable();
        Mock<DbSet<Tutor>> mockSetTutor = tutors.BuildMockDbSet();
        this.tutorRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetTutor.Object);

        IQueryable<Administrator> administrators = new List<Administrator>().AsQueryable();
        Mock<DbSet<Administrator>> mockSetAdministrator = administrators.BuildMockDbSet();
        this.administratorRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetAdministrator.Object);

        this.passwordEncrypterMock.Setup(x => x.Encrypt(registryDTO.Password)).Returns("encryptedPassword");
        this.tokenGeneratorMock.Setup(x => x.Generate(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns("generatedToken");
        this.storageClientMock.Setup(x => x.UploadFile(It.IsAny<string>(), It.IsAny<string>()));

        // Act
        var result = await this.userManager.RegisterTutor(registryDTO);

        // Assert
        Assert.Null(result.ErrorMessage);
    }

    #endregion

    #region GetStudents tests

    /// <summary>
    /// Tests that the get students method returns the expected student data.
    /// </summary>
    [Fact]
    public async Task GetStudentsTest()
    {
        // Arrange
        Tutor tutor = new()
        {
            Id = 1,
            Name = "Tutor name",
            Surname = "Tutor surname",
            ProfilePictureName = "Tutor profile picture name",
            Email = "tutor@gmail.com",
            Password = "encryptedPassword"
        };

        List<Student> studentsList = new()
        {
            new()
            {
                Id = 1,
                Name = "First student name",
                Surname = "First student surname",
                ProfilePictureName = "First student profile picture name",
                Email = "firstStudent@gmail.com",
                Password = "encryptedPassword"
            },
            new()
            {
                Id = 2,
                Name = "Second student name",
                Surname = "Second student surname",
                ProfilePictureName = "Second student profile picture name",
                Email = "secondStudent@gmail.com",
                Password = "encryptedPassword",
                TFG = new()
                {
                    Name = "TFG",
                    Tutor = tutor
                }
            },
        };

        IQueryable<Student> students = studentsList.AsQueryable();
        Mock<DbSet<Student>> mockSetStudent = students.BuildMockDbSet();
        this.studentRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetStudent.Object);

        // Act
        var result = await this.userManager.GetStudents();

        // Assert
        List<StudentDTO> expectedResult = new()
        {
            new()
            {
                Id = 1,
                Name = "First student name",
                Surname = "First student surname",
                TFG = false
            },
            new()
            {
                Id = 2,
                Name = "Second student name",
                Surname = "Second student surname",
                TFG = true
            }
        };
        Assert.Equivalent(expectedResult, result);
    }

    #endregion

    #region GetTutors tests

    /// <summary>
    /// Tests that the get tutors and TFGs method returns the expected tutors and TFGs data.
    /// </summary>
    [Fact]
    public async Task GetTutorsTest()
    {
        // Arrange
        Department department = new()
        {
            Id = 1,
            Name = "Department"
        };

        List<Student> studentsList = new()
        {
            new()
            {
                Id = 1,
                Name = "First student name",
                Surname = "First student surname",
                ProfilePictureName = "First student profile picture name",
                Email = "firstStudent@gmail.com",
                Password = "encryptedPassword"
            },
            new()
            {
                Id = 2,
                Name = "Second student name",
                Surname = "Second student surname",
                ProfilePictureName = "Second student profile picture name",
                Email = "secondStudent@gmail.com",
                Password = "encryptedPassword"
            },
        };

        List<Tutor> tutorsList = new()
        {
            new()
            {
                Id = 1,
                Name = "First tutor name",
                Surname = "First tutor surname",
                ProfilePictureName = "First tutor profile picture name",
                Email = "firstTutor@gmail.com",
                Password = "encryptedPassword",
                Department = department,
                TFGs = new List<TFG>()
                {
                    new()
                    {
                        Id = 1,
                        Name = "First TFG",
                        Student = studentsList[0]
                    },
                    new()
                    {
                        Id = 2,
                        Name = "Second TFG",
                        Student = studentsList[1]
                    },
                }
            },
            new()
            {
                Id = 2,
                Name = "Second tutor name",
                Surname = "Second tutor surname",
                ProfilePictureName = "Second tutor profile picture name",
                Email = "secondTutor@gmail.com",
                Password = "encryptedPassword",
                Department = department,
                TFGs = new List<TFG>()
            }
        };

        IQueryable<Tutor> tutors = tutorsList.AsQueryable();
        Mock<DbSet<Tutor>> mockSetTutor = tutors.BuildMockDbSet();
        this.tutorRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetTutor.Object);

        // Act
        var result = await this.userManager.GetTutorsAndTFGs();

        // Assert
        List<TutorDTO> expectedResult = new()
        {
            new()
            {
                Id = 1,
                Name = "First tutor name",
                Surname = "First tutor surname",
                DepartmentName = "Department",
                Email = "firstTutor@gmail.com",
                TFGs = {"First TFG", "Second TFG"}
            },
            new()
            {
                Id = 2,
                Name = "Second tutor name",
                Surname = "Second tutor surname",
                DepartmentName = "Department",
                Email = "secondTutor@gmail.com",
                TFGs = { }
            }
        };
        Assert.Equivalent(expectedResult, result);
    }

    #endregion

    #region GetDepartments tests

    /// <summary>
    /// Tests that the get departments method returns the expected department data.
    /// </summary>
    [Fact]
    public async Task GetDepartmentsTest()
    {
        // Arrange
        List<Department> departmentsList = new()
        {
            new()
            {
                Id = 1,
                Name = "Advanced Software"
            },
            new()
            {
                Id = 2,
                Name = "Artificial Intelligence"
            }
        };


        IQueryable<Department> departments = departmentsList.AsQueryable();
        Mock<DbSet<Department>> mockSetDepartment = departments.BuildMockDbSet();
        this.departmentRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetDepartment.Object);

        // Act
        var result = await this.userManager.GetDepartments();

        // Assert
        List<DepartmentDTO> expectedResult = new()
        {
            new()
            {
                Id = 1,
                Name = "Advanced Software"
            },
            new()
            {
                Id = 2,
                Name = "Artificial Intelligence"
            }
        };
        Assert.Equivalent(expectedResult, result);
    }

    #endregion

    #region GetStudentProfile tests

    /// <summary>
    /// Tests that GetStudentProfile returns the expected student profile data.
    /// </summary>
    [Fact]
    public async Task GetStudentProfileShouldReturnCorrectProfileTest()
    {
        // Arrange
        int studentId = 1;
        StudentProfileDTO expectedProfile = new StudentProfileDTO
        {
            Name = "Student Name",
            Surname = "Student Surname",
            Email = "student@example.com",
            AlertEmail = true,
            CalificationEmail = false,
            TotalTaskHours = 40,
            AnticipationDaysForFewerThanTotalTaskHoursTasks = 5,
            AnticipationDaysForMoreThanTotalTaskHoursTasks = 10
        };

        Student student = new Student
        {
            Id = studentId,
            Name = "Student Name",
            Surname = "Student Surname",
            Email = "student@example.com",
            AlertConfiguration = new StudentAlertConfiguration
            {
                AlertEmail = true,
                CalificationEmail = false,
                TotalTaskHours = 40,
                AnticipationDaysForFewerThanTotalTaskHoursTasks = 5,
                AnticipationDaysForMoreThanTotalTaskHoursTasks = 10
            }
        };

        List<Student> studentList = new List<Student> { student };
        IQueryable<Student> studentQueryable = studentList.AsQueryable();
        Mock<DbSet<Student>> mockSetStudent = studentQueryable.BuildMockDbSet();
        this.studentRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetStudent.Object);

        // Act
        StudentProfileDTO result = await this.userManager.GetStudentProfile(studentId);

        // Assert
        Assert.Equal(expectedProfile.Name, result.Name);
        Assert.Equal(expectedProfile.Surname, result.Surname);
        Assert.Equal(expectedProfile.Email, result.Email);
        Assert.Equal(expectedProfile.AlertEmail, result.AlertEmail);
        Assert.Equal(expectedProfile.CalificationEmail, result.CalificationEmail);
        Assert.Equal(expectedProfile.TotalTaskHours, result.TotalTaskHours);
        Assert.Equal(expectedProfile.AnticipationDaysForFewerThanTotalTaskHoursTasks, result.AnticipationDaysForFewerThanTotalTaskHoursTasks);
        Assert.Equal(expectedProfile.AnticipationDaysForMoreThanTotalTaskHoursTasks, result.AnticipationDaysForMoreThanTotalTaskHoursTasks);
    }

    #endregion

    #region GetTutorProfile tests

    /// <summary>
    /// Tests that GetTutorProfile returns the expected tutor profile data.
    /// </summary>
    [Fact]
    public async Task GetTutorProfileShouldReturnTutorProfileDataTest()
    {
        // Arrange
        int tutorId = 1;
        Tutor tutor = new()
        {
            Id = 1,
            Name = "Tutor name",
            Surname = "Tutor surname",
            ProfilePictureName = "Tutor profile picture name",
            Email = "tutor@gmail.com",
            Password = "encryptedPassword"
        };

        IQueryable<Tutor> tutors = new List<Tutor> { tutor }.AsQueryable();
        Mock<DbSet<Tutor>> mockSetTutor = tutors.BuildMockDbSet();
        this.tutorRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetTutor.Object);

        // Act
        var result = await this.userManager.GetTutorProfile(tutorId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Tutor name", result.Name);
        Assert.Equal("Tutor surname", result.Surname);
        Assert.Equal("tutor@gmail.com", result.Email);
    }

    #endregion

    #region UpdateStudentProfileShouldUpdateSuccessfullyTest tests

    /// <summary>
    /// Tests that UpdateStudentProfile updates the student's profile successfully.
    /// </summary>

    [Fact]
    public async Task UpdateStudentProfileShouldUpdateSuccessfullyTest()
    {
        // Arrange
        int studentId = 1;
        UpdateStudentProfileDTO updateDTO = new UpdateStudentProfileDTO
        {
            Id = studentId,
            Email = "updated@example.com",
            AlertEmail = false,
            CalificationEmail = false,
            TotalTaskHours = 50,
            AnticipationDaysForFewerThanTotalTaskHoursTasks = 7,
            AnticipationDaysForMoreThanTotalTaskHoursTasks = 12
        };

        Student existingStudent = new Student
        {
            Id = studentId,
            Name = "Student Name",
            Surname = "Student Surname",
            Email = "student@example.com",
            AlertConfiguration = new StudentAlertConfiguration
            {
                AlertEmail = true,
                CalificationEmail = true,
                TotalTaskHours = 40,
                AnticipationDaysForFewerThanTotalTaskHoursTasks = 5,
                AnticipationDaysForMoreThanTotalTaskHoursTasks = 10
            }
        };

        List<Student> studentList = new List<Student> { existingStudent };
        IQueryable<Student> studentQueryable = studentList.AsQueryable();
        Mock<DbSet<Student>> mockSetStudent = studentQueryable.BuildMockDbSet();
        this.studentRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetStudent.Object);

        this.studentRepositoryMock.Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(existingStudent);

        this.studentRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Student>()))
            .Callback<Student>(student =>
            {
                Student studentToUpdate = studentList.SingleOrDefault(s => s.Id == student.Id);
                if (studentToUpdate != null)
                {
                    studentToUpdate.Email = student.Email;
                    studentToUpdate.AlertConfiguration.AlertEmail = student.AlertConfiguration.AlertEmail;
                    studentToUpdate.AlertConfiguration.TotalTaskHours = student.AlertConfiguration.TotalTaskHours;
                    studentToUpdate.AlertConfiguration.AnticipationDaysForFewerThanTotalTaskHoursTasks = student.AlertConfiguration.AnticipationDaysForFewerThanTotalTaskHoursTasks;
                    studentToUpdate.AlertConfiguration.AnticipationDaysForMoreThanTotalTaskHoursTasks = student.AlertConfiguration.AnticipationDaysForMoreThanTotalTaskHoursTasks;
                }
            })
            .Returns(Task.CompletedTask);

        // Act
        await this.userManager.UpdateStudentProfile(updateDTO);

        // Assert
        Student updatedStudent = studentList.Single(s => s.Id == studentId);
        Assert.Equal("updated@example.com", updatedStudent.Email);
        Assert.False(updatedStudent.AlertConfiguration.AlertEmail);
        Assert.False(updatedStudent.AlertConfiguration.CalificationEmail);
        Assert.Equal(50, updatedStudent.AlertConfiguration.TotalTaskHours);
        Assert.Equal(7, updatedStudent.AlertConfiguration.AnticipationDaysForFewerThanTotalTaskHoursTasks);
        Assert.Equal(12, updatedStudent.AlertConfiguration.AnticipationDaysForMoreThanTotalTaskHoursTasks);
    }

    #endregion

    #region UpdateTutorProfile tests

    /// <summary>
    /// Tests that UpdateTutorProfile updates the tutor's profile successfully.
    /// </summary>
    [Fact]
    public async Task UpdateTutorProfileShouldUpdateSuccessfullyTest()
    {
        // Arrange
        int tutorId = 1;
        UpdateProfileDTO updateDTO = new UpdateProfileDTO
        {
            Id = tutorId,
            Email = "Updated email"
        };

        Tutor existingTutor = new Tutor
        {
            Id = tutorId,
            Name = "Old name",
            Surname = "Old surname",
            ProfilePictureName = "Old profile picture name",
            Email = "tutor@gmail.com",
            Password = "encryptedPassword"
        };

        List<Tutor> tutorList = new List<Tutor> { existingTutor };
        IQueryable<Tutor> tutorQueryable = tutorList.AsQueryable();
        Mock<DbSet<Tutor>> mockSetTutor = tutorQueryable.BuildMockDbSet();

        this.tutorRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetTutor.Object);
        this.tutorRepositoryMock.Setup(x => x.GetByIdAsync(tutorId))
            .ReturnsAsync(existingTutor);
        this.tutorRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Tutor>()))
            .Callback<Tutor>(t =>
            {
                Tutor tutorToUpdate = tutorList.SingleOrDefault(tt => tt.Id == t.Id);
                if (tutorToUpdate != null)
                {
                    tutorToUpdate.Email = t.Email;
                }
            })
            .Returns(Task.CompletedTask);

        // Act
        await this.userManager.UpdateTutorProfile(updateDTO);

        // Assert
        Tutor updatedTutor = tutorList.Single(t => t.Id == tutorId);
        Assert.Equal("Updated email", updatedTutor.Email);
    }

    #endregion
}