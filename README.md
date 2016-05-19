# FileListenerService
A lightweight Windows service which notifies a URL endpoint any time the files in a directory change

To use this service: edit the values in app.config. 

*RootDirectory* is the the root directory; the service will notify the URL endpoint any time the files in this directory (or any subirectories) change.

*UrlEndpoint* is the address of your remote API which the service will notify. It will send a POST request with a JSON body containing two variables: *Path* is the relative path (from the root directory) of the modified file, and *Event* is the type of the change (currently only Created or Changed).

*DirsToIgnore* is a semicolon-delimited blacklist of any top-level directories which you would like the service to ignore. The endpoint will not be notified of any changes within these directories.

The console app can be used to test the service without needing to install it; it will log any changes to tracked files.

*Note:* many file operations may fire multiple events; for example, saving the same file from different text editors will fire between 1 and 3 events, and pasting a file into a folder produces a Created and a Changed event in immediate sequence on my machine. Make sure your API can handle that, or suppress multiple identical events in a short timespan.
