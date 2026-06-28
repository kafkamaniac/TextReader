using System.Configuration;
using System.Data;
using System.Windows;

namespace TextReader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }


}
