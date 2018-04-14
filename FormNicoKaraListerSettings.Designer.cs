namespace NicoKaraLister
{
	partial class FormNicoKaraListerSettings
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormNicoKaraListerSettings));
			this.label1 = new System.Windows.Forms.Label();
			this.ButtonCancel = new System.Windows.Forms.Button();
			this.ButtonOK = new System.Windows.Forms.Button();
			this.ButtonImportInfo = new System.Windows.Forms.Button();
			this.ButtonExportInfo = new System.Windows.Forms.Button();
			this.CheckBoxCheckRss = new System.Windows.Forms.CheckBox();
			this.ButtonCheckRss = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.ButtonSaveLog = new System.Windows.Forms.Button();
			this.ProgressBarCheckRss = new System.Windows.Forms.ProgressBar();
			this.panel2 = new System.Windows.Forms.Panel();
			this.SaveFileDialogLog = new System.Windows.Forms.SaveFileDialog();
			this.SaveFileDialogExport = new System.Windows.Forms.SaveFileDialog();
			this.OpenFileDialogImport = new System.Windows.Forms.OpenFileDialog();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.TextBoxIdPrefix = new System.Windows.Forms.TextBox();
			this.ButtonHelp = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(171, 12);
			this.label1.TabIndex = 0;
			this.label1.Text = "楽曲情報・番組情報のメンテナンス";
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.Location = new System.Drawing.Point(312, 348);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = new System.Drawing.Size(96, 28);
			this.ButtonCancel.TabIndex = 14;
			this.ButtonCancel.Text = "キャンセル";
			this.ButtonCancel.UseVisualStyleBackColor = true;
			// 
			// ButtonOK
			// 
			this.ButtonOK.Location = new System.Drawing.Point(200, 348);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = new System.Drawing.Size(96, 28);
			this.ButtonOK.TabIndex = 13;
			this.ButtonOK.Text = "OK";
			this.ButtonOK.UseVisualStyleBackColor = true;
			this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// ButtonImportInfo
			// 
			this.ButtonImportInfo.Location = new System.Drawing.Point(200, 40);
			this.ButtonImportInfo.Name = "ButtonImportInfo";
			this.ButtonImportInfo.Size = new System.Drawing.Size(208, 28);
			this.ButtonImportInfo.TabIndex = 1;
			this.ButtonImportInfo.Text = "インポート (&I)";
			this.ButtonImportInfo.UseVisualStyleBackColor = true;
			this.ButtonImportInfo.Click += new System.EventHandler(this.ButtonImportInfo_Click);
			// 
			// ButtonExportInfo
			// 
			this.ButtonExportInfo.Location = new System.Drawing.Point(200, 76);
			this.ButtonExportInfo.Name = "ButtonExportInfo";
			this.ButtonExportInfo.Size = new System.Drawing.Size(208, 28);
			this.ButtonExportInfo.TabIndex = 2;
			this.ButtonExportInfo.Text = "エクスポート (&E)";
			this.ButtonExportInfo.UseVisualStyleBackColor = true;
			this.ButtonExportInfo.Click += new System.EventHandler(this.ButtonExportInfo_Click);
			// 
			// CheckBoxCheckRss
			// 
			this.CheckBoxCheckRss.AutoSize = true;
			this.CheckBoxCheckRss.Location = new System.Drawing.Point(16, 196);
			this.CheckBoxCheckRss.Name = "CheckBoxCheckRss";
			this.CheckBoxCheckRss.Size = new System.Drawing.Size(310, 16);
			this.CheckBoxCheckRss.TabIndex = 6;
			this.CheckBoxCheckRss.Text = "ニコカラりすたーの最新情報・更新版を自動的に確認する (&L)";
			this.CheckBoxCheckRss.UseVisualStyleBackColor = true;
			this.CheckBoxCheckRss.CheckedChanged += new System.EventHandler(this.CheckBoxCheckRss_CheckedChanged);
			// 
			// ButtonCheckRss
			// 
			this.ButtonCheckRss.Location = new System.Drawing.Point(200, 220);
			this.ButtonCheckRss.Name = "ButtonCheckRss";
			this.ButtonCheckRss.Size = new System.Drawing.Size(208, 28);
			this.ButtonCheckRss.TabIndex = 8;
			this.ButtonCheckRss.Text = "今すぐ最新情報を確認する (&A)";
			this.ButtonCheckRss.UseVisualStyleBackColor = true;
			this.ButtonCheckRss.Click += new System.EventHandler(this.ButtonCheckRss_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(16, 276);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(75, 12);
			this.label2.TabIndex = 9;
			this.label2.Text = "ログを保存する";
			// 
			// ButtonSaveLog
			// 
			this.ButtonSaveLog.Location = new System.Drawing.Point(200, 292);
			this.ButtonSaveLog.Name = "ButtonSaveLog";
			this.ButtonSaveLog.Size = new System.Drawing.Size(208, 28);
			this.ButtonSaveLog.TabIndex = 10;
			this.ButtonSaveLog.Text = "ログ保存 (&X)";
			this.ButtonSaveLog.UseVisualStyleBackColor = true;
			this.ButtonSaveLog.Click += new System.EventHandler(this.ButtonSaveLog_Click);
			// 
			// ProgressBarCheckRss
			// 
			this.ProgressBarCheckRss.Location = new System.Drawing.Point(16, 220);
			this.ProgressBarCheckRss.MarqueeAnimationSpeed = 10;
			this.ProgressBarCheckRss.Name = "ProgressBarCheckRss";
			this.ProgressBarCheckRss.Size = new System.Drawing.Size(168, 28);
			this.ProgressBarCheckRss.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
			this.ProgressBarCheckRss.TabIndex = 7;
			this.ProgressBarCheckRss.Visible = false;
			// 
			// panel2
			// 
			this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel2.Location = new System.Drawing.Point(0, 332);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(500, 5);
			this.panel2.TabIndex = 11;
			// 
			// SaveFileDialogLog
			// 
			this.SaveFileDialogLog.Filter = "ログファイル|*.lga";
			// 
			// SaveFileDialogExport
			// 
			this.SaveFileDialogExport.Filter = "楽曲番組情報ファイル|*.nklinfo";
			// 
			// OpenFileDialogImport
			// 
			this.OpenFileDialogImport.Filter = "楽曲番組情報ファイル|*.nklinfo";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(16, 120);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(249, 12);
			this.label3.TabIndex = 3;
			this.label3.Text = "新規の楽曲 ID・番組 ID の先頭に付与する文字列";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(32, 140);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(352, 12);
			this.label4.TabIndex = 4;
			this.label4.Text = "※自分だけが使う文字列にすると、他人と重複する可能性が減るでしょう。";
			// 
			// TextBoxIdPrefix
			// 
			this.TextBoxIdPrefix.Location = new System.Drawing.Point(200, 160);
			this.TextBoxIdPrefix.Name = "TextBoxIdPrefix";
			this.TextBoxIdPrefix.Size = new System.Drawing.Size(208, 19);
			this.TextBoxIdPrefix.TabIndex = 5;
			// 
			// ButtonHelp
			// 
			this.ButtonHelp.Location = new System.Drawing.Point(16, 348);
			this.ButtonHelp.Name = "ButtonHelp";
			this.ButtonHelp.Size = new System.Drawing.Size(96, 28);
			this.ButtonHelp.TabIndex = 12;
			this.ButtonHelp.Text = "ヘルプ (&H)";
			this.ButtonHelp.UseVisualStyleBackColor = true;
			this.ButtonHelp.Click += new System.EventHandler(this.ButtonHelp_Click);
			// 
			// FormNicoKaraListerSettings
			// 
			this.AcceptButton = this.ButtonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.ButtonCancel;
			this.ClientSize = new System.Drawing.Size(425, 391);
			this.Controls.Add(this.ButtonHelp);
			this.Controls.Add(this.TextBoxIdPrefix);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.ProgressBarCheckRss);
			this.Controls.Add(this.ButtonSaveLog);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.ButtonCheckRss);
			this.Controls.Add(this.CheckBoxCheckRss);
			this.Controls.Add(this.ButtonExportInfo);
			this.Controls.Add(this.ButtonImportInfo);
			this.Controls.Add(this.ButtonCancel);
			this.Controls.Add(this.ButtonOK);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormNicoKaraListerSettings";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormNicoKaraListerSettings_FormClosed);
			this.Load += new System.EventHandler(this.FormNicoKaraListerSettings_Load);
			this.Shown += new System.EventHandler(this.FormNicoKaraListerSettings_Shown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button ButtonCancel;
		private System.Windows.Forms.Button ButtonOK;
		private System.Windows.Forms.Button ButtonImportInfo;
		private System.Windows.Forms.Button ButtonExportInfo;
		private System.Windows.Forms.CheckBox CheckBoxCheckRss;
		private System.Windows.Forms.Button ButtonCheckRss;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button ButtonSaveLog;
		private System.Windows.Forms.ProgressBar ProgressBarCheckRss;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.SaveFileDialog SaveFileDialogLog;
		private System.Windows.Forms.SaveFileDialog SaveFileDialogExport;
		private System.Windows.Forms.OpenFileDialog OpenFileDialogImport;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox TextBoxIdPrefix;
		private System.Windows.Forms.Button ButtonHelp;
	}
}