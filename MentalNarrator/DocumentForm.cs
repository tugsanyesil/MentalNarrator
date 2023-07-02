using System.IO;
using System.Windows.Forms;

namespace MentalNarrator
{
    public partial class DocumentForm : Form
    {
        public DocumentForm(string[] Documents)
        {
            InitializeComponent();
            DocumentListBox.PreviewKeyDown += DocumentListBox_PreviewKeyDown;
            DocumentListBox.Items.AddRange(Documents);
        }

        private void DocumentListBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (-1 != DocumentListBox.SelectedIndex)
                {
                    var FileName = DocumentListBox.Items[DocumentListBox.SelectedIndex].ToString();
                    if (File.Exists(FileName))
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(FileName);
                        } catch { }
                    }
                }
                this.Close();
            }
            else
            {
                if (e.Control && e.KeyCode == Keys.C)
                {
                    if (-1 != DocumentListBox.SelectedIndex)
                    {
                        var FileName = DocumentListBox.Items[DocumentListBox.SelectedIndex].ToString();
                        Clipboard.SetData(DataFormats.StringFormat, FileName);
                    }
                }
            }
        }
    }
}
