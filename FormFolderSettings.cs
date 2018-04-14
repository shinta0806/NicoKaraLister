// ============================================================================
// 
// FolderSettings の設定を行うフォーム
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using NicoKaraLister.Shared;
using Shinta;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace NicoKaraLister
{
	public partial class FormFolderSettings : Form
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FormFolderSettings(String oFolder, NicoKaraListerSettings oNicoKaraListerSettings, OutputSettings oOutputSettings, LogWriter oLogWriter)
		{
			InitializeComponent();

			// 変数初期化
			mFolder = oFolder;
			mNicoKaraListerSettings = oNicoKaraListerSettings;
			mOutputSettings = oOutputSettings;
			mLogWriter = oLogWriter;
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// 設定対象フォルダー
		private String mFolder;

		// 設定が変更された
		private Boolean mIsDirty = false;

		// 楽曲別名 CSV データ（フルパス, 内容）
		private Dictionary<String, List<List<String>>> mSongAliasCsvs;

		// 番組別名 CSV データ（フルパス, 内容）
		private Dictionary<String, List<List<String>>> mProgramAliasCsvs;

		// 楽曲 CSV データ（フルパス, 内容）
		private Dictionary<String, List<List<String>>> mSongCsvs;

		// 番組 CSV データ（フルパス, 内容）
		private Dictionary<String, List<List<String>>> mProgramCsvs;

		// 環境設定
		private NicoKaraListerSettings mNicoKaraListerSettings;

		// 出力設定
		private OutputSettings mOutputSettings;

		// ログ
		private LogWriter mLogWriter;

		// フォルダー設定フォーム上で時間のかかるタスクが多重起動されるのを抑止する
		private Object mTaskLock = new Object();

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// テキストボックスに入力されているファイル命名規則をリストボックスに追加
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void AddFileNameRule()
		{
			CheckFileNameRule(true);

			// 追加
			ListBoxFileNameRules.Items.Add(TextBoxFileNameRule.Text);
			ListBoxFileNameRules.SelectedIndex = ListBoxFileNameRules.Items.Count - 1;
			TextBoxFileNameRule.Text = null;
			mIsDirty = true;
		}

		// --------------------------------------------------------------------
		// テキストボックスに入力されている固定値項目をリストボックスに追加
		// --------------------------------------------------------------------
		private void AddFolderNameRule()
		{
			Int32 aListBoxIndex = ListBoxFolderNameRulesIndex();

			if (aListBoxIndex < 0)
			{
				// 未登録なので新規登録
				ListBoxFolderNameRules.Items.Add(FolderNameRuleFromComponent());
				ListBoxFolderNameRules.SelectedIndex = ListBoxFolderNameRules.Items.Count - 1;
			}
			else
			{
				// 既に登録済みなので置換
				ListBoxFolderNameRules.Items[aListBoxIndex] = FolderNameRuleFromComponent();
				ListBoxFolderNameRules.SelectedIndex = aListBoxIndex;
			}

			TextBoxFolderNameRuleValue.Text = null;
			mIsDirty = true;
		}

		// --------------------------------------------------------------------
		// イベントハンドラー：プレブス一覧の編集ボタンがクリックされた
		// --------------------------------------------------------------------
		private void ButtonEditInfoClicked(Int32 oRowIndex)
		{
			// ファイル命名規則とフォルダー固定値を適用
			FolderSettingsInDisk aFolderSettingsInDisk = NklCommon.LoadFolderSettings(mFolder);
			FolderSettingsInMemory aFolderSettingsInMemory = NklCommon.CreateFolderSettingsInMemory(aFolderSettingsInDisk);
			Dictionary<String, String> aDic = NklCommon.MatchFileNameRulesAndFolderRule
					(Path.GetFileNameWithoutExtension((String)DataGridViewPreview.Rows[oRowIndex].Cells[(Int32)PreviewColumns.File].Value), aFolderSettingsInMemory);

			// 楽曲名が取得できていない場合は編集不可
			if (String.IsNullOrEmpty(aDic[NklCommon.RULE_VAR_TITLE]))
			{
				ShowLogMessage(TraceEventType.Error, "ファイル名から楽曲名を取得できていないため、編集できません。\nファイル命名規則を確認して下さい。");
				return;
			}

			using (FormEditInfo aFormEditInfo = new FormEditInfo(aDic, mNicoKaraListerSettings, mLogWriter))
			{
				aFormEditInfo.ShowDialog(this);
			}
		}

		// --------------------------------------------------------------------
		// テキストボックスに入力されているファイル命名規則が適正か確認
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void CheckFileNameRule(Boolean oCheckSelectedLine)
		{
			// 入力が空の場合はボタンは押されないはずだが念のため
			if (String.IsNullOrEmpty(TextBoxFileNameRule.Text))
			{
				throw new Exception("命名規則が入力されていません。");
			}

			// 変数が含まれているか
			if (TextBoxFileNameRule.Text.IndexOf(NklCommon.RULE_VAR_BEGIN) < 0)
			{
				throw new Exception("命名規則に <変数> が含まれていません。");
			}

			// 既存のものと重複していないか
			foreach (String aRule in ListBoxFileNameRules.Items)
			{
				if (TextBoxFileNameRule.Text == aRule)
				{
					throw new Exception("同じ命名規則が既に追加されています。");
				}
			}

			// 変数・ワイルドカードが隣り合っているとうまく解析できない
			String aNormalizedNewRule = NormalizeRule(TextBoxFileNameRule.Text);
			if (aNormalizedNewRule.IndexOf(NklCommon.RULE_VAR_ANY + NklCommon.RULE_VAR_ANY) >= 0)
			{
				throw new Exception("<変数> や " + NklCommon.RULE_VAR_ANY + " が連続していると正常にファイル名を解析できません。");
			}

			// 競合する命名規則が無いか
			for (Int32 i = 0; i < ListBoxFileNameRules.Items.Count; i++)
			{
				if (ListBoxFileNameRules.GetSelected(i) && !oCheckSelectedLine)
				{
					continue;
				}

				if (NormalizeRule((String)ListBoxFileNameRules.Items[i]) == aNormalizedNewRule)
				{
					throw new Exception("競合する命名規則が既に追加されています：\n" + (String)ListBoxFileNameRules.Items[i]);
				}
			}
		}

		// --------------------------------------------------------------------
		// コンポーネントの値を設定に格納
		// --------------------------------------------------------------------
		private FolderSettingsInDisk ComposToSettings()
		{
			FolderSettingsInDisk aFolderSettings = new FolderSettingsInDisk();

			aFolderSettings.AppVer = NklCommon.APP_VER;

			foreach (String aItem in ListBoxFileNameRules.Items)
			{
				aFolderSettings.FileNameRules.Add(aItem);
			}

			foreach (String aItem in ListBoxFolderNameRules.Items)
			{
				aFolderSettings.FolderNameRules.Add(aItem);
			}

			return aFolderSettings;
		}

		// --------------------------------------------------------------------
		// イベントハンドラー：変数名メニューがクリックされた
		// --------------------------------------------------------------------
		private void ContextMenuVarNamesItem_Click(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				ToolStripItem aItem = (ToolStripItem)oSender;
				String aKey = FindRuleVarName(aItem.Text);
				String aWrappedVarName = WrapVarName(aKey);
				Int32 aSelectionStart = TextBoxFileNameRule.SelectionStart;

				// カーソル位置に挿入
				TextBoxFileNameRule.Text = TextBoxFileNameRule.Text.Substring(0, aSelectionStart) + aWrappedVarName
						+ TextBoxFileNameRule.Text.Substring(aSelectionStart + TextBoxFileNameRule.SelectionLength);

				// <-> ボタンにフォーカスが移っているので戻す
				TextBoxFileNameRule.Focus();
				TextBoxFileNameRule.SelectionStart = aSelectionStart + aWrappedVarName.Length;
				TextBoxFileNameRule.SelectionLength = 0;
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "変数メニュークリック時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}

		}

		// --------------------------------------------------------------------
		// ファイル命名規則の変数の表示用文字列を生成
		// --------------------------------------------------------------------
		private List<String> CreateRuleVarLabels()
		{
			List<String> aLabels = new List<String>();
			TextInfo aTextInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
			Dictionary<String, String> aVarMap = NklCommon.CreateRuleDictionaryWithDescription();
			foreach (KeyValuePair<String, String> aVar in aVarMap)
			{
				String aKey;
				if (aVar.Key == NklCommon.RULE_VAR_ANY)
				{
					aKey = aVar.Key;
				}
				else
				{
					aKey = NklCommon.RULE_VAR_BEGIN + aTextInfo.ToTitleCase(aVar.Key) + NklCommon.RULE_VAR_END;
				}
				aLabels.Add(aKey + "（" + aVar.Value + "）");
			}
			return aLabels;
		}

		// --------------------------------------------------------------------
		// UI 無効化（時間のかかる処理実行時用）
		// --------------------------------------------------------------------
		private void DisableComponents()
		{
			Invoke(new Action(() =>
			{
				TabControlRules.Enabled = false;
				ButtonPreview.Enabled = false;
				ButtonDeleteSettings.Enabled = false;
				ButtonOK.Enabled = false;
			}));
		}

		// --------------------------------------------------------------------
		// UI 有効化
		// --------------------------------------------------------------------
		private void EnableComponents()
		{
			Invoke(new Action(() =>
			{
				TabControlRules.Enabled = true;
				ButtonPreview.Enabled = true;

				// ButtonDeleteSettings は状況によって状態が異なる
				UpdateSettingsFileStatus();

				ButtonOK.Enabled = true;
			}));
		}

		// --------------------------------------------------------------------
		// 文字列の中に含まれている命名規則の変数名を返す
		// 文字列の中には <Name> 形式で変数名を含んでいる必要がある
		// 返す変数名には <> は含まない
		// --------------------------------------------------------------------
		private String FindRuleVarName(String oString)
		{
			Dictionary<String, String> aVarMap = NklCommon.CreateRuleDictionary();
			foreach (String aKey in aVarMap.Keys)
			{
				if (oString.IndexOf(NklCommon.RULE_VAR_BEGIN + aKey + NklCommon.RULE_VAR_END, StringComparison.CurrentCultureIgnoreCase) >= 0)
				{
					return aKey;
				}
			}
			if (oString.IndexOf(NklCommon.RULE_VAR_ANY) >= 0)
			{
				return NklCommon.RULE_VAR_ANY;
			}
			return null;
		}

		// --------------------------------------------------------------------
		// 入力された固定値
		// --------------------------------------------------------------------
		private String FolderNameRuleFromComponent()
		{
			String aKey = FindRuleVarName((String)ComboBoxFolderNameRuleName.Items[ComboBoxFolderNameRuleName.SelectedIndex]);
			return WrapVarName(aKey) + "=" + TextBoxFolderNameRuleValue.Text;
		}

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// タイトルバー
			UpdateTitleBar();

			// ラベル
			LabelFolder.Text = mFolder;

			// <-> ボタン
			List<String> aLabels = CreateRuleVarLabels();
			foreach (String aLabel in aLabels)
			{
				ContextMenuVarNames.Items.Add(aLabel, null, ContextMenuVarNamesItem_Click);
			}

			// 項目
			foreach (String aLabel in aLabels)
			{
				if (aLabel.IndexOf(NklCommon.RULE_VAR_ANY) < 0)
				{
					ComboBoxFolderNameRuleName.Items.Add(aLabel);
				}
			}
			ComboBoxFolderNameRuleName.SelectedIndex = 0;

			// データグリッドビュー
			InitDataGridView();

			// 設計時サイズ以下にできないようにする
			MinimumSize = Size;

			Common.CascadeForm(this);
		}

		// --------------------------------------------------------------------
		// データグリッドビュー初期化
		// --------------------------------------------------------------------
		private void InitDataGridView()
		{
			// ファイル
			ColumnFile.Width = 250;

			// 項目と値
			ColumnAnalyze.Width = 400;

			// 編集
			ColumnEdit.Width = 50;
		}

		// --------------------------------------------------------------------
		// 編集する必要がありそうなファイルに飛ぶ
		// --------------------------------------------------------------------
		private void JumpToNextCandidate(Object oDummy)
		{
			try
			{
				// 準備
				DisableComponents();
				SetCursor(Cursors.WaitCursor);

				// 初めての場合は CSV をロード
				if (mProgramCsvs == null)
				{
					NklCommon.LoadCsvs(mNicoKaraListerSettings, out mProgramCsvs, out mSongCsvs, out mProgramAliasCsvs, out mSongAliasCsvs);
				}

				Invoke(new Action(() =>
				{
					Int32 aRowIndex = -1;
					if (DataGridViewPreview.SelectedRows.Count > 0)
					{
						aRowIndex = DataGridViewPreview.SelectedRows[0].Index;
					}

					// マッチ準備
					FolderSettingsInDisk aFolderSettingsInDisk = NklCommon.LoadFolderSettings(mFolder);
					FolderSettingsInMemory aFolderSettingsInMemory = NklCommon.CreateFolderSettingsInMemory(aFolderSettingsInDisk);

					for (; ; )
					{
						aRowIndex++;
						if (aRowIndex >= DataGridViewPreview.RowCount)
						{
							ShowLogMessage(TraceEventType.Information, "ファイル名から取得した情報が楽曲情報・番組情報に未登録のファイルは見つかりませんでした。");
							DataGridViewPreview.ClearSelection();
							return;
						}

						// ファイル命名規則とフォルダー固定値を適用
						Dictionary<String, String> aDic = NklCommon.MatchFileNameRulesAndFolderRule(
								Path.GetFileNameWithoutExtension((String)DataGridViewPreview.Rows[aRowIndex].Cells[(Int32)PreviewColumns.File].Value), aFolderSettingsInMemory);

						// 楽曲名が空かどうか
						if (String.IsNullOrEmpty(aDic[NklCommon.RULE_VAR_TITLE]))
						{
							break;
						}

						// 楽曲名が anison.info と不一致かどうか
						String aSongOrigin = SongOriginCsv(aDic[NklCommon.RULE_VAR_TITLE]);
						List<String> aSongCsvRecord = NklCommon.FindCsvRecord(mSongCsvs, (Int32)SongCsvColumns.Name, aSongOrigin);
						if (aSongCsvRecord == null)
						{
							break;
						}

						// 番組名がある場合、番組名が anison.info と不一致かどうか
						if (!String.IsNullOrEmpty(aDic[NklCommon.RULE_VAR_PROGRAM]))
						{
							String aProgramOrigin = ProgramOriginCsv(aDic[NklCommon.RULE_VAR_PROGRAM]);
							List<String> aProgramCsvRecord = NklCommon.FindCsvRecord(mProgramCsvs, (Int32)ProgramCsvColumns.Name, aProgramOrigin);
							if (aProgramCsvRecord == null)
							{
								break;
							}
						}
					}

					DataGridViewPreview.Rows[aRowIndex].Selected = true;

					// 検出行が完全に表示されていない場合はスクロールする
					Int32 aBeforeRowIndex = aRowIndex > 0 ? aRowIndex - 1 : aRowIndex;
					Int32 aAfterRowIndex = aRowIndex < DataGridViewPreview.RowCount - 1 ? aRowIndex + 1 : aRowIndex;
					if (!DataGridViewPreview.Rows[aBeforeRowIndex].Displayed || !DataGridViewPreview.Rows[aAfterRowIndex].Displayed)
					{
						DataGridViewPreview.FirstDisplayedScrollingRowIndex = aRowIndex;
					}
				}));
			}
			catch (OperationCanceledException)
			{
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "未登録検出を中止しました。");
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "未登録検出時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				// 後片付け
				SetCursor(Cursors.Default);
				EnableComponents();
			}
		}

		// --------------------------------------------------------------------
		// コンボボックスで選択されている項目は、リストボックスの何番目に登録されているか
		// --------------------------------------------------------------------
		private Int32 ListBoxFolderNameRulesIndex()
		{
			String aKey = FindRuleVarName((String)ComboBoxFolderNameRuleName.Items[ComboBoxFolderNameRuleName.SelectedIndex]);
			String aVarName = WrapVarName(aKey);
			for (Int32 i = 0; i < ListBoxFolderNameRules.Items.Count; i++)
			{
				if (((String)ListBoxFolderNameRules.Items[i]).IndexOf(aVarName) == 0)
				{
					return i;
				}
			}
			return -1;
		}

		// --------------------------------------------------------------------
		// 命名規則の変数部分を全てワイルドカードにする
		// --------------------------------------------------------------------
		private String NormalizeRule(String oRule)
		{
			return Regex.Replace(oRule, @"\<.*?\>", NklCommon.RULE_VAR_ANY);
		}

		// --------------------------------------------------------------------
		// FormNicoKaraLister.ProgramOrigin() の CSV 版
		// 番組 ID 検索アルゴリズムは CreateInfoDbProgramAliasTableInsert() と同様とする
		// --------------------------------------------------------------------
		private String ProgramOriginCsv(String oProgram)
		{
			List<String> aProgramAliasRecord = NklCommon.FindCsvRecord(mProgramAliasCsvs, (Int32)ProgramAliasCsvColumns.Alias, oProgram);
			if (aProgramAliasRecord == null)
			{
				return oProgram;
			}

			// 番組名が指定されたものとして番組 ID を検索
			List<String> aProgramRecord;
			if (String.IsNullOrEmpty(aProgramAliasRecord[(Int32)ProgramAliasCsvColumns.ForceId]))
			{
				aProgramRecord = NklCommon.FindCsvRecord(mProgramCsvs, (Int32)ProgramCsvColumns.Name, aProgramAliasRecord[(Int32)ProgramAliasCsvColumns.NameOrId]);
				if (aProgramRecord != null)
				{
					return aProgramRecord[(Int32)ProgramCsvColumns.Name];
				}
			}

			// 番組 ID が指定されたものとして番組 ID を検索
			aProgramRecord = NklCommon.FindCsvRecord(mProgramCsvs, (Int32)ProgramCsvColumns.Id, aProgramAliasRecord[(Int32)ProgramAliasCsvColumns.NameOrId]);
			if (aProgramRecord != null)
			{
				return aProgramRecord[(Int32)ProgramCsvColumns.Name];
			}

			return oProgram;
		}

		// --------------------------------------------------------------------
		// 設定が更新されていれば保存
		// ＜例外＞ OperationCanceledException, Exception
		// --------------------------------------------------------------------
		private void SaveSettingsIfNeeded()
		{
			// 設定途中のものを確認
			if (!String.IsNullOrEmpty(TextBoxFileNameRule.Text))
			{
				switch (MessageBox.Show("ファイル命名規則に入力中の\n" + TextBoxFileNameRule.Text + "\nはまだ命名規則として追加されていません。\n追加しますか？",
						"確認", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation))
				{
					case DialogResult.Yes:
						AddFileNameRule();
						break;
					case DialogResult.No:
						break;
					case DialogResult.Cancel:
						throw new OperationCanceledException("保存を中止しました。");
				}
			}
			if (!String.IsNullOrEmpty(TextBoxFolderNameRuleValue.Text))
			{
				switch (MessageBox.Show("固定値項目に入力中の\n" + TextBoxFolderNameRuleValue.Text + "\nはまだ固定値として追加されていません。\n追加しますか？",
						"確認", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation))
				{
					case DialogResult.Yes:
						AddFolderNameRule();
						break;
					case DialogResult.No:
						break;
					case DialogResult.Cancel:
						throw new OperationCanceledException("保存を中止しました。");
				}
			}

			if (!mIsDirty)
			{
				return;
			}

			FolderSettingsInDisk aFolderSettings = ComposToSettings();
			Common.Serialize(mFolder + "\\" + NklCommon.FILE_NAME_NICO_KARA_LISTER_CONFIG, aFolderSettings);
			UpdateSettingsFileStatus();
		}

		// --------------------------------------------------------------------
		// カーソル形状の設定
		// --------------------------------------------------------------------
		private void SetCursor(Cursor oCursor)
		{
			Invoke(new Action(() =>
			{
				Capture = true;
				Cursor = oCursor;
				Capture = false;
			}));
		}

		// --------------------------------------------------------------------
		// 設定をコンポーネントに反映
		// --------------------------------------------------------------------
		private void SettingsToCompos()
		{
			// クリア
			ListBoxFileNameRules.Items.Clear();
			ListBoxFolderNameRules.Items.Clear();

			// 設定
			FolderSettingsInDisk aSettings = NklCommon.LoadFolderSettings(mFolder);
			foreach (String aFileNameRule in aSettings.FileNameRules)
			{
				ListBoxFileNameRules.Items.Add(aFileNameRule);
			}
			foreach (String aFolderNameRule in aSettings.FolderNameRules)
			{
				ListBoxFolderNameRules.Items.Add(aFolderNameRule);
			}
		}

		// --------------------------------------------------------------------
		// このフォームを親としてログ表示関数を呼びだす
		// --------------------------------------------------------------------
		private DialogResult ShowLogMessage(TraceEventType oEventType, String oMessage, Boolean oSuppressMessageBox = false)
		{
			mLogWriter.FrontForm = this;
			return mLogWriter.ShowLogMessage(oEventType, oMessage, oSuppressMessageBox);
		}

		// --------------------------------------------------------------------
		// FormNicoKaraLister.SongOrigin() の CSV 版
		// 楽曲 ID 検索アルゴリズムは CreateInfoDbSongAliasTableInsert() と同様とする
		// --------------------------------------------------------------------
		private String SongOriginCsv(String oTitle)
		{
			List<String> aSongAliasRecord = NklCommon.FindCsvRecord(mSongAliasCsvs, (Int32)SongAliasCsvColumns.Alias, oTitle);
			if (aSongAliasRecord == null)
			{
				return oTitle;
			}

			// 楽曲名が指定されたものとして楽曲 ID を検索
			List<String> aSongRecord;
			if (String.IsNullOrEmpty(aSongAliasRecord[(Int32)SongAliasCsvColumns.ForceId]))
			{
				aSongRecord = NklCommon.FindCsvRecord(mSongCsvs, (Int32)SongCsvColumns.Name, aSongAliasRecord[(Int32)SongAliasCsvColumns.NameOrId]);
				if (aSongRecord != null)
				{
					return aSongRecord[(Int32)SongCsvColumns.Name];
				}
			}

			// 楽曲 ID が指定されたものとして楽曲 ID を検索
			aSongRecord = NklCommon.FindCsvRecord(mSongCsvs, (Int32)SongCsvColumns.Id, aSongAliasRecord[(Int32)SongAliasCsvColumns.NameOrId]);
			if (aSongRecord != null)
			{
				return aSongRecord[(Int32)SongCsvColumns.Name];
			}

			return oTitle;
		}

		// --------------------------------------------------------------------
		// リストボックスの 2 つのアイテムを入れ替える
		// --------------------------------------------------------------------
		private void SwapListItem(ListBox oListBox, Int32 oLhsIndex, Int32 oRhsIndex)
		{
			String aTmp = (String)oListBox.Items[oLhsIndex];
			oListBox.Items[oLhsIndex] = oListBox.Items[oRhsIndex];
			oListBox.Items[oRhsIndex] = aTmp;
		}

		// --------------------------------------------------------------------
		// データグリッドビューを更新
		// --------------------------------------------------------------------
		private void UpdateButtonJump()
		{
			ButtonJump.Enabled = DataGridViewPreview.Rows.Count > 0;
		}

		// --------------------------------------------------------------------
		// データグリッドビューを更新
		// --------------------------------------------------------------------
		private void UpdateDataGridViewPreview(Object oDummy)
		{
			try
			{
				// 準備
				DisableComponents();
				SetCursor(Cursors.WaitCursor);

				Invoke(new Action(() =>
				{
					// クリア
					DataGridViewPreview.Rows.Clear();

					// 検索
					String[] aAllPathes = Directory.GetFiles(mFolder);

					// マッチをリストに追加
					FolderSettingsInDisk aFolderSettingsInDisk = NklCommon.LoadFolderSettings(mFolder);
					FolderSettingsInMemory aFolderSettingsInMemory = NklCommon.CreateFolderSettingsInMemory(aFolderSettingsInDisk);
					Dictionary<String, String> aRuleMap = NklCommon.CreateRuleDictionaryWithDescription();
					foreach (String aPath in aAllPathes)
					{
						if (!mOutputSettings.TargetExts.Contains(Path.GetExtension(aPath).ToLower()))
						{
							continue;
						}

						// ファイル命名規則とフォルダー固定値を適用
						Dictionary<String, String> aDic = NklCommon.MatchFileNameRulesAndFolderRule(Path.GetFileNameWithoutExtension(aPath), aFolderSettingsInMemory);

						// DGV 追加
						DataGridViewPreview.Rows.Add();
						Int32 aIndex = DataGridViewPreview.Rows.Count - 1;

						// ファイル
						DataGridViewPreview.Rows[aIndex].Cells[(Int32)PreviewColumns.File].Value = Path.GetFileName(aPath);

						// 項目と値
						StringBuilder aSB = new StringBuilder();
						foreach (KeyValuePair<String, String> aKvp in aDic)
						{
							if (aKvp.Key != NklCommon.RULE_VAR_ANY && !String.IsNullOrEmpty(aKvp.Value))
							{
								aSB.Append(aRuleMap[aKvp.Key] + "=" + aKvp.Value + ", ");
							}
						}
						DataGridViewPreview.Rows[aIndex].Cells[(Int32)PreviewColumns.Matches].Value = aSB.ToString();

						// 編集
						DataGridViewPreview.Rows[aIndex].Cells[(Int32)PreviewColumns.Edit].Value = "編集";
					}

					// 選択解除
					DataGridViewPreview.ClearSelection();

					// 次の編集候補ボタン
					UpdateButtonJump();
				}));
			}
			catch (OperationCanceledException)
			{
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "ファイル検索結果更新を中止しました。");
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "ファイル検索結果更新更新時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				// 後片付け
				SetCursor(Cursors.Default);
				EnableComponents();
			}
		}

		// --------------------------------------------------------------------
		// 設定ファイルの状況を表示
		// --------------------------------------------------------------------
		private void UpdateSettingsFileStatus()
		{
			FolderSettingsStatus aStatus = NklCommon.FolderSettingsStatus(mFolder);

			switch (aStatus)
			{
				case FolderSettingsStatus.None:
					LabelSettingsFileStatus.Text = "このフォルダーの設定がありません。";
					ButtonDeleteSettings.Enabled = false;
					break;
				case FolderSettingsStatus.Set:
					LabelSettingsFileStatus.Text = "このフォルダーは設定済みです。";
					ButtonDeleteSettings.Enabled = true;
					break;
				case FolderSettingsStatus.Inherit:
					LabelSettingsFileStatus.Text = "親フォルダーの設定を参照しています（設定を変更しても親フォルダーには影響ありません）。";
					ButtonDeleteSettings.Enabled = false;
					break;
				default:
					Debug.Assert(false, "UpdateLabelSettingsFileStatus() bad FolderSettingsStatus");
					break;
			}
		}

		// --------------------------------------------------------------------
		// タブ内のコンポーネントの状態を更新
		// --------------------------------------------------------------------
		private void UpdateTabControlRules()
		{
			// ファイル名命名規則
			ButtonAddFileNameRule.Enabled = !String.IsNullOrEmpty(TextBoxFileNameRule.Text);
			ButtonReplaceFileNameRule.Enabled = !String.IsNullOrEmpty(TextBoxFileNameRule.Text) && (ListBoxFileNameRules.SelectedIndex >= 0);
			ButtonDeleteFileNameRule.Enabled = (ListBoxFileNameRules.SelectedIndex >= 0);
			ButtonUpFileNameRule.Enabled = (ListBoxFileNameRules.SelectedIndex > 0);
			ButtonDownFileNameRule.Enabled = (0 <= ListBoxFileNameRules.SelectedIndex && ListBoxFileNameRules.SelectedIndex < ListBoxFileNameRules.Items.Count - 1);

			// フォルダー固定値
			ButtonAddFolderNameRule.Enabled = !String.IsNullOrEmpty(TextBoxFolderNameRuleValue.Text);
			ButtonDeleteFolderNameRule.Enabled = (ListBoxFolderNameRules.SelectedIndex >= 0);
			ButtonUpFolderNameRule.Enabled = (ListBoxFolderNameRules.SelectedIndex > 0);
			ButtonDownFolderNameRule.Enabled = (0 <= ListBoxFolderNameRules.SelectedIndex && ListBoxFolderNameRules.SelectedIndex < ListBoxFolderNameRules.Items.Count - 1);
		}

		// --------------------------------------------------------------------
		// タイトルバーを更新
		// --------------------------------------------------------------------
		private void UpdateTitleBar()
		{
			Text = "フォルダー設定";
#if DEBUG
			Text = "［デバッグ］" + Text;
#endif
		}

		// --------------------------------------------------------------------
		// 変数名を <> で囲む
		// --------------------------------------------------------------------
		private String WrapVarName(String oVarName)
		{
			if (oVarName == NklCommon.RULE_VAR_ANY)
			{
				return NklCommon.RULE_VAR_ANY;
			}
			else
			{
				TextInfo aTextInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
				return NklCommon.RULE_VAR_BEGIN + aTextInfo.ToTitleCase(oVarName) + NklCommon.RULE_VAR_END;
			}
		}


		// ====================================================================
		// IDE 生成イベントハンドラー
		// ====================================================================

		private void FormFolderSettings_Load(object sender, EventArgs e)
		{
			try
			{
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "フォルダー設定フォームを開きます。");
				Init();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "フォルダー設定フォームロード時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormFolderSettings_Shown(object sender, EventArgs e)
		{
			try
			{
				UpdateSettingsFileStatus();
				SettingsToCompos();
				UpdateTabControlRules();
				UpdateButtonJump();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "フォルダー設定フォーム表示時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonVar_Click(object sender, EventArgs e)
		{
			try
			{
				ContextMenuVarNames.Show(Cursor.Position);
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "変数メニュー表示時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonAddFileNameRule_Click(object sender, EventArgs e)
		{
			try
			{
				AddFileNameRule();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "ファイル名命名規則追加時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TextBoxFileNameRule_TextChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateTabControlRules();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "ファイル名命名規則テキストボックス変更時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ListBoxFileNameRules_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateTabControlRules();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "ファイル名命名規則リストボックス選択時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonReplaceFileNameRule_Click(object sender, EventArgs e)
		{
			try
			{
				CheckFileNameRule(false);

				// 置換
				ListBoxFileNameRules.Items[ListBoxFileNameRules.SelectedIndex] = TextBoxFileNameRule.Text;
				TextBoxFileNameRule.Text = null;
				mIsDirty = true;
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "ファイル名命名規則置換時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonDeleteFileNameRule_Click(object sender, EventArgs e)
		{
			try
			{
				ListBoxFileNameRules.Items.RemoveAt(ListBoxFileNameRules.SelectedIndex);
				mIsDirty = true;
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "ファイル名命名規則削除時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonUpFileNameRule_Click(object sender, EventArgs e)
		{
			try
			{
				SwapListItem(ListBoxFileNameRules, ListBoxFileNameRules.SelectedIndex - 1, ListBoxFileNameRules.SelectedIndex);
				ListBoxFileNameRules.SelectedIndex -= 1;
				mIsDirty = true;
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "ファイル名命名規則順番繰り上げ時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonDownFileNameRule_Click(object sender, EventArgs e)
		{
			try
			{
				SwapListItem(ListBoxFileNameRules, ListBoxFileNameRules.SelectedIndex + 1, ListBoxFileNameRules.SelectedIndex);
				ListBoxFileNameRules.SelectedIndex += 1;
				mIsDirty = true;
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "ファイル名命名規則順番繰り下げ時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private async void ButtonPreview_Click(object sender, EventArgs e)
		{
			try
			{
				// 保存
				SaveSettingsIfNeeded();

				// 検索
				await NklCommon.LaunchTaskAsync<Object>(UpdateDataGridViewPreview, mTaskLock, null);
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "ファイル検索時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TextBoxFolderNameRuleValue_TextChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateTabControlRules();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "固定値項目テキストボックス変更時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ListBoxFolderNameRules_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateTabControlRules();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "固定値項目リストボックス選択時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonAddFolderNameRule_Click(object sender, EventArgs e)
		{
			try
			{
				AddFolderNameRule();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "固定値項目追加時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ComboBoxFolderNameRuleName_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				// リストボックスにコンボボックスと同じ項目があれば選択する
				ListBoxFolderNameRules.SelectedIndex = ListBoxFolderNameRulesIndex();

				String aRuleName = FindRuleVarName((String)ComboBoxFolderNameRuleName.Items[ComboBoxFolderNameRuleName.SelectedIndex]);
				if (aRuleName == NklCommon.RULE_VAR_ON_VOCAL || aRuleName == NklCommon.RULE_VAR_OFF_VOCAL)
				{
					// オンボ・オフボの場合は規定値を入力
					TextBoxFolderNameRuleValue.Text = NklCommon.RULE_VALUE_VOCAL_DEFAULT.ToString();
				}
				else
				{
					// オンボ・オフボの規定値を解除
					if (TextBoxFolderNameRuleValue.Text == NklCommon.RULE_VALUE_VOCAL_DEFAULT.ToString())
					{
						TextBoxFolderNameRuleValue.Text = null;
					}
				}

			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "固定値項目名選択時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonDeleteFolderNameRule_Click(object sender, EventArgs e)
		{
			try
			{
				ListBoxFolderNameRules.Items.RemoveAt(ListBoxFolderNameRules.SelectedIndex);
				mIsDirty = true;
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "固定値項目削除時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonUpFolderNameRule_Click(object sender, EventArgs e)
		{
			try
			{
				SwapListItem(ListBoxFolderNameRules, ListBoxFolderNameRules.SelectedIndex - 1, ListBoxFolderNameRules.SelectedIndex);
				ListBoxFolderNameRules.SelectedIndex -= 1;
				mIsDirty = true;
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "固定値項目順番繰り上げ時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonDownFolderNameRule_Click(object sender, EventArgs e)
		{
			try
			{
				SwapListItem(ListBoxFolderNameRules, ListBoxFolderNameRules.SelectedIndex + 1, ListBoxFolderNameRules.SelectedIndex);
				ListBoxFolderNameRules.SelectedIndex += 1;
				mIsDirty = true;
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "固定値項目順番繰り下げ時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ListBoxFileNameRules_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				if (ListBoxFileNameRules.SelectedIndex < 0)
				{
					return;
				}
				TextBoxFileNameRule.Text = (String)ListBoxFileNameRules.Items[ListBoxFileNameRules.SelectedIndex];
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "ファイル名命名規則ダブルクリック時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ListBoxFolderNameRules_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				if (ListBoxFolderNameRules.SelectedIndex < 0)
				{
					return;
				}

				// コンボボックス設定
				String aKey = FindRuleVarName((String)ListBoxFolderNameRules.Items[ListBoxFolderNameRules.SelectedIndex]);
				if (String.IsNullOrEmpty(aKey))
				{
					return;
				}
				String aVarName = WrapVarName(aKey);
				for (Int32 i = 0; i < ComboBoxFolderNameRuleName.Items.Count; i++)
				{
					if (ComboBoxFolderNameRuleName.Items[i].ToString().IndexOf(aVarName) == 0)
					{
						ComboBoxFolderNameRuleName.SelectedIndex = i;
						break;
					}
				}

				// テキストボックス設定
				String aRule = (String)ListBoxFolderNameRules.Items[ListBoxFolderNameRules.SelectedIndex];
				Int32 aEqualPos = aRule.IndexOf('=');
				TextBoxFolderNameRuleValue.Text = aRule.Substring(aEqualPos + 1);
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "固定値項目ダブルクリック時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonHelp_Click(object sender, EventArgs e)
		{
			try
			{
				NklCommon.ShowHelp("FolderSettei");
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "ヘルプボタン（フォルダー設定フォーム）クリック時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonDeleteSettings_Click(object sender, EventArgs e)
		{
			try
			{
				if (MessageBox.Show("フォルダー設定を削除します。\nよろしいですか？", "確認",
						MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
				{
					return;
				}

				File.Delete(mFolder + "\\" + NklCommon.FILE_NAME_NICO_KARA_LISTER_CONFIG);

				// UI に反映（フォーム Shown() と同様の処理）
				UpdateSettingsFileStatus();
				SettingsToCompos();
				UpdateTabControlRules();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "設定削除ボタンクリック時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			try
			{
				SaveSettingsIfNeeded();
				DialogResult = DialogResult.OK;
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "フォルダー設定保存時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void DataGridViewPreview_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
				if (e.ColumnIndex == (Int32)PreviewColumns.Edit)
				{
					ButtonEditInfoClicked(e.RowIndex);
				}
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "プレビュー一覧クリック時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormFolderSettings_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "フォルダー設定フォームを閉じました。");
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "フォルダー設定フォームクローズ時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private async void ButtonJump_Click(object sender, EventArgs e)
		{
			try
			{
				await NklCommon.LaunchTaskAsync<Object>(JumpToNextCandidate, mTaskLock, null);
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "未登録検出クリック時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
	// public partial class FormFolderSettings ___END___

}
// namespace NicoKaraLister ___END___