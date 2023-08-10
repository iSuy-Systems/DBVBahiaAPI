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
        private readonly IProdutoService _produtoService;
        private readonly IMapper _mapper;
        private readonly IFornecedorRepository _fornecedorRepository;

        public ProdutosController(INotificador notificador,
                                  IProdutoRepository produtoRepository,
                                  IProdutoService produtoService,
                                  IMapper mapper,
                                  IUser user,
                                  IFornecedorRepository fornecedorRepository) : base(notificador, user)
        {
            _produtoRepository = produtoRepository;
            _produtoService = produtoService;
            _mapper = mapper;
            _fornecedorRepository = fornecedorRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProdutoViewModel>>> ObterTodos()
        {
            var produtosViewModel = Enumerable.Empty<ProdutoViewModel>();

            var produtos = await _produtoRepository.ObterProdutosFornecedores();

            if(produtos.Any()) { 
                produtosViewModel = _mapper.Map<IEnumerable<ProdutoViewModel>>(produtos);
            }

            return CustomResponse(produtosViewModel);
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


            if (!ValidUploadArquivo(produtoViewModel.Picture))
            {
                return CustomResponse(ModelState);
            }

            var produto = _mapper.Map<Produto>(produtoViewModel);

            await UpdateProdutoInFornecedor(produto);

            return CustomResponse(produtoViewModel);
        }

        private async Task<bool> UpdateProdutoInFornecedor(Produto produto)
        {
            var fornecedor = await _fornecedorRepository.ObterPorId(produto.FornecedorId);

            if (fornecedor == null)
            {
                NotificarErro("Fornecedor não encontrado");
                return false;
            }

            fornecedor.Produtos.Add(produto);

            await _fornecedorRepository.Atualizar(fornecedor);
            return true;
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

            if (!string.IsNullOrEmpty(produtoViewModel.Picture.ImagemUpload64))
            {
                produtoAtualizacao.Picture = produtoViewModel.Picture;
            }

            produtoAtualizacao.FornecedorId = produtoViewModel.FornecedorId;
            produtoAtualizacao.Nome = produtoViewModel.Nome;
            produtoAtualizacao.Descricao = produtoViewModel.Descricao;
            produtoAtualizacao.Valor = produtoViewModel.Valor;
            produtoAtualizacao.Ativo = produtoViewModel.Ativo;

            var produto = _mapper.Map<Produto>(produtoAtualizacao);

            await UpdateProdutoInFornecedor(produto);

            return CustomResponse(produtoAtualizacao);
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

        private bool ValidUploadArquivo(PictureViewModel pictureViewModel)
        {
            if (string.IsNullOrEmpty(pictureViewModel.ImagemUpload64))
            {
                NotificarErro("Forneça uma imagem para este produto!");
                return false;
            }
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

            if (!ValidUploadArquivoAlternativo(produtoViewModel.Picture))
            {
                return CustomResponse(ModelState);
            }

            var product = _mapper.Map<Produto>(produtoViewModel);

            await UpdateProdutoInFornecedor(product);

            return CustomResponse(produtoViewModel);
        }

        private bool ValidUploadArquivoAlternativo(PictureViewModel pictureViewModel)
        {
            if (pictureViewModel.ImagemUpload.Length > 5222510)
            {
                NotificarErro("Tamanho de imagem excedido. Máximo permitido é 5MB!");
                return false;
            }

            if (pictureViewModel is null || pictureViewModel.ImagemUpload == null || pictureViewModel.ImagemUpload.Length == 0)
            {
                NotificarErro("Forneça uma imagem para este produto!");
                return false;
            }
            return true;
        }
        #endregion UploadAlternativo
    }
}