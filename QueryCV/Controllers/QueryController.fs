namespace QueryCV.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open cvParser

type query = {
    query: string
}

[<ApiController>]
[<Route("[controller]")>]
type QueryController (logger : ILogger<QueryController>) =
    inherit ControllerBase()

    [<HttpGet>]
    member _.Get([<FromBody>] query: query) =
        // printfn "%A" query.query
        api.query query.query
