using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statements.Models;

internal abstract class StatementWithParametersAndBody : StatementWithParameters
{
    public BracketToken? OpeningBracket;
    public BracketToken? ClosingBracket;
}