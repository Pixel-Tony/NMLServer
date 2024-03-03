using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal sealed class TernaryOperation(ExpressionAST? parent, TernaryOpToken questionMark) : ExpressionAST(parent)
{
    public const int Precedence = Grammar.TernaryOperatorPrecedence;

    private ExpressionAST? _condition;
    private readonly TernaryOpToken _questionMark = questionMark;
    public ExpressionAST? TrueBranch;
    public ColonToken? Colon;
    public ExpressionAST? FalseBranch;

    public override int start => _condition?.start ?? _questionMark.start;

    public override int end
        => FalseBranch?.end ?? (
            Colon is not null
                ? Colon.start + 1
                : TrueBranch?.end ?? _questionMark.start + 1
        );

    public TernaryOperation(ExpressionAST? parent, ExpressionAST? condition, TernaryOpToken questionMark)
        : this(parent, questionMark)
    {
        _condition = condition;
    }

    protected override void Replace(ExpressionAST target, FunctionCall value)
    {
        if (target == _condition)
        {
            _condition = value;
        }
        else if (target == TrueBranch)
        {
            TrueBranch = value;
        }
        else
        {
            FalseBranch = value;
        }
    }
}