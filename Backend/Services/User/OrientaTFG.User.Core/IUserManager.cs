using OrientaTFG.User.Core.DTOs;

namespace OrientaTFG.User.Core;

public interface IUserManager
{
    /// <summary>
    /// Login method for all users
    /// </summary>
    /// <param name="logInDTO">The user's email and password</param>
    /// <returns>Token if the login was successful, error message otherwise</returns>
    Task<LogInResponseDTO> LogIn(LogInDTO logInDTO);

    /// <summary>
    /// Gets the students list
    /// </summary>
    /// <returns>The students list</returns>
    Task<List<StudentDTO>> GetStudents();

    /// <summary>
    /// Gets the tutors list
    /// </summary>
    /// <returns>The tutors list</returns>
   Task<List<TutorDTO>> GetTutors();
}
