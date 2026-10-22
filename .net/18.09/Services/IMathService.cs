namespace MathApi.Services;

public interface IMathService
{
    double CircleArea(double r);

    double SphereVolume(double r);

    IReadOnlyList<int> Palindromes(int count, int from);
}
