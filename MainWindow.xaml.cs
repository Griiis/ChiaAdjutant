using ChiaAdjutant.Data;
using System;
using System.Media;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ChiaAdjutant
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TextBlock _totalTryBlock;
        private TextBlock _filterPassBlock;
        private TextBlock _proofFoundBlock;

        private TextBlock _smallestTimeBlock;
        private TextBlock _longestTimeBlock;
        private TextBlock _deltaTimeBlock;

        //---Вкладка консольного вывода
        private RichTextBox _consoleTextBox;
        private CheckBox _showFailedCheckbox;
        private CheckBox _autoScroll;

        //---Вкладка настроек
        private CheckBox _soundChekBox;



        private Timer _updateTimer = new Timer(10);
        private Timer _soundPlayDelay = new Timer(1000);
        private AsyncDebugReader _asyncDebugReader;

        private int _totalTryStat = 0;
        private int _filterPassedStat = 0;
        private int _proofFoundStat = 0;

        private double _smallestTimeStat = 1000;
        private double _longestTimeStat = 0;
        private double _timeDeltaStat = 0;

        private bool _soundAvailable;

        public MainWindow()
        {
            InitializeComponent();
            InitializeStatBlock();


            _consoleTextBox = (RichTextBox)FindName("Console_Out");

            _showFailedCheckbox = (CheckBox)FindName("ShowFail");
            _autoScroll = (CheckBox)FindName("AutoScroll");

            _soundChekBox = (CheckBox)FindName("Settings_Sound");

            _updateTimer.Elapsed += UpdateTimer_Elapsed;
            _updateTimer.AutoReset = true;
            _updateTimer.Start();

            _soundPlayDelay.Start();
            _soundPlayDelay.Elapsed += SoundPlayDelay_Elapsed;

            _asyncDebugReader = new AsyncDebugReader();
        }

        private void SoundPlayDelay_Elapsed(object sender, ElapsedEventArgs e)
        {
            _soundAvailable = true;
        }

        private void InitializeStatBlock()
        {
            _totalTryBlock = (TextBlock)FindName("TotalTryStat");
            _filterPassBlock = (TextBlock)FindName("FilterPassStat");
            _proofFoundBlock = (TextBlock)FindName("ProofFoundStat");

            _smallestTimeBlock = (TextBlock)FindName("SmallestTimeStat");
            _longestTimeBlock = (TextBlock)FindName("LongestTimeStat");
            _deltaTimeBlock = (TextBlock)FindName("TimeDeltaStat");
        }

        private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    MainProgramm();
                });
            }
            catch (Exception ex)
            {

            }
        }

        public void MainProgramm()
        {
            ConsoleRuntime();
        }

        private  void ConsoleRuntime()
        {
            PlotPassEventData passEventData = _asyncDebugReader.GetNextPassEvent();
            if (passEventData != null)
            {
                UpdateStatistic(passEventData);
                SoundNotification(passEventData);
                if (_showFailedCheckbox.IsChecked == false && !passEventData.IsAnyPass)
                {
                    
                    return;
                }

                Run run = new Run(passEventData.ToString());
                Paragraph paragraph = new Paragraph(run);

                if (!passEventData.IsAnyPass)
                {
                    paragraph.Foreground = Brushes.MediumVioletRed;
                }

                if (passEventData.IsAnyPass)
                {
                    paragraph.Foreground = Brushes.DarkOrange;
                }

                if (passEventData.IsWinPass)
                {
                    paragraph.Foreground = Brushes.Green;
                }

                _consoleTextBox.Document.Blocks.Add(paragraph);
                if(_autoScroll.IsChecked == true)
                {
                    _consoleTextBox.ScrollToEnd();
                }
            }
        }

        private void UpdateStatistic(PlotPassEventData eventData)
        {
            _totalTryStat++;
            if(eventData.IsAnyPass)
            {
                _filterPassedStat++;
            }
            if (eventData.IsWinPass)
            {
                _proofFoundStat++;
            }

            if(_smallestTimeStat > eventData.PassTime)
            {
                _smallestTimeStat = eventData.PassTime;
            }

            if (_longestTimeStat < eventData.PassTime)
            {
                _longestTimeStat = eventData.PassTime;
            }

            _timeDeltaStat = _longestTimeStat - _smallestTimeStat;

            _totalTryBlock.Text = _totalTryStat.ToString();
            _filterPassBlock.Text = _filterPassedStat.ToString();
            _proofFoundBlock.Text = _proofFoundStat.ToString();

            _smallestTimeBlock.Text = _smallestTimeStat.ToString();

            if(_longestTimeStat > 10)
            {
                _longestTimeBlock.Foreground = Brushes.Red;
            }
            else if(_longestTimeStat > 5)
            {
                _longestTimeBlock.Foreground = Brushes.OrangeRed;
            }
            else if(_longestTimeStat > 1)
            {
                _longestTimeBlock.Foreground = Brushes.Yellow;
            }
            else
            {
                _longestTimeBlock.Foreground = Brushes.White;
            }

            _longestTimeBlock.Text = _longestTimeStat.ToString();           

            _deltaTimeBlock.Text = _timeDeltaStat.ToString();
        }

        private void SoundNotification(PlotPassEventData eventData)
        {
            if(_soundChekBox.IsChecked == false || _soundAvailable == false)
            {
                _soundPlayDelay.Start();
                return;
            }
            SoundPlayer soundPlayer;
            if (eventData.IsWinPass)
            {
                soundPlayer = new SoundPlayer(Resource1.filter_pass_proof);
            }
            else if (eventData.IsAnyPass)
            {
                soundPlayer = new SoundPlayer(Resource1.filter_pass_no_proof);
            }
            else
            {
                soundPlayer = new SoundPlayer(Resource1.plot_missed);
            }

            soundPlayer.Play();
            _soundAvailable = false;
            _soundPlayDelay.Start();
        }
    }
}
