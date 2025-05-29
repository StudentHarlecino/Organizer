using System;
using System.Collections.Generic;

namespace Organizer.Models;

public partial class TaskCategory
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Color { get; set; }

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
