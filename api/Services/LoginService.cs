using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using weatherReport.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace weatherReport.Services
{
    public interface ILoginService
    {
        public string AuthenticateUser(User login);
        public string ValidateToken(string token);
    }

    public class LoginService:ILoginService
    {
        private IConfiguration _config;

        public LoginService() { }

        public LoginService(IConfiguration config)
        {
            _config = config;
        }

        //User Authentication
        public string AuthenticateUser(User login)
        {
            var user = ValidateUser(login);

            if (user!= null)
            {
                var tokenString = GenerateJSONWebToken(login.UserName);
                return tokenString;
            }
            return null;

        }

        //Generate access token
        private string GenerateJSONWebToken(string userName)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
                        { 
                            new Claim(JwtRegisteredClaimNames.UniqueName,userName) 
                        };
            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddMinutes(4000),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //Validating Access Token
        public string ValidateToken(string token)
        {
            string username = null;
            ClaimsPrincipal principal = new LoginService(_config).GetPrincipal(token);
            if (principal == null)
                return null;
            ClaimsIdentity identity = null;
            try
            {
                identity = (ClaimsIdentity)principal.Identity;
            }
            catch (NullReferenceException)
            {
                return null;
            }

            username = identity.Name;
            return username;
        }

        //Retriving Claims Principal
        private ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
                if (jwtToken == null)
                    return null;
                
                TokenValidationParameters parameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]))
                };
                SecurityToken securityToken;
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token,
                      parameters, out securityToken);
                return principal;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private User ValidateUser(User login)
        {
            User user = null;

            //Validate the User Credentials  
            //Demo Purpose, I have Passed HardCoded User Information  
            if (login.UserName == "kirang")
            {
                user = new User { UserName = "kirang" };
            }
            return user;
        }
    }
}
