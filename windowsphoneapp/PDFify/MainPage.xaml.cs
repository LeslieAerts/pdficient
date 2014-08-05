using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PDFify.Resources;
using System.IO.IsolatedStorage;
using System.IO;
using Windows.Storage;
using System.Windows.Controls.Primitives;
using System.Text;

namespace PDFify
{
    public partial class MainPage : PhoneApplicationPage
    {
        PhotoChooserTask photoChooserTask;
        CameraCaptureTask cameraCaptureTask;
        BitmapImage currentImage;
        Stream currentImageSourceStream;
        String currentImageLocation;
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            currentImage = new BitmapImage();
            currentImageLocation = "";
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();   
        }


        private void btCamera_ManipulationStarted_1(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            cameraCaptureTask = new CameraCaptureTask();
            cameraCaptureTask.Completed += new EventHandler<PhotoResult>(cameraCaptureTask_Completed);
            cameraCaptureTask.Show();
        }

        private void btGallery_ManipulationStarted_1(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            photoChooserTask = new PhotoChooserTask();
            photoChooserTask.Completed += new EventHandler<PhotoResult>(photoChooserTask_Completed);
            photoChooserTask.Show();
        }

        void cameraCaptureTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                MessageBox.Show(e.OriginalFileName);

                SetCurrentPhoto(e);
            }
        }

        private void SetCurrentPhoto(PhotoResult e)
        {
            currentImageLocation = e.OriginalFileName;
            currentImageSourceStream = e.ChosenPhoto;
            currentImage.SetSource(e.ChosenPhoto);
            imgPreview.Source = currentImage;
        }

        void photoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                //MessageBox.Show(e.ChosenPhoto.Length.ToString());

                //Code to display the photo on the page in an image control named myImage.
                SetCurrentPhoto(e);
            }
        }

        private async void btExport_ManipulationStarted(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {

            if (currentImage == null && currentImageLocation.Length < 1)
            {
                MessageBox.Show("Please select a valid image");
                return;
            }

            string pdfName = "export.pdf";

            //Create a PDF folder if it does not already exist
            StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
            var pdfFolder = await local.CreateFolderAsync("Generated PDFs", CreationCollisionOption.OpenIfExists);
            var pdfFile = await pdfFolder.CreateFileAsync(pdfName, CreationCollisionOption.ReplaceExisting);

            var stream = await System.IO.WindowsRuntimeStorageExtensions.OpenStreamForWriteAsync(pdfFile);
            StreamWriter writer = new StreamWriter(stream);
            List<long> xrefs = new List<long>();

            //Writing the actual PDF
            writer.WriteLine("%");
            writer.WriteLine("%PDF-1.5");
            writer.WriteLine("%");

            //1
            writer.Flush();
            stream.Flush();
            xrefs.Add(stream.Position);
            writer.WriteLine("");
            writer.WriteLine("%The PDF catalog");
            writer.WriteLine(xrefs.Count + " 0 obj");
            writer.WriteLine("<<");
            writer.WriteLine("  /Type /Catalog");
            writer.WriteLine("  /Pages 2 0 R");
            writer.WriteLine(">>");
            writer.WriteLine("endobj");

            writer.Flush();
            stream.Flush();
            xrefs.Add(stream.Position);

            //2
            //Declare the page list
            writer.WriteLine("");
            writer.WriteLine("%The page list");
            writer.WriteLine(xrefs.Count + " 0 obj");
            writer.WriteLine("<<");
            writer.WriteLine("  /Type /Pages");
            writer.WriteLine("  /Kids [3 0 R]");
            //writer.WriteLine("  /Resources 3 0 R");
            writer.WriteLine("/Count 1");
            writer.WriteLine(">>");
            writer.WriteLine("endobj");

            writer.Flush();
            stream.Flush();
            xrefs.Add(stream.Position);


            //3
            //Declare the page object
            writer.WriteLine("");
            writer.WriteLine("%Actual page, with references to all objects it uses (image and resources and whatnot)");
            writer.WriteLine(xrefs.Count + " 0 obj");
            writer.WriteLine("<<");
            writer.WriteLine("  /Type /Page");
            writer.WriteLine("  /Parent 2 0 R");
            writer.WriteLine("  /Resources 4 0 R");
            writer.WriteLine("  /MediaBox [0 0 " + currentImage.PixelWidth + " " + currentImage.PixelHeight + "]");
            //612 792
            writer.WriteLine("  /Contents 6 0 R");
            writer.WriteLine(">>");
            writer.WriteLine("endobj");

            writer.Flush();
            stream.Flush();
            xrefs.Add(stream.Position);

            //4
            //Declare the stuff this PDF has (Or something?)
            writer.WriteLine("");
            writer.WriteLine("%Resources this pdf uses");
            writer.WriteLine(xrefs.Count + " 0 obj");
            writer.WriteLine("<<");
            writer.WriteLine("/ProcSet [/PDF /ImageC]");
            writer.WriteLine("/XObject << Im1 5 0 R >>");
            writer.WriteLine(">>");
            writer.WriteLine("endobj");

            writer.Flush();
            stream.Flush();
            xrefs.Add(stream.Position);

            //5
            writer.WriteLine("");
            writer.WriteLine("%image declaration");
            writer.WriteLine(xrefs.Count + " 0 obj");
            writer.WriteLine("<<");
            writer.WriteLine("  /Type /XObject"); //Specify the XOBject
            writer.WriteLine("  /Subtype /Image"); //It is an image
            writer.WriteLine("  /Width " + currentImage.PixelWidth); //The dimensions of the image
            writer.WriteLine("  /Height " + currentImage.PixelHeight);
            writer.WriteLine("   /ColorSpace /DeviceRGB");
            writer.WriteLine("  /BitsPerComponent 8");
            writer.WriteLine("  /Length 83183");
            //writer.WriteLine("  /Filter /DCTDecode");
            writer.WriteLine(">>");

            writer.Flush();
            stream.Flush();
            //xrefs.Add(stream.Position);

            //Writing the actual image stream
            writer.WriteLine("stream");

            byte[] data = new byte[8192];

            using (MemoryStream imageStream = new MemoryStream())
            {
                WriteableBitmap wBitmap = new WriteableBitmap(currentImage);
                wBitmap.SaveJpeg(imageStream, wBitmap.PixelWidth, wBitmap.PixelHeight, 0, 100);
                imageStream.Seek(0, SeekOrigin.Begin);
                //data = imageStream.ToArray();

                while (true)
                {

                    int length = imageStream.Read(data, 0, data.Length);
                    if (length != 0)
                    {
                        //writer.Write(data, 0, length);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            //BinaryWriter binWriter = new BinaryWriter(stream, Encoding.UTF8);

           // foreach (byte b in data)
           // {
            //    writer.Write(b);
//
            //}
            writer.Flush();
            // binWriter.Flush();
            writer.WriteLine("");
            writer.WriteLine("endstream");
            writer.WriteLine("endobj");

            writer.Flush();
            stream.Flush();
            xrefs.Add(stream.Position);

            //6
            writer.WriteLine("");
            writer.WriteLine("%the placing of the image");
            writer.WriteLine(xrefs.Count + " 0 obj");
            writer.WriteLine("  <<");
            writer.WriteLine("      /Length 8192");
            writer.WriteLine("  >>");
            writer.WriteLine("          stream");
            writer.WriteLine("          q");
            writer.WriteLine("              " + 256 + " 0 0 " + 256 + " 0 0 cm");
            writer.WriteLine("              /Im1 Do");
            writer.WriteLine("          Q");
            writer.WriteLine("          endstream");
            writer.WriteLine("endobj");
            writer.WriteLine(">>");

            writer.Flush();
            stream.Flush();
            //xrefs.Add(stream.Position);

            //Closing
            //PDF-XREFS. This part of the PDF is an index table into every object #1..#7 that we defined.

            long xref_pos = stream.Position;
            writer.WriteLine("xref");
            writer.WriteLine("1 " + (xrefs.Count));

            foreach (long xref in xrefs)
            {
                writer.WriteLine("{0:0000000000} {1:00000} n", xref, 0);
            }

            // PDF-TRAILER. Every PDF ends with this trailer.
            writer.WriteLine("trailer");
            writer.WriteLine("<<");
            writer.WriteLine("  /Size " + xrefs.Count);
            writer.WriteLine("  /Root 1 0 R");
            writer.WriteLine(">>");
            writer.WriteLine("startxref");
            writer.WriteLine(xref_pos);
            writer.WriteLine("%%EOF");

            writer.Dispose();

            MessageBox.Show("The file has been saved to storage under the name " + pdfName);

        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}