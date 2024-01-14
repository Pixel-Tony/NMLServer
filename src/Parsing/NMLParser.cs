using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Statements;
using NMLServer.Parsing.Statements.Models;

namespace NMLServer.Parsing;

/* One ring to rule them all */
internal sealed class NMLParser : BlockParser
{
    public static NMLFileRoot Apply()
    {
        var root = new NMLFileRoot();
        while (areTokensLeft)
        {
            var current = Tokens[Pointer];
            var statement = current switch
            {
                IdentifierToken
                // top-level does not allow for "param" block, so the only way it can be used is in the assignment
                or KeywordToken { IsExpressionUsable: true }
                or BracketToken { Bracket: not ('{' or '}') }
                    => AssignmentParser.Apply(),

                KeywordToken { IsBlock: true } keyword
                    => ParseBlockStatement(keyword),

                KeywordToken keywordToken
                    => ParseFunctionStatement(keywordToken),

                _ => null
            };

            if (statement is null)
            {
                UnexpectedTokens.Add(current);
                Pointer++;
                continue;
            }
            root.Children.Add(statement);
        }
        return root;
    }
}