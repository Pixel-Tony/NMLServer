using MediatR;

namespace NMLServer.Analysis;

internal static class UnitTask
{
    public static readonly Task<Unit> Result = Task.FromResult(Unit.Value);
}