using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Serializers;
using Microsoft.Extensions.Options;
using StorageCompany.Core.Entities;
using StorageCompany.Core.Enums;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Interfaces.Services;
using StorageCompany.Core.Validators;

namespace StorageCompany.Core.Services;

public class SecurityService(IOptionsMonitor<AppOptions> optionsMonitor, IUserRepository repository) : ISecurityService
{
    public async Task<AuthResponse> Login(AuthLoginRequest dto)
    {
        var user = await repository.GetByEmailAsync(dto.Email) ?? throw new ValidationException("Email Not Found");
        
        VerifyPasswordOrThrow(dto.Password + user.PasswordSalt, user.PasswordHash);

        return new AuthResponse
        {
            Jwt = GenerateJwt(new JwtClaims
            {
                Id = user.Id.ToString(),
                Email = user.Email,
                Role = user.Role,
                Exp = DateTimeOffset.UtcNow.AddHours(2).ToUnixTimeSeconds().ToString()
            })
        };
    }

    public async Task<AuthResponse> Register(AuthRegisterRequest dto)
    {
        var user = await repository.GetByEmailAsync(dto.Email);
        
        if (user is not null) {
            throw new ValidationException("User already Exists");
        }

        PasswordPolicyValidator.Validate(dto.Password, dto.Role);

        var salt  = GenerateSalt();
        var hash = HashPassword(dto.Password + salt);
        var role = dto.Role == "admin" ? Constants.AdminRole : Constants.CustomerRole;
        
        var newUser = await repository.AddUser(new User
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = role
        });

        return new AuthResponse
        {
            Jwt = GenerateJwt(new JwtClaims
            {
                Id = newUser.Id.ToString(),
                Email = newUser.Email,
                Role = newUser.Role,
                Exp = DateTimeOffset.UtcNow.AddHours(2).ToUnixTimeSeconds().ToString()
            })
        };
    }
    
    /// <summary>
    ///     Gives hex representation of SHA512 hash
    /// </summary>
    /// <param name="password"></param>
    /// <returns></returns>
    public string HashPassword(string password)
    {
        using var sha512 = SHA512.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha512.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    public void VerifyPasswordOrThrow(string password, string hashedPassword)
    {
        if(HashPassword(password) != hashedPassword) {
            throw new AuthenticationException("Invalid login");
        }
    }

    public string GenerateSalt()
    {
        return Guid.NewGuid().ToString();
    }

    public string GenerateJwt(JwtClaims claims)
    {
        var tokenbuilder = new JwtBuilder()
            .WithAlgorithm(new HMACSHA512Algorithm())
            .WithSecret(optionsMonitor.CurrentValue.JwtSecret)
            .WithUrlEncoder(new JwtBase64UrlEncoder())
            .WithJsonSerializer(new JsonNetSerializer());

        foreach (var claim in claims.GetType().GetProperties()) {
            tokenbuilder.AddClaim(claim.Name, claim.GetValue(claims)!.ToString());
        }
        return tokenbuilder.Encode();
    }
    
    public JwtClaims VerifyJwtOrThrow(string jwt)
    {
        var token = new JwtBuilder()
            .WithAlgorithm(new HMACSHA512Algorithm())
            .WithSecret(optionsMonitor.CurrentValue.JwtSecret)
            .WithUrlEncoder(new JwtBase64UrlEncoder())
            .WithJsonSerializer(new JsonNetSerializer())
            .MustVerifySignature()
            .Decode<JwtClaims>(jwt);

        if (DateTimeOffset.FromUnixTimeSeconds(long.Parse(token.Exp)) < DateTimeOffset.UtcNow) {
            throw new AuthenticationException("Token expired");
        }

        return token;
    }
}