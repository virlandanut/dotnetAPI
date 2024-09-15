using System.Security.Cryptography;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly DataContextDapper _dapper;
    private readonly AuthHelper _authHelper;

    public AuthController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
        _authHelper = new AuthHelper(config);
    }

    [AllowAnonymous]
    [HttpPost("Register")]
    public IActionResult Register(UserForRegistrationDto userForRegistration)
    {
        if (userForRegistration.Password == userForRegistration.PasswordConfirm)
        {
            string sqlCheckUserExists =
                "SELECT Email FROM TutorialAppSchema.Auth WHERE Email = @Email";
            IEnumerable<string> existingUsers = _dapper.LoadData<string>(
                sqlCheckUserExists,
                new { userForRegistration.Email }
            );
            if (existingUsers.Count() == 0)
            {
                byte[] passwordSalt = new byte[128 / 8];
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    rng.GetNonZeroBytes(passwordSalt);
                }

                byte[] passwordHash = _authHelper.GetPasswordHash(
                    userForRegistration.Password,
                    passwordSalt
                );

                string sqlAddAuth =
                    @"INSERT INTO TutorialAppSchema.Auth ([Email], [PasswordHash], [PasswordSalt])
                                      VALUES(@Email, @PasswordHash, @PasswordSalt)";

                if (
                    _dapper.ExecuteSql(
                        sqlAddAuth,
                        new
                        {
                            userForRegistration.Email,
                            passwordHash,
                            passwordSalt,
                        }
                    )
                )
                {
                    string sqlAddUser =
                        @"INSERT INTO TutorialAppSchema.Users([FirstName], [LastName], [Email], [Gender], [Active])
                       VALUES(@FirstName, @LastName, @Email, @Gender, 1)";
                    if (
                        _dapper.ExecuteSql(
                            sqlAddUser,
                            new
                            {
                                userForRegistration.FirstName,
                                userForRegistration.LastName,
                                userForRegistration.Email,
                                userForRegistration.Gender,
                            }
                        )
                    )
                    {
                        return Ok();
                    }
                    throw new Exception("Failed to Add User");
                }

                throw new Exception("Failed to Register User");
            }
            throw new Exception("User with this email already exists!");
        }
        throw new Exception("Passwords do not match!");
    }

    [AllowAnonymous]
    [HttpPost("Login")]
    public IActionResult Login(UserForLoginDto userForLogin)
    {
        string sqlForHashAndSalt =
            @"SELECT [PasswordHash], [PasswordSalt] 
                                     FROM TutorialAppSchema.Auth WHERE Email = @Email";
        UserForLoginConfirmationDto userForLoginConfirmation =
            _dapper.LoadDataSingle<UserForLoginConfirmationDto>(
                sqlForHashAndSalt,
                new { userForLogin.Email }
            );

        byte[] passwordHash = _authHelper.GetPasswordHash(
            userForLogin.Password,
            userForLoginConfirmation.PasswordSalt
        );

        if (passwordHash.Length != userForLoginConfirmation.PasswordHash.Length)
        {
            return StatusCode(401, "Incorrect Password");
        }

        for (int index = 0; index < passwordHash.Length; index++)
        {
            if (passwordHash[index] != userForLoginConfirmation.PasswordHash[index])
            {
                return StatusCode(401, "Incorrect Password");
            }
        }

        string userIdSql = "SELECT [UserId] FROM TutorialAppSchema.Users WHERE Email = @Email";

        int userId = _dapper.LoadDataSingle<int>(userIdSql, new { userForLogin.Email });

        return Ok(new Dictionary<string, string> { { "token", _authHelper.CreateToken(userId) } });
    }

    [HttpGet("RefreshToken")]
    public IActionResult RefreshToken()
    {
        string userId = User.FindFirst("userId")?.Value + "";

        string userIdSql = "SELECT [UserId] FROM TutorialAppSchema.Users WHERE UserId = @UserId";

        int userIdDatabase = _dapper.LoadDataSingle<int>(userIdSql, new { userId });

        return Ok(
            new Dictionary<string, string> { { "token", _authHelper.CreateToken(userIdDatabase) } }
        );
    }
}
