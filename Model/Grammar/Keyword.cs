namespace NMLServer.Model.Grammar;

// used bits: 0-3, 4, 8-11, 16-19, 20-23, 24-29, 31
internal enum Keyword
{
    None = 0,

#pragma warning disable format

    _SymbolKind_Mask        = 0xF,

    _Kind_Shift = 2 * 4,
    _Kind_Mask              = 0xF << _Kind_Shift,
    _Kind_None              = None,
    _Kind_ExpressionUsable  = 0x1 << _Kind_Shift,
    _Kind_DefinesStatement  = 0x2 << _Kind_Shift,
    _Kind_Block             = (0x4 << _Kind_Shift) | _Kind_DefinesStatement,
    _Kind_Call              = (0x8 << _Kind_Shift) | _Kind_DefinesStatement,

    _SymbolIndex_Shift      = 3 * 4,
    _SymbolIndex_Mask       = 0xF << _SymbolIndex_Shift,
    _SymbolIndex_0          = 1 << _SymbolIndex_Shift,
    _SymbolIndex_1          = 2 << _SymbolIndex_Shift,
    _SymbolIndex_2          = 3 << _SymbolIndex_Shift,
    _SymbolIndex_3          = 4 << _SymbolIndex_Shift,
    _SymbolIndex_4          = 5 << _SymbolIndex_Shift,
    _SymbolIndex_5          = 6 << _SymbolIndex_Shift,

    _MinParams_Shift        = 4 * 4,
    _MinParams_Mask         = 0xF << _MinParams_Shift,
    _MinParams_0            = 1 << _MinParams_Shift,
    _MinParams_1            = 2 << _MinParams_Shift,
    _MinParams_2            = 3 << _MinParams_Shift,
    _MinParams_3            = 4 << _MinParams_Shift,
    _MinParams_4            = 5 << _MinParams_Shift,
    _MinParams_5            = 6 << _MinParams_Shift,
    _MinParams_6            = 7 << _MinParams_Shift,

    _MaxParams_Shift        = 5 * 4,
    _MaxParams_Mask         = 0xF << _MaxParams_Shift,
    _MaxParams_0            = 1 << _MaxParams_Shift,
    _MaxParams_1            = 2 << _MaxParams_Shift,
    _MaxParams_2            = 3 << _MaxParams_Shift,
    _MaxParams_3            = 4 << _MaxParams_Shift,
    _MaxParams_4            = 5 << _MaxParams_Shift,
    _MaxParams_5            = 6 << _MaxParams_Shift,
    _MaxParams_6            = 7 << _MaxParams_Shift,
    _MaxParams_7            = 8 << _MaxParams_Shift,

    // TODO finish SymbolIndex, SymbolKind; implement custom identifier providers

    _Type_Shift             = 6 * 4,    // they only use up to 6 bits
    AlternativeSprites      = (01 << _Type_Shift) | _Kind_Block | _MinParams_3 | _MaxParams_5 | _RequiresParens,
    BaseCost                = (02 << _Type_Shift) | _Kind_Block | _MinParams_0 | _MaxParams_0,
    BaseGraphics            = (03 << _Type_Shift) | _Kind_Block | _MinParams_1 | _MaxParams_2 | _RequiresParens,
    CargoTable              = (04 << _Type_Shift) | _Kind_Block | _MinParams_0 | _MaxParams_0,
    Const                   = (05 << _Type_Shift) | _Kind_DefinesStatement,
    Deactivate              = (06 << _Type_Shift) | _Kind_Call | _MinParams_1 | _RequiresParens,
    DisableItem             = (07 << _Type_Shift) | _Kind_Call | _MinParams_1 | _MaxParams_3 | _RequiresParens,
    Else                    = (08 << _Type_Shift) | _Kind_Block | _MinParams_0 | _MaxParams_0,
    EngineOverride          = (09 << _Type_Shift) | _Kind_Call | _MinParams_1 | _MaxParams_2 | _RequiresParens,
    Error                   = (10 << _Type_Shift) | _Kind_Call | _MinParams_2 | _MaxParams_5 | _RequiresParens,
    FontGlyph               = (11 << _Type_Shift) | _Kind_Block | _MinParams_2 | _MaxParams_3 | _RequiresParens,
    Graphics                = (12 << _Type_Shift) | _Kind_Block | _MinParams_0 | _MaxParams_0,
    Grf                     = (13 << _Type_Shift) | _Kind_Block | _MinParams_0 | _MaxParams_0,
    If                      = (14 << _Type_Shift) | _Kind_Block | _MinParams_1 | _MaxParams_1 | _RequiresParens,
    Item                    = (15 << _Type_Shift) | _Kind_Block | _MinParams_1 | _MaxParams_4 | _RequiresParens | _SymbolIndex_1 | SymbolKind.Variable |  _AllowsRedefinitions,
    LiveryOverride          = (16 << _Type_Shift) | _Kind_Block | _MinParams_1 | _MaxParams_1 | _RequiresParens,
    Param                   = (17 << _Type_Shift) | _Kind_ExpressionUsable | _MinParams_1 | _MaxParams_1,
    Produce                 = (18 << _Type_Shift) | _Kind_Block | _MinParams_6 | _MaxParams_7 | _RequiresParens | _SymbolIndex_0 | SymbolKind.Function,
    Property                = (19 << _Type_Shift) | _Kind_Block | _MinParams_0 | _MaxParams_0,
    RailTypeTable           = (20 << _Type_Shift) | _Kind_Block | _MinParams_0 | _MaxParams_0,
    RandomSwitch            = (21 << _Type_Shift) | _Kind_Block | _MinParams_3 | _MaxParams_4 | _RequiresParens | _SymbolIndex_2 | SymbolKind.Function,
    RecolourSprite          = (22 << _Type_Shift) | _Kind_Block | _MinParams_0 | _MaxParams_0,
    Replace                 = (23 << _Type_Shift) | _Kind_Block | _MinParams_1 | _MaxParams_2 | _RequiresParens,
    ReplaceNew              = (24 << _Type_Shift) | _Kind_Block | _MinParams_1 | _MaxParams_3 | _RequiresParens,
    Return                  =  25 << _Type_Shift,
    RoadTypeTable           = (26 << _Type_Shift) | _Kind_Block | _MinParams_0 | _MaxParams_0,
    SnowLine                = (27 << _Type_Shift) | _Kind_Block | _MinParams_1 | _MaxParams_1 | _RequiresParens,
    Sort                    = (28 << _Type_Shift) | _Kind_Call | _MinParams_2 | _MaxParams_2 | _RequiresParens,
    SpriteGroup             = (29 << _Type_Shift) | _Kind_Block | _MinParams_1 | _MaxParams_1 | _SymbolIndex_0 | SymbolKind.Constant,
    SpriteLayout            = (30 << _Type_Shift) | _Kind_Block | _MinParams_1 | _MaxParams_1 | _SymbolIndex_0 | SymbolKind.Constant,
    SpriteSet               = (31 << _Type_Shift) | _Kind_Block | _MinParams_1 | _MaxParams_5 | _RequiresParens | _SymbolIndex_0 | SymbolKind.Constant,
    String                  = (32 << _Type_Shift) | _Kind_ExpressionUsable | _MinParams_1 | _MaxParams_1 | _RequiresParens,
    Switch                  = (33 << _Type_Shift) | _Kind_Block | _MinParams_4 | _RequiresParens | _SymbolIndex_2 | SymbolKind.Function,
    Template                = (34 << _Type_Shift) | _Kind_Block | _MinParams_0 | _RequiresParens | SymbolKind.Function,
    TileLayout              = (35 << _Type_Shift) | _Kind_Block | _MinParams_1 | _MaxParams_1 | _SymbolIndex_0 | SymbolKind.Constant,
    TownNames               = (36 << _Type_Shift) | _Kind_Block | _MinParams_1 | _MaxParams_1 | _SymbolIndex_0 | _RequiresParens | SymbolKind.Constant,
    TramTypeTable           = (37 << _Type_Shift) | _Kind_Block | _MinParams_0 | _MaxParams_0,
    Var                     = (38 << _Type_Shift) | _Kind_ExpressionUsable | _MinParams_4 | _MaxParams_4 | _RequiresParens,
    While                   = (39 << _Type_Shift) | _Kind_Block | _MinParams_1 | _MaxParams_1 | _RequiresParens,

    _AllowsRedefinitions    = 1 << (7 * 4 + 2),
    _RequiresParens         = 1 << (7 * 4 + 3),
#pragma warning restore format
}