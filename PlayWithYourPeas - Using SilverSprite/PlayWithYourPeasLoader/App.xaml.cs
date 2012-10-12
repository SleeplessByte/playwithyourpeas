using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Reflection;
using SilverlightLoader;

namespace PlayWithYourPeas.Silverlight.Loader
{
    public partial class App : Application
    {

        public App()
        {
            this.Startup += this.Application_Startup;
            this.Exit += this.Application_Exit;
            this.UnhandledException += this.Application_UnhandledException;

            InitializeComponent();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // create a loader page
            Progress loader = new Progress();

            // set the visual root to loader page
            this.RootVisual = loader;

            // Debug helper -> make sure we have html params!
            if (e.InitParams.Count == 0)
            {
                MessageBox.Show("Error: You must tell your loader what files to load in your web page(init params) or manually in the code!", "Loader Error Message", MessageBoxButton.OK);
                //throw new Exception("Loader init params error");
                return;
            }
            // create package download manager and start the download process using html supplied InitParams
            // note the last param sets a 100KB max download speed for debuging and simulation mode!
            PackageDownloadManager pdm = new PackageDownloadManager(loader, e.InitParams, 0);

            // another option is to use a hand coded list ->
            //List<Uri> myDownloadList = new List<Uri>();
            //myDownloadList.Add(new Uri("ClientBin/Beegger.xap", UriKind.RelativeOrAbsolute));
            //ParamUtil.fixRelativeLinks(ref myDownloadList);
            //PackageDownloadManager pdm = new PackageDownloadManager(loader, myDownloadList, 50);
        }

        private void Application_Exit(object sender, EventArgs e)
        {

        }
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // If the app is running outside of the debugger then report the exception using
            // the browser's exception mechanism. On IE this will display it a yellow alert 
            // icon in the status bar and Firefox will display a script error.
            if (!System.Diagnostics.Debugger.IsAttached)
            {

                // NOTE: This will allow the application to continue running after an exception has been thrown
                // but not handled. 
                // For production applications this error handling should be replaced with something that will 
                // report the error to the website and stop the application.
                e.Handled = true;
                Deployment.Current.Dispatcher.BeginInvoke(delegate { ReportErrorToDOM(e); });
            }
        }
        private void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
                errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");

                System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight 2 Application " + errorMsg + "\");");
            }
            catch (Exception)
            {
            }
        }
    }
}
