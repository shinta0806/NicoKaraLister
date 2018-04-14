// ============================================================================
// 
// 楽曲情報・番組情報を検索するフォーム
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
using System.Windows.Forms;

namespace NicoKaraLister
{
	public partial class FormSearchInfo : Form
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FormSearchInfo(String oItemName, Dictionary<String, List<List<String>>> oCsvs, Int32 oColumnIndex, String oKeyword, LogWriter oLogWriter)
		{
			InitializeComponent();

			// 初期化
			mItemName = oItemName;
			mCsvs = oCsvs;
			mColumnIndex = oColumnIndex;
			mLogWriter = oLogWriter;

			// コンポーネント
			TextBoxKeyword.Text = oKeyword;
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 選択された情報
		public String SelectedInfo { get; set; }

		// ====================================================================
		// prvate メンバー変数
		// ====================================================================

		// 検索項目名
		private String mItemName;

		// 検索元データ
		private Dictionary<String, List<List<String>>> mCsvs;

		// 検索項目が格納されている CSV の列
		private Int32 mColumnIndex;

		// ログ
		private LogWriter mLogWriter;

		// ====================================================================
		// prvate メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// タイトルバー
			Text = mItemName + "を検索";
#if DEBUG
			Text = "［デバッグ］" + Text;
#endif

			// 説明
			LabelDescription.Text = mItemName + "を、既に登録されている情報から検索します。";

			Common.CascadeForm(this);
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
		// 選択ボタンの状態を更新
		// --------------------------------------------------------------------
		private void UpdateButtonSelect()
		{
			ButtonSelect.Enabled = ListBoxFounds.SelectedIndex >= 0;
		}

		// ====================================================================
		// IDE 生成イベントハンドラー
		// ====================================================================

		private void FormSearchOrigin_Load(object sender, EventArgs e)
		{
			try
			{
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "情報検索フォームを開きます。");
				Init();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "情報検索フォームロード時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormSearchOrigin_Shown(object sender, EventArgs e)
		{
			try
			{
				UpdateButtonSelect();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "情報検索フォーム表示時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSearch_Click(object sender, EventArgs e)
		{
			try
			{
				ListBoxFounds.Items.Clear();

				// 検索
				List<List<String>> aRecords = NklCommon.FindCsvRecordsIncludes(mCsvs, mColumnIndex, TextBoxKeyword.Text);
				if (aRecords.Count == 0)
				{
					ShowLogMessage(TraceEventType.Error, "「" + TextBoxKeyword.Text + "」を含む" + mItemName + "はありません。");
					return;
				}

				// 重複を除外
				List<String> aHits = new List<String>();
				for (Int32 i = 0; i < aRecords.Count; i++)
				{
					String aName = aRecords[i][mColumnIndex];
					if (!String.IsNullOrEmpty(aName) && !aHits.Contains(aName))
					{
						aHits.Add(aName);
					}
				}
				aHits.Sort();

				// リストボックスに表示
				ListBoxFounds.Items.AddRange(aHits.ToArray());
				ListBoxFounds.Focus();
				ListBoxFounds.SelectedIndex = 0;
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "情報検索時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ListBoxFounds_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateButtonSelect();
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "情報検索フォームリスト選択時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSelect_Click(object sender, EventArgs e)
		{
			try
			{
				SelectedInfo = (String)ListBoxFounds.Items[ListBoxFounds.SelectedIndex];
				DialogResult = DialogResult.OK;
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "情報検索フォーム選択決定時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TextBoxKeyword_KeyDown(object sender, KeyEventArgs e)
		{
			try
			{
				if (e.KeyCode == Keys.Enter)
				{
					ButtonSearch.PerformClick();
				}
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "情報検索フォーム検索キーワード入力時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TextBoxKeyword_Enter(object sender, EventArgs e)
		{
			try
			{
				// テキストボックスがエンターキーを検出できるようにする
				AcceptButton = null;
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "情報検索フォーム検索キーワードフォーカス時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TextBoxKeyword_Leave(object sender, EventArgs e)
		{
			try
			{
				AcceptButton = ButtonSelect;
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "情報検索フォーム検索キーワードフォーカス解除時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormSearchOrigin_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "情報検索フォームを閉じました。");
			}
			catch (Exception oExcep)
			{
				ShowLogMessage(TraceEventType.Error, "情報検索フォームクローズ時エラー：\n" + oExcep.Message);
				ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
	// public partial class FormSearchInfo ___END___

}
// namespace NicoKaraLister ___END___
