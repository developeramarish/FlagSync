﻿using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FlagLib.Patterns;

namespace FlagSync.View
{
    public class LogMessage : ViewModelBase<LogMessage>
    {
        private int progress;
        private long? fileSize;

        /// <summary>
        /// Gets or sets the type (file or directory).
        /// </summary>
        /// <value>The type.</value>
        public string Type { get; private set; }

        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        /// <value>The action.</value>
        public string Action { get; private set; }

        /// <summary>
        /// Gets or sets the path of file A.
        /// </summary>
        /// <value>The path of file A.</value>
        public string SourcePath { get; private set; }

        /// <summary>
        /// Gets or sets the path of file B.
        /// </summary>
        /// <value>The path of file B.</value>
        public string TargetPath { get; private set; }

        /// <summary>
        /// Gets or sets the current progress of the file (0 - 100).
        /// </summary>
        /// <value>The current progress of the file.</value>
        public int Progress
        {
            get { return this.progress; }
            set
            {
                if (value < 0 || value > 100)
                    throw new ArgumentOutOfRangeException("value", "Value must be between 0 and 100");

                if (this.Progress != value)
                {
                    this.progress = value;
                    this.OnPropertyChanged(vm => vm.Progress);
                    this.OnPropertyChanged(vm => vm.IsInProgress);
                    this.OnPropertyChanged(vm => vm.Image);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current file is in progress.
        /// </summary>
        /// <value>true if the current file is in progress; otherwise, false.</value>
        public bool IsInProgress
        {
            get { return this.Progress != 100; }
        }

        /// <summary>
        /// Gets the image for the log message.
        /// </summary>
        public ImageSource Image
        {
            get
            {
                if (IsInProgress && !IsErrorMessage)
                    return null;

                string successImagePath = "pack://application:,,,/FlagSync;component/Images/Success.png";
                string errorImagePath = "pack://application:,,,/FlagSync;component/Images/Error.png";

                BitmapImage logo = new BitmapImage();

                logo.BeginInit();
                logo.UriSource = new Uri(this.IsErrorMessage ? errorImagePath : successImagePath);
                logo.EndInit();
                logo.Freeze();

                return logo;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the log message is a error message.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the log message is a error message; otherwise, <c>false</c>.
        /// </value>
        public bool IsErrorMessage { get; private set; }

        /// <summary>
        /// Gets the size of the file.
        /// </summary>
        /// <value>
        /// The size of the file.
        /// </value>
        public string FileSize
        {
            get
            {
                if (this.fileSize.HasValue)
                {
                    string[] suffix = { "B", "KB", "MB", "GB", "TB" };
                    int i;
                    double dblSByte = this.fileSize.Value;

                    for (i = 0; (int)(this.fileSize / 1024) > 0; i++, this.fileSize /= 1024)
                    {
                        dblSByte = this.fileSize.Value / 1024.0;
                    }

                    //Bytes shouldn't have decimal places
                    string format = i == 0 ? "{0} {1}" : "{0:0.00} {1}";

                    return String.Format(format, dblSByte, suffix[i]);
                }

                else
                {
                    return String.Empty;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessage"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="action">The action.</param>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="targetPath">The target path.</param>
        /// <param name="isErrorMessage">if set to <c>true</c>, the message is a error message.</param>
        /// <param name="fileSize">Size of the file.</param>
        public LogMessage(string type, string action, string sourcePath, string targetPath, bool isErrorMessage, long? fileSize)
        {
            this.Type = type;
            this.Action = action;
            this.SourcePath = sourcePath;
            this.TargetPath = targetPath;
            this.IsErrorMessage = isErrorMessage;
            this.fileSize = fileSize;
        }
    }
}