using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OrderManagement.API.Endpoints;

public static class AuthEndpoints
{
    private const string FixedEmail = "dev@martech.com";
    private const string FixedPassword = "Senha@123";

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", Handle)
            .AllowAnonymous()
            .WithName("Login")
            .WithSummary("Authenticate and receive a JWT token")
            .Produces<LoginResponse>()
            .Produces(401);

        return app;
    }

    private static IResult Handle(LoginRequest request, IConfiguration configuration)
    {
        if (request.Email != FixedEmail || request.Password != FixedPassword)
            return Results.Unauthorized();

        var token = GenerateToken(request.Email, configuration);

        return Results.Ok(new LoginResponse(token));
    }

    private static string GenerateToken(string email, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]!;
        var issuer = jwtSettings["Issuer"]!;
        var audience = jwtSettings["Audience"]!;
        var expiresInMinutes = int.Parse(jwtSettings["ExpiresInMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed record LoginRequest(string Email, string Password);
    private sealed record LoginResponse(string Token);
}
