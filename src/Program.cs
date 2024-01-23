using System.Text;
using LanguageServer;
using LanguageServer.Client;
using LanguageServer.Parameters;
using LanguageServer.Parameters.General;
using LanguageServer.Parameters.TextDocument;
using LanguageServer.Parameters.Window;
using LanguageServer.Parameters.Workspace;
using NMLServer.Lexing;
using NMLServer.Lexing.Tokens;
using NMLServer.Parsing;
using NMLServer.Parsing.Statement;

namespace NMLServer;

public static class Logger
{
    public static Proxy? A;

    public static void Log(string message)
    {
        A?.Window.LogMessage(new LogMessageParams
        {
            type = MessageType.Log,
            message = "[info] " + message
        });
    }
}

internal class DocumentManager
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
        SetContext(document.text);

        var tokens = new Lexer(document.text).Process().tokens;
        Logger.Log($"lexed {tokens.Length} items");

        var state = new ParsingState(tokens);
        var result = new NMLFile(state);
        Logger.Log($"parsed {result.children.Count()} items until the end!");

        var unexpectedTokens = state.unexpectedTokens.Take(100).ToArray();

        List<Diagnostic> diagnostics = new(200);
        foreach (var unexpectedToken in unexpectedTokens)
        {
            diagnostics.Add(
                new Diagnostic
                {
                    severity = DiagnosticSeverity.Error,
                    message = "Unexpected token",
                    range = new LanguageServer.Parameters.Range
                    {
                        start = this[unexpectedToken.Start],
                        end = this[unexpectedToken is MulticharToken hasEnd
                            ? hasEnd.End
                            : unexpectedToken.Start + 1]
                    }
                }
            );
        }
        a.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams
        {
            diagnostics = diagnostics.ToArray(),
            uri = document.uri
        });
    }

    public void Change(VersionedTextDocumentIdentifier document, IEnumerable<TextDocumentContentChangeEvent> events,
        Proxy a)
    {
        var target = _textDocuments.Find(d => d.uri == document.uri);
        if (target is null)
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
        ++target.version;

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

    private void SetContext(string current)
    {
        _current = current;
    }

    private string? _current;

    private Position this[int start]
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
    private static readonly DocumentManager _documents = new();

    public Application(Stream input, Stream output) : base(input, output)
    {
        Logger.A = Proxy;
    }

    protected override Result<InitializeResult, ResponseError<InitializeErrorData>> Initialize(InitializeParams @params)
    {
        Logger.Log("Server is online.");
        return Result<InitializeResult, ResponseError<InitializeErrorData>>.Success(new InitializeResult
            {
                capabilities = new ServerCapabilities
                {
                    textDocumentSync = TextDocumentSyncKind.Full,
                    colorProvider = true
                }
            }
        );
    }

    protected override void DidChangeConfiguration(DidChangeConfigurationParams @params)
    {
        Logger.Log("Changed configuration.");
    }

    protected override void DidChangeTextDocument(DidChangeTextDocumentParams @params)
    {
        Logger.Log("Changed doc!");
        _documents.Change(@params.textDocument, @params.contentChanges, Proxy);
    }

    protected override void DidCloseTextDocument(DidCloseTextDocumentParams @params)
    {
        Logger.Log("Closed a document.");
        _documents.Remove(@params.textDocument.uri);
    }

    protected override void DidOpenTextDocument(DidOpenTextDocumentParams @params)
    {
        Logger.Log("Opened a new doc.");
        _documents.Add(@params.textDocument, Proxy);
    }

    protected override VoidResult<ResponseError> Shutdown()
    {
        Logger.Log("Language Server is about to shutdown.");
        _isOpen = false;
        return VoidResult<ResponseError>.Success();
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