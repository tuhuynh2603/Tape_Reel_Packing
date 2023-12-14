using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;

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
            listSummary.Add(new StatisticData() { nameSummary = "Checked", valueSummary_Camera1 = 0, valueSummary_Camera2 = 0, color = Brushes.WhiteSmoke });
            listSummary.Add(new StatisticData() { nameSummary = "Passed", valueSummary_Camera1 = 0, valueSummary_Camera2 = 0,  color = Brushes.Lime });
            listSummary.Add(new StatisticData() { nameSummary = "Failed", valueSummary_Camera1 = 0, valueSummary_Camera2 = 0, color = Brushes.Red });
            listSummary.Add(new StatisticData() { nameSummary = "Yield %", valueSummary_Camera1 = 0, valueSummary_Camera2 = 0, color = Brushes.Lime });

            lboxStatistic.ItemsSource = listSummary;
        }
        public void UpdateValueStatistic(int result, int nTrack)
        {
            if (nTrack == 0)
            {
                listSummary[0].valueSummary_Camera1 += 1;
                if (result == 0)
                    listSummary[1].valueSummary_Camera1 += 1;
                else
                    listSummary[2].valueSummary_Camera1 += 1;

                listSummary[3].valueSummary_Camera1 = Math.Round((listSummary[1].valueSummary_Camera1 / listSummary[0].valueSummary_Camera1) * 100, 2);

            }
            else
            {
                listSummary[0].valueSummary_Camera2 += 1;
                if (result == 0)
                    listSummary[1].valueSummary_Camera2 += 1;
                else
                    listSummary[2].valueSummary_Camera2 += 1;

                listSummary[3].valueSummary_Camera2 = Math.Round((listSummary[1].valueSummary_Camera2 / listSummary[0].valueSummary_Camera2) * 100, 2);

            }
            lboxStatistic.ItemsSource = null; lboxStatistic.ItemsSource = listSummary;
        }
        public void ClearStatistic(int nTrackID)
        {
            foreach (var def in listSummary)
            {
                def.valueSummary_Camera1 = 0;
                def.valueSummary_Camera2 = 0;
            }
            lboxStatistic.ItemsSource = null;
            lboxStatistic.ItemsSource = listSummary;
        }
    }

    public class StatisticData
    {
        public string nameSummary { get; set; }
        public double valueSummary_Camera1 { get; set; }
        public double valueSummary_Camera2 { get; set; }

        public Brush color { get; set; }
    }
}
