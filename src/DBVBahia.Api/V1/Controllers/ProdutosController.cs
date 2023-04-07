using AutoMapper;
using DBVBahia.Api.Controllers;
using DBVBahia.Api.Data;
using DBVBahia.Api.Extensions;
using DBVBahia.Api.ViewModels;
using DBVBahia.Business.Intefaces;
using DBVBahia.Business.Models;
using DBVBahia.Data.Context;
using DBVBahia.Data.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

			var imagemNome = Guid.NewGuid() + "_" + produtoViewModel.Imagem;
			if (!(await UploadArquivo(produtoViewModel.ImagemUpload, imagemNome)))
			{
				return CustomResponse(produtoViewModel);
			}

			produtoViewModel.Imagem = imagemNome;
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

			if (string.IsNullOrEmpty(produtoViewModel.Imagem))
				produtoViewModel.Imagem = produtoAtualizacao.Imagem;

			if (!ModelState.IsValid) return CustomResponse(ModelState);

			if (produtoViewModel.ImagemUpload != null)
			{
				var imagemNome = Guid.NewGuid() + "_" + produtoViewModel.Imagem;
				if (!(await UploadArquivo(produtoViewModel.ImagemUpload, imagemNome)))
				{
					return CustomResponse(ModelState);
				}

				produtoAtualizacao.Imagem = imagemNome;
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

		private async Task<bool> UploadArquivo(string arquivo, string imgNome)
		{
			if (string.IsNullOrEmpty(arquivo))
			{
				NotificarErro("Forneça uma imagem para este produto!");
				return false;
			}

			var imageDataByteArray = Convert.FromBase64String(arquivo);

			var imageName = imgNome + ".jpg"; // or .png, .gif, etc.

			var imageExists = await _pictureRepository.Buscar(i => i.Nome == imageName);

			if (imageExists.Any())
			{
				NotificarErro("Já existe um arquivo com este nome!");
				return false;
			}

			var imagem = new Picture
			{
				Nome = imageName,
				Image = imageDataByteArray
			};

			await _pictureRepository.Adicionar(imagem);
			await _pictureRepository.SaveChanges();

			return true;
		}

		#region UploadAlternativo
		[RequestSizeLimit(5222510)]
		[HttpPost("Adicionar")]
		public async Task<ActionResult<ProdutoViewModel>> AdicionarAlternativo([FromForm] ProdutoImagemViewModel produtoViewModel)
		{
			if (!ModelState.IsValid)
			{
				return CustomResponse(ModelState);
			}

			if (produtoViewModel.ImagemUpload.Length > 5222510)
			{
				return BadRequest(new FileNotFoundException());
			}

			if (!await UploadArquivoAlternativo(produtoViewModel.ImagemUpload))
			{
				return CustomResponse(ModelState);
			}

			var pictures = await _pictureRepository.Buscar(i => i.Nome == produtoViewModel.ImagemUpload.FileName);

			produtoViewModel.PictureId = pictures.FirstOrDefault().Id;
			await _produtoService.Adicionar(_mapper.Map<Produto>(produtoViewModel));

			return CustomResponse(produtoViewModel);
		}

		[HttpPost("imagem")]
		public ActionResult AdicionarImagem(IFormFile file)
		{
			return Ok(file);
		}



		private async Task<bool> UploadArquivoAlternativo(IFormFile arquivo)
		{
			if (arquivo == null || arquivo.Length == 0)
			{
				NotificarErro("Forneça uma imagem para este produto!");
				return false;
			}

			var pictures = await _pictureRepository.Buscar(i => i.Nome == arquivo.FileName);

			if (pictures.Any())
			{
				NotificarErro("Já existe um arquivo com este nome!");
				return false;
			}

			var file = arquivo.OpenReadStream();
			byte[] fileToSave;

			using (MemoryStream ms = new MemoryStream())
			{
				await file.CopyToAsync(ms);
				fileToSave = ms.ToArray();
			}


			var pictureViewModel = new PictureViewModel
			{
				Image = fileToSave,
				Name = arquivo.FileName
			};

			var picture = _mapper.Map<Picture>(pictureViewModel);

			await _pictureRepository.Adicionar(picture);
			await _pictureRepository.SaveChanges();

			return true;
		}

		#endregion
	}
}