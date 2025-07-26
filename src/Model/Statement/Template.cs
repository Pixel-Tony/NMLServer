using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

// TODO templates use `function(...params)` syntax in parameter part, this needs separate handling
internal sealed class Template(ref ParsingState state, KeywordToken keyword)
    : BaseSpriteHolder(ref state, keyword, new ParamInfo(false, (1, 1)));