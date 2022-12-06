//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.EntityFrameworkCore;
//using APIServer.Data;
//using APIServer.Models;
//using APIServer.Authorization;
//using APIServer.Helpers;
//using APIServer.Services;
//using System.Text.Json.Serialization;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllers();


//{
//    var services = builder.Services;
//    var env = builder.Environment;

//    services.AddDbContext<DataContext>();
//    services.AddCors();
//    services.AddControllers()
//        .AddJsonOptions(x => x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

//    // configure strongly typed settings object
//    services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

//    // configure DI for application services
//    services.AddScoped<IJwtUtils, JwtUtils>();
//    services.AddScoped<IUserService, UserService>();
//}


///*builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
//builder.Services.AddAuthorization();
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidIssuer = AuthOptions.ISSUER,
//            ValidateAudience = true,
//            ValidAudience = AuthOptions.AUDIENCE,
//            ValidateLifetime = true,
//            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
//            ValidateIssuerSigningKey = true,
//        };
//    });
//builder.Services.AddDbContext<APIServerContext>(options =>
//    options.UseMySQL(builder.Configuration.GetConnectionString("MVCServerContext") ?? throw new InvalidOperationException("Connection string 'MVCServerContext' not found.")));
//*/
//var app = builder.Build();

///*app.UseAuthentication();
//app.UseAuthorization();*/

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}



////app.UseHttpsRedirection();



///*app.Map("/login/{username}", (string username) =>
//{
//    var context = new APIServerContext();
//    User? user = 
//    var claims = new List<Claim> { new Claim(ClaimTypes.Name, username) };
//    // создаем JWT-токен
//    var jwt = new JwtSecurityToken(
//            issuer: AuthOptions.ISSUER,
//            audience: AuthOptions.AUDIENCE,
//            claims: claims,
//            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
//            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

//    return new JwtSecurityTokenHandler().WriteToken(jwt);
//});*/

////app.Map("/data", [Authorize] () => new { message = "Hello World!" });

////app.MapControllers();


//{
//    // global cors policy
//    app.UseCors(x => x
//        .SetIsOriginAllowed(origin => true)
//        .AllowAnyMethod()
//        .AllowAnyHeader()
//        .AllowCredentials());

//    // global error handler
//    app.UseMiddleware<ErrorHandlerMiddleware>();

//    // custom jwt auth middleware
//    app.UseMiddleware<JwtMiddleware>();

//    app.MapControllers();
//}

//app.Run();


///*public class AuthOptions
//{
//    public const string ISSUER = "AuthServer";
//    public const string AUDIENCE = "AuthClient";
//    const string KEY = "0eJE7qV6vHYTlLscHAMBacCe";
//    public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
//        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
//}*/