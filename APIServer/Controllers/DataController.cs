using APIServer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using APIServer.Models;
using System;
using MySqlX.XDevAPI.Common;
using Microsoft.AspNetCore.Authorization;

namespace APIServer.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class DataController : Controller
    {
        private readonly APIServerContext _context;

        public DataController(APIServerContext context)
        {
            _context = context;
        }
        /*public IResult Post(UserLogin userData)
        {
            User? user = _context.User.FirstOrDefault(u => u.Username == userData.Username && u.Password == userData.Password);
            if (user is null) return Results.Unauthorized();
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Username) };
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    claims: claims,
                    expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            var response = new
            {
                access_token = encodedJwt,
                username = user.Username
            };
            return Results.Json(response);*/
        [HttpGet]
        public string Get()
        {
            var nameIdentifier = this.HttpContext.User.Claims
            .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            return "123123";
        }
    }
}
