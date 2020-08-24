# Tezal.NET
An open-source parallel file downloader

## Current state
*One may consume the project for their console applications and download files in several chunks simultaneously.

## Future
*Error-handling
*Tests
*Pausing and saving download progress
*Building desktop and mobile clients

## How to use
The project has not yet been distributed as a Nuget package. However, one may download the project and try it out. Basic usage scenario so far is as follows:
```c#
var fileUrl = "https://linkto.file";
var destinationPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
var downloader = new ParallelDownloader(fileUrl, destinationPath);
downloader.StartDownloadParallel();
```
For efficiency, the number of parallel requests is equal to the number of logical cores in the processor. 

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License
[MIT](https://choosealicense.com/licenses/mit/)
