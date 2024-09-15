using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAPI.Helpers;

public class AuthHelper
{
    private readonly IConfiguration _config;

    public AuthHelper(IConfiguration config)
    {
        _config = config;
    }

    public byte[] GetPasswordHash(string password, byte[] passwordSalt)
    {
        string passwordSaltPlusString =
            _config.GetSection("AppSettings:PasswordKey").Value
            + Convert.ToBase64String(passwordSalt);

        byte[] passwordHash = KeyDerivation.Pbkdf2(
            password: password,
            salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8
        );

        return passwordHash;
    }

    public string CreateToken(int userId)
    {
        Claim[] claims = [new("userId", userId.ToString())];

        string? tokenKeyString = _config.GetSection("Appsettings:TokenKey").Value;

        SymmetricSecurityKey tokenKey =
            new(Encoding.UTF8.GetBytes(tokenKeyString != null ? tokenKeyString : ""));

        SigningCredentials credentials = new(tokenKey, SecurityAlgorithms.HmacSha512Signature);

        SecurityTokenDescriptor descriptor =
            new()
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = credentials,
                Expires = DateTime.Now.AddDays(1),
            };

        JwtSecurityTokenHandler tokenHandler = new();

        SecurityToken securityToken = tokenHandler.CreateToken(descriptor);

        return tokenHandler.WriteToken(securityToken);
    }
}
