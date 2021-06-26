using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace MusicBeePlugin
{
	public partial class DevTokenForm : Form
	{
		public DevTokenForm()
		{
			InitializeComponent();
		}

        public void SaveButtonClick(object sender, EventArgs e)
        {
            // textBox2 is the Client ID
            // textBox1 is the ClientSecret
            if (string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("You need to enter a Client ID!", "Error");
                return;
            }
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("You need to enter a Client Secret!", "Error");
                return;
            }

            JObject jObj = new JObject();
            JProperty id = new JProperty("client_id", textBox2.Text);
            JProperty secret = new JProperty("client_secret", textBox1.Text);

            jObj.Add(id);
            jObj.Add(secret);

            string jsonData = jObj.ToString();
            File.WriteAllText(SpotifyAuthHandler.spotifyAuthPath, jsonData);

            Close();
        }
    }

    public partial class DevTokenForm
    {
        public Label label1;
        public Label label2;
        public Label label3;
        public TextBox textBox1;
        public TextBox textBox2;
        public Button button1;

        private void InitializeComponent()
        {
            this.label1 = new Label();
            this.label2 = new Label();
            this.label3 = new Label();
            this.textBox1 = new TextBox();
            this.textBox2 = new TextBox();
            this.button1 = new Button();

            #region Labels

            this.label1.Image = null;
            this.label1.Text = "You can receive developer tokens here\nhttps://developer.spotify.com/dashboard/app" +
                "lications";
            this.label1.Location = new Point(16, 8);
            this.label1.Name = "label1";
            this.label1.Size = new Size(320, 32);
            this.label1.TabIndex = 0;

            this.label2.Image = null;
            this.label2.Text = "Client Secret";
            this.label2.Location = new Point(16, 72);
            this.label2.Name = "label2";
            this.label2.Size = new Size(80, 18);
            this.label2.TabIndex = 2;

            this.label3.Image = null;
            this.label3.Text = "Client ID";
            this.label3.Location = new Point(16, 48);
            this.label3.Name = "label3";
            this.label3.Size = new Size(56, 18);
            this.label3.TabIndex = 2;

            #endregion

            #region Text Boxes

            this.textBox1.Text = "";
            this.textBox1.BackColor = SystemColors.Window;
            this.textBox1.ForeColor = SystemColors.WindowText;
            this.textBox1.Cursor = Cursors.IBeam;
            this.textBox1.Location = new Point(96, 72);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new Size(200, 20);
            this.textBox1.TabIndex = 1;

            this.textBox2.Text = "";
            this.textBox2.BackColor = SystemColors.Window;
            this.textBox2.ForeColor = SystemColors.WindowText;
            this.textBox2.Cursor = Cursors.IBeam;
            this.textBox2.Location = new Point(72, 48);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new Size(200, 20);
            this.textBox2.TabIndex = 1;

            #endregion

            // Save Button
            this.button1.ImeMode = ImeMode.Disable;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Location = new Point(256, 96);
            this.button1.Name = "button1";
            this.button1.Size = new Size(80, 24);
            this.button1.TabIndex = 3;
            this.button1.Click += SaveButtonClick;

            // Final Form Setup
            this.ClientSize = new Size(354, 130);
            this.Text = "Input Developer Tokens";
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.button1);
            this.Name = "DevTokenForm";
        }
    }
}
