using DBVBahia.Api.Extensions;
using DBVBahia.Api.V1.Controllers;
using DBVBahia.Api.ViewModels;
using DBVBahia.Business.Intefaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace DBVBahia.Api.Tests.V1.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<SignInManager<IdentityUser>> _signInManager;
        private readonly Mock<UserManager<IdentityUser>> _userManager;
        private readonly Mock<ILogger<AuthController>> _logger;
        private readonly Mock<INotificador> _notificador;
        private readonly Mock<IUser> _user;
        private readonly Mock<IOptions<AppSettings>> _appSettings;

        public AuthControllerTests()
        {
            _userManager = MockUserManager<IdentityUser>();
            _signInManager = new Mock<SignInManager<IdentityUser>>();
            _appSettings = new Mock<IOptions<AppSettings>>();
            _logger = new Mock<ILogger<AuthController>>();
            _notificador = new Mock<INotificador>();
            _user = new Mock<IUser>();
        }

        [Fact(Skip = "Problema com o SignInManager")]
        public async Task Registrar_DeveRetornarOkResultQuandoRegistroBemSucedido()
        {
            // Arrange
            _userManager.Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                           .ReturnsAsync(IdentityResult.Success);

            _signInManager.Setup(sm => sm.SignInAsync(It.IsAny<IdentityUser>(), false, null))
                             .Returns(Task.FromResult<object>(null));

            var authController = new AuthController(_notificador.Object, NewSignInManager(_userManager.Object), _userManager.Object,
                _appSettings.Object, _user.Object, _logger.Object);

            var registerUser = new RegisterUserViewModel
            {
                Email = "test@example.com",
                Password = "123456"
            };

            // Act
            var result = await authController.Registrar(registerUser);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var loginResponseViewModel = Assert.IsType<LoginResponseViewModel>(okResult.Value);
        }

        // Método auxiliar para criar um mock do UserManager
        private Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            return new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        // Método auxiliar para criar um mock do SignInManager
        private SignInManager<TUser> NewSignInManager<TUser>(UserManager<TUser> userManager) where TUser : class
        {
            var httpContext = new DefaultHttpContext();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<TUser>>();
            var options = new Mock<IOptions<IdentityOptions>>();
            var logger = new Mock<ILogger<SignInManager<TUser>>>();
            var scheme = new Mock<IAuthenticationSchemeProvider>();
            var confirmation = new Mock<IUserConfirmation<TUser>>();

            return new SignInManager<TUser>(
                userManager,
                httpContextAccessorMock.Object,
                claimsFactory.Object,
                options.Object,
                logger.Object,
                scheme.Object,
                confirmation.Object
            );
        }
    }
}