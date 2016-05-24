using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Image_Retrieval_Application
{
    public partial class frm_main : Form
    {
        // TODO: In production replace switch from solutionDirectory to applicationDirectory
        //string applicationDirectory = Path.GetDirectoryName("../../" + Application.ExecutablePath);
        static string solutionDirectory = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));

        public frm_main()
        {
            InitializeComponent();

            // Maximize form on application start
            //WindowState = System.Windows.Forms.FormWindowState.Maximized;

            this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;
            this.WindowState = FormWindowState.Maximized;
        }

        private void frm_main_Load(object sender, EventArgs e)
        {
            // Add document complete handler on webbrowser component
            webbrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(onDocumentCompleted);

            // Load index on start
            webbrowser.Url = new Uri("file:///" + solutionDirectory + "\\web\\index.html");
        }

        private void onDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            addEventHandlers();
        }

        protected void addEventHandlers()
        {
            // Add search button event handler
            HtmlElement searchButton = webbrowser.Document.GetElementById("search-button");
            searchButton.AttachEventHandler("onclick", (sender, args) => onSearchButtonClicked(searchButton, EventArgs.Empty));
        }

        protected void onSearchButtonClicked(object sender, EventArgs args)
        {
            HtmlElement searchField = webbrowser.Document.GetElementById("search-text");
            string searchFieldValue = searchField.GetAttribute("value");
            Program.startTagSearch(searchFieldValue);
        }
    }
}
