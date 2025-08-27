namespace Chat.API;

public static class Styling
{
    public static string Color(this string that, string color)
    {
        return $"<color={color}>{that}</color>";
    }
    public static string Bold(this string that)
    {
        return $"<b>{that}</b>";
    }
    public static string Italic(this string that)
    {
        return $"<i>{that}</i>";
    }
    public static string Underline(this string that)
    {
        return $"<ul>{that}</ul>";
    }
    public static string Size(this string that, int size)
    {
        return $"<size={size}>{that}</size>";
    }
}
