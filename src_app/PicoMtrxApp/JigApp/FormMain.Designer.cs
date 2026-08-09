namespace JigApp
{
    partial class FormMain
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.label_BoardId = new System.Windows.Forms.Label();
            this.button_Connect = new System.Windows.Forms.Button();
            this.comboBox_Port = new System.Windows.Forms.ComboBox();
            this.label_FwName = new System.Windows.Forms.Label();
            this.label_FwVer = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label_ConnectStatus = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label_AppVer = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label_AppName = new System.Windows.Forms.Label();
            this.textBox_AppLog = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.button_ClearAppLog = new System.Windows.Forms.Button();
            this.groupBox_Mtrx = new System.Windows.Forms.GroupBox();
            this.button_ConvertMp4ToMtrx = new System.Windows.Forms.Button();
            this.button_OpenMtrxFile = new System.Windows.Forms.Button();
            this.label_ConvertProgress = new System.Windows.Forms.Label();
            this.progressBar_Convert = new System.Windows.Forms.ProgressBar();
            this.label_FileName = new System.Windows.Forms.Label();
            this.label_CropMode = new System.Windows.Forms.Label();
            this.comboBox_CropMode = new System.Windows.Forms.ComboBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox_Mtrx.SuspendLayout();
            this.SuspendLayout();
            // 
            // label_BoardId
            // 
            this.label_BoardId.AutoSize = true;
            this.label_BoardId.Location = new System.Drawing.Point(219, 157);
            this.label_BoardId.Name = "label_BoardId";
            this.label_BoardId.Size = new System.Drawing.Size(43, 21);
            this.label_BoardId.TabIndex = 11;
            this.label_BoardId.Text = "---";
            // 
            // button_Connect
            // 
            this.button_Connect.Location = new System.Drawing.Point(254, 105);
            this.button_Connect.Name = "button_Connect";
            this.button_Connect.Size = new System.Drawing.Size(170, 50);
            this.button_Connect.TabIndex = 10;
            this.button_Connect.TabStop = false;
            this.button_Connect.Text = "connect";
            this.button_Connect.UseVisualStyleBackColor = true;
            this.button_Connect.Click += new System.EventHandler(this.button_Connect_Click);
            // 
            // comboBox_Port
            // 
            this.comboBox_Port.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Port.FormattingEnabled = true;
            this.comboBox_Port.Location = new System.Drawing.Point(46, 54);
            this.comboBox_Port.MaxDropDownItems = 100;
            this.comboBox_Port.Name = "comboBox_Port";
            this.comboBox_Port.Size = new System.Drawing.Size(154, 29);
            this.comboBox_Port.TabIndex = 9;
            this.comboBox_Port.TabStop = false;
            // 
            // label_FwName
            // 
            this.label_FwName.AutoSize = true;
            this.label_FwName.Location = new System.Drawing.Point(219, 94);
            this.label_FwName.Name = "label_FwName";
            this.label_FwName.Size = new System.Drawing.Size(43, 21);
            this.label_FwName.TabIndex = 29;
            this.label_FwName.Text = "---";
            // 
            // label_FwVer
            // 
            this.label_FwVer.AutoSize = true;
            this.label_FwVer.Location = new System.Drawing.Point(219, 125);
            this.label_FwVer.Name = "label_FwVer";
            this.label_FwVer.Size = new System.Drawing.Size(43, 21);
            this.label_FwVer.TabIndex = 30;
            this.label_FwVer.Text = "---";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label_ConnectStatus);
            this.groupBox1.Controls.Add(this.button_Connect);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.comboBox_Port);
            this.groupBox1.Location = new System.Drawing.Point(14, 7);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(477, 180);
            this.groupBox1.TabIndex = 31;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Connect";
            // 
            // label_ConnectStatus
            // 
            this.label_ConnectStatus.BackColor = System.Drawing.SystemColors.Control;
            this.label_ConnectStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_ConnectStatus.Font = new System.Drawing.Font("ＭＳ ゴシック", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label_ConnectStatus.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label_ConnectStatus.Location = new System.Drawing.Point(46, 105);
            this.label_ConnectStatus.Name = "label_ConnectStatus";
            this.label_ConnectStatus.Size = new System.Drawing.Size(154, 50);
            this.label_ConnectStatus.TabIndex = 39;
            this.label_ConnectStatus.Text = "disconnected";
            this.label_ConnectStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(42, 30);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(109, 21);
            this.label4.TabIndex = 32;
            this.label4.Text = "COM Port:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label_AppVer);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label_FwVer);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.label_AppName);
            this.groupBox2.Controls.Add(this.label_FwName);
            this.groupBox2.Controls.Add(this.label_BoardId);
            this.groupBox2.Location = new System.Drawing.Point(14, 200);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(477, 193);
            this.groupBox2.TabIndex = 32;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "App/FW Information";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 63);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(142, 21);
            this.label7.TabIndex = 39;
            this.label7.Text = "App Version:";
            // 
            // label_AppVer
            // 
            this.label_AppVer.AutoSize = true;
            this.label_AppVer.Location = new System.Drawing.Point(219, 61);
            this.label_AppVer.Name = "label_AppVer";
            this.label_AppVer.Size = new System.Drawing.Size(43, 21);
            this.label_AppVer.TabIndex = 38;
            this.label_AppVer.Text = "---";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 32);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(109, 21);
            this.label5.TabIndex = 38;
            this.label5.Text = "App Name:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 157);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(186, 21);
            this.label3.TabIndex = 35;
            this.label3.Text = "Unique Board ID:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 127);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(131, 21);
            this.label2.TabIndex = 34;
            this.label2.Text = "FW Version:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 94);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 21);
            this.label1.TabIndex = 33;
            this.label1.Text = "FW Name:";
            // 
            // label_AppName
            // 
            this.label_AppName.AutoSize = true;
            this.label_AppName.Location = new System.Drawing.Point(219, 32);
            this.label_AppName.Name = "label_AppName";
            this.label_AppName.Size = new System.Drawing.Size(43, 21);
            this.label_AppName.TabIndex = 32;
            this.label_AppName.Text = "---";
            // 
            // textBox_AppLog
            // 
            this.textBox_AppLog.Location = new System.Drawing.Point(508, 51);
            this.textBox_AppLog.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_AppLog.Multiline = true;
            this.textBox_AppLog.Name = "textBox_AppLog";
            this.textBox_AppLog.ReadOnly = true;
            this.textBox_AppLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_AppLog.Size = new System.Drawing.Size(309, 254);
            this.textBox_AppLog.TabIndex = 39;
            this.textBox_AppLog.TabStop = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(504, 18);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(98, 21);
            this.label9.TabIndex = 40;
            this.label9.Text = "App Log:";
            // 
            // button_ClearAppLog
            // 
            this.button_ClearAppLog.Location = new System.Drawing.Point(699, 313);
            this.button_ClearAppLog.Name = "button_ClearAppLog";
            this.button_ClearAppLog.Size = new System.Drawing.Size(121, 50);
            this.button_ClearAppLog.TabIndex = 41;
            this.button_ClearAppLog.TabStop = false;
            this.button_ClearAppLog.Text = "clear";
            this.button_ClearAppLog.UseVisualStyleBackColor = true;
            this.button_ClearAppLog.Click += new System.EventHandler(this.button_ClearAppLog_Click);
            // 
            // groupBox_Mtrx
            // 
            this.groupBox_Mtrx.Controls.Add(this.button_ConvertMp4ToMtrx);
            this.groupBox_Mtrx.Controls.Add(this.button_OpenMtrxFile);
            this.groupBox_Mtrx.Controls.Add(this.label_ConvertProgress);
            this.groupBox_Mtrx.Controls.Add(this.progressBar_Convert);
            this.groupBox_Mtrx.Controls.Add(this.label_FileName);
            this.groupBox_Mtrx.Controls.Add(this.label_CropMode);
            this.groupBox_Mtrx.Controls.Add(this.comboBox_CropMode);
            this.groupBox_Mtrx.Location = new System.Drawing.Point(12, 406);
            this.groupBox_Mtrx.Name = "groupBox_Mtrx";
            this.groupBox_Mtrx.Size = new System.Drawing.Size(805, 401);
            this.groupBox_Mtrx.TabIndex = 42;
            this.groupBox_Mtrx.TabStop = false;
            this.groupBox_Mtrx.Text = "LED Matrix";
            // 
            // button_ConvertMp4ToMtrx
            // 
            this.button_ConvertMp4ToMtrx.Location = new System.Drawing.Point(209, 103);
            this.button_ConvertMp4ToMtrx.Name = "button_ConvertMp4ToMtrx";
            this.button_ConvertMp4ToMtrx.Size = new System.Drawing.Size(397, 50);
            this.button_ConvertMp4ToMtrx.TabIndex = 0;
            this.button_ConvertMp4ToMtrx.TabStop = false;
            this.button_ConvertMp4ToMtrx.Text = "Convert mp4 to mtrx file";
            this.button_ConvertMp4ToMtrx.UseVisualStyleBackColor = true;
            this.button_ConvertMp4ToMtrx.Click += new System.EventHandler(this.button_ConvertMp4ToMtrxFile_Click);
            // 
            // button_OpenMtrxFile
            // 
            this.button_OpenMtrxFile.Location = new System.Drawing.Point(209, 330);
            this.button_OpenMtrxFile.Name = "button_OpenMtrxFile";
            this.button_OpenMtrxFile.Size = new System.Drawing.Size(397, 50);
            this.button_OpenMtrxFile.TabIndex = 1;
            this.button_OpenMtrxFile.TabStop = false;
            this.button_OpenMtrxFile.Text = "Open mtrx file";
            this.button_OpenMtrxFile.UseVisualStyleBackColor = true;
            this.button_OpenMtrxFile.Click += new System.EventHandler(this.button_OpenMtrxFile_Click);
            // 
            // label_ConvertProgress
            // 
            this.label_ConvertProgress.Location = new System.Drawing.Point(310, 296);
            this.label_ConvertProgress.Name = "label_ConvertProgress";
            this.label_ConvertProgress.Size = new System.Drawing.Size(185, 31);
            this.label_ConvertProgress.TabIndex = 2;
            this.label_ConvertProgress.Text = "---";
            this.label_ConvertProgress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // progressBar_Convert
            // 
            this.progressBar_Convert.Location = new System.Drawing.Point(18, 254);
            this.progressBar_Convert.Name = "progressBar_Convert";
            this.progressBar_Convert.Size = new System.Drawing.Size(771, 34);
            this.progressBar_Convert.TabIndex = 3;
            this.progressBar_Convert.TabStop = false;
            // 
            // label_FileName
            // 
            this.label_FileName.Location = new System.Drawing.Point(18, 158);
            this.label_FileName.Name = "label_FileName";
            this.label_FileName.Size = new System.Drawing.Size(771, 88);
            this.label_FileName.TabIndex = 4;
            this.label_FileName.Text = "---";
            this.label_FileName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_CropMode
            // 
            this.label_CropMode.Location = new System.Drawing.Point(14, 28);
            this.label_CropMode.Name = "label_CropMode";
            this.label_CropMode.Size = new System.Drawing.Size(350, 21);
            this.label_CropMode.TabIndex = 5;
            this.label_CropMode.Text = "Crop Mode:";
            // 
            // comboBox_CropMode
            // 
            this.comboBox_CropMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_CropMode.DropDownWidth = 771;
            this.comboBox_CropMode.FormattingEnabled = true;
            this.comboBox_CropMode.Location = new System.Drawing.Point(14, 52);
            this.comboBox_CropMode.Name = "comboBox_CropMode";
            this.comboBox_CropMode.Size = new System.Drawing.Size(771, 29);
            this.comboBox_CropMode.TabIndex = 6;
            this.comboBox_CropMode.TabStop = false;
            // 
            // FormMain
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(832, 822);
            this.Controls.Add(this.groupBox_Mtrx);
            this.Controls.Add(this.button_ClearAppLog);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.textBox_AppLog);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("ＭＳ ゴシック", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox_Mtrx.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label_BoardId;
        private System.Windows.Forms.Button button_Connect;
        private System.Windows.Forms.ComboBox comboBox_Port;
        private System.Windows.Forms.Label label_FwName;
        private System.Windows.Forms.Label label_FwVer;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label_AppName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label_ConnectStatus;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label_AppVer;
        private System.Windows.Forms.TextBox textBox_AppLog;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button button_ClearAppLog;
        private System.Windows.Forms.GroupBox groupBox_Mtrx;
        private System.Windows.Forms.Button button_ConvertMp4ToMtrx;
        private System.Windows.Forms.Button button_OpenMtrxFile;
        private System.Windows.Forms.Label label_ConvertProgress;
        private System.Windows.Forms.ProgressBar progressBar_Convert;
        private System.Windows.Forms.Label label_FileName;
        private System.Windows.Forms.Label label_CropMode;
        private System.Windows.Forms.ComboBox comboBox_CropMode;
    }
}
