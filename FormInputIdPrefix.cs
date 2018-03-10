// ============================================================================
// 
// ID 接頭辞の入力を行うフォーム
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace NicoKaraLister
{
	public partial class FormInputIdPrefix : Form
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FormInputIdPrefix(LogWriter oLogWriter)
		{
			InitializeComponent();

			// 初期化
			mLogWriter = oLogWriter;
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// ID 接頭辞
		public String IdPrefix { get; set; }

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// ログ
		private LogWriter mLogWriter;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// タイトルバー
#if DEBUG
			Text = "［デバッグ］" + Text;
#endif
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

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			try
			{
				if (String.IsNullOrEmpty(TextBoxIdPrefix.Text))
				{
					ShowLogMessage(TraceEventType.Error, "ID 接頭辞を入力して下さい。");
					return;
				}
				if (TextBoxIdPrefix.Text.IndexOf('-') >= 0)
				{
					ShowLogMessage(TraceEventType.Error, "ID 接頭辞に \"_\" は使えません。");
					return;
				}

				IdPrefix = TextBoxIdPrefix.Text;
				DialogResult = DialogResult.OK;
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "ID 接頭辞決定時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormInputIdPrefix_Load(object sender, EventArgs e)
		{
			try
			{
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "ID 接頭辞入力フォームを開きます。");
				Init();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "ID 接頭辞入力フォームロード時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormInputIdPrefix_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "ID 接頭辞入力フォームを閉じました。");
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "ID 接頭辞入力フォームクローズ時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
	// public partial class FormInputIdPrefix ___END___

}
// namespace NicoKaraLister ___END___
