using PUBG_InterruptTimeControl.Components.Modal.Action;
using PUBG_InterruptTimeControl.Components.Object.Services;
using PUBG_InterruptTimeControl.Service.Msg;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceProcess;
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

namespace PUBG_InterruptTimeControl.Components.Function.Windows
{
    /// <summary>
    /// MouseDoubleClick.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Services : UserControl
    {
        private MsgService msgService;
        private ServiceController[] serviceControllerList = null;
        private List<ListViewServicesObject> allServicesList; // 따로 만들어서 itemSources에 넣는 이유는 Status, startType가 읽기전용이라 한글로 바꿀 수 없어서 따로만들어서 추가하는 용도로 넣음
        private List<ListViewServicesObject> necessityList;

        public Services()
        {
            InitializeComponent();
            msgService = new MsgService();
            allServicesList = GetAllServices();

            if (allServicesList.Count <= 0)
                ControlExit();
            else
                necessityList = GetNecessityServices();
        }
        private void Services_Loaded(object sender, RoutedEventArgs e)
        {
            Checkbox_Necessity.IsChecked = true;
            LV_Service.ItemsSource = necessityList;
        }


        #region Fucntion
        private List<ListViewServicesObject> GetNecessityServices()
        {
            var necessityServicesList = new List<ListViewServicesObject>();
            var necessityNameList = GetNecessityServicesNameList();

            allServicesList.ToList().ForEach(service =>
            {
                if (necessityNameList.Contains(service.serviceName.ToLowerInvariant()))
                    necessityServicesList.Add(service);
            });

            return necessityServicesList;
        }

        private List<ListViewServicesObject> GetAllServices()
        {
            if (allServicesList != null) // 값이 있는 경우 + 값이 1개라도 있는 경우 제거
                allServicesList.Clear();

            // 1. 모든 서비스 가져옴
            serviceControllerList = Util.Service.GetServiceList();

            // 2. 잘 가져왔는지 체크
            if (serviceControllerList.Length < 0)
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "서비스를 불러오지 못했습니다.");
                return null;
            }

            // 3. 모든 서비스에서 각종 리스트 판별 후 추가 및 컬러 설정
            var list = new List<ListViewServicesObject>();
            foreach (ServiceController service in serviceControllerList)
                list.Add(new ListViewServicesObject(null, service.ServiceName, service.DisplayName, service.StartType.ToString(), service.Status.ToString()));

            return list;
        }

        private List<string> GetNecessityServicesNameList()
        {
            return new List<string>()
            {
                "graphicsperfsvc",
                "themes",
                "wsearch",
                "xblauthmanager",
                "displayenhancementservice"
            };
        }

        private void ChangedList(List<ListViewServicesObject> list)
        {
            LV_Service.ItemsSource = list;

            // 5. name으로 오름차순 정렬
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(LV_Service.ItemsSource);
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription("serviceName", ListSortDirection.Ascending));
            view.Refresh();

        }
        private void RefreshAllList()
        {
            allServicesList = GetAllServices();
            necessityList = GetNecessityServices();

            if (Checkbox_Necessity.IsChecked == true)
                ChangedList(necessityList);
            else
                ChangedList(allServicesList);
        }
        private void ControlExit()
        {
            var window = Window.GetWindow(this);
            window.Close();
        }
        #endregion

        #region MouseRight ContextMenu Handler
        private void MenuItem1_RefreshHandler(object sender, RoutedEventArgs e)
        {
            if (Checkbox_Necessity.IsChecked == true)
                ChangedList(necessityList);
            else
                ChangedList(allServicesList);

        }

        private void MenuItem2_StatusToggleHandler(object sender, RoutedEventArgs e)
        {

            var menuItem = sender as MenuItem;
            var selectListviewItem = LV_Service.SelectedItem as ListViewServicesObject;

            bool result = true;
            switch (menuItem.Header)
            {
                case "시작":
                    if (Util.Service.StatusChange(selectListviewItem.serviceName, ServiceControllerStatus.Running) == false)
                        result = false;
                    break;
                case "중지":
                    if (Util.Service.StatusChange(selectListviewItem.serviceName, ServiceControllerStatus.Stopped) == false)
                        result = false;
                    break;
                default:
                    break;
            }

            if (result == false)
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "프로그램보다 더 높은 권한을 가진 서비스입니다.\r\n변경에 실패했습니다.");
            else
                RefreshAllList();
        }

        private void MenuItem3_StartTypeToggleHandler(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var selectListviewItem = LV_Service.SelectedItem as ListViewServicesObject;

            bool result = true;
            switch (menuItem.Header)
            {
                case "자동":
                    if (Util.Service.ConfigChange(selectListviewItem.serviceName, DllImport.Service.ServiceStartupType.Automatic) == false)
                        result = false;
                    break;
                case "수동":
                    if (Util.Service.ConfigChange(selectListviewItem.serviceName, DllImport.Service.ServiceStartupType.Manual) == false)
                        result = false;
                    break;
                case "사용 안함":
                    if (Util.Service.ConfigChange(selectListviewItem.serviceName, DllImport.Service.ServiceStartupType.Disabled) == false)
                        result = false;
                    break;
                default:
                    break;
            }

            if (result == false)
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "프로그램보다 더 높은 권한을 가진 서비스입니다.\r\n변경에 실패했습니다.");
            else
                RefreshAllList();
        }

        private void MenuItem4_DeleteHandler(object sender, RoutedEventArgs e)
        {
            var SelectLvItem = LV_Service.SelectedItem as ListViewServicesObject;

            if (msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.YesNo, "삭제하면 복구할 수 없습니다.\r\n정말로 삭제하시겠습니까?") == (int)MsgEnum.Result.Yes)
            {
                if (Util.Service.Remove(SelectLvItem.serviceName) == false)
                    msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "프로그램보다 더 높은 권한을 가진 서비스입니다.\r\n변경에 실패했습니다.");
                else
                    RefreshAllList();
            }
        }
        #endregion

        #region ListViewHandler
        private void ListViewColumnClickHandler(object sender, RoutedEventArgs e)
        {
            //헤더 눌렀을때 정렬
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    ICollectionView view = (ICollectionView)CollectionViewSource.GetDefaultView(LV_Service.ItemsSource);

                    //헤더 이름에 공백이 있으면 Sort가 안됨. 그래서 Replace로 지워줌.
                    string ReplaceContent = headerClicked.Content.ToString();
                    ReplaceContent = ReplaceContent.Replace(" ", "");

                    if (view != null)
                    {
                        if ((view.SortDescriptions.Count > 0) && (view.SortDescriptions[0].Direction == ListSortDirection.Ascending))
                        {
                            view.SortDescriptions.Clear();
                            view.SortDescriptions.Add(new SortDescription(ReplaceContent, ListSortDirection.Descending));
                        }
                        else
                        {
                            view.SortDescriptions.Clear();
                            view.SortDescriptions.Add(new SortDescription(ReplaceContent, ListSortDirection.Ascending));
                        }
                    }
                    view.Refresh();
                }
            }
        }


        #endregion

        #region Handler
        private void Checkbox_Necessity_Checked(object sender, RoutedEventArgs e)
        {
            ChangedList(necessityList);
        }

        private void Checkbox_Necessity_UnChecked(object sender, RoutedEventArgs e)
        {
            ChangedList(allServicesList);
        }
        #endregion

    }
}
