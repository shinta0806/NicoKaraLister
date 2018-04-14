// ============================================================================
// 
// ニコカラりすたー共通で使用する、定数・関数
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Hnx8.ReadJEnc;
using Shinta;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NicoKaraLister.Shared
{
	// ====================================================================
	// public 列挙子
	// ====================================================================

	// --------------------------------------------------------------------
	// CSV ファイルの文字コード
	// --------------------------------------------------------------------
	public enum CsvEncoding
	{
		AutoDetect,
		ShiftJis,
		Jis,
		EucJp,
		Utf16Le,
		Utf16Be,
		Utf8,
		__End__,
	}

	// --------------------------------------------------------------------
	// フォルダー一覧 DGV の列
	// --------------------------------------------------------------------
	public enum FolderColumns
	{
		IsValid,
		Folder,
		SettingsExist,
		Settings,
	}

	// --------------------------------------------------------------------
	// フォルダー設定の状態
	// --------------------------------------------------------------------
	public enum FolderSettingsStatus
	{
		None,       // 設定ファイルが存在しない
		Set,        // 当該フォルダーに設定ファイルが存在する
		Inherit,    // 親フォルダーの設定を引き継ぐ
	}

	// --------------------------------------------------------------------
	// リスト出力する項目（ほぼ TFound 準拠）
	// --------------------------------------------------------------------
	public enum OutputItems
	{
		Path,                   // フルパス
		FileName,               // ファイル名
		Head,                   // 頭文字
		Worker,                 // ニコカラ制作者
		Track,                  // トラック
		SmartTrack,             // スマートトラック
		Comment,                // 備考
		LastWriteTime,          // 最終更新日時
		FileSize,               // ファイルサイズ
		ProgramCategory,        // 番組分類
		ProgramGameCategory,    // ゲーム種別
		ProgramName,            // 番組名
		ProgramRuby,            // バングミメイ
		ProgramSubName,         // 番組名補
		ProgramSubRuby,         // バングミメイホ
		ProgramNumStories,      // 放映話数
		ProgramAgeLimit,        // 年齢制限
		ProgramBeginDate,       // 放映開始日
		SongOpEd,               // 摘要
		SongCastSeq,            // 放映順
		SongName,               // 楽曲名
		SongRuby,               // ガッキョクメイ
		SongArtist,             // 歌手名
		__End__,
	}

	// --------------------------------------------------------------------
	// program_alias.csv の列インデックス
	// --------------------------------------------------------------------
	public enum ProgramAliasCsvColumns
	{
		LineIndex,  // CSV には列が無く、プログラム側で付与
		NameOrId,
		Alias,
		ForceId,
		__End__,
	}

	// --------------------------------------------------------------------
	// program.csv の列インデックス
	// --------------------------------------------------------------------
	public enum ProgramCsvColumns
	{
		LineIndex,  // CSV には列が無く、プログラム側で付与
		Id,
		Category,
		GameCategory,
		Name,
		Ruby,
		SubName,
		SubRuby,
		NumStories,
		AgeLimit,
		BeginDate,
		__End__,
	}

	// --------------------------------------------------------------------
	// プレビュー DGV の列
	// --------------------------------------------------------------------
	public enum PreviewColumns
	{
		File,
		Matches,
		Edit,
	}

	// --------------------------------------------------------------------
	// anison_alias.csv 等の列インデックス
	// --------------------------------------------------------------------
	public enum SongAliasCsvColumns
	{
		LineIndex,  // CSV には列が無く、プログラム側で付与
		NameOrId,
		Alias,
		ForceId,
		__End__,
	}

	// --------------------------------------------------------------------
	// anison.csv 等の列インデックス
	// --------------------------------------------------------------------
	public enum SongCsvColumns
	{
		LineIndex,  // CSV には列が無く、プログラム側で付与
		ProgramId,
		Category,
		ProgramName,
		OpEd,
		CastSeq,
		Id,
		Name,
		Artist,
		__End__,
	}

	// ====================================================================
	// public デリゲート
	// ====================================================================
	public delegate void TaskAsyncDelegate<T>(T oVar);

	// ====================================================================
	// ニコカラりすたー共通
	// ====================================================================

	public class NklCommon
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		// --------------------------------------------------------------------
		// アプリの基本情報
		// --------------------------------------------------------------------
		public const String APP_ID = "NicoKaraLister";
		public const String APP_NAME_J = "ニコカラりすたー";
		public const String APP_VER = "Ver 3.26";
		public const String COPYRIGHT_J = "Copyright (C) 2017-2018 by SHINTA";

		// --------------------------------------------------------------------
		// 拡張子
		// --------------------------------------------------------------------
		public const String FILE_EXT_NKLINFO = ".nklinfo";

		// --------------------------------------------------------------------
		// フォルダー名
		// --------------------------------------------------------------------
		public const String FOLDER_NAME_CSVS = "CSVs\\";
		public const String FOLDER_NAME_DATABASE = "Database\\";
		public const String FOLDER_NAME_NICO_KARA_LISTER = "NicoKaraLister\\";
		public const String FOLDER_NAME_TEMPLATES = "Templates\\";
		public const String FOLDER_NAME_USER_CSVS = "UserCSVs\\";
		public const String FOLDER_NAME_USER_TEMPLATES = "UserTemplates\\";

		// --------------------------------------------------------------------
		// CSV ファイル名
		// --------------------------------------------------------------------
		public const String FILE_NAME_ALIAS_SUFFIX = "_alias";
		public const String FILE_NAME_ANISON_CSV = "anison" + Common.FILE_EXT_CSV;
		public const String FILE_NAME_ANISON_ALIAS_CSV = "anison" + FILE_NAME_ALIAS_SUFFIX + Common.FILE_EXT_CSV;
		public const String FILE_NAME_GAME_CSV = "game" + Common.FILE_EXT_CSV;
		public const String FILE_NAME_GAME_ALIAS_CSV = "game" + FILE_NAME_ALIAS_SUFFIX + Common.FILE_EXT_CSV;
		public const String FILE_NAME_MISC_CSV = "misc" + Common.FILE_EXT_CSV;
		public const String FILE_NAME_MISC_ALIAS_CSV = "misc" + FILE_NAME_ALIAS_SUFFIX + Common.FILE_EXT_CSV;
		public const String FILE_NAME_PROGRAM_CSV = "program" + Common.FILE_EXT_CSV;
		public const String FILE_NAME_PROGRAM_ALIAS_CSV = "program" + FILE_NAME_ALIAS_SUFFIX + Common.FILE_EXT_CSV;
		public const String FILE_NAME_SF_CSV = "sf" + Common.FILE_EXT_CSV;
		public const String FILE_NAME_SF_ALIAS_CSV = "sf" + FILE_NAME_ALIAS_SUFFIX + Common.FILE_EXT_CSV;

		// --------------------------------------------------------------------
		// データベースファイル名
		// --------------------------------------------------------------------
		//public const String FILE_NAME_ALIAS_DB = "Alias" + Common.FILE_EXT_SQLITE3;
		public const String FILE_NAME_INFO_DB = "Info" + Common.FILE_EXT_SQLITE3;

		// --------------------------------------------------------------------
		// その他のファイル名
		// --------------------------------------------------------------------
		public const String FILE_NAME_NICO_KARA_LISTER_CONFIG = APP_ID + Common.FILE_EXT_CONFIG;
		public const String FILE_PREFIX_INFO = "Info";

		// --------------------------------------------------------------------
		// アプリ独自ルールでの変数名（小文字で表記）
		// --------------------------------------------------------------------

		// 番組マスターにも同様の項目があるもの
		public const String RULE_VAR_CATEGORY = "category";
		public const String RULE_VAR_GAME_CATEGORY = "gamecategory";
		public const String RULE_VAR_PROGRAM = "program";
		//public const String RULE_VAR_PROGRAM_SUB = "programsub";
		//public const String RULE_VAR_NUM_STORIES = "numstories";
		public const String RULE_VAR_AGE_LIMIT = "agelimit";
		//public const String RULE_VAR_BEGINDATE = "begindate";

		// 楽曲マスターにも同様の項目があるもの
		public const String RULE_VAR_OP_ED = "oped";
		//public const String RULE_VAR_CAST_SEQ = "castseq";
		public const String RULE_VAR_TITLE = "title";
		public const String RULE_VAR_ARTIST = "artist";

		// ファイル名からのみ取得可能なもの
		public const String RULE_VAR_TITLE_RUBY = "titleruby";
		public const String RULE_VAR_WORKER = "worker";
		public const String RULE_VAR_TRACK = "track";
		public const String RULE_VAR_ON_VOCAL = "onvocal";
		public const String RULE_VAR_OFF_VOCAL = "offvocal";
		//public const String RULE_VAR_COMPOSER = "composer";
		//public const String RULE_VAR_LYRIST = "lyrist";
		public const String RULE_VAR_COMMENT = "comment";

		// その他
		public const String RULE_VAR_ANY = "*";

		// 開始終了
		public const String RULE_VAR_BEGIN = "<";
		public const String RULE_VAR_END = ">";

		// --------------------------------------------------------------------
		// 出力設定
		// --------------------------------------------------------------------

		// 新着日数の最小値
		public const Int32 NEW_DAYS_MIN = 1;

		// enum.OutputItems の表示名
		public static readonly String[] OUTPUT_ITEM_NAMES = new String[] { "フルパス", "ファイル名", "頭文字", "ニコカラ制作者", "トラック", "スマートトラック",
				"備考", "最終更新日時", "ファイルサイズ", "番組分類", "ゲーム種別", "番組名", "番組名ルビ", "番組名補", "番組名補ルビ", "放映話数", "年齢制限",
				"放映開始日", "摘要", "放映順", "楽曲名", "楽曲名ルビ", "歌手名", "__End__" };

		// --------------------------------------------------------------------
		// その他
		// --------------------------------------------------------------------

		// 番組分類の「その他」
		public const String CATEGORY_MISC = "その他";

		// 番組分類の「NEW」
		public const String CATEGORY_NEW = "NEW";

		// 日付の書式指定子
		public const String DATE_FORMAT = "yyyy/MM/dd";

		// 時刻の書式指定子
		public const String TIME_FORMAT = "HH:mm:ss";

		// 頭文字の「その他」
		public const String HEAD_MISC = CATEGORY_MISC;

		// RULE_VAR_ON_VOCAL / RULE_VAR_OFF_VOCAL のデフォルト値
		public const Int32 RULE_VALUE_VOCAL_DEFAULT = 1;

		// 日付が指定されていない場合はこの年にする
		public const Int32 INVALID_YEAR = 1900;

		// CSV ファイルバックアップ世代数
		public const Int32 NUM_CSV_BACKUP_GENERATIONS = 5;

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// ログ
		public static LogWriter LogWriter { get; set; }

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// CSV ファイルのバックアップを作成する
		// anison.csv のバックアップ検索時に anison_alias.csv のバックアップファイルを検出しないよう注意
		// --------------------------------------------------------------------
		public static void BackupCsv(String oCsvPath)
		{
			try
			{
				if (!File.Exists(oCsvPath))
				{
					return;
				}

				FileInfo aCsvFileInfo = new FileInfo(oCsvPath);
				String aBackupCsvPath = Path.GetDirectoryName(oCsvPath) + "\\" + Path.GetFileNameWithoutExtension(oCsvPath)
						+ "_(bak)_" + aCsvFileInfo.LastWriteTime.ToString("yyyy_MM_dd") + Common.FILE_EXT_BAK;

				// バックアップ先が既に存在していれば削除
				if (File.Exists(aBackupCsvPath))
				{
					File.Delete(aBackupCsvPath);
				}

				// バックアップ
				File.Move(oCsvPath, aBackupCsvPath);
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "CSV バックアップ作成：" + aBackupCsvPath);

				// 溢れたバックアップを削除
				List<FileInfo> aBackupFileInfos = new List<FileInfo>();
				String[] aBackupFiles = Directory.GetFiles(Path.GetDirectoryName(oCsvPath), Path.GetFileNameWithoutExtension(oCsvPath) + "_(bak)_*" + Common.FILE_EXT_BAK);
				foreach (String aBackupFile in aBackupFiles)
				{
					aBackupFileInfos.Add(new FileInfo(aBackupFile));
				}
				aBackupFileInfos.Sort((a, b) => -a.LastWriteTime.CompareTo(b.LastWriteTime));
				for (Int32 i = aBackupFileInfos.Count - 1; i >= NUM_CSV_BACKUP_GENERATIONS; i--)
				{
					//Debug.WriteLine("BackupCsv() 削除: " + aBackupFileInfos[i].FullName);
					File.Delete(aBackupFileInfos[i].FullName);
				}
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Warning, "CSV ファイルのバックアップ作成が完了しませんでした：" + oCsvPath + "\n" + oExcep.Message, true);
			}
		}

		// --------------------------------------------------------------------
		// 番組分類に応じて格納すべき CSV ファイル名（パス無し）を返す
		// --------------------------------------------------------------------
		public static String CategoryToSongCsvFileName(String oCategory)
		{
			if (String.IsNullOrEmpty(oCategory))
			{
				return FILE_NAME_MISC_CSV;
			}

			Dictionary<String, String> aCategoryUnityMap = CreateCategoryUnityMap();
			if (aCategoryUnityMap.ContainsKey(oCategory))
			{
				oCategory = aCategoryUnityMap[oCategory];
			}

			switch (oCategory)
			{
				case "アニメ":
					return FILE_NAME_ANISON_CSV;
				case "ゲーム":
					return FILE_NAME_GAME_CSV;
				case "特撮":
					return FILE_NAME_SF_CSV;
				default:
					return FILE_NAME_MISC_CSV;
			}
		}

		// --------------------------------------------------------------------
		// ID 接頭辞の正当性を確認
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		public static void CheckIdPrefix(String oIdPrefix)
		{
			if (String.IsNullOrEmpty(oIdPrefix))
			{
				throw new Exception("新規の楽曲 ID・番組 ID の先頭に付与する接頭辞を入力して下さい。");
			}
			if (oIdPrefix.IndexOf('_') >= 0)
			{
				throw new Exception("新規の楽曲 ID・番組 ID の先頭に付与する接頭辞に \"_\" は使えません。");
			}
		}

		// --------------------------------------------------------------------
		// 番組分類統合用マップを作成
		// --------------------------------------------------------------------
		public static Dictionary<String, String> CreateCategoryUnityMap()
		{
			Dictionary<String, String> aMap = new Dictionary<String, String>();

			aMap["Webアニメーション"] = "アニメ";
			aMap["オリジナルビデオアニメーション"] = "アニメ";
			aMap["テレビアニメーション"] = "アニメ";
			aMap["劇場用アニメーション"] = "アニメ";
			aMap["Webラジオ"] = "ラジオ";
			aMap["Web特撮"] = "特撮";
			aMap["オリジナル特撮ビデオ"] = "特撮";
			aMap["テレビ特撮"] = "特撮";
			aMap["テレビ特撮スペシャル"] = "特撮";
			aMap["劇場用特撮"] = "特撮";

			return aMap;
		}

		// --------------------------------------------------------------------
		// データベースに接続
		// --------------------------------------------------------------------
		public static SQLiteConnection CreateDbConnection(String oPath)
		{
			SQLiteConnectionStringBuilder aConnectionString = new SQLiteConnectionStringBuilder
			{
				DataSource = oPath,
			};
			SQLiteConnection aConnection = new SQLiteConnection(aConnectionString.ToString());
			return aConnection.OpenAndReturn();
		}

		// --------------------------------------------------------------------
		// データベースの中にプロパティーテーブルを作成
		// --------------------------------------------------------------------
		public static void CreateDbPropertyTable(SQLiteCommand oCmd, DataContext oContext)
		{
			// テーブル作成
			LinqUtils.CreateTable(oCmd, typeof(TProperty));

			// 更新
			UpdateDbProperty(oContext);
		}

		// --------------------------------------------------------------------
		// 設定ファイルのルールを動作時用に変換
		// --------------------------------------------------------------------
		public static FolderSettingsInMemory CreateFolderSettingsInMemory(FolderSettingsInDisk oFolderSettingsInDisk)
		{
			FolderSettingsInMemory aFolderSettingsInMemory = new FolderSettingsInMemory();
			String aRule;
			List<String> aGroups;

			// フォルダー命名規則を辞書に格納
			foreach (String aInDisk in oFolderSettingsInDisk.FolderNameRules)
			{
				Int32 aEqualPos = aInDisk.IndexOf('=');
				if (aEqualPos < 2)
				{
					continue;
				}
				if (aInDisk[0] != NklCommon.RULE_VAR_BEGIN[0])
				{
					continue;
				}
				if (aInDisk[aEqualPos - 1] != NklCommon.RULE_VAR_END[0])
				{
					continue;
				}

				aFolderSettingsInMemory.FolderNameRules[aInDisk.Substring(1, aEqualPos - 2).ToLower()] = aInDisk.Substring(aEqualPos + 1);
			}

			// ファイル命名規則を正規表現に変換
			for (Int32 i = 0; i < oFolderSettingsInDisk.FileNameRules.Count; i++)
			{
				// ワイルドカードのみ <> で囲まれていないので、処理をやりやすくするために <> で囲む
				String aFileNameRule = oFolderSettingsInDisk.FileNameRules[i].Replace(NklCommon.RULE_VAR_ANY,
						NklCommon.RULE_VAR_BEGIN + NklCommon.RULE_VAR_ANY + NklCommon.RULE_VAR_END);

				MakeRegexPattern(aFileNameRule, out aRule, out aGroups);
				aFolderSettingsInMemory.FileNameRules.Add(aRule);
				aFolderSettingsInMemory.FileRegexGroups.Add(aGroups);
			}

			return aFolderSettingsInMemory;
		}

		// --------------------------------------------------------------------
		// 検出ファイルデータベースの中にテーブルを作成
		// --------------------------------------------------------------------
		public static void CreateFoundDbTables(SQLiteConnection oConnection)
		{
			using (SQLiteCommand aCmd = new SQLiteCommand(oConnection))
			{
				// テーブル作成
				List<String> aUniques = new List<String>();
				aUniques.Add(TFound.FIELD_NAME_FOUND_UID);
				LinqUtils.CreateTable(aCmd, typeof(TFound), aUniques);

				// インデックス作成
				List<String> aIndices = new List<String>();
				aIndices.Add(TProgram.FIELD_NAME_PROGRAM_CATEGORY);
				aIndices.Add(TFound.FIELD_NAME_FOUND_HEAD);
				aIndices.Add(TProgram.FIELD_NAME_PROGRAM_RUBY);
				aIndices.Add(TProgram.FIELD_NAME_PROGRAM_NAME);
				aIndices.Add(TFound.FIELD_NAME_FOUND_TITLE_RUBY);
				aIndices.Add(TSong.FIELD_NAME_SONG_NAME);
				LinqUtils.CreateIndex(aCmd, LinqUtils.TableName(typeof(TFound)), aIndices);

				// プロパティーテーブル作成
				using (DataContext aContext = new DataContext(oConnection))
				{
					NklCommon.CreateDbPropertyTable(aCmd, aContext);
				}
			}
		}

		// --------------------------------------------------------------------
		// アプリ独自の変数を格納する変数を生成し、定義済みキーをすべて初期化（キーには <> は含まない）
		// ・キーが無いと LINQ で例外が発生することがあるため
		// ・キーの有無と値の null の 2 度チェックは面倒くさいため
		// --------------------------------------------------------------------
		public static Dictionary<String, String> CreateRuleDictionary()
		{
			Dictionary<String, String> aVarMapWith = CreateRuleDictionaryWithDescription();
			Dictionary<String, String> aVarMap = new Dictionary<String, String>();

			foreach (String aKey in aVarMapWith.Keys)
			{
				aVarMap[aKey] = null;
			}

			return aVarMap;
		}

		// --------------------------------------------------------------------
		// アプリ独自の変数とその説明
		// --------------------------------------------------------------------
		public static Dictionary<String, String> CreateRuleDictionaryWithDescription()
		{
			Dictionary<String, String> aVarMap = new Dictionary<String, String>();

			// 番組マスターにも同様の項目があるもの
			aVarMap[NklCommon.RULE_VAR_CATEGORY] = "番組分類";
			aVarMap[NklCommon.RULE_VAR_GAME_CATEGORY] = "ゲーム種別";
			aVarMap[NklCommon.RULE_VAR_PROGRAM] = "番組名";
			aVarMap[NklCommon.RULE_VAR_AGE_LIMIT] = "年齢制限";

			// 楽曲マスターにも同様の項目があるもの
			aVarMap[NklCommon.RULE_VAR_OP_ED] = "摘要（OP/ED 別）";
			aVarMap[NklCommon.RULE_VAR_TITLE] = "楽曲名";

			// ファイル名からのみ取得可能なもの
			aVarMap[NklCommon.RULE_VAR_TITLE_RUBY] = "ガッキョクメイ";

			// 楽曲マスターにも同様の項目があるもの
			aVarMap[NklCommon.RULE_VAR_ARTIST] = "歌手名";

			// ファイル名からのみ取得可能なもの
			aVarMap[NklCommon.RULE_VAR_WORKER] = "ニコカラ制作者";
			aVarMap[NklCommon.RULE_VAR_TRACK] = "トラック情報";
			aVarMap[NklCommon.RULE_VAR_ON_VOCAL] = "オンボーカルトラック";
			aVarMap[NklCommon.RULE_VAR_OFF_VOCAL] = "オフボーカルトラック";
			//aVarMap[NklCommon.RULE_VAR_COMPOSER] = "作曲者";
			//aVarMap[NklCommon.RULE_VAR_LYRIST] = "作詞者";
			aVarMap[NklCommon.RULE_VAR_COMMENT] = "コメント";

			// その他
			aVarMap[NklCommon.RULE_VAR_ANY] = "無視する部分";

			return aVarMap;
		}

		// --------------------------------------------------------------------
		// 楽曲の別名 CSV のファイルリストを作成（パス無し）
		// --------------------------------------------------------------------
		public static List<String> CreateSongAliasCsvList()
		{
			List<String> aSongAliasCsvs = new List<String>();
			aSongAliasCsvs.Add(NklCommon.FILE_NAME_ANISON_ALIAS_CSV);
			aSongAliasCsvs.Add(NklCommon.FILE_NAME_GAME_ALIAS_CSV);
			aSongAliasCsvs.Add(NklCommon.FILE_NAME_SF_ALIAS_CSV);
			aSongAliasCsvs.Add(NklCommon.FILE_NAME_MISC_ALIAS_CSV);
			return aSongAliasCsvs;
		}

		// --------------------------------------------------------------------
		// 楽曲 CSV のファイルリストを作成（パス無し）
		// --------------------------------------------------------------------
		public static List<String> CreateSongCsvList()
		{
			List<String> aSongCsvs = new List<String>();
			aSongCsvs.Add(FILE_NAME_ANISON_CSV);
			aSongCsvs.Add(FILE_NAME_GAME_CSV);
			aSongCsvs.Add(FILE_NAME_SF_CSV);
			aSongCsvs.Add(FILE_NAME_MISC_CSV);
			return aSongCsvs;
		}

		// --------------------------------------------------------------------
		// Enum から CSV タイトルを作成する
		// --------------------------------------------------------------------
		public static List<String> CsvTitle<T>()
		{
			// タイトル（先頭の LineIndex と末尾の __End__ は除く）
			T[] aColumns = (T[])Enum.GetValues(typeof(T));
			List<String> aTitle = new List<String>();
			for (Int32 i = 1; i < aColumns.Length - 1; i++)
			{
				aTitle.Add(aColumns[i].ToString());
			}

			return aTitle;
		}

		// --------------------------------------------------------------------
		// データベースを削除する
		// 本当はファイルを削除したいが、ファイルにアクセスするとハンドルを掴んだままとなり削除できないため、
		// 全テーブルをドロップする
		// --------------------------------------------------------------------
		public static void DeleteOldDb(String oDbPath)
		{
			try
			{
				ShowLogMessage(TraceEventType.Verbose, "DeleteOldDb() DB 削除: " + oDbPath);
				using (SQLiteConnection aDbConnection = CreateDbConnection(oDbPath))
				{
					LinqUtils.DropAllTables(aDbConnection);
				}
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Warning, "古いデータベースを削除できませんでした。\n" + oExcep.Message + "\n" + oDbPath, true);
			}
		}

		// --------------------------------------------------------------------
		// CsvEncoding から Encoding を得る
		// --------------------------------------------------------------------
		public static Encoding EncodingFromCsvEncoding(CsvEncoding oCsvEncoding)
		{
			Encoding aEncoding = null;
			switch (oCsvEncoding)
			{
				case CsvEncoding.ShiftJis:
					aEncoding = Encoding.GetEncoding(Common.CODE_PAGE_SHIFT_JIS);
					break;
				case CsvEncoding.Jis:
					aEncoding = Encoding.GetEncoding(Common.CODE_PAGE_JIS);
					break;
				case CsvEncoding.EucJp:
					aEncoding = Encoding.GetEncoding(Common.CODE_PAGE_EUC_JP);
					break;
				case CsvEncoding.Utf16Le:
					aEncoding = Encoding.Unicode;
					break;
				case CsvEncoding.Utf16Be:
					aEncoding = Encoding.GetEncoding(Common.CODE_PAGE_UTF_16_BE);
					break;
				case CsvEncoding.Utf8:
					aEncoding = Encoding.UTF8;
					break;
				default:
					Debug.Assert(false, "EncodingFromCsvEncoding() bad csv encoding");
					break;
			}
			return aEncoding;
		}

		// --------------------------------------------------------------------
		// CSV から対応する別名 CSV ファイル名を得る
		// 楽曲 CSV にも番組 CSV にも使える
		// --------------------------------------------------------------------
		public static String FileNameToAliasFileName(String oSongFileName)
		{
			return Path.GetFileNameWithoutExtension(oSongFileName) + FILE_NAME_ALIAS_SUFFIX + Path.GetExtension(oSongFileName);
		}

		// --------------------------------------------------------------------
		// 指定された列の文字列と一致する行（1 行）を返す
		// 見つからない場合は null を返す
		// --------------------------------------------------------------------
		public static List<String> FindCsvRecord(Dictionary<String, List<List<String>>> oCsvs, Int32 oColumnIndex, String oTargetString)
		{
			String aCsvPath;
			return FindCsvRecord(oCsvs, oColumnIndex, oTargetString, out aCsvPath);
		}

		// --------------------------------------------------------------------
		// 指定された列の文字列と最初に一致する行（1 行）と、当該行を格納している CSV ファイルのパスを返す
		// 見つからない場合は null を返す
		// 楽曲情報・番組情報編集ウィンドウでの作業のように、情報キャッシュデータベースが更新されていない状態で
		// 情報にアクセスする必要がある場合に、この関数で CSV に直接アクセスする
		// --------------------------------------------------------------------
		public static List<String> FindCsvRecord(Dictionary<String, List<List<String>>> oCsvs, Int32 oColumnIndex, String oTargetString, out String oCsvPath)
		{
			foreach (KeyValuePair<String, List<List<String>>> aCsv in oCsvs)
			{
				for (Int32 i = 0; i < aCsv.Value.Count; i++)
				{
					List<String> aRecord = aCsv.Value[i];
					if (aRecord[oColumnIndex] == oTargetString)
					{
						oCsvPath = aCsv.Key;
						return aRecord;
					}
				}
			}

			oCsvPath = null;
			return null;
		}

		// --------------------------------------------------------------------
		// 指定された列の文字列と一致する行（複数行）を返す（キーワード完全一致）
		// 見つからない場合は null ではなく空のリストを返す
		// --------------------------------------------------------------------
		public static List<List<String>> FindCsvRecords(Dictionary<String, List<List<String>>> oCsvs, Int32 oColumnIndex, String oTargetString)
		{
			List<List<String>> aRecords = new List<List<String>>();

			foreach (KeyValuePair<String, List<List<String>>> aCsv in oCsvs)
			{
				for (Int32 i = 0; i < aCsv.Value.Count; i++)
				{
					List<String> aRecord = aCsv.Value[i];
					if (aRecord[oColumnIndex] == oTargetString)
					{
						aRecords.Add(aRecord);
					}
				}
			}

			return aRecords;
		}

		// --------------------------------------------------------------------
		// 指定された列の文字列と一致する行（複数行）を返す（キーワード部分一致検索、大文字小文字区別なし）
		// 見つからない場合は null ではなく空のリストを返す
		// --------------------------------------------------------------------
		public static List<List<String>> FindCsvRecordsIncludes(Dictionary<String, List<List<String>>> oCsvs, Int32 oColumnIndex, String oTargetString)
		{
			List<List<String>> aRecords = new List<List<String>>();

			foreach (KeyValuePair<String, List<List<String>>> aCsv in oCsvs)
			{
				for (Int32 i = 0; i < aCsv.Value.Count; i++)
				{
					List<String> aRecord = aCsv.Value[i];
					if (aRecord[oColumnIndex].IndexOf(oTargetString, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						aRecords.Add(aRecord);
					}
				}
			}

			return aRecords;
		}

		// --------------------------------------------------------------------
		// 指定されたフォルダーのフォルダー設定ファイルがあるフォルダーを返す
		// --------------------------------------------------------------------
		public static String FindSettingsFolder(String oFolder)
		{
			while (!String.IsNullOrEmpty(oFolder))
			{
				if (File.Exists(oFolder + "\\" + NklCommon.FILE_NAME_NICO_KARA_LISTER_CONFIG))
				{
					return oFolder;
				}
				oFolder = Path.GetDirectoryName(oFolder);
			}
			return null;
		}

		// --------------------------------------------------------------------
		// 指定されたフォルダーの設定有無
		// --------------------------------------------------------------------
		public static FolderSettingsStatus FolderSettingsStatus(String oFolder)
		{
			String aFolderSettingsFolder = FindSettingsFolder(oFolder);
			if (String.IsNullOrEmpty(aFolderSettingsFolder))
			{
				return Shared.FolderSettingsStatus.None;
			}
			else if (IsSamePath(oFolder, aFolderSettingsFolder))
			{
				return Shared.FolderSettingsStatus.Set;
			}
			else
			{
				return Shared.FolderSettingsStatus.Inherit;
			}
		}

		// --------------------------------------------------------------------
		// 同一のファイル・フォルダーかどうか
		// 末尾の '\\' 有無や大文字小文字にかかわらず比較する
		// いずれかが null の場合は false とする
		// --------------------------------------------------------------------
		public static Boolean IsSamePath(String oPath1, String oPath2)
		{
			if (String.IsNullOrEmpty(oPath1) || String.IsNullOrEmpty(oPath2))
			{
				return false;
			}

			// 末尾の '\\' を除去
			if (oPath1[oPath1.Length - 1] == '\\')
			{
				oPath1 = oPath1.Substring(0, oPath1.Length - 1);
			}
			if (oPath2[oPath2.Length - 1] == '\\')
			{
				oPath2 = oPath2.Substring(0, oPath2.Length - 1);
			}
			return (oPath1.ToLower() == oPath2.ToLower());
		}

		// --------------------------------------------------------------------
		// システム CSV かどうか
		// --------------------------------------------------------------------
		public static Boolean IsSystemCsvPath(String oCsvPath)
		{
			return (oCsvPath.IndexOf("\\" + FOLDER_NAME_CSVS) >= 0);
		}

		// --------------------------------------------------------------------
		// 関数を非同期駆動
		// --------------------------------------------------------------------
		public static async Task LaunchTaskAsync<T>(TaskAsyncDelegate<T> oDelegate, Object oTaskLock, T oVar)
		{
			await Task.Run(() =>
			{
				Boolean aGoLocked = false;

				try
				{
					// 終了時に強制終了されないように設定
					Thread.CurrentThread.IsBackground = false;

					Monitor.TryEnter(oTaskLock, ref aGoLocked);
					if (!aGoLocked)
					{
						ShowLogMessage(TraceEventType.Error, "同時実行できない処理を実行中のため、新たな処理をスキップしました：" + oDelegate.Method.Name);
						return;
					}

					// 関数処理
					ShowLogMessage(TraceEventType.Verbose, "バックグラウンド処理開始：" + oDelegate.Method.Name);
					oDelegate(oVar);
					ShowLogMessage(TraceEventType.Verbose, "バックグラウンド処理終了：" + oDelegate.Method.Name);
				}
				catch (Exception oExcep)
				{
					ShowLogMessage(TraceEventType.Error, "バックグラウンド処理 " + oDelegate.Method.Name + "実行時エラー：\n" + oExcep.Message);
					ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
				}
				finally
				{
					if (aGoLocked)
					{
						Monitor.Exit(oTaskLock);
					}
				}
			});
		}

		// --------------------------------------------------------------------
		// ちょちょいと自動更新を起動
		// --------------------------------------------------------------------
		public static Boolean LaunchUpdater(Boolean oCheckLatest, Boolean oForceShow, IntPtr oHWnd, Boolean oClearUpdateCache, Boolean oForceInstall)
		{
			// 固定部分
			UpdaterLauncher aUpdaterLauncher = new UpdaterLauncher();
			aUpdaterLauncher.ID = APP_ID;
			aUpdaterLauncher.Name = NklCommon.APP_NAME_J;
			aUpdaterLauncher.Wait = 3;
			aUpdaterLauncher.UpdateRss = "http://shinta.coresv.com/soft/NicoKaraLister_AutoUpdate.xml";
			aUpdaterLauncher.CurrentVer = NklCommon.APP_VER;

			// 変動部分
			if (oCheckLatest)
			{
				aUpdaterLauncher.LatestRss = "http://shinta.coresv.com/soft/NicoKaraLister_JPN.xml";
			}
			aUpdaterLauncher.LogWriter = LogWriter;
			aUpdaterLauncher.ForceShow = oForceShow;
			aUpdaterLauncher.NotifyHWnd = oHWnd;
			aUpdaterLauncher.ClearUpdateCache = oClearUpdateCache;
			aUpdaterLauncher.ForceInstall = oForceInstall;

			// 起動
			return aUpdaterLauncher.Launch(oForceShow);
		}

		// --------------------------------------------------------------------
		// 環境設定の文字コードに従って CSV ファイルを読み込む
		// 下処理も行う
		// oNumColumns: 行番号も含めた列数
		// --------------------------------------------------------------------
		public static List<List<String>> LoadCsv(String oPath, NicoKaraListerSettings oNicoKaraListerSettings, Int32 oNumColumns)
		{
			List<List<String>> aCsv;

			try
			{
				Encoding aEncoding;
				if (oNicoKaraListerSettings.CsvEncoding == CsvEncoding.AutoDetect)
				{
					// 文字コード自動判別
					FileInfo aFileInfo = new FileInfo(oPath);
					using (FileReader aReader = new FileReader(aFileInfo))
					{
						aEncoding = aReader.Read(aFileInfo).GetEncoding();
					}
				}
				else
				{
					aEncoding = NklCommon.EncodingFromCsvEncoding(oNicoKaraListerSettings.CsvEncoding);
				}
				if (aEncoding == null)
				{
					throw new Exception("文字コードを判定できませんでした。");
				}
				aCsv = CsvManager.LoadCsv(oPath, aEncoding, true, true);

				// 規定列数に満たない行を削除
				for (Int32 i = aCsv.Count - 1; i >= 0; i--)
				{
					if (aCsv[i].Count < oNumColumns)
					{
						ShowLogMessage(TraceEventType.Warning,
								(Int32.Parse(aCsv[i][0]) + 2).ToString("#,0") + " 行目の項目数が不足しているため無視します。", true);
						aCsv.RemoveAt(i);
					}
				}

				// 空白削除
				for (Int32 i = 0; i < aCsv.Count; i++)
				{
					List<String> aRecord = aCsv[i];
					for (Int32 j = 0; j < aRecord.Count; j++)
					{
						aRecord[j] = aRecord[j].Trim();
					}
				}
			}
			catch (Exception oExcep)
			{
				aCsv = new List<List<String>>();
				ShowLogMessage(TraceEventType.Warning, "CSV ファイルを読み込めませんでした。\n" + oExcep.Message + "\n" + oPath, true);
			}
			return aCsv;
		}

		// --------------------------------------------------------------------
		// 環境設定の文字コードに従って全ての CSV ファイルを読み込む
		// CSV のまま処理を行う場合向け
		// --------------------------------------------------------------------
		public static void LoadCsvs(NicoKaraListerSettings oNicoKaraListerSettings, out Dictionary<String, List<List<String>>> oProgramCsvs,
				out Dictionary<String, List<List<String>>> oSongCsvs, out Dictionary<String, List<List<String>>> oProgramAliasCsvs,
				out Dictionary<String, List<List<String>>> oSongAliasCsvs)
		{
			// 番組 CSV データ（ユーザー CSV 優先）
			oProgramCsvs = new Dictionary<String, List<List<String>>>();
			LoadCsv(oProgramCsvs, UserCsvPath(FILE_NAME_PROGRAM_CSV), oNicoKaraListerSettings, (Int32)ProgramCsvColumns.__End__);
			LoadCsv(oProgramCsvs, SystemCsvPath(FILE_NAME_PROGRAM_CSV), oNicoKaraListerSettings, (Int32)ProgramCsvColumns.__End__);

			// 楽曲 CSV データ（ユーザー CSV 優先）
			oSongCsvs = new Dictionary<String, List<List<String>>>();
			List<String> aSongCsvFileNames = CreateSongCsvList();
			foreach (String aFileName in aSongCsvFileNames)
			{
				LoadCsv(oSongCsvs, UserCsvPath(aFileName), oNicoKaraListerSettings, (Int32)SongCsvColumns.__End__);
			}
			foreach (String aFileName in aSongCsvFileNames)
			{
				LoadCsv(oSongCsvs, SystemCsvPath(aFileName), oNicoKaraListerSettings, (Int32)SongCsvColumns.__End__);
			}

			// 番組別名 CSV データ（ユーザー CSV 優先）
			oProgramAliasCsvs = new Dictionary<String, List<List<String>>>();
			LoadCsv(oProgramAliasCsvs, UserCsvPath(FILE_NAME_PROGRAM_ALIAS_CSV), oNicoKaraListerSettings, (Int32)ProgramAliasCsvColumns.__End__);
			LoadCsv(oProgramAliasCsvs, SystemCsvPath(FILE_NAME_PROGRAM_ALIAS_CSV), oNicoKaraListerSettings, (Int32)ProgramAliasCsvColumns.__End__);

			// 楽曲別名 CSV データ（ユーザー CSV 優先）
			oSongAliasCsvs = new Dictionary<String, List<List<String>>>();
			List<String> aSongAliasCsvFileNames = CreateSongAliasCsvList();
			foreach (String aFileName in aSongAliasCsvFileNames)
			{
				LoadCsv(oSongAliasCsvs, UserCsvPath(aFileName), oNicoKaraListerSettings, (Int32)SongAliasCsvColumns.__End__);
			}
			foreach (String aFileName in aSongAliasCsvFileNames)
			{
				LoadCsv(oSongAliasCsvs, SystemCsvPath(aFileName), oNicoKaraListerSettings, (Int32)SongAliasCsvColumns.__End__);
			}
		}

		// --------------------------------------------------------------------
		// フォルダー設定を読み込む
		// 見つからない場合は null ではなく空のインスタンスを返す
		// --------------------------------------------------------------------
		public static FolderSettingsInDisk LoadFolderSettings(String oFolder)
		{
			FolderSettingsInDisk aFolderSettings = new FolderSettingsInDisk();
			try
			{
				String aFolderSettingsFolder = NklCommon.FindSettingsFolder(oFolder);
				if (!String.IsNullOrEmpty(aFolderSettingsFolder))
				{
					aFolderSettings = Common.Deserialize<FolderSettingsInDisk>(aFolderSettingsFolder + "\\" + NklCommon.FILE_NAME_NICO_KARA_LISTER_CONFIG);
				}
			}
			catch (Exception)
			{
			}

			// 項目が null の場合はインスタンスを作成
			if (aFolderSettings.FileNameRules == null)
			{
				aFolderSettings.FileNameRules = new List<String>();
			}
			if (aFolderSettings.FolderNameRules == null)
			{
				aFolderSettings.FolderNameRules = new List<String>();
			}

			return aFolderSettings;
		}

		// --------------------------------------------------------------------
		// 環境情報をログする
		// --------------------------------------------------------------------
		public static void LogEnvironmentInfo()
		{
			SystemEnvironment aSE = new SystemEnvironment();
			aSE.LogEnvironment(LogWriter);
		}

		// --------------------------------------------------------------------
		// ファイル名とファイル命名規則がマッチするか確認し、マッチしたマップを返す
		// ＜引数＞ oFileNameBody: 拡張子無し
		// --------------------------------------------------------------------
		public static Dictionary<String, String> MatchFileNameRules(String oFileNameBody, FolderSettingsInMemory oFolderSettingsInMemory)
		{
			Dictionary<String, String> aDic = NklCommon.CreateRuleDictionary();
			Match aMatch = null;
			Int32 aMatchIndex = -1;

			// ファイル名と合致する命名規則を探す
			for (Int32 i = 0; i < oFolderSettingsInMemory.FileNameRules.Count; i++)
			{
				aMatch = Regex.Match(oFileNameBody, oFolderSettingsInMemory.FileNameRules[i], RegexOptions.None);
				if (aMatch.Success)
				{
					aMatchIndex = i;
					break;
				}
			}
			if (aMatchIndex < 0)
			{
				return aDic;
			}

			for (Int32 i = 0; i < oFolderSettingsInMemory.FileRegexGroups[aMatchIndex].Count; i++)
			{
				// 定義されているキーのみ格納する
				if (aDic.ContainsKey(oFolderSettingsInMemory.FileRegexGroups[aMatchIndex][i]))
				{
					// aMatch.Groups[0] にはマッチした全体の値が入っているので無視し、[1] から実際の値が入っている
					aDic[oFolderSettingsInMemory.FileRegexGroups[aMatchIndex][i]] = aMatch.Groups[i + 1].Value.Trim();
				}
			}

			return aDic;
		}

		// --------------------------------------------------------------------
		// ファイル名とファイル命名規則・フォルダー固定値がマッチするか確認し、マッチしたマップを返す
		// ＜引数＞ oFileNameBody: 拡張子無し
		// --------------------------------------------------------------------
		public static Dictionary<String, String> MatchFileNameRulesAndFolderRule(String oFileNameBody, FolderSettingsInMemory oFolderSettingsInMemory)
		{
			// ファイル名命名規則
			Dictionary<String, String> aDic = NklCommon.MatchFileNameRules(oFileNameBody, oFolderSettingsInMemory);

			// フォルダー命名規則をマージ
			foreach (KeyValuePair<String, String> aFolderRule in oFolderSettingsInMemory.FolderNameRules)
			{
				if (aDic.ContainsKey(aFolderRule.Key) && String.IsNullOrEmpty(aDic[aFolderRule.Key]))
				{
					aDic[aFolderRule.Key] = aFolderRule.Value;
				}
			}

			return aDic;
		}

		// --------------------------------------------------------------------
		// CSV レコードの各列が完全に一致するものがあるか
		// oKeyColumnIndex はどの列でも構わない
		// --------------------------------------------------------------------
		public static Boolean RecordExists(Dictionary<String, List<List<String>>> oCsvs, List<String> oRecord, Int32 oKeyColumnIndex)
		{
			List<List<String>> aRecords = FindCsvRecords(oCsvs, oKeyColumnIndex, oRecord[oKeyColumnIndex]);
			for (Int32 i = 0; i < aRecords.Count; i++)
			{
				Boolean aAllMatched = true;

				// 0 列目は行番号なので無視する
				for (Int32 j = 1; j < oRecord.Count; j++)
				{
					if (oRecord[j] != aRecords[i][j])
					{
						aAllMatched = false;
						break;
					}
				}
				if (aAllMatched)
				{
					return true;
				}
			}

			return false;
		}

		// --------------------------------------------------------------------
		// 設定保存フォルダのパス（末尾 '\\'）
		// 存在しない場合は作成される
		// --------------------------------------------------------------------
		public static String SettingsPath()
		{
			return Path.GetDirectoryName(Application.UserAppDataPath) + "\\";
		}

		// --------------------------------------------------------------------
		// ヘルプの表示
		// --------------------------------------------------------------------
		public static void ShowHelp(String oAnchor = null)
		{
			String aHelpPath = null;

			try
			{
				aHelpPath = Path.GetDirectoryName(Application.ExecutablePath) + "\\";

				if (String.IsNullOrEmpty(oAnchor))
				{
					aHelpPath += FILE_NAME_HELP_PREFIX + Common.FILE_EXT_HTML;
				}
				else
				{
					aHelpPath += FOLDER_NAME_HELP_PARTS + FILE_NAME_HELP_PREFIX + "_" + oAnchor + Common.FILE_EXT_HTML;
				}

				Process.Start(aHelpPath);
			}
			catch (Exception)
			{
				ShowLogMessage(TraceEventType.Error, "ヘルプを表示できませんでした。\n" + aHelpPath);
			}
		}

		// --------------------------------------------------------------------
		// 親を無しにしてログ表示関数を呼びだす
		// --------------------------------------------------------------------
		public static DialogResult ShowLogMessage(TraceEventType oEventType, String oMessage, Boolean oSuppressMessageBox = false)
		{
			LogWriter.FrontForm = null;
			return LogWriter.ShowLogMessage(oEventType, oMessage, oSuppressMessageBox);
		}

		// --------------------------------------------------------------------
		// 公式 CSV ファイルのフルパス
		// --------------------------------------------------------------------
		public static String SystemCsvPath(String oFileName)
		{
			return Path.GetDirectoryName(Application.ExecutablePath) + "\\" + FOLDER_NAME_CSVS + oFileName;
		}

		// --------------------------------------------------------------------
		// テンポラリファイルのパス（呼びだす度に異なるファイル、拡張子なし）
		// --------------------------------------------------------------------
		public static String TempFilePath()
		{
			// マルチスレッドでも安全にインクリメント
			Int32 aCounter = Interlocked.Increment(ref smTempFilePathCounter);
			return TempPath() + Thread.CurrentThread.ManagedThreadId.ToString() + "_" + aCounter.ToString();
		}

		// --------------------------------------------------------------------
		// テンポラリフォルダのパス（末尾 '\\'）
		// 存在しない場合は作成する
		// --------------------------------------------------------------------
		public static String TempPath()
		{
			String aPath = Path.GetTempPath() + FOLDER_NAME_NICO_KARA_LISTER + Process.GetCurrentProcess().Id.ToString() + "\\";
			if (!Directory.Exists(aPath))
			{
				try
				{
					Directory.CreateDirectory(aPath);
				}
				catch
				{
				}
			}
			return aPath;
		}

		// --------------------------------------------------------------------
		// データベースのプロパティーを更新
		// --------------------------------------------------------------------
		public static void UpdateDbProperty(DataContext oContext)
		{
			Table<TProperty> aTableProperty = oContext.GetTable<TProperty>();

			// 古いプロパティーを削除
			IQueryable<TProperty> aDelTargets =
					from x in aTableProperty
					select x;
			aTableProperty.DeleteAllOnSubmit(aDelTargets);
			oContext.SubmitChanges();

			// 新しいプロパティーを挿入
			aTableProperty.InsertOnSubmit(new TProperty { Uid = 0, AppVer = NklCommon.APP_VER });
			oContext.SubmitChanges();
		}

		// --------------------------------------------------------------------
		// ユーザー CSV ファイルのフルパス
		// --------------------------------------------------------------------
		public static String UserCsvPath(String oFileName)
		{
			return Path.GetDirectoryName(Application.ExecutablePath) + "\\" + FOLDER_NAME_USER_CSVS + oFileName;
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		private const String FILE_NAME_HELP_PREFIX = APP_ID + "_JPN";
		private const String FOLDER_NAME_HELP_PARTS = "HelpParts\\";

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// TempFilePath() 用カウンター（同じスレッドでもファイル名が分かれるようにするため）
		private static Int32 smTempFilePathCounter = 0;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 環境設定の文字コードに従って CSV ファイルを読み込む
		// CSV のまま処理を行う場合向け
		// --------------------------------------------------------------------
		private static void LoadCsv(Dictionary<String, List<List<String>>> oCsvs, String oCsvPath, NicoKaraListerSettings oNicoKaraListerSettings, Int32 oNumColumns)
		{
			List<List<String>> aCsvContents;

			if (File.Exists(oCsvPath))
			{
				aCsvContents = LoadCsv(oCsvPath, oNicoKaraListerSettings, oNumColumns);
			}
			else
			{
				aCsvContents = new List<List<String>>();
			}

			// 格納
			oCsvs[oCsvPath] = aCsvContents;
		}

		// --------------------------------------------------------------------
		// 設定ファイルのルール表記を正規表現に変換
		// --------------------------------------------------------------------
		private static void MakeRegexPattern(String oRuleInDisk, out String oRuleInMemory, out List<String> oGroups)
		{
			oGroups = new List<String>();

			// 元が空なら空で返す
			if (String.IsNullOrEmpty(oRuleInDisk))
			{
				oRuleInMemory = String.Empty;
				return;
			}

			StringBuilder aSB = new StringBuilder();
			Int32 aBeginPos = 0;
			Int32 aEndPos;
			Boolean aLongestExists = false;
			while (aBeginPos < oRuleInDisk.Length)
			{
				if (oRuleInDisk[aBeginPos] == NklCommon.RULE_VAR_BEGIN[0])
				{
					// 変数を解析
					aEndPos = MakeRegexPatternFindVarEnd(oRuleInDisk, aBeginPos + 1);
					if (aEndPos < 0)
					{
						throw new Exception("命名規則の " + (aBeginPos + 1) + " 文字目の < に対応する > がありません。\n" + oRuleInDisk);
					}

					// 変数の <> は取り除く
					String aVarName = oRuleInDisk.Substring(aBeginPos + 1, aEndPos - aBeginPos - 1).ToLower();
					oGroups.Add(aVarName);

					// 番組名・楽曲名は区切り文字を含むこともあるため最長一致で検索する
					// また、最低 1 つは最長一致が無いとマッチしない
					if (aVarName == NklCommon.RULE_VAR_PROGRAM || aVarName == NklCommon.RULE_VAR_TITLE
							|| !aLongestExists && aEndPos == oRuleInDisk.Length - 1)
					{
						aSB.Append("(.*)");
						aLongestExists = true;
					}
					else
					{
						aSB.Append("(.*?)");
					}

					aBeginPos = aEndPos + 1;
				}
				else if (@".$^{[(|)*+?\".IndexOf(oRuleInDisk[aBeginPos]) >= 0)
				{
					// エスケープが必要な文字
					aSB.Append('\\');
					aSB.Append(oRuleInDisk[aBeginPos]);
					aBeginPos++;
				}
				else
				{
					// そのまま追加
					aSB.Append(oRuleInDisk[aBeginPos]);
					aBeginPos++;
				}
			}
			aSB.Append("$");
			oRuleInMemory = aSB.ToString();
		}

		// --------------------------------------------------------------------
		// <Title> 等の開始 < に対する終了 > の位置を返す
		// ＜引数＞ oBeginPos：開始 < の次の位置
		// --------------------------------------------------------------------
		private static Int32 MakeRegexPatternFindVarEnd(String oString, Int32 oBeginPos)
		{
			while (oBeginPos < oString.Length)
			{
				if (oString[oBeginPos] == NklCommon.RULE_VAR_END[0])
				{
					return oBeginPos;
				}
				oBeginPos++;
			}
			return -1;
		}



	}
	// public class NklCommon ___END___

}
// namespace NicoKaraLister.Shared ___END___
