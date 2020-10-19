using System;

using Android.Gms.Vision;
using Android.Graphics;
using Android.Runtime;
using Android.Views;

namespace Atomus.Scanner.Barcode
{
    public class BarcodeSurfaceHolderCallback : Java.Lang.Object, ISurfaceHolderCallback, ICore
    {
        #region Declare
        private SurfaceView surfaceView;
        private CameraSource cameraSource;
        private BarCodeCameraPreview cameraPreview;
        #endregion

        #region Property
        #endregion

        #region INIT
        public BarcodeSurfaceHolderCallback(CameraSource cameraSource, SurfaceView surfaceView, BarCodeCameraPreview cameraPreview)
        {
            this.cameraSource = cameraSource;
            this.surfaceView = surfaceView;
            this.cameraPreview = cameraPreview;
        }
        #endregion

        #region Event
        void ISurfaceHolderCallback.SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
            float previewSizeRatio;
            float scaleX;
            float scaleY;
            float surfaceViewRatio;

            if (this.cameraSource == null)
                return;

            previewSizeRatio = 0;

            try
            {
                switch (this.cameraPreview.Orientation)
                {
                    case 0:
                    case 180:
                        previewSizeRatio = (float)this.cameraSource.PreviewSize.Width / (float)this.cameraSource.PreviewSize.Height;
                        break;
                    case 90:
                    case 270:
                        previewSizeRatio = (float)this.cameraSource.PreviewSize.Height / (float)this.cameraSource.PreviewSize.Width;
                        break;
                }

                surfaceViewRatio = (float)this.surfaceView.Width / (float)this.surfaceView.Height;

                if (previewSizeRatio < surfaceViewRatio)
                {
                    scaleX = 1;
                    scaleY = surfaceViewRatio / previewSizeRatio;
                }
                else
                {
                    scaleX = previewSizeRatio / surfaceViewRatio;
                    scaleY = 1;
                }

                this.surfaceView.ScaleX = scaleX;
                this.surfaceView.ScaleY = scaleY;
            }
            catch (Exception ex)
            {
                Console.WriteLine("BarcodeSurfaceHolderCallback Exception : {0}", ex.ToString());
            }
        }

        void ISurfaceHolderCallback.SurfaceCreated(ISurfaceHolder holder)
        {
            try
            {
                this.cameraSource.Start(this.surfaceView.Holder);

                BarCodeCameraPreview.GetCamera().SetDisplayOrientation(this.cameraPreview.Orientation);
            }
            catch (Exception ex)
            {
                Console.WriteLine("BarcodeSurfaceHolderCallback Exception : {0}", ex.ToString());
            }
        }

        void ISurfaceHolderCallback.SurfaceDestroyed(ISurfaceHolder holder)
        {
            this.cameraSource.Stop();
        }
        #endregion

        #region Etc
        #endregion
    }
}