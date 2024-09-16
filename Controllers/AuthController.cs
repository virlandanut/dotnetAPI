using AutoMapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
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
    private readonly ReusableSql _reusableSql;
    private readonly IMapper _mapper;

    public AuthController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
        _authHelper = new AuthHelper(config);
        _reusableSql = new ReusableSql(config);
        _mapper = new Mapper(
            new MapperConfiguration(configuration =>
            {
                configuration.CreateMap<UserForRegistrationDto, UserComplete>();
            })
        );
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
                UserForLoginDto userForSetPassword = new UserForLoginDto()
                {
                    Email = userForRegistration.Email,
                    Password = userForRegistration.Password,
                };
                if (_authHelper.ResetPasswordRequest(userForSetPassword))
                {
                    UserComplete userComplete = _mapper.Map<UserComplete>(userForRegistration);
                    userComplete.Active = true;

                    if (_reusableSql.Upsert(userComplete))
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

    [HttpPut("ResetPassword")]
    public IActionResult ResetPassword(UserForLoginDto userForPasswordReset)
    {
        if (_authHelper.ResetPasswordRequest(userForPasswordReset))
        {
            return Ok();
        }

        throw new Exception("Failed to update password!");
    }

    [AllowAnonymous]
    [HttpPost("Login")]
    public IActionResult Login(UserForLoginDto userForLogin)
    {
        string sqlForHashAndSalt =
            @"TutorialAppSchema.spLoginConfirmation_Get
                                     @Email = @emailParam";

        UserForLoginConfirmationDto userForLoginConfirmation =
            _dapper.LoadDataSingle<UserForLoginConfirmationDto>(
                sqlForHashAndSalt,
                new { emailParam = userForLogin.Email }
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
