using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

internal abstract partial class BaseSwitch(ref ParsingState state, KeywordToken keyword)
    : BaseBlockStatement<BaseSwitch.Line>(ref state, keyword);