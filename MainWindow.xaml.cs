using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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

    public class MainViewModel : INotifyPropertyChanged, IDataErrorInfo
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
            StartFinderCommand = new RelayCommand<string>(x =>
            {
                if (string.IsNullOrWhiteSpace(Dir)) return;
                Result.Clear();
                ItemsTotal = 0;
                ItemsProcessed = 0;
                _finder = new Finder();
                _finder.Dir = Dir;
                _finder.FileMask = FileMask;
                _finder.FindText = FindText;
                _finder.ExcludeFileMask = ExcludeMask;
                _finder.InAllDirectories = AllDirSearch;
                if (x == "Find")
                {
                    _finder.IsReplace = false;
                }
                else if (x == "Replace")
                {
                    _finder.IsReplace = true;
                    _finder.ReplaceText = ReplaceText;
                }

                var context = TaskScheduler.FromCurrentSynchronizationContext();
                ButtonsUnlock = false;
                Task.Run(() => _finder.GetFiles()).ContinueWith(res =>
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        ItemsTotal = res.Result.Length;
                    });
                    foreach (var item in _finder.Find(res.Result))
                    {
                        if (item != null)
                        {
                            App.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Result.Add(item);
                                ItemsProcessed++;
                            });
                        }
                        else
                        {
                            App.Current.Dispatcher.Invoke((Action)delegate
                            {
                                ItemsProcessed++;
                            });
                        }
                    }
                }).ContinueWith(_ =>
                {
                    ButtonsUnlock = true;
                }, context);
            });
            
        }

        public ICommand StartFinderCommand { get; }
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

        private bool _buttonsUnlock = false;
        public bool ButtonsUnlock
        {
            get => _buttonsUnlock;

            set
            {
                if (_buttonsUnlock == value) return;

                _buttonsUnlock = value;
                OnPropertyChanged(nameof(ButtonsUnlock));
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

                try
                {
                    Directory.GetFiles(Dir, FileMask, SearchOption.TopDirectoryOnly);
                    ButtonsUnlock = true;
                    _errorsDictionary[nameof(Dir)] = null;
                }
                catch (Exception ex)
                {
                    ButtonsUnlock = false;
                    _errorsDictionary[nameof(Dir)] = ex.Message;
                }

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

        private int _itemsTotal = 0;
        public int ItemsTotal
        {
            get => _itemsTotal;

            set
            {
                if (_itemsTotal == value) return;
                _itemsTotal = value;
                OnPropertyChanged(nameof(ItemsTotal));
                OnPropertyChanged(nameof(ProcessText));
            }
        }

        private int _itemsProcessed = 0;
        public int ItemsProcessed
        {
            get => _itemsProcessed;

            set
            {
                if (_itemsProcessed == value) return;
                _itemsProcessed = value;
                OnPropertyChanged(nameof(ItemsProcessed));
                OnPropertyChanged(nameof(ProcessText));
            }
        }

        public string ProcessText
        {
            get => ItemsProcessed + " of "+ ItemsTotal;
        }

        public ObservableCollection<ResultItem> Result { get; }

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Dictionary<string, string> _errorsDictionary = new Dictionary<string, string>();

        public string Error
        {
            get
            {
                return string.Join(Environment.NewLine,
                    _errorsDictionary.Values.Where(x => string.IsNullOrWhiteSpace(x) == false));
            }
        }
        public string this[string equat] => _errorsDictionary.TryGetValue(equat, out var error) ? error : null;

    }

}
