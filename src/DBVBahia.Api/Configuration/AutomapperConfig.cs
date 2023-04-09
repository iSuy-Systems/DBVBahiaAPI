using AutoMapper;
using DBVBahia.Api.ViewModels;
using DBVBahia.Business.Models;

namespace DBVBahia.Api.Configuration
{
    public class AutomapperConfig : Profile
    {
        public AutomapperConfig()
        {
            CreateMap<Fornecedor, FornecedorViewModel>()
                .ForMember(dest => dest.Produtos, opt => opt.Ignore())
                .AfterMap((src, dest, context) =>
                {
                    if (src.Produtos != null && src.Produtos.Any())
                    {
                        dest.Produtos = context.Mapper.Map<IEnumerable<ProdutoViewModel>>(src.Produtos);
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.Produtos, opt => opt.Ignore())
                .AfterMap((src, dest, context) =>
                {
                    if (src.Produtos != null && src.Produtos.Any())
                    {
                        dest.Produtos = context.Mapper.Map<IList<Produto>>(src.Produtos);
                    }
                });

            CreateMap<Endereco, EnderecoViewModel>().ReverseMap();
            CreateMap<Picture, PictureViewModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Nome))
                .ForMember(dest => dest.ImagemUpload, opt => opt.Ignore())
                .ForMember(dest => dest.ImagemUpload64, opt => opt.Ignore())
                .AfterMap((src, dest, context) =>
                {
                    if (src.Image != null && src.Image.Length > 0)
                    {
                        var imageBase64 = Convert.ToBase64String(src.Image);
                        dest.ImagemUpload64 = imageBase64;
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Image, opt => opt.Ignore())
                .AfterMap((src, dest, context) =>
                {
                    if (!string.IsNullOrEmpty(src.ImagemUpload64))
                    {
                        var imageBytes = Convert.FromBase64String(src.ImagemUpload64);
                        dest.Image = imageBytes;
                    }
                });

            CreateMap<Produto, ProdutoViewModel>()
                .ForPath(dest => dest.NomeFornecedor, opt => opt.MapFrom(src => src.Fornecedor.Nome))
                .ForPath(dest => dest.Picture, opt => opt.Ignore())
                .AfterMap((src, dest, context) =>
                {
                    if (src.Picture != null)
                    {
                        dest.Picture = context.Mapper.Map<PictureViewModel>(src.Picture);
                    }
                })
                .ReverseMap()
                .ForPath(dest => dest.Fornecedor, opt => opt.Ignore())
                .ForPath(dest => dest.Fornecedor.Nome, opt => opt.MapFrom(src => src.NomeFornecedor))
                .AfterMap((src, dest, context) =>
                {
                    if (src.FornecedorId != Guid.Empty)
                    {
                        dest.Fornecedor = new Fornecedor
                        {
                            Id = src.FornecedorId,
                        };
                    }
                });
        }
    }
}