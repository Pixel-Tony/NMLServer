using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;

namespace NMLServer.Model.Lexis;

internal sealed class CommentToken(int start, int end) : BaseMulticharToken(start, end)
{
     public override string SemanticType => SemanticTokenTypes.Comment;
}