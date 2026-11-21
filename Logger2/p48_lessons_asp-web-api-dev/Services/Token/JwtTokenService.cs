using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using p48_lessons_asp.Data;
using p48_lessons_asp.Data.Entity;
using p48_lessons_asp.Data.Enum;
using p48_lessons_asp.Options;

namespace p48_lessons_asp.Services.Token;

public class JwtTokenService : ITokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly DataContext _dataContext;

    public JwtTokenService(IOptions<JwtOptions> options, DataContext dataContext)
    {
        _jwtOptions = options.Value;
        _dataContext = dataContext;
    }
    public async Task<TokenPair> CreateSessionAsync(User user)
    {
        var accessExpires = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes);
        var refreshExpires = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Login),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var accessToken = GenerateAccessToken(claims, accessExpires);
        var refreshToken = GenerateRefreshToken();

        var accessEntity = new JwtToken
        {
            Id = Guid.NewGuid(),
            Token = accessToken,
            Type = TokenType.Access,
            ExpirationDate = accessExpires
        };

        var refreshEntity = new JwtToken
        {
            Id = Guid.NewGuid(),
            Token = refreshToken,
            Type = TokenType.Refresh,
            ParentTokenId = accessEntity.Id,
            ExpirationDate = refreshExpires
        };

        _dataContext.Tokens.AddRange(accessEntity, refreshEntity);
        _dataContext.TokenJournals.AddRange(
            new TokenJournal
            {
                Id = Guid.NewGuid(),
                TokenId = accessEntity.Id,
                UserId = user.Id,
                ActivatedAt = DateTime.UtcNow
            },
            new TokenJournal
            {
                Id = Guid.NewGuid(),
                TokenId = refreshEntity.Id,
                UserId = user.Id,
                ActivatedAt = DateTime.UtcNow
            }
        );
        await _dataContext.SaveChangesAsync();

        return new TokenPair(accessToken, refreshToken);

    }

    public async Task<TokenPair?> RefreshSessionAsync(string refreshToken)
    {
        var journal = await _dataContext.TokenJournals
            .Include(tj => tj.Token)
            .Include(tj => tj.User)
            .FirstOrDefaultAsync(tj =>
                tj.Token!.Token == refreshToken &&
                tj.Token.Type == TokenType.Refresh);

        if (journal == null || journal.DeactivatedAt != null || journal.Token!.ExpirationDate <= DateTime.UtcNow) return null;

        if (journal.User == null || journal.User.DeletedAt != null) return null;

        journal.DeactivatedAt = DateTime.UtcNow;

        if (journal.Token.ParentTokenId.HasValue)
        {
            var oldAccess = await _dataContext.TokenJournals
                .FirstOrDefaultAsync(tj => tj.TokenId == journal.Token.ParentTokenId.Value && tj.DeactivatedAt == null);

            if (oldAccess != null)
                oldAccess.DeactivatedAt = DateTime.UtcNow;
        }

        return await CreateSessionAsync(journal.User);
    }

    public async Task DeactivateSessionAsync(string accessToken)
    {
        var accessJournal = await _dataContext.TokenJournals
            .Include(tj => tj.Token)
            .FirstOrDefaultAsync(tj => tj.Token!.Token == accessToken && tj.DeactivatedAt == null);

        if (accessJournal == null) return;

        accessJournal.DeactivatedAt = DateTime.UtcNow;

        var refreshJournal = await _dataContext.TokenJournals
            .Include(tj => tj.Token)
            .FirstOrDefaultAsync(tj => tj.Token!.ParentTokenId == accessJournal.TokenId && tj.DeactivatedAt == null);
        if (refreshJournal != null) refreshJournal.DeactivatedAt = DateTime.UtcNow;

        await _dataContext.SaveChangesAsync();

    }

    private string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    private string GenerateAccessToken(IEnumerable<Claim> claims, DateTime expires)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
