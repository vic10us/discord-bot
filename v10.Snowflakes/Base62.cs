namespace v10.Snowflakes;

public static class Base62Extensions
{
    private const string Base62Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public static string Base62Encode(this long number)
    {
        if (number < 0)
        {
            throw new ArgumentException("Input number must be non-negative.");
        }

        if (number == 0)
        {
            return Base62Chars[0].ToString();
        }

        string shortUrl = string.Empty;
        int base62 = Base62Chars.Length;

        while (number > 0)
        {
            int remainder = (int)(number % base62);
            shortUrl = Base62Chars[remainder] + shortUrl;
            number /= base62;
        }

        return shortUrl;
    }

    public static long Base62Decode(this string base62str)
    {
        long number = 0;
        int base62 = Base62Chars.Length;

        for (int i = 0; i < base62str.Length; i++)
        {
            char c = base62str[i];
            int charValue = Base62Chars.IndexOf(c);

            if (charValue == -1)
            {
                throw new ArgumentException("Invalid character in the input string.");
            }

            number = number * base62 + charValue;
        }

        return number;
    }
}
