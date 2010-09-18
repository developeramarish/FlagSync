﻿using System;
using System.Collections.Generic;

namespace FlagSync.Core
{
    public class JobWorker
    {
        public enum SyncMode
        {
            Backup,
            Sync
        }

        public event EventHandler FilesCounted;
        public event EventHandler Finished;
        public event EventHandler<FileProceededEventArgs> FileProceeded;
        public event EventHandler<FileCopyEventArgs> FoundNewerFile;
        public event EventHandler<FileCopyEventArgs> FoundModifiedFile;
        public event EventHandler<FileCopyErrorEventArgs> FileCopyError;
        public event EventHandler<FileDeletionEventArgs> FileDeleted;
        public event EventHandler<DirectoryCreationEventArgs> DirectoryCreated;
        public event EventHandler<DirectoryDeletionEventArgs> DirectoryDeleted;
        public event EventHandler<JobEventArgs> JobStarted;
        public event EventHandler<JobEventArgs> JobFinished;

        private System.Threading.Thread workerThread;

        private Job currentJob;

        private Queue<Job> jobQueue = new Queue<Job>();

        public long TotalWrittenBytes
        {
            get;
            private set;
        }

        public FileCounter.FileCounterResults FileCounterResult
        {
            get;
            private set;
        }

        private volatile bool paused;
        public bool Paused
        {
            get
            {
                return this.paused;
            }
        }

        public void Stop()
        {
            if (currentJob != null)
            {
                this.currentJob.Stop();
                this.jobQueue.Clear();
            }
        }

        public void Pause()
        {
            if (currentJob != null)
            {
                this.currentJob.Pause();
                this.paused = true;
            }
        }

        public void Continue()
        {
            if (currentJob != null)
            {
                this.currentJob.Continue();
                this.paused = false;
            }
        }

        private void DoNextJob()
        {
            if(this.jobQueue.Count > 0)
            {
                this.currentJob = this.jobQueue.Dequeue();
                this.InitEvents();
                this.workerThread = new System.Threading.Thread(this.currentJob.Start);
                this.workerThread.Start();
                this.OnJobStarted();
            }

            else
            {
                this.OnFinished();
            }
        }        
        
        public void Start(IEnumerable<JobSettings> jobs, bool preview)
        {
            this.TotalWrittenBytes = 0;

            foreach (JobSettings job in jobs)
            {
                switch (job.SyncMode)
                {
                    case SyncMode.Backup:
                        this.jobQueue.Enqueue(new BackupJob(job, preview));
                        break;

                    case SyncMode.Sync:
                        this.jobQueue.Enqueue(new SyncJob(job, preview));
                        break;
                }
            }

            this.FileCounterResult = this.GetFileCounterResults();

            if (this.FilesCounted != null)
            {
                this.FilesCounted.Invoke(this, new EventArgs());
            }

            this.DoNextJob();
        }

        private FileCounter.FileCounterResults GetFileCounterResults()
        {
            FileCounter.FileCounterResults result = new FileCounter.FileCounterResults();

            FileCounter counter = new FileCounter();

            foreach (Job job in this.jobQueue)
            {
                result += counter.CountJobFiles(job.Settings);
            }
            
            return result;
        }

        private void InitEvents()
        {
            this.currentJob.DirectoryCreated += new EventHandler<DirectoryCreationEventArgs>(currentJob_DirectoryCreated);
            this.currentJob.DirectoryDeleted += new EventHandler<DirectoryDeletionEventArgs>(currentJob_DirectoryDeleted);
            this.currentJob.FileCopyError += new EventHandler<FileCopyErrorEventArgs>(currentJob_FileCopyError);
            this.currentJob.FileDeleted += new EventHandler<FileDeletionEventArgs>(currentJob_FileDeleted);
            this.currentJob.FileProceeded += new EventHandler<FileProceededEventArgs>(currentJob_FileProceeded);
            this.currentJob.Finished += new EventHandler(currentJob_Finished);
            this.currentJob.FoundModifiedFile += new EventHandler<FileCopyEventArgs>(currentJob_FoundModifiedFile);
            this.currentJob.FoundNewFile += new EventHandler<FileCopyEventArgs>(currentJob_FoundNewerFile);
        }

        void currentJob_FoundNewerFile(object sender, FileCopyEventArgs e)
        {
            if (this.FoundNewerFile != null)
            {
                this.FoundNewerFile.Invoke(this, e);
            }
        }

        void currentJob_FoundModifiedFile(object sender, FileCopyEventArgs e)
        {
            if (this.FoundModifiedFile != null)
            {
                this.FoundModifiedFile.Invoke(this, e);
            }
        }

        void currentJob_Finished(object sender, EventArgs e)
        {
            Job job = (Job)sender;

            this.OnJobFinished(job.Settings);

            this.TotalWrittenBytes += job.WrittenBytes;

            this.DoNextJob();
        }

        void currentJob_FileProceeded(object sender, FileProceededEventArgs e)
        {
            if (this.FileProceeded != null)
            {
                this.FileProceeded.Invoke(this, e);
            }
        }

        void currentJob_FileDeleted(object sender, FileDeletionEventArgs e)
        {
            if (this.FileDeleted != null)
            {
                this.FileDeleted.Invoke(this, e);
            }
        }

        void currentJob_FileCopyError(object sender, FileCopyErrorEventArgs e)
        {
            if (this.FileCopyError != null)
            {
                this.FileCopyError.Invoke(this, e);
            }
        }

        void currentJob_DirectoryDeleted(object sender, DirectoryDeletionEventArgs e)
        {
            if (this.DirectoryDeleted != null)
            {
                this.DirectoryDeleted.Invoke(this, e);
            }
        }

        void currentJob_DirectoryCreated(object sender, DirectoryCreationEventArgs e)
        {
            if (this.DirectoryCreated != null)
            {
                this.DirectoryCreated.Invoke(this, e);
            }
        }

        private void OnFinished()
        {
            if (this.Finished != null)
            {
                this.Finished.Invoke(this, new EventArgs());
            }
        }

        private void OnJobStarted()
        {
            if(this.jobQueue != null)
            {
                this.JobStarted.Invoke(this, new JobEventArgs(this.currentJob.Settings));
            }
        }

        private void OnJobFinished(JobSettings job)
        {
            if(this.JobFinished != null)
            {
                this.JobFinished.Invoke(this, new JobEventArgs(job));
            }
        }
    }
}