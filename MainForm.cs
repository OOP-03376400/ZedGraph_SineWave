﻿using System;
using System.Drawing;
using System.Windows.Forms;
using ZedGraph;

namespace SineWave
{
    public static class ZedGraphExtensions
    {
        public static bool IsRising(this IPointList points)
        {
            if (points.Count < 2)
                return false;

            return points[points.Count - 1].Y > points[points.Count - 2].Y;
        }

        public static PointPair Last(this IPointList points)
        {
            return points[points.Count - 1];
        }
    }

    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private GraphPane _myPane;
        private RollingPointPairList _points;
        private RollingPointPairList _peaks;

        private double _sinPart;
        private double _timePart;
        private XDate _currentTime;
        private int _i;

        private bool _scrolling = true;

        private void Form1_Load(object sender, EventArgs e)
        {
            _currentTime = new XDate(DateTime.Now);
            _timePart = 1 / 5000.0;
            _sinPart = 2 * Math.PI / 5000.0;
            _i = 0;

            zedGraphControl1.IsShowHScrollBar = true;
            zedGraphControl1.IsAutoScrollRange = true;

            _myPane = zedGraphControl1.GraphPane;
            _myPane.Title.Text = "Live Data";
            _myPane.XAxis.Title.Text = "Time";
            _myPane.YAxis.Title.Text = "Value";

            _myPane.YAxis.Scale.Min = -1.2;
            _myPane.YAxis.Scale.Max = 1.2;
            _myPane.YAxis.Scale.MajorStep = 1;
            _myPane.YAxis.Scale.MinorStep = 1;

            _myPane.XAxis.Type = AxisType.Date;
            _myPane.XAxis.Scale.Format = "mm:ss:fff";
            _myPane.XAxis.Scale.FontSpec.Angle = 60;
            _myPane.XAxis.Scale.FontSpec.Size = 12;
            _myPane.XAxis.Scale.MajorUnit = DateUnit.Millisecond;
            _myPane.XAxis.Scale.MajorStep = 500;
            _myPane.XAxis.Scale.MinorUnit = DateUnit.Millisecond;
            _myPane.XAxis.Scale.MinorStep = 250;

            _peaks = new RollingPointPairList(15000);
            var peaksCurve = _myPane.AddCurve("Peak Points", _peaks, Color.Red, SymbolType.Circle);
            peaksCurve.Line.IsVisible = false;
            peaksCurve.Symbol.Fill.Color = Color.Red;
            peaksCurve.Symbol.Fill.Type = FillType.Solid;
            peaksCurve.Symbol.Size = 10;

            _points = new RollingPointPairList(15000);
            var pointsCurve = _myPane.AddCurve("Sine Wave", _points, Color.Blue, SymbolType.None);
            pointsCurve.Line.Width = 3;

        }

        private bool _rising = false;

        private void generateData_Tick(object sender, EventArgs e)
        {
            if (_scrolling)
            {
                for (var j = 0; j < 50; j++)
                {
                    _i++;
                    _currentTime.AddSeconds(_timePart);
                    _points.Add(_currentTime, Math.Sin(_sinPart * _i));

                    if (_points.IsRising())
                        _rising = true;

                    if (_rising && !_points.IsRising())
                    {
                        _peaks.Add(_points.Last());
                        _rising = false;
                    }
                }
            }
        }

        private void graphUpdate_Tick(object sender, EventArgs e)
        {
            if (_scrolling)
            {
                _myPane.XAxis.Scale.Max = new XDate(_currentTime.DateTime.AddMilliseconds(0));
                _myPane.XAxis.Scale.Min = new XDate(_currentTime.DateTime.AddMilliseconds(-1000));
            }
            _myPane.XAxis.Scale.BaseTic = new XDate(new XDate(_myPane.XAxis.Scale.Min).DateTime.Floor(new TimeSpan(0, 0, 0, 0, (int)_myPane.XAxis.Scale.MajorStep)));
            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
        }

        private void zedGraphControl1_ScrollEvent(object sender, ScrollEventArgs e)
        {
            //_scrolling = true; 
        }

        private void resumeScrolling_Click(object sender, EventArgs e)
        {
            if (!_scrolling)
            {
                resumeScrolling.Text = @"Pause Scrolling";
                _scrolling = true;
            }
            else
            {
                resumeScrolling.Text = @"Resume Scrolling";
                _scrolling = false;
            }
        }

    }

    public static class DateExtensions
    {
        public static DateTime Round(this DateTime date, TimeSpan span)
        {
            var ticks = (date.Ticks + span.Ticks / 2 + 1) / span.Ticks;
            return new DateTime(ticks * span.Ticks);
        }
        public static DateTime Floor(this DateTime date, TimeSpan span)
        {
            var ticks = date.Ticks / span.Ticks;
            return new DateTime(ticks * span.Ticks);
        }
        public static DateTime Ceil(this DateTime date, TimeSpan span)
        {
            var ticks = (date.Ticks + span.Ticks - 1) / span.Ticks;
            return new DateTime(ticks * span.Ticks);
        }
    }

}
