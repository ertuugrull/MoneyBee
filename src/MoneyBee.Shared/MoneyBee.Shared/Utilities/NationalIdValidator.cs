namespace MoneyBee.Shared.Utilities;

public static class NationalIdValidator
{
    public static bool IsValid(string nationalId)
    {
        if (string.IsNullOrWhiteSpace(nationalId))
            return false;

        if (nationalId.Length != 11)
            return false;

        if (!nationalId.All(char.IsDigit))
            return false;

        if (nationalId[0] == '0')
            return false;

        int[] digits = nationalId.Select(c => c - '0').ToArray();

        int sumOdd = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
        int sumEven = digits[1] + digits[3] + digits[5] + digits[7];

        int digit10 = ((sumOdd * 7) - sumEven) % 10;
        if (digit10 < 0) digit10 += 10;

        if (digits[9] != digit10)
            return false;

        int sumFirst10 = digits.Take(10).Sum();
        int digit11 = sumFirst10 % 10;

        if (digits[10] != digit11)
            return false;

        return true;
    }
}
