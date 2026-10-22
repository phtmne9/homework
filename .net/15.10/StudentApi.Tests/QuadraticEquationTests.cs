using StudentApi.Math;
using Xunit;

namespace StudentApi.Tests;

public class QuadraticEquationTests
{
    [Fact]
    public void TwoRoots_WhenDiscriminantPositive()
    {
        var roots = QuadraticEquation.Solve(1, -5, 6);

        Assert.Equal(2, roots.Length);
        Assert.Contains(3, roots);
        Assert.Contains(2, roots);
    }

    [Fact]
    public void OneRoot_WhenDiscriminantZero()
    {
        var roots = QuadraticEquation.Solve(1, -2, 1);

        Assert.Single(roots);
        Assert.Equal(1, roots[0]);
    }

    [Fact]
    public void NoRoots_WhenDiscriminantNegative()
    {
        var roots = QuadraticEquation.Solve(1, 0, 5);

        Assert.Empty(roots);
    }

    [Fact]
    public void Throws_WhenAIsZero()
    {
        Assert.Throws<ArgumentException>(() => QuadraticEquation.Solve(0, 2, 1));
    }

    [Theory]
    [InlineData(2, -4, -6, 3, -1)]
    [InlineData(1, 1, -12, 3, -4)]
    public void TwoRoots_ForDifferentCoefficients(
        double a, double b, double c, double first, double second)
    {
        var roots = QuadraticEquation.Solve(a, b, c);

        Assert.Equal(2, roots.Length);
        Assert.Contains(first, roots);
        Assert.Contains(second, roots);
    }
}
