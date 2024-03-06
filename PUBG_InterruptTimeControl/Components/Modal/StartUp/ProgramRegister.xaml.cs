using Microsoft.Win32;
using PUBG_InterruptTimeControl.Components.Modal.Action;
using PUBG_InterruptTimeControl.Service.Msg;
using PUBG_InterruptTimeControl.Service.PgReg;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace PUBG_InterruptTimeControl.Components.StartUp
{
    /// <summary>
    /// CustomModal.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ProgramRegister : Window
    {
        const string programRegPath = @"HKEY_CURRENT_USER\SOFTWARE\CatPubg";
        MsgService msgService;
        ProgramUtilService pgRegService;
        OpenFileDialog fileDialog;

        #region Constructor
        public ProgramRegister()
        {
            InitializeComponent();
            msgService = new MsgService();
            pgRegService = new ProgramUtilService();
            fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Exe Files (.exe)|*.exe";
            RegExistsLabelChange();
        }
        #endregion

        #region Function
        private string GetSelctFilePath()
        {
            if (fileDialog.ShowDialog() == false)
                return null;

            if (Path.GetFileName(fileDialog.FileName).ToLower().Equals("tslgame.exe") == false)
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "Tslgame.exe를 지정해주세요.");
                return null;
            }
            else if (File.Exists(Path.GetDirectoryName(fileDialog.FileName)+@"\tslgame_be.exe") == false)
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "정식적인 배그 폴더의 위치가 아닙니다.\n다시 지정해주세요.");
                return null;
            }

            return Path.GetDirectoryName(fileDialog.FileName);
        }

        private void RegExistsLabelChange()
        {
            if (Util.Reg.Exist(programRegPath, "SteamPath"))
            {
                Label_SteamApplyCheck.Content = "등록됨";
                Label_SteamApplyCheck.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF21CA4A"));
            }
            else
            {
                Label_SteamApplyCheck.Content = "미등록";
                Label_SteamApplyCheck.Foreground = Brushes.Red;
            }

            if (Util.Reg.Exist(programRegPath, "KakaoPath"))
            {
                Label_KakaoApplyCheck.Content = "등록됨";
                Label_KakaoApplyCheck.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF21CA4A"));
            }
            else
            {
                Label_KakaoApplyCheck.Content = "미등록";
                Label_KakaoApplyCheck.Foreground = Brushes.Red;
            }

        }
        #endregion

        #region Handler
        private void Button_Confirmation_Click(object sender, RoutedEventArgs e)
        {
            if (Label_SteamApplyCheck.Content.Equals("등록됨") || Label_KakaoApplyCheck.Content.Equals("등록됨"))
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "두 서버 중 하나는 등록이 돼야 합니다.");
                return;
            }
        }

        private void Button_SteamPubg_Click(object sender, RoutedEventArgs e)
        {
            var path = GetSelctFilePath();
            if (path == null)
                return;

            if (pgRegService.Registerer(programRegPath, "SteamPath", path, Util.Reg.RegValueKind.SZ))
                RegExistsLabelChange();

        }

        private void Button_KakaoPubg_Click(object sender, RoutedEventArgs e)
        {
            var path = GetSelctFilePath();
            if (path == null)
                return;

            if (pgRegService.Registerer(programRegPath, "KakaoPath", path, Util.Reg.RegValueKind.SZ))
                RegExistsLabelChange();
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Button_SteamRegDelete_Click(object sender, RoutedEventArgs e)
        {
            if (pgRegService.RegDelete(programRegPath, "SteamPath"))
            {
                Label_SteamApplyCheck.Content = "미등록";
                Label_SteamApplyCheck.Foreground = Brushes.Red;
            }
        }

        private void Button_KakaoRegDelete_Click(object sender, RoutedEventArgs e)
        {
            if (pgRegService.RegDelete(programRegPath, "KakaoPath"))
            {
                Label_KakaoApplyCheck.Content = "미등록";
                Label_KakaoApplyCheck.Foreground = Brushes.Red;
            }
        }
        #endregion
    }
}
