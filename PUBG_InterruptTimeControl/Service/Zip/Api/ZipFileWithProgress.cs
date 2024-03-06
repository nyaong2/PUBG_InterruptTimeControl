using PUBG_InterruptTimeControl.Service.Enum;
using PUBG_InterruptTimeControl.Service.Wrapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PUBG_InterruptTimeControl.Service.Zip.Api
{
    internal class ZipFileWithProgress : IDisposable
    {
        private readonly string path, extractPath;

        public delegate void ProgressChangedHandler(double? totalNumbers, long? totalCount);
        public event ProgressChangedHandler ProgressChanged;
        public ZipFileWithProgress(string path, string extractPath)
        {
            this.path = path;
            this.extractPath = extractPath;
        }

        public void ExtractWithProgress()
        {
            using (var archive = ZipFile.OpenRead(path))
            {
                var totalEntries = archive.Entries.Count;
                var progressMax = (double)totalEntries;
                var processedEntries = 0;

                foreach (var entry in archive.Entries)
                {
                    var destPath = Path.Combine(extractPath, entry.FullName);
                    var extractEntryPath = Path.GetDirectoryName(destPath);

                    if (Directory.Exists(extractEntryPath) == false)
                        Directory.CreateDirectory(extractEntryPath);
                    entry.ExtractToFile(destPath, true);

                    Application.Current.Dispatcher.Invoke(() =>
                    { // 크로스 스레드 방지
                        TriggerProgressChanged(++processedEntries, totalEntries);
                    });
                }
            }
        }
        private void TriggerProgressChanged(long? totalNumbers, long? totalCount)
        {
            if (ProgressChanged == null)
                return;
            
            ProgressChanged(totalNumbers, totalCount);
        }

        public void Dispose()
        {
            ProgressChanged = null;
        }
    }
}
