using System;
using System.Collections.Generic;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.Debts;
using FluentAssertions;
using Xunit;

namespace AzureTest.UnitTest.Debts;

public class DebtFilterAndAnalyticsTests
{
    [Fact]
    public void IPagedFilter_Properties_ShouldBeAssignableOnBaseAndFilter()
    {
        // Arrange
        IPagedFilter filter = new GetAllDebtsFilters();
        var userIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var fromDate = new DateTime(2026, 1, 1);
        var toDate = new DateTime(2026, 12, 31);

        // Act
        filter.UserIds = userIds;
        filter.CreatedOnFrom = fromDate;
        filter.CreatedOnTo = toDate;
        filter.SearchTerm = "Loan";

        // Assert
        filter.UserIds.Should().HaveCount(2);
        filter.CreatedOnFrom.Should().Be(fromDate);
        filter.CreatedOnTo.Should().Be(toDate);
        filter.SearchTerm.Should().Be("Loan");
    }

    [Fact]
    public void GetAllDebtsFilters_Should_DefaultToSortByDateOfBorrowDesc()
    {
        // Arrange & Act
        var filters = new GetAllDebtsFilters();

        // Assert
        filters.SortBy.Should().Be("DateOfBorrow");
        filters.SortDescending.Should().BeTrue();
    }
}
