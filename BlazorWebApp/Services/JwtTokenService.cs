using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
public class JwtTokenService : IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly RSA _rsa;
    private readonly SigningCredentials _signingCredentials;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        var privateKeyBase64 = _configuration["JwtSettings:PrivateKeyBase64"];

        if (string.IsNullOrWhiteSpace(privateKeyBase64))
            throw new InvalidOperationException("JWT Private Key is not configured properly.");

        var privateKeyPem = Encoding.UTF8.GetString(Convert.FromBase64String(privateKeyBase64));
        _rsa = RSA.Create();
        _rsa.ImportFromPem(privateKeyPem);

        _signingCredentials = new SigningCredentials(
            new RsaSecurityKey(_rsa),
            SecurityAlgorithms.RsaSha256
        );
    }

    public string GenerateToken(string userId, string email)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: _signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    // This is called automatically at the end of the request scope
    public void Dispose()
    {
        _rsa?.Dispose();
    }
}
