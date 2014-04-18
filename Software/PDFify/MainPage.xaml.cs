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

namespace PDFify
{
    public partial class MainPage : PhoneApplicationPage
    {
        PhotoChooserTask photoChooserTask;
        CameraCaptureTask cameraCaptureTask;
        BitmapImage currentImage;
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
                currentImage.SetSource(e.ChosenPhoto);
                currentImageLocation = e.OriginalFileName;
                imgPreview.Source = currentImage;
            }
        }

        void photoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                //MessageBox.Show(e.ChosenPhoto.Length.ToString());

                //Code to display the photo on the page in an image control named myImage.

                currentImage.SetSource(e.ChosenPhoto);
                currentImageLocation = e.OriginalFileName;
                imgPreview.Source = currentImage;
            }
        }

        private void btExport_ManipulationStarted(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            /*
            if (currentImage == null && currentImageLocation.Length < 1)
            {
                MessageBox.Show("Please select a valid image");
                return;
            }
            */

            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            MessageBox.Show(currentImageLocation);

            String name = Path.GetFullPath(currentImageLocation);
           MessageBox.Show(name);
            XImage pdfImg = XImage.FromFile(name);
            try
            {
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something went wrong! Please try again");
                
                return;
            }

            //IsolatedStorageFile fileStorage = IsolatedStorageFile.GetUserStoreForApplication();
            //StreamWriter Writer = new StreamWriter(new IsolatedStorageFileStream("TestFile.txt", FileMode.OpenOrCreate, fileStorage));

            String fileName = "export.pdf";
            document.Save(fileName);
            document.Close();
           
            MessageBox.Show("The file has been saved to storage under the name " + fileName);

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