using System;
using System.Collections.Generic;

namespace Organizer.Models;

public partial class TaskHistory
{
    public int HistoryId { get; set; }

    public int TaskId { get; set; }

    public DateTime? ChangedAt { get; set; }

    public string ChangeDescription { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
