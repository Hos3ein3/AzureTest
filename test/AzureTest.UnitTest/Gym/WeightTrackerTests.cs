using System;
using AzureTest.Domain.Documents.Gym;
using FluentAssertions;
using Xunit;

namespace AzureTest.UnitTest.Gym;

public class WeightTrackerTests
{
    [Theory]
    [InlineData(115.0, 180.0, 35.5, "Obese Class II")]
    [InlineData(95.0, 180.0, 29.3, "Overweight")]
    [InlineData(85.0, 180.0, 26.2, "Overweight")]
    [InlineData(70.0, 180.0, 21.6, "Normal")]
    [InlineData(50.0, 180.0, 15.4, "Underweight")]
    public void UserWeightLog_Should_CalculateBmiAndCategory_Accurately(double weightKg, double heightCm, double expectedBmi, string expectedCategory)
    {
        // Arrange & Act
        var log = UserWeightLog.Create(Guid.NewGuid(), weightKg, heightCm, DateTime.Today, "Test note");

        // Assert
        log.WeightKg.Should().Be(weightKg);
        log.HeightCm.Should().Be(heightCm);
        log.Bmi.Should().BeApproximately(expectedBmi, 0.1);
        log.BmiCategory.Should().Be(expectedCategory);
    }

    [Fact]
    public void UserWeightLog_ZeroHeight_ShouldReturnZeroBmi()
    {
        // Arrange & Act
        var log = new UserWeightLog { WeightKg = 80, HeightCm = 0 };

        // Assert
        log.Bmi.Should().Be(0);
        log.BmiCategory.Should().Be("Underweight");
    }
}
