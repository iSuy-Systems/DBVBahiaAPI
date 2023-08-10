using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using DBVBahia.Api.V1.Controllers;
using DBVBahia.Api.ViewModels;
using DBVBahia.Business.Intefaces;
using DBVBahia.Business.Models;
using DBVBahia.Data.Repository;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBVBahia.Api.Tests.V1.Controllers
{
    public class ProdutosControllerTests
    {
        private readonly Mock<IProdutoRepository> _produtoRepositoryMock;
        private readonly Mock<IProdutoService> _produtoServiceMock;
        private readonly Mock<IFornecedorRepository> _fornecedorRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IUser> _userMock;
        private readonly Mock<INotificador> _notificadorMock;
        private Fixture _fixture;

        public ProdutosControllerTests()
        {
            _produtoRepositoryMock = new Mock<IProdutoRepository>();
            _produtoServiceMock = new Mock<IProdutoService>();
            _fornecedorRepositoryMock = new Mock<IFornecedorRepository>();
            _mapperMock = new Mock<IMapper>();
            _userMock = new Mock<IUser>();
            _notificadorMock = new Mock<INotificador>();
            _fixture = new Fixture();
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _fixture.Customize(new AutoMoqCustomization());
        }

        [Fact]
        public async Task ObterTodos_DeveRetornarListaDeProdutoViewModel()
        {
            // Arrange
            var produtos = _fixture.CreateMany<Produto>(3);
            
            _produtoRepositoryMock.Setup(repository => repository.ObterProdutosFornecedores())
                                  .ReturnsAsync(produtos);

            var produtoViewModels = _fixture.CreateMany<ProdutoViewModel>(3);

            _mapperMock.Setup(mapper => mapper.Map<IEnumerable<ProdutoViewModel>>(produtos))
                       .Returns(produtoViewModels);

            var controller = new ProdutosController(
                _notificadorMock.Object,
                _produtoRepositoryMock.Object,
                _produtoServiceMock.Object,
                _mapperMock.Object,
                _userMock.Object,
                _fornecedorRepositoryMock.Object
            );

            // Act
            var result = await controller.ObterTodos();

            // Assert
            // Verifique se o resultado é um OkObjectResult
            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            // Verifique se a propriedade "success" é verdadeira
            var successProperty = okResult.Value.GetType().GetProperty("success");
            Assert.NotNull(successProperty);
            var successValue = successProperty.GetValue(okResult.Value);
            Assert.IsType<bool>(successValue);
            Assert.True((bool)successValue);

            // Verifique se a propriedade "data" é do tipo correto (FornecedorViewModel)
            var dataProperty = okResult.Value.GetType().GetProperty("data");
            Assert.NotNull(dataProperty);
            var dataValue = dataProperty.GetValue(okResult.Value);
            var produtoViewModelList = Assert.IsAssignableFrom<IEnumerable<ProdutoViewModel>>(dataValue);
            Assert.Equal(produtos.Count(), produtoViewModelList.Count());
        }

        [Fact]
        public async Task ObterTodos_DeveRetornarListaVaziaQuandoNaoHaProdutos()
        {
            // Arrange
            var produtos = new List<Produto>(); // Lista vazia

            _produtoRepositoryMock.Setup(repository => repository.ObterProdutosFornecedores())
                                  .ReturnsAsync(produtos);

            var controller = new ProdutosController(
                _notificadorMock.Object,
                _produtoRepositoryMock.Object,
                _produtoServiceMock.Object,
                _mapperMock.Object,
                _userMock.Object,
                _fornecedorRepositoryMock.Object
            );

            // Act
            var result = await controller.ObterTodos();

            // Assert
            // Verifique se o resultado é um OkObjectResult
            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            // Verifique se a propriedade "success" é verdadeira
            var successProperty = okResult.Value.GetType().GetProperty("success");
            Assert.NotNull(successProperty);
            var successValue = successProperty.GetValue(okResult.Value);
            Assert.IsType<bool>(successValue);
            Assert.True((bool)successValue);

            // Verifique se a propriedade "data" é do tipo correto (FornecedorViewModel)
            var dataProperty = okResult.Value.GetType().GetProperty("data");
            Assert.NotNull(dataProperty);
            var dataValue = dataProperty.GetValue(okResult.Value);
            var produtoViewModelList = Assert.IsAssignableFrom<IEnumerable<ProdutoViewModel>>(dataValue);
            Assert.Empty(produtoViewModelList);
        }
    }
}
