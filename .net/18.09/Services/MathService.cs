namespace MathApi.Services;

public class MathService : IMathService
{
    public double CircleArea(double r) => Math.PI * r * r;

    public double SphereVolume(double r) => 4.0 / 3.0 * Math.PI * r * r * r;

    public IReadOnlyList<int> Palindromes(int count, int from)
    {
        var result = new List<int>(count);

        for (var n = from; result.Count < count; n++)
        {
            if (IsPalindrome(n))
                result.Add(n);
        }

        return result;
    }

    private static bool IsPalindrome(int n)
    {
        if (n < 0)
            return false;

        long rev = 0;

        for (var x = n; x > 0; x /= 10)
            rev = rev * 10 + x % 10;

        return rev == n;
    }
}
