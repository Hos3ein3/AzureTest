using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AzureTest.MVC.Extensions;
using Shouldly;
using Xunit;

namespace AzureTest.UnitTest.Extensions;

public enum TestSampleStatus
{
    [Display(Name = "In Progress")]
    [Description("Currently being worked on")]
    InProgress = 1,

    [Display(Name = "Completed State")]
    Completed = 2,

    [Description("Archived record description")]
    Archived = 3,

    Plain = 4
}

public class EnumExtensionsTests
{
    [Fact]
    public void GetDisplayName_ShouldReturnDisplayName_WhenDisplayAttributeExists()
    {
        TestSampleStatus.InProgress.GetDisplayName().ShouldBe("In Progress");
        TestSampleStatus.Completed.GetDisplayName().ShouldBe("Completed State");
    }

    [Fact]
    public void GetDisplayName_ShouldReturnEnumName_WhenNoDisplayAttribute()
    {
        TestSampleStatus.Archived.GetDisplayName().ShouldBe("Archived");
        TestSampleStatus.Plain.GetDisplayName().ShouldBe("Plain");
    }

    [Fact]
    public void GetDescription_ShouldReturnDescription_WhenDescriptionAttributeExists()
    {
        TestSampleStatus.InProgress.GetDescription().ShouldBe("Currently being worked on");
        TestSampleStatus.Archived.GetDescription().ShouldBe("Archived record description");
        TestSampleStatus.Plain.GetDescription().ShouldBeNull();
    }

    [Fact]
    public void GetDisplayAndDescription_ShouldCombineBoth_WhenBothAttributesExist()
    {
        TestSampleStatus.InProgress.GetDisplayAndDescription().ShouldBe("In Progress - Currently being worked on");
        TestSampleStatus.Completed.GetDisplayAndDescription().ShouldBe("Completed State");
        TestSampleStatus.Archived.GetDisplayAndDescription().ShouldBe("Archived record description");
        TestSampleStatus.Plain.GetDisplayAndDescription().ShouldBe("Plain");
    }

    [Fact]
    public void ToSelectList_WithSelectedValue_ShouldMarkCorrectItemAsSelected()
    {
        var list = EnumExtensions.ToSelectList<TestSampleStatus>(TestSampleStatus.Completed);
        list.Count.ShouldBe(4);
        list.Single(x => x.Value == "2").Selected.ShouldBeTrue();
        list.Single(x => x.Value == "1").Selected.ShouldBeFalse();
    }

    [Fact]
    public void ToSelectList_WithNullFirstOption_ShouldPrependNullOption()
    {
        var list = EnumExtensions.ToSelectList<TestSampleStatus>(selectedValue: null, includeNullFirstOption: true, nullOptionText: "-- Choose Status --");
        list.Count.ShouldBe(5);
        list.First().Value.ShouldBe(string.Empty);
        list.First().Text.ShouldBe("-- Choose Status --");
        list.First().Selected.ShouldBeTrue();
    }

    [Fact]
    public void ToSelectListWithDisplayAndDescription_ShouldFormatTextCorrectly()
    {
        var list = EnumExtensions.ToSelectListWithDisplayAndDescription<TestSampleStatus>();
        list.Single(x => x.Value == "1").Text.ShouldBe("In Progress - Currently being worked on");
    }

    [Fact]
    public void ToSelectListMultiple_ShouldMarkAllSelectedItems()
    {
        var selected = new[] { TestSampleStatus.InProgress, TestSampleStatus.Archived };
        var list = EnumExtensions.ToSelectListMultiple(selected);
        list.Single(x => x.Value == "1").Selected.ShouldBeTrue();
        list.Single(x => x.Value == "3").Selected.ShouldBeTrue();
        list.Single(x => x.Value == "2").Selected.ShouldBeFalse();
        list.Single(x => x.Value == "4").Selected.ShouldBeFalse();
    }

    [Fact]
    public void Instance_ToSelectList_ShouldWorkDirectlyOnEnumValue()
    {
        var item = TestSampleStatus.Completed;
        var list = item.ToSelectList(includeNullFirstOption: true);
        list.Count.ShouldBe(5);
        list.Single(x => x.Value == "2").Selected.ShouldBeTrue();
    }
}
