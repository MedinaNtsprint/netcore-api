using APICore.API.Controllers;
using APICore.Common.DTO.Request;
using APICore.Data;
using APICore.Data.Entities;
using APICore.Data.Entities.Enums;
using APICore.Data.UoW;
using APICore.Services;
using APICore.Services.Exceptions;
using APICore.Services.Impls;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Moq;
using System;
using System.Threading.Tasks;
using Wangkanai.Detection.Services;
using Xunit;

namespace APICore.Tests.Integration.Account
{
    public class RegisterAction
    {
        private DbContextOptions<CoreDbContext> ContextOptions { get; }
        private readonly IStorageService storageService;

        public RegisterAction()
        {
            ContextOptions = new DbContextOptionsBuilder<CoreDbContext>()
                                       .UseInMemoryDatabase("TestRegisterDatabase")
                                       .Options;
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
                    Id = 3,
                    Email = "pepe@itguy.com",
                    FullName = "Pepe Delgado",
                    Gender = 0,
                    Phone = "+53 12345678",
                    Password = @"gM3vIavHvte3fimrk2uVIIoAB//f2TmRuTy4IWwNWp0=",
                    Status = StatusEnum.ACTIVE
                });

                await context.SaveChangesAsync();
            }
        }

    [Fact(DisplayName = "Successfully Register Should Return Created Status Code (201)")]
    public async Task SuccessfullyRegisterShouldReturnCreated()
        {
            // ARRANGE
            var fakeUserRequest = new SignUpRequest(
                FullName: "Carlos Perez",
                Password: @"S3cretP@$$",
                ConfirmationPassword: @"S3cretP@$$",
                Email: @"carlos@itguy.com",
                Birthday: DateTime.Now,
                Phone: "+53 12345678",
                Gender: 0
            );

            using var context = new CoreDbContext(ContextOptions);
            var accountService = new AccountService(new Mock<IConfiguration>().Object, new UnitOfWork(context), new Mock<IStringLocalizer<IAccountService>>().Object, new Mock<IDetectionService>().Object, storageService);
            var accountController = new AccountController(accountService, new Mock<AutoMapper.IMapper>().Object, new Mock<IEmailService>().Object, new Mock<IWebHostEnvironment>().Object);

            // ACT
            var taskResult = (ObjectResult)await accountController.Register(fakeUserRequest);

            // ASSERT
            Assert.Equal(201, taskResult.StatusCode);
        }

    [Fact(DisplayName = "Empty Email Should Return Bad Request Exception")]
    public async Task EmptyEmailShouldReturnBadRequestException()
        {
            // ARRANGE
            var fakeUserRequest = new SignUpRequest(
                FullName: "Pepe Perez",
                Password: @"S3cretP@$$",
                ConfirmationPassword: @"S3cretP@$$",
                Email: "",
                Birthday: DateTime.Now,
                Phone: "+53 12345678",
                Gender: 0
            );

            using var context = new CoreDbContext(ContextOptions);
            var accountService = new AccountService(new Mock<IConfiguration>().Object, new UnitOfWork(context), new Mock<IStringLocalizer<IAccountService>>().Object, new Mock<IDetectionService>().Object, storageService);
            var accountController = new AccountController(accountService, new Mock<AutoMapper.IMapper>().Object, new Mock<IEmailService>().Object, new Mock<IWebHostEnvironment>().Object);

            // ACT & ASSERT
            await Assert.ThrowsAsync<APICore.Services.Exceptions.EmptyEmailBadRequestException>(() => accountController.Register(fakeUserRequest));
        }

    [Fact(DisplayName = "Email In Use Should Return Bad Request Exception")]
    public async Task EmailInUseShouldReturnBadRequestException()
        {
            // ARRANGE
            var fakeUserRequest = new SignUpRequest(
                FullName: "Any Name",
                Password: @"S3cretP@$$",
                ConfirmationPassword: @"S3cretP@$$",
                Email: "pepe@itguy.com",
                Birthday: DateTime.Now,
                Phone: "+53 00000000",
                Gender: 0
            );

            using var context = new CoreDbContext(ContextOptions);
            var accountService = new AccountService(new Mock<IConfiguration>().Object, new UnitOfWork(context), new Mock<IStringLocalizer<IAccountService>>().Object, new Mock<IDetectionService>().Object, storageService);
            var accountController = new AccountController(accountService, new Mock<AutoMapper.IMapper>().Object, new Mock<IEmailService>().Object, new Mock<IWebHostEnvironment>().Object);

            // ACT & ASSERT
            await Assert.ThrowsAsync<APICore.Services.Exceptions.EmailInUseBadRequestException>(() => accountController.Register(fakeUserRequest));
        }

    [Fact(DisplayName = "Empty Password Should Return Bad Request Exception")]
    public async Task EmptyPasswordShouldReturnBadRequestException()
        {
            // ARRANGE
            var fakeUserRequest = new SignUpRequest(
                FullName: "Pepe Perez",
                Password: "",
                ConfirmationPassword: @"S3cretP@$$",
                Email: @"pepe2@itguy.com",
                Birthday: DateTime.Now,
                Phone: "+53 12345678",
                Gender: 0
            );

            using var context = new CoreDbContext(ContextOptions);
            var accountService = new AccountService(new Mock<IConfiguration>().Object, new UnitOfWork(context), new Mock<IStringLocalizer<IAccountService>>().Object, new Mock<IDetectionService>().Object, storageService);
            var accountController = new AccountController(accountService, new Mock<AutoMapper.IMapper>().Object, new Mock<IEmailService>().Object, new Mock<IWebHostEnvironment>().Object);

            // ACT & ASSERT
            await Assert.ThrowsAsync<APICore.Services.Exceptions.PasswordRequirementsBadRequestException>(() => accountController.Register(fakeUserRequest));
        }

    [Fact(DisplayName = "Small Password Should Return Bad Request Exception")]
    public async Task SmallPasswordShouldReturnBadRequestException()
        {
            // ARRANGE
            var fakeUserRequest = new SignUpRequest(
                FullName: "Pepe Perez",
                Password: "S3cr",
                ConfirmationPassword: @"S3cretP@$$",
                Email: "pepe2@itguy.com",
                Birthday: DateTime.Now,
                Phone: "+53 12345678",
                Gender: 0
            );

            using var context = new CoreDbContext(ContextOptions);
            var accountService = new AccountService(new Mock<IConfiguration>().Object, new UnitOfWork(context), new Mock<IStringLocalizer<IAccountService>>().Object, new Mock<IDetectionService>().Object, storageService);
            var accountController = new AccountController(accountService, new Mock<AutoMapper.IMapper>().Object, new Mock<IEmailService>().Object, new Mock<IWebHostEnvironment>().Object);

            // ACT & ASSERT
            await Assert.ThrowsAsync<APICore.Services.Exceptions.PasswordRequirementsBadRequestException>(() => accountController.Register(fakeUserRequest));
        }

    [Fact(DisplayName = "Passwords Doesn't Match Should Return Bad Request Exception")]
    public async Task PasswordDoesntMatchShouldReturnBadRequestException()
        {
            // ARRANGE
            var fakeUserRequest = new SignUpRequest(
                FullName: "Pepe Perez",
                Password: @"Z3cretP@$$",
                ConfirmationPassword: @"S3cretP@$$",
                Email: "pepe2@itguy.com",
                Birthday: DateTime.Now,
                Phone: "+53 12345678",
                Gender: 0
            );

            using var context = new CoreDbContext(ContextOptions);
            var accountService = new AccountService(new Mock<IConfiguration>().Object, new UnitOfWork(context), new Mock<IStringLocalizer<IAccountService>>().Object, new Mock<IDetectionService>().Object, storageService);
            var accountController = new AccountController(accountService, new Mock<AutoMapper.IMapper>().Object, new Mock<IEmailService>().Object, new Mock<IWebHostEnvironment>().Object);

            // ACT & ASSERT
            await Assert.ThrowsAsync<APICore.Services.Exceptions.PasswordsDoesntMatchBadRequestException>(() => accountController.Register(fakeUserRequest));
        }
    }
}
