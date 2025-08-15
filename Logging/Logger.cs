namespace NMLServer.Logging;

internal static class Logger
{
    public static void Debug(object? message) => Debug(message?.ToString());

    public static void Debug(string? message) => DebugAsync(message).Wait();

    public static async Task DebugAsync(string? message)
    {
#if DEBUG
        await Console.Error.WriteLineAsync($"[{DateTime.Now}] {message}");
#endif
    }
}