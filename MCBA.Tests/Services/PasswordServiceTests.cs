using MCBA.Data;
using MCBA.Models;
using MCBA.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using SimpleHashing.Net;
using Xunit;

namespace MCBA.Tests.Services;

public class PasswordServiceTests : IDisposable
{
    private readonly DatabaseContext _context;
    private readonly Mock<ISimpleHash> _mockHasher;
    private readonly PasswordService _service;

    public PasswordServiceTests()
    {
        _context = new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>().
            UseSqlite($"Data Source=file:{Guid.NewGuid()}?mode=memory&cache=shared").Options);

        _context.Database.EnsureCreated();

        _mockHasher = new Mock<ISimpleHash>();
        _service = new PasswordService(_context, _mockHasher.Object);
    }

    [Fact]
    public void GetLoginByCustomerId_ExistingCustomer_ReturnsLogin()
    {
        // Arrange
        var customer = new Customer { CustomerId = 2100, Name = "Test Customer" };
        _context.Customers.Add(customer);

        var login = new Login
        {
            LoginId = "12345678",
            CustomerId = 2100,
            PasswordHash = "hashedpassword"
        };
        _context.Logins.Add(login);
        _context.SaveChanges();

        // Act
        var result = _service.GetLoginByCustomerId(2100);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("12345678", result.LoginId);
        Assert.Equal(2100, result.CustomerId);
        Assert.Equal("hashedpassword", result.PasswordHash);
    }

    [Fact]
    public void GetLoginByCustomerId_NonExistingCustomer_ReturnsNull()
    {
        // Act
        var result = _service.GetLoginByCustomerId(9999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void VerifyPassword_ValidPassword_ReturnsTrue()
    {
        // Arrange
        var login = new Login
        {
            LoginId = "12345678",
            CustomerId = 2100,
            PasswordHash = "hashedpassword"
        };
        var password = "correctpassword";

        _mockHasher.Setup(h => h.Verify(password, login.PasswordHash)).Returns(true);

        // Act
        var result = _service.VerifyPassword(login, password);

        // Assert
        Assert.True(result);
        _mockHasher.Verify(h => h.Verify(password, login.PasswordHash), Times.Once);
    }

    [Fact]
    public void VerifyPassword_InvalidPassword_ReturnsFalse()
    {
        // Arrange
        var login = new Login
        {
            LoginId = "12345678",
            CustomerId = 2100,
            PasswordHash = "hashedpassword"
        };
        var password = "wrongpassword";

        _mockHasher.Setup(h => h.Verify(password, login.PasswordHash)).Returns(false);

        // Act
        var result = _service.VerifyPassword(login, password);

        // Assert
        Assert.False(result);
        _mockHasher.Verify(h => h.Verify(password, login.PasswordHash), Times.Once);
    }

    [Fact]
    public void UpdatePassword_UpdatesPasswordHash()
    {
        // Arrange
        var customer = new Customer { CustomerId = 2100, Name = "Test Customer" };
        _context.Customers.Add(customer);

        var login = new Login
        {
            LoginId = "12345678",
            CustomerId = 2100,
            PasswordHash = "oldhash"
        };
        _context.Logins.Add(login);
        _context.SaveChanges();

        var newPassword = "newpassword";
        var newHash = "newhashedpassword";

        _mockHasher.Setup(h => h.Compute(newPassword)).Returns(newHash);

        // Act
        _service.UpdatePassword(login, newPassword);

        // Assert
        Assert.Equal(newHash, login.PasswordHash);
        _mockHasher.Verify(h => h.Compute(newPassword), Times.Once);

        var updatedLogin = _context.Logins.Find(login.LoginId);
        Assert.NotNull(updatedLogin);
        Assert.Equal(newHash, updatedLogin.PasswordHash);
    }

    [Fact]
    public void UpdatePassword_SavesChangesToDatabase()
    {
        // Arrange
        var customer = new Customer { CustomerId = 2100, Name = "Test Customer" };
        _context.Customers.Add(customer);

        var login = new Login
        {
            LoginId = "12345678",
            CustomerId = 2100,
            PasswordHash = "oldhash"
        };
        _context.Logins.Add(login);
        _context.SaveChanges();

        var newPassword = "newpassword";
        var newHash = "newhashedpassword";

        _mockHasher.Setup(h => h.Compute(newPassword)).Returns(newHash);

        // Act
        _service.UpdatePassword(login, newPassword);

        // Assert
        var updatedLogin = _context.Logins.Find(login.LoginId);
        Assert.NotNull(updatedLogin);
        Assert.Equal(newHash, updatedLogin.PasswordHash);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}