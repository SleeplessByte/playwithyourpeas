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
using SilverlightLoader;

namespace PlayWithYourPeas.Silverlight.Loader
{
    public partial class Progress : UserControl, SilverlightLoader.ISilverlightLoader
    {
        public Progress()
        {
            InitializeComponent();
        }

        #region ISilverlightLoader Members

        void ISilverlightLoader.initCallback(System.Collections.Generic.List<Uri> packageSourceList)
        {
        }

        void ISilverlightLoader.downloadStartCallback(Uri packageSource)
        {
            
        }

        void ISilverlightLoader.downloadProgressCallback(Uri packageSource, DownloadProgressEventArgs eventArgs)
        {
            float offset = ((float)eventArgs.ProgressPercentage * 4 / 100f) * 1000;
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, (int)offset);
            LoadingTextCtrl.Text = eventArgs.ProgressPercentage.ToString() + "%";
        }

        void ISilverlightLoader.downloadCompleteCallback(Uri packageSource, DownloadCompleteEventArgs eventArgs)
        {
            XapUtil.setCurrentXapFile(packageSource);
        }

        #endregion
    }
}
