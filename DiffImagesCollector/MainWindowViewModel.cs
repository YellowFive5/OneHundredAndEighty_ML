#region Usings

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

#endregion

namespace DiffImagesCollector
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Bitmap

        public BitmapImage BackgroundBitmap
        {
            get => backgroundBitmap;
            set
            {
                backgroundBitmap = value;
                OnPropertyChanged(nameof(BackgroundBitmap));
            }
        }

        private BitmapImage backgroundBitmap;

        public BitmapImage ThrowBitmap
        {
            get => throwBitmap;
            set
            {
                throwBitmap = value;
                OnPropertyChanged(nameof(ThrowBitmap));
            }
        }

        private BitmapImage throwBitmap;

        public BitmapImage ThrowProcessedBitmap
        {
            get => throwProcessedBitmap;
            set
            {
                throwProcessedBitmap = value;
                OnPropertyChanged(nameof(ThrowProcessedBitmap));
            }
        }

        private BitmapImage throwProcessedBitmap;

        public BitmapImage DiffBitmap
        {
            get => diffBitmap;
            set
            {
                diffBitmap = value;
                OnPropertyChanged(nameof(DiffBitmap));
            }
        }

        private BitmapImage diffBitmap;

        #endregion

        #region Images

        private Image<Bgr, byte> backgroundRawImage;
        private Image<Gray, byte> backgroundProcessedImage;
        private Image<Bgr, byte> throwRawImage;
        private Image<Gray, byte> throwProcessedImage;
        private Image<Gray, byte> diffImage;

        #endregion

        public void TakeBackgroundCapture()
        {
            var videoCapture = new VideoCapture(0, VideoCapture.API.DShow);
            videoCapture.SetCaptureProperty(CapProp.FrameWidth, 1280);
            videoCapture.SetCaptureProperty(CapProp.FrameHeight, 720);
            // videoCapture.SetCaptureProperty(CapProp.FrameWidth, 1920);
            // videoCapture.SetCaptureProperty(CapProp.FrameHeight, 1080);
            backgroundRawImage = videoCapture.QueryFrame()
                                             .ToImage<Bgr, byte>();

            backgroundProcessedImage = backgroundRawImage.Clone().Convert<Gray, byte>();

            BackgroundBitmap = ImageToBitmapImage(backgroundRawImage);

            ThrowBitmap = BackgroundBitmap;
            
            ThrowProcessedBitmap = ImageToBitmapImage(backgroundProcessedImage);

            var blackBlank = backgroundProcessedImage.Clone();
            blackBlank.SetValue(new Bgr(Color.Black).MCvScalar);
            DiffBitmap = ImageToBitmapImage(blackBlank);

            videoCapture.Dispose();
        }

        public void TakeCapture()
        {
            var videoCapture = new VideoCapture(0, VideoCapture.API.DShow);
            videoCapture.SetCaptureProperty(CapProp.FrameWidth, 1280);
            videoCapture.SetCaptureProperty(CapProp.FrameHeight, 720);
            // videoCapture.SetCaptureProperty(CapProp.FrameWidth, 1920);
            // videoCapture.SetCaptureProperty(CapProp.FrameHeight, 1080);
            throwRawImage = videoCapture.QueryFrame()
                                        .ToImage<Bgr, byte>();

            backgroundRawImage = throwRawImage ?? backgroundRawImage;
            BackgroundBitmap = ThrowBitmap ?? BackgroundBitmap;
            
            ThrowBitmap = ImageToBitmapImage(throwRawImage);

            throwProcessedImage = throwRawImage.Clone().Convert<Gray, byte>();
            ThrowProcessedBitmap = ImageToBitmapImage(throwProcessedImage);

            diffImage = throwProcessedImage.AbsDiff(backgroundProcessedImage);
            DiffBitmap = ImageToBitmapImage(diffImage);

            backgroundProcessedImage = throwProcessedImage.Clone();

            videoCapture.Dispose();
        }

        private BitmapImage ImageToBitmapImage(Image<Bgr, byte> image)
        {
            var imageToSave = new BitmapImage();

            using (var stream = new MemoryStream())
            {
                image.ToBitmap().Save(stream, ImageFormat.Bmp);
                imageToSave.BeginInit();
                imageToSave.StreamSource = new MemoryStream(stream.ToArray());
                imageToSave.EndInit();
            }

            return imageToSave;
        }

        private BitmapImage ImageToBitmapImage(Image<Gray, byte> image)
        {
            var imageToSave = new BitmapImage();

            using (var stream = new MemoryStream())
            {
                image.ToBitmap().Save(stream, ImageFormat.Bmp);
                imageToSave.BeginInit();
                imageToSave.StreamSource = new MemoryStream(stream.ToArray());
                imageToSave.EndInit();
            }

            return imageToSave;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}