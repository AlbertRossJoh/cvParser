module cvParser.Definitions

    open System.Collections.Generic
    open System.Text.Json
    open System.Text.Json.Nodes
    open FuzzySharp
    open Types

    
   
    type aExp =
        | N of int
        | Add of aExp * aExp
        | Sub of aExp * aExp
        | Mul of aExp * aExp
        | Div of aExp * aExp
        | Mod of aExp * aExp
    
    type qExp =
        | Last of qExp
        | First of qExp
        | Like of string*qExp
        | Index of int32*qExp
        | Get of string*qExp
        | Let of string*qExp
        | Extract
        | List 
        | Vals 
    
    let (.+.) a b = Add (a, b)
    let (.-.) a b = Sub (a, b)
    let (.*.) a b = Mul (a, b)
    let (./.) a b = Div (a, b)
    let (.%.) a b = Mod (a, b)
    
    
    let incr (st: state) =
        {st with lineNr = st.lineNr+1u}

    let rec firstDefElm (st: state) =
        Option.map (fun (elm: JsonElement) ->
            match elm.ValueKind with
            | JsonValueKind.Array -> {st with elm = elm.EnumerateArray() |> Seq.tryHead}
            | JsonValueKind.Object -> {st with elm = elm.EnumerateObject() |> Seq.tryHead |> Option.map _.Value}
            | _ -> st
        ) st.elm
            
    let lastDefElm (st: state) =
        Option.map (fun (elm: JsonElement) ->
            match elm.ValueKind with
            | JsonValueKind.Array -> {st with elm = elm.EnumerateArray() |> Seq.tryLast}
            | JsonValueKind.Object -> {st with elm = elm.EnumerateObject() |> Seq.tryLast |> Option.map _.Value}
            | _ -> st
        ) st.elm
        
    let likeDefElm term (st: state) =
        Option.map (fun (elm: JsonElement) ->
            let search arr = Process.ExtractOne(term, arr)
            match elm.ValueKind with
            | JsonValueKind.Array ->
                let enum = elm.EnumerateArray()
                let res =
                    enum
                    |> Seq.map _.ToString()
                    |> Seq.toArray
                    |> search
                {st with elm = enum |> Seq.tryFind (fun x -> x.ToString() = res.Value)}
            | JsonValueKind.Object ->
                let enum = elm.EnumerateObject()
                let result =
                    enum
                    |> Seq.map (fun (x: JsonProperty) -> x.Name)
                    |> Seq.toArray
                    |> search
                {st with elm = enum |> Seq.tryFind (fun x -> x.Name = result.Value) |> Option.map _.Value}
            | _ -> st
        ) st.elm

    let rec idxDefElm idx (st: state) =
        Option.map (fun (elm: JsonElement) ->
            match elm.ValueKind with
            | JsonValueKind.Array ->
                {st with elm = elm.EnumerateArray() |> Seq.tryItem idx}
            | JsonValueKind.Object ->
                {st with elm = elm.EnumerateObject() |> Seq.tryItem idx|> Option.map _.Value}
            | _ -> st
        ) st.elm

        
    let rec getDefElm item (st:state) =
        Option.map (fun (elm: JsonElement) ->
            match elm.ValueKind with
            | JsonValueKind.Object -> {st with elm = (elm.EnumerateObject() |> Seq.tryFind (fun x -> x.Name = item) |> Option.map _.Value)}
            | JsonValueKind.Array -> {st with elm = elm.EnumerateArray() |> Seq.tryFind (fun x -> x.ToString() = item)}
            | _ -> st
        ) st.elm
        
    let rec listDefElm (st:state) =
        Option.map (fun (elm: JsonElement) ->
            match elm.ValueKind with
            | JsonValueKind.Object ->
                let enum = elm.EnumerateObject()
                           |> Seq.toList
                           |> List.map (_.Name)
                           |> JsonSerializer.Serialize
                           |> JsonDocument.Parse
                           |> _.RootElement
                {st with elm = Some(enum) }
            | JsonValueKind.Array ->
                let enum = elm.EnumerateArray()
                           |> Seq.toList
                           |> List.map (_.ToString())
                           |> JsonSerializer.Serialize
                           |> JsonDocument.Parse
                           |> _.RootElement
                {st with elm = Some(enum) }
            | _ -> st
        ) st.elm
        
    let rec valsDefElm (st:state) =
        Option.map (fun (elm: JsonElement) ->
            match elm.ValueKind with
            | JsonValueKind.Object ->
                let enum = elm.EnumerateObject()
                           |> Seq.toList
                           |> List.map (_.Value)
                           |> JsonSerializer.Serialize
                           |> JsonDocument.Parse
                           |> _.RootElement
                {st with elm = Some(enum) }
            | JsonValueKind.Array ->
                let enum = elm.EnumerateArray()
                           |> Seq.toList
                           |> List.map (_.ToString())
                           |> JsonSerializer.Serialize
                           |> JsonDocument.Parse
                           |> _.RootElement
                {st with elm = Some(enum) }
            | _ -> st
        ) st.elm
    let rec letDefElm name elm (st:state) =
        {st with state = Map.add name elm st.state} |> incr
        
    let extractDefElm (st: state) =
        st.elm
    
    
    let first = SM(fun st -> firstDefElm st |> Option.map (fun x -> (), x))
    let last = SM(fun st -> lastDefElm st |> Option.map (fun x -> (), x))
    let list = SM(fun st -> listDefElm st |> Option.map (fun x -> x.elm, st))
    let vals = SM(fun st -> valsDefElm st |> Option.map (fun x -> x.elm, st))
    let like term = SM(fun st -> likeDefElm term st |> Option.map (fun x -> (), x))
    let index idx = SM(fun st -> idxDefElm idx st |> Option.map (fun x -> (), x))
    let get term = SM(fun st -> getDefElm term st |> Option.map (fun x -> (), x))
    let extract = SM(fun st -> Some(extractDefElm st, st))
    let letSM name elm = SM(fun st -> Option.map(fun elm -> (), letDefElm name elm st) elm)
    let binop f sm1 sm2 =
        sm1 >>= fun i1 ->
        sm2 >>= fun i2 ->
        ret (f i1 i2)
    
    let rec arithEval a : StateMonad<int, state> =
        let nonZeroSndArgBinop f sm1 sm2 =
            sm1 >>= fun i1 ->
            sm2 >>= fun i2 ->
            if i2 <> 0
            then ret (f i1 i2)
            else fail
        match a with
        | N i -> ret i
        | Add (x, y) -> binop (+) (arithEval x) (arithEval y)
        | Sub (x, y) -> binop (-) (arithEval x) (arithEval y)
        | Mul (x, y) -> binop (*) (arithEval x) (arithEval y)
        | Div (x, y) -> nonZeroSndArgBinop (/) (arithEval x) (arithEval y)
        | Mod (x, y) -> nonZeroSndArgBinop (%) (arithEval x) (arithEval y)
    
    let rec queryEval a : StateMonad<JsonElement option, state> =
        match a with
        | Last exp -> last >>>= queryEval exp
        | First exp -> first >>>= queryEval exp
        | Get (item, exp) -> get item >>>= queryEval exp
        | Index (exp, qExp) ->
            // arithEval exp >>=
            index exp >>>= queryEval qExp
        | Like (s, qExp) -> like s >>>= queryEval qExp
        | Extract -> extract
        | Let(s, qExp) ->
            let res = 
                queryEval qExp >>= fun e ->
                letSM s e
            queryEval qExp
        | List -> list
        | Vals -> vals
                
            
    let eval data (SM(f)) = f data
