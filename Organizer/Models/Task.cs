using System;
using System.Collections.Generic;

namespace Organizer.Models;

public partial class Task
{
    public int TaskId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int Priority { get; set; }

    public bool Completed { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CategoryId { get; set; }

    public DateOnly? DeadlineDate { get; set; }

    public virtual TaskCategory? Category { get; set; }

    public virtual ICollection<File> Files { get; set; } = new List<File>();

    public DateTime? LastNotificationSent { get; set; }
}
