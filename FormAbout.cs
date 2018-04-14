// ============================================================================
// 
// バージョン情報フォーム
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using NicoKaraLister.Shared;
using Shinta;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace NicoKaraLister
{
	public partial class FormAbout : Form
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FormAbout(LogWriter oLogWriter)
		{
			InitializeComponent();

			mLogWriter = oLogWriter;
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// ログ
		private LogWriter mLogWriter;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// LinkLabel のクリックを集約
		// --------------------------------------------------------------------
		private void LinkLabels_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			string aLink = String.Empty;

			try
			{
				// MSDN を見ると e.Link.LinkData がリンク先のように読めなくもないが、実際には
				// 値が入っていないので sender をキャストしてリンク先を取得する
				e.Link.Visited = true;
				aLink = ((LinkLabel)sender).Text;
				Process.Start(aLink);
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "リンク先を表示できませんでした。\n" + aLink);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
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

		// ====================================================================
		// IDE 生成イベントハンドラー
		// ====================================================================

		private void FormAbout_Load(object sender, EventArgs e)
		{
			try
			{
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "バージョン情報フォームを開きます。");

				// 表示
				Text = NklCommon.APP_NAME_J + "のバージョン情報";
#if DEBUG
				Text = "［デバッグ］" + Text;
#endif
				LabelAppName.Text = NklCommon.APP_NAME_J;
				LabelAppVer.Text = NklCommon.APP_VER;
				LabelCopyright.Text = NklCommon.COPYRIGHT_J;

				// コントロール
				ActiveControl = ButtonOK;

				Common.CascadeForm(this);
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "バージョン情報フォームロード時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormAbout_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "バージョン情報フォームを閉じました。");
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "バージョン情報フォームクローズ時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
	// public partial class FormAbout ___END___

}
// namespace NicoKaraLister ___END___
