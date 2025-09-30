namespace TodoLite.Models;

public class TodoItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Text { get; set; } = string.Empty;
    public string Status { get; set; } = "todo"; // todo | progress | done
    public string CreatorUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
