using PUBG_InterruptTimeControl.Components.Function.Danger;
using PUBG_InterruptTimeControl.Components.Function.Etc;
using PUBG_InterruptTimeControl.Components.Function.Mouse;
using PUBG_InterruptTimeControl.Components.Function.Windows;
using System;
using System.Collections.Generic;
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

namespace PUBG_InterruptTimeControl.Components
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BottomDisign : UserControl
    {
        public event Action<UserControl> activeUserControlEvent; 

        public BottomDisign()
        {
            InitializeComponent();
        }


        #region Button Event
        private void Button_MouseDoubleClick_Click(object sender, RoutedEventArgs e)
        {
            activeUserControlEvent(new MouseDoubleClick());
        }
        private void Button_MouseAccessiblityReg_Click(object sender, RoutedEventArgs e)
        {
            activeUserControlEvent(new MouseAccessibilityReg());
        }

        private void Button_PowerOption_Click(object sender, RoutedEventArgs e)
        {
            activeUserControlEvent(new PowerOption());
        }
        private void Button_Services_Click(object sender, RoutedEventArgs e)
        {
            activeUserControlEvent(new Services());
        }
        private void Button_ProcessorPriority_Click(object sender, RoutedEventArgs e)
        {
            activeUserControlEvent(new Win32Priority());
        }
        private void Button_NtpServer_Click(object sender, RoutedEventArgs e)
        {
            activeUserControlEvent(new NtpServer());
        }

        private void Button_NvidiaBin_Click(object sender, RoutedEventArgs e)
        {
            activeUserControlEvent(new NvidiaBin());
        }
        private void Button_BeServerPath_Click(object sender, RoutedEventArgs e)
        {
            activeUserControlEvent(new BEServerChange());
        }
        private void Button_ProcessDeny_Click(object sender, RoutedEventArgs e)
        {
            activeUserControlEvent(new ProcessDeny());
        }
        private void Button_WindowsMagnifier_Click(object sender, RoutedEventArgs e)
        {
            activeUserControlEvent(new WindowsMagnifier());
        }
        private void Button_WindowsVarietyReg_Click(object sender, RoutedEventArgs e)
        {
            activeUserControlEvent(new WindowsVarietyReg());
        }
        private void Button_WindowsVarietyInstruction_Click(object sender, RoutedEventArgs e)
        {
            activeUserControlEvent(new WindowsVarietyInstruction());
        }

        private void Button_DatChange_Click(object sender, RoutedEventArgs e)
        {
            activeUserControlEvent(new DatChange());
        }
        private void Button_FileEngine_Click(object sender, RoutedEventArgs e)
        {
            activeUserControlEvent(new FileEngine());
        }
        private void Button_StartOptionProgram_Click(object sender, RoutedEventArgs e)
        {
            activeUserControlEvent(new StartOptionProgram());
        }
        #endregion

    }
}
