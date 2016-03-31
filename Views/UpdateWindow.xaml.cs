// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateWindow.xaml.cs" company="LeagueSharp.Loader">
//   Copyright (c) LeagueSharp.Loader. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace LeagueSharp.Loader.Views
{
    #region

    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;

    using LeagueSharp.Loader.Class;
    using LeagueSharp.Loader.Data;

    using Polly;

    #endregion

    public enum UpdateAction
    {
        Core, 

        Loader
    }

    /// <summary>
    ///     Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : INotifyPropertyChanged
    {
        private string progressText;

        private string updateMessage;

        public UpdateWindow(UpdateAction action, string url)
        {
            this.InitializeComponent();
            this.DataContext = this;
            this.Action = action;
            this.UpdateUrl = url;
        }

        public UpdateWindow()
        {
            this.InitializeComponent();
            this.DataContext = this;

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                this.UpdateMessage = this.FindResource("Updating").ToString();
                this.ProgressText = this.FindResource("UpdateText").ToString();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string ProgressText
        {
            get
            {
                return this.progressText;
            }

            set
            {
                if (Equals(value, this.progressText))
                {
                    return;
                }

                this.progressText = value;
                this.OnPropertyChanged();
            }
        }

        public string UpdateMessage
        {
            get
            {
                return this.updateMessage;
            }

            set
            {
                if (Equals(value, this.updateMessage))
                {
                    return;
                }

                this.updateMessage = value;
                this.OnPropertyChanged();
            }
        }

        private UpdateAction Action { get; set; }

        private string UpdateUrl { get; set; }

        public async Task<bool> Update()
        {
            this.Focus();
            var result = false;
            this.UpdateProgressBar.Value = 0;
            this.UpdateProgressBar.Maximum = 100;

            switch (this.Action)
            {
                case UpdateAction.Loader:
                    result = await this.UpdateLoader();
                    break;
                case UpdateAction.Core:
                    result = await this.UpdateCore();
                    break;
            }

            Application.Current.Dispatcher.InvokeAsync(
                async () =>
                {
                    await Task.Delay(250);
                    this.Close();
                });

            return result;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task<bool> UpdateCore()
        {
            this.UpdateMessage = "Core " + this.FindResource("Updating");

            try
            {
                if (File.Exists(Updater.UpdateZip))
                {
                    File.Delete(Updater.UpdateZip);
                    Thread.Sleep(500);
                }

                var downloadResult = await Policy
                                               .Handle<Exception>()
                                               .WaitAndRetryAsync(
                                                   3, 
                                                   retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
                                                   (exception, timeSpan, context) => { Utility.Log(LogStatus.Error, $"ReTry {exception.Message}"); })
                                               .ExecuteAndCaptureAsync(
                                                   () =>
                                                   {
                                                       using (var client = new WebClient())
                                                       {
                                                           client.DownloadProgressChanged += this.WebClientOnDownloadProgressChanged;
                                                           return client.DownloadFileTaskAsync(this.UpdateUrl, Updater.UpdateZip);
                                                       }
                                                   }, 
                                                   true);

                if (downloadResult.Outcome == OutcomeType.Failure)
                {
                    return false;
                }

                using (var archive = ZipFile.OpenRead(Updater.UpdateZip))
                {
                    foreach (var entry in archive.Entries)
                    {
                        try
                        {
                            File.Delete(Path.Combine(Directories.CoreDirectory, entry.FullName));
                            entry.ExtractToFile(Path.Combine(Directories.CoreDirectory, entry.FullName), true);
                        }
                        catch
                        {
                            File.WriteAllText(Directories.CoreFilePath, "-"); // force an update
                            return false;
                        }
                    }
                }

                PathRandomizer.CopyFiles();
                Config.Instance.TosAccepted = false;

                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(Utility.GetMultiLanguageText("FailedToDownload") + e);
            }

            return false;
        }

        private async Task<bool> UpdateLoader()
        {
            this.UpdateMessage = "Loader " + this.FindResource("Updating");

            var downloadResult = await Policy
                                           .Handle<Exception>()
                                           .WaitAndRetryAsync(
                                               3, 
                                               retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
                                               (exception, timeSpan, context) => { Utility.Log(LogStatus.Error, $"ReTry {exception.Message}"); })
                                           .ExecuteAndCaptureAsync(
                                               () =>
                                               {
                                                   using (var client = new WebClient())
                                                   {
                                                       client.DownloadProgressChanged += this.WebClientOnDownloadProgressChanged;
                                                       return client.DownloadFileTaskAsync(this.UpdateUrl, Updater.SetupFile);
                                                   }
                                               }, 
                                               true);

            if (downloadResult.Outcome == OutcomeType.Failure)
            {
                MessageBox.Show(Utility.GetMultiLanguageText("LoaderUpdateFailed") + "\n" + downloadResult.FinalException);
                Environment.Exit(0);
            }

            Config.Instance.TosAccepted = false;
            Config.Save(false);

            new Process
            {
                StartInfo =
                {
                    FileName = Updater.SetupFile, 
                    Arguments = "/VERYSILENT /DIR=\"" + Directories.CurrentDirectory + "\""
                }
            }.Start();

            Environment.Exit(0);

            return true;
        }

        private void WebClientOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs args)
        {
            Application.Current.Dispatcher.InvokeAsync(
                () =>
                {
                    this.UpdateProgressBar.Value = args.ProgressPercentage;

                    this.ProgressText = string.Format(
                        this.FindResource("UpdateText").ToString(), 
                        args.BytesReceived / 1024, 
                        args.TotalBytesToReceive / 1024);
                });
        }
    }
}