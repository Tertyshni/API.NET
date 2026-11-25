using System.Security.Claims;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using p48_lessons_asp.Data;
using p48_lessons_asp.DTO.User;
using p48_lessons_asp.Extensions;
using p48_lessons_asp.Models.UserRequestModels;
using p48_lessons_asp.Services.Kdf;
using p48_lessons_asp.Services.Random;

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
        public UserController(DataContext db, IValidator<CreateUserRequest> userValidator, IRandomService randomService, IKdfService kdfService, ILogger<UserController> logger, IMapper mapper)
        {
            _db = db;
            _userValidator = userValidator;
            _randomService = randomService;
            _kdfService = kdfService;
            _logger = logger;
            _mapper = mapper;
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
                PasswordSalt = salt
            };

            try
            {
                _db.Users.Add(newUser);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Successfully created new User with ID: {userId}", newUser.Id);
                return new ObjectResult(new
                {
                    StatusCode = 201,
                    Message="User created",
                    Data = _mapper.Map<UserDto>(newUser)
                }){StatusCode = 201};
            }
            catch(OperationCanceledException ex)
            {
                _logger.LogError(ex, "New user not saved. Database trouble");
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
