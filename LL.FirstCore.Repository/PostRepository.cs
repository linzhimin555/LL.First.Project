using LL.FirstCore.IRepository;
using LL.FirstCore.Model;
using LL.FirstCore.Repository.Base;
using LL.FirstCore.Repository.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace LL.FirstCore.Repository
{
    public class PostRepository : BaseRepository<Post>, IPostRepository
    {
        public PostRepository(BaseDbContext dbContext) : base(dbContext)
        {

        }
    }
}
