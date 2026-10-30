using System.ComponentModel.DataAnnotations;

namespace AzureTest.Domain.Enums;

public enum PreferredTimeZone
{
    [Display(Name = "UTC (Coordinated Universal Time)")]
    Utc = 0,

    [Display(Name = "Honolulu (Hawaii Time)")]
    Hawaii = 1,

    [Display(Name = "Anchorage (Alaska Time)")]
    Alaska = 2,

    [Display(Name = "Los Angeles, San Francisco (Pacific Time)")]
    Pacific = 3,

    [Display(Name = "Denver, Phoenix (Mountain Time)")]
    Mountain = 4,

    [Display(Name = "Chicago, Dallas (Central Time)")]
    Central = 5,

    [Display(Name = "New York, Toronto (Eastern Time)")]
    Eastern = 6,

    [Display(Name = "Halifax (Atlantic Time)")]
    Atlantic = 7,

    [Display(Name = "São Paulo, Buenos Aires (Brasilia / Argentina Time)")]
    Brasilia = 8,

    [Display(Name = "London, Dublin, Lisbon (Greenwich Mean Time)")]
    London = 9,

    [Display(Name = "Paris, Berlin, Amsterdam, Rome (Central European Time)")]
    CentralEuropean = 10,

    [Display(Name = "Athens, Istanbul, Bucharest (Eastern European Time)")]
    EasternEuropean = 11,

    [Display(Name = "Cairo (Egypt Time)")]
    Cairo = 12,

    [Display(Name = "Johannesburg (South Africa Time)")]
    Johannesburg = 13,

    [Display(Name = "Moscow, Riyadh, Baghdad (Arabia / Moscow Time)")]
    Moscow = 14,

    [Display(Name = "Tehran (Iran Time)")]
    Tehran = 15,

    [Display(Name = "Dubai, Abu Dhabi (Gulf Time)")]
    Dubai = 16,

    [Display(Name = "Karachi, Tashkent (Pakistan / West Asia Time)")]
    Karachi = 17,

    [Display(Name = "Mumbai, New Delhi (India Time)")]
    India = 18,

    [Display(Name = "Bangkok, Jakarta, Hanoi (Indochina Time)")]
    Bangkok = 19,

    [Display(Name = "Singapore, Hong Kong, Beijing (Singapore / China Time)")]
    Singapore = 20,

    [Display(Name = "Tokyo, Seoul (Japan / Korea Time)")]
    Tokyo = 21,

    [Display(Name = "Sydney, Melbourne (Australian Eastern Time)")]
    Sydney = 22,

    [Display(Name = "Auckland, Wellington (New Zealand Time)")]
    Auckland = 23
}
