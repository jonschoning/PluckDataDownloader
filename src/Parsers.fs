namespace PluckDataDownloader

module Parsers =
    open System
    open System.IO
    open System.Text
    open System.Xml.XPath
    
    // add XPathNavigator extensions
    type XPathNavigator with
        
        member this.StrValue(xpath : string) =
            let v = this.SelectSingleNode(xpath).Value
            if isNull v then ""
            else v
        
        member this.HasMorePages() =
            not << isNull 
            <| this.SelectSingleNode
                   ("/ContentEnvelope/ContentHeader/NextPageUrl")
    
    type Parser =
        | RatingParser of xpNav : XPathNavigator
        | ReviewParser of xpNav : XPathNavigator
        
        member this.HasMorePages() =
            match this with
            | RatingParser xpNav -> xpNav.HasMorePages()
            | ReviewParser xpNav -> xpNav.HasMorePages()
        
        member this.AppendParsedPageTo(sb : StringBuilder) =
            match this with
            | RatingParser xpNav -> 
                for n in xpNav.Select("//Rating") do
                    let node = n :?> XPathNavigator
                    let authorId = node.StrValue("Key/RatedBy/Key")
                    let authorEmailAddress = node.StrValue("AuthorEmailAddress")
                    let titleId =
                        node.StrValue("RatedKey/Key").Replace("Title", "")
                    let rating = node.StrValue("Value")
                    let createdOn = node.StrValue("CreatedOn")
                    sb.AppendLine
                        (CSVFormat.MakeRow
                             ([ authorId; authorEmailAddress; titleId; rating; 
                                createdOn ])) |> ignore
            | ReviewParser xpNav -> 
                for n in xpNav.Select("//Review") do
                    let node = n :?> XPathNavigator
                    let authorId = node.StrValue("ReviewedBy/Key")
                    let authorEmailAddress = node.StrValue("AuthorEmailAddress")
                    let titleId =
                        node.StrValue("ReviewedKey/Key").Replace("Title", "")
                    let rating = node.StrValue("Value")
                    let review = node.StrValue("Text")
                    let createdOn = node.StrValue("CreatedOn")
                    sb.AppendLine
                        (CSVFormat.MakeRow
                             ([ authorId; authorEmailAddress; titleId; rating; 
                                review; createdOn ])) |> ignore
    
    let CreateNavigator str =
        (XPathDocument(new StringReader(str))).CreateNavigator()
    
    let CreateFromQueryState contentType data =
        match contentType with
        | PluckContentType.Rating -> 
            data
            |> CreateNavigator
            |> RatingParser
        | PluckContentType.Review -> 
            data
            |> CreateNavigator
            |> ReviewParser
        | t -> raise (NotSupportedException(t.ToString()))
    
    let GetCSVHeader =
        function 
        | PluckContentType.Rating -> 
            CSVFormat.MakeRow 
                [ "authorId"; "authorEmailAddress"; "titleId"; "rating"; "createdOn" ]
        | PluckContentType.Review -> 
            CSVFormat.MakeRow 
                [ "authorId"; "authorEmailAddress"; "titleId"; "rating"; "review"; "createdOn" ]
        | t -> raise (NotSupportedException(t.ToString()))
    
    let TrimDateStr(createdOn : string) =
        let index = createdOn.IndexOf("T")
        if (index > 0) then createdOn.Substring(0, index)
        else createdOn
