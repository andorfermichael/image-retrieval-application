﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Image_Retrieval_Application
{
    public partial class frm_main : Form
    {
        public frm_main()
        {
            InitializeComponent();

            // Maximize form on application start
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
        }
    }
}
