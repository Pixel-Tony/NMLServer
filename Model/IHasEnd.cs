namespace NMLServer.Model;

internal interface IHasEnd : IComparable<IHasEnd, int>
{
    public int End { get; }

    int IComparable<IHasEnd, int>.CompareTo(int other) => End.CompareTo(other);
}