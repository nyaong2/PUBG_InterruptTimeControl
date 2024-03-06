using System;
using System.CodeDom;
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

namespace PUBG_InterruptTimeControl.Components.Modal
{
    /// <summary>
    /// CustomModal.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SmallPopupModal : Window
    {
        UserControl userControl;

        public SmallPopupModal(UserControl userControl)
        {
            InitializeComponent();
            this.userControl = userControl;
            ComponentGrid.Children.Add(this.userControl);
        }

        private void CustomModal_Unloaded(object sender, RoutedEventArgs e)
        {
            ComponentGrid.Children.Remove(userControl);
            userControl = null;
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
