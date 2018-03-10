// ============================================================================
// 
// 楽曲情報と番組情報の編集を行うフォーム
// 
// ============================================================================

// ----------------------------------------------------------------------------
// このフォームは SQLite DB ではなく CSV にアクセスする（SQLite DB だと情報が古い場合があるため）
// 読み込みは CSVs フォルダーおよび UserCSVs フォルダー、書き込みは UserCSVs フォルダーのみ
// 楽曲名はファイル命名規則（またはフォルダー固定値）で指定必須
// ----------------------------------------------------------------------------

using NicoKaraLister.Shared;
using Shinta;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace NicoKaraLister
{
	public partial class FormEditInfo : Form
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FormEditInfo(Dictionary<String, String> oDicByFile, NicoKaraListerSettings oNicoKaraListerSettings, LogWriter oLogWriter)
		{
			InitializeComponent();

			// 変数初期化
			mDicByFile = oDicByFile;
			mNicoKaraListerSettings = oNicoKaraListerSettings;
			mLogWriter = oLogWriter;
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// ID 接頭辞の次に来る接頭辞
		private const String SONG_ID_SECOND_PREFIX = "_S_";
		private const String PROGRAM_ID_SECOND_PREFIX = "_P_";

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// ファイル名・フォルダー固定値から取得した情報
		private Dictionary<String, String> mDicByFile;

		// 楽曲別名 CSV データ（フルパス, 内容）
		private Dictionary<String, List<List<String>>> mSongAliasCsvs;

		// 番組別名 CSV データ（フルパス, 内容）
		private Dictionary<String, List<List<String>>> mProgramAliasCsvs;

		// 楽曲 CSV データ（フルパス, 内容）
		private Dictionary<String, List<List<String>>> mSongCsvs;

		// 番組 CSV データ（フルパス, 内容）
		private Dictionary<String, List<List<String>>> mProgramCsvs;

		// 楽曲 CSV データの中からヒットしたレコード
		private List<List<String>> mMatchedSongs;

		// 番組 CSV データの中からヒットしたレコード
		private List<List<String>> mMatchedPrograms;

		// 別名適用後に楽曲 CSV データの中からヒットしたレコード
		private List<List<String>> mMatchedSongsWithAlias;

		// 別名適用後に番組 CSV データの中からヒットしたレコード
		private List<List<String>> mMatchedProgramsWithAlias;

		// 楽曲新規 ID 選択時の挙動制御用
		private Int32 mComboBoxSongIdLastSelectedIndex;

		// 番組新規 ID 選択時の挙動制御用
		private Int32 mComboBoxProgramIdLastSelectedIndex;

		// 環境設定
		private NicoKaraListerSettings mNicoKaraListerSettings;

		// ログ
		private LogWriter mLogWriter;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 適用可能な番組名の別名を検索してコンポーネントに反映
		// --------------------------------------------------------------------
		private void ApplyProgramAlias()
		{
			if (String.IsNullOrEmpty(mDicByFile[NklCommon.RULE_VAR_PROGRAM]))
			{
				return;
			}

			List<String> aRecord = NklCommon.FindCsvRecord(mProgramAliasCsvs, (Int32)ProgramAliasCsvColumns.Alias, mDicByFile[NklCommon.RULE_VAR_PROGRAM]);
			if (aRecord != null)
			{
				CheckBoxUseProgramAlias.Checked = true;
				TextBoxProgramNameOrigin.Text = ProgramOrigin(aRecord);
			}
		}

		// --------------------------------------------------------------------
		// 適用可能な楽曲名の別名を検索してコンポーネントに反映
		// --------------------------------------------------------------------
		private void ApplySongAlias()
		{
			if (String.IsNullOrEmpty(mDicByFile[NklCommon.RULE_VAR_TITLE]))
			{
				return;
			}

			List<String> aRecord = NklCommon.FindCsvRecord(mSongAliasCsvs, (Int32)SongAliasCsvColumns.Alias, mDicByFile[NklCommon.RULE_VAR_TITLE]);
			if (aRecord != null)
			{
				CheckBoxUseSongAlias.Checked = true;
				TextBoxTitleOrigin.Text = SongOrigin(aRecord);
			}
		}

		// --------------------------------------------------------------------
		// ユーザーに入力された値が適切か確認
		// 新規 ID は発行済みの前提
		// ＜例外＞ OperationCanceledException
		// --------------------------------------------------------------------
		private void CheckInput(List<String> oInputSongAliasRecord, List<String> oInputProgramAliasRecord, List<String> oInputSongRecord, List<String> oInputProgramRecord)
		{
			try
			{
				// 楽曲名エイリアスの確認
				if (CheckBoxUseSongAlias.Checked)
				{
					if (String.IsNullOrEmpty(oInputSongAliasRecord[(Int32)SongAliasCsvColumns.NameOrId]))
					{
						throw new Exception("楽曲名の正式名称を入力して下さい。");
					}
					if (oInputSongAliasRecord[(Int32)SongAliasCsvColumns.Alias] == oInputSongAliasRecord[(Int32)SongAliasCsvColumns.NameOrId])
					{
						throw new Exception("楽曲名と正式名称が同じです。\n修正不要の場合は、「楽曲名を修正する」のチェックを外して下さい。");
					}

					// 楽曲名→ID
					Debug.Assert(!String.IsNullOrEmpty(oInputSongRecord[(Int32)SongCsvColumns.Id]), "CheckInput() 楽曲 ID が空");
					oInputSongAliasRecord[(Int32)SongAliasCsvColumns.NameOrId] = oInputSongRecord[(Int32)SongCsvColumns.Id];
					oInputSongAliasRecord[(Int32)SongAliasCsvColumns.ForceId] = "1";
				}

				// ファイル名等から取得した番組名（およびその正式名称）と、楽曲の関連番組が異なる場合
				String aProgramNameByFile = CheckBoxUseProgramAlias.Checked ? TextBoxProgramNameOrigin.Text : mDicByFile[NklCommon.RULE_VAR_PROGRAM];
				if (!String.IsNullOrEmpty(aProgramNameByFile) && aProgramNameByFile != oInputSongRecord[(Int32)SongCsvColumns.ProgramName])
				{
					if (MessageBox.Show("ファイル名・フォルダー固定値から取得した番組名と、楽曲名に関連付けられている番組名が異なっています。\n番組名を自動で修正しますか？",
							"確認", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
					{
						// 修正
						CheckBoxUseProgramAlias.Checked = true;
						TextBoxProgramNameOrigin.Text = oInputSongRecord[(Int32)SongCsvColumns.ProgramName];

						// 処理を最初からやり直し（入力項目を変えたので、取り込み・チェックを再度行うため）
						Invoke(new Action(() =>
						{
							ButtonOK.PerformClick();
						}));

						// 今回の処理はキャンセル
						throw new OperationCanceledException();
					}
					else
					{
						throw new Exception("番組名を修正して下さい。");
					}
				}

				// 楽曲の関連番組
				if (oInputSongRecord[(Int32)SongCsvColumns.ProgramName] != oInputProgramRecord[(Int32)ProgramCsvColumns.Name])
				{
					throw new Exception("楽曲名に関連付けられている番組名と、番組情報で入力されている番組名が異なっています。");
				}

				// 番組名エイリアスの確認
				if (CheckBoxUseProgramAlias.Checked)
				{
					if (String.IsNullOrEmpty(oInputProgramAliasRecord[(Int32)ProgramAliasCsvColumns.NameOrId]))
					{
						throw new Exception("番組名の正式名称を入力して下さい。");
					}
					if (oInputProgramAliasRecord[(Int32)ProgramAliasCsvColumns.Alias] == oInputProgramAliasRecord[(Int32)ProgramAliasCsvColumns.NameOrId])
					{
						throw new Exception("番組名と正式名称が同じです。\n修正不要の場合は、「番組名を修正する」のチェックを外して下さい。");
					}

					// 番組名→ID
					Debug.Assert(!String.IsNullOrEmpty(oInputProgramRecord[(Int32)ProgramCsvColumns.Id]), "CheckInput() 番組 ID が空");
					oInputProgramAliasRecord[(Int32)ProgramAliasCsvColumns.NameOrId] = oInputProgramRecord[(Int32)ProgramCsvColumns.Id];
					oInputProgramAliasRecord[(Int32)ProgramAliasCsvColumns.ForceId] = "1";
				}

				// 楽曲情報
				if (RadioButtonHasProgram.Checked && String.IsNullOrEmpty(TextBoxProgramNameRelated.Text))
				{
					throw new Exception("関連番組名を入力して下さい。");
				}

				// 関連番組 ID を取得
				oInputSongRecord[(Int32)SongCsvColumns.ProgramId] = RadioButtonHasProgram.Checked ? oInputProgramRecord[(Int32)ProgramCsvColumns.Id] : null;

				// 楽曲が重複していないか確認
				List<List<String>> aSongRecords = NklCommon.FindCsvRecords(mSongCsvs, (Int32)SongCsvColumns.Name, oInputSongRecord[(Int32)SongCsvColumns.Name]);
				foreach (List<String> aSongRecord in aSongRecords)
				{
					// ファイル名命名規則で取得できる項目がすべて重複しているのを NG とする
					if (oInputSongRecord[(Int32)SongCsvColumns.Id] != aSongRecord[(Int32)SongCsvColumns.Id]
							&& IsEqual(oInputSongRecord[(Int32)SongCsvColumns.OpEd], aSongRecord[(Int32)SongCsvColumns.OpEd])
							&& IsEqual(oInputSongRecord[(Int32)SongCsvColumns.Artist], aSongRecord[(Int32)SongCsvColumns.Artist])
							&& IsEqual(oInputSongRecord[(Int32)SongCsvColumns.ProgramId], aSongRecord[(Int32)SongCsvColumns.ProgramId]))
					{
						throw new Exception("入力された楽曲情報が楽曲 ID \"" + aSongRecord[(Int32)SongCsvColumns.Id] + "\" の内容と重複しているため、登録できません。\n"
								+ "楽曲 ID の選択を変更するか、または入力内容を修正して下さい。");
					}
				}

				// 番組が重複していないか確認
				List<List<String>> aProgramRecords = NklCommon.FindCsvRecords(mProgramCsvs, (Int32)ProgramCsvColumns.Name, oInputProgramRecord[(Int32)ProgramCsvColumns.Name]);
				foreach (List<String> aProgramRecord in aProgramRecords)
				{
					// ファイル名命名規則で取得できる項目がすべて重複しているのを NG とする
					if (oInputProgramRecord[(Int32)ProgramCsvColumns.Id] != aProgramRecord[(Int32)ProgramCsvColumns.Id]
							&& IsEqual(oInputProgramRecord[(Int32)ProgramCsvColumns.Category], aProgramRecord[(Int32)ProgramCsvColumns.Category])
							&& IsEqual(oInputProgramRecord[(Int32)ProgramCsvColumns.GameCategory], aProgramRecord[(Int32)ProgramCsvColumns.GameCategory])
							&& IsEqual(oInputProgramRecord[(Int32)ProgramCsvColumns.AgeLimit], aProgramRecord[(Int32)ProgramCsvColumns.AgeLimit]))
					{
						throw new Exception("入力された番組情報が番組 ID \"" + aProgramRecord[(Int32)ProgramCsvColumns.Id] + "\" の内容と重複しているため、登録できません。"
								+ "番組 ID の選択を変更するか、または入力内容を修正して下さい。");
					}
				}

				// 放映開始日
				if (!String.IsNullOrEmpty(oInputProgramRecord[(Int32)ProgramCsvColumns.BeginDate])
						&& !Regex.IsMatch(oInputProgramRecord[(Int32)ProgramCsvColumns.BeginDate], @"^[0-9][0-9][0-9][0-9]\-[0-9][0-9]\-[0-9][0-9]$"))
				{
					throw new Exception("放映開始日は yyyy-mm-dd 形式で入力して下さい。\n（例）2000-01-02");
				}
			}
			catch (OperationCanceledException)
			{
				throw new OperationCanceledException();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
				throw new OperationCanceledException();
			}
		}

		// --------------------------------------------------------------------
		// 入力された情報を変数に格納
		// --------------------------------------------------------------------
		private void ComposToRecord(out List<String> oSongAliasRecord, out List<String> oProgramAliasRecord, out List<String> oSongRecord, out List<String> oProgramRecord)
		{
			// 楽曲別名（NameOrId は、未確定新規 ID の場合があるため名称で格納：新規 ID 解決後に CheckInput() で ID 化する）
			oSongAliasRecord = new List<String>(new String[(Int32)SongAliasCsvColumns.__End__]);
			oSongAliasRecord[(Int32)SongAliasCsvColumns.Alias] = mDicByFile[NklCommon.RULE_VAR_TITLE];
			oSongAliasRecord[(Int32)SongAliasCsvColumns.NameOrId] = CheckBoxUseSongAlias.Checked ? TextBoxTitleOrigin.Text : null;

			// 番組別名（NameOrId は、未確定新規 ID の場合があるため名称で格納：新規 ID 解決後に CheckInput() で ID 化する）
			oProgramAliasRecord = new List<String>(new String[(Int32)ProgramAliasCsvColumns.__End__]);
			oProgramAliasRecord[(Int32)ProgramAliasCsvColumns.Alias] = mDicByFile[NklCommon.RULE_VAR_PROGRAM];
			oProgramAliasRecord[(Int32)ProgramAliasCsvColumns.NameOrId] = CheckBoxUseProgramAlias.Checked ? TextBoxProgramNameOrigin.Text : null;
			Debug.WriteLine("ComposToRecord() oProgramAliasRecord[(Int32)ProgramAliasCsvColumns.NameOrId]: " + oProgramAliasRecord[(Int32)ProgramAliasCsvColumns.NameOrId]);

			// 楽曲情報
			oSongRecord = new List<String>(new String[(Int32)SongCsvColumns.__End__]);
			oSongRecord[(Int32)SongCsvColumns.Id] = ComboBoxSongId.SelectedIndex > 0 ? (String)ComboBoxSongId.Items[ComboBoxSongId.SelectedIndex] : null;
			oSongRecord[(Int32)SongCsvColumns.Name] = LabelTitleForSongInfo.Text;
			oSongRecord[(Int32)SongCsvColumns.Artist] = TextBoxArtist.Text;
			oSongRecord[(Int32)SongCsvColumns.OpEd] = TextBoxOpEd.Text;
			oSongRecord[(Int32)SongCsvColumns.CastSeq] = TextBoxCastSeq.Text;
			oSongRecord[(Int32)SongCsvColumns.ProgramName] = RadioButtonHasProgram.Checked ? TextBoxProgramNameRelated.Text : null;

			// 番組情報
			oProgramRecord = new List<String>(new String[(Int32)ProgramCsvColumns.__End__]);
			if (GroupBoxProgramInfo.Enabled)
			{
				oProgramRecord[(Int32)ProgramCsvColumns.Id] = ComboBoxProgramId.SelectedIndex > 0 ? (String)ComboBoxProgramId.Items[ComboBoxProgramId.SelectedIndex] : null;
				oProgramRecord[(Int32)ProgramCsvColumns.Ruby] = TextBoxProgramRuby.Text;
				oProgramRecord[(Int32)ProgramCsvColumns.Name] = LabelProgramNameForProgramInfo.Text;
				oProgramRecord[(Int32)ProgramCsvColumns.Category] = TextBoxCategory.Text;
				oProgramRecord[(Int32)ProgramCsvColumns.SubRuby] = TextBoxSubRuby.Text;
				oProgramRecord[(Int32)ProgramCsvColumns.SubName] = TextBoxSubName.Text;
				oProgramRecord[(Int32)ProgramCsvColumns.GameCategory] = TextBoxGameCategory.Text;
				oProgramRecord[(Int32)ProgramCsvColumns.NumStories] = TextBoxNumStories.Text;
				oProgramRecord[(Int32)ProgramCsvColumns.AgeLimit] = TextBoxAgeLimit.Text;
				oProgramRecord[(Int32)ProgramCsvColumns.BeginDate] = TextBoxBeginDate.Text;
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー：年齢制限メニューがクリックされた
		// --------------------------------------------------------------------
		private void ContextMenuAgeLimitItem_Click(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				ToolStripItem aItem = (ToolStripItem)oSender;
				TextBoxAgeLimit.Text = aItem.Text;
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "年齢制限メニュークリック時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー：番組分類メニューがクリックされた
		// --------------------------------------------------------------------
		private void ContextMenuCategoryItem_Click(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				ToolStripItem aItem = (ToolStripItem)oSender;
				TextBoxCategory.Text = aItem.Text;
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "番組分類メニュークリック時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー：ゲーム種別メニューがクリックされた
		// --------------------------------------------------------------------
		private void ContextMenuGameCategoryItem_Click(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				ToolStripItem aItem = (ToolStripItem)oSender;
				TextBoxGameCategory.Text = aItem.Text;
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "ゲーム種別メニュークリック時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー：摘要メニューがクリックされた
		// --------------------------------------------------------------------
		private void ContextMenuOpEdItem_Click(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				ToolStripItem aItem = (ToolStripItem)oSender;
				TextBoxOpEd.Text = aItem.Text;
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "摘要メニュークリック時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// 選択用コンテキストメニューを作成
		// --------------------------------------------------------------------
		private void CreateSelectMenuIfNeeded(Dictionary<String, List<List<String>>> oCsvs, Int32 oColumnIndex, ContextMenuStrip oContextMenu, EventHandler oEventHandler)
		{
			if (oContextMenu.Items.Count > 0)
			{
				return;
			}

			List<String> aSelects = new List<String>();

			foreach (KeyValuePair<String, List<List<String>>> aCsv in oCsvs)
			{
				for (Int32 i = 0; i < aCsv.Value.Count; i++)
				{
					List<String> aRecord = aCsv.Value[i];
					if (!String.IsNullOrEmpty(aRecord[oColumnIndex]) && !aSelects.Contains(aRecord[oColumnIndex]))
					{
						aSelects.Add(aRecord[oColumnIndex]);
					}
				}
			}
			aSelects.Sort();

			for (Int32 i = 0; i < aSelects.Count; i++)
			{
				oContextMenu.Items.Add(aSelects[i], null, oEventHandler);
			}
		}

		// --------------------------------------------------------------------
		// 未使用の ID を検索
		// --------------------------------------------------------------------
		private String FindNextId(Dictionary<String, List<List<String>>> oCsvs, Int32 oColumnIndex, String oSecondPrefix, Int32 oLastIdNumber)
		{
			Int32 aIdNumber = oLastIdNumber + 1;

			for (; ; )
			{
				String aId = mNicoKaraListerSettings.IdPrefix + oSecondPrefix + aIdNumber.ToString();
				if (NklCommon.FindCsvRecord(oCsvs, oColumnIndex, aId) == null)
				{
					return aId;
				}
				aIdNumber++;
			}
		}

		// --------------------------------------------------------------------
		// 与えられた番組名を CSV の中から探す
		// 見つからない場合は null ではなく空のリストを返す
		// --------------------------------------------------------------------
		private List<List<String>> FindPrograms(String oProgramName)
		{
			List<List<String>> aPrograms = new List<List<String>>();

			// 番組名は必須
			if (String.IsNullOrEmpty(oProgramName))
			{
				return aPrograms;
			}

			// 番組名で検索
			aPrograms = NklCommon.FindCsvRecords(mProgramCsvs, (Int32)ProgramCsvColumns.Name, oProgramName);

			// その他の条件がある場合は、それを満たさないものを除外
			for (Int32 i = aPrograms.Count - 1; i >= 0; i--)
			{
				if (!String.IsNullOrEmpty(mDicByFile[NklCommon.RULE_VAR_CATEGORY]) && aPrograms[i][(Int32)ProgramCsvColumns.Category] != mDicByFile[NklCommon.RULE_VAR_CATEGORY]
						|| !String.IsNullOrEmpty(mDicByFile[NklCommon.RULE_VAR_GAME_CATEGORY]) && aPrograms[i][(Int32)ProgramCsvColumns.GameCategory] != mDicByFile[NklCommon.RULE_VAR_GAME_CATEGORY]
						|| !String.IsNullOrEmpty(mDicByFile[NklCommon.RULE_VAR_AGE_LIMIT]) && aPrograms[i][(Int32)ProgramCsvColumns.AgeLimit] != mDicByFile[NklCommon.RULE_VAR_AGE_LIMIT])
				{
					aPrograms.RemoveAt(i);
				}
			}

			return aPrograms;
		}

		// --------------------------------------------------------------------
		// 楽曲と番組が紐付いているペアを 1 つ返す
		// 見つからない場合は null を返す
		// --------------------------------------------------------------------
		private void FindRelatedSongAndProgram(out String oSongId, out String oProgramId)
		{
			for (Int32 i = 0; i < mMatchedSongsWithAlias.Count; i++)
			{
				for (Int32 j = 0; j < mMatchedProgramsWithAlias.Count; j++)
				{
					if (IsRelated(mMatchedSongsWithAlias[i], mMatchedProgramsWithAlias[j]))
					{
						// 楽曲情報・情報情報が紐付いたものを採用する
						oSongId = mMatchedSongsWithAlias[i][(Int32)SongCsvColumns.Id];
						oProgramId = mMatchedProgramsWithAlias[j][(Int32)ProgramCsvColumns.Id];
						return;
					}
				}
			}

			oSongId = null;
			oProgramId = null;
		}

		// --------------------------------------------------------------------
		// 指定された楽曲名を CSV の中から探す
		// 見つからない場合は null ではなく空のリストを返す
		// --------------------------------------------------------------------
		private List<List<String>> FindSongs(String oTitle)
		{
			List<List<String>> aSongs = new List<List<String>>();

			// 楽曲名は必須
			if (String.IsNullOrEmpty(oTitle))
			{
				return aSongs;
			}

			// 番組名で検索
			aSongs = NklCommon.FindCsvRecords(mSongCsvs, (Int32)SongCsvColumns.Name, oTitle);

			// その他の条件がある場合は、それを満たさないものを除外
			for (Int32 i = aSongs.Count - 1; i >= 0; i--)
			{
				if (!String.IsNullOrEmpty(mDicByFile[NklCommon.RULE_VAR_OP_ED]) && aSongs[i][(Int32)SongCsvColumns.OpEd] != mDicByFile[NklCommon.RULE_VAR_OP_ED]
						|| !String.IsNullOrEmpty(mDicByFile[NklCommon.RULE_VAR_ARTIST]) && aSongs[i][(Int32)SongCsvColumns.Artist] != mDicByFile[NklCommon.RULE_VAR_ARTIST])
				{
					aSongs.RemoveAt(i);
				}
			}

			return aSongs;
		}

		// --------------------------------------------------------------------
		// 別名解決した楽曲名・番組名を CSV の中から探す
		// 複数の結果が保存される場合もある
		// --------------------------------------------------------------------
		private void FindSongsAndProgramsWithAlias()
		{
			mMatchedSongsWithAlias = FindSongs(CheckBoxUseSongAlias.Checked ? TextBoxTitleOrigin.Text : mDicByFile[NklCommon.RULE_VAR_TITLE]);
			mMatchedProgramsWithAlias = FindPrograms(CheckBoxUseProgramAlias.Checked ? TextBoxProgramNameOrigin.Text : mDicByFile[NklCommon.RULE_VAR_PROGRAM]);
		}

		// --------------------------------------------------------------------
		// 番組名 CSV 登録済みラベルに表示するテキスト
		// --------------------------------------------------------------------
		private String LabelProgramNameCsvText(Boolean oIsMatched)
		{
			if (oIsMatched)
			{
				return "（登録済み番組名）";
			}
			else
			{
				return "（未登録番組名）";
			}
		}

		// --------------------------------------------------------------------
		// 楽曲名 CSV 登録済みラベルに表示するテキスト
		// --------------------------------------------------------------------
		private String LabelTitleCsvText(Boolean oIsMatched)
		{
			if (oIsMatched)
			{
				return "（登録済み楽曲名）";
			}
			else
			{
				return "（未登録楽曲名）";
			}
		}

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// タイトルバー
#if DEBUG
			Text = "［デバッグ］" + Text;
#endif

			// その他
			mComboBoxSongIdLastSelectedIndex = -1;
			mComboBoxProgramIdLastSelectedIndex = -1;
		}

		// --------------------------------------------------------------------
		// 文字列が等しいか（String.Empty と null も等しいと見なす）
		// --------------------------------------------------------------------
		private Boolean IsEqual(String oStr1, String oStr2)
		{
			if (String.IsNullOrEmpty(oStr1) && String.IsNullOrEmpty(oStr2))
			{
				return true;
			}

			return oStr1 == oStr2;
		}

		// --------------------------------------------------------------------
		// 楽曲データ・番組データが紐付いているか
		// --------------------------------------------------------------------
		private Boolean IsRelated(List<String> oSongRecord, List<String> oProgramRecord)
		{
			return oSongRecord[(Int32)SongCsvColumns.ProgramId] == oProgramRecord[(Int32)ProgramCsvColumns.Id];
		}

		// --------------------------------------------------------------------
		// 新規 ID を発行
		// ＜例外＞ OperationCanceledException
		// --------------------------------------------------------------------
		private void IssueNewId(List<String> oInputSongRecord, List<String> oInputProgramRecord)
		{
			// プレフィックスの確認
			if ((String.IsNullOrEmpty(oInputSongRecord[(Int32)SongCsvColumns.Id]) || String.IsNullOrEmpty(oInputProgramRecord[(Int32)ProgramCsvColumns.Id]))
					&& String.IsNullOrEmpty(mNicoKaraListerSettings.IdPrefix))
			{
				using (FormInputIdPrefix aFormInputIdPrefix = new FormInputIdPrefix(mLogWriter))
				{
					if (aFormInputIdPrefix.ShowDialog() != DialogResult.OK)
					{
						throw new OperationCanceledException();
					}
					mNicoKaraListerSettings.IdPrefix = aFormInputIdPrefix.IdPrefix;
				}
			}

			// 楽曲 ID
			if (String.IsNullOrEmpty(oInputSongRecord[(Int32)SongCsvColumns.Id]))
			{
				oInputSongRecord[(Int32)SongCsvColumns.Id] = FindNextId(mSongCsvs, (Int32)SongCsvColumns.Id, SONG_ID_SECOND_PREFIX, mNicoKaraListerSettings.LastSongIdNumber);
			}

			// 番組 ID
			if (String.IsNullOrEmpty(oInputProgramRecord[(Int32)ProgramCsvColumns.Id]) && !String.IsNullOrEmpty(oInputProgramRecord[(Int32)ProgramCsvColumns.Name]))
			{
				oInputProgramRecord[(Int32)ProgramCsvColumns.Id] = FindNextId(mProgramCsvs, (Int32)ProgramCsvColumns.Id, PROGRAM_ID_SECOND_PREFIX, mNicoKaraListerSettings.LastProgramIdNumber);
			}
		}

		// --------------------------------------------------------------------
		// CSV 行から元の番組名を取得
		// --------------------------------------------------------------------
		private String ProgramOrigin(List<String> oProgramAliasCsvRecord)
		{
			if (String.IsNullOrEmpty(oProgramAliasCsvRecord[(Int32)ProgramAliasCsvColumns.ForceId]))
			{
				// ProgramAliasCsvColumns.NameOrId 列は元の番組名
				return oProgramAliasCsvRecord[(Int32)ProgramAliasCsvColumns.NameOrId];
			}

			// ProgramAliasCsvColumns.NameOrId 列は ID なので、ID を検索する
			List<String> aProgramCsvRecord = NklCommon.FindCsvRecord(mProgramCsvs, (Int32)ProgramCsvColumns.Id, oProgramAliasCsvRecord[(Int32)ProgramAliasCsvColumns.NameOrId]);
			if (aProgramCsvRecord != null)
			{
				return aProgramCsvRecord[(Int32)ProgramCsvColumns.Name];
			}

			// ID が見つからない場合は、ID そのものを返す
			return oProgramAliasCsvRecord[(Int32)ProgramAliasCsvColumns.NameOrId];
		}

		// --------------------------------------------------------------------
		// イベントハンドラー：関連番組の有無が変更された
		// --------------------------------------------------------------------
		private void RadioButtonProgram_CheckedChanged(Object oSender, EventArgs oEventArgs)
		{
			TextBoxProgramNameRelated.Enabled = RadioButtonHasProgram.Checked;
			ButtonSearchProgramNameRelated.Enabled = RadioButtonHasProgram.Checked;
			GroupBoxProgramInfo.Enabled = RadioButtonHasProgram.Checked;
			UpdateComboBoxProgramId();
		}

		// --------------------------------------------------------------------
		// 保存
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void Save(List<String> oInputSongAliasRecord, List<String> oInputProgramAliasRecord, List<String> oInputSongRecord, List<String> oInputProgramRecord)
		{
			String aNewSongCsvFileName = NklCommon.CategoryToSongCsvFileName(oInputProgramRecord[(Int32)ProgramCsvColumns.Category]);

			// 楽曲別名
			String aNewSongAliasCsvFileName = NklCommon.FileNameToAliasFileName(aNewSongCsvFileName);
			SaveInfo<SongAliasCsvColumns>(mSongAliasCsvs, oInputSongAliasRecord, (Int32)SongAliasCsvColumns.Alias, (Int32)SongAliasCsvColumns.NameOrId,
					NklCommon.UserCsvPath(aNewSongAliasCsvFileName));

			// 番組別名
			SaveInfo<ProgramAliasCsvColumns>(mProgramAliasCsvs, oInputProgramAliasRecord, (Int32)ProgramAliasCsvColumns.Alias, (Int32)ProgramAliasCsvColumns.NameOrId,
					NklCommon.UserCsvPath(NklCommon.FILE_NAME_PROGRAM_ALIAS_CSV));

			// 楽曲情報
			SaveInfo<SongCsvColumns>(mSongCsvs, oInputSongRecord, (Int32)SongCsvColumns.Id, -1, NklCommon.UserCsvPath(aNewSongCsvFileName));

			// 番組情報
			if (!String.IsNullOrEmpty(oInputProgramRecord[(Int32)ProgramCsvColumns.Id]))
			{
				SaveInfo<ProgramCsvColumns>(mProgramCsvs, oInputProgramRecord, (Int32)ProgramCsvColumns.Id, (Int32)ProgramCsvColumns.Id,
						NklCommon.UserCsvPath(NklCommon.FILE_NAME_PROGRAM_CSV));
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報の保存
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void SaveInfo<T>(Dictionary<String, List<List<String>>> oCsvs, List<String> oInputRecord, Int32 oKeyColumnIndex, Int32 oEmptyCheckColumnIndex, String oNewCsvPath)
		{
			List<String> aTitle = NklCommon.CsvTitle<T>();

			// 旧データがユーザー CSV 内にある場合はそのレコードを削除
			String aOldCsvPath;
			String aBackupedOldCsvPath = null;
			Boolean aOldCsvBackuped = false;
			List<String> aRecord;
			while ((aRecord = NklCommon.FindCsvRecord(oCsvs, oKeyColumnIndex, oInputRecord[oKeyColumnIndex], out aOldCsvPath)) != null
					&& !NklCommon.IsSystemCsvPath(aOldCsvPath))
			{
				oCsvs[aOldCsvPath].Remove(aRecord);

				// 旧ファイルの保存
				NklCommon.BackupCsv(aOldCsvPath);
				aBackupedOldCsvPath = aOldCsvPath;
				aOldCsvBackuped = true;
				CsvManager.SaveCsv(aOldCsvPath, oCsvs[aOldCsvPath], "\r\n", Encoding.UTF8, aTitle, true);

				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "CSV レコード削除：" + aOldCsvPath + " / " + oInputRecord[oKeyColumnIndex]);
			}

			// 新データを追加
			if (oEmptyCheckColumnIndex >= 0 && String.IsNullOrEmpty(oInputRecord[oEmptyCheckColumnIndex]))
			{
				// 空チェック列が空なので追加しない
			}
			else
			{
				// システム CSV に完全に同じ内容が登録されているなら、追加不要
				if (!NklCommon.RecordExists(oCsvs, oInputRecord, oKeyColumnIndex))
				{
					if (!oCsvs.ContainsKey(oNewCsvPath))
					{
						oCsvs[oNewCsvPath] = new List<List<String>>();
					}
					oCsvs[oNewCsvPath].Add(oInputRecord);

					// 新ファイルの保存
					if (aBackupedOldCsvPath != oNewCsvPath || !aOldCsvBackuped)
					{
						NklCommon.BackupCsv(oNewCsvPath);
					}
					CsvManager.SaveCsv(oNewCsvPath, oCsvs[oNewCsvPath], "\r\n", Encoding.UTF8, aTitle, true);

					mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "CSV レコード追加：" + oNewCsvPath + " / " + oInputRecord[oKeyColumnIndex]);
				}
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
		// CSV 行から元の楽曲名を取得
		// --------------------------------------------------------------------
		private String SongOrigin(List<String> oSongAliasCsvRecord)
		{
			if (String.IsNullOrEmpty(oSongAliasCsvRecord[(Int32)SongAliasCsvColumns.ForceId]))
			{
				// SongAliasCsvColumns.NameOrId 列は元の楽曲名
				return oSongAliasCsvRecord[(Int32)SongAliasCsvColumns.NameOrId];
			}

			// SongAliasCsvColumns.NameOrId 列は ID なので、ID を検索する
			List<String> aSongCsvRecord = NklCommon.FindCsvRecord(mSongCsvs, (Int32)SongCsvColumns.Id, oSongAliasCsvRecord[(Int32)SongAliasCsvColumns.NameOrId]);
			if (aSongCsvRecord != null)
			{
				return aSongCsvRecord[(Int32)SongCsvColumns.Name];
			}

			// ID が見つからない場合は、ID そのものを返す
			return oSongAliasCsvRecord[(Int32)SongAliasCsvColumns.NameOrId];
		}

		// --------------------------------------------------------------------
		// 別名関連コンポーネントの状態を更新
		// --------------------------------------------------------------------
		private void UpdateAliasComponents()
		{
			TextBoxTitleOrigin.Enabled = CheckBoxUseSongAlias.Checked;
			ButtonSearchTitleOrigin.Enabled = CheckBoxUseSongAlias.Checked;
			if (CheckBoxUseSongAlias.Checked)
			{
				if (String.IsNullOrEmpty(TextBoxTitleOrigin.Text))
				{
					TextBoxTitleOrigin.Text = mDicByFile[NklCommon.RULE_VAR_TITLE];
				}
				LabelTitleCsvWithAlias.Text = LabelTitleCsvText(mMatchedSongsWithAlias.Count > 0);
			}
			else
			{
				LabelTitleCsvWithAlias.Text = null;
			}

			if (String.IsNullOrEmpty(mDicByFile[NklCommon.RULE_VAR_PROGRAM]))
			{
				CheckBoxUseProgramAlias.Checked = false;
				CheckBoxUseProgramAlias.Enabled = false;
			}
			TextBoxProgramNameOrigin.Enabled = CheckBoxUseProgramAlias.Checked;
			ButtonSearchProgramNameOrigin.Enabled = CheckBoxUseProgramAlias.Checked;
			if (CheckBoxUseProgramAlias.Checked)
			{
				if (String.IsNullOrEmpty(TextBoxProgramNameOrigin.Text))
				{
					TextBoxProgramNameOrigin.Text = mDicByFile[NklCommon.RULE_VAR_PROGRAM];
				}
				LabelProgramNameCsvWithAlias.Text = LabelProgramNameCsvText(mMatchedProgramsWithAlias.Count > 0);
			}
			else
			{
				LabelProgramNameCsvWithAlias.Text = null;
			}
		}

		// --------------------------------------------------------------------
		// ファイル名・フォルダー固定値から取得した情報を表示
		// --------------------------------------------------------------------
		private void UpdateByFileComponents()
		{
			// 「ファイル名・フォルダー固定値から取得した情報」欄
			LabelTitleByFile.Text = mDicByFile[NklCommon.RULE_VAR_TITLE];
			LabelOpEdByFile.Text = mDicByFile[NklCommon.RULE_VAR_OP_ED];
			LabelArtistByFile.Text = mDicByFile[NklCommon.RULE_VAR_ARTIST];
			LabelProgramNameByFile.Text = mDicByFile[NklCommon.RULE_VAR_PROGRAM];
			LabelCategoryByFile.Text = mDicByFile[NklCommon.RULE_VAR_CATEGORY];
			LabelGameCategoryByFile.Text = mDicByFile[NklCommon.RULE_VAR_GAME_CATEGORY];
			LabelAgeLimitByFile.Text = mDicByFile[NklCommon.RULE_VAR_AGE_LIMIT];

			// 「表記揺れの修正」欄
			LabelTitleForAlias.Text = mDicByFile[NklCommon.RULE_VAR_TITLE];
			LabelTitleCsv.Text = LabelTitleCsvText(mMatchedSongs.Count > 0);
			LabelProgramNameForAlias.Text = mDicByFile[NklCommon.RULE_VAR_PROGRAM];
			LabelProgramNameCsv.Text = LabelProgramNameCsvText(mMatchedPrograms.Count > 0);
		}

		// --------------------------------------------------------------------
		// 番組 ID コンボボックスの選択肢を更新
		// --------------------------------------------------------------------
		private void UpdateComboBoxProgramId()
		{
			ComboBoxProgramId.Items.Clear();

			// 先頭は新規作成
			ComboBoxProgramId.Items.Add("（新規番組）");

			// 既存（mMatchedProgramsWithAlias）
			foreach (List<String> aRecord in mMatchedProgramsWithAlias)
			{
				// 関連番組で入力されたものと異なる場合は除外
				if (RadioButtonHasProgram.Checked && aRecord[(Int32)ProgramCsvColumns.Id] != TextBoxProgramNameRelated.Text)
				{
					continue;
				}

				if (ComboBoxProgramId.FindStringExact(aRecord[(Int32)ProgramCsvColumns.Id]) < 0)
				{
					ComboBoxProgramId.Items.Add(aRecord[(Int32)ProgramCsvColumns.Id]);
				}
			}

			// 既存（関連番組で入力されたもの）
			if (RadioButtonHasProgram.Checked)
			{
				List<List<String>> aPrograms = NklCommon.FindCsvRecords(mProgramCsvs, (Int32)ProgramCsvColumns.Name, TextBoxProgramNameRelated.Text);
				foreach (List<String> aRecord in aPrograms)
				{
					if (ComboBoxProgramId.FindStringExact(aRecord[(Int32)ProgramCsvColumns.Id]) < 0)
					{
						ComboBoxProgramId.Items.Add(aRecord[(Int32)ProgramCsvColumns.Id]);
					}
				}
			}

			// 選択
			Int32 aSelectedIndex = 0;
			if (ComboBoxProgramId.Items.Count > 1)
			{
				if (ComboBoxSongId.SelectedIndex > 0)
				{
					// 選択されている楽曲 ID に紐付く番組 ID を選択する
					List<String> aSongRecord = NklCommon.FindCsvRecord(mSongCsvs, (Int32)SongCsvColumns.Id, (String)ComboBoxSongId.Items[ComboBoxSongId.SelectedIndex]);
					if (aSongRecord != null)
					{
						aSelectedIndex = ComboBoxProgramId.FindStringExact(aSongRecord[(Int32)SongCsvColumns.ProgramId]);
					}
				}

				// 見つからない場合は、新規作成の次の項目を選択
				if (aSelectedIndex <= 0)
				{
					aSelectedIndex = 1;
				}
			}
			ComboBoxProgramId.SelectedIndex = aSelectedIndex;

			// ラベル
			if (ComboBoxProgramId.Items.Count > 2)
			{
				LabelProgramIdInfo.Text = "（複数の番組 ID があります）";
			}
			else
			{
				LabelProgramIdInfo.Text = null;
			}
		}

		// --------------------------------------------------------------------
		// 楽曲 ID コンボボックスの選択肢を更新
		// --------------------------------------------------------------------
		private void UpdateComboBoxSongId()
		{
			ComboBoxSongId.Items.Clear();

			// 先頭は新規作成
			ComboBoxSongId.Items.Add("（新規楽曲）");

			// 既存編集
			foreach (List<String> aRecord in mMatchedSongsWithAlias)
			{
				ComboBoxSongId.Items.Add(aRecord[(Int32)SongCsvColumns.Id]);
			}

			// 選択
			Int32 aSelectedIndex = 0;
			if (ComboBoxSongId.Items.Count > 1)
			{
				// 番組と関連している楽曲があればそれを選択
				String aSongId;
				String aProgramId;
				FindRelatedSongAndProgram(out aSongId, out aProgramId);
				if (!String.IsNullOrEmpty(aSongId))
				{
					aSelectedIndex = ComboBoxSongId.FindStringExact(aSongId);
				}

				// 見つからない場合は、新規作成の次の項目を選択
				if (aSelectedIndex <= 0)
				{
					aSelectedIndex = 1;
				}
			}
			ComboBoxSongId.SelectedIndex = aSelectedIndex;

			// ラベル
			if (ComboBoxSongId.Items.Count > 2)
			{
				LabelSongIdInfo.Text = "（複数の楽曲 ID があります）";
			}
			else
			{
				LabelSongIdInfo.Text = null;
			}
		}

		// --------------------------------------------------------------------
		// 楽曲 ID・番組 ID の番号を更新
		// --------------------------------------------------------------------
		private void UpdateLastIdNumbers(List<String> oInputSongRecord, List<String> oInputProgramRecord)
		{
			// 楽曲 ID
			String aSongIdPrefix = mNicoKaraListerSettings.IdPrefix + SONG_ID_SECOND_PREFIX;
			if (oInputSongRecord[(Int32)SongCsvColumns.Id].IndexOf(aSongIdPrefix) == 0)
			{
				Int32 aSongIdNumber;
				Int32.TryParse(oInputSongRecord[(Int32)SongCsvColumns.Id].Substring(aSongIdPrefix.Length), out aSongIdNumber);
				if (aSongIdNumber > mNicoKaraListerSettings.LastSongIdNumber)
				{
					mNicoKaraListerSettings.LastSongIdNumber = aSongIdNumber;
					mNicoKaraListerSettings.Save();
				}
			}

			// 番組 ID
			String aProgramIdPrefix = mNicoKaraListerSettings.IdPrefix + PROGRAM_ID_SECOND_PREFIX;
			if (!String.IsNullOrEmpty(oInputProgramRecord[(Int32)ProgramCsvColumns.Id]) && oInputProgramRecord[(Int32)ProgramCsvColumns.Id].IndexOf(aProgramIdPrefix) == 0)
			{
				Int32 aProgramIdNumber;
				Int32.TryParse(oInputProgramRecord[(Int32)ProgramCsvColumns.Id].Substring(aProgramIdPrefix.Length), out aProgramIdNumber);
				if (aProgramIdNumber > mNicoKaraListerSettings.LastProgramIdNumber)
				{
					mNicoKaraListerSettings.LastProgramIdNumber = aProgramIdNumber;
					mNicoKaraListerSettings.Save();
				}
			}
		}


		// ====================================================================
		// IDE 生成イベントハンドラー
		// ====================================================================

		private void FormEditInfo_Load(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			try
			{
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "情報編集フォームを開きます。");
				Init();
				NklCommon.LoadCsvs(mNicoKaraListerSettings, out mProgramCsvs, out mSongCsvs, out mProgramAliasCsvs, out mSongAliasCsvs);
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "情報編集フォームロード時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				Cursor = Cursors.Default;
			}
		}

		private void FormEditInfo_Shown(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			try
			{
				// ファイル名から指定された情報そのままで検索
				mMatchedSongs = FindSongs(mDicByFile[NklCommon.RULE_VAR_TITLE]);
				mMatchedPrograms = FindPrograms(mDicByFile[NklCommon.RULE_VAR_PROGRAM]);

				// 別名解決
				ApplySongAlias();
				ApplyProgramAlias();

				// 別名解決後の検索
				FindSongsAndProgramsWithAlias();

				// 反映
				UpdateByFileComponents();
				UpdateAliasComponents();
				UpdateComboBoxSongId();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "情報編集フォーム表示時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				Cursor = Cursors.Default;
			}
		}

		private void CheckBoxUseSongAlias_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				FindSongsAndProgramsWithAlias();
				UpdateAliasComponents();
				UpdateComboBoxSongId();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "楽曲名修正有無変更時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TextBoxTitleOrigin_TextChanged(object sender, EventArgs e)
		{
			try
			{
				FindSongsAndProgramsWithAlias();
				LabelTitleCsvWithAlias.Text = LabelTitleCsvText(mMatchedSongsWithAlias.Count > 0);
				UpdateComboBoxSongId();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "楽曲名修正時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void CheckBoxUseProgramAlias_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				FindSongsAndProgramsWithAlias();
				UpdateAliasComponents();
				UpdateComboBoxProgramId();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "番組名修正有無変更時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TextBoxProgramNameOrigin_TextChanged(object sender, EventArgs e)
		{
			try
			{
				FindSongsAndProgramsWithAlias();
				LabelProgramNameCsvWithAlias.Text = LabelProgramNameCsvText(mMatchedProgramsWithAlias.Count > 0);
				UpdateComboBoxProgramId();

				// 正式名称が登録済みで、かつ、関連番組が未登録の場合は、関連番組を正式名称に変更する
				if (mMatchedProgramsWithAlias.Count > 0 && LabelProgramNameCsvRelated.Text == LabelProgramNameCsvText(false))
				{
					TextBoxProgramNameRelated.Text = TextBoxProgramNameOrigin.Text;
				}
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "番組名修正時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSearchTitle_Click(object sender, EventArgs e)
		{
			try
			{
				using (FormSearchInfo aFormSearchOrigin = new FormSearchInfo("楽曲名の正式名称", mSongCsvs, (Int32)SongCsvColumns.Name, TextBoxTitleOrigin.Text, mLogWriter))
				{
					if (aFormSearchOrigin.ShowDialog() == DialogResult.OK)
					{
						TextBoxTitleOrigin.Text = aFormSearchOrigin.SelectedInfo;
					}
				}
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "楽曲名正式名称検索時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ComboBoxSongID_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				LabelTitleForSongInfo.Text = CheckBoxUseSongAlias.Checked ? TextBoxTitleOrigin.Text : mDicByFile[NklCommon.RULE_VAR_TITLE];

				if (ComboBoxSongId.SelectedIndex == 0)
				{
					// 入力値に従って関連番組を設定
					String aProgramName = CheckBoxUseProgramAlias.Checked ? TextBoxProgramNameOrigin.Text : mDicByFile[NklCommon.RULE_VAR_PROGRAM];
					if (String.IsNullOrEmpty(aProgramName))
					{
						RadioButtonNoProgram.Checked = true;
					}
					else
					{
						RadioButtonHasProgram.Checked = true;
						TextBoxProgramNameRelated.Text = aProgramName;
					}

					if (mComboBoxSongIdLastSelectedIndex == 0)
					{
						// 引き続き新規楽曲が選択されている場合はクリアしない
						return;
					}

					// 新規楽曲としてクリア
					TextBoxOpEd.Text = null;
					TextBoxCastSeq.Text = null;
					TextBoxArtist.Text = null;
				}
				else
				{
					// 既存楽曲
					//List<String> aRecord = mMatchedSongsWithAlias[ComboBoxSongId.SelectedIndex - 1];
					List<String> aRecord = NklCommon.FindCsvRecord(mSongCsvs, (Int32)SongCsvColumns.Id, (String)ComboBoxSongId.Items[ComboBoxSongId.SelectedIndex]);
					TextBoxOpEd.Text = aRecord[(Int32)SongCsvColumns.OpEd];
					TextBoxCastSeq.Text = aRecord[(Int32)SongCsvColumns.CastSeq];
					TextBoxArtist.Text = aRecord[(Int32)SongCsvColumns.Artist];
					if (String.IsNullOrEmpty(aRecord[(Int32)SongCsvColumns.ProgramId]))
					{
						RadioButtonNoProgram.Checked = true;
					}
					else
					{
						RadioButtonHasProgram.Checked = true;
						List<String> aProgramRecord = NklCommon.FindCsvRecord(mProgramCsvs, (Int32)ProgramCsvColumns.Id, aRecord[(Int32)SongCsvColumns.ProgramId]);
						if (aProgramRecord == null)
						{
							TextBoxProgramNameRelated.Text = null;
						}
						else
						{
							TextBoxProgramNameRelated.Text = aProgramRecord[(Int32)ProgramCsvColumns.Name];
						}
					}
				}

				// 紐付く番組情報を更新
				UpdateComboBoxProgramId();

				mComboBoxSongIdLastSelectedIndex = ComboBoxSongId.SelectedIndex;
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "楽曲 ID 選択時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSearchProgramNameOrigin_Click(object sender, EventArgs e)
		{
			try
			{
				using (FormSearchInfo aFormSearchOrigin = new FormSearchInfo("番組名の正式名称", mProgramCsvs, (Int32)ProgramCsvColumns.Name, TextBoxProgramNameOrigin.Text, mLogWriter))
				{
					if (aFormSearchOrigin.ShowDialog() == DialogResult.OK)
					{
						TextBoxProgramNameOrigin.Text = aFormSearchOrigin.SelectedInfo;
					}
				}
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "番組名正式名称検索時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSearchProgramName_Click(object sender, EventArgs e)
		{
			try
			{
				using (FormSearchInfo aFormSearchOrigin = new FormSearchInfo("関連する番組名", mProgramCsvs, (Int32)ProgramCsvColumns.Name, TextBoxProgramNameRelated.Text, mLogWriter))
				{
					if (aFormSearchOrigin.ShowDialog() == DialogResult.OK)
					{
						TextBoxProgramNameRelated.Text = aFormSearchOrigin.SelectedInfo;
					}
				}
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "関連番組検索時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TextBoxProgramName_TextChanged(object sender, EventArgs e)
		{
			try
			{
				List<String> aRecord = NklCommon.FindCsvRecord(mProgramCsvs, (Int32)ProgramCsvColumns.Name, TextBoxProgramNameRelated.Text);
				LabelProgramNameCsvRelated.Text = LabelProgramNameCsvText(aRecord != null);
				UpdateComboBoxProgramId();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "関連番組入力時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ComboBoxProgramId_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (ComboBoxProgramId.SelectedIndex == 0)
				{
					// 入力値に従って番組名を設定
					LabelProgramNameForProgramInfo.Text = TextBoxProgramNameRelated.Text;

					if (mComboBoxProgramIdLastSelectedIndex == 0)
					{
						// 引き続き新規番組が選択されている場合はクリアしない
						return;
					}

					// 新規番組としてクリア
					TextBoxProgramRuby.Text = null;
					TextBoxCategory.Text = null;
					TextBoxSubRuby.Text = null;
					TextBoxSubName.Text = null;
					TextBoxGameCategory.Text = null;
					TextBoxNumStories.Text = null;
					TextBoxAgeLimit.Text = null;
					TextBoxBeginDate.Text = null;
				}
				else
				{
					// 既存番組
					List<String> aRecord = NklCommon.FindCsvRecord(mProgramCsvs, (Int32)ProgramCsvColumns.Id, (String)ComboBoxProgramId.Items[ComboBoxProgramId.SelectedIndex]);
					TextBoxProgramRuby.Text = aRecord[(Int32)ProgramCsvColumns.Ruby];
					LabelProgramNameForProgramInfo.Text = aRecord[(Int32)ProgramCsvColumns.Name];
					TextBoxCategory.Text = aRecord[(Int32)ProgramCsvColumns.Category];
					TextBoxSubRuby.Text = aRecord[(Int32)ProgramCsvColumns.SubRuby];
					TextBoxSubName.Text = aRecord[(Int32)ProgramCsvColumns.SubName];
					TextBoxGameCategory.Text = aRecord[(Int32)ProgramCsvColumns.GameCategory];
					TextBoxNumStories.Text = aRecord[(Int32)ProgramCsvColumns.NumStories];
					TextBoxAgeLimit.Text = aRecord[(Int32)ProgramCsvColumns.AgeLimit];
					TextBoxBeginDate.Text = aRecord[(Int32)ProgramCsvColumns.BeginDate];
				}

				mComboBoxProgramIdLastSelectedIndex = ComboBoxProgramId.SelectedIndex;
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "番組 ID 選択時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			try
			{
				List<String> aSongAliasRecord;
				List<String> aProgramAliasRecord;
				List<String> aSongRecord;
				List<String> aProgramRecord;
				ComposToRecord(out aSongAliasRecord, out aProgramAliasRecord, out aSongRecord, out aProgramRecord);
				IssueNewId(aSongRecord, aProgramRecord);
				CheckInput(aSongAliasRecord, aProgramAliasRecord, aSongRecord, aProgramRecord);
				Save(aSongAliasRecord, aProgramAliasRecord, aSongRecord, aProgramRecord);
				UpdateLastIdNumbers(aSongRecord, aProgramRecord);
				DialogResult = DialogResult.OK;
			}
			catch (OperationCanceledException)
			{
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲情報・番組情報決定を中止しました。");
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "楽曲情報・番組情報決定時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormEditInfo_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "情報編集フォームを閉じました。");
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "情報編集フォームクローズ時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSelectOpEd_Click(object sender, EventArgs e)
		{
			try
			{
				CreateSelectMenuIfNeeded(mSongCsvs, (Int32)SongCsvColumns.OpEd, ContextMenuOpEd, ContextMenuOpEdItem_Click);
				ContextMenuOpEd.Show(Cursor.Position);
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "摘要選択ボタン押下時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSearchArtist_Click(object sender, EventArgs e)
		{
			try
			{
				using (FormSearchInfo aFormSearchOrigin = new FormSearchInfo("歌手名", mSongCsvs, (Int32)SongCsvColumns.Artist, TextBoxArtist.Text, mLogWriter))
				{
					if (aFormSearchOrigin.ShowDialog() == DialogResult.OK)
					{
						TextBoxArtist.Text = aFormSearchOrigin.SelectedInfo;
					}
				}
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "歌手名検索時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSelectCategory_Click(object sender, EventArgs e)
		{
			try
			{
				CreateSelectMenuIfNeeded(mProgramCsvs, (Int32)ProgramCsvColumns.Category, ContextMenuCategory, ContextMenuCategoryItem_Click);
				ContextMenuCategory.Show(Cursor.Position);
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "番組分類選択ボタン押下時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSelectGameCategory_Click(object sender, EventArgs e)
		{
			try
			{
				CreateSelectMenuIfNeeded(mProgramCsvs, (Int32)ProgramCsvColumns.GameCategory, ContextMenuGameCategory, ContextMenuGameCategoryItem_Click);
				ContextMenuGameCategory.Show(Cursor.Position);
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "ゲーム種別選択ボタン押下時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSelectAgeLimit_Click(object sender, EventArgs e)
		{
			try
			{
				CreateSelectMenuIfNeeded(mProgramCsvs, (Int32)ProgramCsvColumns.AgeLimit, ContextMenuAgeLimit, ContextMenuAgeLimitItem_Click);
				ContextMenuAgeLimit.Show(Cursor.Position);
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "年齢制限選択ボタン押下時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
	// public partial class FormEditInfo ___END___

}
// namespace NicoKaraLister ___END___