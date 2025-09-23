using APICore.API.Controllers;
using APICore.Common.DTO.Request;
using APICore.Data;
using APICore.Data.Entities;
using APICore.Data.Entities.Enums;
using APICore.Data.UoW;
using APICore.Services;
using APICore.Services.Exceptions;
using APICore.Services.Impls;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Moq;
using System.Threading.Tasks;
using Wangkanai.Detection.Models;
using Wangkanai.Detection.Services;
using Xunit;

namespace APICore.Tests.Integration.Account
{
    public class LoginAction
    {
        private DbContextOptions<CoreDbContext> ContextOptions { get; }
        private readonly Mock<IConfiguration> Config;
        private readonly Mock<IDetectionService> DetectionService;
        private readonly IStorageService storageService;

        public LoginAction()
        {
            ContextOptions = new DbContextOptionsBuilder<CoreDbContext>()
                                                   .UseInMemoryDatabase("TestLoginDatabase")
                                                   .Options;
            Config = new Mock<IConfiguration>();
            Config.Setup(setup => setup.GetSection("BearerTokens")["Issuer"]).Returns(@"http://apicore.com");
            // Use a test key with sufficient length (>= 32 chars) for HS256
            Config.Setup(setup => setup.GetSection("BearerTokens")["Key"]).Returns(@"very-long-test-key-that-is-at-least-32-chars!!!");
            Config.Setup(setup => setup.GetSection("BearerTokens")["Audience"]).Returns(@"Any");
            Config.Setup(setup => setup.GetSection("BearerTokens")["AccessTokenExpirationHours"]).Returns("7");
            Config.Setup(setup => setup.GetSection("BearerTokens")["RefreshTokenExpirationHours"]).Returns("60");

            DetectionService = new Mock<IDetectionService>();
            DetectionService.Setup(setup => setup.UserAgent).Returns(new UserAgent(@"Mozilla / 5.0(Windows NT 10.0; Win64; x64; rv: 86.0) Gecko / 20100101 Firefox / 86.0"));

            storageService = new Mock<IStorageService>().Object;

            SeedAsync().Wait();
        }

        private async Task SeedAsync()
        {
            await using var context = new CoreDbContext(ContextOptions);

            if (await context.Users.AnyAsync() == false)
            {
                await context.Users.AddAsync(new User
                {
                    Id = 1,
                    Email = "carlos@itguy.com",
                    FullName = "Carlos Delgado",
                    Gender = 0,
                    Phone = "+53 12345678",
                    Password = @"gM3vIavHvte3fimrk2uVIIoAB//f2TmRuTy4IWwNWp0=",
                    Status = StatusEnum.ACTIVE
                });

                await context.SaveChangesAsync();
            }
        }

    [Fact(DisplayName = "Successfully Login Should Return Ok Status Code (200)")]
    public async Task SuccessfullyLoginShouldReturnOk()
        {
            // ARRANGE
            var fakeLoginRequest = new LoginRequest(
                Email: "carlos@itguy.com",
                Password: "S3cretP@$$"
            );

            var httpContext = new DefaultHttpContext();
            using var context = new CoreDbContext(ContextOptions);

            var accountService = new AccountService(Config.Object, new UnitOfWork(context), new Mock<IStringLocalizer<IAccountService>>().Object, DetectionService.Object, storageService);
            var accountController = new AccountController(accountService, new Mock<AutoMapper.IMapper>().Object, new Mock<IEmailService>().Object, new Mock<IWebHostEnvironment>().Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

            // ACT
            var taskResult = (ObjectResult)await accountController.Login(fakeLoginRequest);

            // ASSERT
            Assert.Equal(200, taskResult.StatusCode);
        }

    [Fact(DisplayName = "Empty Email On Login Should Return Not Found Exception")]
    public async Task EmptyEmailOnLoginShouldReturnBadRequestException()
        {
            // ARRANGE
            var fakeLoginRequest = new LoginRequest(
                Email: "",
                Password: "S3cretP@$$"
            );

            var httpContext = new DefaultHttpContext();
            using var context = new CoreDbContext(ContextOptions);

            var accountService = new AccountService(Config.Object, new UnitOfWork(context), new Mock<IStringLocalizer<IAccountService>>().Object, DetectionService.Object, storageService);
            var accountController = new AccountController(accountService, new Mock<AutoMapper.IMapper>().Object, new Mock<IEmailService>().Object, new Mock<IWebHostEnvironment>().Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

            // ACT
            await Assert.ThrowsAsync<APICore.Services.Exceptions.UserNotFoundException>(() => accountController.Login(fakeLoginRequest));
        }

    [Fact(DisplayName = "Wrong Password Should Return Unauthorized Exception")]
    public async Task WrongPasswordShouldReturnUnauthorizedException()
        {
            // ARRANGE
            var fakeLoginRequest = new LoginRequest(
                Email: "carlos@itguy.com",
                Password: "Z3cretP@$$"
            );

            var httpContext = new DefaultHttpContext();
            using var context = new CoreDbContext(ContextOptions);

            var accountService = new AccountService(Config.Object, new UnitOfWork(context), new Mock<IStringLocalizer<IAccountService>>().Object, DetectionService.Object, storageService);
            var accountController = new AccountController(accountService, new Mock<AutoMapper.IMapper>().Object, new Mock<IEmailService>().Object, new Mock<IWebHostEnvironment>().Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

            // ACT
            await Assert.ThrowsAsync<APICore.Services.Exceptions.UnauthorizedException>(() => accountController.Login(fakeLoginRequest));
        }
    }
}
