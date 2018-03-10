namespace NicoKaraLister
{
	partial class FormNicoKaraLister
	{
		/// <summary>
		/// 必要なデザイナー変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		/// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormNicoKaraLister));
			this.TextBoxParentFolder = new System.Windows.Forms.TextBox();
			this.ButtonGo = new System.Windows.Forms.Button();
			this.TextBoxLog = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.TextBoxOutputFolder = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.DataGridViewTargetFolders = new System.Windows.Forms.DataGridView();
			this.ColumnIsValid = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.ColumnFolder = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnSettingsExist = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnSettings = new System.Windows.Forms.DataGridViewButtonColumn();
			this.ButtonBrowseParentFolder = new System.Windows.Forms.Button();
			this.ButtonBrowseOutputFolder = new System.Windows.Forms.Button();
			this.ButtonHelp = new System.Windows.Forms.Button();
			this.ButtonSettings = new System.Windows.Forms.Button();
			this.panel2 = new System.Windows.Forms.Panel();
			this.panel1 = new System.Windows.Forms.Panel();
			this.FolderBrowserDialogFolder = new System.Windows.Forms.FolderBrowserDialog();
			this.label2 = new System.Windows.Forms.Label();
			this.ComboBoxOutputFormat = new System.Windows.Forms.ComboBox();
			this.ButtonOutputSettings = new System.Windows.Forms.Button();
			this.ContextMenuHelp = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ヘルプHToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.改訂履歴UToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.バージョン情報ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ButtonParentFolderHistory = new System.Windows.Forms.Button();
			this.ButtonOutputFolderHistory = new System.Windows.Forms.Button();
			this.ContextMenuParentFolderHistory = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ContextMenuOutputFolderHistory = new System.Windows.Forms.ContextMenuStrip(this.components);
			((System.ComponentModel.ISupportInitialize)(this.DataGridViewTargetFolders)).BeginInit();
			this.ContextMenuHelp.SuspendLayout();
			this.SuspendLayout();
			// 
			// TextBoxParentFolder
			// 
			this.TextBoxParentFolder.AllowDrop = true;
			this.TextBoxParentFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TextBoxParentFolder.Location = new System.Drawing.Point(152, 16);
			this.TextBoxParentFolder.Name = "TextBoxParentFolder";
			this.TextBoxParentFolder.Size = new System.Drawing.Size(488, 19);
			this.TextBoxParentFolder.TabIndex = 1;
			this.TextBoxParentFolder.TextChanged += new System.EventHandler(this.TextBoxParentFolder_TextChanged);
			this.TextBoxParentFolder.DragDrop += new System.Windows.Forms.DragEventHandler(this.TextBoxFolder_DragDrop);
			this.TextBoxParentFolder.DragEnter += new System.Windows.Forms.DragEventHandler(this.TextBoxFolder_DragEnter);
			// 
			// ButtonGo
			// 
			this.ButtonGo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonGo.Location = new System.Drawing.Point(576, 320);
			this.ButtonGo.Name = "ButtonGo";
			this.ButtonGo.Size = new System.Drawing.Size(192, 28);
			this.ButtonGo.TabIndex = 15;
			this.ButtonGo.Text = "リスト生成 (&G)";
			this.ButtonGo.UseVisualStyleBackColor = true;
			this.ButtonGo.Click += new System.EventHandler(this.ButtonGo_Click);
			// 
			// TextBoxLog
			// 
			this.TextBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TextBoxLog.Location = new System.Drawing.Point(16, 380);
			this.TextBoxLog.Multiline = true;
			this.TextBoxLog.Name = "TextBoxLog";
			this.TextBoxLog.ReadOnly = true;
			this.TextBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.TextBoxLog.Size = new System.Drawing.Size(752, 172);
			this.TextBoxLog.TabIndex = 17;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(136, 20);
			this.label1.TabIndex = 0;
			this.label1.Text = "リスト化対象フォルダー (&L)：";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// TextBoxOutputFolder
			// 
			this.TextBoxOutputFolder.AllowDrop = true;
			this.TextBoxOutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TextBoxOutputFolder.Location = new System.Drawing.Point(152, 232);
			this.TextBoxOutputFolder.Name = "TextBoxOutputFolder";
			this.TextBoxOutputFolder.Size = new System.Drawing.Size(488, 19);
			this.TextBoxOutputFolder.TabIndex = 6;
			this.TextBoxOutputFolder.DragDrop += new System.Windows.Forms.DragEventHandler(this.TextBoxFolder_DragDrop);
			this.TextBoxOutputFolder.DragEnter += new System.Windows.Forms.DragEventHandler(this.TextBoxFolder_DragEnter);
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label3.Location = new System.Drawing.Point(16, 232);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(136, 20);
			this.label3.TabIndex = 5;
			this.label3.Text = "リスト出力先フォルダー (&O)：";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// DataGridViewTargetFolders
			// 
			this.DataGridViewTargetFolders.AllowUserToAddRows = false;
			this.DataGridViewTargetFolders.AllowUserToDeleteRows = false;
			this.DataGridViewTargetFolders.AllowUserToResizeRows = false;
			this.DataGridViewTargetFolders.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DataGridViewTargetFolders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.DataGridViewTargetFolders.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnIsValid,
            this.ColumnFolder,
            this.ColumnSettingsExist,
            this.ColumnSettings});
			this.DataGridViewTargetFolders.Location = new System.Drawing.Point(16, 48);
			this.DataGridViewTargetFolders.Name = "DataGridViewTargetFolders";
			this.DataGridViewTargetFolders.RowHeadersVisible = false;
			this.DataGridViewTargetFolders.RowTemplate.Height = 21;
			this.DataGridViewTargetFolders.Size = new System.Drawing.Size(752, 172);
			this.DataGridViewTargetFolders.TabIndex = 4;
			this.DataGridViewTargetFolders.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewTargetFolders_CellContentClick);
			// 
			// ColumnIsValid
			// 
			this.ColumnIsValid.HeaderText = "対象";
			this.ColumnIsValid.Name = "ColumnIsValid";
			// 
			// ColumnFolder
			// 
			this.ColumnFolder.HeaderText = "フォルダー";
			this.ColumnFolder.Name = "ColumnFolder";
			this.ColumnFolder.ReadOnly = true;
			this.ColumnFolder.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// ColumnSettingsExist
			// 
			this.ColumnSettingsExist.HeaderText = "設定有無";
			this.ColumnSettingsExist.Name = "ColumnSettingsExist";
			this.ColumnSettingsExist.ReadOnly = true;
			this.ColumnSettingsExist.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// ColumnSettings
			// 
			this.ColumnSettings.HeaderText = "設定";
			this.ColumnSettings.Name = "ColumnSettings";
			// 
			// ButtonBrowseParentFolder
			// 
			this.ButtonBrowseParentFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonBrowseParentFolder.Location = new System.Drawing.Point(672, 12);
			this.ButtonBrowseParentFolder.Name = "ButtonBrowseParentFolder";
			this.ButtonBrowseParentFolder.Size = new System.Drawing.Size(96, 28);
			this.ButtonBrowseParentFolder.TabIndex = 3;
			this.ButtonBrowseParentFolder.Text = "参照 (&1)";
			this.ButtonBrowseParentFolder.UseVisualStyleBackColor = true;
			this.ButtonBrowseParentFolder.Click += new System.EventHandler(this.ButtonBrowseParentFolder_Click);
			// 
			// ButtonBrowseOutputFolder
			// 
			this.ButtonBrowseOutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonBrowseOutputFolder.Location = new System.Drawing.Point(672, 228);
			this.ButtonBrowseOutputFolder.Name = "ButtonBrowseOutputFolder";
			this.ButtonBrowseOutputFolder.Size = new System.Drawing.Size(96, 28);
			this.ButtonBrowseOutputFolder.TabIndex = 8;
			this.ButtonBrowseOutputFolder.Text = "参照 (&2)";
			this.ButtonBrowseOutputFolder.UseVisualStyleBackColor = true;
			this.ButtonBrowseOutputFolder.Click += new System.EventHandler(this.ButtonBrowseOutputFolder_Click);
			// 
			// ButtonHelp
			// 
			this.ButtonHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ButtonHelp.Location = new System.Drawing.Point(16, 320);
			this.ButtonHelp.Name = "ButtonHelp";
			this.ButtonHelp.Size = new System.Drawing.Size(96, 28);
			this.ButtonHelp.TabIndex = 13;
			this.ButtonHelp.Text = "ヘルプ (&H)";
			this.ButtonHelp.UseVisualStyleBackColor = true;
			this.ButtonHelp.Click += new System.EventHandler(this.ButtonHelp_Click);
			// 
			// ButtonSettings
			// 
			this.ButtonSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ButtonSettings.Location = new System.Drawing.Point(128, 320);
			this.ButtonSettings.Name = "ButtonSettings";
			this.ButtonSettings.Size = new System.Drawing.Size(96, 28);
			this.ButtonSettings.TabIndex = 14;
			this.ButtonSettings.Text = "環境設定 (&S)";
			this.ButtonSettings.UseVisualStyleBackColor = true;
			this.ButtonSettings.Click += new System.EventHandler(this.ButtonSettings_Click);
			// 
			// panel2
			// 
			this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel2.Location = new System.Drawing.Point(0, 304);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(1000, 5);
			this.panel2.TabIndex = 12;
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel1.Location = new System.Drawing.Point(0, 360);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(1000, 5);
			this.panel1.TabIndex = 16;
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label2.Location = new System.Drawing.Point(16, 268);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(136, 20);
			this.label2.TabIndex = 9;
			this.label2.Text = "リスト出力形式 (&F)：";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ComboBoxOutputFormat
			// 
			this.ComboBoxOutputFormat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ComboBoxOutputFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ComboBoxOutputFormat.FormattingEnabled = true;
			this.ComboBoxOutputFormat.Location = new System.Drawing.Point(152, 268);
			this.ComboBoxOutputFormat.Name = "ComboBoxOutputFormat";
			this.ComboBoxOutputFormat.Size = new System.Drawing.Size(512, 20);
			this.ComboBoxOutputFormat.TabIndex = 10;
			this.ComboBoxOutputFormat.SelectedIndexChanged += new System.EventHandler(this.ComboBoxOutputFormat_SelectedIndexChanged);
			// 
			// ButtonOutputSettings
			// 
			this.ButtonOutputSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonOutputSettings.Location = new System.Drawing.Point(672, 264);
			this.ButtonOutputSettings.Name = "ButtonOutputSettings";
			this.ButtonOutputSettings.Size = new System.Drawing.Size(96, 28);
			this.ButtonOutputSettings.TabIndex = 11;
			this.ButtonOutputSettings.Text = "出力設定 (&E)";
			this.ButtonOutputSettings.UseVisualStyleBackColor = true;
			this.ButtonOutputSettings.Click += new System.EventHandler(this.ButtonOutputSettings_Click);
			// 
			// ContextMenuHelp
			// 
			this.ContextMenuHelp.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ヘルプHToolStripMenuItem,
            this.toolStripSeparator1,
            this.改訂履歴UToolStripMenuItem,
            this.バージョン情報ToolStripMenuItem});
			this.ContextMenuHelp.Name = "ContextMenuHelp";
			this.ContextMenuHelp.Size = new System.Drawing.Size(162, 76);
			// 
			// ヘルプHToolStripMenuItem
			// 
			this.ヘルプHToolStripMenuItem.Name = "ヘルプHToolStripMenuItem";
			this.ヘルプHToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
			this.ヘルプHToolStripMenuItem.Text = "ヘルプ (&H)";
			this.ヘルプHToolStripMenuItem.Click += new System.EventHandler(this.ヘルプHToolStripMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(158, 6);
			// 
			// 改訂履歴UToolStripMenuItem
			// 
			this.改訂履歴UToolStripMenuItem.Name = "改訂履歴UToolStripMenuItem";
			this.改訂履歴UToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
			this.改訂履歴UToolStripMenuItem.Text = "改訂履歴 (&U)";
			this.改訂履歴UToolStripMenuItem.Click += new System.EventHandler(this.改訂履歴UToolStripMenuItem_Click);
			// 
			// バージョン情報ToolStripMenuItem
			// 
			this.バージョン情報ToolStripMenuItem.Name = "バージョン情報ToolStripMenuItem";
			this.バージョン情報ToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
			this.バージョン情報ToolStripMenuItem.Text = "バージョン情報 (&A)";
			this.バージョン情報ToolStripMenuItem.Click += new System.EventHandler(this.バージョン情報ToolStripMenuItem_Click);
			// 
			// ButtonParentFolderHistory
			// 
			this.ButtonParentFolderHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonParentFolderHistory.Font = new System.Drawing.Font("MS UI Gothic", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.ButtonParentFolderHistory.Location = new System.Drawing.Point(640, 16);
			this.ButtonParentFolderHistory.Name = "ButtonParentFolderHistory";
			this.ButtonParentFolderHistory.Size = new System.Drawing.Size(24, 20);
			this.ButtonParentFolderHistory.TabIndex = 2;
			this.ButtonParentFolderHistory.Text = "▼";
			this.ButtonParentFolderHistory.UseVisualStyleBackColor = true;
			this.ButtonParentFolderHistory.Click += new System.EventHandler(this.ButtonParentFolderHistory_Click);
			// 
			// ButtonOutputFolderHistory
			// 
			this.ButtonOutputFolderHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonOutputFolderHistory.Font = new System.Drawing.Font("MS UI Gothic", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.ButtonOutputFolderHistory.Location = new System.Drawing.Point(640, 232);
			this.ButtonOutputFolderHistory.Name = "ButtonOutputFolderHistory";
			this.ButtonOutputFolderHistory.Size = new System.Drawing.Size(24, 20);
			this.ButtonOutputFolderHistory.TabIndex = 7;
			this.ButtonOutputFolderHistory.Text = "▼";
			this.ButtonOutputFolderHistory.UseVisualStyleBackColor = true;
			this.ButtonOutputFolderHistory.Click += new System.EventHandler(this.ButtonOutputFolderHistory_Click);
			// 
			// ContextMenuParentFolderHistory
			// 
			this.ContextMenuParentFolderHistory.Name = "ContextMenuParentFolderHistory";
			this.ContextMenuParentFolderHistory.Size = new System.Drawing.Size(61, 4);
			// 
			// ContextMenuOutputFolderHistory
			// 
			this.ContextMenuOutputFolderHistory.Name = "ContextMenuOutputFolderHistory";
			this.ContextMenuOutputFolderHistory.Size = new System.Drawing.Size(61, 4);
			// 
			// FormNicoKaraLister
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(784, 569);
			this.Controls.Add(this.ButtonOutputFolderHistory);
			this.Controls.Add(this.ButtonParentFolderHistory);
			this.Controls.Add(this.ButtonOutputSettings);
			this.Controls.Add(this.ComboBoxOutputFormat);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.ButtonSettings);
			this.Controls.Add(this.ButtonHelp);
			this.Controls.Add(this.ButtonBrowseOutputFolder);
			this.Controls.Add(this.ButtonBrowseParentFolder);
			this.Controls.Add(this.DataGridViewTargetFolders);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.TextBoxOutputFolder);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.TextBoxLog);
			this.Controls.Add(this.ButtonGo);
			this.Controls.Add(this.TextBoxParentFolder);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FormNicoKaraLister";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormNicoKaraLister_FormClosed);
			this.Load += new System.EventHandler(this.FormNicoKaraLister_Load);
			this.Shown += new System.EventHandler(this.FormNicoKaraLister_Shown);
			((System.ComponentModel.ISupportInitialize)(this.DataGridViewTargetFolders)).EndInit();
			this.ContextMenuHelp.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox TextBoxParentFolder;
		private System.Windows.Forms.Button ButtonGo;
		private System.Windows.Forms.TextBox TextBoxLog;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox TextBoxOutputFolder;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.DataGridView DataGridViewTargetFolders;
		private System.Windows.Forms.Button ButtonBrowseParentFolder;
		private System.Windows.Forms.Button ButtonBrowseOutputFolder;
		private System.Windows.Forms.Button ButtonHelp;
		private System.Windows.Forms.Button ButtonSettings;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnIsValid;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnFolder;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnSettingsExist;
		private System.Windows.Forms.DataGridViewButtonColumn ColumnSettings;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.FolderBrowserDialog FolderBrowserDialogFolder;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox ComboBoxOutputFormat;
		private System.Windows.Forms.Button ButtonOutputSettings;
		private System.Windows.Forms.ContextMenuStrip ContextMenuHelp;
		private System.Windows.Forms.ToolStripMenuItem ヘルプHToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem 改訂履歴UToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem バージョン情報ToolStripMenuItem;
		private System.Windows.Forms.Button ButtonParentFolderHistory;
		private System.Windows.Forms.Button ButtonOutputFolderHistory;
		private System.Windows.Forms.ContextMenuStrip ContextMenuParentFolderHistory;
		private System.Windows.Forms.ContextMenuStrip ContextMenuOutputFolderHistory;
	}
}

