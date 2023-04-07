using AutoMapper;
using DBVBahia.Api.Controllers;
using DBVBahia.Api.ViewModels;
using DBVBahia.Business.Intefaces;
using DBVBahia.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DBVBahia.Api.V1.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/produtos")]
    public class ProdutosController : MainController
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IPictureRepository _pictureRepository;
        private readonly IProdutoService _produtoService;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProdutosController(INotificador notificador,
                                  IProdutoRepository produtoRepository,
                                  IProdutoService produtoService,
                                  IMapper mapper,
                                  IUser user,
                                  IPictureRepository imageRepository,
                                  IHttpContextAccessor httpContextAccessor) : base(notificador, user)
        {
            _produtoRepository = produtoRepository;
            _produtoService = produtoService;
            _mapper = mapper;
            _pictureRepository = imageRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<IEnumerable<ProdutoViewModel>> ObterTodos()
        {
            return _mapper.Map<IEnumerable<ProdutoViewModel>>(await _produtoRepository.ObterProdutosFornecedores());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> ObterPorId(Guid id)
        {
            var produtoViewModel = await ObterProduto(id);

            if (produtoViewModel == null) return NotFound();

            return produtoViewModel;
        }

        [HttpPost]
        public async Task<ActionResult<ProdutoViewModel>> Adicionar(ProdutoViewModel produtoViewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            if (!(await UploadArquivo(produtoViewModel.Picture)))
            {
                return CustomResponse(produtoViewModel);
            }

            await _produtoService.Adicionar(_mapper.Map<Produto>(produtoViewModel));

            return CustomResponse(produtoViewModel);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Atualizar(Guid id, ProdutoViewModel produtoViewModel)
        {
            if (id != produtoViewModel.Id)
            {
                NotificarErro("Os ids informados não são iguais!");
                return CustomResponse();
            }

            var produtoAtualizacao = await ObterProduto(id);

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            if (produtoViewModel.Picture.ImagemUpload != null)
            {
                if (!(await UploadArquivo(produtoViewModel.Picture)))
                {
                    return CustomResponse(ModelState);
                }

                var pictures = await _pictureRepository.Buscar(i => i.Nome == produtoViewModel.Picture.Name);

                produtoAtualizacao.Picture = _mapper.Map<PictureViewModel>(pictures.FirstOrDefault());
            }

            produtoAtualizacao.FornecedorId = produtoViewModel.FornecedorId;
            produtoAtualizacao.Nome = produtoViewModel.Nome;
            produtoAtualizacao.Descricao = produtoViewModel.Descricao;
            produtoAtualizacao.Valor = produtoViewModel.Valor;
            produtoAtualizacao.Ativo = produtoViewModel.Ativo;

            await _produtoService.Atualizar(_mapper.Map<Produto>(produtoAtualizacao));

            return CustomResponse(produtoViewModel);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> Excluir(Guid id)
        {
            var produto = await ObterProduto(id);

            if (produto == null) return NotFound();

            await _produtoService.Remover(id);

            return CustomResponse(produto);
        }

        private async Task<ProdutoViewModel> ObterProduto(Guid id)
        {
            return _mapper.Map<ProdutoViewModel>(await _produtoRepository.ObterProdutoFornecedor(id));
        }

        private async Task<bool> UploadArquivo(PictureViewModel pictureViewModel)
        {
            if (string.IsNullOrEmpty(pictureViewModel.ImagemUpload64))
            {
                NotificarErro("Forneça uma imagem para este produto!");
                return false;
            }

            var imageDataByteArray = Convert.FromBase64String(pictureViewModel.ImagemUpload64);

            var imageName = pictureViewModel + ".jpg"; // or .png, .gif, etc.

            var imageExists = await _pictureRepository.Buscar(i => i.Nome == imageName);

            if (imageExists.Any())
            {
                NotificarErro("Já existe um arquivo com este nome!");
                return false;
            }

            var picture = _mapper.Map<Picture>(pictureViewModel);

            await _pictureRepository.Adicionar(picture);

            return true;
        }

        #region UploadAlternativo

        [RequestSizeLimit(5222510)]
        [HttpPost("Adicionar")]
        public async Task<ActionResult<ProdutoViewModel>> AdicionarAlternativo([FromForm] ProdutoViewModel produtoViewModel)
        {
            if (!ModelState.IsValid)
            {
                return CustomResponse(ModelState);
            }

            if (produtoViewModel.Picture.ImagemUpload.Length > 5222510)
            {
                return BadRequest(new FileNotFoundException());
            }

            if (!await UploadArquivoAlternativo(produtoViewModel.Picture))
            {
                return CustomResponse(ModelState);
            }

            var pictures = await _pictureRepository.Buscar(i => i.Nome == produtoViewModel.Picture.ImagemUpload.FileName);

            produtoViewModel.Picture = _mapper.Map<PictureViewModel>(pictures.FirstOrDefault());
            await _produtoService.Adicionar(_mapper.Map<Produto>(produtoViewModel));

            return CustomResponse(produtoViewModel);
        }

        [HttpPost("imagem")]
        public ActionResult AdicionarImagem(IFormFile file)
        {
            return Ok(file);
        }

        private async Task<bool> UploadArquivoAlternativo(PictureViewModel pictureViewModel)
        {
            if (pictureViewModel is null || pictureViewModel.ImagemUpload == null || pictureViewModel.ImagemUpload.Length == 0)
            {
                NotificarErro("Forneça uma imagem para este produto!");
                return false;
            }

            var pictures = await _pictureRepository.Buscar(i => i.Nome == pictureViewModel.Name);

            if (pictures.Any())
            {
                NotificarErro("Já existe um arquivo com este nome!");
                return false;
            }

            var picture = _mapper.Map<Picture>(pictureViewModel);

            await _pictureRepository.Adicionar(picture);

            return true;
        }

        #endregion UploadAlternativo
    }
}