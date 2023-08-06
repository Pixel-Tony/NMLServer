using NMLServer.Lexing;
using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing;

internal class Parser
{
    private IEnumerator<Token>? _tokens;

    public void Parse(Lexer lexer)
    {
        try
        {
            _tokens = lexer.Tokenize().GetEnumerator();

            throw new NotImplementedException();
        }
        finally
        {
            _tokens?.Dispose();
        }
    }
}