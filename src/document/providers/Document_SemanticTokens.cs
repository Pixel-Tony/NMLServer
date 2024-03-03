using NMLServer.Lexing;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;

namespace NMLServer;

internal partial class Document
{
    // TODO: full -> incremental
    public void ProvideSemanticTokens(SemanticTokensBuilder builder)
    {
        foreach (var token in _tokens)
        {
            if (!Grammar.GetTokenSemanticType(token, out var type))
            {
                continue;
            }
            // only comments are allowed to span multiple lines
            if (token is not CommentToken)
            {
                var (line, @char) = LocalToProtocol(token.start);
                builder.Push(line, @char, token.length, type);
                continue;
            }
            foreach (var (line, @char, length) in LocalToProtocol(token.start, token.length))
            {
                builder.Push(line, @char, length, type);
            }
        }
    }
}