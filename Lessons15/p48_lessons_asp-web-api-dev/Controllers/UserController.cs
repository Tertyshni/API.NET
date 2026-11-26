using System.Security.Claims;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using p48_lessons_asp.Data;
using p48_lessons_asp.DTO.User;
using p48_lessons_asp.Extensions;
using p48_lessons_asp.Models.UserRequestModels;
using p48_lessons_asp.Services.Email;
using p48_lessons_asp.Services.Kdf;
using p48_lessons_asp.Services.Random;
using p48_lessons_asp.Services.Token;

namespace p48_lessons_asp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DataContext _db;
        private readonly IValidator<CreateUserRequest> _userValidator;
        private readonly IKdfService _kdfService;
        private readonly IRandomService _randomService;
        private readonly ILogger<UserController> _logger;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        public UserController(DataContext db, 
            IValidator<CreateUserRequest> userValidator, 
            IRandomService randomService, 
            IKdfService kdfService, 
            ILogger<UserController> logger, 
            IMapper mapper, 
            IEmailService emailService,
            ITokenService tokenService)
        {
            _db = db;
            _userValidator = userValidator;
            _randomService = randomService;
            _kdfService = kdfService;
            _logger = logger;
            _mapper = mapper;
            _emailService = emailService;
            _tokenService = tokenService;
        }

        [Authorize]
        [HttpGet]
        [Route("profile")]
        public async Task<IActionResult> GetCurrentProfile()
        {
            return Ok(HttpContext.User.GetUserId() + " Role: "+ HttpContext.User.GetUserRole());
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewUser([FromBody] CreateUserRequest model)
        {
            if (model == null)
                return BadRequest(new { StatusCode = 400, Message = "User data is required" });

            var validationResult = await _userValidator.GetErrorsAsync(model);
            if (validationResult != null)
            {
                return new ObjectResult(new
                {
                    StatusCode = 400,
                    Message="Validation error",
                    Errors = validationResult
                }){StatusCode = 400};
            }
            var salt = _randomService.RandomString(10);
            Data.Entity.User newUser = new()
            {
                Login = model.Login,
                PasswordHash = _kdfService.GetDerivedKey(model.Password, salt),
                PasswordSalt = salt,
                Email = model.Email,
                EmailVerificationCode = _randomService.ConfirmCode(6),
                EmailVerificationCodeExpires = DateTime.UtcNow.AddMinutes(10)
            };

            try
            {
                _db.Users.Add(newUser);
                await _db.SaveChangesAsync();

                var tokens = await _tokenService.CreateSessionAsync(newUser);

                try
                {
                    await _emailService.SendEmailAsync(model.Email, "Welcome to our API",
                        $"Your verification code: {newUser.EmailVerificationCode}");
                }
                catch (Exception emailException)
                {
                    _logger.LogWarning(emailException, "User created, but verification email could not be sent");
                }

                _logger.LogInformation("Successfully created new User with ID: {userId}", newUser.Id);
                return new ObjectResult(new
                {
                    StatusCode = 201,
                    Message = "User created",
                    Data = new
                    {
                        User = _mapper.Map<UserDto>(newUser),
                        Tokens = tokens
                    }
                }) { StatusCode = 201 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not complete user registration");
                return new ObjectResult(new
                {
                    StatusCode = 503,
                    Message="Service unavaliable",
                    data = (object) null!
                }){StatusCode = StatusCodes.Status503ServiceUnavailable};
            }
        }

    }
}
