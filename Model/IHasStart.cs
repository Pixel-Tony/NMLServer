namespace NMLServer.Model;

internal interface IHasStart : IComparable<IHasStart, int>
{
    public int Start { get; }

    int IComparable<IHasStart, int>.CompareTo(int other) => Start.CompareTo(other);
}