using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Text_Editor
{
    public partial class MainForm : Form
    {
        // conains the path to the currently opened file
        internal string currentFilePath;

        public MainForm()
        {
            InitializeComponent();
            currentFilePath = "";
        }

        // clears text selection
        private void ClearSelection(object sender = null, EventArgs e = null)
        {
            this.ClearSelectionBackground();
            this.mainRichTb.DeselectAll();
        }

        // clears selection backColor
        private void ClearSelectionBackground()
        {
            this.mainRichTb.SelectionStart = 0;
            this.mainRichTb.SelectionLength = this.mainRichTb.Text.Length;
            this.mainRichTb.SelectionBackColor = this.mainRichTb.BackColor;
        }

        // changing background when hovering on the tooltip items
        private void ApplyHoverEffect(object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            pb.BackColor = Color.LightBlue;
        }
        private void ReleaseHoverEffect(object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            pb.BackColor = this.BackColor;
        }

        // basic editor functionality
        private void Undo(object sender, EventArgs e)
        {
            mainRichTb.Undo();
        }
        private void Redo(object sender, EventArgs e)
        {
            mainRichTb.Redo();
        }
        private void Cut(object sender, EventArgs e)
        {
            mainRichTb.Cut();
        }
        private void Copy(object sender, EventArgs e)
        {
            mainRichTb.Copy();
        }
        private void Paste(object sender, EventArgs e)
        {
            mainRichTb.Paste();
        }
        private void SelectAll(object sender, EventArgs e)
        {
            mainRichTb.SelectAll();
            mainRichTb.Focus();
        }
        private void DeleteSelectedText(object sender, EventArgs e)
        {
            int selectionLength = mainRichTb.SelectionLength;
            mainRichTb.Text = mainRichTb.Text.Remove(mainRichTb.SelectionStart, selectionLength);
        }
        private void InsertCurrentDate(object sender, EventArgs e)
        {
            mainRichTb.SelectedText = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }
        private void UpdateFont(object sender, EventArgs e)
        {
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                mainRichTb.SelectionFont = fontDialog.Font;
                mainRichTb.SelectionColor = fontDialog.Color;
            }
        }
        private void UpdateBackColor(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                if (item.Tag.ToString() == "text")
                {
                    mainRichTb.SelectionBackColor = colorDialog.Color;
                }
                else if (item.Tag.ToString() == "panel")
                {
                    mainRichTb.BackColor = colorDialog.Color;
                }
            }
        }
        private void MakeFormFullScreen(object sender, EventArgs e)
        {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
        }
        private void NormalizeScreen(object sender, EventArgs e)
        {
            FormBorderStyle = FormBorderStyle.Sizable;
            WindowState = FormWindowState.Normal;
        }
        private void DisplayAboutForm(object sender, EventArgs e)
        {
            AboutForm aboutForm = new AboutForm(this.currentFilePath);
            aboutForm.Show();
        }
        private void Exit(object sender, EventArgs e)
        {
            this.Close();
        }

        // enabling and disabling "Select" and "Replace" buttons when changing the text in the corresponding textboxes
        // getting the corresponding textbox reference using their Tag property
        private void UpdateButtonState(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if(tb.Text.Trim().Length > 0)
            {
                if(tb.Tag.ToString() == "find")
                {
                    findTextButton.Enabled = true;
                }else if (tb.Tag.ToString() == "replace")
                {
                    replaceTextButton.Enabled = true;
                }
            }
            else
            {
                if (tb.Tag.ToString() == "find")
                {
                    findTextButton.Enabled = false;
                }
                else if (tb.Tag.ToString() == "replace")
                {
                    replaceTextButton.Enabled = false;
                }
            }
        }

        // selecting all string occurences in the textbox
        private void FindStringOccurences(object sender, EventArgs e)
        {
            this.ClearSelection();

            string stringToReplace = findTextTb.Text.Trim();

            // using regular expression to find all matches in the text
            MatchCollection matches = Regex.Matches(mainRichTb.Text, stringToReplace);

            foreach (Match match in matches)
            {
                // adding match to selection and updating it visually
                mainRichTb.Select(match.Index, match.Length);
                mainRichTb.SelectionBackColor = Color.Yellow;
            }
        }

        // replacing all text occurences with the given stiring from textbox
        private void ReplaceTextOccurences(object sender, EventArgs e)
        {
            if(findTextTb.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please select the searched text before replacing.", "Invalid Operation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string stringToReplace = findTextTb.Text.Trim();
            string replacementString = replaceTextTb.Text.Trim();

            mainRichTb.Text = mainRichTb.Text.Replace(stringToReplace, replacementString);
        }

        // creating new file functionality
        private void CreateNewFile(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Do you want to save your file before continuing?", "Save your changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            
            if(dialogResult == DialogResult.Yes)
            {

                if (File.Exists(this.currentFilePath))
                {
                    // if file already exists, save it directly without using saveFileDialog
                    this.SaveFileDirectly();
                }
                else
                {
                    // if the file doesn't exist, save it using saveFileDialog
                    this.SaveFile();
                }
                // clear current file path and reset form
                this.currentFilePath = "";
                mainRichTb.Text = findTextTb.Text = replaceTextTb.Text = String.Empty;
            }
            if(dialogResult == DialogResult.No)
            {
                // clear current file path and reset form
                this.currentFilePath = "";
                mainRichTb.Text = findTextTb.Text = replaceTextTb.Text = String.Empty;
            }
        
        }

        // loading file using openFileDialog
        private void OpenFile(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // get the path and extention of specified file
                    this.currentFilePath = openFileDialog.FileName;
                    string fileExtention = Path.GetExtension(this.currentFilePath);

                    // check file type and load it using different methods depending on its type
                    if (fileExtention == ".rtf")
                    {
                        // load rtf file with all of its encoded specifications
                        mainRichTb.LoadFile(this.currentFilePath);
                    }
                    else if (fileExtention == ".txt")
                    {
                        // read the contents of the txt file into a stream
                        var fileStream = openFileDialog.OpenFile();
                        using (StreamReader reader = new StreamReader(fileStream))
                        {
                            mainRichTb.Text = reader.ReadToEnd();
                        }
                    }
                }catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Invalid Operation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // saving file directly on Ctrl+S without using saveFileDialog
        private void SaveFileDirectly(object sender = null, EventArgs e = null)
        {
            try
            {
                // when pressing Ctrl+S on newly created file, check if the file exists and save it using saveFileDialog
                if(this.currentFilePath.Trim().Length == 0)
                {
                    this.SaveFile();
                    return;
                }

                // get file extension and save it differently depending on its type
                string fileExtension = Path.GetExtension(this.currentFilePath);

                if (fileExtension == ".txt")
                {
                    using (StreamWriter sw = File.CreateText(this.currentFilePath))
                    {
                        sw.Write(mainRichTb.Text);
                    }
                }
                else if (fileExtension == ".rtf")
                {
                    // save the rtf file with all of its encoded specifications
                    mainRichTb.SaveFile(this.currentFilePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Invalid Operation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }

        // saving file using saveFileDialog
        private void SaveFile(object sender = null, EventArgs e = null)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)

            {
                this.currentFilePath = saveFileDialog.FileName;
                var extension = Path.GetExtension(saveFileDialog.FileName);

                StreamWriter writer = new StreamWriter(saveFileDialog.OpenFile());
                
                // saveFileDialog functionality, depending on the file type
                if (extension == ".txt")
                {
                    writer.Write(mainRichTb.Text);
                }
                else if(extension == ".rtf")
                {
                    writer.Write(mainRichTb.Rtf);
                }

                // dispose stream from stack
                writer.Dispose();
                writer.Close();
            }
        }
    }
}
