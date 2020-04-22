using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using weatherReport.Models;
using weatherReport.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;

namespace api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IConfiguration _config;
        private readonly ILogger<LoginController> _logger;

        public LoginController(ILogger<LoginController> logger,IConfiguration config)
        {
            _config = config;
            _logger = logger;
        }

        // https://localhost:44395/login
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody]User login)
        {
            IActionResult response = Unauthorized();
            var loginService = new LoginService(_config);
            var tokenString =loginService.AuthenticateUser(login);

            if (tokenString != null)
            {
                _logger.LogInformation("User token created successfully");
               return Ok(new { token = tokenString });
            }
            _logger.LogError("Unauthorized user detected");
            return Unauthorized(new { StatusCode = 401 , message = "Invalid user"}) ; 
        }

    }
}