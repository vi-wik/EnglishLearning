using EnglishLearning.Business.Manager;

namespace EnglishLearning.App
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            DataFileManager.Init();

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Exception exception = e?.ExceptionObject as Exception;

                if(exception!=null)
                {
                    LogManager.LogException(exception);
                }                
            };

            MainPage = new AppShell();
        }
    }
}
