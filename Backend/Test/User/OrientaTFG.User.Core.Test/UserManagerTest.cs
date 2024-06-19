using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using OrientaTFG.Shared.Infrastructure.Model;
using OrientaTFG.Shared.Infrastructure.Repository;
using OrientaTFG.User.Core.DTOs;
using OrientaTFG.User.Core.Utils.StorageClient;
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
        this.passwordEncrypterMock = new Mock<IPasswordEncrypter>();
        this.tokenGeneratorMock = new Mock<ITokenGenerator>();
        this.configurationMock = new Mock<IConfiguration>();
        this.storageClientMock = new Mock<IStorageClient>();

        this.userManager = new UserManager(this.studentRepositoryMock.Object, this.tutorRepositoryMock.Object, this.administratorRepositoryMock.Object, this.passwordEncrypterMock.Object, this.tokenGeneratorMock.Object, this.configurationMock.Object, this.storageClientMock.Object);
    }

    #region LogIn tests

    /// <summary>
    /// Tests that the login returns the expected message when there is not any register user with the same email
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
    /// Tests that the login returns the expected message when the user's password is not correct and also increases its login retries count
    /// </summary>
    [Fact]
    public async Task LogInShouldReturnExpectedErrorMessageWhenUserPasswordIsIncorrectAndIncrementLoginRetriesCountTest()
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
            Password = "encryptedPassword",
            LogInBlocked = false,
            LogInRetries = 0
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
        Assert.Equal(1, tutor.LogInRetries);
    }

    /// <summary>
    /// Tests that the login returns the expected message when the user's password is not correct, increases its login retries count and blocks the login because the limit has been reached
    /// </summary>
    [Fact]
    public async Task LogInShouldReturnExpectedErrorMessageWhenUserPasswordIsIncorrectAndIncrementLoginRetriesCountAndBlockLogInTest()
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
            Password = "encryptedPassword",
            LogInBlocked = false,
            LogInRetries = 4
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

        this.passwordEncrypterMock.Setup(x => x.Encrypt(logInDTO.Password)).Returns("diferrentEncryptedPassword");

        // Act
        var result = await this.userManager.LogIn(logInDTO);

        // Assert
        Assert.Equal("La contraseña introducida no es correcta.", result.ErrorMessage);
        Assert.Equal(5, student.LogInRetries);
        Assert.True(student.LogInBlocked);
    }

    /// <summary>
    /// Tests that the login returns the expected message when the user's login is blocked
    /// </summary>
    [Fact]
    public async Task LogInShouldReturnExpectedErrorMessageWhenUserLogInIsBlockedTest()
    {
        // Arrange
        LogInDTO logInDTO = new()
        {
            Email = "test@test.com",
            Password = "password"
        };

        Administrator administrator = new()
        {
            Email = "test@test.com",
            Password = "encryptedPassword",
            LogInBlocked = true,
            LogInRetries = 5
        };

        IQueryable<Student> students = new List<Student>().AsQueryable();
        Mock<DbSet<Student>> mockSetStudent = students.BuildMockDbSet();
        this.studentRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetStudent.Object);

        IQueryable<Tutor> tutors = new List<Tutor>().AsQueryable();
        Mock<DbSet<Tutor>> mockSetTutor = tutors.BuildMockDbSet();
        this.tutorRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetTutor.Object);

        IQueryable<Administrator> administrators = new List<Administrator>() { administrator }.AsQueryable();
        Mock<DbSet<Administrator>> mockSetAdministrator = administrators.BuildMockDbSet();
        this.administratorRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetAdministrator.Object);

        this.passwordEncrypterMock.Setup(x => x.Encrypt(logInDTO.Password)).Returns("diferrentEncryptedPassword");

        // Act
        var result = await this.userManager.LogIn(logInDTO);

        // Assert
        Assert.Equal("Se ha superado el límite de reintentos de inicio de sesión fallidos y el login del usuario ha sido bloqueado.", result.ErrorMessage);
    }

    /// <summary>
    /// Tests that the login is succesful
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
            Password = "encryptedPassword",
            LogInBlocked = false,
            LogInRetries = 2
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
        this.tokenGeneratorMock.Setup(x => x.Generate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns("generatedToken");

        // Act
        var result = await this.userManager.LogIn(logInDTO);

        // Assert
        Assert.Equal("generatedToken", result.Token);
    }

    #endregion

    #region GetStudents tests

    /// <summary>
    /// Tests that get students returns data 
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
            Password = "encryptedPassword",
            LogInRetries = 0,
            LogInBlocked = false
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
                Password = "encryptedPassword",
                LogInRetries = 2,
                LogInBlocked = false
            },
            new()
            {
                Id = 2,
                Name = "Second student name",
                Surname = "Second student surname",
                ProfilePictureName = "Second student profile picture name",
                Email = "secondStudent@gmail.com",
                Password = "encryptedPassword",
                LogInRetries = 0,
                LogInBlocked = false,
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
                ProfilePicture = [],
                TFG = false
            },
            new()
            {
                Id = 2,
                Name = "Second student name",
                Surname = "Second student surname",
                ProfilePicture = [],
                TFG = true
            }
        };
        Assert.Equivalent(expectedResult, result);
    }

    #endregion

    #region GetTutors tests

    /// <summary>
    /// Tests that get tutors returns data 
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
                Password = "encryptedPassword",
                LogInRetries = 2,
                LogInBlocked = false
            },
            new()
            {
                Id = 2,
                Name = "Second student name",
                Surname = "Second student surname",
                ProfilePictureName = "Second student profile picture name",
                Email = "secondStudent@gmail.com",
                Password = "encryptedPassword",
                LogInRetries = 0,
                LogInBlocked = false
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
                LogInRetries = 0,
                LogInBlocked = false,
                Department = department,
                TFGs = new List<TFG>()
                {
                    new()
                    {
                        Id = 1,
                        Student = studentsList[0]
                    },
                    new()
                    {
                        Id = 2,
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
                LogInRetries = 0,
                LogInBlocked = false,
                Department = department
            }
        };

        IQueryable<Tutor> tutors = tutorsList.AsQueryable();
        Mock<DbSet<Tutor>> mockSetTutor = tutors.BuildMockDbSet();
        this.tutorRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockSetTutor.Object);

        // Act
        var result = await this.userManager.GetTutors();

        // Assert
        List<TutorDTO> expectedResult = new()
        {
            new()
            {
                Id = 1,
                Name = "First tutor name",
                Surname = "First tutor surname",
                ProfilePicture = [],
                DepartmentName = "Department",
                Email = "firstTutor@gmail.com",
                Password = "encryptedPassword"
            },
            new()
            {
                Id = 2,
                Name = "Second tutor name",
                Surname = "Second tutor surname",
                ProfilePicture = [],
                DepartmentName = "Department",
                Email = "secondTutor@gmail.com",
                Password = "encryptedPassword"
            }
        };
        Assert.Equivalent(expectedResult, result);
    }

    #endregion
}