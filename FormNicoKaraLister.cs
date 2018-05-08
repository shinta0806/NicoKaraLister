// ============================================================================
// 
// メインフォーム
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 歌手名等をファイル名から取得している場合、新規入力時にそれを入れておく
// CSV レコードの更新日時をどこかに持つ（インポート上書き用）
// 番組名を修正して、かつ、楽曲に紐付く番組が新規の場合は、修正後の番組を紐付ける
// 出力設定でアイテムがゼロの場合をチェックしているか？
// MakeRegexPattern() で末尾と同じ対策を先頭にやらなくて大丈夫か？
// ----------------------------------------------------------------------------

using NicoKaraLister.Shared;
using Shinta;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace NicoKaraLister
{
	public partial class FormNicoKaraLister : Form
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FormNicoKaraLister()
		{
			InitializeComponent();
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// CSV 登録進捗表示間隔
		private const Int32 NUM_CSV_INSERT_PROGRESS = 1000;

		// フォルダー履歴保持数
		private const Int32 NUM_FOLDER_HISTORIES = 10;

		// ログ記録用
		private const String TRACE_SOURCE_NAME = "NicoKaraL";

		// 改訂履歴ファイル
		private const String FILE_NAME_HISTORY = "NicoKaraLister_History_JPN.txt";

		// 頭文字変換用
		private const String HEAD_CONVERT_FROM = "ぁぃぅぇぉゕゖゃゅょゎゔがぎぐげござじずぜぞだぢづでどばびぶべぼぱぴぷぺぽゐゑ";
		private const String HEAD_CONVERT_TO = "あいうえおかけやゆよわうかきくけこさしすせそたちつてとはひふへほはひふへほいえ";

		// スマートトラック判定用のオフボーカル単語（小文字表記、両端を | で括る）
		private const String OFF_VOCAL_WORDS = "|cho|cut|dam|inst|inst+cho|joy|off|offvocal|vc|オフボ|オフボーカル|ボイキャン|ボーカルキャンセル|配信|";

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// 環境設定
		private NicoKaraListerSettings mNicoKaraListerSettings = new NicoKaraListerSettings();

		// 検出ファイルリストデータベース
		private SQLiteConnection mFoundDbConnection;

		// 出力形式
		private List<OutputWriter> mOutputWriters = new List<OutputWriter>();

		// 番組分類統合用
		private Dictionary<String, String> mCategoryUnityMap;

		// 終了時タスク安全中断用
		private CancellationTokenSource mClosingCancellationTokenSource = new CancellationTokenSource();

		// ログ
		private LogWriter mLogWriter;

		// メインフォーム上で時間のかかるタスクが多重起動されるのを抑止する
		private Object mTaskLock = new Object();

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// フォルダー一覧にフォルダーを追加
		// --------------------------------------------------------------------
		private void AddFolderToDataGridViewTargetFolders(String oFolder)
		{
			DataGridViewTargetFolders.Rows.Add();
			Int32 aIndex = DataGridViewTargetFolders.Rows.Count - 1;

			// 対象
			DataGridViewTargetFolders.Rows[aIndex].Cells[(Int32)FolderColumns.IsValid].Value = true;

			// フォルダーパス
			DataGridViewTargetFolders.Rows[aIndex].Cells[(Int32)FolderColumns.Folder].Value = oFolder;

			// 設定有無
			DataGridViewTargetFolders.Rows[aIndex].Cells[(Int32)FolderColumns.SettingsExist].Value
					= FolderSettingsStatusString(NklCommon.FolderSettingsStatus(oFolder));

			// 設定
			DataGridViewTargetFolders.Rows[aIndex].Cells[(Int32)FolderColumns.Settings].Value = "設定";
		}

		// --------------------------------------------------------------------
		// 履歴追加：[0] が最新履歴
		// --------------------------------------------------------------------
		private void AddFolderHistory(List<String> oHistory, String oText)
		{
			// 登録済みであれば一旦削除
			oHistory.Remove(oText);

			// 追加
			oHistory.Insert(0, oText);

			// 溢れた分を削除
			if (oHistory.Count > NUM_FOLDER_HISTORIES)
			{
				oHistory.RemoveRange(NUM_FOLDER_HISTORIES, oHistory.Count - NUM_FOLDER_HISTORIES);
			}
		}

		// --------------------------------------------------------------------
		// トラック情報からオンボーカル・オフボーカルがあるか解析する
		// --------------------------------------------------------------------
		private void AnalyzeSmartTrack(String oTrack, out Boolean oHasOn, out Boolean oHasOff)
		{
			oHasOn = false;
			oHasOff = false;

			if (String.IsNullOrEmpty(oTrack))
			{
				return;
			}

			String[] aTracks = oTrack.Split(new Char[] { '-', '_', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
			for (Int32 i = 0; i < aTracks.Length; i++)
			{
				Int32 aPos = OFF_VOCAL_WORDS.IndexOf("|" + aTracks[i] + "|", StringComparison.OrdinalIgnoreCase);
				if (aPos >= 0)
				{
					oHasOff = true;
				}
				else
				{
					oHasOn = true;
				}
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー：フォルダー一覧の設定ボタンがクリックされた
		// --------------------------------------------------------------------
		private async void ButtonFolderSettingsClicked(Int32 oRowIndex)
		{
			OutputWriter aSelectedOutputWriter = SelectedOutputWriter();

			using (FormFolderSettings aFormFolderSettings
					= new FormFolderSettings((String)DataGridViewTargetFolders.Rows[oRowIndex].Cells[(Int32)FolderColumns.Folder].Value, mNicoKaraListerSettings,
					aSelectedOutputWriter.OutputSettings, mLogWriter))
			{
				aFormFolderSettings.ShowDialog(this);
			}

			// フォルダー設定の有無の表示を更新
			// キャンセルでも実行（設定削除→キャンセルの場合は更新が必要）
			await NklCommon.LaunchTaskAsync(UpdateDataGridViewTargetFoldersSettingsExist, mTaskLock, oRowIndex);
		}

		// --------------------------------------------------------------------
		// ユーザーに入力された値が適切か確認
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void CheckInput()
		{
			Invoke(new Action(() =>
			{
				if (String.IsNullOrEmpty(TextBoxParentFolder.Text))
				{
					throw new Exception("リスト化対象フォルダーを指定して下さい。");
				}
				if (!Directory.Exists(TextBoxParentFolder.Text))
				{
					throw new Exception("指定されたリスト化対象フォルダーは存在しません。");
				}
				if (String.IsNullOrEmpty(TextBoxOutputFolder.Text))
				{
					throw new Exception("出力先フォルダーを指定して下さい。");
				}
				if (!Directory.Exists(TextBoxOutputFolder.Text))
				{
					if (MessageBox.Show("指定された出力先フォルダーは存在しません。\n作成しますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
							== DialogResult.No)
					{
						throw new OperationCanceledException();
					}
					Directory.CreateDirectory(TextBoxOutputFolder.Text);
				}

				OutputWriter aSelectedOutputWriter = SelectedOutputWriter();
				if (File.Exists(TextBoxOutputFolder.Text + "\\" + aSelectedOutputWriter.TopFileName))
				{
					if (MessageBox.Show("出力先のファイルが既に存在します。\n上書きされますがよろしいですか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
							== DialogResult.No)
					{
						throw new OperationCanceledException();
					}
				}
			}));
		}

		// --------------------------------------------------------------------
		// CSV レコード同士の比較（指定された列で比較）
		// --------------------------------------------------------------------
		private Int32 CompareCsv(List<String> oLhs, List<String> oRhs, Int32 oColumnIndex)
		{
			if (oLhs.Count == 0 && oRhs.Count == 0)
			{
				return 0;
			}
			if (oLhs.Count == 0)
			{
				return -1;
			}
			if (oRhs.Count == 0)
			{
				return 1;
			}
			return String.Compare(oLhs[oColumnIndex], oRhs[oColumnIndex]);
		}

		// --------------------------------------------------------------------
		// program.csv レコード同士の比較
		// --------------------------------------------------------------------
		private Int32 CompareProgramCsv(List<String> oLhs, List<String> oRhs)
		{
			return CompareCsv(oLhs, oRhs, (Int32)ProgramCsvColumns.Id);
		}

		// --------------------------------------------------------------------
		// anison.csv 等のレコード同士の比較
		// --------------------------------------------------------------------
		private Int32 CompareSongCsv(List<String> oLhs, List<String> oRhs)
		{
			return CompareCsv(oLhs, oRhs, (Int32)SongCsvColumns.Id);
		}

		// --------------------------------------------------------------------
		// イベントハンドラー：出力先フォルダー履歴メニューがクリックされた
		// --------------------------------------------------------------------
		private void ContextMenuOutputFolderHistoryItem_Click(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				ToolStripItem aItem = (ToolStripItem)oSender;
				TextBoxOutputFolder.Text = aItem.Text;
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "出力先フォルダー履歴メニュークリック時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー：リスト化対象フォルダー履歴メニューがクリックされた
		// --------------------------------------------------------------------
		private void ContextMenuParentFolderHistoryItem_Click(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				ToolStripItem aItem = (ToolStripItem)oSender;
				TextBoxParentFolder.Text = aItem.Text;
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "リスト化対象フォルダー履歴メニュークリック時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// 検出ファイルデータベース内にある、指定されたテーブルの件数を数える
		// --------------------------------------------------------------------
		private Int32 CountFoundDbRecord<T>() where T : class
		{
			return CountInfoDbRecord<T>(mFoundDbConnection);
		}

		// --------------------------------------------------------------------
		// 指定されたデータベース内にある、指定されたテーブルの件数を数える
		// --------------------------------------------------------------------
		private Int32 CountInfoDbRecord<T>(SQLiteConnection oConnection) where T : class
		{
			try
			{
				using (DataContext aContext = new DataContext(oConnection))
				{
					Table<T> aTable = aContext.GetTable<T>();
					return aTable.Count();
				}
			}
			catch (Exception)
			{
				// DB が存在してテーブルが存在しない場合は
				// SQL logic error or missing database no such table: <TableName>
				// のような例外が発生する
				return 0;
			}
		}

		// --------------------------------------------------------------------
		// 情報キャッシュデータベース内にある、指定されたテーブルの件数を数える
		// --------------------------------------------------------------------
		private Int32 CountInfoDbRecord<T>() where T : class
		{
			using (SQLiteConnection aConnection = CreateInfoDbConnection())
			{
				return CountInfoDbRecord<T>(aConnection);
			}
		}

		// --------------------------------------------------------------------
		// 検出ファイルデータベースを作成
		// --------------------------------------------------------------------
		private void CreateFoundDb()
		{
			mFoundDbConnection = CreateFoundDbConnection();
			NklCommon.CreateFoundDbTables(mFoundDbConnection);
		}

		// --------------------------------------------------------------------
		// 検出ファイルデータベースに接続
		// --------------------------------------------------------------------
		private SQLiteConnection CreateFoundDbConnection()
		{
			return NklCommon.CreateDbConnection(":memory:");
		}

		// --------------------------------------------------------------------
		// 情報キャッシュデータベースを作成
		// --------------------------------------------------------------------
		private void CreateInfoDb()
		{
			ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "情報キャッシュデータベースを更新します...");

			using (SQLiteConnection aConnection = CreateInfoDbConnection())
			{
				using (SQLiteCommand aCmd = new SQLiteCommand(aConnection))
				{
					// データ挿入
					using (DataContext aContext = new DataContext(aConnection))
					{
						// 楽曲→番組への関連があるため、最初に番組を登録する
						CreateInfoDbProgramTable(aCmd, aContext);
						CreateInfoDbSongTable(aCmd, aContext);
						CreateInfoDbProgramAliasTable(aCmd, aContext);
						CreateInfoDbSongAliasTable(aCmd, aContext);
						NklCommon.CreateDbPropertyTable(aCmd, aContext);
					}
				}
			}

			mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();
		}

		// --------------------------------------------------------------------
		// 情報データベースに接続
		// --------------------------------------------------------------------
		private SQLiteConnection CreateInfoDbConnection()
		{
			return NklCommon.CreateDbConnection(InfoDbPath());
		}

		// --------------------------------------------------------------------
		// DB の中に番組別名テーブルを作成
		// --------------------------------------------------------------------
		private void CreateInfoDbProgramAliasTable(SQLiteCommand oCmd, DataContext oContext)
		{
			// テーブル作成
			LinqUtils.CreateTable(oCmd, typeof(TProgramAlias));

			// インデックス作成（JOIN および検索の高速化）
			List<String> aIndices = new List<String>();
			aIndices.Add(TProgram.FIELD_NAME_PROGRAM_ID);
			aIndices.Add(TProgramAlias.FIELD_NAME_PROGRAM_ALIAS_ALIAS);
			LinqUtils.CreateIndex(oCmd, LinqUtils.TableName(typeof(TProgramAlias)), aIndices);

			// データ挿入（ユーザー CSV 優先）
			CreateInfoDbProgramAliasTableInsert(NklCommon.UserCsvPath(NklCommon.FILE_NAME_PROGRAM_ALIAS_CSV), oContext);
			CreateInfoDbProgramAliasTableInsert(NklCommon.SystemCsvPath(NklCommon.FILE_NAME_PROGRAM_ALIAS_CSV), oContext);
		}

		// --------------------------------------------------------------------
		// 番組別名テーブルのレコードを CSV から挿入
		// --------------------------------------------------------------------
		private void CreateInfoDbProgramAliasTableInsert(String oCsvPath, DataContext oContext)
		{
			if (!File.Exists(oCsvPath))
			{
				return;
			}
			ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, oCsvPath + " から番組別名データベースに登録中...");

			List<List<String>> aCsvContents = NklCommon.LoadCsv(oCsvPath, mNicoKaraListerSettings, (Int32)ProgramAliasCsvColumns.__End__);
			mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();

			Table<TProgram> aTable = oContext.GetTable<TProgram>();
			Table<TProgramAlias> aTableAlias = oContext.GetTable<TProgramAlias>();
			Int32 aNumRecords = CountInfoDbRecord<TProgramAlias>();

			for (Int32 i = 0; i < aCsvContents.Count; i++)
			{
				List<String> aRecord = aCsvContents[i];
				if (String.IsNullOrEmpty(aRecord[(Int32)ProgramAliasCsvColumns.NameOrId]) || String.IsNullOrEmpty(aRecord[(Int32)ProgramAliasCsvColumns.Alias]))
				{
					ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aRecord[(Int32)ProgramAliasCsvColumns.LineIndex]) + 2).ToString("#,0") + " 行目の番組名・別名が指定されていないため無視します。", true);
					continue;
				}

				// 番組名が指定されたものとして番組 ID を検索
				String aProgramId = null;
				IQueryable<TProgram> aQueryResult;
				if (String.IsNullOrEmpty(aRecord[(Int32)ProgramAliasCsvColumns.ForceId]))
				{
					aQueryResult =
							from x in aTable
							where x.Name == aRecord[(Int32)ProgramAliasCsvColumns.NameOrId]
							select x;
					foreach (TProgram aResultOne in aQueryResult)
					{
						aProgramId = aResultOne.Id;
						break;
					}
				}

				// 番組 ID が指定されたものとして番組 ID を検索
				if (aProgramId == null)
				{
					aQueryResult =
							from x in aTable
							where x.Id == aRecord[(Int32)ProgramAliasCsvColumns.NameOrId]
							select x;
					foreach (TProgram aResultOne in aQueryResult)
					{
						aProgramId = aResultOne.Id;
						break;
					}
				}

				if (aProgramId == null)
				{
					ShowLogMessage(TraceEventType.Warning,
							"指定された番組名・番組 ID が番組データベースにありません：" + aRecord[(Int32)ProgramAliasCsvColumns.NameOrId], true);
					continue;
				}

				// 挿入
				aTableAlias.InsertOnSubmit(new TProgramAlias
				{
					Uid = aNumRecords,
					ProgramId = aProgramId,
					Alias = aRecord[(Int32)ProgramAliasCsvColumns.Alias],
				});

				aNumRecords++;

				if (aNumRecords % NUM_CSV_INSERT_PROGRESS == 0)
				{
					ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "進捗：" + aNumRecords.ToString("#,0") + " 番組...");
					mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}
			}
			mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();

			// 確定
			oContext.SubmitChanges();
			mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();
		}

		// --------------------------------------------------------------------
		// DB の中に番組マスターテーブルを作成
		// --------------------------------------------------------------------
		private void CreateInfoDbProgramTable(SQLiteCommand oCmd, DataContext oContext)
		{
			// テーブル作成
			List<String> aUniques = new List<String>();
			aUniques.Add(TProgram.FIELD_NAME_PROGRAM_ID);
			LinqUtils.CreateTable(oCmd, typeof(TProgram), aUniques);

			// インデックス作成（JOIN および検索の高速化）
			List<String> aIndices = new List<String>();
			aIndices.Add(TProgram.FIELD_NAME_PROGRAM_ID);
			aIndices.Add(TProgram.FIELD_NAME_PROGRAM_NAME);
			aIndices.Add(TProgram.FIELD_NAME_PROGRAM_BEGIN_DATE);
			LinqUtils.CreateIndex(oCmd, LinqUtils.TableName(typeof(TProgram)), aIndices);

			// データ挿入（ユーザー CSV 優先）
			CreateInfoDbProgramTableInsert(NklCommon.UserCsvPath(NklCommon.FILE_NAME_PROGRAM_CSV), oContext);
			CreateInfoDbProgramTableInsert(NklCommon.SystemCsvPath(NklCommon.FILE_NAME_PROGRAM_CSV), oContext);
		}

		// --------------------------------------------------------------------
		// 番組マスターテーブルのレコードを CSV から挿入
		// --------------------------------------------------------------------
		private void CreateInfoDbProgramTableInsert(String oCsvPath, DataContext oContext)
		{
			if (!File.Exists(oCsvPath))
			{
				return;
			}
			ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, oCsvPath + " から番組マスターデータベースに登録中...");

			List<List<String>> aCsvContents = NklCommon.LoadCsv(oCsvPath, mNicoKaraListerSettings, (Int32)ProgramCsvColumns.__End__);
			aCsvContents.Sort(CompareProgramCsv);
			mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();

			Table<TProgram> aTable = oContext.GetTable<TProgram>();
			Int32 aNumRecords = CountInfoDbRecord<TProgram>();
			List<String> aPrevRecord = null;

			for (Int32 i = 0; i < aCsvContents.Count; i++)
			{
				List<String> aRecord = aCsvContents[i];

				// 番組 ID の解析
				if (String.IsNullOrEmpty(aRecord[(Int32)ProgramCsvColumns.Id]))
				{
					ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aRecord[(Int32)ProgramCsvColumns.LineIndex]) + 2).ToString("#,0") + " 行目の番組 ID が指定されていないため無視します。", true);
					continue;
				}
				if (aPrevRecord != null && aRecord[(Int32)ProgramCsvColumns.Id] == aPrevRecord[(Int32)ProgramCsvColumns.Id])
				{
					ShowLogMessage(TraceEventType.Warning,
							"番組 ID（" + aRecord[(Int32)ProgramCsvColumns.Id] + "）が重複しているため無視します（登録済："
							+ aPrevRecord[(Int32)ProgramCsvColumns.Name] + "、無視：" + aRecord[(Int32)ProgramCsvColumns.Name] + "）。", true);
					continue;
				}
				Boolean aDup = false;
				IQueryable<TProgram> aQueryResult =
						from x in aTable
						where x.Id == aRecord[(Int32)ProgramCsvColumns.Id]
						select x;
				foreach (TProgram aResultOne in aQueryResult)
				{
					ShowLogMessage(TraceEventType.Warning,
							"番組 ID（" + aRecord[(Int32)ProgramCsvColumns.Id] + "）は別 CSV で登録済のため無視します（登録済："
							+ aResultOne.Name + "、無視：" + aRecord[(Int32)ProgramCsvColumns.Name] + "）。", true);
					aDup = true;
					break;
				}
				if (aDup)
				{
					continue;
				}

				// 放映開始日の解析
				DateTime aBeginDate;
				aBeginDate = CsvDateStringToDateTime(aRecord[(Int32)ProgramCsvColumns.BeginDate]);

				// 挿入
				aTable.InsertOnSubmit(new TProgram
				{
					Uid = aNumRecords,
					Id = aRecord[(Int32)ProgramCsvColumns.Id],
					Category = aRecord[(Int32)ProgramCsvColumns.Category],
					GameCategory = aRecord[(Int32)ProgramCsvColumns.GameCategory],
					Name = aRecord[(Int32)ProgramCsvColumns.Name],
					Ruby = aRecord[(Int32)ProgramCsvColumns.Ruby],
					SubName = aRecord[(Int32)ProgramCsvColumns.SubName],
					SubRuby = aRecord[(Int32)ProgramCsvColumns.SubRuby],
					NumStories = aRecord[(Int32)ProgramCsvColumns.NumStories],
					AgeLimit = aRecord[(Int32)ProgramCsvColumns.AgeLimit],
					BeginDate = JulianDay.DateTimeToJulianDay(aBeginDate),
				});

				aNumRecords++;
				aPrevRecord = aRecord;

				if (aNumRecords % NUM_CSV_INSERT_PROGRESS == 0)
				{
					ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "進捗：" + aNumRecords.ToString("#,0") + " 番組...");
					mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}
			}
			mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();

			// 確定
			oContext.SubmitChanges();
			mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();
		}

		// --------------------------------------------------------------------
		// DB の中に楽曲別名テーブルを作成
		// --------------------------------------------------------------------
		private void CreateInfoDbSongAliasTable(SQLiteCommand oCmd, DataContext oContext)
		{
			// テーブル作成
			LinqUtils.CreateTable(oCmd, typeof(TSongAlias));

			// インデックス作成（JOIN および検索の高速化）
			List<String> aIndices = new List<String>();
			aIndices.Add(TSong.FIELD_NAME_SONG_ID);
			aIndices.Add(TSongAlias.FIELD_NAME_SONG_ALIAS_ALIAS);
			LinqUtils.CreateIndex(oCmd, LinqUtils.TableName(typeof(TSongAlias)), aIndices);

			// データ挿入（ユーザー CSV 優先）
			List<String> aSongAliasCsvs = NklCommon.CreateSongAliasCsvList();
			foreach (String aSongAliasCsv in aSongAliasCsvs)
			{
				CreateInfoDbSongAliasTableInsert(NklCommon.UserCsvPath(aSongAliasCsv), oContext);
			}
			foreach (String aSongAliasCsv in aSongAliasCsvs)
			{
				CreateInfoDbSongAliasTableInsert(NklCommon.SystemCsvPath(aSongAliasCsv), oContext);
			}
		}

		// --------------------------------------------------------------------
		// 楽曲別名テーブルのレコードを CSV から挿入
		// --------------------------------------------------------------------
		private void CreateInfoDbSongAliasTableInsert(String oCsvPath, DataContext oContext)
		{
			if (!File.Exists(oCsvPath))
			{
				return;
			}
			ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, oCsvPath + " から楽曲別名データベースに登録中...");

			List<List<String>> aCsvContents = NklCommon.LoadCsv(oCsvPath, mNicoKaraListerSettings, (Int32)SongAliasCsvColumns.__End__);
			mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();

			Table<TSong> aTable = oContext.GetTable<TSong>();
			Table<TSongAlias> aTableAlias = oContext.GetTable<TSongAlias>();
			Int32 aNumRecords = CountInfoDbRecord<TSongAlias>();

			for (Int32 i = 0; i < aCsvContents.Count; i++)
			{
				List<String> aRecord = aCsvContents[i];

				if (String.IsNullOrEmpty(aRecord[(Int32)SongAliasCsvColumns.NameOrId]) || String.IsNullOrEmpty(aRecord[(Int32)SongAliasCsvColumns.Alias]))
				{
					ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aRecord[(Int32)SongAliasCsvColumns.LineIndex]) + 2).ToString("#,0") + " 行目の楽曲名・別名が指定されていないため無視します。", true);
					continue;
				}

				// 楽曲名が指定されたものとして楽曲 ID を検索
				String aSongId = null;
				IQueryable<TSong> aQueryResult;
				if (String.IsNullOrEmpty(aRecord[(Int32)SongAliasCsvColumns.ForceId]))
				{
					aQueryResult =
							from x in aTable
							where x.Name == aRecord[(Int32)SongAliasCsvColumns.NameOrId]
							select x;
					foreach (TSong aResultOne in aQueryResult)
					{
						aSongId = aResultOne.Id;
						break;
					}
				}

				// 楽曲 ID が指定されたものとして楽曲 ID を検索
				if (aSongId == null)
				{
					aQueryResult =
							from x in aTable
							where x.Id == aRecord[(Int32)SongAliasCsvColumns.NameOrId]
							select x;
					foreach (TSong aResultOne in aQueryResult)
					{
						aSongId = aResultOne.Id;
						break;
					}
				}

				if (aSongId == null)
				{
					ShowLogMessage(TraceEventType.Warning,
							"指定された楽曲名・楽曲 ID が番組データベースにありません：" + aRecord[(Int32)SongAliasCsvColumns.NameOrId], true);
					continue;
				}

				// 挿入
				aTableAlias.InsertOnSubmit(new TSongAlias
				{
					Uid = aNumRecords,
					SongId = aSongId,
					Alias = aRecord[(Int32)SongAliasCsvColumns.Alias],
				});

				aNumRecords++;

				if (aNumRecords % NUM_CSV_INSERT_PROGRESS == 0)
				{
					ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "進捗：" + aNumRecords.ToString("#,0") + " 楽曲...");
					mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}
			}
			mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();

			// 確定
			oContext.SubmitChanges();
			mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();
		}

		// --------------------------------------------------------------------
		// DB の中に楽曲マスターテーブルを作成
		// --------------------------------------------------------------------
		private void CreateInfoDbSongTable(SQLiteCommand oCmd, DataContext oContext)
		{
			// テーブル作成
			List<String> aUniques = new List<String>();
			aUniques.Add(TSong.FIELD_NAME_SONG_ID);
			LinqUtils.CreateTable(oCmd, typeof(TSong), aUniques);

			// インデックス作成（JOIN および検索の高速化）
			List<String> aIndices = new List<String>();
			aIndices.Add(TSong.FIELD_NAME_SONG_ID);
			aIndices.Add(TSong.FIELD_NAME_SONG_OP_ED);
			aIndices.Add(TSong.FIELD_NAME_SONG_NAME);
			aIndices.Add(TSong.FIELD_NAME_SONG_ARTIST);
			LinqUtils.CreateIndex(oCmd, LinqUtils.TableName(typeof(TSong)), aIndices);

			// データ挿入（ユーザー CSV 優先）
			List<String> aSongCsvs = NklCommon.CreateSongCsvList();
			foreach (String aSongCsv in aSongCsvs)
			{
				CreateInfoDbSongTableInsert(NklCommon.UserCsvPath(aSongCsv), oContext);
			}
			foreach (String aSongCsv in aSongCsvs)
			{
				CreateInfoDbSongTableInsert(NklCommon.SystemCsvPath(aSongCsv), oContext);
			}
		}

		// --------------------------------------------------------------------
		// 楽曲マスターテーブルのレコードを CSV から挿入
		// --------------------------------------------------------------------
		private void CreateInfoDbSongTableInsert(String oCsvPath, DataContext oContext)
		{
			if (!File.Exists(oCsvPath))
			{
				return;
			}
			ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, oCsvPath + " から楽曲マスターデータベースに登録中...");

			List<List<String>> aCsvContents = NklCommon.LoadCsv(oCsvPath, mNicoKaraListerSettings, (Int32)SongCsvColumns.__End__);
			aCsvContents.Sort(CompareSongCsv);
			mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();

			Table<TSong> aTable = oContext.GetTable<TSong>();
			Int32 aNumRecords = CountInfoDbRecord<TSong>();
			List<String> aPrevRecord = null;

			for (Int32 i = 0; i < aCsvContents.Count; i++)
			{
				List<String> aRecord = aCsvContents[i];

				// 楽曲 ID の解析
				if (String.IsNullOrEmpty(aRecord[(Int32)SongCsvColumns.Id]))
				{
					ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aRecord[(Int32)SongCsvColumns.LineIndex]) + 2).ToString("#,0") + " 行目の楽曲 ID が指定されていないため無視します。", true);
					continue;
				}
				if (aPrevRecord != null && aRecord[(Int32)SongCsvColumns.Id] == aPrevRecord[(Int32)SongCsvColumns.Id])
				{
					ShowLogMessage(TraceEventType.Warning,
							"楽曲 ID（" + aRecord[(Int32)SongCsvColumns.Id] + "）が重複しているため無視します（登録済："
							+ aPrevRecord[(Int32)SongCsvColumns.Name] + "、無視：" + aRecord[(Int32)SongCsvColumns.Name] + "）。", true);
					continue;
				}
				Boolean aDup = false;
				IQueryable<TSong> aQueryResult =
						from x in aTable
						where x.Id == aRecord[(Int32)SongCsvColumns.Id]
						select x;
				foreach (TSong aResultOne in aQueryResult)
				{
					ShowLogMessage(TraceEventType.Warning,
							"楽曲 ID（" + aRecord[(Int32)SongCsvColumns.Id] + "）は別 CSV で登録済のため無視します（登録済："
							+ aResultOne.Name + "、無視：" + aRecord[(Int32)SongCsvColumns.Name] + "）。", true);
					aDup = true;
					break;
				}
				if (aDup)
				{
					continue;
				}

				// 挿入
				aTable.InsertOnSubmit(new TSong
				{
					Uid = aNumRecords,
					ProgramId = aRecord[(Int32)SongCsvColumns.ProgramId],
					Category = aRecord[(Int32)SongCsvColumns.Category],
					ProgramName = aRecord[(Int32)SongCsvColumns.ProgramName],
					OpEd = aRecord[(Int32)SongCsvColumns.OpEd],
					CastSeq = aRecord[(Int32)SongCsvColumns.CastSeq],
					Id = aRecord[(Int32)SongCsvColumns.Id],
					Name = aRecord[(Int32)SongCsvColumns.Name],
					Artist = aRecord[(Int32)SongCsvColumns.Artist],
				});

				aNumRecords++;
				aPrevRecord = aRecord;

				if (aNumRecords % NUM_CSV_INSERT_PROGRESS == 0)
				{
					ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "進捗：" + aNumRecords.ToString("#,0") + " 楽曲...");
					mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}
			}
			mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();

			// 確定
			oContext.SubmitChanges();
			mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();
		}

		// --------------------------------------------------------------------
		// "yyyy-mm-dd" を DateTime に変換
		// 月日が 00 になっている場合があるため、DateTime.ParseExact() は使えない
		// --------------------------------------------------------------------
		private DateTime CsvDateStringToDateTime(String oCsvDateString)
		{
			if (String.IsNullOrEmpty(oCsvDateString) || oCsvDateString.Length < 10)
			{
				return new DateTime(NklCommon.INVALID_YEAR, 1, 1);
			}

			Int32 aYear;
			Int32 aMonth;
			Int32 aDay;

			try
			{
				aYear = Int32.Parse(oCsvDateString.Substring(0, 4));
				if (aYear < NklCommon.INVALID_YEAR)
				{
					aYear = NklCommon.INVALID_YEAR;
				}
				aMonth = Int32.Parse(oCsvDateString.Substring(5, 2));
				if (aMonth <= 0 || aMonth > 12)
				{
					aMonth = 1;
				}
				aDay = Int32.Parse(oCsvDateString.Substring(8, 2));
				if (aDay <= 0 || aDay > 31)
				{
					aDay = 1;
				}
			}
			catch (Exception)
			{
				return new DateTime(NklCommon.INVALID_YEAR, 1, 1);
			}

			return new DateTime(aYear, aMonth, aDay);
		}

		// --------------------------------------------------------------------
		// ディスク上のデータベースのフルパス
		// --------------------------------------------------------------------
		private String DbPath(String oFileName)
		{
			return Path.GetDirectoryName(Application.ExecutablePath) + "\\" + NklCommon.FOLDER_NAME_DATABASE + oFileName;
		}

		// --------------------------------------------------------------------
		// 指定されたデータベースファイルのプロパティーを取得
		// --------------------------------------------------------------------
		private TProperty DbProperty(String oDbPath)
		{
			try
			{
				using (SQLiteConnection aDbConnection = NklCommon.CreateDbConnection(oDbPath))
				{
					using (DataContext aDbContext = new DataContext(aDbConnection))
					{
						Table<TProperty> aTableProperty = aDbContext.GetTable<TProperty>();
						IQueryable<TProperty> aQueryResult =
								from x in aTableProperty
								select x;
						foreach (TProperty aProperty in aQueryResult)
						{
							return aProperty;
						}
					}
				}
			}
			catch (Exception)
			{
			}

			return null;
		}

		// --------------------------------------------------------------------
		// 情報キャッシュデータベースが CSV より古かったら削除する
		// --------------------------------------------------------------------
		private void DeleteOldInfoDb()
		{
			// 元 CSV（パス無し）
			List<String> aCsvs = new List<String>();
			aCsvs.Add(NklCommon.FILE_NAME_PROGRAM_CSV);
			aCsvs.AddRange(NklCommon.CreateSongCsvList());
			aCsvs.Add(NklCommon.FILE_NAME_PROGRAM_ALIAS_CSV);
			aCsvs.AddRange(NklCommon.CreateSongAliasCsvList());

			Boolean aIsDelete = IsDbOld(InfoDbPath(), aCsvs);

			// 削除処理
			if (aIsDelete)
			{
				NklCommon.DeleteOldDb(InfoDbPath());
			}

			mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();
		}

		// --------------------------------------------------------------------
		// UI 無効化（時間のかかる処理実行時用）
		// --------------------------------------------------------------------
		private void DisableComponents()
		{
			Invoke(new Action(() =>
			{
				TextBoxParentFolder.Enabled = false;
				ButtonParentFolderHistory.Enabled = false;
				ButtonBrowseParentFolder.Enabled = false;
				DataGridViewTargetFolders.Enabled = false;
				TextBoxOutputFolder.Enabled = false;
				ButtonOutputFolderHistory.Enabled = false;
				ButtonBrowseOutputFolder.Enabled = false;
				ComboBoxOutputFormat.Enabled = false;
				ButtonOutputSettings.Enabled = false;
				ButtonSettings.Enabled = false;
				ButtonGo.Enabled = false;
				TextBoxLog.Text = null;
			}));
		}

		// --------------------------------------------------------------------
		// UI 有効化
		// --------------------------------------------------------------------
		private void EnableComponents()
		{
			if (!mClosingCancellationTokenSource.Token.IsCancellationRequested)
			{
				Invoke(new Action(() =>
				{
					TextBoxParentFolder.Enabled = true;
					UpdateParentFolderHistoryComponents();
					ButtonBrowseParentFolder.Enabled = true;
					DataGridViewTargetFolders.Enabled = true;
					TextBoxOutputFolder.Enabled = true;
					UpdateOutputFolderHistoryComponents();
					ButtonBrowseOutputFolder.Enabled = true;
					ComboBoxOutputFormat.Enabled = true;
					UpdateButtonOutputSettings();
					ButtonSettings.Enabled = true;
					ButtonGo.Enabled = true;
				}));
			}

		}

		// --------------------------------------------------------------------
		// 指定フォルダ内のファイルを検索して検出ファイルデータベースに追加
		// ファイルは再帰検索しない
		// --------------------------------------------------------------------
		private void Find(String oFolderPath)
		{
			OutputWriter aSelectedOutputWriter = null;
			Invoke(new Action(() =>
			{
				aSelectedOutputWriter = SelectedOutputWriter();
			}));

			// フォルダー設定を読み込む
			FolderSettingsInDisk aFolderSettingsInDisk = NklCommon.LoadFolderSettings(oFolderPath);
			FolderSettingsInMemory aFolderSettingsInMemory = NklCommon.CreateFolderSettingsInMemory(aFolderSettingsInDisk);

			using (SQLiteConnection aInfoDbConnection = CreateInfoDbConnection())
			{
				using (SQLiteCommand aInfoDbCmd = new SQLiteCommand(aInfoDbConnection))
				{
					using (DataContext aFoundDbContext = new DataContext(mFoundDbConnection))
					{
						Table<TFound> aTableFound = aFoundDbContext.GetTable<TFound>();
						Int32 aUid = CountFoundDbRecord<TFound>();

						// 検索
						String[] aAllPathes;
						try
						{
							aAllPathes = Directory.GetFiles(oFolderPath);
						}
						catch (Exception)
						{
							return;
						}

						// 挿入
						foreach (String aPath in aAllPathes)
						{
							if (!aSelectedOutputWriter.OutputSettings.TargetExts.Contains(Path.GetExtension(aPath).ToLower()))
							{
								continue;
							}

							TFound aRecord = new TFound();
							aRecord.Uid = aUid;
							aRecord.Path = aPath;
							FileInfo aFileInfo = new FileInfo(aPath);
							aRecord.LastWriteTime = JulianDay.DateTimeToJulianDay(aFileInfo.LastWriteTime);
							aRecord.FileSize = aFileInfo.Length;
							SetTFoundValue(aRecord, aFolderSettingsInMemory, aInfoDbCmd);
							aTableFound.InsertOnSubmit(aRecord);

							aUid++;
						}

						mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();

						// コミット
						aFoundDbContext.SubmitChanges();

						mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();
					}

				}

			}
		}

		// --------------------------------------------------------------------
		// リスト化対象フォルダーのサブフォルダーを列挙
		// SearchOption.AllDirectories 付きで Directory.GetDirectories を呼びだすと、
		// ごみ箱のようにアクセス権限の無いフォルダーの中も列挙しようとして例外が
		// 発生し中断してしまう。
		// 面倒だが 1 フォルダーずつ列挙する
		// --------------------------------------------------------------------
		private List<String> FindSubFolders()
		{
			List<String> aFolders = new List<String>();
			String aParentFolder = null;

			Invoke(new Action(() =>
			{
				// 親フォルダー
				aParentFolder = TextBoxParentFolder.Text;
			}));
			FindSubFoldersSub(aFolders, aParentFolder);

			return aFolders;
		}

		// --------------------------------------------------------------------
		// FindSubFolders() の子関数
		// --------------------------------------------------------------------
		private void FindSubFoldersSub(List<String> oFolders, String oFolder)
		{
			mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();

			// 指定フォルダー
			oFolders.Add(oFolder);

			// 指定フォルダーのサブフォルダー
			try
			{
				String[] aSubFolders = Directory.GetDirectories(oFolder, "*", SearchOption.TopDirectoryOnly);
				foreach (String aSubFolder in aSubFolders)
				{
					FindSubFoldersSub(oFolders, aSubFolder);
				}
			}
			catch (Exception)
			{
			}
		}

		// --------------------------------------------------------------------
		// リスト生成の後片付け
		// --------------------------------------------------------------------
		private void FinishGenerateList()
		{
			// データベース
			if (mFoundDbConnection != null)
			{
				mFoundDbConnection.Dispose();
				mFoundDbConnection = null;
			}

			// ログ設定
			mLogWriter.TextBoxDisplay = null;

			// UI
			EnableComponents();
		}

		// --------------------------------------------------------------------
		// フォルダーの設定有無を表す文字列
		// --------------------------------------------------------------------
		private String FolderSettingsStatusString(FolderSettingsStatus oStatus)
		{
			switch (oStatus)
			{
				case FolderSettingsStatus.None:
					return "無";
				case FolderSettingsStatus.Set:
					return "有";
				case FolderSettingsStatus.Inherit:
					return "親に有";
				default:
					Debug.Assert(false, "FolderSettingsStatusString() bad FolderSettingsStatus");
					return null;
			}
		}

		// --------------------------------------------------------------------
		// リスト生成のトップレベル関数
		// --------------------------------------------------------------------
		private void GenerateList(Object oDummy)
		{
			try
			{
				// 準備
				PrepareGenerateList();

				// 検索
				List<String> aFolders = TargetFolders();
				if (aFolders.Count == 0)
				{
					throw new Exception("リスト化対象のフォルダーが 1 つも選択されていません。");
				}
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "リスト化対象のフォルダー数：" + aFolders.Count.ToString("#,0"));
				foreach (String aFolder in aFolders)
				{
					ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "検索中... " + aFolder);
					Find(aFolder);
				}

				// 出力
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "リスト生成中... ");
				String aOutputFilePath;
				Output(out aOutputFilePath);
				ShowLogMessage(TraceEventType.Information, "リスト生成が完了しました。");

				// 表示
				try
				{
					Process.Start(aOutputFilePath);
				}
				catch (Exception)
				{
					ShowLogMessage(TraceEventType.Error, "出力先ファイルを開けませんでした。\n" + aOutputFilePath);
				}
			}
			catch (OperationCanceledException)
			{
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "リスト生成を中止しました。");
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "リスト生成時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				FinishGenerateList();
			}
		}

		// --------------------------------------------------------------------
		// 頭文字を返す
		// 半角大文字英数、ひらがな（濁点なし）、その他、のいずれか
		// --------------------------------------------------------------------
		private String Head(String oString)
		{
			if (String.IsNullOrEmpty(oString))
			{
				return NklCommon.HEAD_MISC;
			}

			Char aChar = oString[0];

			// カタカナをひらがなに変換
			if ('ァ' <= aChar && aChar <= 'ヶ')
			{
				aChar = (Char)(aChar - 0x0060);
			}

			// 濁点・小文字をノーマルに変換
			Int32 aHeadConvertPos = HEAD_CONVERT_FROM.IndexOf(aChar);
			if (aHeadConvertPos >= 0)
			{
				aChar = HEAD_CONVERT_TO[aHeadConvertPos];
			}

			// ひらがなを返す
			if ('あ' <= aChar && aChar <= 'ん')
			{
				return new string(aChar, 1);
			}

			// 全角英数を半角英数に変換
			if ('０' <= aChar && aChar <= 'ｚ')
			{
				aChar = (Char)(aChar - 0xFEE0);
			}

			// 数字を返す
			if ('0' <= aChar && aChar <= '9')
			{
				return new string(aChar, 1);
			}

			// 英字を返す
			if ('A' <= aChar && aChar <= 'Z')
			{
				return new string(aChar, 1);
			}
			if ('a' <= aChar && aChar <= 'z')
			{
				return new string(aChar, 1).ToUpper();
			}

			return NklCommon.HEAD_MISC;
		}

		// --------------------------------------------------------------------
		// 情報キャッシュデータベースのフルパス
		// --------------------------------------------------------------------
		private String InfoDbPath()
		{
			return DbPath(NklCommon.FILE_NAME_INFO_DB);
		}

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// ログ初期化
			mLogWriter = new LogWriter(TRACE_SOURCE_NAME);
			mLogWriter.ApplicationQuitToken = mClosingCancellationTokenSource.Token;
			NklCommon.LogWriter = mLogWriter;

			// タイトルバー
			Text = NklCommon.APP_NAME_J;
#if DEBUG
			Text = "［デバッグ］" + Text;
#endif

			// データグリッドビュー
			InitDataGridView();

			// 設定の読み込み
			mNicoKaraListerSettings.Reload();
			if (mNicoKaraListerSettings.ParentFolderHistory == null)
			{
				mNicoKaraListerSettings.ParentFolderHistory = new List<String>();
			}
			if (mNicoKaraListerSettings.OutputFolderHistory == null)
			{
				mNicoKaraListerSettings.OutputFolderHistory = new List<String>();
			}

			// リスト出力形式
			mOutputWriters.Add(new HtmlOutputWriter());
			mOutputWriters.Add(new YukariOutputWriter());
			mOutputWriters.Add(new CsvOutputWriter());
			LoadOutputSettings();
			foreach (OutputWriter aOutputWriter in mOutputWriters)
			{
				// 現時点で設定可能なプロパティー（変わらないもの）は設定しておく
				aOutputWriter.NicoKaraListerSettings = mNicoKaraListerSettings;
				aOutputWriter.LogWriter = mLogWriter;

				// 追加
				ComboBoxOutputFormat.Items.Add(aOutputWriter.FormatName);
			}
			ComboBoxOutputFormat.SelectedIndex = 0;

			// 履歴
			UpdateParentFolderHistoryComponents();
			UpdateOutputFolderHistoryComponents();

			// 番組分類統合マップ初期化
			mCategoryUnityMap = NklCommon.CreateCategoryUnityMap();

			// 設計時サイズ以下にできないようにする
			MinimumSize = Size;

			// 終了時の状態を復元
			TextBoxParentFolder.Text = mNicoKaraListerSettings.ParentFolder;
			TextBoxOutputFolder.Text = mNicoKaraListerSettings.OutputFolder;
			if (0 <= mNicoKaraListerSettings.OutputFormat && mNicoKaraListerSettings.OutputFormat < ComboBoxOutputFormat.Items.Count)
			{
				ComboBoxOutputFormat.SelectedIndex = mNicoKaraListerSettings.OutputFormat;
			}

			// enum.OutputItems チェック
			Debug.Assert(NklCommon.OUTPUT_ITEM_NAMES.Length == (Int32)OutputItems.__End__ + 1, "Init() NklCommon.OUTPUT_ITEM_NAMES の数が不正");
		}

		// --------------------------------------------------------------------
		// データグリッドビュー初期化
		// --------------------------------------------------------------------
		private void InitDataGridView()
		{
			// 対象
			ColumnIsValid.Width = 40;

			// フォルダー
			ColumnFolder.Width = 550;

			// 設定有無
			ColumnSettingsExist.Width = 80;

			// 設定
			ColumnSettings.Width = 50;
		}

		// --------------------------------------------------------------------
		// データベースが CSV より古いかどうか（データベースを削除すべきか）
		// ＜引数＞ oDbPath: フルパス, oCsvs: ファイル名のみ
		// --------------------------------------------------------------------
		private Boolean IsDbOld(String oDbPath, List<String> oCsvs)
		{
			FileInfo aDbInfo = new FileInfo(oDbPath);
			if (!aDbInfo.Exists)
			{
				return false;
			}

			Boolean aIsDelete = false;

			// プロパティー
			TProperty aProperty = DbProperty(oDbPath);
			if (aProperty == null)
			{
				aIsDelete = true;
			}
			if (!aIsDelete && Common.CompareVersionString(aProperty.AppVer, "Ver 1.21 α") < 0)
			{
				aIsDelete = true;
			}

			// 元 CSV よりも情報キャッシュデータベースが新しいか確認
			if (!aIsDelete)
			{
				// 各 CSV に対し、CSVs および UserCSVs の 2 つのパスが必要
				List<FileInfo> aCsvInfos = new List<FileInfo>();
				foreach (String aCsv in oCsvs)
				{
					aCsvInfos.Add(new FileInfo(NklCommon.SystemCsvPath(aCsv)));
					aCsvInfos.Add(new FileInfo(NklCommon.UserCsvPath(aCsv)));
				}

				// 更新日時が古かったら削除
				foreach (FileInfo aCsvInfo in aCsvInfos)
				{
					if (aCsvInfo.Exists && aDbInfo.LastWriteTime < aCsvInfo.LastWriteTime)
					{
						aIsDelete = true;
						break;
					}
				}
			}

			return aIsDelete;
		}

		// --------------------------------------------------------------------
		// すべての出力者の OutputSettings を読み込む
		// --------------------------------------------------------------------
		private void LoadOutputSettings()
		{
			foreach (OutputWriter aOutputWriter in mOutputWriters)
			{
				aOutputWriter.OutputSettings.Load();
			}
		}

		// --------------------------------------------------------------------
		// 新バージョンで初回起動された時の処理を行う
		// --------------------------------------------------------------------
		private void NewVersionLaunched()
		{
			String aNewVerMsg;

			// α・β警告、ならびに、更新時のメッセージ（2017/01/09）
			// 新規・更新のご挨拶
			if (String.IsNullOrEmpty(mNicoKaraListerSettings.PrevLaunchVer))
			{
				// 新規
				aNewVerMsg = "【初回起動】\n\n";
				aNewVerMsg += NklCommon.APP_NAME_J + "をダウンロードしていただき、ありがとうございます。";
			}
			else
			{
				aNewVerMsg = "【更新起動】\n\n";
				aNewVerMsg += NklCommon.APP_NAME_J + "を更新していただき、ありがとうございます。\n";
				aNewVerMsg += "更新内容については［ヘルプ→改訂履歴］メニューをご参照ください。";
			}

			// α・βの注意
			if (NklCommon.APP_VER.IndexOf("α") >= 0)
			{
				aNewVerMsg += "\n\nこのバージョンは開発途上のアルファバージョンです。\n"
						+ "使用前にヘルプをよく読み、注意してお使い下さい。";
			}
			else if (NklCommon.APP_VER.IndexOf("β") >= 0)
			{
				aNewVerMsg += "\n\nこのバージョンは開発途上のベータバージョンです。\n"
						+ "使用前にヘルプをよく読み、注意してお使い下さい。";
			}

			// 表示
			ShowLogMessage(TraceEventType.Information, aNewVerMsg);

			// Zone ID 削除
			Common.DeleteZoneID(Path.GetDirectoryName(Application.ExecutablePath), SearchOption.AllDirectories);
		}


		// --------------------------------------------------------------------
		// リスト出力
		// --------------------------------------------------------------------
		private void Output(out String oOutputFilePath)
		{
			String aOutputFolderPath = null;
			OutputWriter aSelectedOutputWriter = null;
			Invoke(new Action(() =>
			{
				aOutputFolderPath = TextBoxOutputFolder.Text + "\\";
				aSelectedOutputWriter = SelectedOutputWriter();
			}));

			using (DataContext aFoundDbContext = new DataContext(mFoundDbConnection))
			{
				Table<TFound> aTableFound = aFoundDbContext.GetTable<TFound>();
				aSelectedOutputWriter.FolderPath = aOutputFolderPath;
				aSelectedOutputWriter.TableFound = aTableFound;

				aSelectedOutputWriter.Output();

				oOutputFilePath = aOutputFolderPath + aSelectedOutputWriter.TopFileName;
			}
		}

		// --------------------------------------------------------------------
		// リスト生成の準備
		// --------------------------------------------------------------------
		private void PrepareGenerateList()
		{
			// UI
			DisableComponents();

			// ログ設定
			mLogWriter.TextBoxDisplay = TextBoxLog;
			ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "リストを生成します...");

			// 入力値チェック
			CheckInput();

			// 履歴管理
			Invoke(new Action(() =>
			{
				AddFolderHistory(mNicoKaraListerSettings.ParentFolderHistory, TextBoxParentFolder.Text);
				UpdateParentFolderHistoryComponents();
				AddFolderHistory(mNicoKaraListerSettings.OutputFolderHistory, TextBoxOutputFolder.Text);
				UpdateOutputFolderHistoryComponents();
			}));
			mNicoKaraListerSettings.Save();

			// 履歴管理で一部 UI が有効化されるので再度無効化
			DisableComponents();

			// データベース作成
			Directory.CreateDirectory(Path.GetDirectoryName(Application.ExecutablePath) + "\\" + NklCommon.FOLDER_NAME_CSVS);
			Directory.CreateDirectory(Path.GetDirectoryName(Application.ExecutablePath) + "\\" + NklCommon.FOLDER_NAME_DATABASE);

			DeleteOldInfoDb();
			if (!File.Exists(InfoDbPath()) || CountInfoDbRecord<TProgram>() == 0)
			{
				CreateInfoDb();
			}

			CreateFoundDb();

			// データベース情報
			ShowInfoDbInfo();
		}

		// --------------------------------------------------------------------
		// 別名から元の番組名を取得
		// oInfoDbCmd を書き換えることに注意
		// --------------------------------------------------------------------
		private String ProgramOrigin(String oAlias, SQLiteCommand oInfoDbCmd)
		{
			if (String.IsNullOrEmpty(oAlias))
			{
				return null;
			}

			oInfoDbCmd.CommandText = "SELECT * FROM " + TProgramAlias.TABLE_NAME_PROGRAM_ALIAS + " LEFT OUTER JOIN " + TProgram.TABLE_NAME_PROGRAM
					+ " ON " + TProgramAlias.TABLE_NAME_PROGRAM_ALIAS + "." + TProgram.FIELD_NAME_PROGRAM_ID + " = " + TProgram.TABLE_NAME_PROGRAM + "." + TProgram.FIELD_NAME_PROGRAM_ID
					+ " WHERE " + TProgramAlias.TABLE_NAME_PROGRAM_ALIAS + "." + TProgramAlias.FIELD_NAME_PROGRAM_ALIAS_ALIAS + " = @alias";
			oInfoDbCmd.Parameters.Add(new SQLiteParameter("@alias", oAlias));

			using (SQLiteDataReader aReader = oInfoDbCmd.ExecuteReader())
			{
				while (aReader.Read())
				{
					return aReader[TProgram.FIELD_NAME_PROGRAM_NAME].ToString();
				}
			}

			return oAlias;
		}

		// --------------------------------------------------------------------
		// 選択された出力アドオン
		// --------------------------------------------------------------------
		private OutputWriter SelectedOutputWriter()
		{
			String aFormatName = (String)ComboBoxOutputFormat.Items[ComboBoxOutputFormat.SelectedIndex];

			foreach (OutputWriter aOutputWriter in mOutputWriters)
			{
				if (aOutputWriter.FormatName == aFormatName)
				{
					return aOutputWriter;
				}
			}

			return null;
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
		// 検出ファイルレコードの値を、フォルダー設定や情報キャッシュデータベースから検索して設定する
		// --------------------------------------------------------------------
		private void SetTFoundValue(TFound oRecord, FolderSettingsInMemory oFolderSettingsInMemory, SQLiteCommand oInfoDbCmd)
		{
			// ファイル名と合致する命名規則を探す
			Dictionary<String, String> aDicByFile = NklCommon.MatchFileNameRules(Path.GetFileNameWithoutExtension(oRecord.Path), oFolderSettingsInMemory);

			// 情報キャッシュデータベースを適用
			SetTFoundValueByInfoDb(oRecord, aDicByFile, oInfoDbCmd);

			// フォルダールール→検出ファイルレコードにコピー
			// 情報キャッシュデータベースの値よりも優先する（上書きする）
			Dictionary<String, String> aDicByFolder = oFolderSettingsInMemory.FolderNameRules;

			// 番組マスターにも同様の項目があるもの
			oRecord.ProgramCategory = aDicByFolder[NklCommon.RULE_VAR_CATEGORY] != null ? aDicByFolder[NklCommon.RULE_VAR_CATEGORY] : oRecord.ProgramCategory;
			oRecord.ProgramGameCategory = aDicByFolder[NklCommon.RULE_VAR_GAME_CATEGORY] != null ? aDicByFolder[NklCommon.RULE_VAR_GAME_CATEGORY] : oRecord.ProgramGameCategory;
			oRecord.ProgramName = aDicByFolder[NklCommon.RULE_VAR_PROGRAM] != null ? aDicByFolder[NklCommon.RULE_VAR_PROGRAM] : oRecord.ProgramName;
			oRecord.ProgramAgeLimit = aDicByFolder[NklCommon.RULE_VAR_AGE_LIMIT] != null ? aDicByFolder[NklCommon.RULE_VAR_AGE_LIMIT] : oRecord.ProgramAgeLimit;

			// 楽曲マスターにも同様の項目があるもの
			oRecord.SongOpEd = aDicByFolder[NklCommon.RULE_VAR_OP_ED] != null ? aDicByFolder[NklCommon.RULE_VAR_OP_ED] : oRecord.SongOpEd;
			oRecord.SongName = aDicByFolder[NklCommon.RULE_VAR_TITLE] != null ? aDicByFolder[NklCommon.RULE_VAR_TITLE] : oRecord.SongName;
			oRecord.SongArtist = aDicByFolder[NklCommon.RULE_VAR_ARTIST] != null ? aDicByFolder[NklCommon.RULE_VAR_ARTIST] : oRecord.SongArtist;

			// 両マスターにないもの
			oRecord.SongRuby = aDicByFolder[NklCommon.RULE_VAR_TITLE_RUBY] != null ? aDicByFolder[NklCommon.RULE_VAR_TITLE_RUBY] : oRecord.SongRuby;
			oRecord.Worker = aDicByFolder[NklCommon.RULE_VAR_WORKER] != null ? aDicByFolder[NklCommon.RULE_VAR_WORKER] : oRecord.Worker;
			oRecord.Track = aDicByFolder[NklCommon.RULE_VAR_TRACK] != null ? aDicByFolder[NklCommon.RULE_VAR_TRACK] : oRecord.Track;
			oRecord.SmartTrackOnVocal = aDicByFolder[NklCommon.RULE_VAR_ON_VOCAL] != null ? NklCommon.RULE_VALUE_VOCAL_DEFAULT : oRecord.SmartTrackOnVocal;
			oRecord.SmartTrackOffVocal = aDicByFolder[NklCommon.RULE_VAR_OFF_VOCAL] != null ? NklCommon.RULE_VALUE_VOCAL_DEFAULT : oRecord.SmartTrackOffVocal;
			oRecord.Comment = aDicByFolder[NklCommon.RULE_VAR_COMMENT] != null ? aDicByFolder[NklCommon.RULE_VAR_COMMENT] : oRecord.Comment;

			// ファイル名→検出ファイルレコードにコピー
			// 情報キャッシュデータベースの値よりも優先する（上書きする）

			// 番組マスターにも同様の項目があるもの
			oRecord.ProgramCategory = aDicByFile[NklCommon.RULE_VAR_CATEGORY] != null ? aDicByFile[NklCommon.RULE_VAR_CATEGORY] : oRecord.ProgramCategory;
			oRecord.ProgramGameCategory = aDicByFile[NklCommon.RULE_VAR_GAME_CATEGORY] != null ? aDicByFile[NklCommon.RULE_VAR_GAME_CATEGORY] : oRecord.ProgramGameCategory;
			oRecord.ProgramName = aDicByFile[NklCommon.RULE_VAR_PROGRAM] != null ? aDicByFile[NklCommon.RULE_VAR_PROGRAM] : oRecord.ProgramName;
			oRecord.ProgramAgeLimit = aDicByFile[NklCommon.RULE_VAR_AGE_LIMIT] != null ? aDicByFile[NklCommon.RULE_VAR_AGE_LIMIT] : oRecord.ProgramAgeLimit;

			// 楽曲マスターにも同様の項目があるもの
			oRecord.SongOpEd = aDicByFile[NklCommon.RULE_VAR_OP_ED] != null ? aDicByFile[NklCommon.RULE_VAR_OP_ED] : oRecord.SongOpEd;
			oRecord.SongName = aDicByFile[NklCommon.RULE_VAR_TITLE] != null ? aDicByFile[NklCommon.RULE_VAR_TITLE] : oRecord.SongName;
			oRecord.SongArtist = aDicByFile[NklCommon.RULE_VAR_ARTIST] != null ? aDicByFile[NklCommon.RULE_VAR_ARTIST] : oRecord.SongArtist;

			// 両マスターにないもの
			oRecord.SongRuby = aDicByFile[NklCommon.RULE_VAR_TITLE_RUBY] != null ? aDicByFile[NklCommon.RULE_VAR_TITLE_RUBY] : oRecord.SongRuby;
			oRecord.Worker = aDicByFile[NklCommon.RULE_VAR_WORKER] != null ? aDicByFile[NklCommon.RULE_VAR_WORKER] : oRecord.Worker;
			oRecord.Track = aDicByFile[NklCommon.RULE_VAR_TRACK] != null ? aDicByFile[NklCommon.RULE_VAR_TRACK] : oRecord.Track;
			oRecord.SmartTrackOnVocal = aDicByFile[NklCommon.RULE_VAR_ON_VOCAL] != null ? NklCommon.RULE_VALUE_VOCAL_DEFAULT : oRecord.SmartTrackOnVocal;
			oRecord.SmartTrackOffVocal = aDicByFile[NklCommon.RULE_VAR_OFF_VOCAL] != null ? NklCommon.RULE_VALUE_VOCAL_DEFAULT : oRecord.SmartTrackOffVocal;
			oRecord.Comment = aDicByFile[NklCommon.RULE_VAR_COMMENT] != null ? aDicByFile[NklCommon.RULE_VAR_COMMENT] : oRecord.Comment;

			// トラック情報からスマートトラック解析
			Boolean aHasOn;
			Boolean aHasOff;
			AnalyzeSmartTrack(oRecord.Track, out aHasOn, out aHasOff);
			if (aHasOn)
			{
				oRecord.SmartTrackOnVocal = NklCommon.RULE_VALUE_VOCAL_DEFAULT;
			}
			if (aHasOff)
			{
				oRecord.SmartTrackOffVocal = NklCommon.RULE_VALUE_VOCAL_DEFAULT;
			}

			// ルビが無い場合は漢字を採用
			if (String.IsNullOrEmpty(oRecord.ProgramRuby))
			{
				oRecord.ProgramRuby = oRecord.ProgramName;
			}
			if (String.IsNullOrEmpty(oRecord.SongRuby))
			{
				oRecord.SongRuby = oRecord.SongName;
			}

			// 頭文字
			if (!String.IsNullOrEmpty(oRecord.ProgramRuby))
			{
				oRecord.Head = Head(oRecord.ProgramRuby);
			}
			else
			{
				oRecord.Head = Head(oRecord.SongRuby);
			}

			// 番組名が無い場合は頭文字を採用（ボカロ曲等のリスト化用）
			if (String.IsNullOrEmpty(oRecord.ProgramName))
			{
				oRecord.ProgramName = oRecord.Head;
			}
		}

		// --------------------------------------------------------------------
		// 検出ファイルレコードの値を、情報キャッシュデータベースから検索して設定する
		// oDicByFile の内容を改変する（別名を元の名前に戻す）ことがある
		// --------------------------------------------------------------------
		private void SetTFoundValueByInfoDb(TFound oRecord, Dictionary<String, String> oDicByFile, SQLiteCommand oInfoDbCmd)
		{
			String aProgramByFile = oDicByFile[NklCommon.RULE_VAR_PROGRAM];
			String aSongByFile = oDicByFile[NklCommon.RULE_VAR_TITLE];

			if (oDicByFile[NklCommon.RULE_VAR_TITLE] != null)
			{
				// ファイル名から「楽曲名」を取得できている場合は、「楽曲名」を含む条件で検索
				// まずは別名機能を使わずに、ファイル名から取得した情報のままで検索
				// LINQ で
				// var aQueryResult =
				//		from s in aTableSong
				//		from p in aTableProgram.Where(x => x.Id == s.ProgramId).DefaultIfEmpty()
				//		where s.Name == "hoge"
				//		select new
				//		{
				//			s,
				//			p
				//		};
				// のようにすると SQLite warning (284): automatic index on sqlite_sq_C473B20(program_id) のような警告が出て動作が遅くなるため LINQ は使わない
				oInfoDbCmd.Parameters.Clear();
				StringBuilder aSB = new StringBuilder();
				aSB.Append("SELECT * FROM " + TSong.TABLE_NAME_SONG + " LEFT OUTER JOIN " + TProgram.TABLE_NAME_PROGRAM
						+ " ON " + TSong.TABLE_NAME_SONG + "." + TProgram.FIELD_NAME_PROGRAM_ID + " = " + TProgram.TABLE_NAME_PROGRAM + "." + TProgram.FIELD_NAME_PROGRAM_ID
						+ " WHERE " + TSong.TABLE_NAME_SONG + "." + TSong.FIELD_NAME_SONG_NAME + " = @title");
				oInfoDbCmd.Parameters.Add(new SQLiteParameter("@title", oDicByFile[NklCommon.RULE_VAR_TITLE]));
				if (oDicByFile[NklCommon.RULE_VAR_CATEGORY] != null)
				{
					aSB.Append(" AND " + TProgram.TABLE_NAME_PROGRAM + "." + TProgram.FIELD_NAME_PROGRAM_CATEGORY + " = @category");
					oInfoDbCmd.Parameters.Add(new SQLiteParameter("@category", oDicByFile[NklCommon.RULE_VAR_CATEGORY]));
				}
				if (oDicByFile[NklCommon.RULE_VAR_PROGRAM] != null)
				{
					aSB.Append(" AND " + TProgram.TABLE_NAME_PROGRAM + "." + TProgram.FIELD_NAME_PROGRAM_NAME + " = @program");
					oInfoDbCmd.Parameters.Add(new SQLiteParameter("@program", oDicByFile[NklCommon.RULE_VAR_PROGRAM]));
				}
				if (oDicByFile[NklCommon.RULE_VAR_OP_ED] != null)
				{
					aSB.Append(" AND " + TSong.TABLE_NAME_SONG + "." + TSong.FIELD_NAME_SONG_OP_ED + " = @oped");
					oInfoDbCmd.Parameters.Add(new SQLiteParameter("@oped", oDicByFile[NklCommon.RULE_VAR_OP_ED]));
				}
				if (oDicByFile[NklCommon.RULE_VAR_ARTIST] != null)
				{
					aSB.Append(" AND " + TSong.TABLE_NAME_SONG + "." + TSong.FIELD_NAME_SONG_ARTIST + " = @artist");
					oInfoDbCmd.Parameters.Add(new SQLiteParameter("@artist", oDicByFile[NklCommon.RULE_VAR_ARTIST]));
				}
				oInfoDbCmd.CommandText = aSB.ToString();
				//Debug.WriteLine("SetTFoundValueByInfoDb() CommandText: " + oInfoDbCmd.CommandText);
				if (SetTFoundValueByInfoDbCmd(oRecord, oInfoDbCmd, true))
				{
					return;
				}

				// ファイル名から取得した情報ではヒットしなかったので、別名を元の名前に戻して検索
				oDicByFile[NklCommon.RULE_VAR_PROGRAM] = ProgramOrigin(aProgramByFile, oInfoDbCmd);
				oDicByFile[NklCommon.RULE_VAR_TITLE] = SongOrigin(aSongByFile, oInfoDbCmd);

				if (oDicByFile[NklCommon.RULE_VAR_PROGRAM] != aProgramByFile || oDicByFile[NklCommon.RULE_VAR_TITLE] != aSongByFile)
				{
					oInfoDbCmd.CommandText = aSB.ToString();

					// oInfoDbCmd.Parameters.Add() は既存のパラメーターを上書きするようなので、削除してから追加、の手順は不要のようだ
					oInfoDbCmd.Parameters.Add(new SQLiteParameter("@program", oDicByFile[NklCommon.RULE_VAR_PROGRAM]));
					oInfoDbCmd.Parameters.Add(new SQLiteParameter("@title", oDicByFile[NklCommon.RULE_VAR_TITLE]));

					if (SetTFoundValueByInfoDbCmd(oRecord, oInfoDbCmd, true))
					{
						return;
					}
				}
			}

			if (oDicByFile[NklCommon.RULE_VAR_PROGRAM] != null)
			{
				// ファイル名から「番組名」を取得できている場合は、「番組名」で検索
				// まずは別名機能を使わずに、ファイル名から取得した情報のままで検索
				String aCmd = "SELECT * FROM " + TProgram.TABLE_NAME_PROGRAM + " WHERE " + TProgram.FIELD_NAME_PROGRAM_NAME + " = @program";
				oInfoDbCmd.CommandText = aCmd;
				oInfoDbCmd.Parameters.Clear();
				oInfoDbCmd.Parameters.Add(new SQLiteParameter("@program", aProgramByFile));

				if (SetTFoundValueByInfoDbCmd(oRecord, oInfoDbCmd, false))
				{
					return;
				}

				// ファイル名から取得した情報ではヒットしなかったので、別名を元の名前に戻して検索
				oDicByFile[NklCommon.RULE_VAR_PROGRAM] = ProgramOrigin(aProgramByFile, oInfoDbCmd);
				if (oDicByFile[NklCommon.RULE_VAR_PROGRAM] != aProgramByFile)
				{
					oInfoDbCmd.CommandText = aCmd;
					oInfoDbCmd.Parameters.Add(new SQLiteParameter("@program", oDicByFile[NklCommon.RULE_VAR_PROGRAM]));

					if (SetTFoundValueByInfoDbCmd(oRecord, oInfoDbCmd, false))
					{
						return;
					}
				}
			}
		}

		// --------------------------------------------------------------------
		// 検出ファイルレコードの値を、SQL コマンドから設定する
		// コマンド実行結果が空の場合は false を返す
		// --------------------------------------------------------------------
		private Boolean SetTFoundValueByInfoDbCmd(TFound oRecord, SQLiteCommand oInfoDbCmd, Boolean oApplySong)
		{
			using (SQLiteDataReader aReader = oInfoDbCmd.ExecuteReader())
			{
				while (aReader.Read())
				{
					// コマンド実行結果があるので、情報キャッシュデータベース→検出ファイルレコードにコピー

					// TProgram 由来
					String aCategory = aReader[TProgram.FIELD_NAME_PROGRAM_CATEGORY].ToString();
					if (mCategoryUnityMap.ContainsKey(aCategory))
					{
						oRecord.ProgramCategory = mCategoryUnityMap[aCategory];
					}
					else
					{
						oRecord.ProgramCategory = aCategory;
					}
					oRecord.ProgramGameCategory = aReader[TProgram.FIELD_NAME_PROGRAM_GAME_CATEGORY].ToString();
					oRecord.ProgramName = aReader[TProgram.FIELD_NAME_PROGRAM_NAME].ToString();
					oRecord.ProgramRuby = aReader[TProgram.FIELD_NAME_PROGRAM_RUBY].ToString();
					oRecord.ProgramSubName = aReader[TProgram.FIELD_NAME_PROGRAM_SUB_NAME].ToString();
					oRecord.ProgramSubRuby = aReader[TProgram.FIELD_NAME_PROGRAM_SUB_RUBY].ToString();
					oRecord.ProgramNumStories = aReader[TProgram.FIELD_NAME_PROGRAM_NUM_STORIES].ToString();
					oRecord.ProgramAgeLimit = aReader[TProgram.FIELD_NAME_PROGRAM_AGE_LIMIT].ToString();
					String aBeginDate = aReader[TProgram.FIELD_NAME_PROGRAM_BEGIN_DATE].ToString();
					if (!String.IsNullOrEmpty(aBeginDate))
					{
						oRecord.ProgramBeginDate = Double.Parse(aReader[TProgram.FIELD_NAME_PROGRAM_BEGIN_DATE].ToString());
					}

					// TSong 由来
					if (oApplySong)
					{
						oRecord.SongCategory = aReader[TSong.FIELD_NAME_SONG_CATEGORY].ToString();
						oRecord.SongOpEd = aReader[TSong.FIELD_NAME_SONG_OP_ED].ToString();
						oRecord.SongCastSeq = aReader[TSong.FIELD_NAME_SONG_CAST_SEQ].ToString();
						oRecord.SongName = aReader[TSong.FIELD_NAME_SONG_NAME].ToString();
						oRecord.SongArtist = aReader[TSong.FIELD_NAME_SONG_ARTIST].ToString();
					}

					return true;

				}
			}
			return false;
		}

		// --------------------------------------------------------------------
		// 改訂履歴の表示
		// --------------------------------------------------------------------
		private void ShowHistory()
		{
			try
			{
				Process.Start(Path.GetDirectoryName(Application.ExecutablePath) + "\\" + FILE_NAME_HISTORY);
			}
			catch (Exception)
			{
				ShowLogMessage(TraceEventType.Error, "改訂履歴を表示できませんでした。\n" + FILE_NAME_HISTORY);
			}
		}

		// --------------------------------------------------------------------
		// 情報データベースの情報を表示
		// --------------------------------------------------------------------
		private void ShowInfoDbInfo()
		{
			ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "番組マスターデータベース："
					+ CountInfoDbRecord<TProgram>().ToString("#,0") + " 件");
			ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲マスターデータベース："
					+ CountInfoDbRecord<TSong>().ToString("#,0") + " 件");
			ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "番組別名データベース："
					+ CountInfoDbRecord<TProgramAlias>().ToString("#,0") + " 件");
			ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲別名データベース："
					+ CountInfoDbRecord<TSongAlias>().ToString("#,0") + " 件");

			mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();
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
		// 別名から元の楽曲名を取得
		// oInfoDbCmd を書き換えることに注意
		// --------------------------------------------------------------------
		private String SongOrigin(String oAlias, SQLiteCommand oInfoDbCmd)
		{
			if (String.IsNullOrEmpty(oAlias))
			{
				return null;
			}

			oInfoDbCmd.CommandText = "SELECT * FROM " + TSongAlias.TABLE_NAME_SONG_ALIAS + " LEFT OUTER JOIN " + TSong.TABLE_NAME_SONG
					+ " ON " + TSongAlias.TABLE_NAME_SONG_ALIAS + "." + TSong.FIELD_NAME_SONG_ID + " = " + TSong.TABLE_NAME_SONG + "." + TSong.FIELD_NAME_SONG_ID
					+ " WHERE " + TSongAlias.TABLE_NAME_SONG_ALIAS + "." + TSongAlias.FIELD_NAME_SONG_ALIAS_ALIAS + " = @alias";
			oInfoDbCmd.Parameters.Add(new SQLiteParameter("@alias", oAlias));

			using (SQLiteDataReader aReader = oInfoDbCmd.ExecuteReader())
			{
				while (aReader.Read())
				{
					return aReader[TSong.FIELD_NAME_SONG_NAME].ToString();
				}
			}

			return oAlias;
		}

		// --------------------------------------------------------------------
		// フォルダー一覧に列挙されているフォルダーのうち、対象チェック付きのフォルダーのみを返す
		// --------------------------------------------------------------------
		private List<String> TargetFolders()
		{
			List<String> aTargetFolders = new List<String>();

			Invoke(new Action(() =>
			{
				for (Int32 i = 0; i < DataGridViewTargetFolders.Rows.Count; i++)
				{
					if ((Boolean)DataGridViewTargetFolders.Rows[i].Cells[(Int32)FolderColumns.IsValid].Value)
					{
						aTargetFolders.Add((String)DataGridViewTargetFolders.Rows[i].Cells[(Int32)FolderColumns.Folder].Value);
					}
				}
			}));
			return aTargetFolders;
		}

		// --------------------------------------------------------------------
		// イベントハンドラー：フォルダー系テキストボックスへのファイルドロップ（フォルダー系テキストボックス共通）
		// --------------------------------------------------------------------
		private void TextBoxFolder_DragDrop(Object oSender, DragEventArgs oEvent)
		{
			try
			{
				String[] aDropFiles = (String[])oEvent.Data.GetData(DataFormats.FileDrop, false);
				TextBox aTextBox = (TextBox)oSender;
				String aPath = aDropFiles[0];
				if (File.Exists(aPath))
				{
					aTextBox.Text = Path.GetDirectoryName(aPath);
				}
				else
				{
					aTextBox.Text = aPath;
				}
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "テキストボックスドラッグ＆ドロップ時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー：ドラッグの受け入れ判定（フォルダー系テキストボックス共通）
		// --------------------------------------------------------------------
		private void TextBoxFolder_DragEnter(Object oSender, DragEventArgs oEvent)
		{
			if (oEvent.Data.GetDataPresent(DataFormats.FileDrop))
			{
				// ファイルのときは受け付ける
				oEvent.Effect = DragDropEffects.Copy;
			}
			else
			{
				// ファイル以外は受け付けない
				oEvent.Effect = DragDropEffects.None;
			}
		}

		// --------------------------------------------------------------------
		// 出力設定ボタンを更新
		// --------------------------------------------------------------------
		private void UpdateButtonOutputSettings()
		{
			OutputWriter aSelectedOutputWriter = SelectedOutputWriter();
			if (aSelectedOutputWriter == null)
			{
				return;
			}

			ButtonOutputSettings.Enabled = aSelectedOutputWriter.IsDialogEnabled();
		}

		// --------------------------------------------------------------------
		// フォルダー一覧を更新
		// --------------------------------------------------------------------
		private void UpdateDataGridViewTargetFolders(Object oDummy)
		{
			try
			{
				// 更新の必要の確認
				String aParentFolder = null;
				Boolean aNeedUpdate = true;
				Invoke(new Action(() =>
				{
					aParentFolder = TextBoxParentFolder.Text;
					if (aNeedUpdate && String.IsNullOrEmpty(aParentFolder))
					{
						DataGridViewTargetFolders.Rows.Clear();
						aNeedUpdate = false;
					}
					if (aNeedUpdate && aParentFolder.Length < 3)
					{
						// "E:" のような '\\' 無しのドライブ名は挙動が変なので 3 文字以上を対象とする
						aNeedUpdate = false;
					}
					if (aNeedUpdate && !Directory.Exists(aParentFolder))
					{
						aNeedUpdate = false;
					}
					if (aNeedUpdate && DataGridViewTargetFolders.Rows.Count > 0
							&& NklCommon.IsSamePath(aParentFolder, (String)DataGridViewTargetFolders.Rows[0].Cells[(Int32)FolderColumns.Folder].Value))
					{
						aNeedUpdate = false;
					}
				}));
				if (!aNeedUpdate)
				{
					return;
				}

				// 準備
				DisableComponents();
				SetCursor(Cursors.WaitCursor);

				// ログ設定
				mLogWriter.TextBoxDisplay = TextBoxLog;
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, aParentFolder + " のサブフォルダーを検索します...");

				// 2,000 個ほどのフォルダーで確認したところ、DataGridViewTargetFolders.SuspendLayout() / ResumeLayout() で
				// 囲んでも高速化されなかった
				Invoke(new Action(() =>
				{
					// クリア
					DataGridViewTargetFolders.Rows.Clear();

					// 検索
					List<String> aFolders = FindSubFolders();
					foreach (String aFolder in aFolders)
					{
						AddFolderToDataGridViewTargetFolders(aFolder);
						mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();
					}
					ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, aFolders.Count.ToString("#,0") + " 個のフォルダーがあります。");
				}));
			}
			catch (OperationCanceledException)
			{
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "フォルダー一覧更新を中止しました。");
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "フォルダー一覧更新時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				// 後片付け
				mLogWriter.TextBoxDisplay = null;
				SetCursor(Cursors.Default);
				EnableComponents();
			}
		}

		// --------------------------------------------------------------------
		// フォルダー一覧の設定の有無を更新（oRowIndex およびそのサブフォルダーの行）
		// --------------------------------------------------------------------
		private void UpdateDataGridViewTargetFoldersSettingsExist(Int32 oRowIndex)
		{
			try
			{
				Invoke(new Action(() =>
				{
					// 更新の必要性を確認
					String aFolder = (String)DataGridViewTargetFolders.Rows[oRowIndex].Cells[(Int32)FolderColumns.Folder].Value;
					if ((String)DataGridViewTargetFolders.Rows[oRowIndex].Cells[(Int32)FolderColumns.SettingsExist].Value
							== FolderSettingsStatusString(NklCommon.FolderSettingsStatus(aFolder)))
					{
						return;
					}

					// 更新準備
					DisableComponents();
					SetCursor(Cursors.WaitCursor);

					// 更新
					for (Int32 i = oRowIndex; i < DataGridViewTargetFolders.Rows.Count; i++)
					{
						String aSubFolder = (String)DataGridViewTargetFolders.Rows[i].Cells[(Int32)FolderColumns.Folder].Value;
						if (aSubFolder.IndexOf(aFolder) != 0)
						{
							break;
						}
						DataGridViewTargetFolders.Rows[i].Cells[(Int32)FolderColumns.SettingsExist].Value
								= FolderSettingsStatusString(NklCommon.FolderSettingsStatus(aSubFolder));
						mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();
					}
				}));

			}
			catch (OperationCanceledException)
			{
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "フォルダー一覧（設定の有無）更新を中止しました。");
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "フォルダー一覧の設定有無の更新時エラー：\n" + oExcep.Message);
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
		// リスト出力先フォルダー履歴コンポーネントの更新
		// --------------------------------------------------------------------
		private void UpdateOutputFolderHistoryComponents()
		{
			ContextMenuOutputFolderHistory.Items.Clear();
			foreach (String aPath in mNicoKaraListerSettings.OutputFolderHistory)
			{
				ContextMenuOutputFolderHistory.Items.Add(aPath, null, ContextMenuOutputFolderHistoryItem_Click);
			}
			ButtonOutputFolderHistory.Enabled = mNicoKaraListerSettings.OutputFolderHistory.Count > 0;
		}

		// --------------------------------------------------------------------
		// リスト化対象フォルダー履歴コンポーネントの更新
		// --------------------------------------------------------------------
		private void UpdateParentFolderHistoryComponents()
		{
			ContextMenuParentFolderHistory.Items.Clear();
			foreach (String aPath in mNicoKaraListerSettings.ParentFolderHistory)
			{
				ContextMenuParentFolderHistory.Items.Add(aPath, null, ContextMenuParentFolderHistoryItem_Click);
			}
			ButtonParentFolderHistory.Enabled = mNicoKaraListerSettings.ParentFolderHistory.Count > 0;
		}

		// ====================================================================
		// IDE 生成イベントハンドラー（メインフォーム）
		// ====================================================================

		private void FormNicoKaraLister_Load(object sender, EventArgs e)
		{
			try
			{
				Init();
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "起動しました：" + NklCommon.APP_NAME_J + " "
						+ NklCommon.APP_VER + " ====================");
#if DEBUG
				ShowLogMessage(TraceEventType.Verbose, "デバッグモード：" + Common.DEBUG_ENABLED_MARK);
#endif

			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "起動時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private async void ButtonGo_Click(object sender, EventArgs e)
		{
			await NklCommon.LaunchTaskAsync<Object>(GenerateList, mTaskLock, null);
		}

		private void FormNicoKaraLister_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				// 終了時タスクキャンセル
				mClosingCancellationTokenSource.Cancel();
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "終了処理中...");

				// 終了時の状態
				mNicoKaraListerSettings.ParentFolder = TextBoxParentFolder.Text;
				mNicoKaraListerSettings.OutputFolder = TextBoxOutputFolder.Text;
				mNicoKaraListerSettings.OutputFormat = ComboBoxOutputFormat.SelectedIndex;
				mNicoKaraListerSettings.PrevLaunchPath = Application.ExecutablePath;
				mNicoKaraListerSettings.PrevLaunchVer = NklCommon.APP_VER;
				mNicoKaraListerSettings.DesktopBounds = DesktopBounds;
				mNicoKaraListerSettings.Save();

				// テンポラリーフォルダー削除
				try
				{
					Directory.Delete(NklCommon.TempPath(), true);
				}
				catch
				{
				}

				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "終了しました：" + NklCommon.APP_NAME_J + " "
						+ NklCommon.APP_VER + " --------------------");

			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "終了時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormNicoKaraLister_Shown(object sender, EventArgs e)
		{
			try
			{
				// 更新起動時とパス変更時の記録
				// 新規起動時は、両フラグが立つのでダブらないように注意
				Boolean aVerChanged = mNicoKaraListerSettings.PrevLaunchVer != NklCommon.APP_VER;
				if (aVerChanged)
				{
					// ユーザーにメッセージ表示する前にログしておく
					if (String.IsNullOrEmpty(mNicoKaraListerSettings.PrevLaunchVer))
					{
						mLogWriter.LogMessage(TraceEventType.Information, "新規起動：" + NklCommon.APP_VER);
					}
					else
					{
						mLogWriter.LogMessage(TraceEventType.Information, "更新起動：" + mNicoKaraListerSettings.PrevLaunchVer + "→" + NklCommon.APP_VER);
					}
				}
				Boolean aPathChanged = (String.Compare(mNicoKaraListerSettings.PrevLaunchPath, Application.ExecutablePath, true) != 0);
				if (aPathChanged && !String.IsNullOrEmpty(mNicoKaraListerSettings.PrevLaunchPath))
				{
					mLogWriter.LogMessage(TraceEventType.Information, "パス変更起動：" + mNicoKaraListerSettings.PrevLaunchPath + "→" + Application.ExecutablePath);
				}

				// 更新起動時とパス変更時の処理
				if (aVerChanged || aPathChanged)
				{
					NklCommon.LogEnvironmentInfo();
				}
				if (aVerChanged)
				{
					NewVersionLaunched();
				}

				// 必要に応じてちょちょいと自動更新を起動
				if (mNicoKaraListerSettings.IsCheckRssNeeded())
				{
					if (NklCommon.LaunchUpdater(true, false, IntPtr.Zero, false, false))
					{
						mNicoKaraListerSettings.RssCheckDate = DateTime.Now.Date;
						mNicoKaraListerSettings.Save();
					}
				}

				Common.CloseIfNet45IsnotInstalled(this, NklCommon.APP_NAME_J, mLogWriter);

				// ログ欄の説明
				TextBoxLog.Text = "この欄に動作状況が表示されます。";
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "ウィンドウ表示時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private async void TextBoxParentFolder_TextChanged(object sender, EventArgs e)
		{
			await NklCommon.LaunchTaskAsync<Object>(UpdateDataGridViewTargetFolders, mTaskLock, null);
		}

		private void DataGridViewTargetFolders_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
				if (e.ColumnIndex == (Int32)FolderColumns.Settings)
				{
					ButtonFolderSettingsClicked(e.RowIndex);
				}
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "フォルダー一覧クリック時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonHelp_Click(object sender, EventArgs e)
		{
			try
			{
				ContextMenuHelp.Show(Cursor.Position);
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "ヘルプボタン（メインウィンドウ）クリック時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonBrowseParentFolder_Click(object sender, EventArgs e)
		{
			try
			{
				FolderBrowserDialogFolder.SelectedPath = TextBoxParentFolder.Text;
				if (FolderBrowserDialogFolder.ShowDialog() != DialogResult.OK)
				{
					return;
				}
				TextBoxParentFolder.Text = FolderBrowserDialogFolder.SelectedPath;
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "リスト化対象フォルダー参照時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonBrowseOutputFolder_Click(object sender, EventArgs e)
		{
			try
			{
				FolderBrowserDialogFolder.SelectedPath = TextBoxOutputFolder.Text;
				if (FolderBrowserDialogFolder.ShowDialog() != DialogResult.OK)
				{
					return;
				}
				TextBoxOutputFolder.Text = FolderBrowserDialogFolder.SelectedPath;
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "リスト出力先フォルダー参照時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ヘルプHToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				NklCommon.ShowHelp();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "ヘルプメニュークリック時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void 改訂履歴UToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				ShowHistory();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "改訂履歴メニュークリック時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void バージョン情報ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				using (FormAbout aFormAbout = new FormAbout(mLogWriter))
				{
					aFormAbout.ShowDialog(this);
				}
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "バージョン情報メニュークリック時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonParentFolderHistory_Click(object sender, EventArgs e)
		{
			try
			{
				ContextMenuParentFolderHistory.Show(TextBoxParentFolder, 0, TextBoxParentFolder.Height);
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "リスト化対象フォルダー履歴ボタンクリック時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonOutputFolderHistory_Click(object sender, EventArgs e)
		{
			try
			{
				ContextMenuOutputFolderHistory.Show(TextBoxOutputFolder, 0, TextBoxOutputFolder.Height);
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "出力先フォルダー履歴ボタンクリック時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ComboBoxOutputFormat_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateButtonOutputSettings();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "リスト出力形式選択時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonOutputSettings_Click(object sender, EventArgs e)
		{
			try
			{
				OutputWriter aSelectedOutputWriter = SelectedOutputWriter();
				if (aSelectedOutputWriter == null)
				{
					return;
				}

				if (aSelectedOutputWriter.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				// 設定変更をすべての出力者に反映
				LoadOutputSettings();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "出力設定ボタンクリック時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSettings_Click(object sender, EventArgs e)
		{
			try
			{
				using (FormNicoKaraListerSettings aFormNicoKaraListerSettings = new FormNicoKaraListerSettings(mNicoKaraListerSettings, mLogWriter))
				{
					aFormNicoKaraListerSettings.ShowDialog(this);
				}
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "環境設定ボタンクリック時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
	// public partial class FormNicoKaraLister ___END___

}
// namespace NicoKaraLister ___END___
