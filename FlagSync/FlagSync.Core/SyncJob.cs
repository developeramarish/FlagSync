﻿using System.IO;

namespace FlagSync.Core
{
    class SyncJob : Job
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyncJob"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="preview">if set to true no files will be deleted, mofified or copied.</param>
        public SyncJob(JobSettings settings, bool preview)
            : base(settings, preview)
        {

        }

        /// <summary>
        /// Starts the job, opies new and modified files from directory A to directory B and then switches the direction
        /// </summary>
        public override void Start()
        {
            //Backup directoryA to directoryB and then otherwise
            this.BackupDirectories(new DirectoryInfo(this.Settings.DirectoryA), new DirectoryInfo(this.Settings.DirectoryB), this.Preview);
            this.BackupDirectories(new DirectoryInfo(this.Settings.DirectoryB), new DirectoryInfo(this.Settings.DirectoryA), this.Preview);

            this.OnFinished(System.EventArgs.Empty);
        }
    }
}
