namespace PluckDataDownloader

module Scheduler =
    open System
    open System.Collections.Concurrent
    open System.Threading
    open Util
    open FSharp.Control
    
    let ExecuteJobsByDate (_log : string -> unit) pluckTypes 
        (startDate : DateTime) (endDate : DateTime) mkJob =
        if (startDate > endDate.Date) then 
            raise (Exception("curDate > endDate.Date"))
        DateRange startDate endDate
        |> Seq.iter (fun curDate -> 
               _log 
                   (String.Format
                        ("*** Processing Date: {0:yyyy-MM-dd}", curDate))
               pluckTypes
               |> Seq.map (mkJob curDate)
               |> Async.Parallel
               |> Async.RunSynchronously
               |> ignore)
    
    let ExecuteJobsChunked chunks (_log : string -> unit) pluckTypes 
        (startDate : DateTime) (endDate : DateTime) mkJob =
        if (startDate > endDate.Date) then 
            raise (Exception("curDate > endDate.Date"))
        DateRange startDate endDate
        |> Seq.map (fun curDate -> 
               pluckTypes
               |> Seq.map (mkJob curDate)
               |> Async.Parallel)
        |> SplitChunks chunks
        |> Seq.iter (Async.Parallel
                     >> Async.RunSynchronously
                     >> ignore)
    
    let ExecuteJobsBounded (size : int) (_log : string -> unit) pluckTypes 
        (startDate : DateTime) (endDate : DateTime) mkJob =
        if (startDate > endDate.Date) then 
            raise (Exception("curDate > endDate.Date"))
        use bc = new BlockingCollection<Async<Unit>>(size)
        // put async work on BlockingCollection
        Async.Start(async { 
                        DateRange startDate endDate
                        |> Seq.map (fun curDate -> 
                               pluckTypes
                               |> Seq.map (mkJob curDate)
                               |> Async.Parallel
                               |> Async.Ignore)
                        |> Seq.iter (bc.Add)
                        bc.CompleteAdding()
                    })
        // each consumer runs it's job synchronously
        let mkConsumer (consumerId : int) =
            async { 
                for job in bc.GetConsumingEnumerable() do
                    try 
                        do! job
                    with ex -> _log ("consumerId: " + ex.Message.ToString())
            }
        { // create `size` number of consumers in parallel
          1..size }
        |> Seq.map (mkConsumer)
        |> Async.Parallel
        |> Async.RunSynchronously
        |> ignore
    
    let ExecuteJobsSlim (size : int) (_log : string -> unit) pluckTypes 
        (startDate : DateTime) (endDate : DateTime) mkJob =
        if (startDate > endDate.Date) then 
            raise (Exception("curDate > endDate.Date"))
        use ss = new SemaphoreSlim(size)
        DateRange startDate endDate
        |> Seq.map ((fun curDate -> 
                    pluckTypes
                    |> Seq.map (mkJob curDate)
                    |> Async.Parallel
                    |> Async.Ignore)
                    >> (fun job -> 
                    async { 
                        do! Async.AwaitTask(ss.WaitAsync())
                        try 
                            do! job
                        finally
                            ss.Release() |> ignore
                    }))
        |> Async.Parallel
        |> Async.Ignore
        |> Async.RunSynchronously
