using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Net;
using System.Collections.Specialized;

namespace GetTruyen
{
    public partial class MainForm : Form
    {
        private Novel novel;
        public MainForm()
        {
            InitializeComponent();
            SetBtns(1, 0);
        }

        delegate void SetInfoDelegate(string name, string author, int so_chuong);
        private void SetInfo(string name, string author, int so_chuong)
        {
            if(tbName.InvokeRequired)
            {
                Invoke(new SetInfoDelegate(SetInfo), 
                    new object[] { name, author, so_chuong });
            }
            else
            {
                this.tbName.Text = name;
                this.tbAuthor.Text = author;
                this.tbCount.Text = so_chuong == 0 ? "Đang xử lý" : so_chuong.ToString();
            }
        }

        delegate void SetListDelegate(ChapterCollection chapters);
        private void SetList(ChapterCollection chapters)
        {
            if (lbChuong.InvokeRequired)
                Invoke(new SetListDelegate(SetList), new object[] { chapters });
            else
            {
                lbChuong.Items.Clear();
                foreach(Chapter chapter in chapters)
                {
                    lbChuong.Items.Add(chapter.Id.ToString()+" - " + chapter.Name);
                }
            }
        }
        delegate void SetDownloadedItemDelegate(int index);
        private void SetDownloadedItem(int index)
        {
            if (lbChuong.InvokeRequired)
                Invoke(new SetDownloadedItemDelegate(SetDownloadedItem), new object[] { index });
            else
            {
                lbChuong.Items[index] = "[Downloaded] "+lbChuong.Items[index];
                lbChuong.SelectedItem = lbChuong.Items[index];
            }
        }

        delegate void SetBtnsDelegate(int AnaBtn=0, int DownBtn=0);
        private void SetBtns(int AnaBtn = 0, int DownBtn = 0)
        {
            if (btnAnalyze.InvokeRequired)
                Invoke(new SetBtnsDelegate(SetBtns), new object[] { AnaBtn, DownBtn });
            else
            {
                btnAnalyze.Enabled = AnaBtn == 0 ? false : true;
                btnDownload.Enabled = DownBtn == 0 ? false : true;
            }
        }

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            ThreadStart addNovel = delegate () 
            {
                novel = new Novel();
                SetInfo("Đang xử lý", "Đang xử lý", 0);
                SetBtns(0, 0);
                if (novel.LoadUrl(tbUrl.Text))
                {
                    SetInfo(novel.Name, novel.Author, 0);
                    novel.LoadChaptersList(novel.HtmlDoc.DocumentNode);
                    SetInfo(novel.Name, novel.Author, novel.Chapters.Count);
                    SetList(novel.Chapters);
                    SetBtns(1, 1);
                }
                else
                {
                MessageBox.Show(
                    novel.Error.ToString(),
                    "Error!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                    );
                }


            };
            Thread thread = new Thread(addNovel);
            thread.Start();
        }

        private void lbChuong_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = lbChuong.IndexFromPoint(e.Location);
            if(index!=ListBox.NoMatches)
            {
                ChapterDisplay chapDisplay = new ChapterDisplay(index,novel);
                chapDisplay.ShowDialog();
                chapDisplay.Dispose();
            }
            
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (novel.Downloading == 0)
            {
                novel.Downloading = 1;
                btnDownload.Text = "Cancel";
                SetBtns(0, 1);
                ThreadStart DownTS = delegate ()
                 {
                     if (!Directory.Exists(novel.folder))
                         Directory.CreateDirectory(novel.folder);
                     string raw_folder = "./"+novel.folder + @"\raw";
                     if (!Directory.Exists(raw_folder))
                         Directory.CreateDirectory(raw_folder);
                     using (StreamWriter info_file = new StreamWriter(raw_folder+@"\info.ini"))
                     {
                         info_file.WriteLine(novel.Name);
                         info_file.WriteLine(novel.Author);
                         info_file.WriteLine(novel.Chapters.Count.ToString());
                         info_file.WriteLine(novel.Url);
                         info_file.Close();
                     }
                     if (novel.Downloading == 1)
                     {
                         for (int i = 0; i < novel.Chapters.Count; i++)
                         {
                             Chapter chapter = novel.Chapters[i];
                             if (novel.Downloading == 1)
                             {
                                 using (StreamWriter fs = new StreamWriter(raw_folder + "\\" + i.ToString() + ".ini"))
                                 {
                                     fs.WriteLine(novel.DownloadChapter(i).Name);
                                     fs.WriteLine(novel.DownloadChapter(i).Contents);
                                     SetDownloadedItem(i);
                                     fs.Close();
                                 }
                             }

                         }
                         SetBtns(1, 1);
                         novel.Downloading = 0;
                     }
                     else
                     {
                        SetList(novel.Chapters);
                     }
                 };
                Thread DownThread = new Thread(DownTS);
                DownThread.Start();
            }
            else
            {
                novel.Downloading = 0;
                btnDownload.Text = "Download";
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }
    }
}
