using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class Template(ref ParsingState state, KeywordToken keyword) : BaseSpriteHolder(ref state, keyword);