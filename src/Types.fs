namespace PluckDataDownloader

open System

type PluckContentType =
    | ExternalResource = 0
    | Rating = 1
    | Review = 2
    | UserProfile = 3

type QueryArgs =
    { ContentType : PluckContentType
      Date : DateTime
      ItemsPerPage : int
      Page : int
      IsFinished : bool }