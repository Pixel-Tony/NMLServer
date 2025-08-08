global using Range = EmmyLua.LanguageServer.Framework.Protocol.Model.DocumentRange;
global using StringView = System.ReadOnlySpan<char>;
global using SourceStorage = System.Collections.Generic.Dictionary<
    EmmyLua.LanguageServer.Framework.Protocol.Model.DocumentUri,
    NMLServer.Model.Document
>;