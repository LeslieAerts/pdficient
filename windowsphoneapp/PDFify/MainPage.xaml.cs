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
            StreamWriter writer = new StreamWriter(stream, System.Text.Encoding.UTF8);
            List<long> xrefs = new List<long>();

            //Writing the actual PDF
            writer.WriteLine("1");
            writer.WriteLine("%PDF-1.2");
            writer.WriteLine("%");
            writer.Flush();

            writer.Flush();
            stream.Flush();

            //#1: catalog - the overall container of the entire PDF
            xrefs.Add(stream.Position);
            writer.WriteLine("1 0 obj");
            writer.WriteLine("<<");
            writer.WriteLine("  /Type /Catalog");
            writer.WriteLine("  /Pages 2 0 R");
            writer.WriteLine(">>");
            writer.WriteLine("endobj");

            //#2: page-list - we have only one child page
            writer.Flush();
            stream.Flush();
            xrefs.Add(stream.Position);

            writer.WriteLine("2 0 obj");
            writer.WriteLine("<<");
            writer.WriteLine("  /Type /Pages");
            writer.WriteLine("  /Kids [3 0 R]");
            writer.WriteLine("  /Count 1");
            writer.WriteLine(">>");
            writer.WriteLine("endobj");

            //#3: page - this is our page. We specify size, font resources, and the contents
            writer.Flush();
            stream.Flush();
            xrefs.Add(stream.Position);
            writer.WriteLine("3 0 obj");
            writer.WriteLine("<<");
            writer.WriteLine("  /Type /Page");
            writer.WriteLine("  /Parent 2 0 R");
            writer.WriteLine("  /MediaBox [0 0 612 792]"); //Default userspace units: 72/inch, origin at bottom left
            writer.WriteLine("  /Resources");
            writer.WriteLine("  <<");
            writer.WriteLine("    /ProcSet [/PDF]"); //This PDF uses only the Text ability
            writer.WriteLine("    /Font");
            writer.WriteLine("    <<");
            writer.WriteLine("      /F0 4 0 R"); //I will define three fonts, #4, #5 and #6
            writer.WriteLine("      /F1 5 0 R");
            writer.WriteLine("      /F2 6 0 R");
            writer.WriteLine("    >>");
            writer.WriteLine("  >>");
            writer.WriteLine("  /Contents 7 0 R");
            writer.WriteLine(">>");
            writer.WriteLine("endobj");

            //#4, #5, #6: three font resources, all using fonts that are built into all PDF-viewers
            //We're going to use WinAnsi character encoding, defined below.
            writer.Flush();
            stream.Flush();
            xrefs.Add(stream.Position);
            writer.WriteLine("4 0 obj");
            writer.WriteLine("<<");
            writer.WriteLine("  /Type /Font");
            writer.WriteLine("  /Subtype /Type1");
            writer.WriteLine("  /Encoding /WinAnsiEncoding");
            writer.WriteLine("  /BaseFont /Times-Roman");
            writer.WriteLine(">>");

            writer.Flush();
            stream.Flush();
            xrefs.Add(stream.Position);

            writer.WriteLine("5 0 obj");
            writer.WriteLine("<<");
            writer.WriteLine("  /Type /Font");
            writer.WriteLine("  /Subtype /Type1");
            writer.WriteLine("  /Encoding /WinAnsiEncoding");
            writer.WriteLine("  /BaseFont /Times-Bold");
            writer.WriteLine(">>");

            writer.Flush();
            stream.Flush();
            xrefs.Add(stream.Position);

            //writer.WriteLine("6 0 obj");
            //writer.WriteLine("<<");
            //writer.WriteLine("  /Type /XObject");
            //writer.WriteLine("  /Subtype /Image");
            //writer.WriteLine("  /Encoding /WinAnsiEncoding");
            //writer.WriteLine("  /BaseFont /Times-Italic");
            //writer.WriteLine(">>");

            //Create an image here
            writer.WriteLine("6 0 obj");
            writer.WriteLine("<<");
            writer.WriteLine("  /Type /XObject"); //Specify the XOBject
            writer.WriteLine("  /Subtype /Image"); //It is an image
            writer.WriteLine("  /Width " + currentImage.PixelWidth); //The dimensions of the image
            writer.WriteLine("  /Height " + currentImage.PixelHeight);
            writer.WriteLine("  /BitsPerComponent 8");
            writer.WriteLine("  /Length 83183");
            writer.WriteLine("  /Filter /ASCII85Decode");
            writer.WriteLine(">>");

            writer.Flush();
            stream.Flush();
            xrefs.Add(stream.Position);


            writer.WriteLine("7 0 obj");
            writer.WriteLine("<<");
            writer.WriteLine("  /Length " + 8192);
            writer.WriteLine(">>");
            writer.WriteLine("stream");

            byte[] data = null;
            using (MemoryStream imageStream = new MemoryStream())
            {
                WriteableBitmap wBitmap = new WriteableBitmap(currentImage);
                wBitmap.SaveJpeg(imageStream, wBitmap.PixelWidth, wBitmap.PixelHeight, 0, 100);
                stream.Seek(0, SeekOrigin.Begin);
                data = imageStream.GetBuffer();
            }

            foreach (byte b in data)
            {
                writer.WriteLine(b);
            }

            // writer.Write(sb.ToString());
            writer.WriteLine("endstream");
            writer.WriteLine("endobj");


            //Closing
            //PDF-XREFS. This part of the PDF is an index table into every object #1..#7 that we defined.
            writer.Flush();
            stream.Flush();
            long xref_pos = stream.Position;
            writer.WriteLine("xref");
            writer.WriteLine("1 " + xrefs.Count);

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