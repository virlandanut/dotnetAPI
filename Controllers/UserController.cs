using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    DataContextDapper _dapper;

    public UserController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
    }

    [HttpGet("GetUsers")]
    public IEnumerable<User> GetUsers()
    {
        string sql =
            @"SELECT [UserId], [FirstName], [LastName], [Email], [Gender], [Active] 
                       FROM TutorialAppSchema.Users";
        IEnumerable<User> users = _dapper.LoadData<User>(sql);
        return users;
    }

    [HttpGet("GetSingleUser/{userId}")]
    public User GetSingleUser(int userId)
    {
        string sql =
            @"SELECT [UserId], [FirstName], [LastName], [Email], [Gender], [Active] 
                       FROM TutorialAppSchema.Users 
                       WHERE UserId = @UserId";
        User? user = _dapper.LoadDataSingle<User>(sql, new { UserId = userId.ToString() });
        if (user != null)
        {
            return user;
        }

        throw new Exception("User does not exist!");
    }

    [HttpPut("EditUser")]
    public IActionResult EditUser(User user)
    {
        string sql =
            @"UPDATE TutorialAppSchema.Users
                       SET [FirstName] = @FirstName, [LastName] = @LastName, [Email] = @Email, [Gender] = @Gender, [Active] = @Active
                       WHERE UserId = @UserId";
        if (
            _dapper.ExecuteSql(
                sql,
                new
                {
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.Gender,
                    Active = user.Active.ToString(),
                    user.UserId,
                }
            )
        )
        {
            return Ok();
        }
        throw new Exception("Failed to Update User");
    }

    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDto user)
    {
        string sql =
            @"INSERT INTO TutorialAppSchema.Users([FirstName], [LastName], [Email], [Gender], [Active])
                       VALUES(@FirstName, @LastName, @Email, @Gender, @Active)";
        if (
            _dapper.ExecuteSql(
                sql,
                new
                {
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.Gender,
                    Active = user.Active.ToString(),
                }
            )
        )
        {
            return Ok();
        }

        throw new Exception("Failed to Add User");
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        string sql = @"DELETE FROM TutorialAppSchema.Users WHERE UserId = @UserId";
        if (_dapper.ExecuteSql(sql, new { UserId = userId }))
        {
            return Ok();
        }

        return new ObjectResult("Failed to delete user") { StatusCode = 500 };
    }

    [HttpGet("GetUserSalaries")]
    public IEnumerable<UserSalary> GetUserSalaries()
    {
        string sql =
            @"SELECT [UserId], [Salary]
                       FROM TutorialAppSchema.UserSalary";
        IEnumerable<UserSalary> usersSalaries = _dapper.LoadData<UserSalary>(sql);
        return usersSalaries;
    }

    [HttpGet("GetSingleUserSalary/{userId}")]
    public UserSalary GetSingleUserSalary(int userId)
    {
        string sql =
            @"SELECT [UserId], [Salary]
                       FROM TutorialAppSchema.UserSalary
                       WHERE UserId = @UserId";
        UserSalary? userSalary = _dapper.LoadDataSingle<UserSalary>(sql, new { UserId = userId });
        if (userSalary != null)
        {
            return userSalary;
        }

        throw new Exception("User salary does not exist!");
    }

    [HttpPut("EditUserSalary")]
    public IActionResult EditUserSalary(UserSalary userSalary)
    {
        string sql =
            @"UPDATE TutorialAppSchema.UserSalary
                       SET [Salary] = @Salary
                       WHERE UserId = @UserId";
        if (_dapper.ExecuteSql(sql, new { userSalary.Salary, userSalary.UserId }))
        {
            return Ok();
        }

        throw new Exception("Failed to Update User Salary");
    }

    [HttpPost("AddUserSalary")]
    public IActionResult AddUserSalary(UserSalary userSalary)
    {
        string sql =
            @"INSERT INTO TutorialAppSchema.UserSalary([UserId],[Salary])
                       VALUES(@UserId, @Salary)";
        if (_dapper.ExecuteSql(sql, new { userSalary.UserId, userSalary.Salary }))
        {
            return Ok();
        }

        throw new Exception("Failed to Add User Salary");
    }

    [HttpDelete("RemoveUserSalary/{userId}")]
    public IActionResult RemoveUserSalary(int userId)
    {
        string sql = @"DELETE FROM TutorialAppSchema.UserSalary WHERE UserId = @UserId";
        if (_dapper.ExecuteSql(sql, new { UserId = userId }))
        {
            return Ok();
        }

        return StatusCode(500, "Failed to delete User Salary");
    }

    [HttpGet("GetUsersJobsInfo")]
    public IEnumerable<UserJobInfo> GetUsersJobsInfo()
    {
        string sql =
            @"SELECT [UserId], [JobTitle], [Department]
                       FROM TutorialAppSchema.UserJobInfo";
        IEnumerable<UserJobInfo> usersJobsInfo = _dapper.LoadData<UserJobInfo>(sql);

        return usersJobsInfo;
    }

    [HttpGet("GetSingleUserJobInfo/{userId}")]
    public UserJobInfo GetSingleUserJobInfo(int userId)
    {
        string sql =
            @"SELECT [UserId], [JobTitle], [Department]
                       FROM TutorialAppSchema.UserJobInfo
                       WHERE UserId = @UserId";
        UserJobInfo? userJobInfo = _dapper.LoadDataSingle<UserJobInfo>(
            sql,
            new { UserId = userId }
        );
        if (userJobInfo != null)
        {
            return userJobInfo;
        }

        throw new Exception("Failed to load User Job Info");
    }

    [HttpPut("EditUserJobInfo")]
    public IActionResult EditUserJobInfo(UserJobInfo userJobInfo)
    {
        string sql =
            @"UPDATE TutorialAppSchema.UserJobInfo
                       SET [JobTitle] = @JobTitle, [Department] = @Department
                       WHERE [UserId] = @UserId";
        if (
            _dapper.ExecuteSql(
                sql,
                new
                {
                    userJobInfo.JobTitle,
                    userJobInfo.Department,
                    userJobInfo.UserId,
                }
            )
        )
        {
            return Ok();
        }

        throw new Exception("Failed to Get User Job Info");
    }

    [HttpPost("AddUserJobInfo")]
    public IActionResult AddUserJobInfo(UserJobInfo userJobInfo)
    {
        string sql =
            @"INSERT INTO TutorialAppSchema.UserJobInfo([UserId], [JobTitle], [Department])
                       VALUES(@UserId, @JobTitle, @Department)";
        if (
            _dapper.ExecuteSql(
                sql,
                new
                {
                    userJobInfo.UserId,
                    userJobInfo.JobTitle,
                    userJobInfo.Department,
                }
            )
        )
        {
            return Ok();
        }

        throw new Exception("Failed to Add User Job Info");
    }

    [HttpDelete("RemoveUserJobInfo/{userId}")]
    public IActionResult RemoveUserJobInfo(int userId)
    {
        string sql = "DELETE FROM TutorialAppSchema.UserJobInfo WHERE UserId = @userId";
        if (_dapper.ExecuteSql(sql, new { userId }))
        {
            return Ok();
        }

        throw new Exception("Failed to Delete User Job Info");
    }
}
