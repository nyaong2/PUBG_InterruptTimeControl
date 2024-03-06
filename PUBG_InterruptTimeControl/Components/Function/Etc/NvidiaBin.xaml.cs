using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using PUBG_InterruptTimeControl.Service.Msg;
using PUBG_InterruptTimeControl.Components.Modal.Action;
using PUBG_InterruptTimeControl.Service.Download;
using PUBG_InterruptTimeControl.Service.Zip;
using PUBG_InterruptTimeControl.Service.Enum;
using PUBG_InterruptTimeControl.Service.PgReg;

namespace PUBG_InterruptTimeControl.Components.Function.Etc
{
    /// <summary>
    /// MouseDoubleClick.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NvidiaBin : UserControl
    {
        private readonly MsgService msgService;
        private readonly DownloadService downloadService;
        private readonly ZipService zipService;
        private readonly ProgramUtilService pgUtilService;
        private readonly string nvidiaBinPath = Environment.GetEnvironmentVariable("windir") + @"\system32\drivers\NVIDIA Corporation\Drs";
        private readonly string binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Data\bin";

        public NvidiaBin()
        {
            InitializeComponent();
            msgService = new MsgService();
            downloadService = new DownloadService();
            zipService = new ZipService();
            pgUtilService = new ProgramUtilService();

        }
        private void NvidiaBin_Loaded(object sender, RoutedEventArgs e) 
        {
            // Initialize Thread가 다안끝나면 ControlExit의 내용물이 제대로 동작하지 않아서 로드되고 난 이후로 동작하도록 변경.
            CheckBin();
            CheckNvidiaFolder();
        }

        #region Function

        private void CheckBin()
        {
            DirectoryInfo di = new DirectoryInfo(binPath);
            if (di.Exists == false)
                di.Create();

            var fileVersionPath = binPath + @"\version.txt";
            var version = pgUtilService.GetFileVersion(fileVersionPath);
            var downloadVersion = downloadService.GetVersion(pgUtilService.url_DatVersion);
            if (version == double.NaN || version < downloadVersion || di.GetFiles().Length <= 1)
            {
                if ((int)MsgEnum.Result.Yes == msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.YesNo,
                                                "bin파일이 존재하지 않거나 구버전입니다.\r\n다운로드 하시겠습니까?"))
                {
                    di.Delete(true);
                    di.Create();
                    if ((downloadService.Download(pgUtilService.url_BinFile) == ResultEnum.Success) &&
                          (zipService.Extract(downloadService.GetDownloadedFilePath(pgUtilService.url_BinFile), binPath) == ResultEnum.Success))
                    {
                        pgUtilService.SetFileVersion(fileVersionPath, downloadVersion);
                        return;
                    }
                }
                ControlExit();
            }
        }

        private void CheckNvidiaFolder()
        {
            if (Directory.Exists(nvidiaBinPath) == false)
                Directory.CreateDirectory(nvidiaBinPath);
        }

        private List<string> GetBinList(int selectMenu)
        {
            var data = new List<string>();
            var firstVersionName = '0'; //파일명 1개 가져오는 것은 char라서 ''로.

            try
            {
                DirectoryInfo di = new DirectoryInfo(binPath);
                var list = di.GetFiles().Select(file => Path.GetFileNameWithoutExtension(file.Name)).ToList();
                list.Sort();

                switch (selectMenu)
                {
                    case 0:
                        firstVersionName = '3';
                        break;
                    case 1:
                        firstVersionName = '4';
                        break;
                    case 2:
                        firstVersionName = '5';
                        break;
                }

                foreach (var item in list)
                {
                    if (item[0].Equals("version") || item[0].Equals(firstVersionName) == false)
                        continue;
                    data.Add(item);
                }
            }
            catch { };

            return data;
        }

        private void ControlExit()
        {
            Window window = Window.GetWindow(this);
            window?.Close();
        }
        #endregion

        #region Handler
        private void ListBox_Menu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectMenu = (sender as ListBox).SelectedIndex;
            ListBox_Bin.ItemsSource = GetBinList(selectMenu);
        }

        private void Button_Apply_Click(object sender, RoutedEventArgs e)
        {
            var selectFileName = ListBox_Bin.SelectedItem as string;
            if (string.IsNullOrEmpty(selectFileName))
            {
                msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "bin파일이 선택되지 않았습니다.");
                return;
            }

            selectFileName += ".bin";
            StringBuilder sb = new StringBuilder();
            sb.Append(binPath)
              .Append(@"\")
              .Append(selectFileName);
            File.Copy(sb.ToString(), nvidiaBinPath + @"\nvdrsdb.bin", true);
        }
        #endregion

    }
}
