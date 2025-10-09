using MCBA.Data;
using MCBA.Models;
using MCBA.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MCBA.Tests.Services;

public class ProfileServiceTests : IDisposable
{
    private readonly DatabaseContext _context;
    private readonly ProfileService _service;

    public ProfileServiceTests()
    {
        _context = new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>().
            UseSqlite($"Data Source=file:{Guid.NewGuid()}?mode=memory&cache=shared").Options);

        _context.Database.EnsureCreated();

        _service = new ProfileService(_context);
    }

    [Fact]
    public void GetCustomer_ExistingCustomer_ReturnsCustomer()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = 2100,
            Name = "John Doe",
            TFN = "123 456 789",
            Address = "123 Main Street",
            City = "Melbourne",
            State = "VIC",
            PostCode = "3000",
            Mobile = "0412 345 678"
        };
        _context.Customers.Add(customer);
        _context.SaveChanges();

        // Act
        var result = _service.GetCustomer(2100);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2100, result.CustomerId);
        Assert.Equal("John Doe", result.Name);
        Assert.Equal("123 456 789", result.TFN);
        Assert.Equal("123 Main Street", result.Address);
        Assert.Equal("Melbourne", result.City);
        Assert.Equal("VIC", result.State);
        Assert.Equal("3000", result.PostCode);
        Assert.Equal("0412 345 678", result.Mobile);
    }

    [Fact]
    public void GetCustomer_NonExistingCustomer_ReturnsNull()
    {
        // Act
        var result = _service.GetCustomer(9999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void UpdateCustomerInfo_ExistingCustomer_UpdatesAllFields()
    {
        // Arrange
        var originalCustomer = new Customer
        {
            CustomerId = 2100,
            Name = "John Doe",
            TFN = "123 456 789",
            Address = "123 Main Street",
            City = "Melbourne",
            State = "VIC",
            PostCode = "3000",
            Mobile = "0412 345 678"
        };
        _context.Customers.Add(originalCustomer);
        _context.SaveChanges();

        var updatedCustomer = new Customer
        {
            CustomerId = 2100,
            Name = "Jane Smith",
            TFN = "987 654 321",
            Address = "456 Oak Avenue",
            City = "Sydney",
            State = "NSW",
            PostCode = "2000",
            Mobile = "0411 111 222"
        };

        // Act
        var result = _service.UpdateCustomerInfo(updatedCustomer);

        // Assert
        Assert.True(result);

        var customerInDb = _context.Customers.Find(2100);
        Assert.NotNull(customerInDb);
        Assert.Equal("Jane Smith", customerInDb.Name);
        Assert.Equal("987 654 321", customerInDb.TFN);
        Assert.Equal("456 Oak Avenue", customerInDb.Address);
        Assert.Equal("Sydney", customerInDb.City);
        Assert.Equal("NSW", customerInDb.State);
        Assert.Equal("2000", customerInDb.PostCode);
        Assert.Equal("0411 111 222", customerInDb.Mobile);
    }

    [Fact]
    public void UpdateCustomerInfo_NonExistingCustomer_ReturnsFalse()
    {
        // Arrange
        var updatedCustomer = new Customer
        {
            CustomerId = 9999,
            Name = "Non Existing",
            Address = "123 Fake Street"
        };

        // Act
        var result = _service.UpdateCustomerInfo(updatedCustomer);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void UpdateCustomerInfo_PartialUpdate_OnlyUpdatesProvidedFields()
    {
        // Arrange
        var originalCustomer = new Customer
        {
            CustomerId = 2100,
            Name = "John Doe",
            TFN = "123 456 789",
            Address = "123 Main Street",
            City = "Melbourne",
            State = "VIC",
            PostCode = "3000",
            Mobile = "0412 345 678"
        };
        _context.Customers.Add(originalCustomer);
        _context.SaveChanges();

        var partialUpdate = new Customer
        {
            CustomerId = 2100,
            Name = "Updated Name",
            Address = "Updated Address"
            // Other fields not provided, so they should be set to null
        };

        // Act
        var result = _service.UpdateCustomerInfo(partialUpdate);

        // Assert
        Assert.True(result);

        var customerInDb = _context.Customers.Find(2100);
        Assert.NotNull(customerInDb);
        Assert.Equal("Updated Name", customerInDb.Name);
        Assert.Equal("Updated Address", customerInDb.Address);
        // Other fields should be set to null since they weren't provided
        Assert.Null(customerInDb.TFN);
        Assert.Null(customerInDb.City);
        Assert.Null(customerInDb.State);
        Assert.Null(customerInDb.PostCode);
        Assert.Null(customerInDb.Mobile);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}