using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Threading;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GetTruyen
{
    public partial class ChapterDisplay : Form
    {
        public ChapterDisplay(int id, Novel novel)
        {
            InitializeComponent();
            ThreadStart threadS = delegate ()
            {
                Chapter cont=novel.DownloadChapter(id);
                lblTitle.Invoke((MethodInvoker)(()=>lblTitle.Text=cont.Name));
                rtbDisplay.Invoke((MethodInvoker)(() => rtbDisplay.Text = cont.Contents));
            };
            Thread thread = new Thread(threadS);
            thread.Start();

        }
    }
}
