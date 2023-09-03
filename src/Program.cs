using System.Text;
using LanguageServer;
using LanguageServer.Client;
using LanguageServer.Parameters;
using LanguageServer.Parameters.General;
using LanguageServer.Parameters.TextDocument;
using LanguageServer.Parameters.Window;
using LanguageServer.Parameters.Workspace;
using NMLServer.Lexing;
using NMLServer.Parsing;
using NMLServer.Parsing.Statement;

namespace NMLServer;

internal interface IPositionConverter
{
    public void SetContext(string text);

    public Position this[int pos] { get; }
}

internal class Analyzer
{
    public void Analyze(GRFBlock tree, Proxy a, Uri uri, IPositionConverter converter, string s)
    {
        List<Diagnostic> diagnostics = new();

        tree.Analyze(diagnostics, converter, s);

        BaseParser.GetUnexpectedTokensDiagnostics(diagnostics, converter);

        a.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams
        {
            diagnostics = diagnostics.ToArray(),
            uri = uri
        });
    }
}

internal class DocumentManager : IPositionConverter
{
    private readonly List<TextDocumentItem> _textDocuments = new(3);

    public void Add(TextDocumentItem document, Proxy a)
    {
        if (_textDocuments.Exists(d => d.uri == document.uri))
        {
            throw new Exception("Specified document is already added");
        }
        _textDocuments.Add(document);
        Analyze(document, a);
    }

    private void Analyze(TextDocumentItem document, Proxy a)
    {
        (this as IPositionConverter).SetContext(document.text);
        BaseParser.Use(new Lexer(document.text).Tokenize().tokens);

        NMLParser.Apply(out var result);
        //TODO
    }
    
    public void Change(VersionedTextDocumentIdentifier document, TextDocumentContentChangeEvent[] events, Proxy a)
    {
        var target = _textDocuments.Find(d => d.uri == document.uri);
        if (target == null)
        {
            throw new Exception();
        }

        if (target.version >= document.version)
        {
            return;
        }

        foreach (var change in events)
        {
            target.text = change.text;
        }
        target.version++;

        Analyze(target, a);
    }

    public void Remove(Uri uri)
    {
        int index = _textDocuments.FindIndex(d => d.uri == uri);
        if (index == -1)
        {
            throw new Exception();
        }
        _textDocuments.RemoveAt(index);
    }

    public void SetContext(string current)
    {
        _current = current;
    }

    private string? _current;

    public Position this[int start]
    {
        get
        {
            int line = 0;
            var lines = _current!.Split('\n');
            while (start > lines[line].Length)
            {
                start -= lines[line].Length + 1;
                line++;
            }
            return new Position
            {
                line = line,
                character = start
            };
        }
    }
}

internal class Application : ServiceConnection
{
    private readonly DocumentManager _documents = new();

    public Application(Stream input, Stream output) : base(input, output)
    { }

    protected override Result<InitializeResult, ResponseError<InitializeErrorData>> Initialize(InitializeParams @params)
    {
        Log("Server is online.");
        return Result<InitializeResult, ResponseError<InitializeErrorData>>.Success(new InitializeResult
            { capabilities = new ServerCapabilities { textDocumentSync = TextDocumentSyncKind.Full } }
        );
    }

    protected override void DidChangeConfiguration(DidChangeConfigurationParams @params)
    {
        Log("Changed configuration.");
    }

    protected override void DidChangeTextDocument(DidChangeTextDocumentParams @params)
    {
        Log("Changed doc!");
        _documents.Change(@params.textDocument, @params.contentChanges, Proxy);
    }

    protected override void DidCloseTextDocument(DidCloseTextDocumentParams @params)
    {
        Log("Closed a document");
        _documents.Remove(@params.textDocument.uri);
    }

    protected override void DidOpenTextDocument(DidOpenTextDocumentParams @params)
    {
        Log("Opened a new doc.");
        _documents.Add(@params.textDocument, Proxy);
    }

    protected override VoidResult<ResponseError> Shutdown()
    {
        Log("Language Server is about to shutdown.");
        _isOpen = false;
        return VoidResult<ResponseError>.Success();
    }

    private void Log(string message)
    {
        Proxy.Window.LogMessage(new LogMessageParams
        {
            type = MessageType.Log,
            message = message
        });
    }

    private bool _isOpen;
    
    public async Task ListenUntilShutdown()
    {
        _isOpen = true;
        while (_isOpen)
        {
            try
            {
                if (!await ReadAndHandle())
                    break;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
    }
}

internal class Program
{
    internal static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        var app = new Application(Console.OpenStandardInput(), Console.OpenStandardOutput());
        try
        {
            await app.ListenUntilShutdown();
        }
        catch (AggregateException ex)
        {
            Console.Error.WriteLine(ex.InnerExceptions[^1]);
            Environment.Exit(-1);
        }
    }
}