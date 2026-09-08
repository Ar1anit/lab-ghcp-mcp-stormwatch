namespace StormWatch.Chat;

public static class EnvironmentFile
{
    public static void Load(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        int lineNumber = 0;
        foreach (string line in File.ReadLines(path))
        {
            lineNumber++;
            string trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            {
                continue;
            }

            int separator = trimmed.IndexOf('=');
            if (separator <= 0)
            {
                throw new InvalidOperationException(
                    $"Invalid environment entry on line {lineNumber}.");
            }

            string name = trimmed[..separator].Trim();
            string value = Unquote(trimmed[(separator + 1)..].Trim());
            if (Environment.GetEnvironmentVariable(name) is null)
            {
                Environment.SetEnvironmentVariable(name, value);
            }
        }
    }

    private static string Unquote(string value) =>
        value.Length >= 2 &&
        ((value[0] == '"' && value[^1] == '"') ||
         (value[0] == '\'' && value[^1] == '\''))
            ? value[1..^1]
            : value;
}