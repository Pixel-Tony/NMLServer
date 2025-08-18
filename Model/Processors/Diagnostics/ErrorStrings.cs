namespace NMLServer.Model.Processors.Diagnostics;

internal static class ErrorStrings
{
    public const string
        Err_UnexpectedToken = "Unexpected token",
        Err_ExpectedSquareBracket = "Expected square bracket for var/param invocation",
        Err_InvalidTarget = "Invalid parameter assignment target",
        Err_MissingSemicolon = "Missing semicolon",
        Err_MissingClosingBracket = "Missing closing bracket",
        Err_MissingAssignedValue = "Missing assigned value",
        Err_UnexpectedTopLevelExpr = "Unexpected expression at top level of the script",
        Err_MissingExpression = "Missing expression",
        Err_UniqueIdentifierExpected = "Unique identifier expected",
        Err_UniqueIdentifierArgsExpected = "Unique identifier and argument list expected",
        Err_ExpectedBlockArguments = "Expected arguments for block",
        Err_UnexpectedBlockArguments = "Unexpected arguments for block",
        Err_ArgumentListInParensExpected = "Expected argument list enclosed in parens",
        Err_MissingLeftParen = "Missing '(' paren",
        Err_MissingRightParen = "Missing ')' paren",
        Err_IncorrectParenTypeLeftParenExpected = "Incorrect paren type, expected '('",
        Err_IncorrectParenTypeRightParenExpected = "Incorrect paren type, expected ')'",
        Err_ExpectedBlockIdentifier = "Expected block identifier",
        Err_InvalidFunctionId = "Invalid function identifier",
        Err_ExpectedFuncArguments = "Expected function arguments",
        Err_ExpectedArguments = "Expected arguments",
        Err_NotEnoughBlockArgs = "Not enough arguments for block",
        Err_ExcessBlockArgs = "Excess arguments for block",
        Err_BlockExpectsArguments = "Block expects arguments",
        Err_BlockExpectsArgument = "Block expects argument",
        Err_ExpectedLeftCurlyBracket = "Expecting opening '{' bracket",
        Err_ExpectedRightCurlyBracket = "Expected closing '}' bracket"
    ;
}