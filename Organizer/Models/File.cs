using System;
using System.Collections.Generic;

namespace Organizer.Models;

public partial class File
{
    public int FileId { get; set; }

    public string OriginalName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public long FileSize { get; set; }

    public string? MimeType { get; set; }

    public DateTime? UploadedAt { get; set; }

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
