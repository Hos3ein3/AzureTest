using System;
using AzureTest.Domain.Enums;
namespace AzureTest.Domain.Documents.Gym;
public class Training {
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public string? MediaUrl { get; set; } = string.Empty;
    public TrainingType Type { get; set; } = TrainingType.SetAndRest;
    public double? BaseCaloriesBurn { get; set; } = 0;
    public List<string>? Equipments { get; set; } = null;

}
