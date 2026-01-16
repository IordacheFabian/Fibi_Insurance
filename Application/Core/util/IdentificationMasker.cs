using System;

namespace Application.Core.util;

public class IdentificationMasker
{
    public static string Mask(string value, int visibleChars = 4)
    {
        if(string.IsNullOrEmpty(value)) return string.Empty;

        if(value.Length <= visibleChars) return value;

        var maskedLen = value.Length - visibleChars;
        return new string('*', maskedLen) + value[^visibleChars..];
    }
}
