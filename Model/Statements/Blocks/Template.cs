using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

// TODO templates use `function(...params)` syntax in parameter part, this needs separate handling
internal sealed class Template(ref ParsingState state, KeywordToken keyword) : BaseSpriteHolder(ref state, keyword);