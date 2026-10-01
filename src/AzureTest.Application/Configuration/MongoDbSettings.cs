

using System.ComponentModel.DataAnnotations;

namespace AzureTest.Application.Configuration;

public class MongoDbSettings
{
    [Required]
    [RegularExpression(@"^[^.\s]+$", ErrorMessage = "DatabaseName cannot contain dots or spaces.")]
    public string DatabaseName { get; set; } = string.Empty;
}
