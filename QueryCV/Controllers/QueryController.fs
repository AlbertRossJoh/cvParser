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

    [<HttpPost>]
    member _.Post([<FromBody>] query: query) =
        logger.LogDebug(query.ToString())
        api.query query.query
