using System.Net;
using System.Net.Sockets;
using System.Text;

static class SharedExtensions // will need to be refactored into a truly shared set of extension methods
{
    public static string GetEnvar(this string name)
        => Environment.GetEnvironmentVariable(name);

    public static int ToInt(this string text, int? defaultValue = null)
    {
        if (defaultValue.HasValue)
        {
            if (int.TryParse(text, out int result))
                return result;
            else
                return defaultValue.Value;
        }
        else
            return int.Parse(text);
    }
}