namespace NMLServer.Model.Processors.Diagnostics;

internal static class ErrorStrings
{
    public const string ExpectedSquareBracket = "Expected square bracket for var/param invocation";
    public const string InvalidTarget = "Invalid parameter assignment target";
    public const string MissingSemicolon = "Missing semicolon";
    public const string MissingClosingBracket = "Missing closing bracket";
    public const string MissingAssignedValue = "Missing assigned value";
    public const string UnexpectedTopLevelExpr = "Unexpected expression at top level of the script";
    public const string ErrorMissingExpr = "Missing expression";
    public const string UniqueIdentifierExpected = "Unique identifier expected";
    public const string UniqueIdentifierArgsExpected = "Unique identifier and argument list expected";
}