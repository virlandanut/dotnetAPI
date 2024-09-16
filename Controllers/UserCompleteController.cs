using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UserCompleteController : ControllerBase
{
    private readonly DataContextDapper _dapper;
    private readonly ReusableSql _reusableSql;

    public UserCompleteController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
        _reusableSql = new ReusableSql(config);
    }

    [HttpGet("GetUsers/{userId}/{isActive}")]
    public IEnumerable<UserComplete> GetUsers(int userId, bool isActive)
    {
        string sql = @"EXEC TutorialAppSchema.spUsers_Get";
        var parameters = new DynamicParameters();

        if (userId != 0)
        {
            sql += " @UserId = @Id";
            parameters.Add("@Id", userId, DbType.Int32);
        }

        if (isActive)
        {
            sql += userId != 0 ? ", @Active = @Status" : " @Active = @Status";
            parameters.Add("@Status", isActive, DbType.Boolean);
        }

        return _dapper.LoadData<UserComplete>(sql, parameters);
    }

    [HttpPut("UpsertUser")]
    public IActionResult Upsert(UserComplete user)
    {
        if (_reusableSql.Upsert(user))
        {
            return Ok();
        }
        throw new Exception("Failed to Upsert User");
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        string sql = @"EXEC TutorialAppSchema.spUser_Delete @UserId = @UserId";
        if (_dapper.ExecuteSql(sql, new { UserId = userId }))
        {
            return Ok();
        }

        return new ObjectResult("Failed to delete user") { StatusCode = 500 };
    }
}
