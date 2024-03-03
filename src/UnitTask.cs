using MediatR;

namespace NMLServer;

internal static class UnitTask
{
    public static readonly Task<Unit> Result = Task.FromResult(Unit.Value);
}