using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Mp3TagSuite
{
    public partial class Main_Form : Form
    {
        private static string mSearchQuery = "";
        public static string SearchQuery
        {
            get { return mSearchQuery; }
            set { mSearchQuery = value; }
        }

        public Main_Form()
        {
            InitializeComponent();

            MenuItem_SaveTag.Enabled = false;
            MenuItem_RemoveTag.Enabled = false;
            MenuItem_ReadTag.Enabled = false;
            MenuItem_Pl_Af.Enabled = false;
            MenuItem_Pl_Sf.Enabled = false;
            MenuItem_Export.Enabled = false;
            if (Properties.Settings.Default.WorkingDirectory != "")
            {
                Load_LV(Properties.Settings.Default.WorkingDirectory);
            }

        }

        private void MenuItem_Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MenuItem_ChangeDirectory_Click(object sender, EventArgs e)
        {
            ChangeDirectory();
        }

        private void ChangeDirectory()
        {
            string WorkingDirectory = Properties.Settings.Default.WorkingDirectory;
            FBD.SelectedPath = WorkingDirectory;
            if (FBD.ShowDialog() == DialogResult.OK)
            {
                Mp3ListView.Items.Clear();
                string RootDirectory = FBD.SelectedPath;
                Properties.Settings.Default.WorkingDirectory = RootDirectory;
                Properties.Settings.Default.Save();
                this.Text = this.Text + " - " + RootDirectory;
                Load_LV(RootDirectory);
            }
        }

        private void Load_LV(string RootDirectory)
        {
            var AllowedExt = new List<string> { ".mp3", ".mp4", ".flacc" };
            var Mp3Files = Directory.GetFiles(RootDirectory, "*.*", SearchOption.AllDirectories).Where(s => AllowedExt.Contains(System.IO.Path.GetExtension(s)));
            //string[] Mp3Files = Directory.GetFiles(RootDirectory);
            foreach (string Mp3File in Mp3Files)
            {
                AddFile(Mp3File);
            }

        }

        private void AddFile(String AudioFilePath)
        {
            string fileName = System.IO.Path.GetFileName(AudioFilePath);
            string filePath = AudioFilePath.Replace(fileName, "");
            TagLib.File file = TagLib.File.Create(AudioFilePath);
            string Artist = file.Tag.FirstPerformer;
            string Album = file.Tag.Album;
            string Title = file.Tag.Title;
            string Length = file.Properties.Duration.ToString();
            string[] row = { fileName, filePath, "", Artist, Album, Title };
            var Mp3Item = new ListViewItem(row);
            if (Mp3ListView.Items.Count % 2 == 0)
            {
                Mp3Item.BackColor = Color.AliceBlue;
            }
            else
            {
                Mp3Item.BackColor = Color.LightGray;
            }
            Mp3ListView.Items.Add(Mp3Item);

            // DataGridView Name DG_AudioFiles <Test>

            //var Artist_CB = new DataGridViewComboBoxColumn();

            //Artist_CB.DataSource = new List<string>() { Artist };

            //DG_AudioFiles.Columns.Add(Artist_CB);

            Int32 rIndex = DG_AudioFiles.Rows.Count -1;
            DG_AudioFiles.Rows.Add(fileName,filePath,"",Artist);
            Debug.WriteLine(rIndex.ToString());

           
            





        }

        private void Mp3ListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Mp3ListView.SelectedItems.Count == 0)
            {
                return;
            }
            if (Mp3ListView.SelectedItems.Count == 1)
            {
                ListViewItem AudioFileRow = Mp3ListView.SelectedItems[0];
                CB_Artist.Text = AudioFileRow.SubItems[3].Text;
                CB_Album.Text = AudioFileRow.SubItems[4].Text;
                CB_Title.Text = AudioFileRow.SubItems[5].Text;

                string Mp3FilePath = AudioFileRow.SubItems[1].Text + AudioFileRow.SubItems[0].Text;
                TagLib.File file = TagLib.File.Create(Mp3FilePath);
                if (file.Tag.Pictures.Length >= 1)
                {
                    var AlbumArt = (byte[])(file.Tag.Pictures[0].Data.Data);
                    PB_AlbumArt.Image = Image.FromStream(new MemoryStream(AlbumArt));
                }
                else
                {
                    PB_AlbumArt.Image = Properties.Resources.AlbumClipArt;
                }
            }
            if (Mp3ListView.SelectedItems.Count > 1)
            {
                PB_AlbumArt.Image = Properties.Resources.AlbumClipArt;
            }
        }

        private void MenuItem_Actions_Click(object sender, EventArgs e)
        {
            TC_Main.SelectTab(TAB_Actions);
        }

        private void BTN_Ok_Click(object sender, EventArgs e)
        {
            if (Mp3ListView.CheckedItems.Count > 0)
            {
                foreach (ListViewItem AudioRow in Mp3ListView.CheckedItems)
                {
                    string AudioFile_Filename = AudioRow.SubItems[0].Text;
                    string AudioFile_Path = AudioRow.SubItems[2].Text;
                    string AudioFilePath = AudioFile_Path + AudioFile_Filename;
                    string Artist = AudioRow.SubItems[3].Text;
                    string Album = AudioRow.SubItems[4].Text;
                    string Title = AudioRow.SubItems[5].Text;

                    if (Artist == "" && Album == "" && Title == "")
                    {

                        iSearch_Form SearchForm = new iSearch_Form();
                        SearchForm.Filename = AudioFile_Filename;
                        if (SearchForm.ShowDialog() == DialogResult.OK)
                        {
                            Debug.WriteLine(mSearchQuery);
                            iLink(mSearchQuery);
                            //AudioRow.Checked = false;
                        }
                        break;
                    }

                    foreach (ListViewItem Action in LV_Actions.CheckedItems)
                    {
                        Debug.WriteLine("Action: " + Action.Text + " on file: " + AudioFilePath);
                        iLink(AudioRow.SubItems[0].Text);
                    }
                }

            }
        }

        private void iLink(string str)
        {
            SearchQuery = "";
            string iLink = "https://itunes.apple.com/search?term={{Query}}&entity=song&limit=200";
            Debug.WriteLine("str value: " + str);
            string iQuery = str;
            if (System.IO.Path.GetExtension(iQuery) != "")
            {
                iQuery = iQuery.Replace(System.IO.Path.GetExtension(iQuery), "").ToLower();
            }
            iQuery = Regex.Replace(iQuery, "[^a-zA-Z0-9 ]+", "", RegexOptions.Compiled);
            iQuery = Regex.Replace(iQuery, @"\s+", "+");
            Debug.WriteLine(iQuery);
            iLink = iLink.Replace("{{Query}}", iQuery);
            Debug.WriteLine(iLink);
            WebClient WC = new WebClient();
            string Results = WC.DownloadString(iLink);
            Debug.WriteLine(Results);
        }

        private void MenuItem_Check_WorkingDirectory_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Properties.Settings.Default.WorkingDirectory);
        }

        private void DG_AudioFiles_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            

        }
    }
}