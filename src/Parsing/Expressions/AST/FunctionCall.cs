using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class FunctionCall : ExpressionAST
{
    public readonly BaseRecordingToken Function;
    public BracketToken? OpeningBracket;
    public ExpressionAST? Arguments;
    public BracketToken? ClosingBracket;

    public FunctionCall(ExpressionAST? parent, BaseRecordingToken token) : base(parent)
    {
        Function = token;
    }
    
    public override string ToString()
    {
        return $"(Func: {Function.value}{OpeningBracket?.Bracket ?? '.'}{Arguments}{ClosingBracket?.Bracket ?? '.'})";
    }
}