using Xunit;

namespace SimpleCircuit.Tests;

public class UtilityTests
{
    [Theory]
    [InlineData(0.0, true)]
    [InlineData(1e-12, true)]
    [InlineData(-1e-12, true)]
    [InlineData(1e-3, false)]
    [InlineData(-1.0, false)]
    public void IsZero_Double(double value, bool expected)
        => Assert.Equal(expected, value.IsZero());

    [Fact]
    public void IsZero_Vector()
    {
        Assert.True(new Vector2(0, 1e-12).IsZero());
        Assert.False(new Vector2(0.5, 0).IsZero());
    }

    [Fact]
    public void IsNaN_DetectsNaN()
    {
        Assert.True(double.NaN.IsNaN());
        Assert.False(1.0.IsNaN());
        Assert.True(new Vector2(double.NaN, 0).IsNaN());
        Assert.False(new Vector2(1, 2).IsNaN());
    }

    [Theory]
    [InlineData(1.0, "1")]
    [InlineData(1.5, "1.5")]
    [InlineData(1.234, "1.23")]   // rounds to 2 decimals
    [InlineData(1.236, "1.24")]
    [InlineData(0.0, "0")]
    [InlineData(-2.5, "-2.5")]
    [InlineData(10.0, "10")]
    public void ToSVG_Double_TrimsTrailingZeros(double value, string expected)
        => Assert.Equal(expected, value.ToSVG());

    [Fact]
    public void ToSVG_Vector_JoinsWithComma()
        => Assert.Equal("1.5,2", new Vector2(1.5, 2).ToSVG());
}
