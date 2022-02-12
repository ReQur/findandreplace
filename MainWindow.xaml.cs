using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using GalaSoft.MvvmLight.CommandWpf;

namespace findandreplace
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private Finder _finder;
        public MainViewModel()
        {
            Result = new ObservableCollection<ResultItem>();

            FindCommand = new RelayCommand<string>(x =>
            {
                Result.Clear();
                _finder = new Finder();
                _finder.Dir = Dir;
                _finder.FileMask = FileMask;
                _finder.FindText = FindText;
                _finder.ExcludeFileMask = ExcludeMask;
                
                var res = _finder.Find();
                foreach (var item in res.Items)
                {
                    Result.Add(item);
                }
            });
        }
        public ICommand FindCommand { get; }


        private string _fileMask = "*.*";
        public string FileMask
        {
            get => _fileMask;

            set
            {
                if (_fileMask == value) return;

                _fileMask = value;
                OnPropertyChanged(nameof(_fileMask));
            }
        }

        private string _exludeMask = "*.dll, *.exe";
        public string ExcludeMask
        {
            get => _exludeMask;

            set
            {
                if (_exludeMask == value) return;

                _exludeMask = value;
                OnPropertyChanged(nameof(_exludeMask));
            }
        }

        private string _dir = "";
        public string Dir
        {
            get => _dir;

            set
            {
                if (_dir == value) return;

                _dir = value;
                OnPropertyChanged(nameof(_dir));
            }
        }

        private string _findText = "";

        public string FindText
        {
            get => _findText;

            set
            {
                if (_findText == value) return;

                _findText = value;
                OnPropertyChanged(nameof(_findText));
            }
        }

        public ObservableCollection<ResultItem> Result { get; }

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}
