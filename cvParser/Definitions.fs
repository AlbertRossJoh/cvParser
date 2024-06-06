module cvParser.Definitions

    open System.Collections
    open System.Collections.Generic
    open System.Text.Json
    open FuzzySharp
    open Graph.Types

   
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
        | Extract
    
    let (.+.) a b = Add (a, b)
    let (.-.) a b = Sub (a, b)
    let (.*.) a b = Mul (a, b)
    let (./.) a b = Div (a, b)
    let (.%.) a b = Mod (a, b)
    
        
    
    let mp f (enum: IEnumerable<'a>) =
        Some(f enum |> Option.get)
    

    let rec firstDefElm (st: JsonElement) =
        match st.ValueKind with
        | JsonValueKind.Array -> st.EnumerateArray() |> Seq.tryHead
        | JsonValueKind.Object -> st.EnumerateObject() |> Seq.tryHead |> Option.map _.Value
        | _ -> Some(st)
            
    let lastDefElm (st: JsonElement) =
        match st.ValueKind with
        | JsonValueKind.Array -> st.EnumerateArray() |> Seq.tryLast
        | JsonValueKind.Object -> st.EnumerateObject() |> Seq.tryLast |> Option.map _.Value
        | _ -> Some(st)
        
    let likeDefElm term (st: JsonElement) =
        let search arr = Process.ExtractOne(term, arr)
        match st.ValueKind with
        | JsonValueKind.Array ->
            let enum = st.EnumerateArray()
            let res =
                enum
                |> Seq.map _.ToString()
                |> Seq.toArray
                |> search
            enum |> Seq.tryFind (fun x -> x.ToString() = res.Value)
        | JsonValueKind.Object ->
            let enum = st.EnumerateObject()
            let result =
                enum
                |> Seq.map (fun (x: JsonProperty) -> x.Name)
                |> Seq.toArray
                |> search
            enum |> Seq.tryFind (fun x -> x.Name = result.Value) |> Option.map _.Value
        | _ -> Some(st)

    let rec idxDefElm idx (st: JsonElement) =
        match st.ValueKind with
        | JsonValueKind.Array ->
            let st = st.EnumerateArray()
            st |> Seq.tryItem idx
        | JsonValueKind.Object ->
             let st = st.EnumerateObject()
             st |> Seq.tryItem idx |> Option.map _.Value
        | _ -> st |> Some

        
    let rec getDefElm item (st:JsonElement) =
        match st.ValueKind with
        | JsonValueKind.Object -> st.EnumerateObject() |> Seq.tryFind (fun x -> x.Name = item) |> Option.map _.Value
        | JsonValueKind.Array -> st.EnumerateArray() |> Seq.tryFind (fun x -> x.ToString() = item) 
        | _ -> st |> Some
    
    let extractDefElm (st: JsonElement) =

        st
    
    
    let first = SM(fun st -> firstDefElm st |> Option.map (fun x -> (), x))
    let last = SM(fun st -> lastDefElm st |> Option.map (fun x -> (), x))
    let like term = SM(fun st -> likeDefElm term st |> Option.map (fun x -> (), x))
    let index idx = SM(fun st -> idxDefElm idx st |> Option.map (fun x -> (), x))
    let get term = SM(fun st -> getDefElm term st |> Option.map (fun x -> (), x))
    let extract = SM(fun st -> Some(extractDefElm st, st))
    let binop f sm1 sm2 =
        sm1 >>= fun i1 ->
        sm2 >>= fun i2 ->
        ret (f i1 i2)
    
    let rec arithEval a : StateMonad<int, JsonElement> =
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
    
    let rec queryEval a : StateMonad<JsonElement, JsonElement> =
        match a with
        | Last exp -> last >>>= queryEval exp
        | First exp -> first >>>= queryEval exp
        | Get (item, exp) -> get item >>>= queryEval exp
        | Index (exp, qExp) ->
            // arithEval exp >>=
            index exp >>>= queryEval qExp
        | Like (s, qExp) -> like s >>>= queryEval qExp
        | Extract -> extract
    let eval data (SM(f)) = f data
