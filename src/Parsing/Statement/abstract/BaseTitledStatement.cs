﻿using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal abstract class BaseTitledStatement : BaseStatementAST
{
    protected readonly KeywordToken Type;

    protected BaseTitledStatement(BaseStatementAST? parent, KeywordToken statementType) : base(parent) =>
        Type = statementType;
}