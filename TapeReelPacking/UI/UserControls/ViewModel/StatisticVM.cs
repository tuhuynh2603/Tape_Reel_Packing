using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using TapeReelPacking.Source.Define;

namespace TapeReelPacking.UI.UserControls.ViewModel
{
    public class StatisticVM : BaseVM
    {

        private ObservableCollection<SummaryTemplateVM> _listSummary;

        public ObservableCollection<SummaryTemplateVM> listSummary
        {
            get => _listSummary;
            set
            {
                _listSummary = value;
                OnPropertyChanged(nameof(listSummary));
            }
        }

        public delegate void UpdateValueStatisticDelegate(int result, int nTrack);
        public static UpdateValueStatisticDelegate updateValueStatisticDelegate;

        public delegate void ClearStatisticDelegate();
        public static ClearStatisticDelegate clearStatisticDelegate;

        public StatisticVM()
        {
            listSummary = new ObservableCollection<SummaryTemplateVM>();

            listSummary.Add(new SummaryTemplateVM() { nameSummary = "Checked", valueSummary_Camera1 = 0, valueSummary_Camera2 = 0, color = Brushes.WhiteSmoke });
            listSummary.Add(new SummaryTemplateVM() { nameSummary = "Passed", valueSummary_Camera1 = 0, valueSummary_Camera2 = 0, color = Brushes.Yellow });
            listSummary.Add(new SummaryTemplateVM() { nameSummary = "Failed", valueSummary_Camera1 = 0, valueSummary_Camera2 = 0, color = Brushes.Red });
            listSummary.Add(new SummaryTemplateVM() { nameSummary = "Yield %", valueSummary_Camera1 = 0, valueSummary_Camera2 = 0, color = Brushes.Lime });

            updateValueStatisticDelegate = UpdateValueStatistic;
            clearStatisticDelegate = ClearStatistic;
        }

        public void UpdateValueStatistic(int result, int nTrack)
        {
            if (result == -(int)ERROR_CODE.NOT_INSPECTED)
                return;
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
        }
        public void ClearStatistic()
        {
            foreach (var def in listSummary)
            {
                def.valueSummary_Camera1 = 0;
                def.valueSummary_Camera2 = 0;
            }
        }



    }
}
