using System;
using System.Collections.Generic;
using System.Threading;
using Android.Content;
using Android.OS;
using Xamarin.Forms;
using Android.Gms.Vision;

namespace Atomus.Scanner.Barcode
{
    public class BarCodeDetectorProcessor : Java.Lang.Object, Detector.IProcessor, ICore
    {
        #region Declare
        private Android.Gms.Vision.CameraSource cameraSource;
        public event Action<List<BarcodeResult>> OnDetected;
        private bool isScanning = true;
        private Context context;
        private Vibrator vibrator;
        #endregion

        #region Property
        #endregion

        #region INIT
        public BarCodeDetectorProcessor(Context context, Android.Gms.Vision.CameraSource cameraSource)
        {
            this.context = context;
            this.cameraSource = cameraSource;
            this.vibrator = (this.context.GetSystemService(Context.VibratorService) as Vibrator);
        }
        #endregion

        #region IO
        void Detector.IProcessor.ReceiveDetections(Detector.Detections detections)
        {
            Rectangle formsBounds;
            Rectangle scanRectangle;
            Rectangle barcodeRectangle;

            if (detections.DetectedItems.Size() < 1 || !this.isScanning)
                return;

            try
            {
                this.isScanning = false;
                List<BarcodeResult> barcodeResults = new List<BarcodeResult>();
                for (int i = 0; i < detections.DetectedItems.Size(); i++)
                {
                    Android.Gms.Vision.Barcodes.Barcode barcode = (detections.DetectedItems.ValueAt(i) as Android.Gms.Vision.Barcodes.Barcode);

                    formsBounds = Xamarin.Forms.Application.Current.MainPage.Bounds;

                    //10~90;
                    //30~60;
                    //barcode.BoundingBox.Width() * 0.1; X
                    //barcode.BoundingBox.Width() * 0.8; Width

                    //barcode.BoundingBox.Height() * 0.3; Y
                    //barcode.BoundingBox.Height() * 0.3; Height

                    //int a = this.cameraSource.PreviewSize.Width;
                    //int b = this.cameraSource.PreviewSize.Height;

                    if (formsBounds.Height > formsBounds.Width)
                        scanRectangle = new Rectangle(this.cameraSource.PreviewSize.Height * 0.1, this.cameraSource.PreviewSize.Width * 0.3
                                                    , this.cameraSource.PreviewSize.Height * 0.8, this.cameraSource.PreviewSize.Width * 0.3);
                    else
                        scanRectangle = new Rectangle(this.cameraSource.PreviewSize.Width * 0.1, this.cameraSource.PreviewSize.Height * 0.3
                                                    , this.cameraSource.PreviewSize.Width * 0.8, this.cameraSource.PreviewSize.Height * 0.3);

                    barcodeRectangle = new Rectangle(barcode.BoundingBox.Left, barcode.BoundingBox.Top, barcode.BoundingBox.Width(), barcode.BoundingBox.Height());

                    if (scanRectangle.Contains(barcodeRectangle))
                        barcodeResults.Add(this.BarcodeResultConvert(barcode));
                }

                if (barcodeResults.Count > 0)
                {
                    OnDetected?.Invoke(barcodeResults);

                    vibrator.Vibrate(1000);

                    Thread.Sleep(3000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("BarCodeDetectorProcessor Exception : {0}", ex.ToString());
            }
            finally
            {
                this.isScanning = true;
            }
        }

        void Detector.IProcessor.Release()
        {
        }
        #endregion

        #region Etc
        private BarcodeResult BarcodeResultConvert(Android.Gms.Vision.Barcodes.Barcode barcode)
        {
            return new BarcodeResult
            {
                Rectangle = new Rectangle(barcode.BoundingBox.Left, barcode.BoundingBox.Top, barcode.BoundingBox.Width(), barcode.BoundingBox.Height())
                ,
                WiFi = barcode.Wifi == null ? null : new WiFi()
                {
                    EncryptionType = barcode.Wifi.EncryptionType
                                                                    ,
                    Password = barcode.Wifi.Password
                                                                    ,
                    Ssid = barcode.Wifi.Ssid
                }
                ,
                SmsValue = barcode.Sms == null ? null : new SmsValue()
                {
                    Message = barcode.Sms.Message
                                                                    ,
                    PhoneNumber = barcode.Sms.PhoneNumber
                }
                ,
                RawValue = barcode.RawValue
                ,
                PhoneValue = barcode.Phone == null ? null : new PhoneValue()
                {
                    Number = barcode.Phone.Number
                                                                    ,
                    Type = barcode.Phone.Type
                }
                ,
                GeoPointValue = barcode.GeoPoint == null ? null :
                                                            new GeoPointValue()
                                                            {
                                                                Lat = barcode.GeoPoint.Lat
                                                             ,
                                                                Lng = barcode.GeoPoint.Lng
                                                            },
                EmailValue = barcode.Email == null ? null :
                                                            new EmailValue()
                                                            {
                                                                Address = barcode.Email.Address
                                                             ,
                                                                Body = barcode.Email.Body
                                                             ,
                                                                Subject = barcode.Email.Subject
                                                             ,
                                                                Type = barcode.Email.Type
                                                            },
                UrlBookmark = barcode.Url == null ? null :
                                                            new UrlBookmark()
                                                            {
                                                                Title = barcode.Url.Title
                                                             ,
                                                                Url = barcode.Url.Url
                                                            },
                DisplayValue = barcode.DisplayValue,
                CornerPoints = barcode.CornerPoints == null ? null :
                                                            this.PointConvert(barcode.CornerPoints),
                ContactInfoValue = barcode.ContactInfo == null ? null :
                                                            new ContactInfoValue()
                                                            {
                                                                Addresses = this.AddressConvert(barcode.ContactInfo.Addresses)
                                                             ,
                                                                Emails = this.EmailsConvert(barcode.ContactInfo.Emails)
                                                             ,
                                                                PersonName = new PersonName()
                                                                {
                                                                    First = barcode.ContactInfo.Name.First
                                                                                                ,
                                                                    FormattedName = barcode.ContactInfo.Name.FormattedName
                                                                                                ,
                                                                    Last = barcode.ContactInfo.Name.Last
                                                                                                ,
                                                                    Middle = barcode.ContactInfo.Name.Middle
                                                                                                ,
                                                                    Prefix = barcode.ContactInfo.Name.Prefix
                                                                                                ,
                                                                    Pronunciation = barcode.ContactInfo.Name.Pronunciation
                                                                                                ,
                                                                    Suffix = barcode.ContactInfo.Name.Suffix
                                                                }
                                                             ,
                                                                Organization = barcode.ContactInfo.Organization
                                                             ,
                                                                PhoneValues = this.PhoneValuesConvert(barcode.ContactInfo.Phones)
                                                             ,
                                                                Title = barcode.ContactInfo.Title
                                                             ,
                                                                Urls = barcode.ContactInfo.Urls
                                                            },
                CalendarEventValue = barcode.CalendarEvent == null ? null :
                                                            new CalendarEventValue()
                                                            {
                                                                Description = barcode.CalendarEvent.Description
                                                                                        ,
                                                                End = new CalendarDateTime()
                                                                {
                                                                    Day = barcode.CalendarEvent.End.Day
                                                                                                                        ,
                                                                    Hours = barcode.CalendarEvent.End.Hours
                                                                                                                        ,
                                                                    IsUtc = barcode.CalendarEvent.End.IsUtc
                                                                                                                        ,
                                                                    Minutes = barcode.CalendarEvent.End.Minutes
                                                                                                                        ,
                                                                    Month = barcode.CalendarEvent.End.Month
                                                                                                                        ,
                                                                    RawValue = barcode.CalendarEvent.End.RawValue
                                                                                                                        ,
                                                                    Seconds = barcode.CalendarEvent.End.Seconds
                                                                                                                        ,
                                                                    Year = barcode.CalendarEvent.End.Year
                                                                }
                                                                                        ,
                                                                Location = barcode.CalendarEvent.Location
                                                                                        ,
                                                                Organizer = barcode.CalendarEvent.Organizer
                                                                                        ,
                                                                Start = new CalendarDateTime()
                                                                {
                                                                    Day = barcode.CalendarEvent.Start.Day
                                                                                                                        ,
                                                                    Hours = barcode.CalendarEvent.Start.Hours
                                                                                                                        ,
                                                                    IsUtc = barcode.CalendarEvent.Start.IsUtc
                                                                                                                        ,
                                                                    Minutes = barcode.CalendarEvent.Start.Minutes
                                                                                                                        ,
                                                                    Month = barcode.CalendarEvent.Start.Month
                                                                                                                        ,
                                                                    RawValue = barcode.CalendarEvent.Start.RawValue
                                                                                                                        ,
                                                                    Seconds = barcode.CalendarEvent.Start.Seconds
                                                                                                                        ,
                                                                    Year = barcode.CalendarEvent.Start.Year
                                                                }
                                                                                        ,
                                                                Status = barcode.CalendarEvent.Status
                                                                                        ,
                                                                Summary = barcode.CalendarEvent.Summary
                                                            },
                BarcodeFormat = (BarcodeFormat)Enum.Parse(typeof(BarcodeFormat), barcode.Format.ToString()),
                BarcodeValueFormat = (BarcodeValueFormat)Enum.Parse(typeof(BarcodeValueFormat), barcode.ValueFormat.ToString()),
                DriverLicenseValue = barcode.DriverLicense == null ? null :
                                                            new DriverLicenseValue()
                                                            {
                                                                AddressCity = barcode.DriverLicense.AddressCity
                                                            ,
                                                                AddressState = barcode.DriverLicense.AddressState
                                                            ,
                                                                AddressStreet = barcode.DriverLicense.AddressStreet
                                                            ,
                                                                AddressZip = barcode.DriverLicense.AddressZip
                                                            ,
                                                                BirthDate = barcode.DriverLicense.BirthDate
                                                            ,
                                                                DocumentType = barcode.DriverLicense.DocumentType
                                                            ,
                                                                ExpiryDate = barcode.DriverLicense.ExpiryDate
                                                            ,
                                                                FirstName = barcode.DriverLicense.FirstName
                                                            ,
                                                                Gender = barcode.DriverLicense.Gender
                                                            ,
                                                                IssueDate = barcode.DriverLicense.IssueDate
                                                            ,
                                                                IssuingCountry = barcode.DriverLicense.IssuingCountry
                                                            ,
                                                                LastName = barcode.DriverLicense.LastName
                                                            ,
                                                                LicenseNumber = barcode.DriverLicense.LicenseNumber
                                                            ,
                                                                MiddleName = barcode.DriverLicense.MiddleName
                                                            },
            };
        }
        private IList<Xamarin.Forms.Point> PointConvert(IList<Android.Graphics.Point> points)
        {
            List<Xamarin.Forms.Point> points1;

            points1 = new List<Xamarin.Forms.Point>();

            foreach (var item in points)
                points1.Add(new Xamarin.Forms.Point() { X = item.X, Y = item.Y });

            return points1;
        }
        private IList<Address> AddressConvert(IList<Android.Gms.Vision.Barcodes.Barcode.Address> addresses)
        {
            List<Address> addresses1;

            addresses1 = new List<Address>();

            foreach (var item in addresses)
                addresses1.Add(new Address() { AddressLines = item.AddressLines, Type = item.Type });

            return addresses1;
        }
        private IList<EmailValue> EmailsConvert(IList<Android.Gms.Vision.Barcodes.Barcode.EmailValue> emailValues)
        {
            List<EmailValue> emailValues1;

            emailValues1 = new List<EmailValue>();

            foreach (var item in emailValues)
                emailValues1.Add(new EmailValue() { Address = item.Address, Body = item.Body, Subject = item.Subject, Type = item.Type });

            return emailValues1;
        }
        private IList<PhoneValue> PhoneValuesConvert(IList<Android.Gms.Vision.Barcodes.Barcode.PhoneValue> phoneValues)
        {
            List<PhoneValue> phoneValues1;

            phoneValues1 = new List<PhoneValue>();

            foreach (var item in phoneValues)
                phoneValues1.Add(new PhoneValue() { Number = item.Number, Type = item.Type });

            return phoneValues1;
        }
        #endregion
    }
}