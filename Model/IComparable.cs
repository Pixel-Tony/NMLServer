namespace NMLServer.Model;

interface IComparable<T, TV> where T : IComparable<T, TV>
{
    public int CompareTo(TV value);
}