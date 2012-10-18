using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
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
        
        public MainWindow()
        {
            InitializeComponent();

            videoClient = new TcpVideoClient("home.romcyber.com", 8080);
            
            ImageViewer.Width = 640;
            ImageViewer.Height = 480;
        }

        public delegate void UiDelegate();

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

        private bool streaming = false;

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            videoClient.StartVideoStreaming(new PictureSize(640, 480));

            streaming = true;

            Task.Factory.StartNew(() =>
                {
                    while (streaming)
                    {
                        var data = videoClient.GetVideoFrame(60);
                        

                        Dispatcher.BeginInvoke((UiDelegate)delegate()
                        {
                            var bitmapImage = LoadImage(data);
                            ImageViewer.Source = bitmapImage;

                            UpdateLayout();
                        }, DispatcherPriority.Normal);
                    }
                });
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            streaming = false;
            videoClient.StopVideoStreaming();
        }
    }
}
