namespace NMLServer.Lexing;

internal abstract class Token(int start)
{
    public virtual int start { get; set; } = start;

    public virtual int end
    {
        get => start + 1;
        protected set => throw new NotSupportedException();
    }

    public virtual int length => end - start;

    public sealed override string ToString() => $"{{{GetType().Name} at {start}}}";
}