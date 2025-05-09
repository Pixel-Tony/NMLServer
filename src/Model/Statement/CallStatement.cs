using NMLServer.Model.Lexis;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal sealed class CallStatement : StatementAST
{
    private readonly KeywordToken _keyword;
    private readonly ExpressionAST? _parameters;
    private readonly SemicolonToken? _semicolon;

    public override int start => _keyword.start;
    public override int end => _semicolon?.end ?? (_parameters?.end ?? _keyword.end);

    public CallStatement(ref ParsingState state, KeywordToken keyword)
    {
        _keyword = keyword;
        state.IncrementSkippingComments();
        _parameters = ExpressionAST.TryParse(ref state);
        _semicolon = state.ExpectSemicolon();
    }
}