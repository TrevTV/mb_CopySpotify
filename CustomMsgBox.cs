/************************************************************************************************
 *                                                                                              *
 * Author: Max Kleyzit.                                                                         *
 * Developed for StresStimulus, free Fiddler extension for load testing of web applications     *
 * http://stresstimulus.stimulustechnology.com/                                                 *
 *                                                                                              *
 ***********************************************************************************************/
 // modified by https://github.com/trevtv
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    /// <summary>
    /// A customizable Dialog box with 3 buttons, custom icon, and checkbox.
    /// </summary>
    partial class CustomMsgBox : Form
    {
        /// <summary>
        /// Create a new instance of the dialog box with a message and title.
        /// </summary>
        /// <param name="message">Message text.</param>
        /// <param name="title">Dialog Box title.</param>
        public CustomMsgBox(string message, string title)
            : this(message, title, MessageBoxIcon.None)
        {
            
        }

        /// <summary>
        /// Create a new instance of the dialog box with a message and title and a standard windows messagebox icon.
        /// </summary>
        /// <param name="message">Message text.</param>
        /// <param name="title">Dialog Box title.</param>
        /// <param name="icon">Standard system messagebox icon.</param>
        public CustomMsgBox(string message, string title, MessageBoxIcon icon)
            : this(message, title, GetMessageBoxIcon(icon))
        {

        }

        /// <summary>
        /// Create a new instance of the dialog box with a message and title and a custom windows icon.
        /// </summary>
        /// <param name="message">Message text.</param>
        /// <param name="title">Dialog Box title.</param>
        /// <param name="icon">Custom icon.</param>
        public CustomMsgBox(string message, string title, Icon icon)
        {
            InitializeComponent();

            this.messageLbl.Text = message;
            this.Text = title;

            this.m_sysIcon = icon;

            if (this.m_sysIcon == null)
                this.messageLbl.Location = new System.Drawing.Point(FORM_X_MARGIN, FORM_Y_MARGIN);
        }

        /// <summary>
        /// Get system icon for MessageBoxIcon.
        /// </summary>
        /// <param name="icon">The MessageBoxIcon value.</param>
        /// <returns>SystemIcon type Icon.</returns>
        static Icon GetMessageBoxIcon(MessageBoxIcon icon)
        {
            switch (icon)
            {
                case MessageBoxIcon.Asterisk:
                    return SystemIcons.Asterisk;
                case MessageBoxIcon.Error:
                    return SystemIcons.Error;
                case MessageBoxIcon.Exclamation:
                    return SystemIcons.Exclamation;
                case MessageBoxIcon.Question:
                    return SystemIcons.Question;
                default:
                    return null;
            }
        }

        #region Setup API

        /// <summary>
        /// Min set width.
        /// </summary>
        int m_minWidth;
        /// <summary>
        /// Min set height.
        /// </summary>
        int m_minHeight;

        /// <summary>
        /// Sets the min size of the dialog box. If the text or button row needs more size then the dialog box will size to fit the text.
        /// </summary>
        /// <param name="width">Min width value.</param>
        /// <param name="height">Min height value.</param>
        public void SetMinSize(int width, int height)
        {
            m_minWidth = width;
            m_minHeight = height;
        }

        /// <summary>
        /// Create up to 3 buttons with no DialogResult values.
        /// </summary>
        /// <param name="names">Array of button names. Must of length 1-3.</param>
        public void SetButtons(params string[] names)
        {
            DialogResult[] drs = new DialogResult[names.Length];
            for (int i = 0; i < names.Length; i++)
                drs[i] = DialogResult.None;
            this.SetButtons(names, drs);
        }

        /// <summary>
        /// Create up to 3 buttons with given DialogResult values.
        /// </summary>
        /// <param name="names">Array of button names. Must of length 1-3.</param>
        /// <param name="results">Array of DialogResult values. Must be same length as names.</param>
        public void SetButtons(string[] names, DialogResult[] results)
        {
            this.SetButtons(names, results, 1);
        }

        /// <summary>
        /// Create up to 3 buttons with given DialogResult values.
        /// </summary>
        /// <param name="names">Array of button names. Must of length 1-3.</param>
        /// <param name="results">Array of DialogResult values. Must be same length as names.</param>
        /// <param name="def">Default Button number. Must be 1-3.</param>
        public void SetButtons(string[] names, DialogResult[] results, int def)
        {
            if (names == null)
                throw new ArgumentNullException(nameof(names), "Button Text is null");

            int count = names.Length;

            if (count < 1 || count > 3)
                throw new ArgumentException("Invalid number of buttons. Must be between 1 and 3.");

            //---- Set Button 1
            m_minButtonRowWidth += SetButtonParams(btn1, names[0], def == 1 ? 1 : 2, results[0]);

            //---- Set Button 2
            if (count > 1)
            {
                m_minButtonRowWidth += SetButtonParams(btn2, names[1], def == 2 ? 1 : 3, results[1]) + BUTTON_SPACE;
            }

            //---- Set Button 3
            if (count > 2)
            {
                m_minButtonRowWidth += SetButtonParams(btn3, names[2], def == 3 ? 1 : 4, results[2]) + BUTTON_SPACE;
            }

        }

        /// <summary>
        /// The min required width of the button and checkbox row. Sum of button widths + checkbox width + margins.
        /// </summary>
        int m_minButtonRowWidth;

        /// <summary>
        /// Sets button text and returns the width.
        /// </summary>
        /// <param name="btn">Button object.</param>
        /// <param name="text">Text of the button.</param>
        /// <param name="tab">TabIndex of the button.</param>
        /// <param name="dr">DialogResult of the button.</param>
        /// <returns>Width of the button.</returns>
        static int SetButtonParams(Button btn, string text, int tab, DialogResult dr)
        {
            btn.Text = text;
            btn.Visible = true;
            btn.DialogResult = dr;
            btn.TabIndex = tab;
            return btn.Size.Width;
        }

        /// <summary>
        /// Enables the checkbox. By default the checkbox is unchecked.
        /// </summary>
        /// <param name="text">Text of the checkbox.</param>
        public void SetCheckbox(string text)
        {
            this.SetCheckbox(text, false);
        }
        
        /// <summary>
        /// Enables the checkbox and the default checked state.
        /// </summary>
        /// <param name="text">Text of the checkbox.</param>
        /// <param name="chcked">Default checked state of the box.</param>
        public void SetCheckbox(string text, bool chcked)
        {
            this.chkBx.Visible = true;
            this.chkBx.Text = text;
            this.chkBx.Checked = chcked;
            this.m_minButtonRowWidth += this.chkBx.Size.Width + CHECKBOX_SPACE;
        }

        #endregion

        #region Sizes and Locations
        private void DialogBox_Load(object sender, EventArgs e)
        {
            if (!btn1.Visible)
                this.SetButtons(new string[] { "OK" }, new DialogResult[] { DialogResult.OK });

            m_minButtonRowWidth += 2 * FORM_X_MARGIN; //add margin to the ends

            this.SetDialogSize();

            this.SetButtonRowLocations();

        }

        const int FORM_Y_MARGIN = 10;
        const int FORM_X_MARGIN = 16;
        const int BUTTON_SPACE = 5;
        const int CHECKBOX_SPACE = 15;
        const int TEXT_Y_MARGIN = 30;

        /// <summary>
        /// Auto fits the dialog box to fit the text and the buttons.
        /// </summary>
        void SetDialogSize()
        {
            int requiredWidth = this.messageLbl.Location.X + this.messageLbl.Size.Width + FORM_X_MARGIN;
            requiredWidth = requiredWidth > m_minButtonRowWidth ? requiredWidth : m_minButtonRowWidth;

            int requiredHeight = this.messageLbl.Location.Y + this.messageLbl.Size.Height - this.btn2.Location.Y + this.ClientSize.Height + TEXT_Y_MARGIN;

            int minSetWidth = this.ClientSize.Width > this.m_minWidth ? this.ClientSize.Width : this.m_minWidth;
            int minSetHeight = this.ClientSize.Height > this.m_minHeight ? this.ClientSize.Height : this.m_minHeight;

            Size s = new Size
            {
                Width = requiredWidth > minSetWidth ? requiredWidth : minSetWidth,
                Height = requiredHeight > minSetHeight ? requiredHeight : minSetHeight
            };
            this.ClientSize = s;
        }

        /// <summary>
        /// Sets the buttons and checkboxe location.
        /// </summary>
        void SetButtonRowLocations()
        {
            int formWidth = this.ClientRectangle.Width;

            int x = formWidth - FORM_X_MARGIN;
            int y = btn1.Location.Y;

            if (btn3.Visible)
            {
                x -= btn3.Size.Width;
                btn3.Location = new Point(x, y);
                x -= BUTTON_SPACE;
            }

            if (btn2.Visible)
            {
                x -= btn2.Size.Width;
                btn2.Location = new Point(x, y);
                x -= BUTTON_SPACE;
            }

            x -= btn1.Size.Width;
            btn1.Location = new Point(x, y);

            if (this.chkBx.Visible)
                this.chkBx.Location = new Point(FORM_X_MARGIN, this.chkBx.Location.Y);

        }
        
        #endregion

        #region Icon Pain
        /// <summary>
        /// The icon to paint.
        /// </summary>
        Icon m_sysIcon;

        /// <summary>
        /// Paint the System Icon in the top left corner.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (m_sysIcon != null)
            {
                Graphics g = e.Graphics;
                g.DrawIconUnstretched(m_sysIcon, new Rectangle(FORM_X_MARGIN, FORM_Y_MARGIN, m_sysIcon.Width, m_sysIcon.Height));
            }

            base.OnPaint(e);
        }
        #endregion

        #region Result API

        /// <summary>
        /// If visible checkbox was checked.
        /// </summary>
        public bool CheckboxChecked
        {
            get
            {
                return this.chkBx.Checked;
            }
        }

        DialogBoxResult m_result;
        /// <summary>
        /// Gets the button that was pressed.
        /// </summary>
        public DialogBoxResult DialogBoxResult
        {
            get
            {
                return m_result;
            }
        }

        private void ButtonClick(object sender, EventArgs e)
        {
            if (sender == btn1)
                this.m_result = DialogBoxResult.Button1;
            else if (sender == btn2)
                this.m_result = DialogBoxResult.Button2;
            else if (sender == btn3)
                this.m_result = DialogBoxResult.Button3;

            if (((Button)sender).DialogResult == DialogResult.None)
                this.Close();
        }

        #endregion
    }

    partial class CustomMsgBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CustomMsgBox));
            this.chkBx = new System.Windows.Forms.CheckBox();
            this.btn1 = new System.Windows.Forms.Button();
            this.btn2 = new System.Windows.Forms.Button();
            this.messageLbl = new System.Windows.Forms.Label();
            this.btn3 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // chkBx
            // 
            this.chkBx.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkBx.AutoSize = true;
            this.chkBx.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkBx.Location = new System.Drawing.Point(12, 114);
            this.chkBx.Name = "chkBx";
            this.chkBx.Size = new System.Drawing.Size(152, 20);
            this.chkBx.TabIndex = 22;
            this.chkBx.Text = "Don\'t show this again";
            this.chkBx.UseVisualStyleBackColor = true;
            this.chkBx.Visible = false;
            // 
            // btn1
            // 
            this.btn1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn1.AutoSize = true;
            this.btn1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn1.Location = new System.Drawing.Point(236, 114);
            this.btn1.Name = "btn1";
            this.btn1.Size = new System.Drawing.Size(75, 23);
            this.btn1.TabIndex = 5;
            this.btn1.Text = "Button1";
            this.btn1.UseVisualStyleBackColor = true;
            this.btn1.Visible = false;
            this.btn1.Click += new System.EventHandler(this.ButtonClick);
            // 
            // btn2
            // 
            this.btn2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn2.AutoSize = true;
            this.btn2.Location = new System.Drawing.Point(317, 114);
            this.btn2.Name = "btn2";
            this.btn2.Size = new System.Drawing.Size(75, 23);
            this.btn2.TabIndex = 6;
            this.btn2.Text = "Button2";
            this.btn2.UseVisualStyleBackColor = true;
            this.btn2.Visible = false;
            this.btn2.Click += new System.EventHandler(this.ButtonClick);
            // 
            // messageLbl
            // 
            this.messageLbl.AutoSize = true;
            this.messageLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.messageLbl.Location = new System.Drawing.Point(58, 10);
            this.messageLbl.Name = "messageLbl";
            this.messageLbl.Size = new System.Drawing.Size(73, 16);
            this.messageLbl.TabIndex = 19;
            this.messageLbl.Text = "[Message]";
            // 
            // btn3
            // 
            this.btn3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn3.AutoSize = true;
            this.btn3.Location = new System.Drawing.Point(398, 114);
            this.btn3.Name = "btn3";
            this.btn3.Size = new System.Drawing.Size(75, 23);
            this.btn3.TabIndex = 7;
            this.btn3.Text = "Button3";
            this.btn3.UseVisualStyleBackColor = true;
            this.btn3.Visible = false;
            this.btn3.Click += new System.EventHandler(this.ButtonClick);
            // 
            // DialogBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.btn1;
            this.ClientSize = new System.Drawing.Size(485, 149);
            this.ControlBox = false;
            this.Controls.Add(this.btn3);
            this.Controls.Add(this.chkBx);
            this.Controls.Add(this.btn1);
            this.Controls.Add(this.btn2);
            this.Controls.Add(this.messageLbl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = System.Drawing.SystemIcons.Information;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DialogBox";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "[Title]";
            this.Load += new System.EventHandler(this.DialogBox_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkBx;
        private System.Windows.Forms.Button btn1;
        private System.Windows.Forms.Button btn2;
        private System.Windows.Forms.Label messageLbl;
        private System.Windows.Forms.Button btn3;
    }

    enum DialogBoxResult
    {
        Button1,
        Button2,
        Button3
    }
}
