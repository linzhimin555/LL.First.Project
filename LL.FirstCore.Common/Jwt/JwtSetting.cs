using System;

namespace LL.FirstCore.Common.Jwt
{
    public class JwtSetting
    {
        public string ValidIssuer { get; set; }

        public string ValidAudience { get; set; }

        public string IssuerSigningKey { get; set; }

        public int Expires { get; set; }
    }
}
