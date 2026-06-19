using System;
using Xunit;

namespace SimpleCircuit.Tests.Drawing;

public class Vector2Tests
{
    [Fact]
    public void Constructor_SetsComponents()
    {
        var v = new Vector2(3, -4);
        Assert.Equal(3, v.X);
        Assert.Equal(-4, v.Y);
    }

    [Fact]
    public void Constants_AreCorrect()
    {
        Assert.Equal(new Vector2(0, 0), Vector2.Zero);
        Assert.Equal(new Vector2(1, 0), Vector2.UX);
        Assert.Equal(new Vector2(0, 1), Vector2.UY);
        Assert.True(Vector2.NaN.IsNaN());
    }

    [Fact]
    public void Length_OfThreeFour_IsFive()
        => Assert.Equal(5.0, new Vector2(3, 4).Length, 9);

    [Fact]
    public void Perpendicular_RotatesNinetyDegrees()
        => Assert.Equal(new Vector2(0, 1), new Vector2(1, 0).Perpendicular);

    [Theory]
    [InlineData(1, 0, 0, 1, 0)]   // orthogonal
    [InlineData(2, 3, 4, 5, 23)]  // 2*4 + 3*5
    public void Dot_ComputesScalarProduct(double ax, double ay, double bx, double by, double expected)
    {
        var a = new Vector2(ax, ay);
        var b = new Vector2(bx, by);
        Assert.Equal(expected, a.Dot(b), 9);
        Assert.Equal(expected, a * b, 9); // operator *(vec, vec) is the dot product
    }

    [Fact]
    public void Rotate_QuarterTurn_MapsXToY()
    {
        var rotated = new Vector2(1, 0).Rotate(Math.PI / 2);
        Assert.Equal(new Vector2(0, 1), rotated); // Equals uses the type's 1e-9 tolerance
    }

    [Fact]
    public void Scale_ScalesEachAxis()
        => Assert.Equal(new Vector2(4, 9), new Vector2(2, 3).Scale(2, 3));

    [Fact]
    public void Normal_ReturnsUnitVectorAtAngle()
    {
        Assert.Equal(new Vector2(1, 0), Vector2.Normal(0));
        Assert.Equal(new Vector2(0, 1), Vector2.Normal(Math.PI / 2));
    }

    [Fact]
    public void Operators_BehaveAsExpected()
    {
        var a = new Vector2(1, 2);
        var b = new Vector2(3, 4);
        Assert.Equal(new Vector2(4, 6), a + b);
        Assert.Equal(new Vector2(-2, -2), a - b);
        Assert.Equal(new Vector2(-1, -2), -a);
        Assert.Equal(new Vector2(2, 4), a * 2);
        Assert.Equal(new Vector2(2, 4), 2 * a); // commutative with scalar
        Assert.Equal(new Vector2(0.5, 1), a / 2);
    }

    [Fact]
    public void AtX_ReturnsPointOnLine()
    {
        var p = Vector2.AtX(1, new Vector2(0, 0), new Vector2(2, 4));
        Assert.Equal(new Vector2(1, 2), p);
    }

    [Fact]
    public void AtX_VerticalLine_ReturnsNaN()
    {
        var p = Vector2.AtX(1, new Vector2(2, 0), new Vector2(2, 5));
        Assert.True(p.IsNaN());
    }

    [Fact]
    public void Equals_UsesTolerance()
    {
        var a = new Vector2(1.0, 1.0);
        Assert.Equal(a, new Vector2(1.0 + 1e-12, 1.0));   // within tolerance
        Assert.NotEqual(a, new Vector2(1.0 + 1e-3, 1.0)); // outside tolerance
    }

    [Fact]
    public void GetHashCode_EqualForEqualVectors()
    {
        var a = new Vector2(1.23456789, 2.0);
        var b = new Vector2(1.23456789, 2.0);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Order_ReordersIntoFirstQuadrant()
    {
        // A direction pointing into the third quadrant should swap both pairs.
        double lowX = 0, highX = 10, lowY = 0, highY = 20;
        var dir = new Vector2(-1, -1).Order(ref lowX, ref highX, ref lowY, ref highY);
        Assert.Equal(new Vector2(1, 1), dir);
        Assert.Equal(10, lowX);
        Assert.Equal(0, highX);
        Assert.Equal(20, lowY);
        Assert.Equal(0, highY);
    }
}
