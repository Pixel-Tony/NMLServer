using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class Replace(ParsingState state, KeywordToken keyword) : BaseSpriteHolder(state, keyword);