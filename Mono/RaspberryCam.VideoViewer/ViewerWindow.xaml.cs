using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AForge.Video.VFW;
using RaspberryCam.Clients;
using System.Linq;

namespace RaspberryCam.VideoViewer
{
    /// <summary>
    /// Interaction logic for ViewerWindow.xaml
    /// </summary>
    public partial class ViewerWindow : Window
    {
        private readonly string captureAviFile;
        private readonly TcpVideoClient videoClient;
        private bool streaming = false;
        private int imageWidth;
        private int imageHeight;
        private int compressionRate;
        private int grabWidth;
        private int grabHeight;
        private AVIWriter aviWriter;

        public ViewerWindow(string serverHostIp, int serverPort, string captureAviFile)
        {
            this.captureAviFile = captureAviFile;
            InitializeComponent();

            videoClient = new TcpVideoClient(serverHostIp, serverPort);

            grabWidth = 320;
            grabHeight = 240;

            imageWidth = grabWidth * 2;
            imageHeight = grabHeight*2;

            Height = imageHeight + 210;

            aviWriter = new AVIWriter();
            
            ImageViewer.Width = imageWidth;
            ImageViewer.Height = imageHeight;
            compressionRate = 30;
            CompressionLabel.Content = string.Format("{0}%", compressionRate);
            CompressionSlider.Value = compressionRate;

            StartVideoButton.Visibility = Visibility.Visible;
            StopVideoButton.Visibility = Visibility.Hidden;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
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

        private readonly List<KeyValuePair<DateTime, int>> frameSizes = new List<KeyValuePair<DateTime, int>>();
        private DateTime lastSpeedRefresh = DateTime.MinValue;

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            videoClient.StartVideoStreaming(new PictureSize(imageWidth/2, imageHeight/2), 20);

            StartVideoButton.Visibility = Visibility.Hidden;
            StopVideoButton.Visibility = Visibility.Visible;

            aviWriter.Open(captureAviFile, grabWidth, grabHeight);

            streaming = true;

            Task.Factory.StartNew(() =>
                {
                    while (streaming)
                    {
                        var data = videoClient.GetVideoFrame(compressionRate);
                        Dispatcher.BeginInvoke((UiDelegate)delegate
                        {
                            frameSizes.Add(new KeyValuePair<DateTime, int>(DateTime.UtcNow, data.Length));

                            if (lastSpeedRefresh < DateTime.UtcNow - TimeSpan.FromSeconds(1))
                            {
                                Task.Factory.StartNew(() => DisplaySpeed(data));
                            }

                            var bitmapImage = LoadImage(data);

                            aviWriter.FrameRate = 20;
                            if (streaming)
                                aviWriter.AddFrame(BitmapImage2Bitmap(bitmapImage));

                            ImageViewer.Source = bitmapImage;

                            UpdateLayout();
                        }, DispatcherPriority.Normal);
                    }

                    videoClient.StopVideoStreaming();
                    Dispatcher.BeginInvoke((UiDelegate)delegate
                        {
                            StartVideoButton.Visibility = Visibility.Visible;
                            StopVideoButton.Visibility = Visibility.Hidden;
                        }, DispatcherPriority.Normal);

                    aviWriter.Close();
                });
        }

        private void DisplaySpeed(byte[] data)
        {
            var lastFrameSizes =
                frameSizes.Where(f => f.Key >= DateTime.UtcNow - TimeSpan.FromSeconds(1)).
                    ToList();
            var totalBytesPerSecond = lastFrameSizes.Sum(f => f.Value);

            Dispatcher.BeginInvoke((UiDelegate)delegate
            {
                SpeedLabel.Content =
                    string.Format("{0} Kb per frame / {1} Kb per second / {2} frames / second",
                                  data.Length / 1024, totalBytesPerSecond / 1024,
                                  lastFrameSizes.Count);
            }, DispatcherPriority.Normal);

            frameSizes.RemoveAll(kv => kv.Key < DateTime.UtcNow - TimeSpan.FromSeconds(1));

            lastSpeedRefresh = DateTime.UtcNow;
        }

        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                var bitmap = new Bitmap(outStream);

                // return bitmap; <-- leads to problems, stream is closed/closing ...
                return new Bitmap(bitmap);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            streaming = false;
        }

        private void CompressionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            compressionRate = (int) CompressionSlider.Value;
            CompressionLabel.Content = string.Format("{0}%", compressionRate);
        }
    }
}
