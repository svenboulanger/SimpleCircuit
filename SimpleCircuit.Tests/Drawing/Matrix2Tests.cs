using System;
using SimpleCircuit.Drawing;
using Xunit;

namespace SimpleCircuit.Tests.Drawing;

public class Matrix2Tests
{
    [Fact]
    public void Identity_IsCorrect()
        => Assert.Equal(new Matrix2(1, 0, 0, 1), Matrix2.Identity);

    [Fact]
    public void Determinant_IsComputed()
        => Assert.Equal(-2.0, new Matrix2(1, 2, 3, 4).Determinant, 9);

    [Fact]
    public void Inverse_TimesOriginal_IsIdentity()
    {
        var m = new Matrix2(1, 2, 3, 4);
        Assert.Equal(Matrix2.Identity, m.Inverse * m);
    }

    [Fact]
    public void TryInvert_NonSingular_Succeeds()
    {
        Assert.True(new Matrix2(1, 2, 3, 4).TryInvert(out var inv));
        Assert.Equal(Matrix2.Identity, inv * new Matrix2(1, 2, 3, 4));
    }

    [Fact]
    public void TryInvert_Singular_Fails()
        => Assert.False(new Matrix2(1, 2, 2, 4).TryInvert(out _)); // determinant 0

    [Fact]
    public void Transposed_SwapsOffDiagonal()
        => Assert.Equal(new Matrix2(1, 3, 2, 4), new Matrix2(1, 2, 3, 4).Transposed);

    [Fact]
    public void IsOrthonormal_TrueForRotation_FalseForScale()
    {
        Assert.True(Matrix2.Rotate(0.7).IsOrthonormal);
        Assert.False(Matrix2.Scale(2).IsOrthonormal);
    }

    [Fact]
    public void Rotate_MatchesVectorRotate()
    {
        double angle = 0.4;
        var v = new Vector2(2, 1);
        Assert.Equal(v.Rotate(angle), Matrix2.Rotate(angle) * v);
    }

    [Fact]
    public void Scale_ScalesVector()
    {
        Assert.Equal(new Vector2(6, 6), Matrix2.Scale(3) * new Vector2(2, 2));
        Assert.Equal(new Vector2(4, 9), Matrix2.Scale(2, 3) * new Vector2(2, 3));
    }

    [Fact]
    public void MatrixProduct_IsComputed()
    {
        var a = new Matrix2(1, 2, 3, 4);
        var b = new Matrix2(5, 6, 7, 8);
        Assert.Equal(new Matrix2(19, 22, 43, 50), a * b);
    }

    [Fact]
    public void ScalarOperators_BehaveAsExpected()
    {
        var m = new Matrix2(1, 2, 3, 4);
        Assert.Equal(new Matrix2(2, 4, 6, 8), m * 2);
        Assert.Equal(new Matrix2(2, 4, 6, 8), 2 * m);
        Assert.Equal(new Matrix2(0.5, 1, 1.5, 2), m / 2);
        Assert.Equal(new Matrix2(-1, -2, -3, -4), -m);
    }

    [Fact]
    public void AddSubtract_BehaveAsExpected()
    {
        var a = new Matrix2(1, 2, 3, 4);
        var b = new Matrix2(4, 3, 2, 1);
        Assert.Equal(new Matrix2(5, 5, 5, 5), a + b);
        Assert.Equal(new Matrix2(-3, -1, 1, 3), a - b);
    }
}
