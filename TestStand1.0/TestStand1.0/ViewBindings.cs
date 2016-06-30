using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using OxyPlot;
using OxyPlot.Series;

namespace TestStand1._0
{
    class ViewBindings:INotifyPropertyChanged
    {
        private LineSeries _lineSeries;
        private LineSeries _lineSeries1;
        private PlotModel _myModel = new PlotModel();
        public PlotModel MyModel
        {
            get
            {
                return _myModel;
            }
            set
            {
                _myModel = value;
                if(PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("MyModel"));
            }

        }
        public ViewBindings()
        {
            _lineSeries = new LineSeries();
            _lineSeries1 = new LineSeries();
            _myModel.Series.Add(_lineSeries);
            _myModel.Series.Add(_lineSeries1);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void AddPoint(DataPoint pt, DataPoint pt1)
        {
            _lineSeries.Points.Add(pt);
            _lineSeries1.Points.Add(pt1);
            _myModel.InvalidatePlot(true);
            MyModel = _myModel;
        }
    }
}
