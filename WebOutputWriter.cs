// ============================================================================
// 
// HTML / PHP リスト出力用基底クラス
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NicoKaraLister.Shared
{
	// ====================================================================
	// HTML / PHP リスト出力用基底クラス
	// ====================================================================

	public abstract class WebOutputWriter : OutputWriter
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public WebOutputWriter()
		{
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 設定画面に入力された値が適正か確認
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		public override void CheckInput()
		{
			base.CheckInput();

			// 新着の日数
			if (CheckBoxEnableNew.Checked)
			{
				Int32 aNewDays;
				Int32.TryParse(TextBoxNewDays.Text, out aNewDays);
				if (aNewDays < NklCommon.NEW_DAYS_MIN)
				{
					throw new Exception("新着の日数は " + NklCommon.NEW_DAYS_MIN.ToString() + " 以上を指定して下さい。");
				}
			}
		}

		// --------------------------------------------------------------------
		// コンポーネントから設定に反映
		// --------------------------------------------------------------------
		public override void ComposToSettings()
		{
			base.ComposToSettings();

			WebOutputSettings aWebOutputSettings = (WebOutputSettings)OutputSettings;

			// 新着
			aWebOutputSettings.EnableNew = CheckBoxEnableNew.Checked;
			Int32 aNewDays;
			Int32.TryParse(TextBoxNewDays.Text, out aNewDays);
			aWebOutputSettings.NewDays = aNewDays;
		}

		// --------------------------------------------------------------------
		// 設定画面のタブページ
		// --------------------------------------------------------------------
		public override List<TabPage> DialogTabPages()
		{
			List<TabPage> aTabPages = base.DialogTabPages();

			// TabPageWebOutputSettings
			TabPageWebOutputSettings = new TabPage();
			TabPageWebOutputSettings.BackColor = SystemColors.Control;
			TabPageWebOutputSettings.Location = new Point(4, 22);
			TabPageWebOutputSettings.Padding = new Padding(3);
			TabPageWebOutputSettings.Size = new Size(456, 386);
			TabPageWebOutputSettings.Text = "HTML";

			// CheckBoxEnableNew
			CheckBoxEnableNew = new CheckBox();
			CheckBoxEnableNew.Location = new Point(16, 16);
			CheckBoxEnableNew.Size = new Size(24, 20);
			CheckBoxEnableNew.UseVisualStyleBackColor = true;
			CheckBoxEnableNew.CheckedChanged += new EventHandler(this.CheckBoxEnableNew_CheckedChanged);
			TabPageWebOutputSettings.Controls.Add(CheckBoxEnableNew);

			// TextBoxNewDays
			TextBoxNewDays = new TextBox();
			TextBoxNewDays.Location = new Point(40, 16);
			TextBoxNewDays.Size = new Size(40, 19);
			TabPageWebOutputSettings.Controls.Add(TextBoxNewDays);

			// LabelNewDays
			LabelNewDays = new Label();
			LabelNewDays.Location = new Point(88, 16);
			LabelNewDays.Size = new Size(352, 20);
			LabelNewDays.Text = "日以内に更新されたファイルを NEW （新着）に記載する";
			LabelNewDays.TextAlign = ContentAlignment.MiddleLeft;
			TabPageWebOutputSettings.Controls.Add(LabelNewDays);

			aTabPages.Add(TabPageWebOutputSettings);

			return aTabPages;
		}

		// --------------------------------------------------------------------
		// 設定画面を有効化するかどうか
		// --------------------------------------------------------------------
		public override Boolean IsDialogEnabled()
		{
			return true;
		}

		// --------------------------------------------------------------------
		// リスト出力
		// --------------------------------------------------------------------
		public override void Output()
		{
			PrepareOutput();
			Int32 aNumNewSongs = OutputNew();
			Dictionary<String, List<HeadInfo>> aCategoriesAndHeadInfos = OutputCategoryAndHeads();
			OutputIndex(aNumNewSongs, aCategoriesAndHeadInfos);
			OutputCss();
			OutputJs();
		}

		// --------------------------------------------------------------------
		// 設定をコンポーネントに反映
		// --------------------------------------------------------------------
		public override void SettingsToCompos()
		{
			base.SettingsToCompos();
			WebOutputSettings aWebOutputSettings = (WebOutputSettings)OutputSettings;

			// 新着
			CheckBoxEnableNew.Checked = aWebOutputSettings.EnableNew;
			TextBoxNewDays.Text = aWebOutputSettings.NewDays.ToString();
			UpdateTextBoxNewDays();
		}

		// ====================================================================
		// protected メンバー変数
		// ====================================================================

		// リストの拡張子（ピリオド含む）
		protected String mListExt;

		// 階層トップの名前（タイトル用）
		protected String mDirectoryTopName;

		// 階層トップのリンク（ページの先頭用）
		protected String mDirectoryTopLink;

		// 追加説明
		protected String mAdditionalDescription;

		// 追加 HTML ヘッダー
		protected String mAdditionalHeader;

		// トップページからリストをリンクする際の引数
		protected String mListLinkArg;

		// コンポーネント
		protected TabPage TabPageWebOutputSettings;
		protected CheckBox CheckBoxEnableNew;
		protected TextBox TextBoxNewDays;
		protected Label LabelNewDays;

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// リストに出力するファイル名の表現
		// --------------------------------------------------------------------
		protected abstract String FileNameDescription(String oFileName);

		// --------------------------------------------------------------------
		// コンストラクターでは行えない準備などを実施
		// --------------------------------------------------------------------
		protected override void PrepareOutput()
		{
			base.PrepareOutput();

			// テーブル項目名（原則 NklCommon.OUTPUT_ITEM_NAMES だが一部見やすいよう変更）
			mThNames = new List<String>(NklCommon.OUTPUT_ITEM_NAMES);
			mThNames[(Int32)OutputItems.Worker] = "制作";
			mThNames[(Int32)OutputItems.SmartTrack] = "On</th><th>Off";
			mThNames[(Int32)OutputItems.FileSize] = "サイズ";

			// 古いファイルを削除
			DeleteOldList();
		}


		// ====================================================================
		// private 定数
		// ====================================================================

		// リストファイル名の先頭文字列（インデックス以外）
		private const String FILE_NAME_PREFIX = "List";

		// HTML テンプレートに記載されている変数
		private const String HTML_VAR_ADDITIONAL_DESCRIPTION = "<!-- $AdditionalDescription$ -->";
		private const String HTML_VAR_ADDITIONAL_HEADER = "<!-- $AdditionalHeader$ -->";
		private const String HTML_VAR_CATEGORY = "<!-- $Category$ -->";
		private const String HTML_VAR_CATEGORY_INDEX = "<!-- $CategoryIndex$ -->";
		private const String HTML_VAR_CLASS_OF_AL = "<!-- $ClassOfAl$ -->";
		private const String HTML_VAR_CLASS_OF_KANA = "<!-- $ClassOfKana$ -->";
		private const String HTML_VAR_CLASS_OF_MISC = "<!-- $ClassOfMisc$ -->";
		private const String HTML_VAR_CLASS_OF_NUM = "<!-- $ClassOfNum$ -->";
		private const String HTML_VAR_DIRECTORY = "<!-- $Directory$ -->";
		private const String HTML_VAR_GENERATE_DATE = "<!-- $GenerateDate$ -->";
		private const String HTML_VAR_GENERATOR = "<!-- $Generator$ -->";
		private const String HTML_VAR_INDICES = "<!-- $Indices$ -->";
		private const String HTML_VAR_NEW = "<!-- $New$ -->";
		private const String HTML_VAR_NUM_SONGS = "<!-- $NumSongs$ -->";
		private const String HTML_VAR_PROGRAMS = "<!-- $Programs$ -->";
		private const String HTML_VAR_TITLE = "<!-- $Title$ -->";

		// テーブル非表示
		private const String CLASS_NAME_INVISIBLE = "class=\"invisible\"";

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// テーブルに表示する項目名
		private List<String> mThNames;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 曲情報を文字列に追加する
		// --------------------------------------------------------------------
		private void AppendSongInfo(StringBuilder oSB, TFound oTFound, Int32 oNumProgramSongs)
		{
			oSB.Append("  <tr class=\"");
			if (oNumProgramSongs % 2 == 0)
			{
				oSB.Append("even");
			}
			else
			{
				oSB.Append("odd");
			}
			oSB.Append("\">\n    ");

			foreach (OutputItems aOutputItem in mRuntimeOutputItems)
			{
				if (aOutputItem == OutputItems.ProgramName)
				{
					continue;
				}

				switch (aOutputItem)
				{
					case OutputItems.Path:
						oSB.Append("<td class=\"small\">" + FileNameDescription(oTFound.Path) + "</td>");
						break;
					case OutputItems.FileName:
						oSB.Append("<td class=\"small\">" + FileNameDescription(Path.GetFileName(oTFound.Path)) + "</td>");
						break;
					case OutputItems.Head:
						oSB.Append("<td>" + oTFound.Head + "</td>");
						break;
					case OutputItems.Worker:
						oSB.Append("<td>" + oTFound.Worker + "</td>");
						break;
					case OutputItems.Track:
						oSB.Append("<td>" + oTFound.Track + "</td>");
						break;
					case OutputItems.SmartTrack:
						oSB.Append("<td>" + (oTFound.SmartTrackOnVocal == NklCommon.RULE_VALUE_VOCAL_DEFAULT ? SMART_TRACK_VALID_MARK : null) + "</td>");
						oSB.Append("<td>" + (oTFound.SmartTrackOffVocal == NklCommon.RULE_VALUE_VOCAL_DEFAULT ? SMART_TRACK_VALID_MARK : null) + "</td>");
						break;
					case OutputItems.Comment:
						oSB.Append("<td class=\"small\">" + oTFound.Comment + "</td>");
						break;
					case OutputItems.LastWriteTime:
						oSB.Append("<td class=\"small\">" + JulianDay.JulianDayToDateTime(oTFound.LastWriteTime).ToString(
								NklCommon.DATE_FORMAT + " " + NklCommon.TIME_FORMAT) + "</td>");
						break;
					case OutputItems.FileSize:
						oSB.Append("<td class=\"small\">" + (oTFound.FileSize / (1024 * 1024)).ToString("#,0") + " MB</td>");
						break;
					case OutputItems.ProgramCategory:
						oSB.Append("<td>" + oTFound.ProgramCategory + "</td>");
						break;
					case OutputItems.ProgramGameCategory:
						oSB.Append("<td>" + oTFound.ProgramGameCategory + "</td>");
						break;
					case OutputItems.ProgramName:
						oSB.Append("<td>" + oTFound.ProgramName + "</td>");
						break;
					case OutputItems.ProgramRuby:
						oSB.Append("<td>" + oTFound.ProgramRuby + "</td>");
						break;
					case OutputItems.ProgramSubName:
						oSB.Append("<td>" + oTFound.ProgramSubName + "</td>");
						break;
					case OutputItems.ProgramSubRuby:
						oSB.Append("<td>" + oTFound.ProgramSubRuby + "</td>");
						break;
					case OutputItems.ProgramNumStories:
						oSB.Append("<td>" + oTFound.ProgramNumStories + "</td>");
						break;
					case OutputItems.ProgramAgeLimit:
						oSB.Append("<td>" + oTFound.ProgramAgeLimit + "</td>");
						break;
					case OutputItems.ProgramBeginDate:
						if (oTFound.ProgramBeginDate != 0.0d)
						{
							oSB.Append("<td class=\"small\">" + JulianDay.JulianDayToDateTime(oTFound.ProgramBeginDate).ToString(NklCommon.DATE_FORMAT) + "</td>");
						}
						else
						{
							oSB.Append("<td></td>");
						}
						break;
					case OutputItems.SongOpEd:
						oSB.Append("<td>" + oTFound.SongOpEd + "</td>");
						break;
					case OutputItems.SongCastSeq:
						oSB.Append("<td>" + oTFound.SongCastSeq + "</td>");
						break;
					case OutputItems.SongName:
						oSB.Append("<td>" + oTFound.SongName + "</td>");
						break;
					case OutputItems.SongRuby:
						oSB.Append("<td>" + oTFound.SongRuby + "</td>");
						break;
					case OutputItems.SongArtist:
						oSB.Append("<td>" + oTFound.SongArtist + "</td>");
						break;
					default:
						Debug.Assert(false, "AppendSongInfo() bad aOutputItem");
						break;
				}
			}

			oSB.Append("\n  </tr>\n");
		}

		// --------------------------------------------------------------------
		// 1 つの番組のまとまりを開始する
		// --------------------------------------------------------------------
		private void BeginProgram(StringBuilder oSB, TFound oTFound, ref Int32 oProgramIndex)
		{
			// 番組名挿入
			oSB.Append("<input type=\"checkbox\" id=\"label" + oProgramIndex + "\" class=\"accparent\"");

			// 番組名 == 頭文字、の場合（ボカロ等）は、リストが最初から開いた状態にする
			if (oTFound.ProgramName == oTFound.Head)
			{
				oSB.Append(" checked=\"checked\"");
			}
			oSB.Append(">\n");
			oSB.Append("<label for=\"label" + oProgramIndex + "\">" + oTFound.ProgramName + "　" + HTML_VAR_NUM_SONGS + "</label>\n");
			oSB.Append("<div class=\"accchild\">\n");

			// テーブルを開く
			oSB.Append("<table>\n");
			oSB.Append("  <tr>\n    ");
			foreach (OutputItems aOutputItem in mRuntimeOutputItems)
			{
				if (aOutputItem == OutputItems.ProgramName)
				{
					continue;
				}

				oSB.Append("<th>" + mThNames[(Int32)aOutputItem] + "</th>");
			}
			oSB.Append("\n  </tr>\n");

			oProgramIndex++;
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void CheckBoxEnableNew_CheckedChanged(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				UpdateTextBoxNewDays();
			}
			catch (Exception oExcep)
			{
				NklCommon.ShowLogMessage(TraceEventType.Error, "新着チェックボックスクリック時エラー：\n" + oExcep.Message);
				NklCommon.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// 古いリストを削除
		// --------------------------------------------------------------------
		private void DeleteOldList()
		{
			Debug.Assert(!String.IsNullOrEmpty(mListExt), "DeleteOldList() mListExt が初期化されていない");

			String[] aListPathes = Directory.GetFiles(FolderPath, FILE_NAME_PREFIX + "_*" + mListExt);

			foreach (String aPath in aListPathes)
			{
				try
				{
					File.Delete(aPath);
				}
				catch (Exception)
				{
					LogWriter.ShowLogMessage(TraceEventType.Error, "古いリストファイル " + Path.GetFileName(aPath) + " を削除できませんでした。", true);
				}
			}
		}

		// --------------------------------------------------------------------
		// 1 つの番組のまとまりを終了する
		// --------------------------------------------------------------------
		private void EndProgram(StringBuilder oSB, Int32 oNumProgramSongs)
		{
			oSB.Append("</table>\n");
			oSB.Append("</div>\n");
			String aBlock = oSB.ToString();
			oSB.Clear();
			oSB.Append(aBlock.Replace(HTML_VAR_NUM_SONGS, "（" + oNumProgramSongs.ToString("#,0") + " 曲）"));
		}

		// --------------------------------------------------------------------
		// 番組分類・番組名の頭文字 1 つ分の内容をファイルに出力
		// --------------------------------------------------------------------
		private void OutputCategoryAndHead(TFound oTFound, StringBuilder oSB, Int32 oNumProgramSongs, Int32 oNumHeadSongs,
				Dictionary<String, List<HeadInfo>> oCategoriesAndHeads)
		{
			// 閉じる
			EndProgram(oSB, oNumProgramSongs);

			String aTemplate = LoadTemplate("HtmlProgramHeadChar");

			// 階層
			String aCategory = oTFound.ProgramCategory;
			if (String.IsNullOrEmpty(aCategory))
			{
				aCategory = NklCommon.CATEGORY_MISC;
			}
			aTemplate = aTemplate.Replace(HTML_VAR_TITLE, mDirectoryTopName + " &gt; " + aCategory + " &gt; " + oTFound.Head);
			aTemplate = aTemplate.Replace(HTML_VAR_ADDITIONAL_HEADER, mAdditionalHeader);
			aTemplate = aTemplate.Replace(HTML_VAR_DIRECTORY, mDirectoryTopLink + " &gt; " + aCategory + " &gt; " + oTFound.Head);
			aTemplate = aTemplate.Replace(HTML_VAR_NUM_SONGS, "（" + oNumHeadSongs.ToString("#,0") + " 曲）");
			aTemplate = aTemplate.Replace(HTML_VAR_ADDITIONAL_DESCRIPTION, mAdditionalDescription);
			aTemplate = aTemplate.Replace(HTML_VAR_GENERATOR, NklCommon.APP_NAME_J + "  " + NklCommon.APP_VER);
			aTemplate = aTemplate.Replace(HTML_VAR_GENERATE_DATE, DateTime.Now.ToString(NklCommon.DATE_FORMAT));
			aTemplate = aTemplate.Replace(HTML_VAR_PROGRAMS, oSB.ToString());

			File.WriteAllText(FolderPath + OutputFileName(aCategory, oTFound.Head), aTemplate, Encoding.UTF8);

			HeadInfo aHeadInfo = new HeadInfo();
			aHeadInfo.Head = oTFound.Head;
			aHeadInfo.NumSongs = oNumHeadSongs;
			if (oCategoriesAndHeads.ContainsKey(aCategory))
			{
				oCategoriesAndHeads[aCategory].Add(aHeadInfo);
			}
			else
			{
				List<HeadInfo> aHeadInfos = new List<HeadInfo>();
				aHeadInfos.Add(aHeadInfo);
				oCategoriesAndHeads[aCategory] = aHeadInfos;
			}
			//Debug.WriteLine("OutputCategoryAndHead() aCategory: " + aCategory + ", aHead: " + oTFound.Head);
		}

		// --------------------------------------------------------------------
		// 番組分類・頭文字ごとのファイルを出力
		// --------------------------------------------------------------------
		private Dictionary<String, List<HeadInfo>> OutputCategoryAndHeads()
		{
			Dictionary<String, List<HeadInfo>> aCategoriesAndHeadInfos = new Dictionary<String, List<HeadInfo>>();

			StringBuilder aSB = new StringBuilder();
			IQueryable<TFound> aQueryResult =
					from x in TableFound
					orderby x.ProgramCategory, x.Head, x.ProgramRuby, x.ProgramName, x.SongRuby, x.SongName
					select x;
			TFound aPrevTFound = null;

			Int32 aNumProgramSongs = 0;
			Int32 aNumHeadSongs = 0;
			Int32 aProgramIndex = 0;
			foreach (TFound aTFound in aQueryResult)
			{
				if (aPrevTFound != null
						&& (aTFound.ProgramCategory != aPrevTFound.ProgramCategory || aTFound.Head != aPrevTFound.Head))
				{
					// これまでの内容を出力
					OutputCategoryAndHead(aPrevTFound, aSB, aNumProgramSongs, aNumHeadSongs, aCategoriesAndHeadInfos);
					aSB.Clear();
					aPrevTFound = null;
					aProgramIndex = 0;
					aNumHeadSongs = 0;
				}

				if (aPrevTFound == null
						|| aPrevTFound != null && aTFound.ProgramName != aPrevTFound.ProgramName)
				{
					if (aPrevTFound != null)
					{
						// 番組の区切り（終了）
						EndProgram(aSB, aNumProgramSongs);
					}

					// 番組の区切り（開始）
					BeginProgram(aSB, aTFound, ref aProgramIndex);
					aNumProgramSongs = 0;
				}

				// 曲情報追加
				AppendSongInfo(aSB, aTFound, aNumProgramSongs);

				// ループ処理
				aPrevTFound = aTFound;
				aNumProgramSongs++;
				aNumHeadSongs++;
			}

			if (aPrevTFound != null)
			{
				OutputCategoryAndHead(aPrevTFound, aSB, aNumProgramSongs, aNumHeadSongs, aCategoriesAndHeadInfos);
			}

			return aCategoriesAndHeadInfos;
		}

		// --------------------------------------------------------------------
		// CSS を出力
		// --------------------------------------------------------------------
		private void OutputCss()
		{
			String aTemplate = LoadTemplate("HtmlCss");
			File.WriteAllText(FolderPath + "List.css", aTemplate, Encoding.UTF8);
		}

		// --------------------------------------------------------------------
		// リストファイル名
		// --------------------------------------------------------------------
		private String OutputFileName(String oCategory, String oHead)
		{
			return FILE_NAME_PREFIX + "_" + StringToHex(oCategory) + "_" + StringToHex(oHead) + mListExt;
		}

		// --------------------------------------------------------------------
		// トップページ
		// --------------------------------------------------------------------
		private void OutputIndex(Int32 oNumNewSongs, Dictionary<String, List<HeadInfo>> oCategoriesAndHeadInfos)
		{
			// 番組分類（その他以外）
			StringBuilder aIndicesSB = new StringBuilder();
			Int32 aCategoryIndex = 0;
			Int32 aNumTotalSongs = 0;
			KeyValuePair<String, List<HeadInfo>> aMiscCategory = new KeyValuePair<String, List<HeadInfo>>(null, null);
			foreach (KeyValuePair<String, List<HeadInfo>> aCategoryAndHeadInfos in oCategoriesAndHeadInfos)
			{
				if (aCategoryAndHeadInfos.Key == NklCommon.CATEGORY_MISC)
				{
					aMiscCategory = aCategoryAndHeadInfos;
					continue;
				}

				String aOneTemplate;
				Int32 aNumCategorySongs;
				OutputIndexOneCategoryContents(aCategoryAndHeadInfos, ref aCategoryIndex, out aOneTemplate, out aNumCategorySongs);
				aIndicesSB.Append(aOneTemplate);
				aNumTotalSongs += aNumCategorySongs;
			}

			// その他
			if (aMiscCategory.Key != null)
			{
				String aOneTemplate;
				Int32 aNumCategorySongs;
				OutputIndexOneCategoryContents(aMiscCategory, ref aCategoryIndex, out aOneTemplate, out aNumCategorySongs);
				aIndicesSB.Append(aOneTemplate);
				aNumTotalSongs += aNumCategorySongs;
			}

			// 新着
			String aNewTemplate = null;
			if (oNumNewSongs > 0)
			{
				aNewTemplate = LoadTemplate("HtmlIndexNew");
				aNewTemplate = aNewTemplate.Replace("<td>" + NklCommon.CATEGORY_NEW + "</td>", "<td class=\"exist\"><a href=\"" + OutputFileName(NklCommon.CATEGORY_NEW, NklCommon.CATEGORY_NEW)
						+ mListLinkArg + "\">" + NklCommon.CATEGORY_NEW + "</a></td>");
				aNewTemplate = aNewTemplate.Replace(HTML_VAR_NUM_SONGS, "（" + oNumNewSongs.ToString("#,0") + " 曲）");
			}

			// トップページ
			String aTopTemplate = LoadTemplate("HtmlTop");
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_TITLE, mDirectoryTopName);
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_ADDITIONAL_HEADER, mAdditionalHeader);
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_DIRECTORY, mDirectoryTopLink);
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_NEW, aNewTemplate);
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_INDICES, aIndicesSB.ToString());
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_NUM_SONGS, "（合計 " + aNumTotalSongs.ToString("#,0") + " 曲）" + (oNumNewSongs > 0 ? "　※ NEW を除く" : null));
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_GENERATOR, NklCommon.APP_NAME_J + "  " + NklCommon.APP_VER);
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_GENERATE_DATE, DateTime.Now.ToString(NklCommon.DATE_FORMAT));
			File.WriteAllText(FolderPath + TopFileName, aTopTemplate, Encoding.UTF8);
		}

		// --------------------------------------------------------------------
		// トップページの 1 つの番組分類の出力内容
		// --------------------------------------------------------------------
		private void OutputIndexOneCategoryContents(KeyValuePair<String, List<HeadInfo>> oCategoryAndHeadInfos, ref Int32 oCategoryIndex,
				out String oOneTemplate, out Int32 oNumCategorySongs)
		{
			String aCategory = oCategoryAndHeadInfos.Key;
			Boolean aHasNum = false;
			Boolean aHasAl = false;
			Boolean aHasKana = false;
			Boolean aHasMisc = false;

			oOneTemplate = LoadTemplate("HtmlIndex");
			oOneTemplate = oOneTemplate.Replace(HTML_VAR_CATEGORY, aCategory);
			oNumCategorySongs = 0;

			foreach (HeadInfo aHeadInfo in oCategoryAndHeadInfos.Value)
			{
				oOneTemplate = oOneTemplate.Replace("<td>" + aHeadInfo.Head + "</td>", "<td class=\"exist\"><a href=\"" + OutputFileName(aCategory, aHeadInfo.Head)
						+ mListLinkArg + "\">" + aHeadInfo.Head + "</a></td>");
				oNumCategorySongs += aHeadInfo.NumSongs;

				if (aHeadInfo.Head == NklCommon.HEAD_MISC)
				{
					aHasMisc = true;
				}
				else if ('0' <= aHeadInfo.Head[0] && aHeadInfo.Head[0] <= '9')
				{
					aHasNum = true;
				}
				else if ('A' <= aHeadInfo.Head[0] && aHeadInfo.Head[0] <= 'Z')
				{
					aHasAl = true;
				}
				else
				{
					aHasKana = true;
				}
			}

			oOneTemplate = oOneTemplate.Replace(HTML_VAR_CATEGORY_INDEX, oCategoryIndex.ToString());
			oOneTemplate = oOneTemplate.Replace(HTML_VAR_NUM_SONGS, "（" + oNumCategorySongs.ToString("#,0") + " 曲）");
			oOneTemplate = oOneTemplate.Replace(HTML_VAR_CLASS_OF_NUM, aHasNum ? null : CLASS_NAME_INVISIBLE);
			oOneTemplate = oOneTemplate.Replace(HTML_VAR_CLASS_OF_AL, aHasAl ? null : CLASS_NAME_INVISIBLE);
			oOneTemplate = oOneTemplate.Replace(HTML_VAR_CLASS_OF_KANA, aHasKana ? null : CLASS_NAME_INVISIBLE);
			oOneTemplate = oOneTemplate.Replace(HTML_VAR_CLASS_OF_MISC, aHasMisc ? null : CLASS_NAME_INVISIBLE);

			oCategoryIndex++;
		}

		// --------------------------------------------------------------------
		// JS を出力
		// --------------------------------------------------------------------
		private void OutputJs()
		{
			String aTemplate = LoadTemplate("HtmlJs");
			File.WriteAllText(FolderPath + "List.js", aTemplate, Encoding.UTF8);
		}

		// --------------------------------------------------------------------
		// 新着を出力
		// --------------------------------------------------------------------
		private Int32 OutputNew()
		{
			WebOutputSettings aWebOutputSettings = (WebOutputSettings)OutputSettings;
			if (!aWebOutputSettings.EnableNew)
			{
				return 0;
			}

			Double aNewDate = JulianDay.DateTimeToJulianDay(DateTime.Now.AddDays(-aWebOutputSettings.NewDays));

			StringBuilder aSB = new StringBuilder();
			IQueryable<TFound> aQueryResult =
					from x in TableFound
					where x.LastWriteTime >= aNewDate
					orderby x.Head, x.ProgramRuby, x.ProgramName, x.SongRuby, x.SongName
					select x;
			TFound aPrevTFound = null;

			Int32 aNumProgramSongs = 0;
			Int32 aNumHeadSongs = 0;
			Int32 aProgramIndex = 0;
			foreach (TFound aTFound in aQueryResult)
			{
				if (aPrevTFound == null
						|| aPrevTFound != null && aTFound.ProgramName != aPrevTFound.ProgramName)
				{
					if (aPrevTFound != null)
					{
						// 番組の区切り（終了）
						EndProgram(aSB, aNumProgramSongs);
					}

					// 番組の区切り（開始）
					BeginProgram(aSB, aTFound, ref aProgramIndex);
					aNumProgramSongs = 0;
				}

				// 曲情報追加
				AppendSongInfo(aSB, aTFound, aNumProgramSongs);

				// ループ処理
				aPrevTFound = aTFound;
				aNumProgramSongs++;
				aNumHeadSongs++;
			}

			if (aNumHeadSongs > 0)
			{
				OutputNew(aSB, aNumProgramSongs, aNumHeadSongs);
			}

			return aNumHeadSongs;
		}

		// --------------------------------------------------------------------
		// 新着をファイルに出力
		// --------------------------------------------------------------------
		private void OutputNew(StringBuilder oSB, Int32 oNumProgramSongs, Int32 oNumHeadSongs)
		{
			// 閉じる
			EndProgram(oSB, oNumProgramSongs);

			String aTemplate = LoadTemplate("HtmlProgramHeadChar");

			// 階層
			aTemplate = aTemplate.Replace(HTML_VAR_TITLE, mDirectoryTopName + " &gt; " + NklCommon.CATEGORY_NEW);
			aTemplate = aTemplate.Replace(HTML_VAR_ADDITIONAL_HEADER, mAdditionalHeader);
			aTemplate = aTemplate.Replace(HTML_VAR_DIRECTORY, mDirectoryTopLink + " &gt; " + NklCommon.CATEGORY_NEW);
			aTemplate = aTemplate.Replace(HTML_VAR_NUM_SONGS, "（" + oNumHeadSongs.ToString("#,0") + " 曲）");
			aTemplate = aTemplate.Replace(HTML_VAR_ADDITIONAL_DESCRIPTION, mAdditionalDescription);
			aTemplate = aTemplate.Replace(HTML_VAR_PROGRAMS, oSB.ToString());

			File.WriteAllText(FolderPath + OutputFileName(NklCommon.CATEGORY_NEW, NklCommon.CATEGORY_NEW), aTemplate, Encoding.UTF8);
		}

		// --------------------------------------------------------------------
		// 文字を UTF-8 HEX に変換
		// --------------------------------------------------------------------
		private String StringToHex(String oString)
		{
			Byte[] aByteData = Encoding.UTF8.GetBytes(oString);
			return BitConverter.ToString(aByteData).Replace("-", String.Empty).ToLower();
		}

		// --------------------------------------------------------------------
		// 新着テキストボックスの状態を更新
		// --------------------------------------------------------------------
		private void UpdateTextBoxNewDays()
		{
			TextBoxNewDays.Enabled = CheckBoxEnableNew.Checked;
		}


	}
	// public class HtmlOutputWriter ___END___

	// ====================================================================
	// 頭文字 1 つの情報
	// ====================================================================

	public class HeadInfo
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public HeadInfo()
		{
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 頭文字
		public String Head { get; set; }

		// 当該頭文字の曲数
		public Int32 NumSongs { get; set; }

	}
	// public class HeadInfo ___END___

}
// namespace NicoKaraLister.Shared ___END___
