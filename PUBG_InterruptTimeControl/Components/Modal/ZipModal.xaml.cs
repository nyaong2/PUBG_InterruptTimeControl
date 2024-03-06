using PUBG_InterruptTimeControl.Components.Modal.Action;
using PUBG_InterruptTimeControl.Service.Download;
using PUBG_InterruptTimeControl.Service.Enum;
using PUBG_InterruptTimeControl.Service.Wrapper;
using PUBG_InterruptTimeControl.Service.Zip.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
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
    public partial class ZipModal : Window
    {
        readonly string path, extractPath;
        public ResultWrapper result;

        public ZipModal(string path, string extractPath)
        {
            InitializeComponent();
            this.path = path;
            this.extractPath = extractPath;
            result = new ResultWrapper();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e) // Service단에서 Task돌리기가 애매해서 모달창에서 돌리는걸로.
        {
            Task.Run(() => Extract(path, extractPath));
        }

        #region Function
        public void Extract(string path, string extractPath)
        {
            using (var api = new ZipFileWithProgress(path, extractPath))
            {
                api.ProgressChanged += (totalNumbers, totalCount) =>
                {
                    if(ProgressBar_Gage.Maximum != (double)totalCount)
                        ProgressBar_Gage.Maximum = (double)totalCount;

                    this.Dispatcher.Invoke(() => //스레드 안전빵
                    {
                        ProgressBar_Gage.Value = (double)totalNumbers;

                        if (ProgressBar_Gage.Value >= ProgressBar_Gage.Maximum)
                            result.Value = ResultEnum.Success;
                    });
                };
                api.ExtractWithProgress();
            }
            this.Dispatcher.Invoke(() => //스레드 안전빵
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
