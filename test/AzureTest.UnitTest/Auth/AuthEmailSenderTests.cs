using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Auth.Commands.ConfirmEmail;
using AzureTest.Application.Auth.Commands.ForgotPassword;
using AzureTest.Application.Auth.Commands.Register;
using AzureTest.Application.Auth.Commands.ResetPassword;
using AzureTest.Application.Auth.Services;
using AzureTest.Application.IdentitySettings;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;
using AzureTest.Domain.Entities.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AzureTest.UnitTest.Auth;

public class AuthEmailSenderTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IEmailSender<ApplicationUser>> _emailSenderMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IAuthSessionService> _authSessionServiceMock;
    private readonly Mock<IQueryHandler<GetIdentitySettingQuery, Result<IdentitySetting>>> _identitySettingQueryMock;

    public AuthEmailSenderTests()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _emailSenderMock = new Mock<IEmailSender<ApplicationUser>>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _authSessionServiceMock = new Mock<IAuthSessionService>();
        _identitySettingQueryMock = new Mock<IQueryHandler<GetIdentitySettingQuery, Result<IdentitySetting>>>();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("example.com");
        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(httpContext);

        _identitySettingQueryMock.Setup(q => q.HandleAsync(It.IsAny<GetIdentitySettingQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentitySetting>.Success(new IdentitySetting()));
    }

    [Fact]
    public async Task RegisterCommandHandler_WhenUserIsCreatedAndEmailNotConfirmed_ShouldSendConfirmationLink()
    {
        // Arrange
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("raw-token-123");

        string capturedLink = string.Empty;
        _emailSenderMock.Setup(e => e.SendConfirmationLinkAsync(It.IsAny<ApplicationUser>(), "newuser@test.com", It.IsAny<string>()))
            .Callback<ApplicationUser, string, string>((u, email, link) => capturedLink = link)
            .Returns(Task.CompletedTask);

        var loggerMock = new Mock<ILogger<RegisterCommandHandler>>();
        var handler = new RegisterCommandHandler(
            _userManagerMock.Object,
            _authSessionServiceMock.Object,
            _identitySettingQueryMock.Object,
            _emailSenderMock.Object,
            _httpContextAccessorMock.Object,
            loggerMock.Object);

        var command = new RegisterCommand("Jane Doe", "newuser@test.com", "Password123!", "Password123!");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Succeeded.Should().BeTrue();
        result.Value.ConfirmationEmailSent.Should().BeTrue();

        _emailSenderMock.Verify(e => e.SendConfirmationLinkAsync(
            It.Is<ApplicationUser>(u => u.Email == "newuser@test.com"),
            "newuser@test.com",
            It.IsAny<string>()), Times.Once);

        capturedLink.Should().Contain("https://example.com/Auth/ConfirmEmail");
        capturedLink.Should().Contain("userId=");
        capturedLink.Should().Contain("code=");
    }

    [Fact]
    public async Task ForgotPasswordCommandHandler_WhenUserExists_ShouldGenerateTokenAndSendPasswordResetLink()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "user@test.com",
            UserName = "user@test.com"
        };

        _userManagerMock.Setup(m => m.FindByEmailAsync("user@test.com"))
            .ReturnsAsync(user);

        _userManagerMock.Setup(m => m.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset-token-abc");

        string capturedLink = string.Empty;
        _emailSenderMock.Setup(e => e.SendPasswordResetLinkAsync(user, "user@test.com", It.IsAny<string>()))
            .Callback<ApplicationUser, string, string>((u, email, link) => capturedLink = link)
            .Returns(Task.CompletedTask);

        var loggerMock = new Mock<ILogger<ForgotPasswordCommandHandler>>();
        var handler = new ForgotPasswordCommandHandler(
            _userManagerMock.Object,
            _emailSenderMock.Object,
            _httpContextAccessorMock.Object,
            loggerMock.Object);

        var command = new ForgotPasswordCommand("user@test.com");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Success.Should().BeTrue();
        result.Value.ResetToken.Should().Be("reset-token-abc");

        _emailSenderMock.Verify(e => e.SendPasswordResetLinkAsync(user, "user@test.com", It.IsAny<string>()), Times.Once);
        capturedLink.Should().Contain("https://example.com/Auth/ResetPassword");
        capturedLink.Should().Contain($"userId={user.Id}");
    }

    [Fact]
    public async Task ConfirmEmailCommandHandler_WhenValidToken_ShouldConfirmEmailSuccessfully()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "confirm@test.com",
            EmailConfirmed = false
        };

        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        var rawToken = "my-confirm-token";
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

        _userManagerMock.Setup(m => m.ConfirmEmailAsync(user, rawToken))
            .ReturnsAsync(IdentityResult.Success);

        var loggerMock = new Mock<ILogger<ConfirmEmailCommandHandler>>();
        var handler = new ConfirmEmailCommandHandler(_userManagerMock.Object, loggerMock.Object);

        var command = new ConfirmEmailCommand(user.Id.ToString(), encodedToken);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Succeeded.Should().BeTrue();
        result.Value.Message.Should().Contain("confirmed successfully");
    }

    [Fact]
    public async Task ResetPasswordCommandHandler_WhenValidToken_ShouldResetPasswordSuccessfully()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "reset@test.com"
        };

        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        var rawToken = "my-reset-token";
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

        _userManagerMock.Setup(m => m.ResetPasswordAsync(user, rawToken, "NewSecurePassword123!"))
            .ReturnsAsync(IdentityResult.Success);

        var loggerMock = new Mock<ILogger<ResetPasswordCommandHandler>>();
        var handler = new ResetPasswordCommandHandler(_userManagerMock.Object, loggerMock.Object);

        var command = new ResetPasswordCommand(user.Id.ToString(), encodedToken, "NewSecurePassword123!", "NewSecurePassword123!");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Succeeded.Should().BeTrue();
        result.Value.Message.Should().Contain("reset successfully");
    }
}
