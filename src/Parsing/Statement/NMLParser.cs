using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Statement.Results;


namespace NMLServer.Parsing.Statement;

internal record struct NMLParseResult()
{
    public readonly List<AssignmentStatementParseResult> Assignments = new();
    public readonly List<BlockStatementParseResult> Blocks = new();
    public readonly List<FunctionStatementParseResult> Functions = new();
    public readonly List<byte> Order = new();
}

internal class NMLParser : BaseParser
{
    public static void Apply(out NMLParseResult result)
    {
        result = new NMLParseResult();
        while (Pointer < Max)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case SemicolonToken:
                case UnitToken:
                case ColonToken:
                case FailedToken:
                case UnaryOpToken:
                case BinaryOpToken:
                case TernaryOpToken:
                case AssignmentToken:
                case BracketToken {Bracket: '{' or '}'}:
                    UnexpectedTokens.Add(current);
                    break;

                case BracketToken:                
                case BaseValueToken:
                case KeywordToken { IsExpressionUsable: true }:
                    AssignmentParser.Apply(out var assignment);
                    result.Assignments.Add(assignment);
                    result.Order.Add(1);
                    break;
                
                case KeywordToken { IsBlock: true } keywordToken:
                    BlockParser.BlockLike(keywordToken, out var block);
                    result.Blocks.Add(block);
                    result.Order.Add(2);
                    break;
                case KeywordToken keywordToken:
                    BlockParser.FunctionLike(keywordToken, out var function);
                    result.Functions.Add(function);
                    result.Order.Add(3);
                    break;
            }
        }
        Pointer++;
    }
}

internal static class AssignmentParser
{
    public static void Apply(out AssignmentStatementParseResult result)
    {
        
        
        // TODO
        throw new Exception();
    }
}