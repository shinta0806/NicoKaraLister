﻿// ============================================================================
// 
// フォルダーごとの設定を管理
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace NicoKaraLister.Shared
{
	// ====================================================================
	// フォルダーごとの設定（フォルダ内に保存する用）
	// ====================================================================

	public class FolderSettingsInDisk
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// --------------------------------------------------------------------
		// 基本情報
		// --------------------------------------------------------------------

		// 保存時のアプリケーションのバージョン
		public String AppVer { get; set; }

		// --------------------------------------------------------------------
		// 設定
		// --------------------------------------------------------------------

		// ファイル命名規則（アプリ独自ルール表記）
		public List<String> FileNameRules { get; set; }

		// フォルダー命名規則（アプリ独自ルール表記）
		public List<String> FolderNameRules { get; set; }

		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FolderSettingsInDisk()
		{
			FileNameRules = new List<String>();
			FolderNameRules = new List<String>();
		}

	}
	// public class FolderSettingsInDisk ___END___

	// ====================================================================
	// フォルダーごとの設定（アプリ動作時用）
	// ====================================================================

	public class FolderSettingsInMemory
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// --------------------------------------------------------------------
		// 設定
		// --------------------------------------------------------------------

		// ファイル命名規則（正規表現）
		public List<String> FileNameRules { get; set; }

		// ファイル正規表現にマッチしたグループが表す項目（TFound のカラム名）
		public List<List<String>> FileRegexGroups { get; set; }

		// フォルダー命名規則（辞書）
		public Dictionary<String, String> FolderNameRules { get; set; }

		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FolderSettingsInMemory()
		{
			FileNameRules = new List<String>();
			FileRegexGroups = new List<List<String>>();
			FolderNameRules = NklCommon.CreateRuleDictionary();
		}

	}
	// public class FolderSettingsInMemory ___END___

}
// namespace NicoKaraLister.Shared ___END___
