using System.Windows;

namespace CrashHandler
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string[] Args
        {
            get; private set;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Args = e.Args;
            base.OnStartup(e); 
        }
    }
}
