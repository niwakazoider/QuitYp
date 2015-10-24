using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace QuitYpApplication
{
    class MainClass
    {
        private QuitServer server;
        private Twitter twitter;
        private MainWindow window;
        private Thread searchThread;
        private Thread serverThread;
        private Thread trialThread;
        private Application app;

        [STAThread]
        static void Main(string[] args)
        {
            new MainClass();
        }

        public MainClass()
        {
            window = new MainWindow();
            window.OnClickEvent += OnPinCode_ButtonClickEvent;
            window.Closing += new CancelEventHandler(MainWindow_Closing);
            window.Show();

            Task.Run(() => {
                trialThread = new Thread(TrialTimer);
                trialThread.Start();

                server = new QuitServer();
                serverThread = new Thread(server.Run);
                serverThread.Start();

                twitter = new Twitter();
                twitter.OnRequestEvent += OnRequest_TwitterEvent;
                twitter.OnIndexTextEvent += OnIndexText_TwitterEvent;
                twitter.OnAuthCompleteEvent += OnAuthComplete_TwitterEvent;
                twitter.OnAuthErrorEvent += OnAuthError_TwitterEvent;
                twitter.Init();
            });

            app = new Application();
            app.Run(window);
        }

        private void TrialTimer()
        {
            try
            {
                Thread.Sleep(10 * 60 * 1000);
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    AppExit();
                }));

            }
            catch (Exception)
            {

            }
        }

        private void OnAuthError_TwitterEvent()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MessageBox.Show(window, "認証に失敗しました。");
                twitter.ClearToken();
            }));
        }

        private void OnAuthComplete_TwitterEvent()
        {
            if (searchThread == null)
            {
                searchThread = new Thread(twitter.Run);
                searchThread.Start();
            }
        }

        private void OnPinCode_ButtonClickEvent()
        {
            twitter.Pin(window.textBox.Text);
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                window.PinGrid.Visibility = Visibility.Hidden;
            }));
        }

        private void OnRequest_TwitterEvent()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                window.PinGrid.Visibility = Visibility.Visible;
            }));
        }

        private void OnIndexText_TwitterEvent(string txt, string raw)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                window.rawDataBox.Text = raw;
            }));
            server.SetChannelData(txt);
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            AppExit();
        }

        private void AppExit()
        {
            if (serverThread != null)
            {
                serverThread.Interrupt();
            }
            if (searchThread != null)
            {
                searchThread.Interrupt();
            }
            if (trialThread != null)
            {
                trialThread.Interrupt();
            }
            if (server != null)
            {
                server.Stop();
            }
            Application.Current.Shutdown();
        }
    }
}
