using DotnetAPI.Data;
using DotnetAPI.Dtos;
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

    [HttpGet("Posts")]
    public IEnumerable<Post> Posts()
    {
        string sql =
            @"SELECT [PostId], [UserId], [PostTitle], [PostContent], [PostCreated], [PostUpdated]
              FROM TutorialAppSchema.Posts";

        return _dapper.LoadData<Post>(sql);
    }

    [HttpGet("PostSingle/{postId}")]
    public Post GetPostSingle(int postId)
    {
        string sql =
            @"SELECT [PostId], [UserId], [PostTitle], [PostContent], [PostCreated], [PostUpdated]
              FROM TutorialAppSchema.Posts
              WHERE [PostId] = @PostId";
        return _dapper.LoadDataSingle<Post>(sql, new { postId });
    }

    [HttpGet("PostByUser/{userId}")]
    public IEnumerable<Post> GetPostsByUser(int userId)
    {
        string sql =
            @"SELECT [PostId], [UserId], [PostTitle], [PostContent], [PostCreated], [PostUpdated]
              FROM TutorialAppSchema.Posts
              WHERE [UserId] = @UserId";

        return _dapper.LoadData<Post>(sql, new { userId });
    }

    [HttpGet("MyPosts")]
    public IEnumerable<Post> GetMyPosts()
    {
        string sql =
            @"SELECT [PostId], [UserId], [PostTitle], [PostContent], [PostCreated], [PostUpdated]
              FROM TutorialAppSchema.Posts
              WHERE [UserId] = @UserId";

        return _dapper.LoadData<Post>(sql, new { UserId = User.FindFirst("userId")?.Value });
    }

    [HttpPost("Post")]
    public IActionResult AddPost(PostToAddDto postToAdd)
    {
        string sql =
            @"INSERT INTO TutorialAppSchema.Posts([UserId], [PostTitle], [PostContent], [PostCreated], [PostUpdated])
                       VALUES(@UserId, @PostTitle, @PostContent, GETDATE(), GETDATE())";
        if (
            _dapper.ExecuteSql(
                sql,
                new
                {
                    UserId = User.FindFirst("userId")?.Value,
                    PostTitle = postToAdd.PostTile,
                    PostContent = postToAdd.PostContent,
                }
            )
        )
        {
            return Ok();
        }

        return StatusCode(500, "Failed to Add Post");
    }

    [HttpPut("Post")]
    public IActionResult EditPost(PostToEditDto postToEdit)
    {
        string sql =
            @"UPDATE TutorialAppSchema.Posts
              SET [PostTitle] = @PostTitle, [PostContent] = @PostContent, [PostUpdated] = GETDATE()
              WHERE [PostId] = @PostId AND [UserId] = @UserId";
        if (
            _dapper.ExecuteSql(
                sql,
                new
                {
                    PostTitle = postToEdit.PostTile,
                    PostContent = postToEdit.PostContent,
                    PostId = postToEdit.PostId.ToString(),
                    UserId = User.FindFirst("userId")?.Value,
                }
            )
        )
        {
            return Ok();
        }

        return StatusCode(500, "Failed to Update Post");
    }

    [HttpDelete("Post/{postId}")]
    public IActionResult DeletePost(int postId)
    {
        string sql =
            @"DELETE FROM TutorialAppSchema.Posts
                       WHERE [PostId] = @PostId AND [UserId] = @UserId";
        if (
            _dapper.ExecuteSql(
                sql,
                new { PostId = postId.ToString(), UserId = User.FindFirst("userId")?.Value }
            )
        )
        {
            return Ok();
        }

        return StatusCode(500, "Failed to Delete Post");
    }

    [HttpGet("PostsBySearch/{searchParam}")]
    public IEnumerable<Post> PostBySearch(string searchParam)
    {
        string sql =
            @"SELECT [PostId], [UserId], [PostTitle], [PostContent], [PostCreated], [PostUpdated]
                       FROM TutorialAppSchema.Posts
                       WHERE [PostTitle] LIKE '%@SearchParam%' OR [PostContent] LIKE '%@SearchParam%'";
        IEnumerable<Post> posts = _dapper.LoadData<Post>(sql, new { SearchParam = searchParam });

        return posts;
    }
}
