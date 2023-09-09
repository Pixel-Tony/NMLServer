namespace NMLServer.Lexing.Tokens;

internal abstract class Token
{
    protected static string Input = null!;

    public readonly int Start;

    protected Token(int start)
    {
        Start = start;
    }

    public static void UpdateSourceInput(string next)
    {
        Input = next;
    }
}