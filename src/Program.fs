module PluckDataDownloader.Main

open NDesk.Options
open System

let ShowHelp(p:OptionSet) =
    printfn "Usage: PluckDataDownloader (specify at least one option)"
    p.WriteOptionDescriptions Console.Out
    printfn ""
    printfn "Pluck Documentation: http://connect.pluck.com/docs/Pluck/contentDownload/PluckContentDownload51.pdf"
    printfn "Merging CSV Files: execute \"copy *.csv merged.csv\" in appropriate csv directory"


[<EntryPoint>]
let main argv =
    let mutable show_help = false
    let mutable startDate = DateTime.Today
    let mutable endDate = DateTime.Today
    let mutable csvDir = "csv"
    let mutable logFile = "log.txt"
    let mutable writeHeaders = false
    let mutable accesskey = None
    let mutable baseuri = None
    let mutable chunks : int option = None
    let mutable bounds : int option = None
    let mutable itemsPerPage = 500
    let mutable plucktypes = [|PluckContentType.Rating; PluckContentType.Review|]

    let p = OptionSet()
    p.Add("key|accesskey="       , " example: XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX"       , fun v -> accesskey <- Some v)      |> ignore
    p.Add("uri|baseuri="         , " example: http://xyz.com/"                            , fun v -> baseuri <- Some v)        |> ignore
    p.Add("whf|writeHeaderFiles" , " default: false"                                      , fun v -> writeHeaders <- isNull v) |> ignore
    p.Add("sd|startDate="        , " default: " + (startDate).ToString("yyyy-MM-dd")      , fun v -> startDate <- v )          |> ignore
    p.Add("ed|endDate="          , " default: "+ (endDate).ToString("yyyy-MM-dd")         , fun v -> endDate <- v )            |> ignore
    p.Add("cd|csvDir="           , " default: " + csvDir                                  , fun v -> csvDir <- v)              |> ignore
    p.Add("lf|logFile="          , " default: " + logFile                                 , fun v -> logFile <- v )            |> ignore
    p.Add("pt|pluckTypes="       , " default: rating, review"                             , fun (v:string) -> plucktypes <- v.Split(',')
                                                                                                           |> Array.map (fun x -> Enum.Parse(typeof<PluckContentType>, x , true) :?> PluckContentType))
                                                                                                                               |> ignore
    p.Add("ipp|itemsPerPage="    , " default:" + (itemsPerPage).ToString() + " max: 1000" , fun v -> itemsPerPage <- v)        |> ignore
    p.Add("c|chunks="            , " example: 30"                                         , fun v -> chunks <- Some v)         |> ignore
    p.Add("b|bounds="            , " example: 30"                                         , fun v -> bounds <- Some v)         |> ignore
    p.Add("h|help"               , "show this message and exit"                           , fun v -> show_help <- isNull v)    |> ignore

    let mutable extra = []
    try
        extra <- p.Parse(argv) |> List.ofSeq
        if Option.isNone accesskey then failwith "accesskey must be supplied"
        if Option.isNone baseuri then failwith "baseuri must be supplied"
        if startDate.Date > endDate.Date then failwith "startDate cannot be greater than endDate"
        if endDate.Date > DateTime.Now.Date then failwith "endDate cannot be greater than current date"
    with
       | e -> printfn "\nPluckDataDownloader: "
              printfn "\n%s\n" e.Message
              printfn "Try `PluckDataDownloader --help' for more information."
              show_help <- true

    if show_help || extra.Length > 0 || argv.Length = 0
    then
        ShowHelp(p)
    else
        let logger =
               Util.Logger logFile

        try
            CSVFileOps.WriteDirs csvDir plucktypes

            if writeHeaders then
                CSVFileOps.WriteCSVHeaders csvDir plucktypes Parsers.GetCSVHeader

            let execQueryStep =
                  QueryRunner.GenContent 
                    (ApiClient.ContentDownload logger baseuri.Value accesskey.Value)
                    (QueryStateProcessor.ParseQueryState logger)
                  >> QueryRunner.FoldContent

            let handleQueryResult =
                   CSVFileOps.WriteCsvFileResultForDate csvDir

            let mkJob =
                   QueryRunner.RunQuery itemsPerPage execQueryStep handleQueryResult

            match (chunks, bounds) with
            | (Some c, _)  -> Scheduler.ExecuteJobsChunked c logger plucktypes startDate endDate mkJob
            | (_ , Some b) -> Scheduler.ExecuteJobsBounded b logger plucktypes startDate endDate mkJob
            | _      -> Scheduler.ExecuteJobsByDate    logger plucktypes startDate endDate mkJob

        with
           | e -> logger (e.ToString())
    0
