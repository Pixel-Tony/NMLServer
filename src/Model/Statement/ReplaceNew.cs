using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class ReplaceNew(ref ParsingState state, KeywordToken keyword) : BaseSpriteHolder(ref state, keyword);