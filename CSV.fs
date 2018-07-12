namespace PluckDataDownloader

open System
open System.IO
open System.Text

module CSVFileOps =
    let WriteDirs csvDir pluckTypes =
        for ct in pluckTypes do
            Directory.CreateDirectory(csvDir + "/" + ct.ToString()) |> ignore

    let WriteCsvFileResultForContentType csvDir ct filename r  =
        File.WriteAllText(csvDir + "/" + ct.ToString() + "/" + filename + ".csv", r)

    let WriteCsvFileResultForDate csvDir (date:DateTime) ct r  =
        WriteCsvFileResultForContentType csvDir ct (date.ToString("yyyy-MM-dd") + "_" + ct.ToString()) r

    let WriteCSVHeaders csvDir pluckTypes getheader =
        for ct in pluckTypes do
            WriteCsvFileResultForContentType csvDir ct "0headers" (getheader(ct) + Environment.NewLine) |> ignore

module CSVFormat =
    let Escape output =
        let mutable ret : string = output
        if isNull output then String.Empty
        else
            if output.Contains(",") || output.Contains("\"") || output.Contains("\n") || output.Contains("\r") then
                ret <- "\"" + output.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", " ") + "\""
            if ret.Length <= 32767 then ret else ret.Substring(0, 32767)

    let MakeRow values = values |> Seq.map Escape |> String.concat ","
