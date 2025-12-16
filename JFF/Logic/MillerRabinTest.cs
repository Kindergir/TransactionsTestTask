namespace JFF.Logic;

public static class MillerRabinTest
{
    public static bool IsPrime(this ulong number)
    {
        switch (number)
        {
            case < 2:
                return false;
            case 2 or 3:
                return true;
        }

        if ((number & 1) == 0)
            return false;

        var previousOddNumber = number - 1;
        var powerOfTwo = 0;
        while ((previousOddNumber & 1) == 0)
        {
            previousOddNumber >>= 1;
            powerOfTwo++;
        }

        Span<ulong> bases =
        [
            2, 325, 9375, 28178, 450775, 9780504, 1795265022
        ];

        foreach (var baseNumber in bases)
        {
            if (baseNumber % number == 0) continue;
            if (!MillerTest(baseNumber, powerOfTwo, previousOddNumber, number))
                return false;
        }

        return true;
    }

    private static bool MillerTest(ulong baseNumber, int powerOfTwo, ulong previousOddNumber, ulong numberToCheck)
    {
        var x = ModularExponentiation(baseNumber, previousOddNumber, numberToCheck);
        if (x == 1 || x == numberToCheck - 1)
            return true;

        for (var i = 1; i < powerOfTwo; i++)
        {
            x = BinaryExponentiation(x, x, numberToCheck);
            if (x == numberToCheck - 1)
                return true;
        }

        return false;
    }

    private static ulong BinaryExponentiation(ulong firstMultiplier, ulong secondMultiplier, ulong mod)
    {
        ulong result = 0;
        while (secondMultiplier > 0)
        {
            if ((secondMultiplier & 1) == 1)
                result = (result + firstMultiplier) % mod;

            firstMultiplier = (firstMultiplier << 1) % mod;
            secondMultiplier >>= 1;
        }
        return result;
    }

    private static ulong ModularExponentiation(ulong numberToExp, ulong power, ulong mod)
    {
        ulong result = 1;
        numberToExp %= mod;

        while (power > 0)
        {
            if ((power & 1) == 1)
                result = BinaryExponentiation(result, numberToExp, mod);

            numberToExp = BinaryExponentiation(numberToExp, numberToExp, mod);
            power >>= 1;
        }

        return result;
    }
}