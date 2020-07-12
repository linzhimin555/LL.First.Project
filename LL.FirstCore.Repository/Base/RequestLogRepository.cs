using LL.FirstCore.IRepository.Base;
using LL.FirstCore.Model.Models;
using LL.FirstCore.Repository.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace LL.FirstCore.Repository.Base
{
    public class RequestLogRepository : BaseRepository<RequestLogEntity>, IRequestLogRepository
    {
        public RequestLogRepository(BaseDbContext dbContext) : base(dbContext)
        {

        }
    }
}
