namespace PluckDataDownloader
module QueryStateProcessor =

    open System
    open System.Text
    open System.Xml

    let SetNextPage state = {state with Page = state.Page + 1}
    let SetFinished state = {state with IsFinished = true}

    let ParseQueryState _log queryArgs downloadStr =
        try
            let parser = Parsers.CreateFromQueryState queryArgs.ContentType downloadStr
            if (parser.HasMorePages()) 
                then Some (parser, SetNextPage queryArgs) 
                else Some (parser, SetFinished queryArgs)
        with
          | :? XmlException as ex ->
            _log (ex.Message.ToString())
            None
