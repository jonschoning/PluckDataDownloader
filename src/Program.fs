module PluckDataDownloader.Main

open Argu
open System

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
let mutable plucktypes = seq [| PluckContentType.Rating; PluckContentType.Review |]

type CLIArguments =
    | [<Mandatory>] AccessKey of string
    | [<Mandatory>] BaseUri of string
    | WriteHeaderFiles of bool
    | StartDate of string
    | EndDate of string
    | CsvDir of string
    | LogFile of string
    | PluckTypes of PluckContentType list
    | ItemsPerPage of int
    | Chunks of int
    | Bounds of int
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | AccessKey _ -> "example: XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX"
            | BaseUri _ -> "example: http://xyz.com/"
            | WriteHeaderFiles _ -> "default: false"
            | StartDate _ -> " default: " + (startDate).ToString("yyyy-MM-dd")
            | EndDate _ -> " default: " + (endDate).ToString("yyyy-MM-dd")
            | CsvDir _ -> "default: " + csvDir
            | LogFile _ -> " default: " + logFile
            | PluckTypes _ -> " default: rating, review"
            | ItemsPerPage _ -> " default:" + (itemsPerPage).ToString() + " max: 1000"
            | Chunks _ -> "example: 30"
            | Bounds _ -> "example: 30"

[<EntryPoint>]
let main argv =
    let parser =
        ArgumentParser.Create<CLIArguments>
            (programName = "PluckDataDownloader.exe")
    let results = parser.ParseCommandLine()
    accesskey <- results.TryGetResult AccessKey
    baseuri <- results.TryGetResult BaseUri
    writeHeaders <- results.GetResult(WriteHeaderFiles, defaultValue = false)
    let sd : string =
        results.GetResult(StartDate, defaultValue = startDate.ToString())
    let sdValid, startDate0 = DateTime.TryParse sd
    if sdValid then startDate <- startDate0
    else 
        printfn "\n%s\n" (parser.PrintUsage())
        failwith ""
    let ed : string =
        results.GetResult(EndDate, defaultValue = endDate.ToString())
    let edValid, endDate0 = DateTime.TryParse ed
    if edValid then endDate <- endDate0
    else 
        printfn "\n%s\n" (parser.PrintUsage())
        failwith ""
    csvDir <- results.GetResult(CsvDir, defaultValue = csvDir)
    logFile <- results.GetResult(LogFile, defaultValue = logFile)
    chunks <- results.TryGetResult Chunks
    bounds <- results.TryGetResult Bounds
    itemsPerPage <- results.GetResult(ItemsPerPage, defaultValue = itemsPerPage)
    plucktypes <- seq 
                  <| results.GetResult
                         (PluckTypes, defaultValue = Seq.toList plucktypes)
    if startDate.Date > endDate.Date then 
        failwith "startDate cannot be greater than endDate"
    if endDate.Date > DateTime.Now.Date then 
        failwith "endDate cannot be greater than current date"
    let logger = Util.Logger logFile
    try 
        CSVFileOps.WriteDirs csvDir plucktypes
        if writeHeaders then 
            CSVFileOps.WriteCSVHeaders csvDir plucktypes Parsers.GetCSVHeader
        let execQueryStep =
            QueryRunner.GenContent 
                (ApiClient.ContentDownload logger baseuri.Value accesskey.Value) 
                (QueryStateProcessor.ParseQueryState logger) 
            >> QueryRunner.FoldContent
        let handleQueryResult = CSVFileOps.WriteCsvFileResultForDate csvDir
        let mkJob =
            QueryRunner.RunQuery itemsPerPage execQueryStep handleQueryResult
        match (chunks, bounds) with
        | (Some c, _) -> Scheduler.ExecuteJobsChunked c logger plucktypes startDate endDate mkJob
        | (_, Some b) -> Scheduler.ExecuteJobsBounded b logger plucktypes startDate endDate mkJob
        | _ -> Scheduler.ExecuteJobsByDate logger plucktypes startDate endDate mkJob
    with e -> logger (e.ToString())
    0
