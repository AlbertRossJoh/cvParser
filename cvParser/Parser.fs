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
    let vals : Parser<string> = pstring "vals"
    let list : Parser<string> = pstring "list"
    let index : Parser<int32> = pstring "index" >*>. pint32
    let like : Parser<char list> = pstring "like" >*>. pchar ''' >>. many palphanumeric .>> pchar '''
    let extract : Parser<string> = pstring "select"
    let pipe : Parser<char> = pchar '|'
    
    

    
    let QueryParse, qref = createParserForwardedToRef()
    // let PipeParse, pref = createParserForwardedToRef()
    
    let queryParse = choice [
        get .>*>. opt QueryParse |>> fun (x, y) -> Get(System.String.Concat x, Option.defaultValue Extract y)
        first >*>. opt QueryParse |>> fun y -> First(Option.defaultValue Extract y) 
        last >*>. opt QueryParse |>> fun y -> Last(Option.defaultValue Extract y) 
        list >*>. opt QueryParse |>> fun _ -> List
        vals >*>. opt QueryParse |>> fun _ -> Vals
        index .>*>. opt QueryParse |>> fun (x, y) -> Index(x, Option.defaultValue Extract y) 
        like .>*>. opt QueryParse |>> fun (x, y) -> Like(System.String.Concat x, Option.defaultValue Extract y)
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