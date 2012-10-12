using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Resources;
using System.Windows.Markup;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Threading;

namespace SilverlightLoader
{
    /// <summary>
    /// Interface for loaders - every loader should inherit and implemant this interface
    /// </summary>
    public interface ISilverlightLoader
    {
        // init with list of packages to download
        void initCallback(List<Uri> packageSourceList);
        // called when package download starts
        void downloadStartCallback(Uri packageSource);
        // called when package download progresses
        void downloadProgressCallback(Uri packageSource, DownloadProgressEventArgs eventArgs);
        // called when package download is complete
        void downloadCompleteCallback(Uri packageSource, DownloadCompleteEventArgs eventArgs);
    }

    /// <summary>
    /// event classes
    /// </summary>
    public class DownloadProgressEventArgs : EventArgs
    {
        public DownloadProgressEventArgs()
        {

        }

        public DownloadProgressEventArgs(DownloadProgressChangedEventArgs e)
        {
            BytesReceived = e.BytesReceived;
            TotalBytesToReceive = e.TotalBytesToReceive;
        }
        // Summary:
        //     Gets the number of bytes received.
        //
        // Returns:
        //     An System.Int64 value that indicates the number of bytes received.
        public long BytesReceived { get; set; }
        //
        // Summary:
        //     Gets the total number of bytes in a System.Net.WebClient data download operation.
        //
        // Returns:
        //     An System.Int64 value that indicates the number of bytes that will be received.
        public long TotalBytesToReceive { get; set; }
        // Summary:
        //     Gets the percentage of an asynchronous operation that has been completed.
        //
        // Returns:
        //     A percentage value that indicates the asynchronous operation progress.
        public int ProgressPercentage
        {
            get
            {
                return (int)Math.Round(((double)BytesReceived / TotalBytesToReceive) * 100);
            }
        }
    }

    public class DownloadCompleteEventArgs : EventArgs
    {
        public DownloadCompleteEventArgs()
        {

        }

        public DownloadCompleteEventArgs(OpenReadCompletedEventArgs e)
        {
            Cancelled = e.Cancelled;
            Error = e.Error;
            if (e.Error == null)
            {
                Result = e.Result;
            }
            else
                Result = null;
        }
        // Summary:
        //     Gets a readable stream that contains the results of the System.Net.WebClient.OpenReadAsync(System.Uri,System.Object)
        //     method.
        //
        // Returns:
        //     A System.IO.Stream that contains the results of the System.Net.WebClient.OpenReadAsync(System.Uri,System.Object)
        //     method.
        //
        // Exceptions:
        //   System.InvalidOperationException:
        //     The asynchronous request was cancelled.
        public Stream Result { get; set; }
        // Summary:
        //     Gets a value that indicates whether an asynchronous operation has been canceled.
        //
        // Returns:
        //     true if the asynchronous operation has been canceled; otherwise, false. The
        //     default is false.
        public bool Cancelled { get; set; }
        //
        // Summary:
        //     Gets a value that indicates which error occurred during an asynchronous operation.
        //
        // Returns:
        //     An System.Exception instance, if an error occurred during an asynchronous
        //     operation; otherwise null.
        public Exception Error { get; set; }

    }

    /// <summary>
    /// Package downloader class - takes care of the actual download of the packages
    /// </summary>
    public class PackageDownloader
    {
        /// <summary>
        /// download a package
        /// </summary>
        /// <param name="packageSource"></param>
        /// <param name="progressCallback"></param>
        public void download(Uri packageSource, ISilverlightLoader progressCallback)
        {
            abort();
            m_progressCallbackInterface = progressCallback;
            m_packageSource = packageSource;
            m_progressCallbackInterface.downloadStartCallback(packageSource);
            m_webClient = new WebClient();
            m_webClient.DownloadProgressChanged += onDownloadProgressChanged;
            m_webClient.OpenReadCompleted += onOpenReadCompleted;
            m_webClient.OpenReadAsync(packageSource);
        }

        public void abort()
        {
            if (m_webClient != null)
            {
                m_webClient.DownloadProgressChanged -= onDownloadProgressChanged;
                m_webClient.CancelAsync();
                m_webClient = null;
            }
        }

        protected void onDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            m_progressCallbackInterface.downloadProgressCallback(m_packageSource, new DownloadProgressEventArgs(e));
        }

        protected void onOpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            m_progressCallbackInterface.downloadCompleteCallback(m_packageSource, new DownloadCompleteEventArgs(e));
        }

        // data
        WebClient m_webClient;
        ISilverlightLoader m_progressCallbackInterface;
        Uri m_packageSource;
    }

    /// <summary>
    /// Package downloader simulater class - takes care of the simulated download of packages
    /// </summary>
    public class PackageDownloaderSimulator
    {

        public void download(Uri packageSource, ISilverlightLoader progressCallback, float maxTransferRateKB)
        {
            abort();
            m_progressCallbackInterface = progressCallback;
            m_packageSource = packageSource;
            m_progressCallbackInterface.downloadStartCallback(packageSource);
            m_webClient = new WebClient();
            m_webClient.DownloadProgressChanged += onDownloadProgressChanged;
            m_webClient.OpenReadCompleted += onOpenReadCompleted;
            m_webClient.OpenReadAsync(packageSource);
            // sim misc
            initTimer();
            m_maxTransferRateKBS = maxTransferRateKB;
        }

        void initTimer()
        {
            m_simulationStartTime = DateTime.Now;
            m_timer = new DispatcherTimer();
            m_timer.Interval = TimeSpan.FromSeconds(1);
            m_timer.Tick += new EventHandler(onTimerTick);
            m_timer.Start();
        }

        void onTimerTick(object sender, EventArgs e)
        {
            // make sure we got a first call initializing the data -> total dl size etc.
            if (m_dlProgressEventArgs == null)
                return;
            //float currentTransferRate = m_dlProgressEvent.BytesReceived / DateTime.Now.Subtract(m_simulationStartTime).Seconds;
            m_dlProgressEventArgs.BytesReceived += (long)(m_maxTransferRateKBS * 1024);
            if (m_dlProgressEventArgs.BytesReceived >= m_dlProgressEventArgs.TotalBytesToReceive)
            {
                // set progress info max size
                m_dlProgressEventArgs.BytesReceived = m_dlProgressEventArgs.TotalBytesToReceive;
                // make sure we first got a download complete call initializing the data -> result
                if (m_dlCompleteEventArgs == null)
                    return;
                // stop timer so we don't get any more calls after this point
                m_timer.Stop();
                // call both progress and complete to simulate current real behaviour...
                m_progressCallbackInterface.downloadProgressCallback(m_packageSource, m_dlProgressEventArgs);
                m_progressCallbackInterface.downloadCompleteCallback(m_packageSource, m_dlCompleteEventArgs);
            }
            else
                m_progressCallbackInterface.downloadProgressCallback(m_packageSource, m_dlProgressEventArgs);
        }

        public void abort()
        {
            if (m_timer != null)
            {
                m_timer.Tick -= onTimerTick;
                m_timer.Stop();
                m_timer = null;
            }

            if (m_webClient != null)
            {
                m_webClient.DownloadProgressChanged -= onDownloadProgressChanged;
                m_webClient.CancelAsync();
                m_webClient = null;
            }
        }

        protected void onDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (m_dlProgressEventArgs == null)
            {
                m_dlProgressEventArgs = new DownloadProgressEventArgs(e);
                m_dlProgressEventArgs.BytesReceived = 0;
            }
        }

        protected void onOpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            m_dlCompleteEventArgs = new DownloadCompleteEventArgs(e);
        }

        // data
        /////////////////////
        // sim interval misc
        /////////////////////
        float m_maxTransferRateKBS;
        DownloadProgressEventArgs m_dlProgressEventArgs = null;
        DownloadCompleteEventArgs m_dlCompleteEventArgs = null;
        DateTime m_simulationStartTime;
        ////////////////////
        WebClient m_webClient;
        DispatcherTimer m_timer;
        ISilverlightLoader m_progressCallbackInterface;
        Uri m_packageSource;
    }

    /// <summary>
    /// Manages the package list download process - takes a package list and loader interface as parameters and manage the download process
    /// </summary>
    public class PackageDownloadManager
    {
        public PackageDownloadManager(ISilverlightLoader loader, IDictionary<string, string> initParams, float maxTransferRateKB)
        {
            // parse the package source list from init params
            List<Uri> packageFileList = ParamUtil.initParamsToUriList(initParams);
            ParamUtil.fixRelativeLinks(ref packageFileList);
            init(loader, packageFileList, maxTransferRateKB);
        }

        public PackageDownloadManager(ISilverlightLoader loader, List<Uri> packageSourceList, float maxTransferRateKB)
        {
            init(loader, packageSourceList, maxTransferRateKB);
        }

        private void init(ISilverlightLoader loader, List<Uri> packageSourceList, float maxTransferRateKB)
        {
            // save transfer rate if any
            m_maxTransferRateKB = maxTransferRateKB;
            // save loader callback interface
            m_loader = loader;
            // call init callback with package list
            m_loader.initCallback(packageSourceList);
            // iterate through list and start downloading the files
            foreach (Uri packageSourceFile in packageSourceList)
            {
                if (!isStreamingUri(packageSourceFile))
                {
                    if (maxTransferRateKB > 0)
                        simulateFileDownload(packageSourceFile);
                    else
                        downloadFile(packageSourceFile);
                }
                else
                {
                    SilverlightStreamingUtil.GetMediaStreamUriCallback gmsCallback;
                    // check if sim mode is on
                    if (maxTransferRateKB > 0)
                        gmsCallback = new SilverlightStreamingUtil.GetMediaStreamUriCallback(simulateFileDownload);
                    else
                        gmsCallback = new SilverlightStreamingUtil.GetMediaStreamUriCallback(downloadFile);
                    // start the streaming media url process
                    SilverlightStreamingUtil streamUtil = new SilverlightStreamingUtil(gmsCallback);
                    streamUtil.getMediaStreamUri(packageSourceFile.ToString());
                }
            }
        }

        // the uri scheme for the streaming protocol -> note this should be chnaged in init params if changed here...
        const string UriSchemeStreaming = "streaming:";

        // check if it is a streaming uri
        bool isStreamingUri(Uri packageSourceFile) { return packageSourceFile.ToString().StartsWith(UriSchemeStreaming); }

        // download file async
        public void downloadFile(Uri source)
        {
            PackageDownloader downloader = new PackageDownloader();
            downloader.download(source, m_loader);
        }

        // simulate file download for easier testing of the loader
        private void simulateFileDownload(Uri source)
        {
            PackageDownloaderSimulator downloader = new PackageDownloaderSimulator();
            TimeSpan totalDlTime = new TimeSpan(0, 0, 0, 10);
            downloader.download(source, m_loader, m_maxTransferRateKB);
        }

        // data
        ISilverlightLoader m_loader;
        float m_maxTransferRateKB;
    }

    /// <summary>
    /// Utility class for manipulating XAP files
    /// </summary>
    public class XapUtil
    {
        // set the source of the current silverlight plugin 
        // note that changing this will effectively change the XAP file that is currently in use!
        // the new XAP will be initialized and displayed in the browser replacing the old XAP source
        public static void setCurrentXapFile(Uri packageSource)
        {
            string path = packageSource.ToString();
            System.Windows.Browser.HtmlPage.Plugin.SetProperty("source", path);
        }
        // get the source of the current silverlight plugin 
        public static Uri getCurrentXapFile()
        {
            string xapSource = (string)System.Windows.Browser.HtmlPage.Plugin.GetProperty("source");
            return new Uri(xapSource);
        }
    }
    /// <summary>
    /// Utility class for parsing parameter strings
    /// </summary>
    public class ParamUtil
    {
        // the key used in init params to tell us what the package download sources are
        static string LoaderSourceKeyName = "LoaderSourceList";
        // delimeters for package source list -> file1;file2;...
        static string[] FileListDelimeters = { ";" };

        public static List<Uri> delimitedStringListToUriList(string delimitedStringList)
        {
            string[] tempStrList = delimitedStringList.Split(FileListDelimeters, StringSplitOptions.RemoveEmptyEntries);
            List<Uri> xapUriFileList = new List<Uri>(tempStrList.Length);
            foreach (string xapFile in tempStrList)
                xapUriFileList.Add(new Uri(xapFile, UriKind.RelativeOrAbsolute));
            return xapUriFileList;
        }

        public static List<Uri> initParamsToUriList(IDictionary<string, string> initParams)
        {
            string delimitedStringList = initParams[LoaderSourceKeyName];
            return delimitedStringListToUriList(delimitedStringList);
        }

        public static void fixRelativeLinks(ref List<Uri> xapUriFileList)
        {
            string basePath = System.Windows.Browser.HtmlPage.Document.DocumentUri.ToString();
            basePath = basePath.Remove(basePath.LastIndexOf('/'));
            // if its not terminated by a '/' add one, otherwise URI constructor does not build xap path correctly
            if (basePath[basePath.Length - 1] != ('/'))
                basePath += '/';
            // modify all relative links...
            // !!! note that stream: url's are absolute url's
            for (int i = 0; i < xapUriFileList.Count; i++)
            {
                if (!xapUriFileList[i].IsAbsoluteUri)
                {
                    Uri xapFile = new Uri(new Uri(basePath), xapUriFileList[i]);
                    xapUriFileList[i] = xapFile;
                }
            }
        }
    }

    /// <summary>
    /// Silverlight streaming service utility class - 
    /// helps manage downloading files hosted on the Microsoft silverlight streaming service (http://dev.live.com/silverlight/)
    /// </summary>
    public class SilverlightStreamingUtil
    {
        // declare a delegate type for getMediaStream result
        public delegate void GetMediaStreamUriCallback(Uri mediaUrl);

        // define the Microsoft Silverlight Streaming service base address
        const string SLSMediaServiceRoot = "http://silverlight.services.live.com/";

        public SilverlightStreamingUtil(GetMediaStreamUriCallback getMediaStreamUriCallback)
        {
            m_getMediaStreamUriCallback = getMediaStreamUriCallback;
        }

        // get a media stream uri from a stream id
        // this is done by sending a specialy crafted request to the streaming service
        // the streamId contains accountId, appName and a fileName if the request if for a non xap resource.
        // example: /57870/TestVideo/video.wmv
        // example: /57870/LoaderTestApp/
        public void getMediaStreamUri(string streamId)
        {
            Match infoMatch = Regex.Match(streamId, "/(.*)/(.*)/(.*)");
            string accountId = infoMatch.Groups[1].Value;
            string appName = infoMatch.Groups[2].Value;
            string fileName = infoMatch.Groups[3].Value;
            getMediaStreamUri(accountId, appName, fileName);
        }

        // get a media stream uri from accountId, appName and a fileName if the request if for a non xap resource.
        public void getMediaStreamUri(string accountId, string appName, string fileName)
        {
            TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            double timestamp = t.TotalMilliseconds;
            string milliseconds = timestamp.ToString().Split(new char[] { '.' })[0];

            Uri targetUri;
            // check if we are requesting a media file or a xap application
            // because for some reason the format is different...
            if (fileName.Length != 0)
                targetUri = new Uri(SLSMediaServiceRoot + string.Format("invoke/local/starth.js?id=bl2&u={0}&p0=/{1}/{2}/{3}", milliseconds, accountId, appName, fileName));
            else
                targetUri = new Uri(SLSMediaServiceRoot + string.Format("invoke/{1}/{2}/starth.js?id=bl2&u={0}", milliseconds, accountId, appName));

            WebClient webClient = new WebClient();
            webClient.DownloadStringAsync(targetUri);
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webClient_DownloadStringCompleted);
        }

        // json data struct returned by streaming svc
        // example: {"version": "2.0", "name": "LoadTest", "width": "800", "height": "600", "source": "LoadTest.xap"}
        public class SilverlightStreamData
        {
            public string version { get; set; }
            public string name { get; set; }
            public string width { get; set; }
            public string height { get; set; }
            public string source { get; set; }
        }

        // download string completed callback
        void webClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                string scriptResponse = e.Result;
                // get first part with base url or media url
                Match mediaMatch = Regex.Match(scriptResponse, "http:.+.\"");
                string mediaUrl = mediaMatch.Value.Remove(mediaMatch.Value.Length - 1);

                // get the json part if any -> applies to xap packages
                string jsonPart = Regex.Match(scriptResponse, "{.+}").Value;
                // if we do have a xap package the get the source info so we can build the url
                if (jsonPart.Length > 0)
                {
                    System.Runtime.Serialization.Json.DataContractJsonSerializer ser = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(SilverlightStreamData));
                    MemoryStream memStream = new MemoryStream(System.Text.Encoding.Unicode.GetBytes(jsonPart));
                    SilverlightStreamData slsData = (SilverlightStreamData)ser.ReadObject(memStream);
                    mediaUrl += "/" + slsData.source;
                }
                // call callback with result
                m_getMediaStreamUriCallback(new Uri(mediaUrl));
            }
        }

        // data
        GetMediaStreamUriCallback m_getMediaStreamUriCallback;
    }
}
