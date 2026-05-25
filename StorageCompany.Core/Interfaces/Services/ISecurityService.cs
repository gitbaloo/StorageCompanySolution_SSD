using StorageCompany.Core.Entities;

namespace StorageCompany.Core.Interfaces.Services;

public interface ISecurityService
{
        public string HashPassword(string password);
        
        public void VerifyPasswordOrThrow(string password, string hashedPassword);
        
        public byte[] GenerateSalt();
        
        public string GenerateJwt(JwtClaims claims);
       
        Task<AuthResponse> Login(AuthLoginRequest dto);
        
        Task<AuthResponse> Register(AuthRegisterRequest dto);
        
        public JwtClaims VerifyJwtOrThrow(string jwt);
}