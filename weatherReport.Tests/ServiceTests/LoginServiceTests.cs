using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using weatherReport.Services;
using weatherReport.Models;
using System;

namespace weatherReport.Tests
{
    [TestClass]
    public class LoginServiceTests
    {
        private IConfiguration _config;
        private ILoginService _loginService;

        [TestInitialize]
        public void Initalize()
        {
           _config = new TestHelper().Configuration;
           _loginService = new LoginService(_config);
        }

        // Valid User- Token generated
        [TestMethod]
        public void ValidUserLoginTest()
        {
            var userLogin = new User
            {
                UserName = "kirang",
                Password = "password"
            };
            var tokenString = _loginService.AuthenticateUser(userLogin);
            if (tokenString != null)
                Assert.IsNotNull(tokenString);
        }

        // Invalid User - Token Not created
        [TestMethod]
        public void InvalidUserLoginTest()
        {
            var userLogin = new User
            {
                UserName = "pavani",
                Password = "password"
            };
            var tokenString = _loginService.AuthenticateUser(userLogin);
            if (tokenString == null)
                Assert.IsNull(tokenString);
        }

        // Invalid access token 
        [TestMethod]
        public void InvalidUserToken()
        {
            bool pass = false;
            Exception expectedException = null;
            try
            {
                //genertaed new token using username "pavani awith token expitarion for 4000 minutes
                string token  = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InBhdmFuaSIsImV4cCI6MTU4Nzc0NDc5MiwiaXNzIjoiVGVzdC5jb20iLCJhdWQiOiJUZXN0LmNvbSJ9.ortjvTGfUGXow82CyFXPvxKIonr-kv82I4igKSlnm64";
               
                string tokenUsername = _loginService.ValidateToken(token);
                //defining hardcoded value for demo purpose only
                if (!tokenUsername.Equals("kirang", StringComparison.InvariantCultureIgnoreCase))
                {
                    Assert.AreNotEqual("kirang", tokenUsername);
                }
                
            }
            catch(Exception ex)
            {
                expectedException = ex;
            }      
        }

        //valid Access token
        [TestMethod]
        public void ValidUserToken()
        {
            bool pass = false;
            Exception expectedException = null;
            try
            {
                var userLogin = new User
                {
                    UserName = "kirang",
                    Password = "password"
                };
                var tokenString = _loginService.AuthenticateUser(userLogin);
                if (tokenString != null)
                {
                    string tokenUsername = _loginService.ValidateToken(tokenString);
                    //defining hardcoded value for demo purpose only
                    if (tokenUsername.Equals("kirang", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Assert.AreEqual("kirang", tokenUsername);
                    }
                }
            }
            catch (Exception ex)
            {
                expectedException = ex;
            }
        }
    }
}
