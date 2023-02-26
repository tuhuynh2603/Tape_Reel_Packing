using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Magnus_WPF_1.UI.UserControls.View
{
    /// <summary>
    /// Interaction logic for StatisticView.xaml
    /// </summary>
    public partial class StatisticView : UserControl
    {
        public ObservableCollection<StatisticData> listSummary = new ObservableCollection<StatisticData>();
        public StatisticView()
        {
            InitializeComponent();
            listSummary.Add(new StatisticData() { nameSummary = "Checked", valueSummary = 0, color = Brushes.WhiteSmoke });
            listSummary.Add(new StatisticData() { nameSummary = "Passed", valueSummary = 0, color = Brushes.Lime });
            listSummary.Add(new StatisticData() { nameSummary = "Failed", valueSummary = 0, color = Brushes.Red });
            listSummary.Add(new StatisticData() { nameSummary = "Yield %", valueSummary = 0, color = Brushes.Lime });

            lboxStatistic.ItemsSource = listSummary;
        }
        public void UpdateValueStatistic(int result)
        {
            listSummary[0].valueSummary += 1;
            if (result == 0)
                listSummary[1].valueSummary += 1;
            else
                listSummary[2].valueSummary += 1;
            listSummary[3].valueSummary = Math.Round((listSummary[1].valueSummary / listSummary[0].valueSummary) * 100, 2);
            lboxStatistic.ItemsSource = null; lboxStatistic.ItemsSource = listSummary;
        }
        public void ClearStatistic()
        {
            foreach (var def in listSummary)
            {
                def.valueSummary = 0;
            }
            lboxStatistic.ItemsSource = null;
            lboxStatistic.ItemsSource = listSummary;
        }
    }

    public class StatisticData
    {
        public string nameSummary { get; set; }
        public double valueSummary { get; set; }
        public Brush color { get; set; }
    }
}
