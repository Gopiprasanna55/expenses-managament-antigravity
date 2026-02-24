using AutoMapper;
using ExpenseManager.Application.DTOs;
using ExpenseManager.Domain.Entities;

namespace ExpenseManager.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Category, CategoryDto>().ReverseMap();
            
            CreateMap<Expense, ExpenseDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty));
            
            CreateMap<ExpenseDto, Expense>();

            CreateMap<Wallet, WalletDto>();
            
            CreateMap<WalletActivity, WalletActivityDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));
            
            CreateMap<ApplicationUser, UserDto>();
        }
    }
}
