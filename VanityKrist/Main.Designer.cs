namespace VanityKrist
{
    partial class Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.label1 = new System.Windows.Forms.Label();
            this.Start = new System.Windows.Forms.Button();
            this.Term = new System.Windows.Forms.TextBox();
            this.Output = new System.Windows.Forms.RichTextBox();
            this.Threads = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Numbers = new System.Windows.Forms.CheckBox();
            this.Stop = new System.Windows.Forms.Button();
            this.Clear = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(143, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Term";
            // 
            // Start
            // 
            this.Start.AutoSize = true;
            this.Start.Location = new System.Drawing.Point(12, 74);
            this.Start.Name = "Start";
            this.Start.Size = new System.Drawing.Size(301, 23);
            this.Start.TabIndex = 1;
            this.Start.Text = "Start";
            this.Start.UseVisualStyleBackColor = true;
            this.Start.Click += new System.EventHandler(this.Start_Click);
            // 
            // Term
            // 
            this.Term.Location = new System.Drawing.Point(12, 25);
            this.Term.MaxLength = 10;
            this.Term.Name = "Term";
            this.Term.Size = new System.Drawing.Size(301, 20);
            this.Term.TabIndex = 3;
            this.Term.TextChanged += new System.EventHandler(this.Term_TextChanged);
            // 
            // Output
            // 
            this.Output.AutoSize = true;
            this.Output.Location = new System.Drawing.Point(12, 103);
            this.Output.Name = "Output";
            this.Output.ReadOnly = true;
            this.Output.Size = new System.Drawing.Size(301, 284);
            this.Output.TabIndex = 4;
            this.Output.Text = "";
            // 
            // Threads
            // 
            this.Threads.Location = new System.Drawing.Point(175, 48);
            this.Threads.MaxLength = 2;
            this.Threads.Name = "Threads";
            this.Threads.Size = new System.Drawing.Size(22, 20);
            this.Threads.TabIndex = 5;
            this.Threads.Text = "4";
            this.Threads.TextChanged += new System.EventHandler(this.Threads_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(120, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Threads:";
            // 
            // Numbers
            // 
            this.Numbers.AutoSize = true;
            this.Numbers.Location = new System.Drawing.Point(12, 51);
            this.Numbers.Name = "Numbers";
            this.Numbers.Size = new System.Drawing.Size(74, 17);
            this.Numbers.TabIndex = 7;
            this.Numbers.Text = "Numbers?";
            this.Numbers.UseVisualStyleBackColor = true;
            this.Numbers.CheckedChanged += new System.EventHandler(this.Numbers_CheckedChanged);
            // 
            // Stop
            // 
            this.Stop.Location = new System.Drawing.Point(238, 48);
            this.Stop.Name = "Stop";
            this.Stop.Size = new System.Drawing.Size(75, 21);
            this.Stop.TabIndex = 8;
            this.Stop.Text = "Stop";
            this.Stop.UseVisualStyleBackColor = true;
            this.Stop.Click += new System.EventHandler(this.Stop_Click);
            // 
            // Clear
            // 
            this.Clear.Location = new System.Drawing.Point(122, 396);
            this.Clear.Name = "Clear";
            this.Clear.Size = new System.Drawing.Size(75, 23);
            this.Clear.TabIndex = 9;
            this.Clear.Text = "Clear";
            this.Clear.UseVisualStyleBackColor = true;
            this.Clear.Click += new System.EventHandler(this.Clear_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(325, 431);
            this.Controls.Add(this.Clear);
            this.Controls.Add(this.Stop);
            this.Controls.Add(this.Numbers);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Threads);
            this.Controls.Add(this.Output);
            this.Controls.Add(this.Term);
            this.Controls.Add(this.Start);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Main";
            this.Text = "VanityKrist";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Start;
        private System.Windows.Forms.TextBox Term;
        private System.Windows.Forms.RichTextBox Output;
        private System.Windows.Forms.TextBox Threads;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox Numbers;
        private System.Windows.Forms.Button Stop;
        private System.Windows.Forms.Button Clear;
    }
}

