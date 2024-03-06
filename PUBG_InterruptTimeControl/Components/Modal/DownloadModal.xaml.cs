using PUBG_InterruptTimeControl.Components.Modal.Action;
using PUBG_InterruptTimeControl.Service.Download;
using PUBG_InterruptTimeControl.Service.Enum;
using PUBG_InterruptTimeControl.Service.Wrapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace PUBG_InterruptTimeControl.Components.Modal
{
    /// <summary>
    /// CustomModal.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DownloadModal : Window
    {
        public ResultWrapper result;

        public DownloadModal()
        {
            InitializeComponent();
            result = new ResultWrapper();
        }

        #region Function
        public async void Download(string url)
        {            
            using (var api = new HttpClientDownloadWithProgress(result, url))
            {
                api.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                {
                    this.Dispatcher.Invoke(() => // 이벤트 비동기로 동작하므로 invoke로.
                    {
                        ProgressBar_Gage.Value = (double)progressPercentage;
                        if (totalBytesDownloaded >= totalFileSize) // 다운로드가 다 됐으면 success
                            result.Value = ResultEnum.Success;
                    });
                };
                await api.Download();
            }
            this.Dispatcher.Invoke(() =>
            {
                this.Close();
            });
        }
        #endregion


        #region Handler
        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

    }
}
