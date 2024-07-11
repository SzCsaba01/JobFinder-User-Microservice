﻿namespace User.Data.Access.Helpers.DTO.User;
public class FilteredUserDto
{
    public string Username { get; set; }
    public string Email { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? Education { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? Experience { get; set; }
    public ICollection<string>? Skills { get; set; }
}
