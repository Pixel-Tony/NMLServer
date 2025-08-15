using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentSymbol;
using NMLServer.Model.Syntax;
using NMLServer.Model.Processors;
using NMLServer.Model.Statements;

namespace NMLServer.Model;

class DocumentSymbolProcessor : IIncrementalNodeProcessor
{
    public readonly List<DocumentSymbol> Content = [];

    public void Trim()
    {
        //
    }

    public void Process(BaseStatement node, NodeProcessingContext context)
    {
        //
    }

    public void FinishIncrement(ref readonly AbstractSyntaxTree ast)
    {
        //
        // private static void TryAddSymbol()
        // {
        //     var start = converter.LocalToProtocol(child.Start);
        //     var symPos = converter.LocalToProtocol(symbol.Start);
        //     var end = converter.Copy().LocalToProtocol(child.End);
        //     var range = new Range(start, end);
        //     var symRange = new Range(symPos, symPos with { Character = symPos.Character + symbol.Length });
        //     symbols.Add(new DocumentSymbol()
        //     {
        //         Kind = GetDocumentSymbolKind(symbol.Kind),
        //         Children = parentList,
        //         Range = range,
        //         SelectionRange = symRange,
        //         Name = tokens.GetSymbolContext(symbol).ToString()
        //     });
        // }
    }
}
