namespace Image_Retrieval_Application
{
    partial class frm_main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.webbrowser = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // webbrowser
            // 
            this.webbrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webbrowser.Location = new System.Drawing.Point(0, 0);
            this.webbrowser.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.webbrowser.MinimumSize = new System.Drawing.Size(13, 13);
            this.webbrowser.Name = "webbrowser";
            this.webbrowser.Size = new System.Drawing.Size(919, 602);
            this.webbrowser.TabIndex = 0;
            // 
            // frm_main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(919, 602);
            this.Controls.Add(this.webbrowser);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "frm_main";
            this.Text = "Image Retrieval Application © Michael Andorfer, Nicola Deufemia, Stefanie Habersa" +
    "tter, Vera Karl";
            this.Load += new System.EventHandler(this.frm_main_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser webbrowser;
    }
}

