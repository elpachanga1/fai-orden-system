using AutoMapper;

namespace Services
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Domain.Models.Product, DataRepository.Models.Product>();
            CreateMap<DataRepository.Models.Product, Domain.Models.Product>();

            CreateMap<Domain.Models.ShoppingCart, DataRepository.Models.ShoppingCart>();
            CreateMap<DataRepository.Models.ShoppingCart, Domain.Models.ShoppingCart>();

            CreateMap<Domain.Models.Item, DataRepository.Models.Item>();
            CreateMap<DataRepository.Models.Item, Domain.Models.Item>();

            CreateMap<Domain.Models.Session, DataRepository.Models.Session>();
            CreateMap<DataRepository.Models.Session, Domain.Models.Session>();

            CreateMap<Domain.Models.User, DataRepository.Models.User>();
            CreateMap<DataRepository.Models.User, Domain.Models.User>();
        }
    }
}
