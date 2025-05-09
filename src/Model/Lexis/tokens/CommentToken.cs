using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;

namespace NMLServer.Model.Lexis;

internal sealed class CommentToken(int start, int end) : BaseMulticharToken(start, end)
{
     internal override string semanticType => SemanticTokenTypes.Comment;
}