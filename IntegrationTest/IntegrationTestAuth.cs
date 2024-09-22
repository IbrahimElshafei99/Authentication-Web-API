using IdentityModel.Client;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NewAuthenticationWebAPI;
using NewAuthenticationWebAPI.Auth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTest 
{
    public class IntegrationTestAuth : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly AppDbContext _context;
        //private readonly HttpClient _client;
        public IntegrationTestAuth(WebApplicationFactory<Startup> factoryServer)
        {
            _factory = factoryServer;
            var scope = _factory.Services.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
        }

        #region Test Register user
        [Fact]
        public async Task Register_shouldAddUserSuccessfullyAndReturnOk()
        {
            //Arrange  
            var _client = _factory.CreateClient();
            RegisterModel registerModel = new RegisterModel()
            {
                Username = "test",
                Password = "Password@12345",
                Email = "test@test.com"
            };


            //Act 
            var response = await _client.PostAsJsonAsync("/api/Authenticate/Register", registerModel);

            //Assert
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                Assert.Contains("User Created Successfully!", responseString);
            }
            else
            {
                Assert.Fail("Api call failed.");
            }

        }
        #endregion

        #region Test Login
        [Fact]
        public async Task Login_Should_Return_Ok_For_Authenticated_User()
        {
            //Arrange 
            var _client = _factory.CreateClient();
            RegisterModel loginModel = new RegisterModel()
            {
                Username = "test",
                Password = "Password@12345"
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(loginModel),
                Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/Authenticate/Login", content);
            response.EnsureSuccessStatusCode();


            var responseString = await response.Content.ReadAsStringAsync();

            var token = JsonConvert.DeserializeObject<TokenResponse>(responseString);
            
            Assert.NotNull(token);

            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

        }

        [Fact]
        public async Task Login_Should_Return_401_For_Authenticated_User()
        {
            //Arrange 
            var _client = _factory.CreateClient();
            RegisterModel loginModel = new RegisterModel()
            {
                Username = "test",
                Password = "Password"
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(loginModel),
                Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/Authenticate/Login", content);

            var result = response.IsSuccessStatusCode;
            Assert.False(result);

        }
        #endregion

        #region Test Create & Delete database in same testcase
        [Fact]
        public async Task Login_should_createDatabase_and_login_then_deleteDatabase()
        {
            var _client = _factory.CreateClient();

            await _client.PostAsJsonAsync("/api/Authenticate/Register", new {
                Username = "test5",
                Password = "Password@0000",
                Email = "test5@test5.com"
            });

            var loginModel = new RegisterModel()
            {
                Username = "test5",
                Password = "Password@0000"
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(loginModel),
                Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/Authenticate/Login", content);
            response.EnsureSuccessStatusCode();


            var responseString = await response.Content.ReadAsStringAsync();

            var token = JsonConvert.DeserializeObject<TokenResponse>(responseString);

            Assert.NotNull(token);

            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();
        }
        #endregion



        #region Test Register admin
        [Fact]
        public async Task Register_admin_shouldAddUserSuccessfullyAndReturnOk()
        {
            var _client = _factory.CreateClient();
            RegisterModel registerModel = new RegisterModel()
            {
                Username = "testAdmin",
                Password = "Admin@12345678",
                Email = "Admin@Admin.com"
            };

            var response = await _client.PostAsJsonAsync("/api/Authenticate/register-admin", registerModel);
            response.EnsureSuccessStatusCode();

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                Assert.Contains("User Created Successfully!", responseString);
            }
            else
            {
                Assert.Fail("Api call failed.");
            }
        }
        #endregion


        #region Test Login admin
        [Fact]
        public async Task Login_Should_Return_Ok_For_Authenticated_Admin()
        {
            //Arrange 
            var _client = _factory.CreateClient();
            RegisterModel loginModel = new RegisterModel()
            {
                Username = "testAdmin",
                Password = "Admin@12345678"
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(loginModel),
                Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/Authenticate/Login", content);
            response.EnsureSuccessStatusCode();


            var responseString = await response.Content.ReadAsStringAsync();

            var token = JsonConvert.DeserializeObject<TokenResponse>(responseString);

            Assert.NotNull(token);
        }

        [Fact]
        public async Task Login_Should_Return_401_For_UnAuthenticated_Admin()
        {
            //Arrange 
            var _client = _factory.CreateClient();
            RegisterModel loginModel = new RegisterModel()
            {
                Username = "adminAdmin",
                Password = "Admin@123"
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(loginModel),
                Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/Authenticate/Login", content);

            var result = response.IsSuccessStatusCode;
            Assert.False(result);
        }

        #endregion


        #region Test TestController

        [Fact]
        public async Task TestController_should_return_string()
        {
            var _client = _factory.CreateClient();

            var response = await _client.GetAsync("/api/Test");
            response.EnsureSuccessStatusCode();

            Assert.NotNull(response);

            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

        }
        #endregion









        public class TokenResponse
        {
            public string Token { get; set; }
        }
    }
}
