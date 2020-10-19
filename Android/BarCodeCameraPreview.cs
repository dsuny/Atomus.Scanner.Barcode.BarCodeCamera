using System;
using System.Collections.Generic;

using Android.Content;
using Android.Gms.Vision;
using Android.Gms.Vision.Barcodes;
using Android.Runtime;
using Android.Views;

namespace Atomus.Scanner.Barcode
{
    public class BarCodeCameraPreview : ViewGroup, ICore
    {
        #region Declare
        private static CameraSource cameraSource;
        private BarcodeDetector barcodeDetector;
        private SurfaceView surfaceView;
        private IWindowManager windowManager;
        public event Action<List<BarcodeResult>> OnDetected;
        private BarCodeDetectorProcessor barcodeDetectorProcessor;
        #endregion

        #region Property
        public int Orientation { get; private set; } = 0;
        public bool IsDisposed { get; private set; } = false;
        #endregion

        #region INIT
        public BarCodeCameraPreview(Context context) : base(context)
        {
            windowManager = Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();

            barcodeDetector = new BarcodeDetector.Builder(context)
               .SetBarcodeFormats(BarCodeCamera.BarcodeFormats)
               .Build();

            cameraSource = new CameraSource
                .Builder(context, barcodeDetector)
                //.SetRequestedPreviewSize(1024, 768)
                .SetAutoFocusEnabled(true)
                .SetRequestedFps(30.0f)
                .Build();

            surfaceView = new SurfaceView(context);
            surfaceView.Holder.AddCallback(new BarcodeSurfaceHolderCallback(cameraSource, surfaceView, this));
            AddView(surfaceView);

            barcodeDetectorProcessor = new BarCodeDetectorProcessor(context, cameraSource);
            barcodeDetectorProcessor.OnDetected += DetectProcessor_OnDetected;
            barcodeDetector.SetProcessor(barcodeDetectorProcessor);
        }
        #endregion

        #region Event
        private void DetectProcessor_OnDetected(List<BarcodeResult> obj)
        {
            OnDetected?.Invoke(obj);
        }

        protected override void Dispose(bool disposing)
        {
            this.IsDisposed = true;
            base.Dispose(disposing);
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            try
            {
                surfaceView.Measure(MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly)
                                    , MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly));
                surfaceView.Layout(0, 0, r - l, r - l);

                var childWidth = r - l;
                var childHeight = b - t;

                for (int i = 0; i < ChildCount; ++i)
                {
                    GetChildAt(i).Layout(0, 0, childWidth, childHeight);
                }

                SetOrientation();
            }
            catch (Exception ex)
            {
                Console.WriteLine("BarCodeCameraPreview Exception : {0}", ex.ToString());
            }
        }
        #endregion

        #region Etc
        public void SetOrientation()
        {
            switch (windowManager.DefaultDisplay.Rotation)
            {
                case SurfaceOrientation.Rotation0:
                    this.Orientation = 0;
                    //camera?.SetDisplayOrientation(90);
                    break;

                case SurfaceOrientation.Rotation90:
                    this.Orientation = 90;
                    //camera?.SetDisplayOrientation(0);
                    break;

                case SurfaceOrientation.Rotation180:
                    this.Orientation = 180;
                    //camera?.SetDisplayOrientation(270);
                    break;

                case SurfaceOrientation.Rotation270:
                    this.Orientation = 270;
                    //camera?.SetDisplayOrientation(180);
                    break;
            }

            for (int i = 0; i < Android.Hardware.Camera.NumberOfCameras; i++)
            {
                Android.Hardware.Camera.CameraInfo cameraInfo = new Android.Hardware.Camera.CameraInfo();
                Android.Hardware.Camera.GetCameraInfo(i, cameraInfo);

                //https://developer.android.com/reference/android/hardware/Camera.html#setDisplayOrientation%28int%29
                if (cameraInfo.Facing == Android.Hardware.CameraFacing.Back)
                //if (cameraInfo.Facing == Android.Hardware.Camera.CameraInfo.CameraFacingBack)
                {
                    this.Orientation = (cameraInfo.Orientation - this.Orientation + 360) % 360;
                }
            }
        }
        public static Android.Hardware.Camera GetCamera()
        {
            Java.Lang.Object javaHero;
            Java.Lang.Reflect.Field[] fields;

            if (cameraSource == null)
                return null;

            try
            {
                javaHero = cameraSource.JavaCast<Java.Lang.Object>();
                fields = javaHero.Class.GetDeclaredFields();

                foreach (Java.Lang.Reflect.Field field in fields)
                {
                    if (field.Type.CanonicalName.Equals("android.hardware.camera", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine(field.Type.CanonicalName);

                        field.Accessible = true;

                        return (Android.Hardware.Camera)field.Get(javaHero);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("BarCodeCameraPreview Exception : {0}", ex.ToString());
                return null;
            }

            return null;
        }
        #endregion
    }
}