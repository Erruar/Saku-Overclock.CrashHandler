using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;
using Path = System.IO.Path;

namespace CrashHandler
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int LinesToLoad = 50;
        private string[] allLines;
        private int currentLineIndex;
        private ScrollViewer scrollViewer;
        private TextBlock textBlock;

        public MainWindow()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            if (App.Args.Length == 0)
            {
                ShowLatestLogContent();
            }
            else
            {
                var message = string.Empty;
                var appName = string.Empty;
                var pathIcon = string.Empty;
                var theme = string.Empty;
                for (var i = 0; i < App.Args.Length; i++) 
                {
                    var line = App.Args[i];
                    if (!string.IsNullOrEmpty(line)) 
                    {
                        if (App.Args.Length > i + 1)
                        {
                            switch (App.Args[i + 1])
                            {
                                case "-message":
                                case "-msg":
                                    message = App.Args.Length > i + 2 ? App.Args[i + 2] : "";
                                    break;
                                case "-theme":
                                case "-thm":
                                    theme = App.Args.Length > i + 2 ? App.Args[i + 2] : "";
                                    break;
                                case "-path":
                                case "-pathicon":
                                case "-pathIcon":
                                case "-PathIcon":
                                case "-Icon":
                                case "-icon":
                                case "-iconPath":
                                case "-IconPath":
                                    pathIcon = App.Args.Length > i + 2 ? App.Args[i + 2] : "";
                                    break;
                                case "-appName":
                                case "-name":
                                case "-app":
                                case "-App":
                                case "-Name":
                                case "-AppName":
                                    appName = App.Args.Length > i + 2 ? App.Args[i + 2] : "/WindowIcon.ico";
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                ShowCrashAlert(App.Args.Contains("-message") || 
                               App.Args.Contains("-msg") ? 
                               message : 
                               (App.Args.Length > 0 ? 
                               App.Args[0] : 
                               ""),
                               pathIcon,
                               appName,
                               theme);
            }
        }

        private void ShowLatestLogContent()
        {
            var logsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SakuOverclock");
            if (Directory.Exists(logsPath))
            {
                var latestLogFile = Directory.GetFiles(logsPath, "Logs_*.txt")
                    .OrderByDescending(GetFileVersion)
                    .FirstOrDefault();

                if (latestLogFile != null)
                {
                    allLines = File.ReadAllLines(latestLogFile);
                    currentLineIndex = allLines.Length;
                    ShowContent(GetNextLines());
                    return;
                }
            }
            ShowContent("No logs found.");
        }

        private string GetNextLines()
        {
            if (allLines == null || currentLineIndex <= 0)
            {
                return string.Empty;
            }

            var linesToTake = Math.Min(LinesToLoad, currentLineIndex);
            currentLineIndex -= linesToTake;
            return string.Join(Environment.NewLine, allLines.Skip(currentLineIndex).Take(linesToTake));
        }

        private Version GetFileVersion(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var versionPart = fileName.Split('_')[1];
            var numericPart = new string(versionPart.Where(char.IsDigit).ToArray());
            return Version.TryParse(numericPart, out var version) ? version : new Version();
        }

        private void ShowCrashAlert(string message, string iconPath, string appName, string theme)
        {
            var whiteBrush = System.Windows.Media.Color.FromArgb(255, 255, 255, 255);
            var transpBlackBrush = System.Windows.Media.Color.FromArgb(178, 0, 0, 0);
            var blackBrush = System.Windows.Media.Color.FromArgb(255, 0, 0, 0);
            var blackTheme = false;
            if (theme.ToLowerInvariant() == "dark")
            {
                UpperBorder.Background = new SolidColorBrush() { Color = transpBlackBrush };
                LogoName.Foreground = new SolidColorBrush() { Color = whiteBrush };
                LogoDescript.Foreground = new SolidColorBrush() { Color = whiteBrush }; 
                Background = new LinearGradientBrush() { GradientStops = 
                    {
                        new GradientStop()
                        {
                            Color = System.Windows.Media.Color.FromRgb(24, 82, 78),
                            Offset = 0.1,
                        },
                        new GradientStop()
                        {
                            Color = System.Windows.Media.Color.FromRgb(0,122,203),
                            Offset = 0.5,
                        },
                        new GradientStop()
                        {
                            Color = System.Windows.Media.Color.FromRgb(120,36,114),
                            Offset = 1.1,
                        }
                    }};
                blackTheme = true;
            }
            var appArea = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Children =
                {
                    GetImage(iconPath),
                    new TextBlock(){ Foreground = new SolidColorBrush() { Color = blackTheme ? whiteBrush : blackBrush }, Text = appName + " unexpectedly shut down", FontFamily = new FontFamily("Cascadia Code"), HorizontalAlignment = HorizontalAlignment.Center, TextWrapping = TextWrapping.Wrap, FontSize = 23, Margin = new Thickness(0,10,0,0) },
                }
            };
            textBlock = new TextBlock
            {
                Margin = new Thickness(0,10,0,0),
                Text = message,
                FontFamily = new FontFamily("Cascadia Code"),
                FontSize = 13,
                Foreground = new SolidColorBrush() { Color = blackTheme ? whiteBrush : blackBrush },
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            scrollViewer = new ScrollViewer
            {
                Content = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                    Children =
                    {
                        appArea,
                        textBlock
                    } 
                },
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            scrollViewer.Loaded += (s, e) => scrollViewer.ScrollToEnd();

            ContentPresent.Children.Add(scrollViewer);
        }

        private System.Windows.Controls.Image GetImage(string iconPath)
        {
            try
            {
                return new System.Windows.Controls.Image()
                {
                    Margin = new Thickness(0, 10, 0, 0),
                    Width = 64,
                    Height = 64,
                    Source = new BitmapImage(
                   new Uri(
                       iconPath != string.Empty ? iconPath : "WindowIcon.ico")),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
            }
            catch 
            {
                return new System.Windows.Controls.Image();
            } 
        }
        private void ShowContent(string content)
        {
            textBlock = new TextBlock
            {
                Text = content,
                FontFamily = new FontFamily("Cascadia Code"),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap
            };

            scrollViewer = new ScrollViewer
            {
                Content = textBlock,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            scrollViewer.Loaded += (s, e) => scrollViewer.ScrollToEnd();

            ContentPresent.Children.Add(scrollViewer);
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (scrollViewer.VerticalOffset == 0 && currentLineIndex > 0)
            {
                var newContent = GetNextLines();
                textBlock.Text = newContent + Environment.NewLine + textBlock.Text;
                scrollViewer.ScrollToVerticalOffset(textBlock.ActualHeight/3);
            }
        } 
        private void closeApp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
        //Свернуть окно
        private void minimizeApp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                WindowState = WindowState.Minimized;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //-------------Закрытие, сворачивание окна-------------\\
        private void Ellipse_MouseEnter(object sender, MouseEventArgs e)
        {
            // ... Эллипсы закрытия и сворачивания окна
            var ellipse = sender as Ellipse;
            ellipse.Fill = Brushes.DarkGray;
            ellipse.Opacity = 0.5;
        }

        private void Ellipse_MouseLeave(object sender, MouseEventArgs e)
        {
            // ... Эллипсы закрытия и сворачивания окна
            var ellipse = sender as Ellipse;
            ellipse.Fill = Brushes.Transparent;
        }
        //-------------Движение окна-------------\\
        private void MovingWin(object sender, RoutedEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
