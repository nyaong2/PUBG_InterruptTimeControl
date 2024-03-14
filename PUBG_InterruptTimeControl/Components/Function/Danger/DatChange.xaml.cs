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
using System.Reflection;
using System.Security.AccessControl;
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

namespace PUBG_InterruptTimeControl.Components.Function.Danger
{
    /// <summary>
    /// MouseDoubleClick.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DatChange : UserControl
    {
        private readonly MsgService msgService;
        private readonly ProgramUtilService pgUtilService;
        private readonly DownloadService downloadService;
        private readonly ZipService zipService;

        private readonly string datPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Data\dat";
        private readonly string str_Steam = "스배";
        private readonly string str_Kakao = "카배";
        private readonly SolidColorBrush selectServerBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF21CA4A"));    

        public DatChange()
        {
            InitializeComponent();
            msgService = new MsgService();
            pgUtilService = new ProgramUtilService();
            downloadService = new DownloadService();
            zipService = new ZipService();
        }

        private void DatChange_Loaded(object sender, RoutedEventArgs e)
        {
            CheckDat();
            ListBox_Dat.ItemsSource = GetDatList();

            //등록이 안되어있으면 비활성화.
            if(String.IsNullOrEmpty(pgUtilService.reg_KakaoPath))
                Button_ServerKakao.IsEnabled = false;
            if (String.IsNullOrEmpty(pgUtilService.reg_SteamPath))
                Button_ServerSteam.IsEnabled = false;
        }

        #region Function
        private void CheckDat()
        {
            DirectoryInfo di = new DirectoryInfo(datPath);
            if (di.Exists == false)
                di.Create();

            var fileVersionPath = datPath + @"\version.txt";
            var version = pgUtilService.GetFileVersion(fileVersionPath);
            var downloadVersion = downloadService.GetVersion(pgUtilService.url_DatVersion);
            if (version == double.NaN || version < downloadVersion || di.GetFiles().Length <= 1)
            {
                if ((int)MsgEnum.Result.Yes == msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.YesNo,
                                                "dat파일이 존재하지 않거나 구버전입니다.\r\n다운로드 하시겠습니까?"))
                {
                    di.Delete(true);
                    di.Create();
                    if ((downloadService.Download(pgUtilService.url_DatFile) == ResultEnum.Success) &&
                          (zipService.Extract(downloadService.GetDownloadedFilePath(pgUtilService.url_DatFile), datPath) == ResultEnum.Success))
                    {
                        pgUtilService.SetFileVersion(fileVersionPath, downloadVersion);
                        return;
                    }
                }
                ControlExit();
            }
        }

        private List<string> GetDatList()
        {
            DirectoryInfo di = new DirectoryInfo(datPath);
            var list = di.GetFiles().Select(file => Path.GetFileNameWithoutExtension(file.Name)).ToList();
            list.RemoveAll(item => item.ToLower().Contains("version"));
            list.Sort();
            return list;
        }

        private void Apply()
        {
            if (Label_CurrentValue.Content.Equals("선택되지 않음"))
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "오른쪽의 서버를 선택해주세요.");
                return;
            }

            var selectFileName = ListBox_Dat.SelectedItem as string;
            if (string.IsNullOrEmpty(selectFileName))
            {
                msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "dat파일이 선택되지 않았습니다.");
                return;
            }

            if (Util.Processes.FindName(new string[] { "Tslgame.exe", "Tslgame_BE.exe" }))
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "배그를 끄고 진행해주세요.");
                return;
            }

            selectFileName += ".dat";
            StringBuilder sb = new StringBuilder();
            sb.Append(datPath)
                .Append(@"\")
                .Append(selectFileName);
            var serverDatPath = Label_CurrentValue.Content.Equals(str_Steam) ? pgUtilService.reg_SteamPath : pgUtilService.reg_KakaoPath;
            serverDatPath += @"\bgsecondary.dat";

            if (Util.File.Permission.Delete("everyone", serverDatPath) == false)
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "적용에 실패했습니다.");
                return;
            }

            try
            {
                if (Util.File.Permission.Delete("everyone", serverDatPath) == false)
                    throw new Exception();
                File.Copy(sb.ToString(), serverDatPath, true);
                Util.File.Permission.Add("everyone", FileSystemRights.FullControl, AccessControlType.Deny, serverDatPath);
            }
            catch
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "적용에 실패했습니다.");
                return;
            }
        }

        private void Restore()
        {
            if (Label_CurrentValue.Content.Equals("선택되지 않음"))
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "오른쪽의 서버를 선택해주세요.");
                return;
            }

            if (Util.Processes.FindName(new string[] { "Tslgame.exe", "Tslgame_BE.exe" }))
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "배그를 끄고 진행해주세요.");
                return;
            }

            var serverDatPath = Label_CurrentValue.Content.Equals(str_Steam) ? pgUtilService.reg_SteamPath : pgUtilService.reg_KakaoPath;
            serverDatPath += @"\bgsecondary.dat";

            if (Util.File.Permission.Delete("everyone", serverDatPath) == false)
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "원상복구에 실패했습니다.");
                return;
            }

            msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "배그를 껐다 켜주셔야 원상복구 됩니다.");
        }

        private void ControlExit()
        {
            Window window = Window.GetWindow(this);
            window?.Close();
        }
        #endregion

        #region Handler
        private void Button_Apply_Click(object sender, RoutedEventArgs e)
        {
            Apply();
        }

        private void Buttn_Restore_Click(object sender, RoutedEventArgs e)
        {
            Restore();
        }

        private void Button_ServerSteam_Click(object sender, RoutedEventArgs e)
        {
            Label_CurrentValue.Content = str_Steam;
            Label_CurrentValue.Foreground = selectServerBrush;
        }

        private void Button_ServerKakao_Click(object sender, RoutedEventArgs e)
        {
            Label_CurrentValue.Content = str_Kakao;
            Label_CurrentValue.Foreground = selectServerBrush;
        }

        #endregion

    }
}
