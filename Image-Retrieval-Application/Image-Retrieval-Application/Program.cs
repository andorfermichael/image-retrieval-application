using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Image_Retrieval_Application
{
    static class Program
    {
        // TODO: In production replace switch from solutionDirectory to applicationDirectory
        //string applicationDirectory = Path.GetDirectoryName("../../" + Application.ExecutablePath);
        static string solutionDirectory = Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()));

        public static void startTagSearch(string searchValue) {
            // TODO: Replace with real function code
            Debug.WriteLine("Search Value: " + searchValue);
        }

        public static void startQueryByExampleSearch(string imageLocation)
        {
            // TODO: Replace with real function code
            Debug.WriteLine("Example Location: " + imageLocation);
        }






        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frm_main());
        }
    }
}
