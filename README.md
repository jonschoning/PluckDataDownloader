# PluckDataDownloader [![Build Status](https://travis-ci.org/jonschoning/PluckDataDownloader.svg?branch=master)](https://travis-ci.org/jonschoning/PluckDataDownloader)

```
PluckDataDownloader: 

accesskey must be supplied
baseuri must be supplied

Try `PluckDataDownloader --help' for more information.
Usage: PluckDataDownloader (specify at least one option)
      --key, --accesskey=VALUE
                              example: XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX
      --uri, --baseuri=VALUE  example: http://xyz.com/
      --whf, --writeHeaderFiles
                              default: false
      --sd, --startDate=VALUE
                              default: 2015-07-01
      --ed, --endDate=VALUE   default: 2015-07-01
      --cd, --csvDir=VALUE    default: csv
      --lf, --logFile=VALUE   default: log.txt
      --pt, --pluckTypes=VALUE
                              default: rating,review
      --ipp, --itemsPerPage=VALUE
                              default:500 max: 1000
  -c, --chunks=VALUE          example: 30
  -h, --help                 show this message and exit

Pluck Documentation: http://connect.pluck.com/docs/Pluck/contentDownload/PluckContentDownload51.pdf
Merging CSV Files: execute "copy *.csv merged.csv" in appropriate csv directory
```
