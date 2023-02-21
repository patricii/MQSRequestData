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
            this.SuspendLayout();
            // 
            // buttonRun
            // 
            this.buttonRun.Location = new System.Drawing.Point(223, 24);
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
            this.labelStatus.Location = new System.Drawing.Point(23, 71);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(52, 13);
            this.labelStatus.TabIndex = 1;
            this.labelStatus.Text = "Status.....";
            // 
            // metroTabControl1
            // 
            this.metroTabControl1.Location = new System.Drawing.Point(24, 103);
            this.metroTabControl1.Name = "metroTabControl1";
            this.metroTabControl1.Size = new System.Drawing.Size(1024, 510);
            this.metroTabControl1.TabIndex = 2;
            this.metroTabControl1.UseSelectable = true;
            // 
            // ApkMQS
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1071, 645);
            this.Controls.Add(this.metroTabControl1);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.buttonRun);
            this.Name = "ApkMQS";
            this.Text = "MQSRequestData";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonRun;
        private System.Windows.Forms.Label labelStatus;
        private MetroFramework.Controls.MetroTabControl metroTabControl1;
    }
}

