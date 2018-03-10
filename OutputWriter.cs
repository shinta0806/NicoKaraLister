// ============================================================================
// 
// リスト出力用基底クラス
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace NicoKaraLister.Shared
{
	public abstract class OutputWriter
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public OutputWriter()
		{
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 出力形式（表示用）
		public String FormatName { get; protected set; }

		// 出力先フォルダー（末尾 '\\' 付き）
		private String mFolderPath;
		public String FolderPath
		{
			get
			{
				return mFolderPath;
			}
			set
			{
				mFolderPath = value;
				if (!String.IsNullOrEmpty(mFolderPath) && mFolderPath[mFolderPath.Length - 1] != '\\')
				{
					mFolderPath += '\\';
				}
			}
		}

		// 出力先インデックスファイル名（パス無し）
		public String TopFileName { get; protected set; }

		// 検索結果テーブル
		public Table<TFound> TableFound { get; set; }

		// 環境設定
		public NicoKaraListerSettings NicoKaraListerSettings { get; set; }

		// 出力設定
		public OutputSettings OutputSettings { get; set; }

		// ログ
		public LogWriter LogWriter { get; set; }

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 設定画面に入力された値が適正か確認
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		public virtual void CheckInput()
		{
			// 入力途中の拡張子を自動追加するか確認
			if (!String.IsNullOrEmpty(TextBoxTargetExt.Text))
			{
				switch (MessageBox.Show("入力中の拡張子\n" + TextBoxTargetExt.Text + "\nはまだ追加されていません。\n追加しますか？",
						"確認", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation))
				{
					case DialogResult.Yes:
						AddExt();
						break;
					case DialogResult.No:
						break;
					case DialogResult.Cancel:
						throw new OperationCanceledException("設定変更を中止しました。");
				}
			}
		}

		// --------------------------------------------------------------------
		// コンポーネントから設定に反映
		// --------------------------------------------------------------------
		public virtual void ComposToSettings()
		{
			// リスト化対象ファイルの拡張子
			OutputSettings.TargetExts.Clear();
			for (Int32 i = 0; i < ListBoxTargetExts.Items.Count; i++)
			{
				OutputSettings.TargetExts.Add((String)ListBoxTargetExts.Items[i]);
			}
			OutputSettings.TargetExts.Sort();

			// 出力項目のタイプ
			OutputSettings.OutputAllItems = RadioButtonOutputAllItems.Checked;

			// 出力項目のリスト
			OutputSettings.SelectedOutputItems.Clear();
			for (Int32 i = 0; i < ListBoxAddedItems.Items.Count; i++)
			{
				Int32 aItem = Array.IndexOf(NklCommon.OUTPUT_ITEM_NAMES, (String)ListBoxAddedItems.Items[i]);
				if (aItem < 0)
				{
					continue;
				}
				OutputSettings.SelectedOutputItems.Add((OutputItems)aItem);
			}
		}

		// --------------------------------------------------------------------
		// 設定画面のタブページ
		// --------------------------------------------------------------------
		public virtual List<TabPage> DialogTabPages()
		{
			List<TabPage> aTabPages = new List<TabPage>();

			// TabPageOutputSettings
			TabPageOutputSettings = new TabPage();
			TabPageOutputSettings.BackColor = SystemColors.Control;
			TabPageOutputSettings.Location = new Point(4, 22);
			TabPageOutputSettings.Padding = new Padding(3);
			TabPageOutputSettings.Size = new Size(456, 386);
			TabPageOutputSettings.Text = "基本設定";

			// LabelTargetExt
			LabelTargetExt = new Label();
			LabelTargetExt.Location = new Point(16, 16);
			LabelTargetExt.Size = new Size(176, 20);
			LabelTargetExt.Text = "リスト化対象ファイルの拡張子 (&E)：";
			LabelTargetExt.TextAlign = ContentAlignment.MiddleLeft;
			TabPageOutputSettings.Controls.Add(LabelTargetExt);

			// TextBoxTargetExt
			TextBoxTargetExt = new TextBox();
			TextBoxTargetExt.Location = new Point(192, 16);
			TextBoxTargetExt.Size = new Size(208, 19);
			TextBoxTargetExt.TextChanged += new EventHandler(TextBoxTargetExt_TextChanged);
			TabPageOutputSettings.Controls.Add(TextBoxTargetExt);

			// ButtonAddExt
			ButtonAddExt = new Button();
			ButtonAddExt.Location = new Point(192, 44);
			ButtonAddExt.Size = new Size(96, 28);
			ButtonAddExt.Text = "↓ 追加 (&A)";
			ButtonAddExt.UseVisualStyleBackColor = true;
			ButtonAddExt.Click += new EventHandler(ButtonAddExt_Click);
			TabPageOutputSettings.Controls.Add(ButtonAddExt);

			// ButtonRemoveExt
			ButtonRemoveExt = new Button();
			ButtonRemoveExt.Location = new Point(304, 44);
			ButtonRemoveExt.Size = new Size(96, 28);
			ButtonRemoveExt.Text = "× 削除 (&R)";
			ButtonRemoveExt.UseVisualStyleBackColor = true;
			ButtonRemoveExt.Click += new EventHandler(ButtonRemoveExt_Click);
			TabPageOutputSettings.Controls.Add(ButtonRemoveExt);

			// ListBoxTargetExts
			ListBoxTargetExts = new ListBox();
			ListBoxTargetExts.FormattingEnabled = true;
			ListBoxTargetExts.ItemHeight = 12;
			ListBoxTargetExts.Location = new Point(192, 80);
			ListBoxTargetExts.Size = new Size(208, 64);
			ListBoxTargetExts.SelectedIndexChanged += new EventHandler(ListBoxTargetExts_SelectedIndexChanged);
			TabPageOutputSettings.Controls.Add(ListBoxTargetExts);

			// LabelOutputItem
			LabelOutputItem = new Label();
			LabelOutputItem.Location = new Point(16, 160);
			LabelOutputItem.Size = new Size(72, 20);
			LabelOutputItem.Text = "出力項目：";
			LabelOutputItem.TextAlign = ContentAlignment.MiddleLeft;
			TabPageOutputSettings.Controls.Add(LabelOutputItem);

			// RadioButtonOutputAllItems
			RadioButtonOutputAllItems = new RadioButton();
			RadioButtonOutputAllItems.Location = new Point(88, 160);
			RadioButtonOutputAllItems.Size = new Size(96, 20);
			RadioButtonOutputAllItems.TabStop = true;
			RadioButtonOutputAllItems.Text = "すべて (&L)";
			RadioButtonOutputAllItems.UseVisualStyleBackColor = true;
			RadioButtonOutputAllItems.CheckedChanged += new EventHandler(RadioButtonOutputItems_CheckedChanged);
			TabPageOutputSettings.Controls.Add(RadioButtonOutputAllItems);

			// RadioButtonOutputAddedItems
			RadioButtonOutputAddedItems = new RadioButton();
			RadioButtonOutputAddedItems.Location = new Point(184, 160);
			RadioButtonOutputAddedItems.Size = new Size(168, 20);
			RadioButtonOutputAddedItems.TabStop = true;
			RadioButtonOutputAddedItems.Text = "以下で追加した項目のみ (&O)";
			RadioButtonOutputAddedItems.UseVisualStyleBackColor = true;
			RadioButtonOutputAddedItems.CheckedChanged += new EventHandler(RadioButtonOutputItems_CheckedChanged);
			TabPageOutputSettings.Controls.Add(RadioButtonOutputAddedItems);

			// LabelRemovedItems
			LabelRemovedItems = new Label();
			LabelRemovedItems.Location = new Point(32, 188);
			LabelRemovedItems.Size = new Size(152, 20);
			LabelRemovedItems.Text = "（出力されない項目）";
			LabelRemovedItems.TextAlign = ContentAlignment.MiddleLeft;
			TabPageOutputSettings.Controls.Add(LabelRemovedItems);

			// ListBoxRemovedItems
			ListBoxRemovedItems = new ListBox();
			ListBoxRemovedItems.FormattingEnabled = true;
			ListBoxRemovedItems.ItemHeight = 12;
			ListBoxRemovedItems.Location = new Point(32, 208);
			ListBoxRemovedItems.Size = new Size(152, 160);
			ListBoxRemovedItems.SelectedIndexChanged += new EventHandler(ListBoxRemovedItems_SelectedIndexChanged);
			TabPageOutputSettings.Controls.Add(ListBoxRemovedItems);

			// ButtonAddItem
			ButtonAddItem = new Button();
			ButtonAddItem.Location = new Point(192, 216);
			ButtonAddItem.Size = new Size(96, 28);
			ButtonAddItem.Text = "→ 追加 (&D)";
			ButtonAddItem.UseVisualStyleBackColor = true;
			ButtonAddItem.Click += new EventHandler(ButtonAddItem_Click);
			TabPageOutputSettings.Controls.Add(ButtonAddItem);

			// ButtonRemoveItem
			ButtonRemoveItem = new Button();
			ButtonRemoveItem.Location = new Point(192, 252);
			ButtonRemoveItem.Size = new Size(96, 28);
			ButtonRemoveItem.Text = "× 削除 (&M)";
			ButtonRemoveItem.UseVisualStyleBackColor = true;
			ButtonRemoveItem.Click += new EventHandler(ButtonRemoveItem_Click);
			TabPageOutputSettings.Controls.Add(ButtonRemoveItem);

			// ButtonUpItem
			ButtonUpItem = new Button();
			ButtonUpItem.Location = new Point(192, 296);
			ButtonUpItem.Size = new Size(96, 28);
			ButtonUpItem.Text = "↑ 上へ (&U)";
			ButtonUpItem.UseVisualStyleBackColor = true;
			ButtonUpItem.Click += new EventHandler(ButtonUpItem_Click);
			TabPageOutputSettings.Controls.Add(ButtonUpItem);

			// ButtonDownItem
			ButtonDownItem = new Button();
			ButtonDownItem.Location = new Point(192, 332);
			ButtonDownItem.Size = new Size(96, 28);
			ButtonDownItem.Text = "↓ 下へ (&W)";
			ButtonDownItem.UseVisualStyleBackColor = true;
			ButtonDownItem.Click += new EventHandler(ButtonDownItem_Click);
			TabPageOutputSettings.Controls.Add(ButtonDownItem);

			// LabelAddedItems
			LabelAddedItems = new Label();
			LabelAddedItems.Location = new Point(296, 188);
			LabelAddedItems.Size = new Size(152, 20);
			LabelAddedItems.Text = "（出力される項目）";
			LabelAddedItems.TextAlign = ContentAlignment.MiddleLeft;
			TabPageOutputSettings.Controls.Add(LabelAddedItems);

			// ListBoxAddedItems
			ListBoxAddedItems = new ListBox();
			ListBoxAddedItems.FormattingEnabled = true;
			ListBoxAddedItems.ItemHeight = 12;
			ListBoxAddedItems.Location = new Point(296, 208);
			ListBoxAddedItems.Size = new Size(152, 160);
			ListBoxAddedItems.SelectedIndexChanged += new EventHandler(ListBoxAddedItems_SelectedIndexChanged);
			TabPageOutputSettings.Controls.Add(ListBoxAddedItems);

			aTabPages.Add(TabPageOutputSettings);

			return aTabPages;
		}

		// --------------------------------------------------------------------
		// 設定画面を有効化するかどうか
		// --------------------------------------------------------------------
		public abstract Boolean IsDialogEnabled();

		// --------------------------------------------------------------------
		// リスト出力
		// --------------------------------------------------------------------
		public abstract void Output();

		// --------------------------------------------------------------------
		// 設定をコンポーネントに反映
		// --------------------------------------------------------------------
		public virtual void SettingsToCompos()
		{
			// リスト化対象ファイルの拡張子
			ListBoxTargetExts.Items.Clear();
			ListBoxTargetExts.Items.AddRange(OutputSettings.TargetExts.ToArray());

			// ボタン
			UpdateButtonAddExt();
			UpdateButtonRemoveExt();

			// 出力項目
			RadioButtonOutputAllItems.Checked = OutputSettings.OutputAllItems;
			RadioButtonOutputAddedItems.Checked = !OutputSettings.OutputAllItems;
			UpdateOutputItemListBoxes();

			// 出力しない項目
			OutputItems[] aOutputItems = (OutputItems[])Enum.GetValues(typeof(OutputItems));
			for (Int32 i = 0; i < aOutputItems.Length - 1; i++)
			{
				if (!OutputSettings.SelectedOutputItems.Contains(aOutputItems[i]))
				{
					ListBoxRemovedItems.Items.Add(NklCommon.OUTPUT_ITEM_NAMES[(Int32)aOutputItems[i]]);
				}
			}

			// 出力する項目
			for (Int32 i = 0; i < OutputSettings.SelectedOutputItems.Count; i++)
			{
				ListBoxAddedItems.Items.Add(NklCommon.OUTPUT_ITEM_NAMES[(Int32)OutputSettings.SelectedOutputItems[i]]);
			}
		}

		// --------------------------------------------------------------------
		// 設定画面表示
		// --------------------------------------------------------------------
		public DialogResult ShowDialog()
		{
			if (!IsDialogEnabled())
			{
				return DialogResult.Cancel;
			}

			using (FormOutputSettings aFormOutputSettings = new FormOutputSettings(this, LogWriter))
			{
				return aFormOutputSettings.ShowDialog();
			}
		}

		// ====================================================================
		// protected 定数
		// ====================================================================

		// スマートトラックでトラック有りの場合の印
		protected const String SMART_TRACK_VALID_MARK = "○";

		// ====================================================================
		// protected メンバー変数
		// ====================================================================

		// 実際の出力項目
		protected List<OutputItems> mRuntimeOutputItems;

		// コンポーネント
		protected TabPage TabPageOutputSettings;
		protected Label LabelTargetExt;
		protected TextBox TextBoxTargetExt;
		protected Button ButtonAddExt;
		protected Button ButtonRemoveExt;
		protected ListBox ListBoxTargetExts;
		protected Label LabelOutputItem;
		protected RadioButton RadioButtonOutputAllItems;
		protected RadioButton RadioButtonOutputAddedItems;
		protected Label LabelRemovedItems;
		protected ListBox ListBoxRemovedItems;
		protected Button ButtonAddItem;
		protected Button ButtonRemoveItem;
		protected Button ButtonUpItem;
		protected Button ButtonDownItem;
		protected Label LabelAddedItems;
		protected ListBox ListBoxAddedItems;

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// テンプレート読み込み
		// --------------------------------------------------------------------
		protected String LoadTemplate(String oFileNameBody)
		{
			// ユーザーテンプレートがある場合は読み込む
			String aUserTemplate = Path.GetDirectoryName(Application.ExecutablePath) + "\\" + NklCommon.FOLDER_NAME_USER_TEMPLATES
					+ oFileNameBody + Common.FILE_EXT_TPL;
			if (File.Exists(aUserTemplate))
			{
				return File.ReadAllText(aUserTemplate);
			}

			// アプリテンプレートを読み込む
			return File.ReadAllText(Path.GetDirectoryName(Application.ExecutablePath) + "\\" + NklCommon.FOLDER_NAME_TEMPLATES
					+ oFileNameBody + Common.FILE_EXT_TPL);
		}

		// --------------------------------------------------------------------
		// コンストラクターでは行えない準備などを実施
		// --------------------------------------------------------------------
		protected virtual void PrepareOutput()
		{
			// OutputSettings.OutputAllItems に基づく設定（コンストラクターでは OutputSettings がロードされていない）
			if (OutputSettings.OutputAllItems)
			{
				mRuntimeOutputItems = new List<OutputItems>();
				OutputItems[] aOutputItems = (OutputItems[])Enum.GetValues(typeof(OutputItems));
				for (Int32 i = 0; i < aOutputItems.Length - 1; i++)
				{
					mRuntimeOutputItems.Add(aOutputItems[i]);
				}
			}
			else
			{
				mRuntimeOutputItems = OutputSettings.SelectedOutputItems;
			}
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 拡張子をリストボックスに追加
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void AddExt()
		{
			String aExt = TextBoxTargetExt.Text;

			// 入力が空の場合はボタンは押されないはずだが念のため
			if (String.IsNullOrEmpty(aExt))
			{
				throw new Exception("拡張子を入力して下さい。");
			}

			// ワイルドカードは除去
			aExt = aExt.Replace("*", "");
			aExt = aExt.Replace("?", "");

			// 除去で空になっていないか
			if (String.IsNullOrEmpty(aExt))
			{
				throw new Exception("有効な拡張子を入力して下さい。");
			}

			// 先頭にピリオド付加
			if (aExt[0] != '.')
			{
				aExt = "." + aExt;
			}

			// 小文字化
			aExt = aExt.ToLower();

			// 重複チェック
			if (ListBoxTargetExts.Items.Contains(aExt))
			{
				throw new Exception("既に追加されています。");
			}

			// 追加
			ListBoxTargetExts.Items.Add(aExt);
			TextBoxTargetExt.Text = null;
			ListBoxTargetExts.SelectedIndex = ListBoxTargetExts.Items.Count - 1;
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void ButtonAddExt_Click(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				AddExt();
			}
			catch (Exception oExcep)
			{
				NklCommon.ShowLogMessage(TraceEventType.Error, "拡張子追加ボタンクリック時エラー：\n" + oExcep.Message);
				NklCommon.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void ButtonAddItem_Click(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				Int32 aAddItem = Array.IndexOf(NklCommon.OUTPUT_ITEM_NAMES, (String)ListBoxRemovedItems.Items[ListBoxRemovedItems.SelectedIndex]);
				if (aAddItem < 0)
				{
					return;
				}

				ListBoxRemovedItems.Items.RemoveAt(ListBoxRemovedItems.SelectedIndex);
				ListBoxAddedItems.Items.Add(NklCommon.OUTPUT_ITEM_NAMES[aAddItem]);
				ListBoxAddedItems.SelectedIndex = ListBoxAddedItems.Items.Count - 1;
			}
			catch (Exception oExcep)
			{
				NklCommon.ShowLogMessage(TraceEventType.Error, "項目追加ボタンクリック時エラー：\n" + oExcep.Message);
				NklCommon.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void ButtonDownItem_Click(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				Int32 aOrgIndex = ListBoxAddedItems.SelectedIndex;
				if (aOrgIndex < 0 || aOrgIndex >= ListBoxAddedItems.Items.Count - 1)
				{
					return;
				}
				String aItem = (String)ListBoxAddedItems.Items[aOrgIndex];
				ListBoxAddedItems.Items.RemoveAt(aOrgIndex);
				ListBoxAddedItems.Items.Insert(aOrgIndex + 1, aItem);
				ListBoxAddedItems.SelectedIndex = aOrgIndex + 1;
			}
			catch (Exception oExcep)
			{
				NklCommon.ShowLogMessage(TraceEventType.Error, "下へボタンクリック時エラー：\n" + oExcep.Message);
				NklCommon.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void ButtonRemoveExt_Click(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				// 選択されていない場合はボタンが押されないはずだが念のため
				if (ListBoxTargetExts.SelectedIndex < 0)
				{
					throw new Exception("削除したい拡張子を選択してください。");
				}

				// 削除
				ListBoxTargetExts.Items.RemoveAt(ListBoxTargetExts.SelectedIndex);
			}
			catch (Exception oExcep)
			{
				NklCommon.ShowLogMessage(TraceEventType.Error, "拡張子削除ボタンクリック時エラー：\n" + oExcep.Message);
				NklCommon.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void ButtonRemoveItem_Click(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				Int32 aRemoveItem = Array.IndexOf(NklCommon.OUTPUT_ITEM_NAMES, (String)ListBoxAddedItems.Items[ListBoxAddedItems.SelectedIndex]);
				if (aRemoveItem < 0)
				{
					return;
				}

				ListBoxAddedItems.Items.RemoveAt(ListBoxAddedItems.SelectedIndex);
				ListBoxRemovedItems.Items.Add(NklCommon.OUTPUT_ITEM_NAMES[aRemoveItem]);
				ListBoxRemovedItems.SelectedIndex = ListBoxRemovedItems.Items.Count - 1;
			}
			catch (Exception oExcep)
			{
				NklCommon.ShowLogMessage(TraceEventType.Error, "項目削除ボタンクリック時エラー：\n" + oExcep.Message);
				NklCommon.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void ButtonUpItem_Click(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				Int32 aOrgIndex = ListBoxAddedItems.SelectedIndex;
				if (aOrgIndex <= 0)
				{
					return;
				}
				String aItem = (String)ListBoxAddedItems.Items[aOrgIndex];
				ListBoxAddedItems.Items.RemoveAt(aOrgIndex);
				ListBoxAddedItems.Items.Insert(aOrgIndex - 1, aItem);
				ListBoxAddedItems.SelectedIndex = aOrgIndex - 1;
			}
			catch (Exception oExcep)
			{
				NklCommon.ShowLogMessage(TraceEventType.Error, "上へボタンクリック時エラー：\n" + oExcep.Message);
				NklCommon.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void ListBoxAddedItems_SelectedIndexChanged(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				UpdateOutputItemButtons();
			}
			catch (Exception oExcep)
			{
				NklCommon.ShowLogMessage(TraceEventType.Error, "追加項目リスト選択時エラー：\n" + oExcep.Message);
				NklCommon.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void ListBoxRemovedItems_SelectedIndexChanged(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				UpdateButtonAddItem();
			}
			catch (Exception oExcep)
			{
				NklCommon.ShowLogMessage(TraceEventType.Error, "削除項目リスト選択時エラー：\n" + oExcep.Message);
				NklCommon.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void ListBoxTargetExts_SelectedIndexChanged(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				UpdateButtonRemoveExt();
			}
			catch (Exception oExcep)
			{
				NklCommon.ShowLogMessage(TraceEventType.Error, "拡張子選択時エラー：\n" + oExcep.Message);
				NklCommon.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void RadioButtonOutputItems_CheckedChanged(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				UpdateOutputItemListBoxes();
				UpdateButtonAddItem();
				UpdateOutputItemButtons();
			}
			catch (Exception oExcep)
			{
				NklCommon.ShowLogMessage(TraceEventType.Error, "出力項目タイプ選択時エラー：\n" + oExcep.Message);
				NklCommon.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void TextBoxTargetExt_TextChanged(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				UpdateButtonAddExt();
			}
			catch (Exception oExcep)
			{
				NklCommon.ShowLogMessage(TraceEventType.Error, "拡張子入力時エラー：\n" + oExcep.Message);
				NklCommon.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// 追加ボタン（拡張子）の状態を更新
		// --------------------------------------------------------------------
		private void UpdateButtonAddExt()
		{
			ButtonAddExt.Enabled = !String.IsNullOrEmpty(TextBoxTargetExt.Text);
		}

		// --------------------------------------------------------------------
		// 追加ボタン（出力項目）の状態を更新
		// --------------------------------------------------------------------
		private void UpdateButtonAddItem()
		{
			ButtonAddItem.Enabled = RadioButtonOutputAddedItems.Checked && (ListBoxRemovedItems.SelectedIndex >= 0);
		}

		// --------------------------------------------------------------------
		// 削除ボタン（拡張子）の状態を更新
		// --------------------------------------------------------------------
		private void UpdateButtonRemoveExt()
		{
			ButtonRemoveExt.Enabled = ListBoxTargetExts.SelectedIndex >= 0;
		}

		// --------------------------------------------------------------------
		// 出力項目関連ボタン（追加以外）を更新
		// --------------------------------------------------------------------
		private void UpdateOutputItemButtons()
		{
			ButtonRemoveItem.Enabled = RadioButtonOutputAddedItems.Checked && (ListBoxAddedItems.SelectedIndex >= 0);
			ButtonUpItem.Enabled = RadioButtonOutputAddedItems.Checked && ListBoxAddedItems.SelectedIndex > 0;
			ButtonDownItem.Enabled = RadioButtonOutputAddedItems.Checked
					&& 0 <= ListBoxAddedItems.SelectedIndex && ListBoxAddedItems.SelectedIndex < ListBoxAddedItems.Items.Count - 1;
		}

		// --------------------------------------------------------------------
		// 出力項目リスト等を更新
		// --------------------------------------------------------------------
		private void UpdateOutputItemListBoxes()
		{
			LabelRemovedItems.Enabled = RadioButtonOutputAddedItems.Checked;
			ListBoxRemovedItems.Enabled = RadioButtonOutputAddedItems.Checked;
			LabelAddedItems.Enabled = RadioButtonOutputAddedItems.Checked;
			ListBoxAddedItems.Enabled = RadioButtonOutputAddedItems.Checked;
		}



	}
	// public class OutputWriter ___END___

}
// namespace NicoKaraLister.Shared ___END___