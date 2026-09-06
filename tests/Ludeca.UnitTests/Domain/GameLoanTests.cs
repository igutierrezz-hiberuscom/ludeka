using System;
using Ludeca.Core.Entities;
using Xunit;

namespace Ludeca.UnitTests.Domain;

public class GameLoanTests
{
    [Fact]
    public void Constructor_ShouldInitializeCorrectly_WhenParametersAreValid()
    {
        // Arrange
        string userId = "user-123";
        Guid gameId = Guid.NewGuid();
        string borrower = "Carlos";
        var loanDate = DateTimeOffset.UtcNow;

        // Act
        var loan = new GameLoan(userId, gameId, borrower, loanDate, "Para jugar este finde");

        // Assert
        Assert.NotEqual(Guid.Empty, loan.Id);
        Assert.Equal(userId, loan.UserId);
        Assert.Equal(gameId, loan.GameId);
        Assert.Equal(borrower, loan.BorrowerName);
        Assert.Equal("Para jugar este finde", loan.Notes);
        Assert.False(loan.IsReturned);
        Assert.Null(loan.ReturnedDate);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowArgumentException_WhenBorrowerNameIsInvalid(string? invalidName)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new GameLoan("user-1", Guid.NewGuid(), invalidName, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void MarkAsReturned_ShouldSetIsReturnedTrueAndRecordDate()
    {
        // Arrange
        var loan = new GameLoan("user-1", Guid.NewGuid(), "Marta", DateTimeOffset.UtcNow.AddDays(-5));
        var returnDate = DateTimeOffset.UtcNow;

        // Act
        loan.MarkAsReturned(returnDate);

        // Assert
        Assert.True(loan.IsReturned);
        Assert.Equal(returnDate, loan.ReturnedDate);
    }
}
