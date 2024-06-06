module cvParser.api

    open System.Text.Json
    open FParsec
    open Parser
    open Definitions
    open JsonParse

    let state = (eval (data.RootElement))
    let query q =
        let err1 = System.Text.Encoding.ASCII.GetBytes(@"{""error"": ""The query did not return anything""}")
        let err2 = System.Text.Encoding.ASCII.GetBytes(@"{""error"": ""The query was malformed""}")
        try
            let res = q |> run queryParse |> getSuccess |> queryEval |> state |> Option.map fst
            if Option.isSome res then
                Option.get res
            else
                JsonDocument.Parse(err1).RootElement
        with
           | _ -> JsonDocument.Parse(err2).RootElement
