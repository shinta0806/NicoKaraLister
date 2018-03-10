// ============================================================================
// 
// リスト出力設定用基底クラス
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 基底部分の設定内容を派生クラス間で共有できるようにするために、
// ・派生クラスで設定を保存する際は、基底部分を別ファイルとして保存する
// ・派生クラスで設定を読み込む際は、別ファイルの基底部分を追加で読み込む
// 本来は ApplicationSettingsBase の派生として実装したいが、Common.ShallowCopy()
//   が使えず上記を実現できないため、通常クラスとして実装する
// ----------------------------------------------------------------------------

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace NicoKaraLister.Shared
{
	public class OutputSettings
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public OutputSettings()
		{
			// 初期化（リストはデシリアライズ時に重複するため初期化しない）
			OutputAllItems = false;
		}

		// ====================================================================
		// public プロパティ
		// ====================================================================

		// リスト化対象ファイルの拡張子
		public List<String> TargetExts { get; set; }

		// 全ての項目を出力する
		public Boolean OutputAllItems { get; set; }

		// 出力項目の選択
		public List<OutputItems> SelectedOutputItems { get; set; }

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 読み込み
		// --------------------------------------------------------------------
		public virtual void Load()
		{
			try
			{
				OutputSettings aTmp = Common.Deserialize<OutputSettings>(NklCommon.SettingsPath() + FILE_NAME_OUTPUT_SETTINGS_CONFIG);
				Common.ShallowCopy(aTmp, this);
			}
			catch (Exception)
			{
			}

			InitIfNeeded();
		}

		// --------------------------------------------------------------------
		// 保存
		// --------------------------------------------------------------------
		public virtual void Save()
		{
			try
			{
				OutputSettings aTmp = new OutputSettings();
				Common.ShallowCopy(this, aTmp);
				Common.Serialize(NklCommon.SettingsPath() + FILE_NAME_OUTPUT_SETTINGS_CONFIG, aTmp);
			}
			catch (Exception)
			{
			}
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// 保存用ファイル名
		private const String FILE_NAME_OUTPUT_SETTINGS_CONFIG = "OutputSettings" + Common.FILE_EXT_CONFIG;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 必要に応じて初期化
		// --------------------------------------------------------------------
		private void InitIfNeeded()
		{
			if (TargetExts == null)
			{
				TargetExts = new List<String>();
			}
			if (TargetExts.Count == 0)
			{
				// 動画・音声をアルファベット順に追加
				TargetExts.Add(Common.FILE_EXT_AVI);
				TargetExts.Add(Common.FILE_EXT_FLV);
				TargetExts.Add(Common.FILE_EXT_M4A);
				TargetExts.Add(Common.FILE_EXT_MKV);
				TargetExts.Add(Common.FILE_EXT_MOV);
				TargetExts.Add(Common.FILE_EXT_MP3);
				TargetExts.Add(Common.FILE_EXT_MP4);
				TargetExts.Add(Common.FILE_EXT_MPG);
				TargetExts.Add(Common.FILE_EXT_WAV);
				TargetExts.Add(Common.FILE_EXT_WMA);
				TargetExts.Add(Common.FILE_EXT_WMV);
			}
			if (SelectedOutputItems == null)
			{
				SelectedOutputItems = new List<OutputItems>();
			}
			if (SelectedOutputItems.Count == 0)
			{
				SelectedOutputItems.Add(OutputItems.ProgramName);
				SelectedOutputItems.Add(OutputItems.SongOpEd);
				SelectedOutputItems.Add(OutputItems.SongName);
				SelectedOutputItems.Add(OutputItems.SongArtist);
				SelectedOutputItems.Add(OutputItems.SmartTrack);
				SelectedOutputItems.Add(OutputItems.Worker);
				SelectedOutputItems.Add(OutputItems.Comment);
				SelectedOutputItems.Add(OutputItems.FileName);
				SelectedOutputItems.Add(OutputItems.FileSize);
			}
		}

	}
	// public class OutputSettings ___END___

}
// namespace NicoKaraLister.Shared ___END___
