module cvParser.api

    open System.Text.Json
    open FParsec
    open Types
    open Parser
    open Definitions
    open JsonParse

    let state = (eval (mkState data.RootElement))
    let query (q: string) =
        let err1 = System.Text.Encoding.ASCII.GetBytes(@"{""error"": ""The query did not return anything""}")
        let err2 = System.Text.Encoding.ASCII.GetBytes(@"{""error"": ""The query was malformed""}")
        let whoami = System.Text.Encoding.ASCII.GetBytes(@"{""whoami"": ""Albert Ross Johannessen""}")
        let commands = System.Text.Encoding.ASCII.GetBytes(
            @"{
            ""commands"": [
                    {""command"":""list"",""description"":""lists the keys int the current object, if it is an array it just displays the items"", ""usage"":""list""},
                    {""command"":""like"",""description"":""gets the item which matches the supplied string the most"", ""usage"":""like 'edu'""},
                    {""command"":""last"",""description"":""gets the last item in the enumeration"", ""usage"":""last""},
                    {""command"":""first"",""description"":""gets the first item in the enumeration"", ""usage"":""first""},
                    {""command"":""get"",""description"":""gets the value corresponding to the key supplied, if it is an array it finds the item that is equal to the argument supplied"", ""usage"":""get 'education'""},
                    {""command"":""index"",""description"":""gets the item at the index given"", ""usage"":""index 1""},
                    {""command"":""vals"",""description"":""gets all the values"", ""usage"":""like 'name' vals""}
            ],
            ""examples"": [""first"", ""like 'cont'"", ""get 'contact'"", ""like 'name' first""]
            }")
        try
            if q.Trim() = "help" then
                JsonDocument.Parse(commands).RootElement
            else if q.Trim() = "whoami" then
                JsonDocument.Parse(whoami).RootElement
            else
                let res = q |> run queryParse |> getSuccess |> queryEval |> state |> Option.map fst
                
                if Option.isSome res then
                    Option.get res |> Option.get
                else
                    JsonDocument.Parse(err1).RootElement
        with
           | e ->
               System.Console.WriteLine(e)
               JsonDocument.Parse(err2).RootElement
