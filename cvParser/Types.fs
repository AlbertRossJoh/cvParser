module cvParser.Types

    open System.Text.Json


    type StateMonad<'a, 'state> = SM of ('state -> ('a * 'state) option)  
    let ret x = SM (fun s -> Some (x, s))  
    let fail  = SM (fun _ -> None)  
    let bind f (SM a) : StateMonad<'b, 'state> =   
        SM (fun s ->   
            match a s with   
            | Some (x, s') ->  let (SM g) = f x               
                               g s'  
            | None -> None)
          
    let (>>=) x f = bind f x  
    let (>>>=) x y = x >>= (fun _ -> y)  
      

    type StateBuilder() =
        member this.Bind(f, x)    = bind x f
        member this.Return(x)     = ret x
        member this.ReturnFrom(x) = x
        member this.Combine(a, b) = a >>= (fun _ -> b)

    let state = StateBuilder()
    
    
    type state = {
        elm: JsonElement option
        lineNr: uint32
        state: Map<string, JsonElement>
    }
    
    let mkState (elm: JsonElement) : state = {
        elm = Some(elm)
        lineNr = 0u
        state = Map.empty 
    }
