using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OrientaTFG.Shared.Infrastructure.Enums;
using OrientaTFG.Shared.Infrastructure.Model;
using OrientaTFG.Shared.Infrastructure.Repository;
using OrientaTFG.Shared.Infrastructure.Utils.StorageClient;
using OrientaTFG.User.Core.DTOs;
using OrientaTFG.User.Core.Utils.PasswordEncrypter;
using OrientaTFG.User.Core.Utils.TokenGenerator;

namespace OrientaTFG.User.Core;

public class UserManager : IUserManager
{
    /// <summary>
    /// The student's repository
    /// </summary>
    private readonly IRepository<Student> studentRepository;

    /// <summary>
    /// The tutor's repository
    /// </summary>
    private readonly IRepository<Tutor> tutorRepository;

    /// <summary>
    /// The administrator's repository
    /// </summary>
    private readonly IRepository<Administrator> administratorRepository;

    /// <summary>
    /// The department's repository
    /// </summary>
    private readonly IRepository<Department> departmentRepository;

    /// <summary>
    /// The student's alert configuration repository
    /// </summary>
    private readonly IRepository<StudentAlertConfiguration> studentAlertConfigurationRepository;

    /// <summary>
    /// The password encrypter
    /// </summary>
    private readonly IPasswordEncrypter passwordEncrypter;

    /// <summary>
    /// The token generator
    /// </summary>
    private readonly ITokenGenerator tokenGenerator;

    /// <summary>
    /// The configuration
    /// </summary>
    private readonly IConfiguration configuration;

    /// <summary>
    /// The storage client
    /// </summary>
    private readonly IStorageClient storageClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserManager"/> class
    /// </summary>
    /// <param name="studentRepository">The student's repository</param>
    /// <param name="tutorRepository">The tutor's repository</param>
    /// <param name="administratorRepository">The administrator's repository</param>
    /// <param name="departmentRepository">The department's repository</param>
    /// <param name="studentAlertConfigurationRepository">The student's alert configuration repository</param>
    /// <param name="passwordEncrypter">The password encrypter</param>
    /// <param name="tokenGenerator">The token generator</param>
    /// <param name="configuration">The configuration</param>
    /// <param name="storageClient">The storage client</param>
    public UserManager(IRepository<Student> studentRepository, IRepository<Tutor> tutorRepository, IRepository<Administrator> administratorRepository, IRepository<Department> departmentRepository, IRepository<StudentAlertConfiguration> studentAlertConfigurationRepository, IPasswordEncrypter passwordEncrypter, ITokenGenerator tokenGenerator, IConfiguration configuration, IStorageClient storageClient)
    {
        this.studentRepository = studentRepository;
        this.tutorRepository = tutorRepository;
        this.administratorRepository = administratorRepository;
        this.departmentRepository = departmentRepository;
        this.studentAlertConfigurationRepository = studentAlertConfigurationRepository;
        this.passwordEncrypter = passwordEncrypter;
        this.tokenGenerator = tokenGenerator;
        this.configuration = configuration;
        this.storageClient = storageClient;
    }

    /// <summary>
    /// Authenticates the user and returns a token if successful.
    /// </summary>
    /// <param name="logInDTO">Contains the user's email and password.</param>
    /// <returns>A token if login is successful; otherwise, an error message.</returns>
    public async Task<LogInResponseDTO> LogIn(LogInDTO logInDTO)
    {
        try
        {
            LogInResponseDTO logInResponseDTO = new();
            Student? student = await studentRepository.AsQueryable().FirstOrDefaultAsync(x => x.Email == logInDTO.Email);

            if (student is null)
            {
                Tutor? tutor = await tutorRepository.AsQueryable().FirstOrDefaultAsync(x => x.Email == logInDTO.Email);

                if (tutor is null)
                {
                    Administrator? administrator = await administratorRepository.AsQueryable().FirstOrDefaultAsync(x => x.Email == logInDTO.Email);

                    if (administrator is null)
                    {
                        logInResponseDTO.ErrorMessage = "No exsite ningún usuario con el email introducido.";
                    }
                    else
                    {
                        string insertedPassword = passwordEncrypter.Encrypt(logInDTO.Password);
                        if (insertedPassword != administrator.Password)
                        {
                            logInResponseDTO.ErrorMessage = "La contraseña introducida no es correcta.";
                        }
                        else
                        {
                            logInResponseDTO.Token = tokenGenerator.Generate(administrator.Id, nameof(RoleEnum.Administrator), configuration["SecretKey"]);
                            logInResponseDTO.Id = administrator.Id;
                            logInResponseDTO.ProfilePicture = await this.storageClient.GetFileContent(administrator.ProfilePictureName);
                            logInResponseDTO.Role = RoleEnum.Administrator;
                        }
                    }
                }
                else
                {
                    string insertedPassword = passwordEncrypter.Encrypt(logInDTO.Password);
                    if (insertedPassword != tutor.Password)
                    {
                        logInResponseDTO.ErrorMessage = "La contraseña introducida no es correcta.";
                    }
                    else
                    {
                        logInResponseDTO.Token = tokenGenerator.Generate(tutor.Id, nameof(RoleEnum.Tutor), configuration["SecretKey"]);
                        logInResponseDTO.Id = tutor.Id;
                        logInResponseDTO.ProfilePicture = await this.storageClient.GetFileContent(tutor.ProfilePictureName);
                        logInResponseDTO.Role = RoleEnum.Tutor;
                    }
                }
            }
            else
            {
                string insertedPassword = passwordEncrypter.Encrypt(logInDTO.Password);
                if (insertedPassword != student.Password)
                {
                    logInResponseDTO.ErrorMessage = "La contraseña introducida no es correcta.";
                }
                else
                {
                    logInResponseDTO.Token = tokenGenerator.Generate(student.Id, nameof(RoleEnum.Estudiante), configuration["SecretKey"]);
                    logInResponseDTO.Id = student.Id;
                    logInResponseDTO.ProfilePicture = await this.storageClient.GetFileContent(student.ProfilePictureName);
                    logInResponseDTO.Role = RoleEnum.Estudiante;
                }
            }

            return logInResponseDTO;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Registers a new student.
    /// </summary>
    /// <param name="registryDTO">Contains the student's details.</param>
    /// <returns>A token if registration is successful; otherwise, an error message.</returns>
    public async Task<LogInResponseDTO> RegisterStudent(RegistryDTO registryDTO)
    {
        try
        {
            LogInResponseDTO logInResponseDTO = new();
            Student? student = await studentRepository.AsQueryable().FirstOrDefaultAsync(x => x.Email == registryDTO.Email);
            Tutor? tutor = await tutorRepository.AsQueryable().FirstOrDefaultAsync(x => x.Email == registryDTO.Email);
            Administrator? administrator = await administratorRepository.AsQueryable().FirstOrDefaultAsync(x => x.Email == registryDTO.Email);

            if (student is not null || tutor is not null || administrator is not null)
            {
                logInResponseDTO.ErrorMessage = "Ya existe un usuario registrado con el mismo correo electrónico.";
            }
            else
            {
                string dateTimeProfilePictureName = $"{DateTime.Now:MM-dd-yyyy-HH:mm:ss}-{registryDTO.ProfilePictureName}";

                student = new()
                {
                    Name = registryDTO.Name,
                    Surname = registryDTO.Surname,
                    Email = registryDTO.Email,
                    ProfilePictureName = dateTimeProfilePictureName,
                    Password = passwordEncrypter.Encrypt(registryDTO.Password),
                    AlertConfiguration = new()
                };

                await storageClient.UploadFile(registryDTO.ProfilePicture, dateTimeProfilePictureName);
                await studentRepository.AddAsync(student);

                logInResponseDTO.Id = studentRepository.AsQueryable().FirstOrDefaultAsync(x => x.Email == registryDTO.Email).Id;
                logInResponseDTO.Token = tokenGenerator.Generate(logInResponseDTO.Id, nameof(RoleEnum.Estudiante), configuration["SecretKey"]);
                logInResponseDTO.ProfilePicture = registryDTO.ProfilePicture;
                logInResponseDTO.Role = RoleEnum.Estudiante;
            }
            return logInResponseDTO;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    /// <summary>
    /// Registers a new tutor.
    /// </summary>
    /// <param name="tutorRegistryDTO">Contains the tutor's details.</param>
    /// <returns>An error message if the registery could not be completed.</returns>
    public async Task<ErrorMessageDTO> RegisterTutor(TutorRegistryDTO tutorRegistryDTO)
    {
        try
        {
            ErrorMessageDTO errorMessageDTO = new();
            Student? student = await studentRepository.AsQueryable().FirstOrDefaultAsync(x => x.Email == tutorRegistryDTO.Email);
            Tutor? tutor = await tutorRepository.AsQueryable().FirstOrDefaultAsync(x => x.Email == tutorRegistryDTO.Email);
            Administrator? administrator = await administratorRepository.AsQueryable().FirstOrDefaultAsync(x => x.Email == tutorRegistryDTO.Email);

            if (student is not null || tutor is not null || administrator is not null)
            {
                errorMessageDTO.ErrorMessage = "Ya existe un usuario registrado con el mismo correo electrónico.";
            }
            else
            {
                string dateTimeProfilePictureName = $"{DateTime.Now:MM-dd-yyyy-HH:mm:ss}-{tutorRegistryDTO.ProfilePictureName}";

                tutor = new()
                {
                    DepartmentId = tutorRegistryDTO.DepartmentId,
                    Name = tutorRegistryDTO.Name,
                    Surname = tutorRegistryDTO.Surname,
                    Email = tutorRegistryDTO.Email,
                    ProfilePictureName = dateTimeProfilePictureName,
                    Password = this.passwordEncrypter.Encrypt(tutorRegistryDTO.Password)
                };

                await storageClient.UploadFile(tutorRegistryDTO.ProfilePicture, dateTimeProfilePictureName);
                await tutorRepository.AddAsync(tutor);
            }
            return errorMessageDTO;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Retrieves the list of all students.
    /// </summary>
    /// <returns>The list of all students.</returns>
    public async Task<List<StudentDTO>> GetStudents()
    {
        try
        {
            List<StudentDTO> studentsList = new();
            List<Student> students = await studentRepository.AsQueryable().Include(student => student.TFG).ToListAsync();

            foreach (Student student in students)
            {
                StudentDTO studentDTO = new StudentDTO
                {
                    Id = student.Id,
                    Name = student.Name,
                    Surname = student.Surname,
                    ProfilePicture = await storageClient.GetFileContent(student.ProfilePictureName),
                    TFG = student.TFG != null
                };
                studentsList.Add(studentDTO);
            }

            return studentsList;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Retrieves the list of all tutors with their TFGs.
    /// </summary>
    /// <returns>The list of all tutors with their TFGs.</returns>
    public async Task<List<TutorDTO>> GetTutorsAndTFGs()
    {
        try
        {
            List<TutorDTO> tutorsList = new();
            List<Tutor> tutors = await tutorRepository.AsQueryable().Include(x => x.Department).Include(x => x.TFGs).ToListAsync();

            foreach (Tutor tutor in tutors)
            {
                TutorDTO tutorDTO = new TutorDTO
                {
                    Id = tutor.Id,
                    Name = tutor.Name,
                    Surname = tutor.Surname,
                    ProfilePicture = await storageClient.GetFileContent(tutor.ProfilePictureName),
                    DepartmentName = tutor.Department?.Name,
                    Email = tutor.Email,
                    TFGs = tutor.TFGs?.Select(x => x.Name).ToList()
                };

                tutorsList.Add(tutorDTO);
            }

            return tutorsList;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Retrieves the student's tutor.
    /// </summary>
    /// <returns>The student's tutor</returns>
    public async Task<UserDTO> GetStudentTutor(int studentId)
    {
        try
        {
            IQueryable<UserDTO> tutorInfo = from tutor in tutorRepository.AsQueryable().Include(x => x.TFGs)
                                            where tutor.TFGs.Any(tfg => tfg.Student.Id == studentId)
                                            select new UserDTO
                                            {
                                                Id = tutor.Id,
                                                Name = tutor.Name,
                                                Surname = tutor.Surname,
                                                ProfilePicture = tutor.ProfilePictureName
                                            };

            UserDTO user = tutorInfo.FirstOrDefault();
            if (user != null)
            {
                user.ProfilePicture = await storageClient.GetFileContent(user.ProfilePicture);
            }
            return user;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Retrieves the list of all departments.
    /// </summary>
    /// <returns>The list of all departments.</returns>
    public async Task<List<DepartmentDTO>> GetDepartments()
    {
        try
        {
            List<Department> departments = await departmentRepository.AsQueryable().ToListAsync();
            return departments.Select(department => new DepartmentDTO
            {
                Id = department.Id,
                Name = department.Name
            }).ToList();
        }
        catch (Exception)
        {
            return new List<DepartmentDTO>();
        }
    }

    /// <summary>
    /// Retrieves the profile of a student.
    /// </summary>
    /// <param name="studentId">The ID of the student.</param>
    /// <returns>The student's profile.</returns>
    public async Task<StudentProfileDTO> GetStudentProfile(int studentId)
    {
        try
        {
            IQueryable<StudentProfileDTO> studentProfile = from student in studentRepository.AsQueryable()
                                                           where student.Id == studentId
                                                           select new StudentProfileDTO
                                                           {
                                                               Name = student.Name,
                                                               Surname = student.Surname,
                                                               Email = student.Email,
                                                               AlertEmail = student.AlertConfiguration.AlertEmail,
                                                               CalificationEmail = student.AlertConfiguration.CalificationEmail,
                                                               TotalTaskHours = student.AlertConfiguration.TotalTaskHours,
                                                               AnticipationDaysForFewerThanTotalTaskHoursTasks = student.AlertConfiguration.AnticipationDaysForFewerThanTotalTaskHoursTasks,
                                                               AnticipationDaysForMoreThanTotalTaskHoursTasks = student.AlertConfiguration.AnticipationDaysForMoreThanTotalTaskHoursTasks
                                                           };

            return studentProfile.FirstOrDefault();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Retrieves the profile of a tutor.
    /// </summary>
    /// <param name="tutorId">The ID of the tutor.</param>
    /// <returns>The tutor's profile.</returns>
    public async Task<ProfileDTO> GetTutorProfile(int tutorId)
    {
        try
        {
            ProfileDTO? tutorProfile = await tutorRepository.AsQueryable()
                .Where(tutor => tutor.Id == tutorId)
                .Select(tutor => new ProfileDTO
                {
                    Name = tutor.Name,
                    Surname = tutor.Surname,
                    Email = tutor.Email
                })
                .FirstOrDefaultAsync();

            return tutorProfile;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Updates the profile of a student.
    /// </summary>
    /// <param name="updateStudentProfileDTO">The data to update the student's profile.</param>
    public async Task UpdateStudentProfile(UpdateStudentProfileDTO updateStudentProfileDTO)
    {
        try
        {
            Student? student = await studentRepository.GetByIdAsync(updateStudentProfileDTO.Id);
            if (student == null)
            {
                return;
            }

            student.Email = updateStudentProfileDTO.Email;
            student.AlertConfiguration.AlertEmail = updateStudentProfileDTO.AlertEmail;
            student.AlertConfiguration.CalificationEmail = updateStudentProfileDTO.AlertEmail;
            student.AlertConfiguration.TotalTaskHours = updateStudentProfileDTO.TotalTaskHours;
            student.AlertConfiguration.AnticipationDaysForFewerThanTotalTaskHoursTasks = updateStudentProfileDTO.AnticipationDaysForFewerThanTotalTaskHoursTasks;
            student.AlertConfiguration.AnticipationDaysForMoreThanTotalTaskHoursTasks = updateStudentProfileDTO.AnticipationDaysForMoreThanTotalTaskHoursTasks;

            await studentRepository.UpdateAsync(student);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Updates the profile of a tutor.
    /// </summary>
    /// <param name="updateProfileDTO">The data to update the tutor's profile.</param>
    public async Task UpdateTutorProfile(UpdateProfileDTO updateProfileDTO)
    {
        try
        {
            Tutor? tutor = await tutorRepository.GetByIdAsync(updateProfileDTO.Id);
            if (tutor == null)
            {
                return;
            }

            tutor.Email = updateProfileDTO.Email;

            await tutorRepository.UpdateAsync(tutor);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
