using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class PostController : ControllerBase
{
    private readonly DataContextDapper _dapper;

    public PostController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
    }

    [HttpGet("Posts/{postId}/{userId}/{searchParam}")]
    public IEnumerable<Post> Posts(int postId = 0, int userId = 0, string searchParam = "None")
    {
        string sql = @"EXEC TutorialAppSchema.spPosts_GET";
        string parameters = "";
        var sqlParameters = new DynamicParameters();

        if (postId != 0)
        {
            parameters += ", @PostId = @postId";
            sqlParameters.Add("postId", postId.ToString());
        }

        if (userId != 0)
        {
            parameters += ", @UserId = @userId";
            sqlParameters.Add("userId", userId.ToString());
        }

        if (searchParam != "None")
        {
            parameters += ", @SearchValue = @searchParam";
            sqlParameters.Add("searchParam", searchParam);
        }
        if (parameters.Length > 0)
        {
            sql += parameters.Substring(1);
        }

        return _dapper.LoadData<Post>(sql, sqlParameters);
    }

    [HttpGet("MyPosts")]
    public IEnumerable<Post> GetMyPosts()
    {
        string sql = @"EXEC TutorialAppSchema.spPosts_GET @UserId = @userId";

        return _dapper.LoadData<Post>(sql, new { userId = User.FindFirst("userId")?.Value });
    }

    [HttpPut("UpsertPost")]
    public IActionResult UpsertPost(Post postToUpsert)
    {
        string sql =
            @"EXEC TutorialAppSchema.spPosts_Upsert 
                     @UserId = @userId, @PostTitle = @postTitle, @PostContent = @postContent";

        var parameters = new DynamicParameters();
        parameters.Add("userId", User.FindFirst("userId")?.Value);
        parameters.Add("postTitle", postToUpsert.PostTitle);
        parameters.Add("postContent", postToUpsert.PostContent);

        if (postToUpsert.PostId > 0)
        {
            sql += ", @PostId = @postId";
            parameters.Add("postId", postToUpsert.PostId);
        }

        if (_dapper.ExecuteSql(sql, parameters))
        {
            return Ok();
        }

        return StatusCode(500, "Failed to Upsert Post");
    }

    [HttpDelete("Post/{postId}")]
    public IActionResult DeletePost(int postId)
    {
        string sql = @"EXEC TutorialAppSchema.spPost_Delete @PostId = @postId, @UserId = @userId";
        if (
            _dapper.ExecuteSql(
                sql,
                new { postId = postId.ToString(), userId = User.FindFirst("userId")?.Value }
            )
        )
        {
            return Ok();
        }

        return StatusCode(500, "Failed to Delete Post");
    }
}
