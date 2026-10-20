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

    [Fact]
    public void UserWeightLog_WithBodyComposition_ShouldStorePercentagesAndCalculateOther()
    {
        // Arrange & Act
        var log = UserWeightLog.Create(
            Guid.NewGuid(),
            weightKg: 80.0,
            heightCm: 180.0,
            date: DateTime.Today,
            notes: "Smart scale sync",
            musclePercent: 42.5,
            bonePercent: 14.5,
            fatPercent: 18.0);

        // Assert
        log.MusclePercent.Should().Be(42.5);
        log.BonePercent.Should().Be(14.5);
        log.FatPercent.Should().Be(18.0);
        log.OtherPercent.Should().Be(25.0);
    }

    [Fact]
    public void UserWeightLog_WithoutBodyComposition_OtherPercentShouldBeNull()
    {
        // Arrange & Act
        var log = UserWeightLog.Create(
            Guid.NewGuid(),
            weightKg: 75.0,
            heightCm: 175.0,
            date: DateTime.Today);

        // Assert
        log.MusclePercent.Should().BeNull();
        log.BonePercent.Should().BeNull();
        log.FatPercent.Should().BeNull();
        log.OtherPercent.Should().BeNull();
    }

    [Fact]
    public void LogUserWeightCommandValidator_WithValidBodyComposition_ShouldPassValidation()
    {
        // Arrange
        var validator = new AzureTest.Application.Gym.LogUserWeightCommandValidator();
        var cmd = new AzureTest.Application.Gym.LogUserWeightCommand(
            Guid.NewGuid(),
            WeightKg: 82.0,
            HeightCm: 180.0,
            Date: DateTime.Today,
            Notes: "Healthy checkin",
            MusclePercent: 45.0,
            BonePercent: 14.0,
            FatPercent: 16.5);

        // Act
        var result = validator.Validate(cmd);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void LogUserWeightCommandValidator_WithSumExceeding100_ShouldFailValidation()
    {
        // Arrange
        var validator = new AzureTest.Application.Gym.LogUserWeightCommandValidator();
        var cmd = new AzureTest.Application.Gym.LogUserWeightCommand(
            Guid.NewGuid(),
            WeightKg: 82.0,
            HeightCm: 180.0,
            Date: DateTime.Today,
            Notes: "Exceeding 100%",
            MusclePercent: 55.0,
            BonePercent: 30.0,
            FatPercent: 25.0); // Total = 110%

        // Act
        var result = validator.Validate(cmd);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("exceed 100%"));
    }

    [Theory]
    [InlineData(-5.0, 15.0, 20.0)]
    [InlineData(40.0, -1.0, 20.0)]
    [InlineData(40.0, 15.0, 105.0)]
    public void LogUserWeightCommandValidator_WithInvalidPercentageRange_ShouldFailValidation(double muscle, double bone, double fat)
    {
        // Arrange
        var validator = new AzureTest.Application.Gym.LogUserWeightCommandValidator();
        var cmd = new AzureTest.Application.Gym.LogUserWeightCommand(
            Guid.NewGuid(),
            WeightKg: 80.0,
            HeightCm: 180.0,
            Date: DateTime.Today,
            Notes: "Invalid range",
            MusclePercent: muscle,
            BonePercent: bone,
            FatPercent: fat);

        // Act
        var result = validator.Validate(cmd);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
