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
        static int resultsPerPage = 8;
        static List<string> cachedResults = new List<string>();

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
            cachedResults = paths;
            drawResults(paths, searchFieldValue, 1, resultsPerPage);
        }

        protected void drawResults(List<string> paths, string searchWord, int currentPage, int resultsPerPage)
        {
            // Get result container
            HtmlElement resultsContainer = webbrowser.Document.GetElementById("results-container");
            if (paths.Count > 0)
                 resultsContainer.InnerHtml = "<h2>" + searchWord + "<small class='text-muted'>  Showing results " + ((resultsPerPage * (currentPage - 1)) + 1) + " to " + resultsPerPage * currentPage + " from " + paths.Count + ":";
            else
                resultsContainer.InnerHtml = "<h2>No results found for '" + searchWord + "'!</h2>";
            // Insert results into results container
            HtmlElement divContainer = webbrowser.Document.CreateElement("div");
            divContainer.SetAttribute("className", "container-fluid");

            foreach (string path in paginate(paths, currentPage, resultsPerPage))
            {
                HtmlElement div = webbrowser.Document.CreateElement("div");
                HtmlElement img = webbrowser.Document.CreateElement("img");
                HtmlElement a = webbrowser.Document.CreateElement("a");

                div.SetAttribute("className", "col-sm-6 col-md-3");
                a.SetAttribute("className", "thumbnail");
                a.SetAttribute("href", path);
                img.SetAttribute("src", path);
                img.SetAttribute("alt", "Placeholder for Resultimage");
                a.AppendChild(img);
                div.AppendChild(a);

                divContainer.AppendChild(div);
            }

            resultsContainer.AppendChild(divContainer);
            Console.WriteLine("-> Generating Pagination for Results");
            generatePaginationHTML(paths, currentPage, resultsPerPage);
        }

        protected void generatePaginationHTML(List<string> results, int currentPage, int resultsPerPage)
        {
            if (currentPage <= 0)
                currentPage = 1;
            HtmlElement resultsControll = webbrowser.Document.GetElementById("results-controll");
            resultsControll.InnerHtml = "";

            HtmlElement ul = webbrowser.Document.CreateElement("ul");
            HtmlElement liPrev = webbrowser.Document.CreateElement("li");
            HtmlElement liNext = webbrowser.Document.CreateElement("li");
            HtmlElement aPrevious = webbrowser.Document.CreateElement("a");
            HtmlElement aNext = webbrowser.Document.CreateElement("a");
            HtmlElement spanPrev = webbrowser.Document.CreateElement("span");
            HtmlElement spanNext = webbrowser.Document.CreateElement("span");

            ul.SetAttribute("className", "pagination");


            //Previous page button
            aPrevious.SetAttribute("a", "#");
            aPrevious.Id = "prev-results";
            aPrevious.SetAttribute("arial-label", "Previous");
            spanPrev.SetAttribute("aria-hidden", "true");
            spanPrev.InnerHtml = "&laquo;";
            aPrevious.AppendChild(spanPrev);
            liPrev.AppendChild(aPrevious);

            if (currentPage == 1)
                liPrev.SetAttribute("className", "disabled");
 
            //Next page button
            aNext.SetAttribute("a", "#");
            aNext.Id = "next-results";
            aNext.SetAttribute("arial-label", "Next");
            spanNext.SetAttribute("aria-hidden", "true");
            spanNext.InnerHtml = "&raquo;";
            aNext.AppendChild(spanNext);
            liNext.AppendChild(aNext);

            if ((results.Count / resultsPerPage) < 1)
                aNext.SetAttribute("className", "disabled");
            
            for (int i = 0; i <= (results.Count / resultsPerPage); i++)
            {
                HtmlElement liTEMP = webbrowser.Document.CreateElement("li");
                HtmlElement aTEMP = webbrowser.Document.CreateElement("a");
                if (i == 0)
                    ul.AppendChild(liPrev);
                if (i == currentPage - 1)
                {
                    liTEMP.SetAttribute("className", "active");
                    aTEMP.Id = "current-page";
                    aTEMP.SetAttribute("href", "#");
                    aTEMP.InnerHtml = (i + 1).ToString();
                    liTEMP.AppendChild(aTEMP);
                    ul.AppendChild(liTEMP);
                }
                else
                {
                    string id = "page-" + (i + 1).ToString();
                    aTEMP.SetAttribute("href", "#");
                    aTEMP.Id = id;
                    aTEMP.InnerHtml = (i + 1).ToString();
                    aTEMP.AttachEventHandler("onclick", (sender, args) => onPageXButtonClicked(aTEMP, EventArgs.Empty));
                    liTEMP.AppendChild(aTEMP);
                    ul.AppendChild(liTEMP);
                }
                if (i == (results.Count / resultsPerPage))
                    ul.AppendChild(liNext);

            }
            resultsControll.AppendChild(ul);

            if (currentPage > 1)
            {
                    // Add previousResults event handler
                    HtmlElement prevResults = webbrowser.Document.GetElementById("prev-results");
                    prevResults.AttachEventHandler("onclick", (sender, args) => onPrevPageButtonClicked(prevResults, EventArgs.Empty));
            }

            if (currentPage < (results.Count / resultsPerPage))
            {
                // Add nextResults event handler
                HtmlElement nextResults = webbrowser.Document.GetElementById("next-results");
                nextResults.AttachEventHandler("onclick", (sender, args) => onNextPageButtonClicked(nextResults, EventArgs.Empty));
            }

            Console.WriteLine(resultsControll.InnerHtml.ToString());
        }

        protected void onNextPageButtonClicked(object sender, EventArgs args)
        {
            // Get search value from input field and start tag search
            HtmlElement searchField = webbrowser.Document.GetElementById("search-field");
            string searchFieldValue = searchField.GetAttribute("value");

            // Get search value from input field and start tag search
            HtmlElement currentPage = webbrowser.Document.GetElementById("current-page");
            int currentPageValue = Int32.Parse(currentPage.GetAttribute("InnerHtml"));
            if (currentPageValue < cachedResults.Count)
                drawResults(cachedResults, searchFieldValue, currentPageValue + 1, resultsPerPage);
        }

        protected void onPrevPageButtonClicked(object sender, EventArgs args)
        { 
            // Get search value from input field and start tag search
            HtmlElement searchField = webbrowser.Document.GetElementById("search-field");
            string searchFieldValue = searchField.GetAttribute("value");

            // Get search value from input field and start tag search
            HtmlElement currentPage = webbrowser.Document.GetElementById("current-page");
            int currentPageValue = Int32.Parse(currentPage.GetAttribute("InnerHtml"));
            if (currentPageValue > 1)
                drawResults(cachedResults, searchFieldValue, currentPageValue - 1, resultsPerPage);
        }

        protected void onPageXButtonClicked(HtmlElement sender, EventArgs args)
        {
            // Get search value from input field and start tag search
            HtmlElement searchField = webbrowser.Document.GetElementById("search-field");
            string searchFieldValue = searchField.GetAttribute("value");

            // Get page-x value
                drawResults(cachedResults, searchFieldValue, Int32.Parse(sender.GetAttribute("Innerhtml")), resultsPerPage);
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

        private static List<string> paginate(List<string> imgpaths, int page, int resultsPerPage)
        {
            List<string> resultsForPage = new List<string>();
            for (int i = 0 + (page - 1) * resultsPerPage; i < (page * resultsPerPage) && i < imgpaths.Count; i++)
            {
                resultsForPage.Add(imgpaths[i]);
            }
            Console.WriteLine("Showing results {0} to {1} of {2} on page {3}", (0 + (page - 1) * resultsPerPage), (page * resultsPerPage), imgpaths.Count, page);
            return resultsForPage;
        }
    }
}
