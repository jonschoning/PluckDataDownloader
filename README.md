# PluckDataDownloader

```
USAGE: PluckDataDownloader.exe [--help] --accesskey <string> --baseuri <string> [--writeheaderfiles <bool>]
                               [--startdate <string>] [--enddate <string>] [--csvdir <string>] [--logfile <string>]
                               [--plucktypes [<externalresource|rating|review|userprofile>...]] [--itemsperpage <int>]
                               [--chunks <int>] [--bounds <int>]

OPTIONS:

    --accesskey <string>  example: XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX
    --baseuri <string>    example: http://xyz.com/
    --writeheaderfiles <bool>
                          default: false
    --startdate <string>   default: 2018-07-12
    --enddate <string>     default: 2018-07-12
    --csvdir <string>     default: csv
    --logfile <string>     default: log.txt
    --plucktypes [<externalresource|rating|review|userprofile>...]
                           default: rating, review
    --itemsperpage <int>   default:500 max: 1000
    --chunks <int>        example: 30
    --bounds <int>        example: 30
    --help                display this list of options.

```
