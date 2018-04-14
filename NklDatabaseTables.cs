// ============================================================================
// 
// データベーステーブル定義をカプセル化
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Data.Linq.Mapping;

namespace NicoKaraLister.Shared
{
	// ====================================================================
	// データベースプロパティーテーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_PROPERTY)]
	public class TProperty
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_PROPERTY = "t_property";
		public const String FIELD_NAME_PROPERTY_UID = "property_uid";
		public const String FIELD_NAME_PROPERTY_APP_VER = "property_app_ver";
		public const String FIELD_NAME_PROPERTY_LAST_UPDATE = "property_uid";

		// ====================================================================
		// フィールド
		// ====================================================================

		// ユニーク ID
		[Column(Name = FIELD_NAME_PROPERTY_UID, DbType = LinqUtils.DB_TYPE_INT32, CanBeNull = false, IsPrimaryKey = true)]
		public Int32 Uid { get; set; }

		// データベース更新時のアプリケーションのバージョン
		[Column(Name = FIELD_NAME_PROPERTY_APP_VER, DbType = LinqUtils.DB_TYPE_STRING)]
		public String AppVer { get; set; }
	}
	// public class TProperty ___END___

	// ====================================================================
	// 番組マスターテーブル
	// anison.info program.csv の内容を格納
	// ====================================================================

	[Table(Name = TABLE_NAME_PROGRAM)]
	public class TProgram
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_PROGRAM = "t_program";
		public const String FIELD_NAME_PROGRAM_UID = "program_uid";
		public const String FIELD_NAME_PROGRAM_ID = "program_id";
		public const String FIELD_NAME_PROGRAM_CATEGORY = "program_category";
		public const String FIELD_NAME_PROGRAM_GAME_CATEGORY = "program_game_category";
		public const String FIELD_NAME_PROGRAM_NAME = "program_name";
		public const String FIELD_NAME_PROGRAM_RUBY = "program_ruby";
		public const String FIELD_NAME_PROGRAM_SUB_NAME = "program_sub_name";
		public const String FIELD_NAME_PROGRAM_SUB_RUBY = "program_sub_ruby";
		public const String FIELD_NAME_PROGRAM_NUM_STORIES = "program_num_stories";
		public const String FIELD_NAME_PROGRAM_AGE_LIMIT = "program_age_limit";
		public const String FIELD_NAME_PROGRAM_BEGIN_DATE = "program_begin_date";

		// ====================================================================
		// フィールド
		// ====================================================================

		// ユニーク ID（CSV には無いフィールド）
		[Column(Name = FIELD_NAME_PROGRAM_UID, DbType = LinqUtils.DB_TYPE_INT32, CanBeNull = false, IsPrimaryKey = true)]
		public Int32 Uid { get; set; }

		// 番組 ID（anison.info では重複していることがあるが、DB に格納する際にユニークにする）
		[Column(Name = FIELD_NAME_PROGRAM_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String Id { get; set; }

		// 番組分類
		[Column(Name = FIELD_NAME_PROGRAM_CATEGORY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Category { get; set; }

		// ゲーム種別
		[Column(Name = FIELD_NAME_PROGRAM_GAME_CATEGORY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String GameCategory { get; set; }

		// 番組名
		[Column(Name = FIELD_NAME_PROGRAM_NAME, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Name { get; set; }

		// バングミメイ
		[Column(Name = FIELD_NAME_PROGRAM_RUBY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Ruby { get; set; }

		// 番組名補
		[Column(Name = FIELD_NAME_PROGRAM_SUB_NAME, DbType = LinqUtils.DB_TYPE_STRING)]
		public String SubName { get; set; }

		// バングミメイホ
		[Column(Name = FIELD_NAME_PROGRAM_SUB_RUBY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String SubRuby { get; set; }

		// 放映話数
		[Column(Name = FIELD_NAME_PROGRAM_NUM_STORIES, DbType = LinqUtils.DB_TYPE_STRING)]
		public String NumStories { get; set; }

		// 年齢制限
		[Column(Name = FIELD_NAME_PROGRAM_AGE_LIMIT, DbType = LinqUtils.DB_TYPE_STRING)]
		public String AgeLimit { get; set; }

		// 放映開始日（修正じゃないユリウス日）
		[Column(Name = FIELD_NAME_PROGRAM_BEGIN_DATE, DbType = LinqUtils.DB_TYPE_DOUBLE)]
		public Double BeginDate { get; set; }

	}
	// public class TProgram ___END___

	// ====================================================================
	// 楽曲マスターテーブル
	// anison.info anison.csv 等の内容を格納
	// program.csv の番組分類と anison.csv の番組分類は異なっている模様
	// ====================================================================

	[Table(Name = TABLE_NAME_SONG)]
	public class TSong
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_SONG = "t_song";
		public const String FIELD_NAME_SONG_UID = "song_uid";
		public const String FIELD_NAME_SONG_CATEGORY = "song_category";
		public const String FIELD_NAME_SONG_OP_ED = "song_op_ed";
		public const String FIELD_NAME_SONG_CAST_SEQ = "song_cast_seq";
		public const String FIELD_NAME_SONG_ID = "song_id";
		public const String FIELD_NAME_SONG_NAME = "song_name";
		public const String FIELD_NAME_SONG_ARTIST = "song_artist";

		// ====================================================================
		// フィールド
		// ====================================================================

		// ユニーク ID（CSV には無いフィールド）
		[Column(Name = FIELD_NAME_SONG_UID, DbType = LinqUtils.DB_TYPE_INT32, CanBeNull = false, IsPrimaryKey = true)]
		public Int32 Uid { get; set; }

		// 番組 ID
		[Column(Name = TProgram.FIELD_NAME_PROGRAM_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String ProgramId { get; set; }

		// 番組分類
		[Column(Name = FIELD_NAME_SONG_CATEGORY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Category { get; set; }

		// 番組名
		[Column(Name = TProgram.FIELD_NAME_PROGRAM_NAME, DbType = LinqUtils.DB_TYPE_STRING)]
		public String ProgramName { get; set; }

		// 適用
		[Column(Name = FIELD_NAME_SONG_OP_ED, DbType = LinqUtils.DB_TYPE_STRING)]
		public String OpEd { get; set; }

		// 放映順
		[Column(Name = FIELD_NAME_SONG_CAST_SEQ, DbType = LinqUtils.DB_TYPE_STRING)]
		public String CastSeq { get; set; }

		// 楽曲 ID（anison.info では重複していることがあるが、DB に格納する際にユニークにする）
		[Column(Name = FIELD_NAME_SONG_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String Id { get; set; }

		// 楽曲名
		[Column(Name = FIELD_NAME_SONG_NAME, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Name { get; set; }

		// 歌手名
		[Column(Name = FIELD_NAME_SONG_ARTIST, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Artist { get; set; }

	}
	// public class TSong ___END___

	// ====================================================================
	// 番組別名テーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_PROGRAM_ALIAS)]
	public class TProgramAlias
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_PROGRAM_ALIAS = "t_program_alias";
		public const String FIELD_NAME_PROGRAM_ALIAS_UID = "program_alias_uid";
		public const String FIELD_NAME_PROGRAM_ALIAS_ALIAS = "program_alias_alias";

		// ====================================================================
		// フィールド
		// ====================================================================

		// ユニーク ID（CSV には無いフィールド）
		[Column(Name = FIELD_NAME_PROGRAM_ALIAS_UID, DbType = LinqUtils.DB_TYPE_INT32, CanBeNull = false, IsPrimaryKey = true)]
		public Int32 Uid { get; set; }

		// 番組 ID
		[Column(Name = TProgram.FIELD_NAME_PROGRAM_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String ProgramId { get; set; }

		// 別名
		[Column(Name = FIELD_NAME_PROGRAM_ALIAS_ALIAS, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Alias { get; set; }
	}
	// public class TProgramAlias ___END___

	// ====================================================================
	// 楽曲別名テーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_SONG_ALIAS)]
	public class TSongAlias
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_SONG_ALIAS = "t_song_alias";
		public const String FIELD_NAME_SONG_ALIAS_UID = "song_alias_uid";
		public const String FIELD_NAME_SONG_ALIAS_ALIAS = "song_alias_alias";

		// ====================================================================
		// フィールド
		// ====================================================================

		// ユニーク ID（CSV には無いフィールド）
		[Column(Name = FIELD_NAME_SONG_ALIAS_UID, DbType = LinqUtils.DB_TYPE_INT32, CanBeNull = false, IsPrimaryKey = true)]
		public Int32 Uid { get; set; }

		// 楽曲 ID
		[Column(Name = TSong.FIELD_NAME_SONG_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String SongId { get; set; }

		// 別名
		[Column(Name = FIELD_NAME_SONG_ALIAS_ALIAS, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Alias { get; set; }
	}
	// public class TSongAlias ___END___

	// ====================================================================
	// 検出ファイルリストテーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_FOUND)]
	public class TFound
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_FOUND = "t_found";
		public const String FIELD_NAME_FOUND_UID = "found_uid";
		public const String FIELD_NAME_FOUND_PATH = "found_path";
		public const String FIELD_NAME_FOUND_HEAD = "found_head";
		public const String FIELD_NAME_FOUND_TITLE_RUBY = "found_title_ruby";
		public const String FIELD_NAME_FOUND_WORKER = "found_worker";
		public const String FIELD_NAME_FOUND_TRACK = "found_track";
		public const String FIELD_NAME_FOUND_SMART_TRACK_ON = "found_smart_track_on";
		public const String FIELD_NAME_FOUND_SMART_TRACK_OFF = "found_smart_track_off";
		public const String FIELD_NAME_FOUND_COMMENT = "found_comment";
		public const String FIELD_NAME_FOUND_LAST_WRITE_TIME = "found_last_write_time";
		public const String FIELD_NAME_FOUND_FILE_SIZE = "found_file_size";

		// ====================================================================
		// フィールド
		// ====================================================================

		// --------------------------------------------------------------------
		// TFound 独自
		// --------------------------------------------------------------------

		// ユニーク ID
		[Column(Name = FIELD_NAME_FOUND_UID, DbType = LinqUtils.DB_TYPE_INT32, CanBeNull = false, IsPrimaryKey = true)]
		public Int32 Uid { get; set; }

		// フルパス
		[Column(Name = FIELD_NAME_FOUND_PATH, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String Path { get; set; }

		// 頭文字（通常は番組名の頭文字、通常はひらがな（濁点なし））
		[Column(Name = FIELD_NAME_FOUND_HEAD, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Head { get; set; }

		// ガッキョクメイ
		[Column(Name = FIELD_NAME_FOUND_TITLE_RUBY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String SongRuby { get; set; }

		// ニコカラ制作者
		[Column(Name = FIELD_NAME_FOUND_WORKER, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Worker { get; set; }

		// トラック情報
		[Column(Name = FIELD_NAME_FOUND_TRACK, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Track { get; set; }

		// スマートトラック：オンボーカル（有なら NklCommon.RULE_VALUE_VOCAL_DEFAULT）
		[Column(Name = FIELD_NAME_FOUND_SMART_TRACK_ON, DbType = LinqUtils.DB_TYPE_INT32)]
		public Int32 SmartTrackOnVocal { get; set; }

		// スマートトラック：オフボーカル（有なら NklCommon.RULE_VALUE_VOCAL_DEFAULT）
		[Column(Name = FIELD_NAME_FOUND_SMART_TRACK_OFF, DbType = LinqUtils.DB_TYPE_INT32)]
		public Int32 SmartTrackOffVocal { get; set; }

		// 備考
		[Column(Name = FIELD_NAME_FOUND_COMMENT, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Comment { get; set; }

		// 最終更新日時（修正じゃないユリウス日）
		[Column(Name = FIELD_NAME_FOUND_LAST_WRITE_TIME, DbType = LinqUtils.DB_TYPE_DOUBLE)]
		public Double LastWriteTime { get; set; }

		// ファイルサイズ
		[Column(Name = FIELD_NAME_FOUND_FILE_SIZE, DbType = LinqUtils.DB_TYPE_INT32)]
		public Int64 FileSize { get; set; }

		// --------------------------------------------------------------------
		// TProgram
		// --------------------------------------------------------------------

		// 番組分類
		[Column(Name = TProgram.FIELD_NAME_PROGRAM_CATEGORY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String ProgramCategory { get; set; }

		// ゲーム種別
		[Column(Name = TProgram.FIELD_NAME_PROGRAM_GAME_CATEGORY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String ProgramGameCategory { get; set; }

		// 番組名
		[Column(Name = TProgram.FIELD_NAME_PROGRAM_NAME, DbType = LinqUtils.DB_TYPE_STRING)]
		public String ProgramName { get; set; }

		// バングミメイ
		[Column(Name = TProgram.FIELD_NAME_PROGRAM_RUBY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String ProgramRuby { get; set; }

		// 番組名補
		[Column(Name = TProgram.FIELD_NAME_PROGRAM_SUB_NAME, DbType = LinqUtils.DB_TYPE_STRING)]
		public String ProgramSubName { get; set; }

		// バングミメイホ
		[Column(Name = TProgram.FIELD_NAME_PROGRAM_SUB_RUBY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String ProgramSubRuby { get; set; }

		// 放映話数
		[Column(Name = TProgram.FIELD_NAME_PROGRAM_NUM_STORIES, DbType = LinqUtils.DB_TYPE_STRING)]
		public String ProgramNumStories { get; set; }

		// 年齢制限
		[Column(Name = TProgram.FIELD_NAME_PROGRAM_AGE_LIMIT, DbType = LinqUtils.DB_TYPE_STRING)]
		public String ProgramAgeLimit { get; set; }

		// 放映開始日
		[Column(Name = TProgram.FIELD_NAME_PROGRAM_BEGIN_DATE, DbType = LinqUtils.DB_TYPE_DOUBLE)]
		public Double ProgramBeginDate { get; set; }

		// --------------------------------------------------------------------
		// TSong
		// --------------------------------------------------------------------

		// 番組分類
		[Column(Name = TSong.FIELD_NAME_SONG_CATEGORY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String SongCategory { get; set; }

		// 摘要
		[Column(Name = TSong.FIELD_NAME_SONG_OP_ED, DbType = LinqUtils.DB_TYPE_STRING)]
		public String SongOpEd { get; set; }

		// 放映順
		[Column(Name = TSong.FIELD_NAME_SONG_CAST_SEQ, DbType = LinqUtils.DB_TYPE_STRING)]
		public String SongCastSeq { get; set; }

		// 楽曲名
		[Column(Name = TSong.FIELD_NAME_SONG_NAME, DbType = LinqUtils.DB_TYPE_STRING)]
		public String SongName { get; set; }

		// 歌手名
		[Column(Name = TSong.FIELD_NAME_SONG_ARTIST, DbType = LinqUtils.DB_TYPE_STRING)]
		public String SongArtist { get; set; }

	}
	// public class TFound ___END___
}
// namespace NicoKaraLister.Shared ___END___
