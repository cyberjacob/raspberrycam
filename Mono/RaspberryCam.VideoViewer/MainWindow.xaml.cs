using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using RaspberryCam.Clients;

namespace RaspberryCam.VideoViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TcpVideoClient videoClient;
        private Timer timer;

        public MainWindow()
        {
            InitializeComponent();

            videoClient = new TcpVideoClient("home.romcyber.com", 8080);
            timer = new Timer();
            timer.Elapsed += DisplayPicture;
        }

        public delegate void MyDelegate();

        private void DisplayPicture(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke((MyDelegate)delegate ()
                {
                    var data = videoClient.GetVideoFrame(60);

                    var bitmapImage = LoadImage(data);
                    ImageViewer.Source = bitmapImage;
                }, 
                DispatcherPriority.Normal);

           
        }

        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            videoClient.StartVideoStreaming(new PictureSize(320, 240));

            timer.Interval = 500;
            timer.Start();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            videoClient.StopVideoStreaming();
        }
    }
}
