using Android.Content;
using Android.Graphics;
using Atomus.Scanner.Barcode;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

//[assembly: Dependency(typeof(BarCodeCamera))]
namespace Atomus.Scanner.Barcode
{
    public class BarCodeCamera : IBarCodeCamera
    {
        #region Declare
        Context context;
        private static BarCodeCameraPreview barCodeCameraPreview;
        private BarCodeCameraView barCodeCameraView;
        #endregion

        #region Property
        internal static bool IsTorch { get; set; } = false;
        public static Android.Gms.Vision.Barcodes.BarcodeFormat BarcodeFormats { get; set; }
            = Android.Gms.Vision.Barcodes.BarcodeFormat.Code128
            | Android.Gms.Vision.Barcodes.BarcodeFormat.Code39
            | Android.Gms.Vision.Barcodes.BarcodeFormat.Code93
            | Android.Gms.Vision.Barcodes.BarcodeFormat.Codabar
            | Android.Gms.Vision.Barcodes.BarcodeFormat.DataMatrix
            | Android.Gms.Vision.Barcodes.BarcodeFormat.Ean13
            | Android.Gms.Vision.Barcodes.BarcodeFormat.Ean8
            | Android.Gms.Vision.Barcodes.BarcodeFormat.Itf
            | Android.Gms.Vision.Barcodes.BarcodeFormat.QrCode
            | Android.Gms.Vision.Barcodes.BarcodeFormat.UpcA
            | Android.Gms.Vision.Barcodes.BarcodeFormat.UpcE
            | Android.Gms.Vision.Barcodes.BarcodeFormat.Pdf417
            | Android.Gms.Vision.Barcodes.BarcodeFormat.Aztec;
        #endregion

        #region INIT
        static BarCodeCamera()
        {
            DependencyService.Register<IBarCodeCamera, BarCodeCamera>();
        }
        public BarCodeCamera() { }
        public BarCodeCamera(Context context)
        {
            this.context = context;
        }
        #endregion

        #region Event
        object IBarCodeCamera.CreateViewGroup(object e)
        {
            if (barCodeCameraPreview == null || barCodeCameraPreview.IsDisposed || !barCodeCameraPreview.Context.Equals(this.context))
            {
                barCodeCameraPreview = new BarCodeCameraPreview(this.context);

                this.barCodeCameraView = (e as ElementChangedEventArgs<BarCodeCameraView>).NewElement;

                barCodeCameraPreview.OnDetected += (list) =>
                {
                    this.barCodeCameraView?.TriggerOnDetected(list);
                };

                this.barCodeCameraView.SizeChanged += CameraView_SizeChanged;

                return barCodeCameraPreview;
            }

            return null;
        }
        void IBarCodeCamera.SetSupportFormat(BarcodeFormat barcodeFormats)
        {
            BarcodeFormats = (Android.Gms.Vision.Barcodes.BarcodeFormat)Enum.Parse(typeof(Android.Gms.Vision.Barcodes.BarcodeFormat), barcodeFormats.ToString());

            if (BarcodeFormats == Android.Gms.Vision.Barcodes.BarcodeFormat.AllFormats)
                BarcodeFormats = Android.Gms.Vision.Barcodes.BarcodeFormat.Code128
                                                | Android.Gms.Vision.Barcodes.BarcodeFormat.Code39
                                                | Android.Gms.Vision.Barcodes.BarcodeFormat.Code93
                                                | Android.Gms.Vision.Barcodes.BarcodeFormat.Codabar
                                                | Android.Gms.Vision.Barcodes.BarcodeFormat.DataMatrix
                                                | Android.Gms.Vision.Barcodes.BarcodeFormat.Ean13
                                                | Android.Gms.Vision.Barcodes.BarcodeFormat.Ean8
                                                | Android.Gms.Vision.Barcodes.BarcodeFormat.Itf
                                                | Android.Gms.Vision.Barcodes.BarcodeFormat.QrCode
                                                | Android.Gms.Vision.Barcodes.BarcodeFormat.UpcA
                                                | Android.Gms.Vision.Barcodes.BarcodeFormat.UpcE
                                                | Android.Gms.Vision.Barcodes.BarcodeFormat.Pdf417
                                                | Android.Gms.Vision.Barcodes.BarcodeFormat.Aztec;
        }

        void IBarCodeCamera.ToggleFlashlight()
        {
            Android.Hardware.Camera Camera;
            Android.Hardware.Camera.Parameters parameters;

            try
            {
                Camera = BarCodeCameraPreview.GetCamera();
                parameters = Camera.GetParameters();

                //prams.focus.setFocusMode(Camera.Parameters.FOCUS_MODE_CONTINUOUS_PICTURE);

                if (!IsTorch)
                    parameters.FlashMode = Android.Hardware.Camera.Parameters.FlashModeTorch;
                else
                    parameters.FlashMode = Android.Hardware.Camera.Parameters.FlashModeOff;

                IsTorch = !IsTorch;
                Camera.SetParameters(parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine("BarCodeCamera Exception : {0}", ex.ToString());
            }
        }

        private void CameraView_SizeChanged(object sender, EventArgs e)
        {
        }
        #endregion
    }
}
