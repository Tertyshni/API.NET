using System;

namespace p48_lessons_asp.DTO.User;

public class UserDto
{
    public Guid Id{get;set;} = Guid.NewGuid();
    public string? FirstName{get;set;}
    public string? LaststName{get;set;}
    public DateTime? BirthdayDate{get;set;}
    public string Login{get;set;} = string.Empty;
    public DateTime CreatedAt{get;set;} = DateTime.UtcNow;
}
