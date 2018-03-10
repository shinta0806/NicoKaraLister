// ============================================================================
// 
// CSV リスト出力クラス
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace NicoKaraLister.Shared
{
	public class CsvOutputWriter : OutputWriter
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public CsvOutputWriter()
		{
			// プロパティー
			FormatName = "CSV";
			TopFileName = "List.csv";
			OutputSettings = new CsvOutputSettings();
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

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
			StringBuilder aSB = new StringBuilder();
			PrepareOutput();

			// ヘッダー
			aSB.Append("No.");
			foreach (OutputItems aOutputItem in mRuntimeOutputItems)
			{
				switch (aOutputItem)
				{
					case OutputItems.SmartTrack:
						aSB.Append(",On,Off");
						break;
					case OutputItems.LastWriteTime:
						aSB.Append(",最終更新日,最終更新時刻");
						break;
					default:
						aSB.Append("," + NklCommon.OUTPUT_ITEM_NAMES[(Int32)aOutputItem]);
						break;
				}
			}
			aSB.Append("\n");

			IQueryable<TFound> aQueryResult =
					from x in TableFound
					orderby x.ProgramCategory, x.Head, x.ProgramRuby, x.ProgramName, x.SongRuby, x.SongName
					select x;

			// コンテンツ
			Int32 aIndex = 1;
			foreach (TFound aTFound in aQueryResult)
			{
				aSB.Append(aIndex.ToString());

				foreach (OutputItems aOutputItem in mRuntimeOutputItems)
				{
					switch (aOutputItem)
					{
						case OutputItems.Path:
							aSB.Append(",\"" + aTFound.Path + "\"");
							break;
						case OutputItems.FileName:
							aSB.Append(",\"" + Path.GetFileName(aTFound.Path) + "\"");
							break;
						case OutputItems.Head:
							aSB.Append(",\"" + aTFound.Head + "\"");
							break;
						case OutputItems.Worker:
							aSB.Append(",\"" + aTFound.Worker + "\"");
							break;
						case OutputItems.Track:
							aSB.Append(",\"" + aTFound.Track + "\"");
							break;
						case OutputItems.SmartTrack:
							aSB.Append(",\"" + (aTFound.SmartTrackOnVocal == NklCommon.RULE_VALUE_VOCAL_DEFAULT ? SMART_TRACK_VALID_MARK : null) + "\"");
							aSB.Append(",\"" + (aTFound.SmartTrackOffVocal == NklCommon.RULE_VALUE_VOCAL_DEFAULT ? SMART_TRACK_VALID_MARK : null) + "\"");
							break;
						case OutputItems.Comment:
							aSB.Append(",\"" + aTFound.Comment + "\"");
							break;
						case OutputItems.LastWriteTime:
							aSB.Append("," + JulianDay.JulianDayToDateTime(aTFound.LastWriteTime).ToString(NklCommon.DATE_FORMAT));
							aSB.Append("," + JulianDay.JulianDayToDateTime(aTFound.LastWriteTime).ToString(NklCommon.TIME_FORMAT));
							break;
						case OutputItems.FileSize:
							aSB.Append("," + aTFound.FileSize);
							break;
						case OutputItems.ProgramCategory:
							aSB.Append(",\"" + aTFound.ProgramCategory + "\"");
							break;
						case OutputItems.ProgramGameCategory:
							aSB.Append(",\"" + aTFound.ProgramGameCategory + "\"");
							break;
						case OutputItems.ProgramName:
							aSB.Append(",\"" + aTFound.ProgramName + "\"");
							break;
						case OutputItems.ProgramRuby:
							aSB.Append(",\"" + aTFound.ProgramRuby + "\"");
							break;
						case OutputItems.ProgramSubName:
							aSB.Append(",\"" + aTFound.ProgramSubName + "\"");
							break;
						case OutputItems.ProgramSubRuby:
							aSB.Append(",\"" + aTFound.ProgramSubRuby + "\"");
							break;
						case OutputItems.ProgramNumStories:
							aSB.Append(",\"" + aTFound.ProgramNumStories + "\"");
							break;
						case OutputItems.ProgramAgeLimit:
							aSB.Append(",\"" + aTFound.ProgramAgeLimit + "\"");
							break;
						case OutputItems.ProgramBeginDate:
							if (aTFound.ProgramBeginDate != 0.0d)
							{
								aSB.Append("," + JulianDay.JulianDayToDateTime(aTFound.ProgramBeginDate).ToString(NklCommon.DATE_FORMAT));
							}
							else
							{
								aSB.Append(",");
							}
							break;
						case OutputItems.SongOpEd:
							aSB.Append(",\"" + aTFound.SongOpEd + "\"");
							break;
						case OutputItems.SongCastSeq:
							aSB.Append(",\"" + aTFound.SongCastSeq + "\"");
							break;
						case OutputItems.SongName:
							aSB.Append(",\"" + aTFound.SongName + "\"");
							break;
						case OutputItems.SongRuby:
							aSB.Append(",\"" + aTFound.SongRuby + "\"");
							break;
						case OutputItems.SongArtist:
							aSB.Append(",\"" + aTFound.SongArtist + "\"");
							break;
						default:
							Debug.Assert(false, "Output() bad aOutputItem");
							break;
					}

				}
				aSB.Append("\n");

				aIndex++;

			}
			File.WriteAllText(FolderPath + TopFileName, aSB.ToString(), Encoding.UTF8);
		}
	}
	// public class CsvOutputWriter ___END___

}
// namespace NicoKaraLister.Shared ___END___
