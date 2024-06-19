using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OrientaTFG.Shared.Infrastructure.Enums;
using OrientaTFG.Shared.Infrastructure.Model;
using OrientaTFG.Shared.Infrastructure.Repository;
using OrientaTFG.User.Core.DTOs;
using OrientaTFG.User.Core.Utils.StorageClient;
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
    /// <param name="passwordEncrypter">The password encrypter</param>
    /// <param name="tokenGenerator">The token generator</param>
    /// <param name="configuration">The configuration</param>
    /// <param name="storageClient">The storage client</param>
    public UserManager(IRepository<Student> studentRepository, IRepository<Tutor> tutorRepository, IRepository<Administrator> administratorRepository, IPasswordEncrypter passwordEncrypter, ITokenGenerator tokenGenerator, IConfiguration configuration, IStorageClient storageClient)
    {
        this.studentRepository = studentRepository;
        this.tutorRepository = tutorRepository;
        this.administratorRepository = administratorRepository;
        this.passwordEncrypter = passwordEncrypter;
        this.tokenGenerator = tokenGenerator;
        this.configuration = configuration;
        this.storageClient = storageClient;
    }

    /// <summary>
    /// Login method for all users
    /// </summary>
    /// <param name="logInDTO">The user's email and password</param>
    /// <returns>Token if the login was successful, error message otherwise</returns>
    public async Task<LogInResponseDTO> LogIn(LogInDTO logInDTO)
    {
        LogInResponseDTO logInResponseDTO = new();
        Student student = await studentRepository.AsQueryable().FirstOrDefaultAsync(x => x.Email == logInDTO.Email);

        if (student is null)
        {
            Tutor tutor = await tutorRepository.AsQueryable().FirstOrDefaultAsync(x => x.Email == logInDTO.Email);

            if (tutor is null)
            {
                Administrator administrator = await administratorRepository.AsQueryable().FirstOrDefaultAsync(x => x.Email == logInDTO.Email);

                if (administrator is null)
                {
                    logInResponseDTO.ErrorMessage = "No exsite ningún usuario con el email introducido.";
                }
                else
                {
                    if (administrator.LogInBlocked)
                    {
                        logInResponseDTO.ErrorMessage = "Se ha superado el límite de reintentos de inicio de sesión fallidos y el login del usuario ha sido bloqueado.";
                    }
                    else
                    {
                        string insertedPassword = passwordEncrypter.Encrypt(logInDTO.Password);
                        if (insertedPassword != administrator.Password)
                        {
                            logInResponseDTO.ErrorMessage = "La contraseña introducida no es correcta.";
                            administrator.LogInRetries++;
                            if (administrator.LogInRetries == 5)
                            {
                                administrator.LogInBlocked = true;
                            }
                            administratorRepository.Update(administrator);
                            administratorRepository.SaveChanges();
                        }
                        else
                        {
                            logInResponseDTO.Token = tokenGenerator.Generate(logInDTO.Email, nameof(RoleEnum.Administrator), configuration["SecretKey"]);
                            logInResponseDTO.Id = administrator.Id;
                            logInResponseDTO.ProfilePicture = await this.storageClient.GetFileContent(administrator.ProfilePictureName);
                            logInResponseDTO.Role = RoleEnum.Administrator;
                        }
                    }
                }
            }
            else
            {
                if (tutor.LogInBlocked)
                {
                    logInResponseDTO.ErrorMessage = "Se ha superado el límite de reintentos de inicio de sesión fallidos y el login del usuario ha sido bloqueado.";
                }
                else
                {
                    string insertedPassword = passwordEncrypter.Encrypt(logInDTO.Password);
                    if (insertedPassword != tutor.Password)
                    {
                        logInResponseDTO.ErrorMessage = "La contraseña introducida no es correcta.";
                        tutor.LogInRetries++;
                        if (tutor.LogInRetries == 5)
                        {
                            tutor.LogInBlocked = true;
                        }
                        tutorRepository.Update(tutor);
                        tutorRepository.SaveChanges();
                    }
                    else
                    {
                        logInResponseDTO.Token = tokenGenerator.Generate(logInDTO.Email, nameof(RoleEnum.Tutor), configuration["SecretKey"]);
                        logInResponseDTO.Id = tutor.Id;
                        logInResponseDTO.ProfilePicture = await this.storageClient.GetFileContent(tutor.ProfilePictureName);
                        logInResponseDTO.Role = RoleEnum.Tutor;
                    }
                }
            }
        }
        else
        {
            if (student.LogInBlocked)
            {
                logInResponseDTO.ErrorMessage = "Se ha superado el límite de reintentos de inicio de sesión fallidos y el login del usuario ha sido bloqueado.";
            }
            else
            {
                string insertedPassword = passwordEncrypter.Encrypt(logInDTO.Password);
                if (insertedPassword != student.Password)
                {
                    logInResponseDTO.ErrorMessage = "La contraseña introducida no es correcta.";
                    student.LogInRetries++;
                    if (student.LogInRetries == 5)
                    {
                        student.LogInBlocked = true;
                    }
                    studentRepository.Update(student);
                    studentRepository.SaveChanges();
                }
                else
                {
                    logInResponseDTO.Token = tokenGenerator.Generate(logInDTO.Email, nameof(RoleEnum.Estudiante), configuration["SecretKey"]);
                    logInResponseDTO.Id = student.Id;
                    logInResponseDTO.ProfilePicture = await this.storageClient.GetFileContent(student.ProfilePictureName);
                    logInResponseDTO.Role = RoleEnum.Estudiante;
                }
            }
        }

        return logInResponseDTO;
    }

    /// <summary>
    /// Gets the students list
    /// </summary>
    /// <returns>The students list</returns>
    public async Task<List<StudentDTO>> GetStudents()
    {
        IQueryable<Student> students = from student in studentRepository.AsQueryable()
                                       select new Student
                                       {
                                           Id = student.Id,
                                           Name = student.Name,
                                           Surname = student.Surname,
                                           ProfilePictureName = student.ProfilePictureName,
                                           TFG = student.TFG
                                       };

        List<StudentDTO> studentsList = new();
        StudentDTO studentDTO;

        foreach (Student student in students)
        {
            studentDTO = new()
            {
                Id = student.Id,
                Name = student.Name,
                Surname = student.Surname,
                ProfilePicture = await this.storageClient.GetFileContent(student.ProfilePictureName),
                TFG = student.TFG != null
            };
            studentsList.Add(studentDTO);
        }
        return studentsList;
    }

    /// <summary>
    /// Gets the tutors list
    /// </summary>
    /// <returns>The tutors list</returns>
    public async Task<List<TutorDTO>> GetTutors()
    {
        IQueryable<Tutor> tutors = from tutor in tutorRepository.AsQueryable()
                                      select new Tutor
                                      {
                                          Id = tutor.Id,
                                          Name = tutor.Name,
                                          Surname = tutor.Surname,
                                          ProfilePictureName = tutor.ProfilePictureName,
                                          Department = tutor.Department,
                                          Email = tutor.Email,
                                          Password = tutor.Password,
                                      };

        List<TutorDTO> tutorsList = new();
        TutorDTO tutorDTO;

        foreach (Tutor tutor in tutors)
        {
            tutorDTO = new()
            {
                Id = tutor.Id,
                Name = tutor.Name,
                Surname = tutor.Surname,
                ProfilePicture = await this.storageClient.GetFileContent(tutor.ProfilePictureName),
                DepartmentName = tutor.Department.Name,
                Email = tutor.Email,
                Password = tutor.Password,
            };
            tutorsList.Add(tutorDTO);
        }
        return tutorsList;
    }
}
