using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Users.Commands.AdminResetPassword;
using AzureTest.Domain.Entities.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AzureTest.UnitTest.Users;

public class AdminResetPasswordTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ILogger<AdminResetPasswordCommandHandler>> _loggerMock;
    private readonly AdminResetPasswordCommandHandler _handler;

    public AdminResetPasswordTests()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _loggerMock = new Mock<ILogger<AdminResetPasswordCommandHandler>>();

        _handler = new AdminResetPasswordCommandHandler(_userManagerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task When_UserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        var command = new AdminResetPasswordCommand(userId, "NewPass123!", "NewPass123!");

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("User not found.");
    }

    [Fact]
    public async Task When_UserHasExistingPassword_ShouldGenerateTokenAndResetPasswordSuccessfully()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "user@test.com",
            UserName = "user@test.com"
        };

        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(m => m.HasPasswordAsync(user))
            .ReturnsAsync(true);

        _userManagerMock.Setup(m => m.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset-token-xyz");

        _userManagerMock.Setup(m => m.ResetPasswordAsync(user, "reset-token-xyz", "NewSecurePassword123!"))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(m => m.ResetAccessFailedCountAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(m => m.UpdateSecurityStampAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var command = new AdminResetPasswordCommand(user.Id, "NewSecurePassword123!", "NewSecurePassword123!");

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Success.Should().BeTrue();
        result.Errors.Should().BeEmpty();

        _userManagerMock.Verify(m => m.ResetPasswordAsync(user, "reset-token-xyz", "NewSecurePassword123!"), Times.Once);
        _userManagerMock.Verify(m => m.ResetAccessFailedCountAsync(user), Times.Once);
        _userManagerMock.Verify(m => m.UpdateSecurityStampAsync(user), Times.Once);
    }

    [Fact]
    public async Task When_UserHasNoPassword_ShouldAddPasswordSuccessfully()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "oauth@test.com",
            UserName = "oauth@test.com"
        };

        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(m => m.HasPasswordAsync(user))
            .ReturnsAsync(false);

        _userManagerMock.Setup(m => m.AddPasswordAsync(user, "BrandNewPassword123!"))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(m => m.ResetAccessFailedCountAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(m => m.UpdateSecurityStampAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var command = new AdminResetPasswordCommand(user.Id, "BrandNewPassword123!", "BrandNewPassword123!");

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Success.Should().BeTrue();
        result.Errors.Should().BeEmpty();

        _userManagerMock.Verify(m => m.AddPasswordAsync(user, "BrandNewPassword123!"), Times.Once);
        _userManagerMock.Verify(m => m.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
        _userManagerMock.Verify(m => m.ResetAccessFailedCountAsync(user), Times.Once);
    }

    [Fact]
    public async Task When_IdentityReturnsErrors_ShouldReturnFailureWithDescriptions()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "user@test.com"
        };

        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(m => m.HasPasswordAsync(user))
            .ReturnsAsync(true);

        _userManagerMock.Setup(m => m.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("token");

        var identityErrors = new[]
        {
            new IdentityError { Code = "PasswordTooShort", Description = "Passwords must be at least 6 characters." },
            new IdentityError { Code = "PasswordRequiresDigit", Description = "Passwords must have at least one digit ('0'-'9')." }
        };

        _userManagerMock.Setup(m => m.ResetPasswordAsync(user, "token", "weak"))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        var command = new AdminResetPasswordCommand(user.Id, "weak", "weak");

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain("Passwords must be at least 6 characters.");
        result.Errors.Should().Contain("Passwords must have at least one digit ('0'-'9').");
    }

    [Theory]
    [InlineData("", "Password123!", "Password123!", false)]
    [InlineData("00000000-0000-0000-0000-000000000000", "Password123!", "Password123!", false)]
    [InlineData("e804f86d-f421-4f9e-a89c-4318c5e93347", "123", "123", false)]
    [InlineData("e804f86d-f421-4f9e-a89c-4318c5e93347", "Password123!", "Different123!", false)]
    [InlineData("e804f86d-f421-4f9e-a89c-4318c5e93347", "Password123!", "Password123!", true)]
    public void Validator_ShouldValidateCommandCorrectly(string userIdStr, string newPassword, string confirmPassword, bool expectedValid)
    {
        var validator = new AdminResetPasswordCommandValidator();
        var userId = string.IsNullOrEmpty(userIdStr) ? Guid.Empty : Guid.Parse(userIdStr);
        var command = new AdminResetPasswordCommand(userId, newPassword, confirmPassword);

        var validationResult = validator.Validate(command);

        validationResult.IsValid.Should().Be(expectedValid);
    }
}
