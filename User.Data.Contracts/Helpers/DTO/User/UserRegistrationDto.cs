﻿using Microsoft.AspNetCore.Http;

namespace User.Data.Access.Helpers.DTO.User;
public class UserRegistrationDto
{
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string RepeatPassword { get; set; }
    public IFormFile? UserCV { get; set; }
}
