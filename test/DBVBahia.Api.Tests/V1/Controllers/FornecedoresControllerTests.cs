using AutoMapper;
using DBVBahia.Api.V1.Controllers;
using DBVBahia.Api.ViewModels;
using DBVBahia.Business.Intefaces;
using DBVBahia.Business.Models;
using DBVBahia.Business.Notificacoes;
using k8s.KubeConfigModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBVBahia.Api.Tests.V1.Controllers
{
    public class FornecedoresControllerTests
    {
        private readonly Mock<IFornecedorRepository> _fornecedorRepositoryMock;
        private readonly Mock<IFornecedorService> _fornecedorServiceMock;
        private readonly Mock<IEnderecoRepository> _enderecoRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IUser> _userMock;
        private readonly Mock<INotificador> _notificadorMock;

        public FornecedoresControllerTests()
        {
            _fornecedorRepositoryMock = new Mock<IFornecedorRepository>();
            _fornecedorServiceMock = new Mock<IFornecedorService>();
            _enderecoRepositoryMock = new Mock<IEnderecoRepository>();
            _mapperMock = new Mock<IMapper>();
            _userMock = new Mock<IUser>();
            _notificadorMock = new Mock<INotificador>();
        }

        [Fact]
        public async Task Adicionar_DeveRetornarOkResultQuandoAdicaoBemSucedida()
        {
            // Arrange
            var fornecedorViewModel = new FornecedorViewModel
            {
                Id = Guid.NewGuid(),
                Documento = "12345",
                Ativo = true,
                Nome = "Jods",
                Produtos = new List<ProdutoViewModel>() { new ProdutoViewModel { Id = Guid.NewGuid() }, new ProdutoViewModel { Id = Guid.NewGuid() } },
                TipoFornecedor = (int)TipoFornecedor.PessoaJuridica,
                Endereco = new EnderecoViewModel { Id = Guid.NewGuid(), Logradouro = "Rua do test", Numero = "222", Cidade = "Aquela lá" }
            };

            _fornecedorServiceMock.Setup(service => service.Adicionar(It.IsAny<Fornecedor>()))
                                 .Returns(Task.CompletedTask);

            var controller = new FornecedoresController(
                _fornecedorRepositoryMock.Object,
                _mapperMock.Object,
                _fornecedorServiceMock.Object,
                _notificadorMock.Object,
                _enderecoRepositoryMock.Object,
                _userMock.Object
            );

            // Act
            var result = await controller.Adicionar(fornecedorViewModel);

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
            Assert.IsType<FornecedorViewModel>(dataValue);
        }

        [Fact]
        public async Task Atualizar_DeveRetornarOkResultQuandoAtualizacaoBemSucedida()
        {
            // Arrange
            var fornecedorViewModel = new FornecedorViewModel
            {
               Id = Guid.NewGuid(),
               Documento = "12345",
               Ativo = true,
               Nome = "Jods",
               Produtos = new List<ProdutoViewModel>() { new ProdutoViewModel { Id = Guid.NewGuid()}, new ProdutoViewModel { Id = Guid.NewGuid() } },
               TipoFornecedor = (int)TipoFornecedor.PessoaJuridica,
               Endereco = new EnderecoViewModel {Id = Guid.NewGuid(), Logradouro = "Rua do test", Numero = "222", Cidade = "Aquela lá" }
            };

            _fornecedorServiceMock.Setup(service => service.Atualizar(It.IsAny<Fornecedor>()))
                                 .Returns(Task.CompletedTask);

            var controller = new FornecedoresController(
                         _fornecedorRepositoryMock.Object,
                         _mapperMock.Object,
                         _fornecedorServiceMock.Object,
                         _notificadorMock.Object,
                         _enderecoRepositoryMock.Object,
                         _userMock.Object
            );

            // Act
            var result = await controller.Atualizar(fornecedorViewModel.Id, fornecedorViewModel);

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
            Assert.IsType<FornecedorViewModel>(dataValue);
        }

        [Fact]
        public async Task Excluir_DeveRetornarOkResultQuandoExclusaoBemSucedida()
        {
            // Arrange
            var fornecedorFromDB = new Fornecedor();

            var fornecedorViewModel = new FornecedorViewModel
            {
                Id = Guid.NewGuid(),
                Documento = "12345",
                Ativo = true,
                Nome = "Jods",
                Produtos = new List<ProdutoViewModel>() { new ProdutoViewModel { Id = Guid.NewGuid() }, new ProdutoViewModel { Id = Guid.NewGuid() } },
                TipoFornecedor = (int)TipoFornecedor.PessoaJuridica,
                Endereco = new EnderecoViewModel { Id = Guid.NewGuid(), Logradouro = "Rua do test", Numero = "222", Cidade = "Aquela lá" }
            };

            _fornecedorServiceMock
                .Setup(service => service.Remover(It.IsAny<Guid>()))                
                .Returns(Task.CompletedTask);

            _fornecedorRepositoryMock
                .Setup(service => service.ObterFornecedorEndereco(It.IsAny<Guid>()))
                .ReturnsAsync(fornecedorFromDB);

            _mapperMock
                .Setup(m => m.Map<FornecedorViewModel>(It.IsAny<Fornecedor>()))
                .Returns(fornecedorViewModel);

            var controller = new FornecedoresController(
                 _fornecedorRepositoryMock.Object,
                 _mapperMock.Object,
                 _fornecedorServiceMock.Object,
                 _notificadorMock.Object,
                 _enderecoRepositoryMock.Object,
                 _userMock.Object
             );

            // Act
            var result = await controller.Excluir(fornecedorViewModel.Id);

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
            Assert.IsType<FornecedorViewModel>(dataValue);
        }

        [Fact]
        public async Task ObterEnderecoPorId_DeveRetornarEnderecoViewModel()
        {
            // Arrange
            var enderecoId = Guid.NewGuid(); 
            var endereco = new Endereco { Id = enderecoId}; 

            _enderecoRepositoryMock
                .Setup(repository => repository.ObterPorId(enderecoId))
                .ReturnsAsync(endereco);

            var enderecoViewModel = new EnderecoViewModel { Id = endereco.Id}; 
            _mapperMock
                .Setup(mapper => mapper.Map<EnderecoViewModel>(endereco))
                .Returns(enderecoViewModel);

            var controller = new FornecedoresController(
                 _fornecedorRepositoryMock.Object,
                 _mapperMock.Object,
                 _fornecedorServiceMock.Object,
                 _notificadorMock.Object,
                 _enderecoRepositoryMock.Object,
                 _userMock.Object
             );

            // Act
            var result = await controller.ObterEnderecoPorId(enderecoId);

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
            var enderecoType = Assert.IsType<EnderecoViewModel>(dataValue);
            Assert.Equal(endereco.Id, enderecoType.Id);
        }

        [Fact]
        public async Task ObterEnderecoPorId_DeveRetornarNotFound()
        {
            // Arrange
            var enderecoId = Guid.NewGuid();

            var controller = new FornecedoresController(
                 _fornecedorRepositoryMock.Object,
                 _mapperMock.Object,
                 _fornecedorServiceMock.Object,
                 _notificadorMock.Object,
                 _enderecoRepositoryMock.Object,
                 _userMock.Object
             );

            // Act
            var result = await controller.ObterEnderecoPorId(enderecoId);

            // Assert
            // Verifique se o resultado é um NotFoundResult
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
