# FileListenerService
A lightweight Windows service which notifies a URL endpoint any time the files in a directory change

To use this service: edit the values in app.config. 

*RootDirectory* is the the root directory; the service will notify the URL endpoint any time the files in this directory (or any subirectories) change.

*UrlEndpoint* is the address of your remote API which the service will notify. It will send a POST request with a JSON body containing two variables: *Path* is the relative path (from the root directory) of the modified file, and *Event* is the type of the change (currently only Created or Changed).

*DirsToIgnore* is a semicolon-delimited blacklist of any top-level directories which you would like the service to ignore. The endpoint will not be notified of any changes within these directories.

The console app can be used to test the service without needing to install it; note that it does not currently create any output, so you'll need to attach with a debugger or capture web requests to observe anything happening.
