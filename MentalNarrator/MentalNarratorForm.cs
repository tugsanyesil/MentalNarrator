using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace MentalNarrator
{
    public partial class MentalNarratorForm : Form
    {
        public Account Account;

        public MentalNarratorForm(Account Account) 
        {
            AllowDrop = true;
            InitializeComponent();

            Controls.AddRange(AppData.Load(this.Account = Account).ToArray());
        }

        private void MentalNarratorForm_Load(object sender, EventArgs e)
        {
            Random = new Random();
        }

        Random Random;
        private void AddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                List<int> Randoms = this.Controls.OfType<Narration>().Where(Narration => Narration.TitleBox.Text.StartsWith("New Title ")).ToList().ConvertAll(Narration => Int32.TryParse(Narration.TitleBox.Text.Substring(("New Title ").Count()), out int ret) ? ret : 0);

                int r = 0;
                while (Randoms.Contains(r = Random.Next())) ;
                Directory.CreateDirectory(Path.Combine("Appdata", Account.Username, "New Title " + r));
                Controls.Add(new Narration("New Title " + r, "Write Something", new Point(Cursor.Position.X - Location.X, Cursor.Position.Y - Location.Y), Static.BoxSize));
            }
            catch { }
        }

        private void MentalNarratorForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Save?", "Closing...", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    AppData.Save(Account, Controls.OfType<Narration>().ToList());
                }
                else if (MessageBox.Show("Sure?", "Closing...", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    MentalNarratorForm_FormClosed(new object(), new FormClosedEventArgs(CloseReason.ApplicationExitCall));
                }
                else if (MessageBox.Show("Really?", "Closing...", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    MentalNarratorForm_FormClosed(new object(), new FormClosedEventArgs(CloseReason.ApplicationExitCall));
                }
                else if (MessageBox.Show("Think About It", "Closing...", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    MentalNarratorForm_FormClosed(new object(), new FormClosedEventArgs(CloseReason.ApplicationExitCall));
                }
            } catch { }
        }
    }
}