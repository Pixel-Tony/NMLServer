namespace NMLServer.Lexing;

internal sealed class AssignmentToken : Token
{
    public override int end { get; }

    public AssignmentToken(int position) : base(position)
    {
        end = Start + 2;
    }
}