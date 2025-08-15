global using Range = EmmyLua.LanguageServer.Framework.Protocol.Model.DocumentRange;
global using StringView = System.ReadOnlySpan<char>;
global using SourceStorage = System.Collections.Generic.Dictionary<
    EmmyLua.LanguageServer.Framework.Protocol.Model.DocumentUri,
    NMLServer.Model.Document
>;
global using AmendResult = (
    EmmyLua.LanguageServer.Framework.Protocol.Model.Position start,
    EmmyLua.LanguageServer.Framework.Protocol.Model.Position oldEnd,
    EmmyLua.LanguageServer.Framework.Protocol.Model.Position newEnd
);
global using ParentStack = System.Collections.Generic.List<(NMLServer.Model.Statements.Blocks.BaseParentStatement? parent, int index)>;