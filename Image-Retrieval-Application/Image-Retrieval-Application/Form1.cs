using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace Image_Retrieval_Application
{
    public partial class frm_main : Form
    {
        // TODO: In production replace switch from solutionDirectory to applicationDirectory
        //string applicationDirectory = Path.GetDirectoryName("../../" + Application.ExecutablePath);
        static string solutionDirectory = Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()));
        static string indexUri = "file:///" + solutionDirectory + @"\web\index.html";
        static string uploadDirectory = solutionDirectory + @"\user\image-upload\";

        public frm_main()
        {
            InitializeComponent();

            // Maximize form on application start
            WindowState = FormWindowState.Maximized;
        }

        private void frm_main_Load(object sender, EventArgs e)
        {
            // Add document complete handler on webbrowser component
            webbrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(onDocumentCompleted);

            // Load index page on start
            webbrowser.Url = new Uri(indexUri);
        }

        private void onDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            // All elements are loaded, add event handlers
            addEventHandlers();
        }

        protected void addEventHandlers()
        {
            // Add searchbutton event handler
            HtmlElement searchButton = webbrowser.Document.GetElementById("search-button");
            searchButton.AttachEventHandler("onclick", (sender, args) => onSearchButtonClicked(searchButton, EventArgs.Empty));

            // Add uploadbutton event handler
            HtmlElement uploadButton = webbrowser.Document.GetElementById("upload-button");
            uploadButton.AttachEventHandler("onclick", (sender, args) => onUploadButtonClicked(uploadButton, EventArgs.Empty));
        }

        protected void onSearchButtonClicked(object sender, EventArgs args)
        {
            // Get search value from input field and start tag search
            HtmlElement searchField = webbrowser.Document.GetElementById("search-field");
            string searchFieldValue = searchField.GetAttribute("value");
            List<string> paths = Program.startTagSearch(searchFieldValue);

            // Get result container
            HtmlElement resultsContainer = webbrowser.Document.GetElementById("results-container");

            // Insert results into results container
            foreach (string path in paths)
            {
                HtmlElement div = webbrowser.Document.CreateElement("div");
                HtmlElement img = webbrowser.Document.CreateElement("img");

                img.SetAttribute("src", path);
                div.AppendChild(img);
                resultsContainer.AppendChild(div);
            }
        }

        protected void onUploadButtonClicked(object sender, EventArgs args)
        {
            // Get image source base64 string
            HtmlElement fileImagePreview = webbrowser.Document.GetElementById("file-preview-image");
            string base64String = fileImagePreview.GetAttribute("src");

            // Get image source base64 string
            HtmlElement fileFooterCaption = webbrowser.Document.GetElementById("file-footer-caption");
            string fileName = fileFooterCaption.GetAttribute("title");

            // Remove meta information
            int index = base64String.IndexOf("data:image/jpeg;base64,");
            string cleanPath = (index < 0) ? base64String : base64String.Remove(index, "data:image/jpeg;base64,".Length);

            // Store image on file system
            SaveByteArrayAsImage(uploadDirectory + fileName, cleanPath);

            Program.startQueryByExampleSearch(uploadDirectory + fileName);
        }

        // Stores base64 string as image on file system
        private void SaveByteArrayAsImage(string fullOutputPath, string base64String)
        {
            byte[] bytes = Convert.FromBase64String(base64String);

            Image image;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                image = Image.FromStream(ms);
                image.Save(fullOutputPath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }
    }
}
