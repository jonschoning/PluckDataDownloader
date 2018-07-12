namespace PluckDataDownloader
module Util =
    open System
    open System.IO
    open System.Text

    let SplitChunks n = 
        let one, append, empty = Seq.singleton, Seq.append, Seq.empty
        
        let go = 
            (0, empty, empty)
            |> Seq.fold (fun (m, cur, acc) x -> 
                   if m = n then (1, one x, append acc (one cur))
                   else (m + 1, append cur (one x), acc))
            >> fun (_, cur, acc) -> append acc (one cur)
        go

    let DateRange (startDate : DateTime) (endDate : DateTime) = 
        Seq.unfold (fun curDate -> 
            if curDate <= endDate.Date then Some(curDate, curDate.AddDays(1.0))
            else None) startDate

    let mutable loggerlock = Object()

    let Logger logfile s = 
        let str = String.Format("{0:s}: {1}", DateTime.Now, s)
        printfn "%s" str
        lock loggerlock (fun () -> 
            use w = System.IO.File.AppendText(logfile)
            w.WriteLine(str))
