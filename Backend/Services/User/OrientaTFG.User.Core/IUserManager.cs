using OrientaTFG.User.Core.DTOs;

namespace OrientaTFG.User.Core;

public interface IUserManager
{
    /// <summary>
    /// Authenticates the user and returns a token if successful.
    /// </summary>
    /// <param name="logInDTO">Contains the user's email and password.</param>
    /// <returns>A token if login is successful; otherwise, an error message.</returns>
    Task<LogInResponseDTO> LogIn(LogInDTO logInDTO);

    /// <summary>
    /// Registers a new student.
    /// </summary>
    /// <param name="registryDTO">Contains the student's details.</param>
    /// <returns>A token if registration is successful; otherwise, an error message.</returns>
    Task<LogInResponseDTO> RegisterStudent(RegistryDTO registryDTO);

    /// <summary>
    /// Registers a new tutor.
    /// </summary>
    /// <param name="tutorRegistryDTO">Contains the tutor's details.</param>
    /// <returns>An error message if the registery could not be completed.</returns>
    Task<ErrorMessageDTO> RegisterTutor(TutorRegistryDTO tutorRegistryDTO);

    /// <summary>
    /// Retrieves the list of all students.
    /// </summary>
    /// <returns>The list of all students.</returns>
    Task<List<StudentDTO>> GetStudents();

    /// <summary>
    /// Retrieves the list of all tutors with their TFGs.
    /// </summary>
    /// <returns>The list of all tutors with their TFGs.</returns>
    Task<List<TutorDTO>> GetTutorsAndTFGs();

    /// <summary>
    /// Retrieves the student's tutor.
    /// </summary>
    /// <returns>The student's tutor</returns>
    Task<UserDTO> GetStudentTutor(int studentId);

    /// <summary>
    /// Retrieves the list of all departments.
    /// </summary>
    /// <returns>The list of all departments.</returns>
    Task<List<DepartmentDTO>> GetDepartments();

    /// <summary>
    /// Retrieves the profile of a student.
    /// </summary>
    /// <param name="studentId">The ID of the student.</param>
    /// <returns>The student's profile.</returns>
    Task<StudentProfileDTO> GetStudentProfile(int studentId);

    /// <summary>
    /// Retrieves the profile of a tutor.
    /// </summary>
    /// <param name="tutorId">The ID of the tutor.</param>
    /// <returns>The tutor's profile.</returns>
    Task<ProfileDTO> GetTutorProfile(int tutorId);

    /// <summary>
    /// Updates the profile of a student.
    /// </summary>
    /// <param name="updateStudentProfileDTO">The data to update the student's profile.</param>
    Task UpdateStudentProfile(UpdateStudentProfileDTO updateStudentProfileDTO);

    /// <summary>
    /// Updates the profile of a tutor.
    /// </summary>
    /// <param name="updateProfileDTO">The data to update the tutor's profile.</param>
    Task UpdateTutorProfile(UpdateProfileDTO updateProfileDTO);
}
