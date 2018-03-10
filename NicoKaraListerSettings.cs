// ============================================================================
// 
// ニコカラりすたーの設定を管理
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;

namespace NicoKaraLister.Shared
{
	// 設定の保存場所を Application.UserAppDataPath 配下にする
	[SettingsProvider(typeof(ApplicationNameSettingsProvider))]
	public class NicoKaraListerSettings : ApplicationSettingsBase
	{
		// ====================================================================
		// public プロパティ
		// ====================================================================

		// --------------------------------------------------------------------
		// 設定
		// --------------------------------------------------------------------

		// CSV 読み込み時の文字コード（書き込みは常に UTF-8）
		private const String KEY_NAME_CSV_ENCODING = "CsvEncoding";
		[UserScopedSetting]
		public CsvEncoding CsvEncoding
		{
			get
			{
				return (CsvEncoding)this[KEY_NAME_CSV_ENCODING];
			}
			set
			{
				this[KEY_NAME_CSV_ENCODING] = value;
			}
		}

		// 楽曲 ID・番組 ID の接頭辞
		private const String KEY_NAME_ID_PREFIX = "IdPrefix";
		[UserScopedSetting]
		public String IdPrefix
		{
			get
			{
				return (String)this[KEY_NAME_ID_PREFIX];
			}
			set
			{
				this[KEY_NAME_ID_PREFIX] = value;
			}
		}

		// --------------------------------------------------------------------
		// メンテナンス
		// --------------------------------------------------------------------

		// 新着情報を確認するかどうか
		private const String KEY_NAME_CHECK_RSS = "CheckRSS";
		[UserScopedSetting]
		[DefaultSettingValue(Common.BOOLEAN_STRING_TRUE)]
		public Boolean CheckRss
		{
			get
			{
				return (Boolean)this[KEY_NAME_CHECK_RSS];
			}
			set
			{
				this[KEY_NAME_CHECK_RSS] = value;
			}
		}

		// --------------------------------------------------------------------
		// 終了時の状態
		// --------------------------------------------------------------------

		// リスト化対象フォルダー
		private const String KEY_NAME_PARENT_FOLDER = "ParentFolder";
		[UserScopedSetting]
		[DefaultSettingValue("")]
		public String ParentFolder
		{
			get
			{
				return (String)this[KEY_NAME_PARENT_FOLDER];
			}
			set
			{
				this[KEY_NAME_PARENT_FOLDER] = value;
			}
		}

		// リスト化対象フォルダーの履歴：[0] が最新履歴
		private const String KEY_NAME_PARENT_FOLDER_HISTORY = "ParentFolderHistory";
		[UserScopedSetting]
		public List<String> ParentFolderHistory
		{
			get
			{
				return (List<String>)this[KEY_NAME_PARENT_FOLDER_HISTORY];
			}
			set
			{
				this[KEY_NAME_PARENT_FOLDER_HISTORY] = value;
			}
		}

		// リスト出力先フォルダー
		private const String KEY_NAME_OUTPUT_FOLDER = "OutputFolder";
		[UserScopedSetting]
		[DefaultSettingValue("")]
		public String OutputFolder
		{
			get
			{
				return (String)this[KEY_NAME_OUTPUT_FOLDER];
			}
			set
			{
				this[KEY_NAME_OUTPUT_FOLDER] = value;
			}
		}

		// リスト出力先フォルダーの履歴：[0] が最新履歴
		private const String KEY_NAME_OUTPUT_FOLDER_HISTORY = "OutputFolderHistory";
		[UserScopedSetting]
		public List<String> OutputFolderHistory
		{
			get
			{
				return (List<String>)this[KEY_NAME_OUTPUT_FOLDER_HISTORY];
			}
			set
			{
				this[KEY_NAME_OUTPUT_FOLDER_HISTORY] = value;
			}
		}

		// リスト出力形式
		private const String KEY_NAME_OUTPUT_FORMAT = "OutputFormat";
		[UserScopedSetting]
		[DefaultSettingValue("0")]
		public Int32 OutputFormat
		{
			get
			{
				return (Int32)this[KEY_NAME_OUTPUT_FORMAT];
			}
			set
			{
				this[KEY_NAME_OUTPUT_FORMAT] = value;
			}
		}

		// 前回発行した楽曲 ID（次回はインクリメントした番号で発行する）
		private const String KEY_NAME_LAST_SONG_ID_NUMBER = "LastSongIdNumber";
		[UserScopedSetting]
		public Int32 LastSongIdNumber
		{
			get
			{
				return (Int32)this[KEY_NAME_LAST_SONG_ID_NUMBER];
			}
			set
			{
				this[KEY_NAME_LAST_SONG_ID_NUMBER] = value;
			}
		}

		// 前回発行した番組 ID（次回はインクリメントした番号で発行する）
		private const String KEY_NAME_LAST_PROGRAM_ID_NUMBER = "LastProgramIdNumber";
		[UserScopedSetting]
		public Int32 LastProgramIdNumber
		{
			get
			{
				return (Int32)this[KEY_NAME_LAST_PROGRAM_ID_NUMBER];
			}
			set
			{
				this[KEY_NAME_LAST_PROGRAM_ID_NUMBER] = value;
			}
		}

		// 前回起動時のバージョン
		private const String KEY_NAME_PREV_LAUNCH_VER = "PrevLaunchVer";
		[UserScopedSetting]
		[DefaultSettingValue("")]
		public String PrevLaunchVer
		{
			get
			{
				return (String)this[KEY_NAME_PREV_LAUNCH_VER];
			}
			set
			{
				this[KEY_NAME_PREV_LAUNCH_VER] = value;
			}
		}

		// 前回起動時のパス
		private const String KEY_NAME_PREV_LAUNCH_PATH = "PrevLaunchPath";
		[UserScopedSetting]
		[DefaultSettingValue("")]
		public String PrevLaunchPath
		{
			get
			{
				return (String)this[KEY_NAME_PREV_LAUNCH_PATH];
			}
			set
			{
				this[KEY_NAME_PREV_LAUNCH_PATH] = value;
			}
		}

		// ウィンドウ位置
		private const String KEY_NAME_DESKTOP_BOUNDS = "DesktopBounds";
		[UserScopedSetting]
		[DefaultSettingValue("")]
		public Rectangle DesktopBounds
		{
			get
			{
				return (Rectangle)this[KEY_NAME_DESKTOP_BOUNDS];
			}
			set
			{
				this[KEY_NAME_DESKTOP_BOUNDS] = value;
			}
		}

		// 新着情報を確認した日付
		private const String KEY_NAME_RSS_CHECK_DATE = "RSSCheckDate";
		[UserScopedSetting]
		[DefaultSettingValue("")]
		public DateTime RssCheckDate
		{
			get
			{
				return (DateTime)this[KEY_NAME_RSS_CHECK_DATE];
			}
			set
			{
				this[KEY_NAME_RSS_CHECK_DATE] = value;
			}
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 簡易コピー
		// --------------------------------------------------------------------
		public NicoKaraListerSettings Clone()
		{
			return (NicoKaraListerSettings)MemberwiseClone();
		}

		// --------------------------------------------------------------------
		// RSS の確認が必要かどうか
		// --------------------------------------------------------------------
		public Boolean IsCheckRssNeeded()
		{
			if (!CheckRss)
			{
				return false;
			}
			DateTime aEmptyDate = new DateTime();
			TimeSpan aDay3 = new TimeSpan(3, 0, 0, 0);
			return RssCheckDate == aEmptyDate || DateTime.Now.Date - RssCheckDate >= aDay3;
		}

	}
	// public class NicoKaraListerSettings ___END___

}
// namespace NicoKaraLister.Shared ___END___


