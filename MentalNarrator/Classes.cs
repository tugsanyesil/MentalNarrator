using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace MentalNarrator
{
    public static class AppData
    {
        public static bool IsAccessValid(string Username, string Password)
        {
            try { return File.ReadAllText(Path.Combine("Appdata", Username, "Password")) == Password; }
            catch { return false; }
        }
        public static string[] Users => Directory.GetDirectories(Directory.CreateDirectory("Appdata").FullName);

        public static void Save(Account Account, List<Narration> Narrations)
        {
            Narrations.ForEach(Narration =>
            {
                try
                {
                    var Root = Path.Combine("AppData", Account.Username, Narration.TitleBox.Text);
                    File.WriteAllText(Path.Combine(Root, "Text"), Narration.NoteBox.Text);
                    File.WriteAllLines(Path.Combine(Root, "Documents"), Narration.Documents.ToArray());
                    using (TextWriter TextWriter = File.CreateText(Path.Combine(Root, "Settings")))
                    {
                        new XmlSerializer(typeof(object[]), new Type[] { typeof(Point), typeof(Size), typeof(List<string>) }).Serialize(TextWriter, new object[] { Narration.Location, Narration.Size });
                    }
                }
                catch { }
            });
        }

        public static List<Narration> Load(Account Account)
        {
            List<Narration> Narrations = new List<Narration>();

            try
            {
                Directory.GetDirectories(Path.Combine("Appdata", Account.Username)).ToList().ForEach(Narration =>
                {
                    var SettingsFileName = Path.Combine(Narration, "Settings");
                    if (File.Exists(SettingsFileName))
                    {
                        using (TextReader TextReader = File.OpenText(SettingsFileName))
                        {
                            object[] os;
                            try
                            {
                                os = new XmlSerializer(typeof(object[]), new Type[] { typeof(Point), typeof(Size) }).Deserialize(TextReader) as object[];
                            }
                            catch
                            {
                                os = new object[] { Point.Empty, Size.Empty };
                            }

                            new List<string> { "Text", "Documents" }.ForEach(s =>
                            {
                                if (!File.Exists(Path.Combine(Narration, s))) { File.Create(Path.Combine(Narration, s)).Close(); }
                            });
                            var s1 = File.ReadAllText(Path.Combine(Narration, "Text"));
                            var s2 = File.ReadAllLines(Path.Combine(Narration, "Documents"));
                            var n = new Narration(Path.GetFileName(Narration), s1, (Point)os[0], (Size)os[1], s2);
                            Narrations.Add(n);
                        }
                    }
                });
            }
            catch { }
            return Narrations;
        }
    }

    public class Narration : Panel
    {
        public CDL TitleBox;
        public RichTextBox NoteBox;
        public List<string> Documents;
        public PictureBox DocumentBox;

        public List<Narration> Parents;
        public List<Narration> Childs;

        public Narration() : this("", "", Point.Empty, Size.Empty) { }
        public Narration(string Title, string Note) : this(Title, Note, Point.Empty, Size.Empty) { }
        public Narration(string Title, string Note, Point Location, Size Size) : this(Title, Note, Point.Empty, Size.Empty, new string[] { }) { }
        public Narration(string Title, string Note, Point Location, Size Size, string[] Documents)
        {
            TitleBox = new CDL(Color.Chocolate, Title, Color.Black, true)
            {
                Location = Point.Empty,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.None,
                BorderStyle = BorderStyle.None
            };
            TitleBox.DraggredControl = this;
            TitleBox.EditBox.KeyPress += TitleBox_KeyPress;

            this.Documents = Documents.ToList();
            Static.Set(out DocumentBox, @"document.ico", new Point(Static.Gap, TitleBox.Size.Height), new Size(24, 24));

            Static.Set(out NoteBox, Note, new Point(Static.Gap, TitleBox.Size.Height + DocumentBox.Size.Height), Size.Empty, Color.Wheat, BorderStyle.None, 10f);

            Static.Set(out ContextMenuStrip Menu, new List<string> { "Delete" });
            this.ContextMenuStrip = Menu;
            this.ContextMenuStrip.ItemClicked += Menu_ItemClicked;

            Parents = new List<Narration>();
            Childs = new List<Narration>();

            Controls.AddRange(new Control[] { TitleBox, NoteBox, DocumentBox });

            this.BackColor = Color.BurlyWood;
            this.BorderStyle = BorderStyle.None;
            this.Location = Location;
            this.Size = Size;
            this.AllowDrop = true;

            MouseDown += Narration_MouseDown;
            MouseUp += Narration_MouseUp;
            MouseMove += Narration_MouseMove;
            DragEnter += Narration_DragEnter;
            DragDrop += Narration_DragDrop;
            DocumentBox.MouseDoubleClick += DocumentBox_MouseDoubleClick;

            this.Narration_Resize(new object(), new MouseEventArgs(MouseButtons.Left, 1, Size.Width, Size.Height, 0));
            this.TitleBox.Label_Drag(new object(), new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
        }

        private void DocumentBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            new DocumentForm(this.Documents.ToArray()).Show();
        }

        private void Narration_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void Narration_DragDrop(object sender, DragEventArgs e)
        {
            Documents.AddRange((string[])e.Data.GetData(DataFormats.FileDrop));
        }

        private void Menu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Name)
            {
                case "Delete":
                    {
                        Directory.Delete(Path.Combine("Appdata", ((MentalNarratorForm)Parent).Account.Username, TitleBox.Text), true);
                        Parent.Controls.Remove(this);

                        this.Dispose();
                    }
                    break;
            }
        }

        private void Narration_MouseDown(object sender, MouseEventArgs e)
        {
            if (this.Cursor == Cursors.Hand)
            {
                this.MouseMove += Narration_Resize;
            }
        }

        private void Narration_MouseUp(object sender, MouseEventArgs e)
        {
            this.MouseMove -= Narration_Resize;
        }

        private void Narration_MouseMove(object sender, MouseEventArgs e)
        {
            this.Cursor = (e.X.IsBetween(this.Size.Width - Static.Gap, this.Size.Width) && e.Y.IsBetween(this.Size.Height - Static.Gap, this.Size.Height)) ? Cursors.Hand : Cursors.Default;
        }

        private void Narration_Resize(object sender, MouseEventArgs e)
        {
            this.Size = new Size(Math.Max(e.Location.X, this.TitleBox.Size.Width + 2 * Static.Gap), Math.Max(e.Location.Y, this.TitleBox.Size.Height));
            TitleBox.Location = new Point((this.Size.Width - TitleBox.Size.Width) / 2, 0);
            NoteBox.Size = new Size(this.Size.Width - Static.Gap - NoteBox.Location.X, this.Size.Height - Static.Gap - NoteBox.Location.Y);
        }

        private void TitleBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            TitleBox.Location = new Point((this.Size.Width - TitleBox.Size.Width) / 2, 0);

            if (e.KeyChar == Convert.ToInt16(Keys.Enter))
            {
                string Root = Path.Combine("Appdata", ((MentalNarratorForm)Parent).Account.Username);
                try
                {
                    Directory.Move(Path.Combine(Root, TitleBox.EditBox.Text), Path.Combine(Root, TitleBox.Text));
                }
                catch (Exception exception) { MessageBox.Show(exception.ToString()); }
            }
        }
    }

    public class Account
    {
        public string Username;
        public string Password;

        public Account() : this("", "") { }

        public Account(string Username, string Password)
        {
            Directory.CreateDirectory(Path.Combine("Appdata", this.Username = Username));
            File.WriteAllText(Path.Combine("Appdata", this.Username, "Password"), this.Password = Password);
        }
    }

    public class DL : Label//DraggableLabel
    {
        public Control DraggredControl;
        public bool IsDraggable
        {
            get { return isDraggable; }
            set
            {
                isDraggable = value;
                if (isDraggable) { Drag += Label_Drag; }
                else { Drag -= Label_Drag; }
            }
        }
        bool isDraggable;
        public MouseEventHandler Drag;
        public Point MouseOldPosition;

        public DL(Color backColor, string text, Color foreColor, bool isDraggable)
        {
            AutoSize = true;
            BorderStyle = BorderStyle.None;
            Font = new Font(Font.FontFamily, 10f);

            BackColor = backColor;
            Text = text;
            ForeColor = foreColor;

            Margin = new Padding(2);
            Padding = new Padding(2);

            Cursor = Cursors.Hand;
            MouseDown += Label_MouseDown;
            MouseUp += Label_MouseUp;
            DraggredControl = this;
            IsDraggable = isDraggable;
        }

        private void Label_MouseDown(object sender, MouseEventArgs e)
        {
            MouseOldPosition = e.Location;
            MouseMove += Drag;
        }

        private void Label_MouseUp(object sender, MouseEventArgs e)
        {
            MouseMove -= Drag;
        }

        public void Label_Drag(object sender, MouseEventArgs e)
        {
            Point l = new Point(DraggredControl.Location.X + e.Location.X - MouseOldPosition.X, DraggredControl.Location.Y + e.Location.Y - MouseOldPosition.Y);
            DraggredControl.Location = new Point(l.X >= 0 ? l.X : 0, l.Y >= 0 ? l.Y : 0);
        }
    }

    public class CDL : DL//ChangeableDraggableLabel
    {
        public TextBox EditBox;

        public CDL(Color BackColor, string Text, Color ForeColor, bool isDraggable) : base(BackColor, Text, ForeColor, isDraggable)
        {
            Static.Set(out ContextMenuStrip LabelMenu, new List<string> { "Change" });
            ContextMenuStrip = LabelMenu;
            ContextMenuStrip.ItemClicked += LabelMenu_ItemClicked;

            Static.Set(out EditBox, Point.Empty, new Size(0, 0), Color.CornflowerBlue, BorderStyle.None, 10f);
            EditBox.KeyPress += TextBox_KeyPress;
            EditBox.Hide();

            Font = new Font(FontFamily.GenericSansSerif, 10f);

            Controls.Add(EditBox);
        }

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToInt16(Keys.Enter))
            {
                EditBox.Hide();

                string oldText = Text;
                Text = EditBox.Text;
                EditBox.Text = oldText;
                EditBox.Size = Size;
            }
        }

        private void LabelMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Name)
            {
                case "Change":
                    {
                        EditBox.Show();
                        EditBox.Size = Size;
                        EditBox.Text = Text;
                    }
                    break;
            }
        }
    }

    public static class Static
    {
        public static int Gap = 10;
        public static Size BoxSize = new Size(250, 100);

        public static bool IsBetween(this int P, int P1, int P2)
        {
            return P1 < P && P < P2;
        }

        public static void Set(out TextBox TB, Point p, Size s, Color c, BorderStyle bs, float FontSize)
        {
            TB = new TextBox()
            {
                BackColor = c,
                BorderStyle = bs,
                Font = new Font(FontFamily.GenericSansSerif, FontSize),
                Location = p,
                Size = s,
                Text = ""
            };
        }

        public static void Set(out RichTextBox RTB, string t, Point p, Size s, Color c, BorderStyle bs, float FontSize)
        {
            RTB = new RichTextBox()
            {
                BackColor = c,
                BorderStyle = bs,
                Font = new Font(FontFamily.GenericSansSerif, FontSize),
                Location = p,
                Size = s,
                Text = t
            };
        }

        public static void Set(out ContextMenuStrip CMS, List<string> Items)
        {
            CMS = new ContextMenuStrip()
            {
                ShowCheckMargin = false,
                ShowImageMargin = false,
            };
            foreach (string Item in Items)
            {
                CMS.Items.Add(new ToolStripMenuItem()
                {
                    Name = Item,
                    Size = new Size(150, 20),
                    Text = Item
                });
            }

            CMS.Size = new Size(150, 25);
            CMS.ResumeLayout(false);
        }

        public static void Set(out PictureBox P, string file, Point p, Size s)
        {
            P = new PictureBox
            {
                Location = p,
                Size = s,
            };

            try
            {
                P.Image = new Bitmap(Image.FromFile(file), s);
            }
            catch
            {
                P.BackColor = Color.Black;
            }
        }
    }
}