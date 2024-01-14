global using NMLAttribute = NMLServer.Parsing.Pair<
    NMLServer.Lexing.Tokens.IdentifierToken,
    NMLServer.Parsing.Expression.ExpressionAST
>;
global using NamesAttribute = NMLServer.Parsing.Pair<
    NMLServer.Lexing.Tokens.IdentifierToken,
    NMLServer.Parsing.Statements.Models.ParameterNames
>;
global using Range = LanguageServer.Parameters.Range;