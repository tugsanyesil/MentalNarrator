using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MentalNarrator
{
    public partial class LogInForm : Form
    {
        public LogInForm()
        {
            InitializeComponent();
        }

        private void LogInForm_Load(object sender, EventArgs e)
        {

        }

        private void LogInButton_Click(object sender, EventArgs e)
        {
            if (UsernameTextBox.Text != "" && PasswordTextBox.Text != "") 
            {
                if (AppData.Users.ToList().TrueForAll(u => u != UsernameTextBox.Text))
                {
                    this.Hide();
                    var MentalNarratorForm = new MentalNarratorForm(new Account(UsernameTextBox.Text, PasswordTextBox.Text));
                    MentalNarratorForm.FormClosed += MentalNarratorForm_FormClosed;
                    MentalNarratorForm.Show();
                }
                else
                {
                    MessageBox.Show("This Username Is being Used Anyway");
                }
            }
            else
            {
                MessageBox.Show("Invalid Username And Password");
            }
           
        }

        private void SignInButton_Click(object sender, EventArgs e)
        {
            if (AppData.IsAccessValid(UsernameTextBox.Text, PasswordTextBox.Text))
            {
                this.Hide();
                var MentalNarratorForm = new MentalNarratorForm(new Account(UsernameTextBox.Text, PasswordTextBox.Text));
                MentalNarratorForm.FormClosed += MentalNarratorForm_FormClosed;
                MentalNarratorForm.Show();
            }
            else
            {
                MessageBox.Show("This Account Does Not Exist");
            }
        }

        private void MentalNarratorForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Close();
        }
    }
}
