module cvParser.Definitions

    open System.Collections
    open System.Collections.Generic
    open System.Text.Json
    open FuzzySharp
    open Graph.Types

    type state =
        | Enum of JsonElement.ObjectEnumerator
        | Prop of JsonProperty
        | Arr of JsonElement.ArrayEnumerator
        | Item of string
    
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
    
    let toState = Enum
        
    
    let mp f (enum: IEnumerable<'a>) =
        Some(f enum |> Option.get)
    
    let getArrOrObjEnumElm (st: JsonElement) =
        if st.ValueKind = JsonValueKind.Array then
            Arr(st.EnumerateArray())
        else if st.ValueKind = JsonValueKind.Object then
            Enum(st.EnumerateObject())
        else
            Item(st.ToString())
    let rec getArrOrObjEnum (st: JsonProperty) =
        getArrOrObjEnumElm st.Value
    let rec firstDef st : state option =
        match st with
        | Enum st -> st |> Seq.tryHead |> Option.map Prop
        | Prop st ->
            firstDef (getArrOrObjEnum st)
        | Arr st -> st |> Seq.tryHead |> Option.map getArrOrObjEnumElm
        | st -> st |> Some
    let rec firstDefElm (st: JsonElement) =
        match st.ValueKind with
        | JsonValueKind.Array -> st.EnumerateArray() |> Seq.tryHead
        | JsonValueKind.Object -> st.EnumerateObject() |> Seq.tryHead |> Option.map _.Value
        | _ -> Some(st)
    let rec lastDef st =
        match st with
        | Enum st -> st |> Seq.tryLast |> Option.map Prop
        | Prop st -> getArrOrObjEnum st |> (firstDef)
        | Arr arrayEnumerator ->
            let mutable arr = arrayEnumerator
            if arr.MoveNext() then
                lastDef (Arr(arr))
            else
                arrayEnumerator.Current |> getArrOrObjEnumElm |> Some
        | st -> st |> Some
            
    let lastDefElm (st: JsonElement) =
        match st.ValueKind with
        | JsonValueKind.Array -> st.EnumerateArray() |> Seq.tryLast
        | JsonValueKind.Object -> st.EnumerateObject() |> Seq.tryLast |> Option.map _.Value
        | _ -> Some(st)
        
    let rec likeDef term (st: state) =
        let search arr = Process.ExtractOne(term, arr)
        match st with
        | Enum st -> 
            let result =
                st
                |> Seq.map (fun (x: JsonProperty) -> x.Name)
                |> Seq.toArray
                |> search
            st
            |> Seq.tryFind (fun x -> x.Name = result.Value) |> Option.map Prop 
        | Prop st ->
            getArrOrObjEnum st |> (likeDef term)
        | Arr arrayEnumerator ->
                let result =
                    arrayEnumerator
                    |> Seq.map _.ToString()
                    |> Seq.toArray
                    |> search
                arrayEnumerator
                |> Seq.tryFind (fun x -> x.ToString() = result.Value) |> Option.map getArrOrObjEnumElm
        | st -> st |> Some
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
    let rec idxDef idx st =
        match st with
        | Enum st -> st |> Seq.tryItem idx |> Option.map Prop
        | Prop st -> getArrOrObjEnum st |> (idxDef idx)
        | Arr arrayEnumerator -> arrayEnumerator |> Seq.tryItem idx |> Option.map getArrOrObjEnumElm
        | st -> st |> Some
    let rec idxDefElm idx (st: JsonElement) =
        match st.ValueKind with
        | JsonValueKind.Array ->
            let st = st.EnumerateArray()
            st |> Seq.tryItem idx
        | JsonValueKind.Object ->
             let st = st.EnumerateObject()
             st |> Seq.tryItem idx |> Option.map _.Value
        | _ -> st |> Some
    let rec getDef item st =
        match st with
        | Enum st -> st |> Seq.tryFind (fun x -> x.Name = item) |> Option.map Prop
        | Prop st -> getArrOrObjEnum st |> (getDef item)
        | Arr arrayEnumerator -> arrayEnumerator |> Seq.tryFind (fun x -> x.ToString() = item) |> Option.map getArrOrObjEnumElm
        | st -> st |> Some
        
    let rec getDefElm item (st:JsonElement) =
        match st.ValueKind with
        | JsonValueKind.Object -> st.EnumerateObject() |> Seq.tryFind (fun x -> x.Name = item) |> Option.map _.Value
        | JsonValueKind.Array -> st.EnumerateArray() |> Seq.tryFind (fun x -> x.ToString() = item) 
        | _ -> st |> Some
    
    let extractDef (st: state) =
        let res = 
            match st with
            | Enum st -> st :> obj
            | Prop st -> st.Value :> obj
            | Arr st -> st |> Seq.toList :> obj
            | Item s -> s 
        res
    let extractDefElm (st: JsonElement) =
        // let res = 
        //     match st with
        //     | Enum st -> st :> obj
        //     | Prop st -> st.Value :> obj
        //     | Arr st -> st |> Seq.toList :> obj
        //     | Item s -> s 
        // res
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
