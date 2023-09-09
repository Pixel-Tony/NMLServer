# Grammar

`grfblock is`
    
    <kw:grf> "{"
        [<id> : <expr> | <paramblock>]*
    "}"

`paramblock is` 
    
    <kw:param> [<num>] "{"
        <name> "{"
            <id> ":" <expr> 
                        | "int"
                        | "bool"
                        | "{" [<num> ":" <expr> ";"]* "}" 
            ";"
        "}"
    "}"

`itemblock is`

    <kw:item> "(" <feat>, <id> ")" "{"
        <kw:property> "{"
            [<id> ":" <expr> ";"]*
        "}"
    }

