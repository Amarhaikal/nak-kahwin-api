using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using nak_kahwin_api.Data;
using nak_kahwin_api.DTOs.Auth;
using nak_kahwin_api.Models;

namespace nak_kahwin_api.Services;

public class AuthService(AppDbContext db, IConfiguration config)
{
    public async Task<AuthResponse?> RegisterAsync(RegisterRequest req)
    {
        var exists = await db.Users.AnyAsync(u => u.Email == req.Email);
        if (exists) return null;

        if (req.Role != "groom" && req.Role != "bride") return null;

        var user = new User
        {
            Name = req.Name,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = req.Role
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var accessToken = GenerateToken(user);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id);

        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: refreshToken.Token,
            UserId: user.Id,
            Name: user.Name,
            Email: user.Email,
            Role: user.Role
        );
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.ToLower().Trim());

        if (user is null) return null;

        var isValid = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
        if (!isValid) return null;

        var accessToken = GenerateToken(user);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id);

        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: refreshToken.Token,
            UserId: user.Id,
            Name: user.Name,
            Email: user.Email,
            Role: user.Role
        );
    }

    public async Task<AuthResponse?> RefreshTokenAsync(string token)
    {
        var refreshToken = await db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken is null || !refreshToken.IsActive)
        {
            return null;
        }

        // Revoke current token (rotation)
        refreshToken.RevokedAt = DateTime.UtcNow;

        // Generate new token pair
        var newAccessToken = GenerateToken(refreshToken.User);
        var newRefreshToken = await GenerateRefreshTokenAsync(refreshToken.UserId);

        return new AuthResponse(
            AccessToken: newAccessToken,
            RefreshToken: newRefreshToken.Token,
            UserId: refreshToken.User.Id,
            Name: refreshToken.User.Name,
            Email: refreshToken.User.Email,
            Role: refreshToken.User.Role
        );
    }

    public async Task<bool> RevokeTokenAsync(string token)
    {
        var refreshToken = await db.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken is null || !refreshToken.IsActive)
        {
            return false;
        }

        refreshToken.RevokedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return true;
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(string userId)
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var tokenString = Convert.ToBase64String(randomNumber);

        var refreshToken = new RefreshToken
        {
            Token = tokenString,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync();

        return refreshToken;
    }

    private string GenerateToken(User user)
    {
        var jwtKey = config["Jwt:Key"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("role", user.Role),
            new Claim("name", user.Name)
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}