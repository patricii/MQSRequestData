namespace MQSRequestData
{
    partial class ApkMQS
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
            this.buttonRun = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.metroTabControl1 = new MetroFramework.Controls.MetroTabControl();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxSave = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonRun
            // 
            this.buttonRun.Location = new System.Drawing.Point(24, 57);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(104, 31);
            this.buttonRun.TabIndex = 0;
            this.buttonRun.Text = "Get MQS Data";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(23, 103);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(46, 13);
            this.labelStatus.TabIndex = 1;
            this.labelStatus.Text = "Status...";
            // 
            // metroTabControl1
            // 
            this.metroTabControl1.Enabled = false;
            this.metroTabControl1.Location = new System.Drawing.Point(24, 119);
            this.metroTabControl1.Name = "metroTabControl1";
            this.metroTabControl1.Size = new System.Drawing.Size(360, 42);
            this.metroTabControl1.TabIndex = 2;
            this.metroTabControl1.UseSelectable = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(134, 75);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Save directory:";
            // 
            // textBoxSave
            // 
            this.textBoxSave.Location = new System.Drawing.Point(218, 68);
            this.textBoxSave.Name = "textBoxSave";
            this.textBoxSave.Size = new System.Drawing.Size(166, 20);
            this.textBoxSave.TabIndex = 4;
            this.textBoxSave.Text = "C:\\temp\\";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::MQSRequestData.Properties.Resources.FLEX_logo;
            this.pictureBox1.Location = new System.Drawing.Point(289, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(95, 50);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(3, 186);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "Developed by A. Patricio";
            // 
            // ApkMQS
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(406, 207);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.textBoxSave);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.metroTabControl1);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.buttonRun);
            this.Name = "ApkMQS";
            this.Text = "MQSRequestData";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonRun;
        private System.Windows.Forms.Label labelStatus;
        private MetroFramework.Controls.MetroTabControl metroTabControl1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxSave;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label2;
    }
}

