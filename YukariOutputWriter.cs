// ============================================================================
// 
// ゆかり用リスト出力クラス
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NicoKaraLister.Shared
{
	// ====================================================================
	// ゆかり用リスト出力クラス
	// ====================================================================

	public class YukariOutputWriter : WebOutputWriter
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public YukariOutputWriter()
		{
			// プロパティー
			FormatName = "ゆかり用リスト";
			TopFileName = "index" + Common.FILE_EXT_PHP;
			OutputSettings = new YukariOutputSettings();

			// メンバー変数
			String aListLinkArg = "<?php empty($yukarisearchlink) ? print \"\" : print \"?yukarihost=\".$yukarihost;?>";

			mListExt = Common.FILE_EXT_PHP;
			mDirectoryTopName = "ゆかり検索 &gt; " + NklCommon.APP_NAME_J + "一覧";
			mDirectoryTopLink = "<a href=\"/search.php" + aListLinkArg + "\">ゆかり検索</a> &gt; <a href=\"" + TopFileName + aListLinkArg + "\">"
					+ NklCommon.APP_NAME_J + "一覧</a>";
			mAdditionalDescription = "ファイル名をクリックすると、ゆかりでリクエストできます。<br>";
			mAdditionalHeader = "<?php\n"
					+ "$yukarisearchlink = '';\n"
					+ "if (array_key_exists('yukarihost', $_REQUEST)) {\n"
					+ "    $yukarihost = $_REQUEST['yukarihost'];\n"
					+ "    $yukarisearchlink = 'http://'.$yukarihost.'/search.php?searchword=';\n"
					+ "}\n"
					+ "?>\n";
			mListLinkArg = aListLinkArg;
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// リスト出力
		// --------------------------------------------------------------------
		public override void Output()
		{
			base.Output();

			// ゆかり検索用データベース作成
			NklCommon.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "ゆかり検索用データベース作成中...");
			using (SQLiteConnection aConnection = NklCommon.CreateDbConnection(YukariDbPath()))
			{
				NklCommon.CreateFoundDbTables(aConnection);

				using (DataContext aContext = new DataContext(aConnection))
				{
					Table<TFound> aTable = aContext.GetTable<TFound>();

					IQueryable<TFound> aQueryResult =
							from x in TableFound
							select x;
					aTable.InsertAllOnSubmit(aQueryResult);
					aContext.SubmitChanges();
				}
			}
		}

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// リストに出力するファイル名の表現
		// ファイル名エスケープに関する備忘
		//   PHP print "" の中
		//     \ と " はファイル名として使われないので気にしなくて良い
		//     ' は "" の中であればエスケープ不要
		//     →従ってエスケープ不要
		//   HTML href "" の中
		//     \ < > はファイル名として使われない
		//     & ' 半角スペースがあっても動作する
		//     →従ってエスケープしなくても動作するようだが、UrlEncode() するほうが作法が良いのでしておく
		// --------------------------------------------------------------------
		protected override String FileNameDescription(String oFileName)
		{
			if (String.IsNullOrEmpty(oFileName))
			{
				return null;
			}

			return "<?php empty($yukarisearchlink) ? print \"" + oFileName + "\" : print \"<a href=\\\"\".$yukarisearchlink.\"" + HttpUtility.UrlEncode(oFileName)
					+ "\\\">" + oFileName + "</a>\";?>";
		}

		// --------------------------------------------------------------------
		// コンストラクターでは行えない準備などを実施
		// --------------------------------------------------------------------
		protected override void PrepareOutput()
		{
			base.PrepareOutput();

			// 古いゆかり用データベースを削除
			NklCommon.DeleteOldDb(YukariDbPath());
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// ゆかり検索用データベースファイル名
		private const String FILE_NAME_YUKARI_DB = "List" + Common.FILE_EXT_SQLITE3;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ゆかり検索用データベースのフルパス
		// --------------------------------------------------------------------
		private String YukariDbPath()
		{
			return FolderPath + FILE_NAME_YUKARI_DB;
		}

	}
	// public class YukariOutputWriter ___END___

}
// namespace NicoKaraLister.Shared ___END___
