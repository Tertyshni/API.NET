using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using p48_lessons_asp.Data;
using p48_lessons_asp.Extensions;
using p48_lessons_asp.Models.AuthRequestModels;
using p48_lessons_asp.Services.Kdf;
using p48_lessons_asp.Services.Token;

namespace p48_lessons_asp.Controllers
{

    public class RefreshRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshRequestValidator : AbstractValidator<RefreshRequest>
    {
        public RefreshRequestValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("RefreshToken is required");
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _db;
        private readonly ITokenService _tokenService;
        private readonly IKdfService _kdfService;
        private readonly ILogger<UserController> _logger;
        private readonly IValidator<LoginRequest> _loginValidator;
        private readonly IValidator<RefreshRequest> _refreshValidator;

        public AuthController(DataContext db, ITokenService tokenService, IKdfService kdfService, ILogger<UserController> logger, IValidator<LoginRequest> loginValidator, IValidator<RefreshRequest> refreshValidator)
        {
            _db = db;
            _tokenService = tokenService;
            _kdfService = kdfService;
            _logger = logger;
            _loginValidator = loginValidator;
            _refreshValidator = refreshValidator;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (model == null) return BadRequest(new { StatusCode = 400, Message = "Login data is required" });

            var validationResult = await _loginValidator.GetErrorsAsync(model);
            if (validationResult != null)
            {
                return new ObjectResult(new
                {
                    StatusCode = 400,
                    Message = "Validation error",
                    Errors = validationResult
                })
                { StatusCode = 400 };
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Login == model.Login && u.DeletedAt == null);
            if (user == null)
            {
                return new ObjectResult(new
                {
                    StatusCode = 404,
                    Message = "User not found",
                })
                { StatusCode = 404 };
            }

            if (user.PasswordHash != _kdfService.GetDerivedKey(model.Password, user.PasswordSalt))
            {
                return new ObjectResult(new
                {
                    StatusCode = 400,
                    Message = "Password wrong",
                })
                { StatusCode = 400 };
            }

            try
            {
                var tokenPairData = await _tokenService.CreateSessionAsync(user);
                return new ObjectResult(new
                {
                    StatusCode = 200,
                    Message = "Ok",
                    Data = new
                    {
                        User = new
                        {
                            user.Id,
                            user.Login,
                            user.Email,
                            user.FirstName,
                            user.LaststName,
                            user.BirthdayDate,
                            user.CreatedAt
                        },
                        Tokens = tokenPairData
                    }
                })
                { StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token service is fault");
                return new ObjectResult(new
                {
                    StatusCode = 503,
                    Message = "Service is unavaliable",
                })
                { StatusCode = 503 };
            }
        }

        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshRequest model)
        {
            if (model == null)
                return BadRequest(new { StatusCode = 400, Message = "Refresh token data is required" });

            var validationResult = await _refreshValidator.GetErrorsAsync(model);
            if (validationResult != null)
                return BadRequest(new { StatusCode = 400, Message = "Validation error", Errors = validationResult });

            try
            {
                var tokens = await _tokenService.RefreshSessionAsync(model.RefreshToken);
                if (tokens == null)
                {
                    return new ObjectResult(new
                    {
                        StatusCode = 401,
                        Message = "Refresh token rejected or invalid",
                    })
                    { StatusCode = 401 };
                }

                return new ObjectResult(new
                {
                    StatusCode = 200,
                    Message = "Ok",
                    Data = tokens
                })
                { StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token service is fault");
                return new ObjectResult(new
                {
                    StatusCode = 503,
                    Message = "Service is unavaliable",
                })
                { StatusCode = 503 };
            }

        }

        [Authorize]
        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var accessToken = await HttpContext.GetTokenAsync("access_token");
                if (!string.IsNullOrEmpty(accessToken))
                {
                    await _tokenService.DeactivateSessionAsync(accessToken);
                }

                return new ObjectResult(new
                {
                    StatusCode = 200,
                    Message = "Session closed",
                })
                { StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token service is fault");
                return new ObjectResult(new
                {
                    StatusCode = 503,
                    Message = "Service is unavaliable",
                })
                { StatusCode = 503 };
            }
        }

    }
}
