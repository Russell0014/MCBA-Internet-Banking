using System.ComponentModel.DataAnnotations;
using System.Linq;
using AdminPortal.Models;
using Xunit;

namespace AdminPortal.Tests.Models;

public class LoginTests
{
    [Fact]
    public void UsernameDisplayAttribute_IsPresent()
    {
        var prop = typeof(Login).GetProperty(nameof(Login.UserName));
        var attr = prop.GetCustomAttributes(typeof(DisplayAttribute), false).Cast<DisplayAttribute>().FirstOrDefault();
        Assert.NotNull(attr);
        Assert.Equal("Username", attr.Name);
    }

    [Theory]
    [InlineData("user", "pass")]
    [InlineData("", "")] // allow empty credentials at model level (UI should validate)
    public void CanCreateLogin_WithValues(string user, string pass)
    {
        var l = new Login { UserName = user, Password = pass };
        Assert.Equal(user, l.UserName);
        Assert.Equal(pass, l.Password);
    }
}
