using PUBG_InterruptTimeControl.Components.Modal.Action;
using PUBG_InterruptTimeControl.Service.Msg;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Shapes;

namespace PUBG_InterruptTimeControl.Components.Function.Windows
{
    /// <summary>
    /// MouseDoubleClick.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PowerOption : UserControl
    {
        #region DllImport
        [DllImport("powrprof.dll")]
        public static extern uint PowerReadACValueIndex(IntPtr RootPowerKey, IntPtr SchemeGuid, ref Guid SubGroupOfPowerSettingsGuid, ref Guid PowerSettingGuid, ref uint AcValueIndex);
        [DllImport("powrprof.dll")]
        public static extern uint PowerWriteACValueIndex(IntPtr RootPowerKey, IntPtr SchemeGuid, ref Guid SubGroupOfPowerSettingsGuid, ref Guid PowerSettingGuid, uint AcValueIndex);
        [DllImport("powrprof.dll")]
        private static extern uint PowerSetActiveScheme(
            [In, Optional] IntPtr UserPowerKey,
            [In] ref Guid ActivePolicyGuid
        );
        [DllImport("powrprof.dll")]
        public static extern uint PowerGetActiveScheme(IntPtr UserRootPowerKey, out IntPtr SchemeGuid);
        [DllImport("kernel32.dll")]
        public static extern IntPtr LocalFree(IntPtr hMem);
        #endregion

        private readonly MsgService msgService = new MsgService();
        private const string GUID_POWERPERFORMANCE = "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c";
        private const string GUID_BALANCE = "381b4222-f694-41f0-9685-ff5bb260df2e";
        private const string GUID_POWERSAVE = "a1841308-3541-4fab-bc81-f71556f20b4a";
        private string GUID_GetSuperPerformance = null;
        private Guid GUID_VIDEO_SUBGROUP = new Guid("7516b95f-f776-4464-8c53-06167f40cc99");
        private Guid GUID_VIDEO_POWERDOWN_TIMEOUT = new Guid("3c0bc021-c8a8-4e07-a973-6b14cbcb2b7e");
        private Guid GUID_SLEEP_SUBGROUP = new Guid("238C9FA8-0AAD-41ED-83F4-97BE242C8F20");
        private Guid GUID_STANDBY_TIMEOUT = new Guid("29F6C1DB-86DA-48C5-9FDB-F2B67B1F44DA");


        public PowerOption()
        {
            InitializeComponent();
            msgService = new MsgService();
        }
        private void PowerOption_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox_Display.Focus();

            GetDisplay();
            GetPowerSaving();
            GUID_GetSuperPerformance = Util.Dos.Cmd("for /f \"tokens=4\" %G in ('powercfg -list ^| findstr /C:\"최고의 성능\"') do %G");
            GetPowerOption();
        }

        #region Function
        private void GetDisplay()
        {
            Label_Display.Content = "NULL";
            IntPtr activePolicyGuid = IntPtr.Zero;
            uint timeout = int.MaxValue;
            if ((PowerGetActiveScheme(IntPtr.Zero, out activePolicyGuid) == 0) &&
                    (PowerReadACValueIndex(IntPtr.Zero, activePolicyGuid, ref GUID_VIDEO_SUBGROUP, ref GUID_VIDEO_POWERDOWN_TIMEOUT, ref timeout) == 0))
                    Label_Display.Content = timeout / 60;
            
        }

        private void SetDisplay()
        {
            var value = int.MaxValue;
            if (int.TryParse(TextBox_Display.Text, out value) == false)
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "숫자 정수를 입력해주세요.");
                return;
            }
            value *= 60;
            IntPtr activePolicyGuid = IntPtr.Zero;
            if ((PowerGetActiveScheme(IntPtr.Zero, out activePolicyGuid) != 0) ||
                    (PowerWriteACValueIndex(IntPtr.Zero, activePolicyGuid, ref GUID_VIDEO_SUBGROUP, ref GUID_VIDEO_POWERDOWN_TIMEOUT, (uint)value) != 0))
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "적용에 실패했습니다.");
                return;
            }
            Label_Display.Content = value / 60;

            msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "적용 되었습니다.");
        }

        private void GetPowerSaving()
        {
            Label_PowerSaving.Content = "NULL";
            IntPtr activePolicyGuid = IntPtr.Zero;
            uint value = int.MaxValue;
            if ((PowerGetActiveScheme(IntPtr.Zero, out activePolicyGuid) == 0) &&
                    (PowerReadACValueIndex(IntPtr.Zero, activePolicyGuid, ref GUID_SLEEP_SUBGROUP, ref GUID_STANDBY_TIMEOUT, ref value) == 0))
                Label_PowerSaving.Content = value / 60;
            else
                Label_PowerSaving.Content = "NULL";
        }

        private void SetPowerSaving()
        {
            var value = int.MaxValue;
            if (int.TryParse(TextBox_PowerSaving.Text, out value) == false)
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "숫자 정수를 입력해주세요.");
                return;
            }
            value *= 60;

            IntPtr activePolicyGuid = IntPtr.Zero;
            if ((PowerGetActiveScheme(IntPtr.Zero, out activePolicyGuid) != 0) ||
                    (PowerWriteACValueIndex(IntPtr.Zero, activePolicyGuid, ref GUID_SLEEP_SUBGROUP, ref GUID_STANDBY_TIMEOUT, (uint)value) != 0))
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "적용에 실패했습니다.");
                return;
            }
            Label_PowerSaving.Content = value / 60;

            msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "적용 되었습니다.");
        }

        private void GetPowerOption()
        {
            IntPtr activeSchemePtr;
            PowerGetActiveScheme(IntPtr.Zero, out activeSchemePtr);

            Guid activeScheme = (Guid)Marshal.PtrToStructure(activeSchemePtr, typeof(Guid));
            LocalFree(activeSchemePtr);

            Label_CurrentValue.Content = CurrentOptionHangle(activeScheme.ToString());
        }

        private void SetPowerOption(string GUID)
        {
            Guid guid = new Guid(GUID);

            if (PowerSetActiveScheme(IntPtr.Zero, ref guid) != 0)
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "전원옵션 변경에 실패했습니다.");
                return;
            }

            Label_CurrentValue.Content = CurrentOptionHangle(GUID);
            GetDisplay();
            GetPowerSaving();

            msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "적용 되었습니다.");
        }

        private string CurrentOptionHangle(string GUID)
        {
            switch (GUID)
            {
                case GUID_POWERPERFORMANCE:
                    return "고성능";
                case GUID_BALANCE:
                    return "균형 조정";
                case GUID_POWERSAVE:
                    return "절전";
                default:
                    if (GUID.Equals(GUID_GetSuperPerformance))
                        return "최고의 성능";
                    return "알 수 없음";
            }
        }
        #endregion

        #region Handler
        private void Button_SuperPerformance_Click(object sender, RoutedEventArgs e)
        {
            GUID_GetSuperPerformance = Util.Dos.Cmd("for /f \"tokens=4\" %G in ('powercfg -list ^| findstr /C:\"최고의 성능\"') do  %G");

            //최고의 성능이 추가가 되어있지 않다면 추가
            if (String.IsNullOrEmpty(GUID_GetSuperPerformance))
            {
                Util.Dos.Cmd("powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61");
                GUID_GetSuperPerformance = Util.Dos.Cmd("for /f \"tokens=4\" %G in ('powercfg -list ^| findstr /C:\"최고의 성능\"') do %G");
            }

            SetPowerOption(GUID_GetSuperPerformance);
        }

        private void Button_Performance_Click(object sender, RoutedEventArgs e)
        {
            SetPowerOption(GUID_POWERPERFORMANCE);
        }

        private void Button_Balance_Click(object sender, RoutedEventArgs e)
        {
            SetPowerOption(GUID_BALANCE);
        }

        private void Button_DisplayApply_Click(object sender, RoutedEventArgs e)
        {
            SetDisplay();
        }

        private void Button_PowerSavingApply_Click(object sender, RoutedEventArgs e)
        {
            SetPowerSaving();
        }
        #endregion



    }
}
