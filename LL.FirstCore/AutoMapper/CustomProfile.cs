using AutoMapper;
using LL.FirstCore.Model.Dto;
using LL.FirstCore.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LL.FirstCore.AutoMapper
{
    public class CustomProfile : Profile
    {
        public CustomProfile()
        {
            CreateMap<BaseUserInfo, UserInfoDto>().ReverseMap();
        }
    }
}
