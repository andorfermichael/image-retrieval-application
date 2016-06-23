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

        protected void onImageClicked(object sender, EventArgs args)
        {
            // Receive clicked element via mouse coordinates
            Point ScreenCoord = new Point(MousePosition.X, MousePosition.Y);
            Point BrowserCoord = webbrowser.PointToClient(ScreenCoord);
            HtmlElement elem = webbrowser.Document.GetElementFromPoint(BrowserCoord);

            long imageID = Convert.ToInt64(Path.GetFileName(elem.GetAttribute("src")).Replace(".jpg", ""));
            string selectedOption = "";

            HtmlElement selectBox = webbrowser.Document.GetElementById("sel1");

            foreach (HtmlElement item in selectBox.Children)
            {
                if (item.GetAttribute("value") == webbrowser.Document.GetElementById("sel1").GetAttribute("value"))
                {
                    selectedOption = item.InnerText;
                }
            }

            //List<string> imagePaths = Program.retrieveSimilarImages(imageID, selectedOption);

            // Insert similar images into modal
            insertSimilarImagesIntoModal(imagePaths);

            // Open modal window
            HtmlDocument doc = webbrowser.Document;
            HtmlElement head = doc.GetElementsByTagName("head")[0];
            HtmlElement s = doc.CreateElement("script");
            s.SetAttribute("text", "function openModel() { $('#my-modal').modal('show'); }");
            head.AppendChild(s);
            webbrowser.Document.InvokeScript("openModel");
        }

        protected void insertSimilarImagesIntoModal(List<string> paths)
        {
            // Get modal body
            HtmlElement modalBody = webbrowser.Document.GetElementById("modal-body");

            // Insert images
            foreach (string path in paths)
            {
                HtmlElement div = webbrowser.Document.CreateElement("div");
                HtmlElement img = webbrowser.Document.CreateElement("img");
                HtmlElement a = webbrowser.Document.CreateElement("a");

                img.SetAttribute("src", path);
                img.SetAttribute("alt", "Placeholder for Resultimage");
                modalBody.AppendChild(img);
            }
        }

        protected void drawResults(List<string> paths, string searchWord, int currentPage, int resultsPerPage)
        {
            // Get result container
            HtmlElement resultsContainer = webbrowser.Document.GetElementById("results-container");
            if (paths.Count > 0)
            {
                int showingTo = 0;
                if (resultsPerPage * currentPage > paths.Count)
                    showingTo = paths.Count;
                else
                    showingTo = resultsPerPage * currentPage;
                resultsContainer.InnerHtml = "<h2>" + searchWord + "<small class='text-muted'>  Showing results " + ((resultsPerPage * (currentPage - 1)) + 1) + " to " + showingTo + " from " + paths.Count + ":";
            }
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
                img.SetAttribute("src", path);
                img.SetAttribute("alt", "Placeholder for Resultimage");
                img.SetAttribute("data-target", "#myModal");
                a.AttachEventHandler("onclick", (sender, args) => onImageClicked(a, EventArgs.Empty));
                a.AppendChild(img);
                div.AppendChild(a);

                divContainer.AppendChild(div);
            }

            resultsContainer.AppendChild(divContainer);
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

        private static List<string> paginate(List<string> imgpaths, int page, int resultsPerPage)
        {
            List<string> resultsForPage = new List<string>();
            for (int i = 0 + (page - 1) * resultsPerPage; i < (page * resultsPerPage) && i < imgpaths.Count; i++)
            {
                resultsForPage.Add(imgpaths[i]);
            }
            return resultsForPage;
        }

        private void webbrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }
    }
}
