module cvParser.JsonParse

    open System.Text
    
    // Could be done in a stream but whatever
    let cvData = System.IO.File.ReadAllBytes("/src/cvParser/CV.json")
    let data = Json.JsonDocument.Parse(cvData)
