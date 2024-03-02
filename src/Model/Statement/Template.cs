using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class Template(ParsingState state, KeywordToken keyword) : BaseSpriteHolder(state, keyword);