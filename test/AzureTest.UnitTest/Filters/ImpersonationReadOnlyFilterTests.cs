using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Infrastructure.Configuration;
using AzureTest.MVC.Filters;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AzureTest.UnitTest.Filters;

public class ImpersonationReadOnlyFilterTests
{
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly Mock<ILogger<ImpersonationReadOnlyFilter>> _loggerMock;

    public ImpersonationReadOnlyFilterTests()
    {
        _tenantProviderMock = new Mock<ITenantProvider>();
        _loggerMock = new Mock<ILogger<ImpersonationReadOnlyFilter>>();
    }

    private ImpersonationReadOnlyFilter CreateFilter(bool allowImpersonationWrites)
    {
        var options = Options.Create(new InfrastructureFeatureOptions
        {
            EnableMultiTenancy = true,
            AllowImpersonationWrites = allowImpersonationWrites
        });

        return new ImpersonationReadOnlyFilter(_tenantProviderMock.Object, options, _loggerMock.Object);
    }

    [Fact]
    public async Task When_AllowImpersonationWrites_IsTrue_And_AdminIsImpersonating_PostRequest_ShouldBeAllowed()
    {
        // Arrange
        var filter = CreateFilter(allowImpersonationWrites: true);
        _tenantProviderMock.Setup(t => t.IsAdminImpersonating).Returns(true);

        var (context, nextMock) = CreateActionExecutingContext("POST", "Debt", "Create", new TestDebtController());

        // Act
        await filter.OnActionExecutionAsync(context, nextMock.Object);

        // Assert
        nextMock.Verify(n => n(), Times.Once);
        context.Result.Should().BeNull();
    }

    [Fact]
    public async Task When_AllowImpersonationWrites_IsFalse_And_AdminIsNotImpersonating_PostRequest_ShouldBeAllowed()
    {
        // Arrange
        var filter = CreateFilter(allowImpersonationWrites: false);
        _tenantProviderMock.Setup(t => t.IsAdminImpersonating).Returns(false);

        var (context, nextMock) = CreateActionExecutingContext("POST", "Debt", "Create", new TestDebtController());

        // Act
        await filter.OnActionExecutionAsync(context, nextMock.Object);

        // Assert
        nextMock.Verify(n => n(), Times.Once);
        context.Result.Should().BeNull();
    }

    [Fact]
    public async Task When_AllowImpersonationWrites_IsFalse_And_AdminIsImpersonating_GetRequest_ShouldBeAllowed()
    {
        // Arrange
        var filter = CreateFilter(allowImpersonationWrites: false);
        _tenantProviderMock.Setup(t => t.IsAdminImpersonating).Returns(true);

        var (context, nextMock) = CreateActionExecutingContext("GET", "Debt", "Index", new TestDebtController());

        // Act
        await filter.OnActionExecutionAsync(context, nextMock.Object);

        // Assert
        nextMock.Verify(n => n(), Times.Once);
        context.Result.Should().BeNull();
    }

    [Fact]
    public async Task When_AllowImpersonationWrites_IsFalse_And_AdminIsImpersonating_TenantController_ShouldBeExempt()
    {
        // Arrange
        var filter = CreateFilter(allowImpersonationWrites: false);
        _tenantProviderMock.Setup(t => t.IsAdminImpersonating).Returns(true);

        var (context, nextMock) = CreateActionExecutingContext("POST", "Tenant", "SwitchTenant", new TestTenantController());

        // Act
        await filter.OnActionExecutionAsync(context, nextMock.Object);

        // Assert: TenantController is exempt so admin can switch or reset tenant
        nextMock.Verify(n => n(), Times.Once);
        context.Result.Should().BeNull();
    }

    [Fact]
    public async Task When_AllowImpersonationWrites_IsFalse_And_AdminIsImpersonating_AuthController_ShouldBeExempt()
    {
        // Arrange
        var filter = CreateFilter(allowImpersonationWrites: false);
        _tenantProviderMock.Setup(t => t.IsAdminImpersonating).Returns(true);

        var (context, nextMock) = CreateActionExecutingContext("POST", "Auth", "Logout", new TestAuthController());

        // Act
        await filter.OnActionExecutionAsync(context, nextMock.Object);

        // Assert: AuthController is exempt so admin can logout safely
        nextMock.Verify(n => n(), Times.Once);
        context.Result.Should().BeNull();
    }

    [Fact]
    public async Task When_AllowImpersonationWrites_IsFalse_And_AdminIsImpersonating_ExplicitAttribute_ShouldBeExempt()
    {
        // Arrange
        var filter = CreateFilter(allowImpersonationWrites: false);
        _tenantProviderMock.Setup(t => t.IsAdminImpersonating).Returns(true);

        var (context, nextMock) = CreateActionExecutingContext("POST", "Debt", "SpecialActionWithAttribute", new TestDebtController());

        // Act
        await filter.OnActionExecutionAsync(context, nextMock.Object);

        // Assert: Action marked with [AllowImpersonationWrite] is permitted
        nextMock.Verify(n => n(), Times.Once);
        context.Result.Should().BeNull();
    }

    [Fact]
    public async Task When_AllowImpersonationWrites_IsFalse_And_AdminIsImpersonating_StandardMvcPost_ShouldBeBlocked_AndSetTempData()
    {
        // Arrange
        var filter = CreateFilter(allowImpersonationWrites: false);
        _tenantProviderMock.Setup(t => t.IsAdminImpersonating).Returns(true);
        _tenantProviderMock.Setup(t => t.RealUserId).Returns(Guid.NewGuid());
        _tenantProviderMock.Setup(t => t.CurrentTenantId).Returns(Guid.NewGuid());

        var controller = new TestDebtController();
        var (context, nextMock) = CreateActionExecutingContext("POST", "Debt", "Create", controller);
        context.HttpContext.Request.Headers.Referer = "http://localhost/Debt/Index";

        // Act
        await filter.OnActionExecutionAsync(context, nextMock.Object);

        // Assert
        nextMock.Verify(n => n(), Times.Never);
        var redirectResult = context.Result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("ReadOnlyImpersonation");
        redirectResult.ControllerName.Should().Be("Error");
        controller.TempData["Error"].Should().Be(ImpersonationReadOnlyFilter.ReadOnlyErrorMessage);
    }

    [Fact]
    public async Task When_AllowImpersonationWrites_IsFalse_And_AdminIsImpersonating_AjaxPost_ShouldBeBlocked_AndReturn403Json()
    {
        // Arrange
        var filter = CreateFilter(allowImpersonationWrites: false);
        _tenantProviderMock.Setup(t => t.IsAdminImpersonating).Returns(true);
        _tenantProviderMock.Setup(t => t.RealUserId).Returns(Guid.NewGuid());
        _tenantProviderMock.Setup(t => t.CurrentTenantId).Returns(Guid.NewGuid());

        var (context, nextMock) = CreateActionExecutingContext("POST", "Debt", "Create", new TestDebtController());
        context.HttpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";

        // Act
        await filter.OnActionExecutionAsync(context, nextMock.Object);

        // Assert
        nextMock.Verify(n => n(), Times.Never);
        var jsonResult = context.Result.Should().BeOfType<JsonResult>().Subject;
        jsonResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    private static (ActionExecutingContext Context, Mock<ActionExecutionDelegate> NextMock) CreateActionExecutingContext(
        string httpMethod,
        string controllerName,
        string actionName,
        Controller controller)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = httpMethod;
        httpContext.Request.Host = new HostString("localhost");
        httpContext.Request.Scheme = "http";
        httpContext.Request.Path = $"/{controllerName}/{actionName}";

        var routeData = new RouteData();
        routeData.Values["controller"] = controllerName;
        routeData.Values["action"] = actionName;

        var methodInfo = controller.GetType().GetMethod(actionName) ??
                         controller.GetType().GetMethod("Index") ??
                         typeof(TestDebtController).GetMethod("Create")!;

        var actionDescriptor = new ControllerActionDescriptor
        {
            ControllerName = controllerName,
            ActionName = actionName,
            ControllerTypeInfo = controller.GetType().GetTypeInfo(),
            MethodInfo = methodInfo
        };

        var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

        controller.ControllerContext = new ControllerContext(actionContext);
        controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            controller);

        var nextMock = new Mock<ActionExecutionDelegate>();
        nextMock.Setup(n => n()).ReturnsAsync(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), controller));

        return (context, nextMock);
    }

    private class TestDebtController : Controller
    {
        public IActionResult Index() => Ok();
        public IActionResult Create() => Ok();

        [AllowImpersonationWrite]
        public IActionResult SpecialActionWithAttribute() => Ok();
    }

    private class TestTenantController : Controller
    {
        public IActionResult SwitchTenant() => Ok();
        public IActionResult ResetTenant() => Ok();
    }

    private class TestAuthController : Controller
    {
        public IActionResult Logout() => Ok();
    }
}
