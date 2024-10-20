﻿
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace SurveyBasket.Authentication
{
    public class JwtProvider(IOptions<JwtOptions> JwtOptions) : IJwtProvider
    {
        private readonly JwtOptions _jwtOptions = JwtOptions.Value;

        public (string token, int ExpiresIn) GenerateToken(ApplicationUser user, IEnumerable<string> roles, IEnumerable<string> permissions)
        {
            Claim[] claims = [
                new (JwtRegisteredClaimNames.Sub,user.Id),
                new (JwtRegisteredClaimNames.Email,user.Email!),
                new (JwtRegisteredClaimNames.GivenName,user.FirstName),
                new (JwtRegisteredClaimNames.FamilyName,user.LastName),
                new (JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
            new(nameof(roles), JsonSerializer.Serialize(roles), JsonClaimValueTypes.JsonArray),
                new(nameof(permissions), JsonSerializer.Serialize(permissions), JsonClaimValueTypes.JsonArray)
            ];

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));

            var SigningCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);



            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes),
                signingCredentials: SigningCredentials
                );

            return (token: new JwtSecurityTokenHandler().WriteToken(token), ExpiresIn: _jwtOptions.ExpiryMinutes * 60);
        }

        public string? ValidateToken(string Token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            try
            {
                tokenHandler.ValidateToken(Token, new TokenValidationParameters
                {
                    IssuerSigningKey = symmetricSecurityKey,
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                return jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value;
            }

            catch
            {
                return null;
            }
        }
    }
}
