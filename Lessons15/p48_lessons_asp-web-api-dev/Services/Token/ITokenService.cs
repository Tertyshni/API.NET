using System;
using System.Security.Claims;
using p48_lessons_asp.Data.Entity;

namespace p48_lessons_asp.Services.Token;

public record TokenPair(string AccessToken, string RefreshToken);

public interface ITokenService
{
    Task<TokenPair> CreateSessionAsync(User user);
    Task<TokenPair?> RefreshSessionAsync(string refreshToken);
    Task DeactivateSessionAsync(string accessToken);
}
