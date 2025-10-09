using MCBA.Models;
using Xunit;

namespace MCBA.Tests.Models;

public class ErrorViewModelTests
{
    [Fact]
    public void ErrorViewModel_CanBeCreated_WithDefaultValues()
    {
        // Arrange & Act
        var errorViewModel = new ErrorViewModel();

        // Assert
        Assert.Null(errorViewModel.RequestId);
        Assert.False(errorViewModel.ShowRequestId);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("abc-123-def")]
    [InlineData("test-request-id")]
    public void RequestId_CanBeSetToValidStrings(string requestId)
    {
        // Arrange
        var errorViewModel = new ErrorViewModel();

        // Act
        errorViewModel.RequestId = requestId;

        // Assert
        Assert.Equal(requestId, errorViewModel.RequestId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ShowRequestId_ReturnsFalse_WhenRequestIdIsNullOrEmpty(string requestId)
    {
        // Arrange
        var errorViewModel = new ErrorViewModel
        {
            RequestId = requestId
        };

        // Act & Assert
        Assert.False(errorViewModel.ShowRequestId);
    }

    [Theory]
    [InlineData("   ")]  // whitespace only - NOT considered empty by string.IsNullOrEmpty
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("test-request-id")]
    [InlineData("a")]  // single character
    [InlineData("request-id-with-dashes-and-123-numbers")]
    public void ShowRequestId_ReturnsTrue_WhenRequestIdHasContent(string requestId)
    {
        // Arrange
        var errorViewModel = new ErrorViewModel
        {
            RequestId = requestId
        };

        // Act & Assert
        Assert.True(errorViewModel.ShowRequestId);
    }

    [Fact]
    public void ShowRequestId_UpdatesDynamically_WhenRequestIdChanges()
    {
        // Arrange
        var errorViewModel = new ErrorViewModel();

        // Initially null
        Assert.Null(errorViewModel.RequestId);
        Assert.False(errorViewModel.ShowRequestId);

        // Set to valid value
        errorViewModel.RequestId = "test-123";
        Assert.Equal("test-123", errorViewModel.RequestId);
        Assert.True(errorViewModel.ShowRequestId);

        // Set back to null
        errorViewModel.RequestId = null;
        Assert.Null(errorViewModel.RequestId);
        Assert.False(errorViewModel.ShowRequestId);

        // Set to empty string
        errorViewModel.RequestId = "";
        Assert.Equal("", errorViewModel.RequestId);
        Assert.False(errorViewModel.ShowRequestId);

        // Set to whitespace (this should return true, not false)
        errorViewModel.RequestId = "   ";
        Assert.Equal("   ", errorViewModel.RequestId);
        Assert.True(errorViewModel.ShowRequestId);  // Whitespace is not null/empty

        // Set back to valid value
        errorViewModel.RequestId = "final-test";
        Assert.Equal("final-test", errorViewModel.RequestId);
        Assert.True(errorViewModel.ShowRequestId);
    }

    [Fact]
    public void ErrorViewModel_CanBeInitialized_WithRequestId()
    {
        // Arrange & Act
        var errorViewModel = new ErrorViewModel
        {
            RequestId = "initial-request-456"
        };

        // Assert
        Assert.Equal("initial-request-456", errorViewModel.RequestId);
        Assert.True(errorViewModel.ShowRequestId);
    }

    [Fact]
    public void ErrorViewModel_CanBeInitialized_WithNullRequestId()
    {
        // Arrange & Act
        var errorViewModel = new ErrorViewModel
        {
            RequestId = null
        };

        // Assert
        Assert.Null(errorViewModel.RequestId);
        Assert.False(errorViewModel.ShowRequestId);
    }
}
