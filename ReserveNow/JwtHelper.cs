using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ReserveNow
{
    public static class JwtHelper
    {
        public static DateTime? GetTokenExpirationTime(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            if (handler.CanReadToken(token))
            {
                var jwtToken = handler.ReadJwtToken(token);
                var expirationClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "exp");
                if (expirationClaim != null && long.TryParse(expirationClaim.Value, out var expirationTime))
                {
                    return DateTimeOffset.FromUnixTimeSeconds(expirationTime).DateTime;
                }
            }
            return null;
        }

        public static bool IsTokenExpired(string token)
        {
            var expirationTime = GetTokenExpirationTime(token);
            return expirationTime == null || expirationTime <= DateTime.UtcNow;
        }
    }
}
