namespace NMLServer.Model.Diagnostics;

// TODO implement interface for each StatementAST item, remove interface
internal interface IDiagnosticProvider
{
    void VerifySyntax(ref readonly DiagnosticContext context);
}

// TODO rework
internal interface IContextProvider
{
    void VerifyContext(ref readonly DiagnosticContext context, IDefinitionsBag definitions);
}