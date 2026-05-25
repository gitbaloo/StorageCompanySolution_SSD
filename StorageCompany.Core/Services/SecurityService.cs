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
using Konscious.Security.Cryptography;

namespace StorageCompany.Core.Services;

public class SecurityService(IOptionsMonitor<AppOptions> optionsMonitor, IUserRepository repository) : ISecurityService
{
    // Argon2 configurations
    private const int SaltSize = 16; // 16 Bytes
    private const int HashSize = 32; // 32 Bytes
    private const int Parallelism = 1; // CPU threads
    private const int Iterations = 2; // Number of hash iterations
    private const int MemorySize = 19456; // Kilobytes ~19 MB of Ram cost 
        
    public async Task<AuthResponse> Login(AuthLoginRequest dto)
    {
        var user = await repository.GetByEmailAsync(dto.Email) ?? throw new ValidationException("Email Not Found");
        
        VerifyPasswordOrThrow(dto.Password, user.PasswordHash);

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
        var hash = HashPassword(dto.Password);
        var role = dto.Role == "admin" ? Constants.AdminRole : Constants.CustomerRole;
        
        var newUser = await repository.AddUser(new User
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            PasswordHash = hash,
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
    
  
    // Hashes a plain-text password using Argon2id and returns a Base64 string
    public string HashPassword(string password)
    {
        
        var salt = GenerateSalt();
        var passwordBytes = Encoding.UTF8.GetBytes(password);

        // Configure Argon2id with the given parameters
        var argon = new Argon2id(passwordBytes)
        {
            Salt = salt,
            DegreeOfParallelism = Parallelism,
            MemorySize = MemorySize,
            Iterations = Iterations,
        };
        
        var hash = argon.GetBytes(HashSize);

        // Combines Salt + Hash into a single byte array
        var combined = new byte[SaltSize + HashSize];
        Buffer.BlockCopy(salt, 0, combined, 0, SaltSize);
        Buffer.BlockCopy(hash, 0, combined, SaltSize, HashSize);
        
        return Convert.ToBase64String(combined);
    }

    // Verifies a plain-text password against a stored one
    public void VerifyPasswordOrThrow(string password, string hashedPassword)
    {
        var combinedBytes = Convert.FromBase64String(hashedPassword);
        var salt = new byte[SaltSize];
        var hash = new byte[HashSize];
        
        Buffer.BlockCopy(combinedBytes, 0, salt, 0, SaltSize);
        Buffer.BlockCopy(combinedBytes, SaltSize, hash, 0, HashSize);

        var passwordBytes = Encoding.UTF8.GetBytes(password);

        var argon = new Argon2id(passwordBytes)
        {
            Salt = salt,
            DegreeOfParallelism = Parallelism,
            MemorySize = MemorySize,
            Iterations = Iterations,
        };
        
        var computedHash = argon.GetBytes(HashSize);

        if (!computedHash.SequenceEqual(hash)) {
            throw new AuthenticationException("Invalid Password");
        }
    }
    
     // Generates a random number with the given size and used as a Salt
    public byte[] GenerateSalt()
    {
        return RandomNumberGenerator.GetBytes(SaltSize);
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