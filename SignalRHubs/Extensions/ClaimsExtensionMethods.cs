using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;

namespace SignalRHubs.Extensions
{
    public static class ClaimsExtensionMethods
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static Guid? GetCustomerId(this ClaimsIdentity identity)
        {
            IEnumerable<Claim> claims = identity.Claims;
            var claim = claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.NameIdentifier));

            if (claim == null)
            {
                return null;
            }

            return Guid.Parse(claim.Value);
        }

        /// <summary>
        /// Returns the GUID on claims, returns Guid.Empty if claims not found
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static String GetUserName(this IIdentity identity)
        {
            var claimsIdentity = (ClaimsIdentity)identity;
            IEnumerable<Claim> claims = claimsIdentity.Claims;
            var claim = claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.NameIdentifier));

            if (claim == null)
            {
                //return Guid.Empty;
                return "";
            }
            
            return claim.Value.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static string GetUserEmail(this IIdentity identity)
        {
            var claimsIdentity = (ClaimsIdentity)identity;
            IEnumerable<Claim> claims = claimsIdentity.Claims;
            var claim = claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.Email));

            return claim.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identity"></param>
        /// <returns>string</returns>
        //public static string GetUserName(this IIdentity identity)
        //{
        //    var claimsIdentity = (ClaimsIdentity)identity;
        //    IEnumerable<Claim> claims = claimsIdentity.Claims;
        //    var claim = claims.FirstOrDefault(x => x.Type.Equals(JwtRegisteredClaimNames.Name));

        //    return claim.Value;
        //}
    }
}
