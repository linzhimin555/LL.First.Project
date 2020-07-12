using LL.FirstCore.IRepository.Base;
using LL.FirstCore.IServices.Base;
using LL.FirstCore.Model.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LL.FirstCore.Services.Base
{
    public class RequestLogServices : BaseServices<RequestLogEntity>, IRequestLogServices
    {
        public RequestLogServices(IRequestLogRepository postRepository) : base(postRepository)
        {

        }
    }
}
