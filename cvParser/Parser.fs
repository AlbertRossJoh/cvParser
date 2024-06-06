module cvParser.Parser

    open FParsec
    open cvParser.Definitions
    type Parser<'a> = Parser<'a, unit>
    
    let (.>*>.) p1 p2 = p1 .>> spaces .>>. p2
    let (.>*>) p1 p2 = p1 .>> spaces .>> p2
    let (>*>.) p1 p2 = p1 .>> spaces >>. p2
    
    let palphanumeric  = satisfy System.Char.IsLetterOrDigit <?> "alphanumeric"

    let get : Parser<char list> = pstring "get" >*>. pchar ''' >>. many palphanumeric .>> pchar '''
    let first : Parser<string> = pstring "first"
    let last : Parser<string> = pstring "last"
    let index : Parser<int32> = pstring "index" >*>. pint32
    let like : Parser<char list> = pstring "like" >*>. pchar ''' >>. many palphanumeric .>> pchar '''
    let extract : Parser<string> = pstring "select"
    let pipe : Parser<char> = pchar '|'
    
    

    
    let QueryParse, qref = createParserForwardedToRef()
    // let PipeParse, pref = createParserForwardedToRef()
    
    let queryParse = choice [
        get .>*> pipe .>*>. QueryParse |>> fun (x, y) -> Get(System.String.Concat x, y)
        first .>*> pipe >*>. QueryParse |>> First
        last .>*> pipe >*>. QueryParse |>> Last
        index .>*> pipe .>*>. QueryParse |>> Index
        like .>*> pipe .>*>. QueryParse |>> fun (x, y) -> Like(System.String.Concat x, y)
        // get |>> fun x -> Get(System.String.Concat x, Extract)
        // first |>> fun _ -> First(Extract)
        // last |>> fun _ -> First(Extract)
        // index |>> fun x -> Index(x, Extract)
        // like |>> fun x -> Get(System.String.Concat x, Extract)
        extract |>> fun _ -> Extract
        satisfy (fun _ -> true) |>> fun _ -> Extract
    ]
    do qref := queryParse
    let getSuccess : ParserResult<'a, unit> -> 'a =
        function
        | Success(s, _, _) -> s
        | failure          -> failwith (sprintf "%A" failure)