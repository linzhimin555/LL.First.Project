using LL.FirstCore.Model;
using LL.FirstCore.Model.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace LL.FirstCore.Repository.Context
{
    public class BaseDbContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<BaseUserInfo> BaseUserInfos { get; set; }
        public BaseDbContext(DbContextOptions<BaseDbContext> options) : base(options)
        {
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {

        }
    }
}
