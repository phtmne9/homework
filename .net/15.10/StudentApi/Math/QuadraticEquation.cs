namespace StudentApi.Math;

public static class QuadraticEquation
{
    public static double[] Solve(double a, double b, double c)
    {
        if (a == 0)
            throw new ArgumentException("Коэффициент a не может быть нулём", nameof(a));

        var d = b * b - 4 * a * c;

        if (d < 0)
            return Array.Empty<double>();

        if (d == 0)
            return new[] { -b / (2 * a) };

        var root = System.Math.Sqrt(d);

        return new[] { (-b + root) / (2 * a), (-b - root) / (2 * a) };
    }
}
