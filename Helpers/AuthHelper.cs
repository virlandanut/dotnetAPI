using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAPI.Helpers;

public class AuthHelper
{
    private readonly IConfiguration _config;
    private readonly DataContextDapper _dapper;

    public AuthHelper(IConfiguration config)
    {
        _config = config;
        _dapper = new DataContextDapper(config);
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

    public bool ResetPasswordRequest(UserForLoginDto userForSetPassword)
    {
        byte[] passwordSalt = new byte[128 / 8];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetNonZeroBytes(passwordSalt);
        }

        byte[] passwordHash = GetPasswordHash(userForSetPassword.Password, passwordSalt);

        string sqlAddAuth =
            @"EXEC TutorialAppSchema.spRegistration_Upsert 
                    @Email = @emailParam, @PasswordHash = @passwordHashParam, @PasswordSalt = @passwordSaltParam";

        return _dapper.ExecuteSql(
            sqlAddAuth,
            new
            {
                emailParam = userForSetPassword.Email,
                passwordHashParam = passwordHash,
                passwordSaltParam = passwordSalt,
            }
        );
    }
}
