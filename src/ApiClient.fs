namespace PluckDataDownloader

module ApiClient =
    open System
    open FSharp.Data
    
    let ContentDownload _log (baseuri : string) (accessKey : string) args =
        async { 
            let uri =
                String.Format
                    ("{0}/ver1.0/ContentDownload/{1}/{2}/{3}/{4}?accessKey={5}", 
                     baseuri, args.ContentType.ToString(), 
                     args.Date.ToString("yyyy-MM-dd"), args.ItemsPerPage, 
                     args.Page, accessKey)
            _log (String.Format("requesting: {0}", uri))
            let! response = Http.AsyncRequest uri
            if (response.StatusCode = 200) then 
                return match response.Body with
                       | Text t -> Some t
                       | _ -> failwith "non-text response"
            else 
                let str = "*** response.IsSuccessStatusCode == False ***"
                _log (str)
                return None
        }
