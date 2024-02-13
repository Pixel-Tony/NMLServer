using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal sealed class TernaryOperation : ExpressionAST
{
    public const int Precedence = Grammar.TernaryOperatorPrecedence;

    private ExpressionAST? _condition;
    private readonly TernaryOpToken _questionMark;
    public ExpressionAST? TrueBranch;
    public ColonToken? Colon;
    public ExpressionAST? FalseBranch;

    public override int start => _condition?.start ?? _questionMark.Start;

    public override int end
        => FalseBranch?.end ?? (
            Colon is not null
                ? Colon.Start + 1
                : TrueBranch?.end ?? _questionMark.Start + 1
        );

    public TernaryOperation(ExpressionAST? parent, TernaryOpToken questionMark) : base(parent)
    {
        _questionMark = questionMark;
    }

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