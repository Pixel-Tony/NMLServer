using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class ReplaceNew(ParsingState state, KeywordToken keyword) : BaseSpriteHolder(state, keyword);