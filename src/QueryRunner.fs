namespace PluckDataDownloader
module QueryRunner =

    open System
    open System.Text
    open FSharp.Control

    let CreateArgs t curDate itemsPerPage =
        { ContentType = t; Date = curDate; ItemsPerPage = itemsPerPage; Page=0; IsFinished=false }

    let InitialState() = StringBuilder()

    let RunQuery itemsPerPage execQueryStep handleQueryResult curDate t =
       async {
           let! s = execQueryStep (CreateArgs t curDate itemsPerPage) 
           return handleQueryResult curDate t (s.ToString())
       }

    let GenContent downloadContent processContent =
        AsyncSeq.unfoldAsync <| fun queryArgs -> 
            async { 
                if(queryArgs.IsFinished) then return None
                else
                    let! result = downloadContent queryArgs
                    return Option.bind (processContent queryArgs) result
            } 

    let FoldContent parsers =
        AsyncSeq.fold (fun s (p:Parsers.Parser)-> p.AppendParsedPageTo(s); s) (InitialState()) parsers