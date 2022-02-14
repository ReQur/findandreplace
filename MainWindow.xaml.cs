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
using CommunityToolkit.Mvvm.Input;

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

            BrowseCommand = new RelayCommand<string>(x =>
            {
                var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
                dialog.Description = "Choose directory for search";
                dialog.UseDescriptionForTitle = true;
                var result = dialog.ShowDialog();
                if (result == true)
                {
                    Dir = dialog.SelectedPath;
                }
            });
            FindCommand = new RelayCommand<string>(x =>
            {
                if (string.IsNullOrWhiteSpace(Dir)) return;
                Result.Clear();
                _finder = new Finder();
                _finder.Dir = Dir;
                _finder.FileMask = FileMask;
                _finder.FindText = FindText;
                _finder.ExcludeFileMask = ExcludeMask;
                _finder.InAllDirectories = AllDirSearch;
                _finder.IsReplace = false;
                
                var res = _finder.Find();
                foreach (var item in res.Items)
                {
                    Result.Add(item);
                }
            });
            ReplaceCommand = new RelayCommand<string>(x =>
            {
                if(string.IsNullOrWhiteSpace(Dir)) return;
                Result.Clear();
                _finder = new Finder();
                _finder.Dir = Dir;
                _finder.FileMask = FileMask;
                _finder.FindText = FindText;
                _finder.ExcludeFileMask = ExcludeMask;
                _finder.InAllDirectories = AllDirSearch;
                _finder.IsReplace = true;
                _finder.ReplaceText = ReplaceText;


                var res = _finder.Find();
                foreach (var item in res.Items)
                {
                    Result.Add(item);
                }
            });
        }
        public ICommand FindCommand { get; }
        public ICommand ReplaceCommand { get; }
        public ICommand BrowseCommand { get; }


        private string _fileMask = "*.*";
        public string FileMask
        {
            get => _fileMask;

            set
            {
                if (_fileMask == value) return;

                _fileMask = value;
                OnPropertyChanged(nameof(FileMask));
            }
        }

        private string _excludeMask = "*.dll, *.exe";
        public string ExcludeMask
        {
            get => _excludeMask;

            set
            {
                if (_excludeMask == value) return;

                _excludeMask = value;
                OnPropertyChanged(nameof(ExcludeMask));
            }
        }

        private bool _allDirSearch = true;
        public bool AllDirSearch
        {
            get =>_allDirSearch;

            set
            {
                if (_allDirSearch == value) return;

                _allDirSearch = value;
                OnPropertyChanged(nameof(AllDirSearch));
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
                OnPropertyChanged(nameof(Dir));
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
                OnPropertyChanged(nameof(FindText));
            }
        }
        private string _replaceText = "";

        public string ReplaceText
        {
            get => _replaceText;

            set
            {
                if (_replaceText == value) return;

                _replaceText = value;
                OnPropertyChanged(nameof(ReplaceText));
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
