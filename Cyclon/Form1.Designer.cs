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
            button3 = new Button();
            button4 = new Button();
            button5 = new Button();
            button6 = new Button();
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
            // button3
            // 
            button3.Location = new Point(751, 471);
            button3.Name = "button3";
            button3.Size = new Size(94, 29);
            button3.TabIndex = 6;
            button3.Text = "Clear";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // button4
            // 
            button4.Location = new Point(896, 473);
            button4.Name = "button4";
            button4.Size = new Size(94, 29);
            button4.TabIndex = 7;
            button4.Text = "toText";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // button5
            // 
            button5.Location = new Point(1045, 476);
            button5.Name = "button5";
            button5.Size = new Size(94, 29);
            button5.TabIndex = 8;
            button5.Text = "toVision";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // button6
            // 
            button6.Location = new Point(341, 469);
            button6.Name = "button6";
            button6.Size = new Size(94, 29);
            button6.TabIndex = 9;
            button6.Text = "exeB";
            button6.UseVisualStyleBackColor = true;
            button6.Click += button6_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1703, 575);
            Controls.Add(button6);
            Controls.Add(button5);
            Controls.Add(button4);
            Controls.Add(button3);
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
        private Button button3;
        private Button button4;
        private Button button5;
        private Button button6;
    }
}
