using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Windows;

namespace MMT.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string UniquePipeName = "{f8e9f7c5-f50d-4213-97ea-461716d60dac}";

        private const string UniqueMutexName = "{4c028672-7a51-456f-8a72-7772169bde4f}";

        private Mutex _mutex;

        private void AppOnStartup(object sender, StartupEventArgs e)
        {
            _mutex = new Mutex(true, UniqueMutexName, out bool isOwned);

            GC.KeepAlive(_mutex);

            if (isOwned)
            {
                FirstAppOnStart(e);
            }
            else
            {
                NotFirstAppOnStart(e);
            }
        }

        private void OpenLink(string link)
        {
            var chooseAppForLink = new ChooseProfileForLink(link);
            chooseAppForLink.Owner = null;
            chooseAppForLink.Show();
        }

        private void FirstAppOnStart(StartupEventArgs e)
        {
            var thread = new Thread(
                () =>
                {
                    using var server = new NamedPipeServerStream(UniquePipeName, PipeDirection.In);
                    while (true)
                    {
                        server.WaitForConnection();
                        using var reader = new StreamReader(server);
                        var link = reader.ReadToEnd();
                        Current.Dispatcher.Invoke(delegate
                        {
                            OpenLink(link);
                        });
                    }
                })
            {
                IsBackground = true
            };

            thread.Start();
                
            if (e.Args.Length > 0)
            {
                OpenLink(e.Args[0]);
            }
        }

        private void NotFirstAppOnStart(StartupEventArgs e)
        {
            using (var client = new NamedPipeClientStream(".", UniquePipeName, PipeDirection.Out))
            {
                client.Connect();
                using var writer = new StreamWriter(client);
                writer.WriteLine(e.Args[0]);
            }
            
            Shutdown();
        }
    }
}