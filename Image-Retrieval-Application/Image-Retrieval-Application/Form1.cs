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
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
        }

        private void frm_main_Load(object sender, EventArgs e)
        {
            webbrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(IntializeDocument);

            // Load index on start
            webbrowser.Url = new Uri("file:///" + solutionDirectory + "\\web\\index.html");
        }

        private void IntializeDocument(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            Debug.WriteLine("Document.ready");
        }
    }
}
