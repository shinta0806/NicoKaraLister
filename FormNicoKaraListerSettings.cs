// ============================================================================
// 
// 環境設定を行うフォーム
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using NicoKaraLister.Shared;
using Shinta;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NicoKaraLister
{
	public partial class FormNicoKaraListerSettings : Form
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FormNicoKaraListerSettings(NicoKaraListerSettings oNicoKaraListerSettings, LogWriter oLogWriter)
		{
			InitializeComponent();

			// 変数初期化
			mNicoKaraListerSettings = oNicoKaraListerSettings;
			mLogWriter = oLogWriter;
		}

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// メッセージハンドラ
		// --------------------------------------------------------------------
		protected override void WndProc(ref Message oMsg)
		{
			if (oMsg.Msg == UpdaterLauncher.WM_UPDATER_UI_DISPLAYED)
			{
				WMUpdaterUIDisplayed();
			}
			base.WndProc(ref oMsg);
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// 環境設定
		private NicoKaraListerSettings mNicoKaraListerSettings;

		// ログ
		private LogWriter mLogWriter;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 入力された値が適正か確認
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void CheckInput()
		{
			// 現在の所チェックする項目無し
		}

		// --------------------------------------------------------------------
		// コンポーネントから設定に反映
		// --------------------------------------------------------------------
		private void ComposToSettings()
		{
			mNicoKaraListerSettings.CheckRss = CheckBoxCheckRss.Checked;
		}

		// --------------------------------------------------------------------
		// 楽曲情報・番組情報をエクスポート
		// --------------------------------------------------------------------
		private void ExportInfo()
		{
			try
			{
				// 準備
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲情報・番組情報をエクスポートします...");

				// テンポラリーフォルダーにコピー
				String aTempFolder = NklCommon.TempFilePath() + "\\" + NklCommon.FILE_PREFIX_INFO + "\\";
				Boolean aExists = false;
				Directory.CreateDirectory(aTempFolder);
				List<String> aCsvs = new List<String>();
				aCsvs.Add(NklCommon.FILE_NAME_PROGRAM_CSV);
				aCsvs.AddRange(NklCommon.CreateSongCsvList());
				aCsvs.Add(NklCommon.FILE_NAME_PROGRAM_ALIAS_CSV);
				aCsvs.AddRange(NklCommon.CreateSongAliasCsvList());
				foreach (String aCsv in aCsvs)
				{
					if (File.Exists(NklCommon.UserCsvPath(aCsv)))
					{
						File.Copy(NklCommon.UserCsvPath(aCsv), aTempFolder + aCsv);
						aExists = true;
					}
				}
				if (!aExists)
				{
					throw new Exception("エクスポートする楽曲情報・番組情報がありません。");
				}

				// 圧縮
				ZipFile.CreateFromDirectory(aTempFolder, SaveFileDialogExport.FileName, CompressionLevel.Optimal, true);

				// 報告
				ShowLogMessage(TraceEventType.Information, "エクスポートが完了しました。");
			}
			catch (OperationCanceledException)
			{
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "エクスポートを中止しました。");
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "エクスポート時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報・番組情報をインポート
		// --------------------------------------------------------------------
		private void ImportInfo()
		{
			try
			{
				// 準備
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲情報・番組情報をインポートします...");

				// 解凍
				String aTempFolder = NklCommon.TempFilePath() + "\\";
				Directory.CreateDirectory(aTempFolder);
				ZipFile.ExtractToDirectory(OpenFileDialogImport.FileName, aTempFolder);
				String aExtractedFolderPath = aTempFolder + NklCommon.FILE_PREFIX_INFO + "\\";

				// CSV 読み込み
				Dictionary<String, List<List<String>>> aProgramCsvs;
				Dictionary<String, List<List<String>>> aSongCsvs;
				Dictionary<String, List<List<String>>> aProgramAliasCsvs;
				Dictionary<String, List<List<String>>> aSongAliasCsvs;
				NklCommon.LoadCsvs(mNicoKaraListerSettings, out aProgramCsvs, out aSongCsvs, out aProgramAliasCsvs, out aSongAliasCsvs);

				// 番組インポート
				Int32 aNumAddedRecords = 0;
				ImportInfo<ProgramCsvColumns>(aProgramCsvs, NklCommon.FILE_NAME_PROGRAM_CSV, aExtractedFolderPath, (Int32)ProgramCsvColumns.Id, ref aNumAddedRecords);

				// 楽曲インポート
				List<String> aSongCsvList = NklCommon.CreateSongCsvList();
				foreach (String aCsvFileName in aSongCsvList)
				{
					ImportInfo<SongCsvColumns>(aSongCsvs, aCsvFileName, aExtractedFolderPath, (Int32)SongCsvColumns.Id, ref aNumAddedRecords);
				}

				// 番組別名インポート
				ImportInfo<ProgramAliasCsvColumns>(aProgramAliasCsvs, NklCommon.FILE_NAME_PROGRAM_ALIAS_CSV, aExtractedFolderPath, (Int32)ProgramAliasCsvColumns.Alias,
						ref aNumAddedRecords);

				// 楽曲別名インポート
				List<String> aSongAliasCsvList = NklCommon.CreateSongAliasCsvList();
				foreach (String aCsvFileName in aSongAliasCsvList)
				{
					ImportInfo<SongAliasCsvColumns>(aSongAliasCsvs, aCsvFileName, aExtractedFolderPath, (Int32)SongAliasCsvColumns.Alias, ref aNumAddedRecords);
				}

				// 報告
				ShowLogMessage(TraceEventType.Information, "インポートが完了しました。\n"
						+ "重複を除く " + aNumAddedRecords + " レコードをインポートしました。");
			}
			catch (OperationCanceledException)
			{
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "インポートを中止しました。");
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "インポート時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報・番組情報をインポート（1 ファイル分）
		// --------------------------------------------------------------------
		private void ImportInfo<T>(Dictionary<String, List<List<String>>> oCsvs, String oCsvFileName, String oExtractedFolderPath, Int32 oKeyColumnIndex,
				ref Int32 oTotalAddedRecords)
		{
			// 解凍された CSV ファイルがあるか確認
			String aExtractedCsvPath = oExtractedFolderPath + oCsvFileName;
			if (!File.Exists(aExtractedCsvPath))
			{
				return;
			}

			Int32 aNumAddedRecords = 0;
			List<String> aTitle = NklCommon.CsvTitle<T>();

			// CSV 読み込み
			ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, oCsvFileName + " をインポートしています...");
			List<List<String>> aCsv = NklCommon.LoadCsv(aExtractedCsvPath, mNicoKaraListerSettings, aTitle.Count + 1);

			// インポート
			String aUserCsvPath = NklCommon.UserCsvPath(oCsvFileName);
			foreach (List<String> aRecord in aCsv)
			{
				// ユーザー CSV に既にキー列が存在している場合はインポートしない
				String aFoundCsvPath;
				List<String> aFoundRecord = NklCommon.FindCsvRecord(oCsvs, oKeyColumnIndex, aRecord[oKeyColumnIndex], out aFoundCsvPath);
				if (aFoundRecord != null && !NklCommon.IsSystemCsvPath(aFoundCsvPath))
				{
					continue;
				}

				// システム CSV に既に同じレコードが存在している場合はインポートしない
				if (NklCommon.RecordExists(oCsvs, aRecord, oKeyColumnIndex))
				{
					continue;
				}

				// インポートする
				if (!oCsvs.ContainsKey(aUserCsvPath))
				{
					oCsvs[aUserCsvPath] = new List<List<String>>();
				}
				oCsvs[aUserCsvPath].Add(aRecord);
				aNumAddedRecords++;
			}

			// 保存
			if (aNumAddedRecords > 0)
			{
				NklCommon.BackupCsv(aUserCsvPath);
				CsvManager.SaveCsv(aUserCsvPath, oCsvs[aUserCsvPath], "\r\n", Encoding.UTF8, aTitle, true);

			}

			// 報告
			ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "重複を除く " + aNumAddedRecords + " レコードをインポートしました。");
			oTotalAddedRecords += aNumAddedRecords;
		}

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// タイトルバー
			Text = "環境設定";
#if DEBUG
			Text = "［デバッグ］" + Text;
#endif
		}

		// --------------------------------------------------------------------
		// 進捗系のコンポーネントをすべて元に戻す
		// --------------------------------------------------------------------
		private void MakeAllComposNormal()
		{
			ProgressBarCheckRss.Visible = false;
			ButtonCheckRss.Enabled = true;
		}

		// --------------------------------------------------------------------
		// 最新情報確認コンポーネントを進捗中にする
		// --------------------------------------------------------------------
		private void MakeLatestComposRunning()
		{
			ProgressBarCheckRss.Visible = true;
			ButtonCheckRss.Enabled = false;
		}

		// --------------------------------------------------------------------
		// 設定をコンポーネントに反映
		// --------------------------------------------------------------------
		private void SettingsToCompos()
		{
			CheckBoxCheckRss.Checked = mNicoKaraListerSettings.CheckRss;
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
		// ちょちょいと自動更新の画面が何かしら表示された
		// --------------------------------------------------------------------
		private void WMUpdaterUIDisplayed()
		{
			MakeAllComposNormal();
		}

		// ====================================================================
		// IDE 生成イベントハンドラー
		// ====================================================================

		private void ButtonCheckRss_Click(object sender, EventArgs e)
		{
			try
			{
				MakeLatestComposRunning();
				if (!NklCommon.LaunchUpdater(true, true, Handle, true, false))
				{
					MakeAllComposNormal();
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "最新情報確認時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormNicoKaraListerSettings_Load(object sender, EventArgs e)
		{
			try
			{
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "環境設定フォームを開きます。");
				Init();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "環境設定フォームロード時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormNicoKaraListerSettings_Shown(object sender, EventArgs e)
		{
			try
			{
				SettingsToCompos();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "環境設定フォーム表示時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormNicoKaraListerSettings_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "環境設定フォームを閉じました。");
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "環境設定フォームクローズ時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			try
			{
				CheckInput();
				ComposToSettings();
				mNicoKaraListerSettings.Save();
				DialogResult = DialogResult.OK;
			}
			catch (OperationCanceledException)
			{
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "環境設定変更を中止しました。");
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "OK ボタンクリック時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSaveLog_Click(object sender, EventArgs e)
		{
			try
			{
				SaveFileDialogLog.FileName = "NicoKaraListerLog_" + DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss");
				if (SaveFileDialogLog.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				// 環境情報保存
				NklCommon.LogEnvironmentInfo();

				ZipFile.CreateFromDirectory(NklCommon.SettingsPath(), SaveFileDialogLog.FileName, CompressionLevel.Optimal, true);
				ShowLogMessage(TraceEventType.Information, "ログ保存完了：\n" + SaveFileDialogLog.FileName);
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "ログ保存時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonExportInfo_Click(object sender, EventArgs e)
		{
			try
			{
				SaveFileDialogExport.FileName = NklCommon.FILE_PREFIX_INFO + "_" + DateTime.Now.ToString("yyyy_MM_dd") + NklCommon.FILE_EXT_NKLINFO;
				if (SaveFileDialogExport.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				ExportInfo();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "エクスポートボタンクリック時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonImportInfo_Click(object sender, EventArgs e)
		{
			try
			{
				if (OpenFileDialogImport.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				ImportInfo();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "インポートボタンクリック時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void CheckBoxCheckRss_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				if (CheckBoxCheckRss.Checked)
				{
					return;
				}
				if (MessageBox.Show("最新情報・更新版の確認を無効にすると、" + NklCommon.APP_NAME_J
						+ "の新版がリリースされても自動的にインストールされず、古いバージョンを使い続けることになります。\n"
						+ "本当に無効にしてもよろしいですか？", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
						!= DialogResult.Yes)
				{
					CheckBoxCheckRss.Checked = true;
				}
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "更新有効無効切替時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
}
