namespace NMLServer;

internal interface IValidatable
{
    /*
     * List of diagnostics in the order of declining importance
     *
     * 1 [ERROR]
     * For every statement - check if all elements of AST are present
     *
     * 2 [ERROR]
     * Check if all expression AST elements are present and of correct type: no mismatched parenthesis, no missing
     * parameters to operators, etc.
     *
     * 3 [ERROR]
     * For every expression in attribute setters, parameters etc. - check for evaluated type to match expected.
     * TODO: merge into BaseStatement
     */
    public void ProvideDiagnostics(DiagnosticsContext context);
}