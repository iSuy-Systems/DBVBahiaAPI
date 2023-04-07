using AutoMapper;
using DBVBahia.Api.ViewModels;
using DBVBahia.Business.Models;

namespace DBVBahia.Api.Configuration
{
    public class AutomapperConfig : Profile
    {
        public AutomapperConfig()
        {
            CreateMap<Fornecedor, FornecedorViewModel>().ReverseMap();
            CreateMap<Endereco, EnderecoViewModel>().ReverseMap();
            CreateMap<Picture, PictureViewModel>()
                .ForMember(dest => dest.ImagemUpload, opt => opt.Ignore())
                .ForMember(dest => dest.ImagemUpload64, opt => opt.Ignore())
                .AfterMap((src, dest, context) =>
                {
                    if(src.Image != null && src.Image.Length > 0)
                    {
                        var imageBase64 = Convert.ToBase64String(src.Image);
                        dest.ImagemUpload64 = imageBase64;
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.Image, opt => opt.Ignore())
                .AfterMap(async (src, dest, context) =>
                {
                    if (!string.IsNullOrEmpty(src.ImagemUpload64))
                    {
                        var imageBytes = Convert.FromBase64String(src.ImagemUpload64);
                        dest.Image = imageBytes;
                    }

                    if (src.ImagemUpload != null && src.ImagemUpload.Length > 0)
                    {
                        var file = src.ImagemUpload.OpenReadStream();
                        byte[] fileToSave;

                        using (MemoryStream ms = new MemoryStream())
                        {
                            await file.CopyToAsync(ms);
                            fileToSave = ms.ToArray();
                        }

                        dest.Image = fileToSave;
                    }
                });

            CreateMap<ProdutoViewModel, Produto>();

            CreateMap<ProdutoImagemViewModel, Produto>().ReverseMap();

            CreateMap<Produto, ProdutoViewModel>()
                .ForMember(dest => dest.NomeFornecedor, opt => opt.MapFrom(src => src.Fornecedor.Nome));
        }
    }
}