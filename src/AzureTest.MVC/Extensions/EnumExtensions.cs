using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AzureTest.MVC.Extensions;

/// <summary>
/// Provides extension and utility methods for enums to generate SelectListItem lists,
/// retrieve display names, descriptions, or combined display labels.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Gets the display name defined by <see cref="DisplayAttribute.Name"/>, or falls back to the enum name.
    /// </summary>
    public static string GetDisplayName(this Enum enumValue)
    {
        var displayAttribute = enumValue.GetType()
            .GetField(enumValue.ToString())?
            .GetCustomAttribute<DisplayAttribute>();

        return displayAttribute?.GetName() ?? displayAttribute?.Name ?? enumValue.ToString();
    }

    /// <summary>
    /// Gets the description defined by <see cref="DescriptionAttribute.Description"/>, or null if not defined.
    /// </summary>
    public static string? GetDescription(this Enum enumValue)
    {
        var descriptionAttribute = enumValue.GetType()
            .GetField(enumValue.ToString())?
            .GetCustomAttribute<DescriptionAttribute>();

        return descriptionAttribute?.Description;
    }

    /// <summary>
    /// Gets a formatted string combining both <see cref="DisplayAttribute"/> and <see cref="DescriptionAttribute"/>.
    /// If both exist, returns "{DisplayName} - {Description}".
    /// If only Display exists, returns DisplayName.
    /// If only Description exists, returns Description.
    /// Otherwise returns the enum name.
    /// </summary>
    public static string GetDisplayAndDescription(this Enum enumValue)
    {
        var field = enumValue.GetType().GetField(enumValue.ToString());
        var display = field?.GetCustomAttribute<DisplayAttribute>();
        var description = field?.GetCustomAttribute<DescriptionAttribute>();

        var name = display?.GetName() ?? display?.Name;
        var desc = description?.Description;

        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(desc))
        {
            return $"{name} - {desc}";
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        if (!string.IsNullOrWhiteSpace(desc))
        {
            return desc;
        }

        return enumValue.ToString();
    }

    /// <summary>
    /// Returns the enum values and display names as KeyValuePair(int value, string displayName).
    /// </summary>
    public static List<KeyValuePair<int, string>> ToList<T>(bool useDisplayAndDescription = false) where T : struct, Enum
    {
        return Enum.GetValues(typeof(T))
            .Cast<T>()
            .Select(e => new KeyValuePair<int, string>(
                Convert.ToInt32(e),
                useDisplayAndDescription ? e.GetDisplayAndDescription() : e.GetDisplayName()))
            .ToList();
    }

    /// <summary>
    /// Converts an enum type to a list of <see cref="SelectListItem"/> with optional selected item and null placeholder.
    /// </summary>
    public static List<SelectListItem> ToSelectList<T>(
        T? selectedValue = null,
        bool includeNullFirstOption = false,
        string nullOptionText = "-- Select --",
        bool useDisplayAndDescription = false) where T : struct, Enum
    {
        var items = new List<SelectListItem>();

        if (includeNullFirstOption)
        {
            items.Add(new SelectListItem
            {
                Value = string.Empty,
                Text = nullOptionText,
                Selected = !selectedValue.HasValue
            });
        }

        items.AddRange(Enum.GetValues(typeof(T))
            .Cast<T>()
            .Select(e => new SelectListItem
            {
                Value = Convert.ToInt32(e).ToString(),
                Text = useDisplayAndDescription ? e.GetDisplayAndDescription() : e.GetDisplayName(),
                Selected = selectedValue.HasValue && e.Equals(selectedValue.Value)
            }));

        return items;
    }

    /// <summary>
    /// Converts an enum type to a list of <see cref="SelectListItem"/> with a single selected value.
    /// </summary>
    public static List<SelectListItem> ToSelectList<T>(
        T selectedValue,
        bool includeNullFirstOption = false,
        string nullOptionText = "-- Select --",
        bool useDisplayAndDescription = false) where T : struct, Enum
    {
        return ToSelectList<T>((T?)selectedValue, includeNullFirstOption, nullOptionText, useDisplayAndDescription);
    }

    /// <summary>
    /// Explicitly converts enum to select list using <see cref="DisplayAttribute"/>.
    /// </summary>
    public static List<SelectListItem> ToSelectListWithDisplay<T>(
        T? selectedValue = null,
        bool includeNullFirstOption = false,
        string nullOptionText = "-- Select --") where T : struct, Enum
    {
        return ToSelectList<T>(selectedValue, includeNullFirstOption, nullOptionText, useDisplayAndDescription: false);
    }

    /// <summary>
    /// Explicitly converts enum to select list combining <see cref="DisplayAttribute"/> and <see cref="DescriptionAttribute"/>.
    /// </summary>
    public static List<SelectListItem> ToSelectListWithDisplayAndDescription<T>(
        T? selectedValue = null,
        bool includeNullFirstOption = false,
        string nullOptionText = "-- Select --") where T : struct, Enum
    {
        return ToSelectList<T>(selectedValue, includeNullFirstOption, nullOptionText, useDisplayAndDescription: true);
    }

    /// <summary>
    /// Converts an enum type to a list of <see cref="SelectListItem"/> with multiple selected values supported.
    /// </summary>
    public static List<SelectListItem> ToSelectListMultiple<T>(
        IEnumerable<T>? selectedValues = null,
        bool useDisplayAndDescription = false) where T : struct, Enum
    {
        var selectedSet = selectedValues != null ? new HashSet<T>(selectedValues) : new HashSet<T>();

        return Enum.GetValues(typeof(T))
            .Cast<T>()
            .Select(e => new SelectListItem
            {
                Value = Convert.ToInt32(e).ToString(),
                Text = useDisplayAndDescription ? e.GetDisplayAndDescription() : e.GetDisplayName(),
                Selected = selectedSet.Contains(e)
            })
            .ToList();
    }

    /// <summary>
    /// Converts an enum type to multi-select list using Display attribute.
    /// </summary>
    public static List<SelectListItem> ToSelectListMultipleWithDisplay<T>(IEnumerable<T>? selectedValues = null) where T : struct, Enum
    {
        return ToSelectListMultiple<T>(selectedValues, useDisplayAndDescription: false);
    }

    /// <summary>
    /// Converts an enum type to multi-select list using Display and Description attributes.
    /// </summary>
    public static List<SelectListItem> ToSelectListMultipleWithDisplayAndDescription<T>(IEnumerable<T>? selectedValues = null) where T : struct, Enum
    {
        return ToSelectListMultiple<T>(selectedValues, useDisplayAndDescription: true);
    }

    /// <summary>
    /// Instance extension method to convert an existing enum value to a select list with this item selected.
    /// </summary>
    public static List<SelectListItem> ToSelectList(
        this Enum enumValue,
        bool includeNullFirstOption = false,
        string nullOptionText = "-- Select --",
        bool useDisplayAndDescription = false)
    {
        var enumType = enumValue.GetType();
        var items = new List<SelectListItem>();

        if (includeNullFirstOption)
        {
            items.Add(new SelectListItem
            {
                Value = string.Empty,
                Text = nullOptionText,
                Selected = false
            });
        }

        foreach (var val in Enum.GetValues(enumType))
        {
            var e = (Enum)val;
            items.Add(new SelectListItem
            {
                Value = Convert.ToInt32(e).ToString(),
                Text = useDisplayAndDescription ? e.GetDisplayAndDescription() : e.GetDisplayName(),
                Selected = e.Equals(enumValue)
            });
        }

        return items;
    }

    /// <summary>
    /// Instance extension method using Display attribute with this item selected.
    /// </summary>
    public static List<SelectListItem> ToSelectListWithDisplay(
        this Enum enumValue,
        bool includeNullFirstOption = false,
        string nullOptionText = "-- Select --")
    {
        return enumValue.ToSelectList(includeNullFirstOption, nullOptionText, useDisplayAndDescription: false);
    }

    /// <summary>
    /// Instance extension method using both Display and Description attributes with this item selected.
    /// </summary>
    public static List<SelectListItem> ToSelectListWithDisplayAndDescription(
        this Enum enumValue,
        bool includeNullFirstOption = false,
        string nullOptionText = "-- Select --")
    {
        return enumValue.ToSelectList(includeNullFirstOption, nullOptionText, useDisplayAndDescription: true);
    }
}
