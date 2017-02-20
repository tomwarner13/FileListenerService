# FileListenerService
A lightweight Windows service which notifies a URL endpoint any time the files in a directory change

To use this service: edit the values in app.config. 

*RootDirectory* is the the root directory; the service will notify the URL endpoint when the files in this directory (or any subirectories) change. Note that most filesystem operations will cause multiple change events; this service will only send a notification when the file is unlocked, and will not send multiple notifications for the same file within 5 seconds of each other.

*UrlEndpoints* is a semicolon-separated list of your remote API endpoints which the service will notify. It will send a POST request with a JSON body containing two variables: *Path* is the relative path (from the root directory) of the modified file, and *Event* is the type of the change (currently only Created or Changed).

*DirsToIgnore* is a semicolon-delimited blacklist of any top-level directories which you would like the service to ignore. The endpoint will not be notified of any changes within these directories.

The console app can be used to test the service without needing to install it; it will log any changes to tracked files.
