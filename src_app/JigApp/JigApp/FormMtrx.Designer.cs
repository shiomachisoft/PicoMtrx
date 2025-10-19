namespace JigApp
{
    partial class FormMtrx
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
            this.button_ConvertMp4ToMtrx = new System.Windows.Forms.Button();
            this.button_OpenMtrxFile = new System.Windows.Forms.Button();
            this.label_ConvertProgress = new System.Windows.Forms.Label();
            this.progressBar_Convert = new System.Windows.Forms.ProgressBar();
            this.label_FileName = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button_ConvertMp4ToMtrx
            // 
            this.button_ConvertMp4ToMtrx.Location = new System.Drawing.Point(263, 49);
            this.button_ConvertMp4ToMtrx.Name = "button_ConvertMp4ToMtrx";
            this.button_ConvertMp4ToMtrx.Size = new System.Drawing.Size(316, 66);
            this.button_ConvertMp4ToMtrx.TabIndex = 51;
            this.button_ConvertMp4ToMtrx.TabStop = false;
            this.button_ConvertMp4ToMtrx.Text = "Convert mp4 to mtrx file";
            this.button_ConvertMp4ToMtrx.UseVisualStyleBackColor = true;
            this.button_ConvertMp4ToMtrx.Click += new System.EventHandler(this.button_ConvertMp4ToMtrxFile_Click);
            // 
            // button_OpenMtrxFile
            // 
            this.button_OpenMtrxFile.Location = new System.Drawing.Point(263, 271);
            this.button_OpenMtrxFile.Name = "button_OpenMtrxFile";
            this.button_OpenMtrxFile.Size = new System.Drawing.Size(316, 66);
            this.button_OpenMtrxFile.TabIndex = 52;
            this.button_OpenMtrxFile.TabStop = false;
            this.button_OpenMtrxFile.Text = "Open mtrx file";
            this.button_OpenMtrxFile.UseVisualStyleBackColor = true;
            this.button_OpenMtrxFile.Click += new System.EventHandler(this.button_OpenMtrxFile_Click);
            // 
            // label_ConvertProgress
            // 
            this.label_ConvertProgress.Location = new System.Drawing.Point(599, 211);
            this.label_ConvertProgress.Name = "label_ConvertProgress";
            this.label_ConvertProgress.Size = new System.Drawing.Size(218, 31);
            this.label_ConvertProgress.TabIndex = 54;
            this.label_ConvertProgress.Text = "---";
            this.label_ConvertProgress.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // progressBar_Convert
            // 
            this.progressBar_Convert.Location = new System.Drawing.Point(25, 171);
            this.progressBar_Convert.Name = "progressBar_Convert";
            this.progressBar_Convert.Size = new System.Drawing.Size(792, 34);
            this.progressBar_Convert.TabIndex = 55;
            // 
            // label_FileName
            // 
            this.label_FileName.Location = new System.Drawing.Point(24, 134);
            this.label_FileName.Name = "label_FileName";
            this.label_FileName.Size = new System.Drawing.Size(793, 34);
            this.label_FileName.TabIndex = 56;
            this.label_FileName.Text = "---";
            this.label_FileName.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // FormMtrx
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(842, 382);
            this.Controls.Add(this.label_FileName);
            this.Controls.Add(this.progressBar_Convert);
            this.Controls.Add(this.label_ConvertProgress);
            this.Controls.Add(this.button_OpenMtrxFile);
            this.Controls.Add(this.button_ConvertMp4ToMtrx);
            this.Name = "FormMtrx";
            this.Text = "MTRX";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_ConvertMp4ToMtrx;
        private System.Windows.Forms.Button button_OpenMtrxFile;
        private System.Windows.Forms.Label label_ConvertProgress;
        private System.Windows.Forms.ProgressBar progressBar_Convert;
        private System.Windows.Forms.Label label_FileName;
    }
}