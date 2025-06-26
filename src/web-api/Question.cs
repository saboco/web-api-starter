namespace web_api;

public class Question()
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public long UserId { get; set; }
    public long Votes { get; set; }
}