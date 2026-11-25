using System;
using System.ComponentModel.DataAnnotations;

namespace p48_lessons_asp.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required(ErrorMessage = "No set Jwt:Issuer")]
    public string Issuer {get;init;} = String.Empty;

    [Required(ErrorMessage = "No set Jwt:Audience")]
    public string Audience {get;init;} = String.Empty;

    [Required(ErrorMessage = "No set Jwt:Key")]
    [MinLength(32, ErrorMessage = "Jwt:Key must have 32 symbols minimum for HmacSha256")]
    public string Key {get;init;} = String.Empty;

    [Range(1, 1440, ErrorMessage = "Lifetime for AccessToken must be in range 1 to 1440 minutes")]
    public int AccessTokenExpirationMinutes{get;init;} = 15;

    [Range(1, 365, ErrorMessage = "Lifetime for RefreshToken must be in range 1 to 365 days")]
    public int RefreshTokenExpirationDays{get;init;} = 7;
    
}
