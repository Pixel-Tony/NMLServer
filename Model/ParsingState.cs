using NMLServer.Model.Tokens;

namespace NMLServer.Model;

internal struct ParsingState(IReadOnlyList<BaseToken> tokens, int offset = 0)
{
    public readonly List<BaseToken> UnexpectedTokens = [];
    private readonly int _max = tokens.Count - 1;
    private int _offset = offset;

    public readonly void AddUnexpected(BaseToken token)
    {
        if (token is not CommentToken)
            UnexpectedTokens.Add(token);
    }

    public void Increment() => ++_offset;

    public readonly BaseToken? CurrentToken => _offset <= _max ? tokens[_offset] : null;

    public BaseToken? NextToken => ++_offset <= _max ? tokens[_offset] : null;
}