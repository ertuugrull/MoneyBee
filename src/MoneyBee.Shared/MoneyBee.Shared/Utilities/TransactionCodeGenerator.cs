namespace MoneyBee.Shared.Utilities;

public static class TransactionCodeGenerator
{
    private static readonly Random Random = new();
    private const string Characters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string Generate()
    {
        var code = new char[8];
        for (int i = 0; i < 8; i++)
        {
            code[i] = Characters[Random.Next(Characters.Length)];
        }
        return new string(code);
    }
}
