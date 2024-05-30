using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewAuthenticationWebAPI.Auth;

namespace NewAuthenticationWebAPI.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "Token Works!!";
        }
    }
}
