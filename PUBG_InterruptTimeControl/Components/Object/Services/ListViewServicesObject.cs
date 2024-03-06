using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PUBG_InterruptTimeControl.Components.Object.Services
{
    internal class ListViewServicesObject
    {
        public SolidColorBrush columnNameColorBox { get; set; }
        public string serviceName { get; set; }
        public string displayName { get; set; }
        public string startType { get; set; }
        public string status { get; set; }

        public ListViewServicesObject(in SolidColorBrush columnNameColorBox, in string serviceName, in string displayName, in string startType, in string status)
        {
            this.columnNameColorBox = (columnNameColorBox == null) ? new SolidColorBrush(Colors.Transparent) : columnNameColorBox;
            this.serviceName = serviceName;
            this.displayName = displayName;
            this.startType = startType.Equals("Automatic") ? "자동"
                            : startType.Equals("Manual") ? "수동"
                            : startType.Equals("Disabled") ? "사용 안함"
                            : "Unknown";

            this.status = status.Equals("Running") ? "실행 중"
                        : status.Equals("Stopped") ? "중지됨"
                        : "Unknown";
        }
    }
}
