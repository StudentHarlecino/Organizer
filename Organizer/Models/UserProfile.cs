using System;
using System.Collections.Generic;

namespace Organizer.Models;

public partial class UserProfile
{
    public int Id { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public string? Email { get; set; }

    public string? AvatarPath { get; set; }

    public bool ReceiveMailNotifications { get; set; } = true;
}
