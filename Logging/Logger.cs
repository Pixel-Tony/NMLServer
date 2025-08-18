namespace NMLServer.Logging;

internal static class Logger
{
    public static void Debug(object? message) => Debug(message?.ToString());

    public static void Debug(string? message) => DebugAsync(message).Wait();

    public static async Task DebugAsync(object? message) => await DebugAsync(message?.ToString());

#if DEBUG
    public static async Task DebugAsync(string? message)
        => await Console.Error.WriteLineAsync($"[{DateTime.Now}] {message}").ConfigureAwait(false);
#else
    public static Task DebugAsync(string? _) => Task.CompletedTask;
#endif
}