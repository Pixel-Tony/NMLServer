using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement.Results;

internal record struct StatementBody
{
    public BracketToken? OpeningBracket;
    public readonly List<NMLAttribute> Attributes;
    public readonly List<BlockStatementParseResult> Blocks;
    public readonly List<NamesAttribute> NamesBlocks;
    public BracketToken? ClosingBracket;

    public StatementBody()
    {
        Attributes = new List<NMLAttribute>();
        Blocks = new List<BlockStatementParseResult>();
        NamesBlocks = new List<NamesAttribute>();
    }

    public StatementBody(int attributesSize, int blocksSize, int namesBlocksSize)
    {
        Attributes = new List<NMLAttribute>(attributesSize);
        Blocks = new List<BlockStatementParseResult>(blocksSize);
        NamesBlocks = new List<NamesAttribute>(namesBlocksSize);
    }
}