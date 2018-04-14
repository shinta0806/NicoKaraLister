namespace NicoKaraLister
{
	partial class FormInputIdPrefix
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormInputIdPrefix));
			this.label1 = new System.Windows.Forms.Label();
			this.TextBoxIdPrefix = new System.Windows.Forms.TextBox();
			this.ButtonCancel = new System.Windows.Forms.Button();
			this.ButtonOK = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.ButtonHelp = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(338, 12);
			this.label1.TabIndex = 0;
			this.label1.Text = "新規の楽曲 ID・番組 ID の先頭に付与する文字列を指定して下さい。";
			// 
			// TextBoxIdPrefix
			// 
			this.TextBoxIdPrefix.Location = new System.Drawing.Point(16, 60);
			this.TextBoxIdPrefix.Name = "TextBoxIdPrefix";
			this.TextBoxIdPrefix.Size = new System.Drawing.Size(344, 19);
			this.TextBoxIdPrefix.TabIndex = 2;
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.Location = new System.Drawing.Point(265, 92);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = new System.Drawing.Size(96, 28);
			this.ButtonCancel.TabIndex = 5;
			this.ButtonCancel.Text = "キャンセル";
			this.ButtonCancel.UseVisualStyleBackColor = true;
			// 
			// ButtonOK
			// 
			this.ButtonOK.Location = new System.Drawing.Point(152, 92);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = new System.Drawing.Size(96, 28);
			this.ButtonOK.TabIndex = 4;
			this.ButtonOK.Text = "OK";
			this.ButtonOK.UseVisualStyleBackColor = true;
			this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(16, 36);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(340, 12);
			this.label2.TabIndex = 1;
			this.label2.Text = "自分だけが使う文字列にすると、他人と重複する可能性が減るでしょう。";
			// 
			// ButtonHelp
			// 
			this.ButtonHelp.Location = new System.Drawing.Point(16, 92);
			this.ButtonHelp.Name = "ButtonHelp";
			this.ButtonHelp.Size = new System.Drawing.Size(96, 28);
			this.ButtonHelp.TabIndex = 3;
			this.ButtonHelp.Text = "ヘルプ (&H)";
			this.ButtonHelp.UseVisualStyleBackColor = true;
			this.ButtonHelp.Click += new System.EventHandler(this.ButtonHelp_Click);
			// 
			// FormInputIdPrefix
			// 
			this.AcceptButton = this.ButtonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.ButtonCancel;
			this.ClientSize = new System.Drawing.Size(378, 135);
			this.Controls.Add(this.ButtonHelp);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.ButtonCancel);
			this.Controls.Add(this.ButtonOK);
			this.Controls.Add(this.TextBoxIdPrefix);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormInputIdPrefix";
			this.Text = "ID 接頭辞の指定";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormInputIdPrefix_FormClosed);
			this.Load += new System.EventHandler(this.FormInputIdPrefix_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox TextBoxIdPrefix;
		private System.Windows.Forms.Button ButtonCancel;
		private System.Windows.Forms.Button ButtonOK;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button ButtonHelp;
	}
}