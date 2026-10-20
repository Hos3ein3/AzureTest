using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Documents.Gym;

public class UserWeightLog : ITenantEntity
{
    public static class Rules
    {
        public const double MinWeightKg = 20.0;
        public const double MaxWeightKg = 400.0;
        public const double MinHeightCm = 50.0;
        public const double MaxHeightCm = 280.0;
        public const double MinPercent = 0.0;
        public const double MaxPercent = 100.0;
        public const int NotesMaxLength = 500;
    }

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public double WeightKg { get; set; }
    public double HeightCm { get; set; }
    public double Bmi => HeightCm > 0 ? Math.Round(WeightKg / Math.Pow(HeightCm / 100.0, 2), 1) : 0;
    public double? MusclePercent { get; set; }
    public double? BonePercent { get; set; }
    public double? FatPercent { get; set; }
    public double? OtherPercent => (MusclePercent.HasValue || BonePercent.HasValue || FatPercent.HasValue)
        ? Math.Max(0.0, Math.Round(100.0 - (MusclePercent ?? 0.0) - (BonePercent ?? 0.0) - (FatPercent ?? 0.0), 1))
        : null;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string BmiCategory => Bmi switch
    {
        < 18.5 => "Underweight",
        < 25.0 => "Normal",
        < 30.0 => "Overweight",
        < 35.0 => "Obese Class I",
        < 40.0 => "Obese Class II",
        _ => "Obese Class III"
    };

    public static UserWeightLog Create(
        Guid userId,
        double weightKg,
        double heightCm,
        DateTime date,
        string? notes = null,
        double? musclePercent = null,
        double? bonePercent = null,
        double? fatPercent = null)
    {
        return new UserWeightLog
        {
            UserId = userId,
            WeightKg = Math.Round(weightKg, 1),
            HeightCm = Math.Round(heightCm, 1),
            Date = date,
            Notes = notes?.Trim(),
            MusclePercent = musclePercent.HasValue ? Math.Round(musclePercent.Value, 1) : null,
            BonePercent = bonePercent.HasValue ? Math.Round(bonePercent.Value, 1) : null,
            FatPercent = fatPercent.HasValue ? Math.Round(fatPercent.Value, 1) : null
        };
    }
}
