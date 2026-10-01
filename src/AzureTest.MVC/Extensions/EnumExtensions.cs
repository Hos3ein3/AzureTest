using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AzureTest.MVC.Extensions;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum enumValue)
    {
        var displayAttribute = enumValue.GetType()
            .GetField(enumValue.ToString())?
            .GetCustomAttribute<DisplayAttribute>();

        return displayAttribute?.Name ?? enumValue.ToString();
    }

    public static List<KeyValuePair<int, string>> ToList<T>() where T : struct, Enum
    {
        return Enum.GetValues(typeof(T))
            .Cast<T>()
            .Select(e => new KeyValuePair<int, string>(
                Convert.ToInt32(e), 
                e.GetDisplayName()))
            .ToList();
    }

    public static IEnumerable<SelectListItem> ToSelectList<T>(T? selectedValue = null) where T : struct, Enum
    {
        return Enum.GetValues(typeof(T))
            .Cast<T>()
            .Select(e => new SelectListItem
            {
                Value = Convert.ToInt32(e).ToString(),
                Text = e.GetDisplayName(),
                Selected = selectedValue.HasValue && e.Equals(selectedValue.Value)
            });
    }
    
    public static IEnumerable<SelectListItem> ToSelectList<T>(T selectedValue) where T : struct, Enum
    {
        return Enum.GetValues(typeof(T))
            .Cast<T>()
            .Select(e => new SelectListItem
            {
                Value = Convert.ToInt32(e).ToString(),
                Text = e.GetDisplayName(),
                Selected = e.Equals(selectedValue)
            });
    }

    public static IEnumerable<SelectListItem> ToSelectListMultiple<T>(IEnumerable<T>? selectedValues = null) where T : struct, Enum
    {
        var selectedSet = selectedValues != null ? new HashSet<T>(selectedValues) : new HashSet<T>();
        
        return Enum.GetValues(typeof(T))
            .Cast<T>()
            .Select(e => new SelectListItem
            {
                Value = Convert.ToInt32(e).ToString(),
                Text = e.GetDisplayName(),
                Selected = selectedSet.Contains(e)
            });
    }
}
