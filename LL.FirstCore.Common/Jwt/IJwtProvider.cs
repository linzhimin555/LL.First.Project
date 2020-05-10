using System;

namespace LL.FirstCore.Common.Jwt
{
    public interface IJwtProvider
    {
        string CreateJwtToken(TokenModel model);

        string GetUserId();
    }
}
