using PUBG_InterruptTimeControl.Components.Modal.Action;
using PUBG_InterruptTimeControl.Service.Download;
using PUBG_InterruptTimeControl.Service.Enum;
using PUBG_InterruptTimeControl.Service.Msg;
using PUBG_InterruptTimeControl.Service.PgReg;
using PUBG_InterruptTimeControl.Service.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using System.Windows.Shapes;

namespace PUBG_InterruptTimeControl.Components.Function.Danger
{
    /// <summary>
    /// MouseDoubleClick.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class StartOptionProgram : UserControl
    {
        private readonly MsgService msgService;
        private readonly DownloadService downloadService;
        private readonly ZipService zipService;
        private readonly ProgramUtilService pgUtilService;
        private string etcPath;
        private string startUpFilePath;
        public StartOptionProgram()
        {
            InitializeComponent();
            msgService = new MsgService();
            downloadService = new DownloadService();
            zipService = new ZipService();
            pgUtilService = new ProgramUtilService();
            etcPath = pgUtilService.processesFilePath + @"\Data\etc";
            startUpFilePath = etcPath + @"\TslGame_BE.exe";
        }
        private void StartOptionProgram_Loaded(object sender, RoutedEventArgs e)
        {
            CheckStartUpFileExist();
            if (String.IsNullOrEmpty(pgUtilService.reg_SteamPath))
            {
                Button_SteamApply.IsEnabled = false;
                Button_SteamRestore.IsEnabled = false;
            }

            if (String.IsNullOrEmpty(pgUtilService.reg_KakaoPath))
            {
                Button_KakaoApply.IsEnabled = false;
                Button_KakaoRestore.IsEnabled = false;
            }
        }

        #region Function
        private void CheckStartUpFileExist()
        {
            DirectoryInfo di = new DirectoryInfo(etcPath);
            if (di.Exists == false)
                di.Create();

            var fileVersionPath = etcPath + @"\version.txt";
            var version = pgUtilService.GetFileVersion(fileVersionPath);
            var downloadVersion = downloadService.GetVersion(pgUtilService.url_DatVersion);
            if (version == double.NaN || version < downloadVersion ||
                di.GetFiles().Any(file => file.Name.Equals("TslGame_BE.exe")) == false || di.GetFiles().Length <= 1)
            {
                if ( msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.YesNo,
                                    "StartUp파일이 존재하지 않거나 구버전입니다.\r\n다운로드 하시겠습니까?") == (int)MsgEnum.Result.Yes)
                {
                    di.Delete(true);
                    di.Create();
                    if ((downloadService.Download(pgUtilService.url_EtcFile) == ResultEnum.Success) &&
                          (zipService.Extract(downloadService.GetDownloadedFilePath(pgUtilService.url_EtcFile), etcPath) == ResultEnum.Success))
                        pgUtilService.SetFileVersion(fileVersionPath, downloadVersion);
                    
                } else
                    ControlExit();
            }
        }

        private void Apply(string regServerPath)
        {
            if(Util.Processes.FindName(new string[] { "Tslgame.exe" }))
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "배그를 끄고 진행해주세요.");
                return;
            }

            var bePath = regServerPath + @"\TslGame_BE.exe";
            if (File.Exists(bePath) == false)
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "적용에 필요한 파일이 존재하지 않습니다.\r\n무결성 검사를 하시고 다시 진행해주세요.");
                return;
            }

            // 이미 적용되어있는지 크기로 확인
            if(new FileInfo(bePath).Length == new FileInfo(startUpFilePath).Length)
            {
                msgService.Show(MsgEnum.Category.Waring, MsgEnum.CloseType.Close, "이미 적용되어 있습니다.");
                return;
            }

            try
            {
                //기존 원본파일 파일명 변경
                File.Move(bePath, regServerPath + @"\TslGame_BE2.exe");
                // 시작옵션 프로그램 복사
                File.Copy(startUpFilePath, bePath);
            } catch
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "적용에 실패했습니다.");
                return;
            }
            msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "적용이 완료되었습니다.");
        }
        private void Restore(string regServerPath)
        {
            if (Util.Processes.FindName(new string[] { "Tslgame.exe" }))
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "배그를 끄고 진행해주세요.");
                return;
            }
            var originalBePath = regServerPath + @"\TslGame_BE.exe";
            var applyBePath = regServerPath + @"\TslGame_BE2.exe";

            // 적용 안되어있는지 크기로 확인
            if (new FileInfo(originalBePath).Length != new FileInfo(startUpFilePath).Length)
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "미 적용 상태입니다.");
                return;
            }

            // 원본파일 이름 변경한 것이 없다면
            if (File.Exists(applyBePath) == false)
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "원상복구에 필요한 파일이 존재하지 않습니다.\r\n무결성 검사를 통해 복구해주세요.");
                return;
            }

            try
            {
                //시작옵션 파일 제거
                File.Delete(regServerPath + @"\TslGame_BE.exe");

                //기존 원본파일 이름변경
                File.Move(applyBePath, originalBePath);
            } catch
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "적용에 실패했습니다.");
                return;
            }
            msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "원상복구 되었습니다.");

        }
        private void ControlExit()
        {
            Window window = Window.GetWindow(this);
            window?.Close();
        }
        #endregion


        #region Handler
        private void Button_SteamApply_Click(object sender, RoutedEventArgs e)
        {
            Apply(pgUtilService.reg_SteamPath);
        }
        private void Button_SteamRestore_Click(object sender, RoutedEventArgs e)
        {
            Restore(pgUtilService.reg_SteamPath);
        }

        private void Button_KakaoApply_Click(object sender, RoutedEventArgs e)
        {
            Apply(pgUtilService.reg_KakaoPath);
        }
        private void Button_KakaoRestore_Click(object sender, RoutedEventArgs e)
        {
            Restore(pgUtilService.reg_KakaoPath);
        }

        #endregion




    }
}
