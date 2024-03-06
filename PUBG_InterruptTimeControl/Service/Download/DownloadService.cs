using PUBG_InterruptTimeControl.Components.Modal;
using PUBG_InterruptTimeControl.Components.Modal.Action;
using PUBG_InterruptTimeControl.Service.Enum;
using PUBG_InterruptTimeControl.Service.Msg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PUBG_InterruptTimeControl.Service.Download
{
    internal class DownloadService
    {
        private readonly MsgService msgService;
        private static DownloadService instance;

        public readonly string downloadPath = Environment.GetEnvironmentVariable("TEMP");

        public DownloadService()
        {
            msgService = new MsgService();
        }

        public static DownloadService Instance
        {
            get
            {
                if (Instance == null)
                    instance = new DownloadService();
                return instance;
            }
        }

        public ResultEnum Download(string url)
        {
            
            var modal = new DownloadModal();
            modal.Download(url);
            modal.ShowDialog();
            var result = modal.result.Value;

            if (result == ResultEnum.Error)
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "다운로드에 실패했습니다.");
            return result;
        }

        public string GetDownloadedFilePath(string url)
        {
            var fileName = Path.GetFileName(new Uri(url).AbsolutePath);
            fileName = fileName.Insert(0, "\\");

            return downloadPath + fileName;
        }

        public double GetVersion(string url)
        {
            using (var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) })
            {
                double version = double.NaN;
                double.TryParse(httpClient.GetStringAsync(url).GetAwaiter().GetResult(), out version);
                return version;
            }
        }

        public string GetHomepageUrl(string url)
        {
            using (var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) })
            {
                return httpClient.GetStringAsync(url).GetAwaiter().GetResult();
            }
        }
    }
}
