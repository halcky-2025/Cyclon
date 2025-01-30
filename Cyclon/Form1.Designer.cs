namespace Cyclon
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            console = new RichTextBox();
            button1 = new Button();
            button2 = new Button();
            panel = new Panel();
            vision = new Panel();
            SuspendLayout();
            // 
            // console
            // 
            console.BackColor = SystemColors.WindowText;
            console.ForeColor = SystemColors.Window;
            console.Location = new Point(582, 18);
            console.Margin = new Padding(2);
            console.Name = "console";
            console.Size = new Size(508, 413);
            console.TabIndex = 1;
            console.Text = "";
            // 
            // button1
            // 
            button1.Location = new Point(466, 468);
            button1.Margin = new Padding(2);
            button1.Name = "button1";
            button1.Size = new Size(90, 27);
            button1.TabIndex = 2;
            button1.Text = "exe";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(609, 469);
            button2.Name = "button2";
            button2.Size = new Size(94, 29);
            button2.TabIndex = 3;
            button2.Text = "browser";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // panel
            // 
            panel.Location = new Point(17, 28);
            panel.Name = "panel";
            panel.Size = new Size(551, 403);
            panel.TabIndex = 4;
            // 
            // vision
            // 
            vision.Location = new Point(1090, 19);
            vision.Name = "vision";
            vision.Size = new Size(583, 412);
            vision.TabIndex = 5;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1703, 575);
            Controls.Add(vision);
            Controls.Add(panel);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(console);
            Margin = new Padding(2);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
        }

        #endregion
        private Button button1;
        private Button button2;
        private Panel panel;
        public RichTextBox console;
        public Panel vision;
    }
}
