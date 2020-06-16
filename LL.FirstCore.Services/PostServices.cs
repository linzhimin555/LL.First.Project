using LL.FirstCore.IRepository;
using LL.FirstCore.IServices;
using LL.FirstCore.Model;
using LL.FirstCore.Services.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace LL.FirstCore.Services
{
    public class PostServices : BaseServices<Post>, IPostServices
    {
        public PostServices(IPostRepository postRepository) : base(postRepository)
        {

        }
    }
}
