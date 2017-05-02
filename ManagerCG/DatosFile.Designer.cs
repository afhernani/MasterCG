namespace ManagerCG
{
    partial class DatosFile
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
            _instancia = null;
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lbfile = new System.Windows.Forms.Label();
            this.lbTime = new System.Windows.Forms.Label();
            this.numericUpDownRatio = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownFrames = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRatio)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFrames)).BeginInit();
            this.SuspendLayout();
            // 
            // lbfile
            // 
            this.lbfile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbfile.Location = new System.Drawing.Point(12, 9);
            this.lbfile.Name = "lbfile";
            this.lbfile.Size = new System.Drawing.Size(321, 56);
            this.lbfile.TabIndex = 0;
            this.lbfile.Text = "labelfile";
            // 
            // lbTime
            // 
            this.lbTime.AutoSize = true;
            this.lbTime.Location = new System.Drawing.Point(14, 70);
            this.lbTime.Name = "lbTime";
            this.lbTime.Size = new System.Drawing.Size(30, 13);
            this.lbTime.TabIndex = 0;
            this.lbTime.Text = "Time";
            // 
            // numericUpDownRatio
            // 
            this.numericUpDownRatio.Location = new System.Drawing.Point(264, 68);
            this.numericUpDownRatio.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownRatio.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownRatio.Name = "numericUpDownRatio";
            this.numericUpDownRatio.Size = new System.Drawing.Size(41, 20);
            this.numericUpDownRatio.TabIndex = 1;
            this.numericUpDownRatio.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownRatio.ValueChanged += new System.EventHandler(this.numericUpDownRatio_ValueChanged);
            // 
            // numericUpDownFrames
            // 
            this.numericUpDownFrames.Location = new System.Drawing.Point(123, 68);
            this.numericUpDownFrames.Minimum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numericUpDownFrames.Name = "numericUpDownFrames";
            this.numericUpDownFrames.Size = new System.Drawing.Size(59, 20);
            this.numericUpDownFrames.TabIndex = 1;
            this.numericUpDownFrames.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numericUpDownFrames.ValueChanged += new System.EventHandler(this.numericUpDownFrames_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(223, 70);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Ratio:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(73, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Frames:";
            // 
            // DatosFile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(343, 99);
            this.Controls.Add(this.numericUpDownFrames);
            this.Controls.Add(this.numericUpDownRatio);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbTime);
            this.Controls.Add(this.lbfile);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "DatosFile";
            this.Text = "DatosFile";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRatio)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFrames)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbfile;
        private System.Windows.Forms.Label lbTime;
        private System.Windows.Forms.NumericUpDown numericUpDownRatio;
        private System.Windows.Forms.NumericUpDown numericUpDownFrames;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}