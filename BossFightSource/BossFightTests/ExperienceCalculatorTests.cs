using Xunit;
namespace BossFight.Tests;

public class ExperienceCalculatorTests
{
    [Theory]
    [InlineData(100, 20, 10, 5)]  // levelsBelow >= 10
    [InlineData(100, 20, 1, 5)]  // levelsBelow >= 10
    [InlineData(100, 20, 11, 24)]  // levelsBelow == 9
    [InlineData(100, 20, 12, 43)]  // levelsBelow == 8
    [InlineData(100, 20, 13, 62)]  // levelsBelow == 7
    [InlineData(100, 20, 14, 81)]  // levelsBelow == 6
    [InlineData(100, 1, 1, 100)]
    [InlineData(100, 1, 2, 100)]
    [InlineData(100, 1, 3, 100)]
    [InlineData(100, 1, 4, 25)]  // pXP * (playerLevel_decimal / monsterLevel_decimal)
    [InlineData(100, 1, 10, 10)]  // pXP * (playerLevel_decimal / monsterLevel_decimal)
    [InlineData(100, 1, 8, 13)]  // Math.Ceiling(result)
    public void CalcXpPenalty(int pXP, int pPlayerLevel, int? pMonsterLevel, int pExpected)
    {
        var result = ExperienceCalculator.CalcXpPenalty(pXP, pPlayerLevel, pMonsterLevel);
        Assert.Equal(pExpected, result);
    }
}
