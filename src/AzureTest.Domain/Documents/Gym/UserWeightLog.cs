using System;

namespace AzureTest.Domain.Documents.Gym;

public class UserWeightLog
{
    public static class Rules
    {
        public const double MinWeightKg = 20.0;
        public const double MaxWeightKg = 400.0;
        public const double MinHeightCm = 50.0;
        public const double MaxHeightCm = 280.0;
        public const int NotesMaxLength = 500;
    }

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }
    public double WeightKg { get; set; }
    public double HeightCm { get; set; }
    public double Bmi => HeightCm > 0 ? Math.Round(WeightKg / Math.Pow(HeightCm / 100.0, 2), 1) : 0;
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

    public static UserWeightLog Create(Guid userId, double weightKg, double heightCm, DateTime date, string? notes = null)
    {
        return new UserWeightLog
        {
            UserId = userId,
            WeightKg = Math.Round(weightKg, 1),
            HeightCm = Math.Round(heightCm, 1),
            Date = date,
            Notes = notes?.Trim()
        };
    }
}
