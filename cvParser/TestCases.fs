namespace cvParser

open FuzzySharp
open Microsoft.FSharp.Core
open NUnit.Framework
open JsonParse
open Definitions
open Types
open cvParser.Definitions
open FParsec
open Parser

[<TestFixture>]
type Tests() =
    [<Test>]
    member this.Test() =
        printfn "%A" (data.RootElement.EnumerateObject())
        let search arr = Process.ExtractOne("nme", arr)
        printfn "%A" (data.RootElement.EnumerateObject()
                        |> Seq.map _.Name
                      |> Seq.toArray |> search)
        ()
    
    [<Test>]
    member this.TestDefs() =
        let query = Like("edu", First(Extract))
        // printfn "%A" ((queryEval query |> (eval (data.RootElement.EnumerateObject() |> toState))) |> Option.map fst |> Option.get)
        ()
    
    [<Test>]
    member this.TestParse() =
        let query = @"like ""edu"" | first | get ""institution"" | select"
        
        // printfn "%A" (query |> run queryParse |> getSuccess |> queryEval|> (eval (data.RootElement.EnumerateObject() |> toState)) |> Option.map fst |> Option.get)
        ()
    [<Test>]
    member this.TestSpeed() =
        let cvData = System.IO.File.ReadAllBytes("/Users/albert/RiderProjects/cvParser/cvParser/delays.json")
        let data = System.Text.Json.JsonDocument.Parse(cvData)
        let query = @"last like 'stat' like 'fli'"
        printfn "%A" (query |> run queryParse |> getSuccess |> queryEval|> (eval (mkState data.RootElement)) |> Option.map fst |> Option.get)
        ()
    
    [<Test>]
    member this.TestList() =
        let query = @"list"
        printfn "%A" (query |> run queryParse |> getSuccess |> queryEval|> (eval (mkState data.RootElement)) |> Option.map fst |> Option.get)
        ()
    