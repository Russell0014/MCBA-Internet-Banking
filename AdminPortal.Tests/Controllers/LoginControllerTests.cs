using AdminPortal.Controllers;
using AdminPortal.Models;
using AdminPortal.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace AdminPortal.Tests.Controllers;

public class LoginControllerTests
{
    private static LoginController CreateControllerWithSession(SimpleSession session)
    {
        var controller = new LoginController();
        var httpContext = new DefaultHttpContext();
        httpContext.Session = session;
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    [Fact]
    public void LoginGet_WhenAlreadyAdmin_RedirectsToPayeeIndex()
    {
        var session = new SimpleSession();
        session.Set("IsAdmin", System.Text.Encoding.UTF8.GetBytes("true"));
        var controller = CreateControllerWithSession(session);

        var result = controller.Login();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Payee", redirect.ControllerName);
    }

    [Fact]
    public void LoginPost_InvalidCredentials_ReturnsViewWithModelError()
    {
        var session = new SimpleSession();
        var controller = CreateControllerWithSession(session);

        var result = controller.Login("wrong", "creds");

        var view = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Contains("LoginFailed", controller.ModelState.Keys);
        Assert.IsType<Login>(view.Model);
    }

    [Fact]
    public void LoginPost_ValidCredentials_SetsSessionAndRedirects()
    {
        var session = new SimpleSession();
        var controller = CreateControllerWithSession(session);

        var result = controller.Login("admin", "admin");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Payee", redirect.ControllerName);

        Assert.True(session.TryGetValue("IsAdmin", out var bytes));
        var val = System.Text.Encoding.UTF8.GetString(bytes);
        Assert.Equal("true", val);
    }

    [Fact]
    public void Logout_ClearsSessionAndRedirectsToLogin()
    {
        var session = new SimpleSession();
        session.Set("IsAdmin", System.Text.Encoding.UTF8.GetBytes("true"));
        var controller = CreateControllerWithSession(session);

        var result = controller.Logout();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirect.ActionName);
        Assert.Equal("Login", redirect.ControllerName);
        Assert.False(session.TryGetValue("IsAdmin", out _));
    }
}
