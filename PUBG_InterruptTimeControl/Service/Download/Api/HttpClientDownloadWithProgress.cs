using PUBG_InterruptTimeControl.Components.Modal.Action;
using PUBG_InterruptTimeControl.Service.Enum;
using PUBG_InterruptTimeControl.Service.Msg;
using PUBG_InterruptTimeControl.Service.Wrapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PUBG_InterruptTimeControl.Service.Download
{
    internal class HttpClientDownloadWithProgress : IDisposable
    {
        private readonly string downloadUrl;
        private readonly string destPath;
        private CancellationTokenSource cts;
        private ResultWrapper result;

        public delegate void ProgressChangedHandler(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage);
        public event ProgressChangedHandler ProgressChanged;

        private HttpClient httpClient;

        public HttpClientDownloadWithProgress(ResultWrapper result, string downloadUrl)
        {
            cts = new CancellationTokenSource();
            this.result = result;
            this.downloadUrl = downloadUrl;
            var fileName = Path.GetFileName(new Uri(downloadUrl).AbsolutePath);
            fileName = fileName.Insert(0, "\\");
            this.destPath = Environment.GetEnvironmentVariable("TEMP") + fileName;
        }

        public async Task Download()
        {
            httpClient = new HttpClient { Timeout = TimeSpan.FromDays(1) };

            try
            {
                using (var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cts.Token))
                    await DownloadFileFromHttpResponseMessage(response);
            }catch {
                result.Value= ResultEnum.Error;
                Dispose();
            }
        }

        private async Task DownloadFileFromHttpResponseMessage(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
            var totalBytes = response.Content.Headers.ContentLength;

            using (var contentStream = await response.Content.ReadAsStreamAsync())
                await ProcessContentStream(totalBytes,contentStream);
        }
        private async Task ProcessContentStream(long? totalDownloadSize, Stream contentStream)
        {
            var totalBytesRead = 0L;
            var readCount = 0L;
            var buffer = new byte[8192];
            var isMoreToRead = true;

            using (var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                do
                {
                    if (cts.Token.IsCancellationRequested)
                        break;
                    var bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                    if (bytesRead == 0)
                    {
                        isMoreToRead = false;
                        TriggerProgressChanged(totalDownloadSize, totalBytesRead);
                        continue;
                    }

                    if (cts.Token.IsCancellationRequested)
                        break;
                    await fileStream.WriteAsync(buffer, 0, bytesRead, cts.Token);

                    totalBytesRead += bytesRead;
                    readCount += 1;

                    if (readCount % 100 == 0)
                        TriggerProgressChanged(totalDownloadSize, totalBytesRead);
                }
                while (isMoreToRead);
            }
            if (cts.Token.IsCancellationRequested) // 다운로드 중에 종료메시지를 보낸 경우 파일 삭제
            {
                try
                {
                    File.Delete(destPath);
                }
                catch { };
            }
        }

        private void TriggerProgressChanged(long? totalDownloadSize, long totalBytesRead)
        {
            if (ProgressChanged == null)
                return;

            double? progressPercentage = null;
            if (totalDownloadSize.HasValue)
                progressPercentage = Math.Round((double)totalBytesRead / totalDownloadSize.Value * 100, 2);

            ProgressChanged(totalDownloadSize, totalBytesRead, progressPercentage);
        }

        public void Dispose()
        {
            if (cts.IsCancellationRequested == false)
                cts.Cancel();

            cts?.Dispose();

            ProgressChanged = null;
            httpClient?.Dispose();

        }
    }
}
