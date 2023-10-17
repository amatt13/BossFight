using BossFight.BossFightEnums;

namespace BossFight.Tests;

public class EnumTextFormatterTests
{
    private enum TestEnum
    {
        TEST_VALUE1 = 1,
        OTHER = 2,
        THIRD_OPTION = 3
    }

    [Fact]
    public void PrintSingleValue()
    {
        Assert.Equal("Test Value1", EnumTextFormatter.EnumPrinter(TestEnum.TEST_VALUE1));
    }

    [Fact]
    public void PrintIEnumerableEnums()
    {
        string result = EnumTextFormatter.EnumPrinter(new List<Enum>{TestEnum.TEST_VALUE1, TestEnum.OTHER}, "-");
        Assert.Equal("Test Value1-Other", result);
    }

    [Fact]
    public void PrintIEnumerableEnums_SpecificEnumType()
    {
        string result = EnumTextFormatter.EnumPrinter(new List<TestEnum>{TestEnum.TEST_VALUE1, TestEnum.OTHER}, "-");
        Assert.Equal("Test Value1-Other", result);
    }

    [Fact]
    public void PrintArrayEnums()
    {
        string result = EnumTextFormatter.EnumPrinter(Enum.GetValues(typeof(TestEnum)), "$");
        Assert.Equal("Test Value1$Other$Third Option", result);
    }

    [Fact]
    public void PrintTypeEnums()
    {
        string result = EnumTextFormatter.EnumPrinter(typeof(TestEnum), "xXx");
        Assert.Equal("Test Value1xXxOtherxXxThird Option", result);
    }
}
