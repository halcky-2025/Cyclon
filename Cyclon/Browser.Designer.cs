namespace Cyclon
{
    partial class Browser
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
            text = new RichTextBox();
            console = new RichTextBox();
            textBox1 = new TextBox();
            button1 = new Button();
            body = new Panel();
            SuspendLayout();
            // 
            // text
            // 
            text.BackColor = SystemColors.Control;
            text.Location = new Point(14, 17);
            text.Name = "text";
            text.ReadOnly = true;
            text.Size = new Size(420, 305);
            text.TabIndex = 0;
            text.Text = "";
            // 
            // console
            // 
            console.BackColor = SystemColors.WindowText;
            console.ForeColor = SystemColors.Window;
            console.Location = new Point(451, 18);
            console.Name = "console";
            console.Size = new Size(337, 304);
            console.TabIndex = 1;
            console.Text = "";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(13, 341);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(675, 27);
            textBox1.TabIndex = 2;
            // 
            // button1
            // 
            button1.Location = new Point(694, 341);
            button1.Name = "button1";
            button1.Size = new Size(94, 29);
            button1.TabIndex = 3;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            // 
            // body
            // 
            body.Location = new Point(18, 400);
            body.Name = "body";
            body.Size = new Size(770, 503);
            body.TabIndex = 4;
            // 
            // Browser
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(807, 930);
            Controls.Add(body);
            Controls.Add(button1);
            Controls.Add(textBox1);
            Controls.Add(console);
            Controls.Add(text);
            Name = "Browser";
            Text = "Browser";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private RichTextBox text;
        private RichTextBox console;
        private TextBox textBox1;
        private Button button1;
        private Panel body;
    }
}