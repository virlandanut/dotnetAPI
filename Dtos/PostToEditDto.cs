namespace DotnetAPI.Dtos;

public partial class PostToEditDto
{
    public int PostId { get; set; }
    public string PostTile { get; set; } = "";
    public string PostContent { get; set; } = "";
}
