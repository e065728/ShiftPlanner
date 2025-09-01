# File Summary

プロジェクト内のソースコードについて、各ファイルを1文で要約します。

## ShiftPlanner プロジェクト

| ファイル | 一言要約 |
| --- | --- |
| Controllers/IRosterAlgorithm.cs | シフト自動生成アルゴリズムの共通インターフェースを定義します。 |
| Controllers/ShiftAnalyzer.cs | シフトデータの集計とCSV出力を行う静的クラスです。 |
| Controllers/ShiftExporter.cs | シフト情報をCSVやPDFへ出力する機能を提供します。 |
| Controllers/ShiftGenerator.cs | ランダム方式でシフト割り当てを生成するクラスです。 |
| Controllers/ShiftGeneratorGreedy.cs | 制約を考慮した貪欲法でシフト割り当てを行うクラスです。 |
| Models/AppSettings.cs | アプリ全体の設定値を保持するデータクラスです。 |
| Models/CustomHoliday.cs | ユーザー定義の祝日を表すモデルです。 |
| Models/JapaneseHolidayHelper.cs | 日本の祝日判定と追加祝日の管理を行うヘルパーです。 |
| Models/Member.cs | 従業員の勤務条件や希望情報を保持するモデルです。 |
| Models/RequestSummary.cs | メンバーごとの希望件数を表示するためのサマリ行です。 |
| Models/RequestType.cs | 勤務希望や有休などの申請種別を定義する列挙体です。 |
| Models/ShiftAssignment.cs | 各日付の勤務名と割当メンバーを表すクラスです。 |
| Models/ShiftConstraints.cs | 週勤務時間や連続勤務上限などの制約値を保持するモデルです。 |
| Models/ShiftFrame.cs | 日付と勤務時間を持つシフト枠を表します。 |
| Models/ShiftLog.cs | JSON Lines形式でシフト関連のログを出力するクラスです。 |
| Models/ShiftRequest.cs | メンバーの勤務希望や休み申請を表すクラスです。 |
| Models/ShiftTime.cs | 勤務時間帯の名称や開始終了時刻を保持するマスタモデルです。 |
| Models/SkillGroup.cs | スキルグループのIDと名称を表すモデルです。 |
| Program.cs | アプリケーションのエントリポイントとしてメインフォームを起動します。 |
| Properties/AssemblyInfo.cs | アセンブリのメタデータを定義する自動生成ファイルです。 |
| Properties/Resources.Designer.cs | 埋め込みリソースへのアクセスを提供する自動生成クラスです。 |
| Properties/Settings.Designer.cs | アプリ設定を管理する自動生成クラスです。 |
| Utilities/ExcelHelper.cs | 簡易的にExcelファイルを生成するためのヘルパーです。 |
| Utilities/SimpleLogger.cs | ファイルへログを書き込む簡易ロガーです。 |
| Views/DataGridViewHelper.cs | DataGridViewの列設定を補助する静的メソッド集です。 |
| Views/HolidayMasterForm.Designer.cs | 祝日マスター編集フォームのデザインコードです。 |
| Views/HolidayMasterForm.cs | 祝日マスターを編集・表示するフォームの本体です。 |
| Views/MainForm.Designer.cs | メイン画面のUI構成を定義するデザインコードです。 |
| Views/MainForm.cs | シフトデータの管理と各機能の起点となるメインフォームです。 |
| Views/MemberMasterForm.Designer.cs | メンバー情報編集フォームのデザインコードです。 |
| Views/MemberMasterForm.cs | メンバーの勤務条件を編集するフォームの本体です。 |
| Views/ShiftRequestForm.Designer.cs | シフト希望入力フォームのデザインコードです。 |
| Views/ShiftRequestForm.cs | シフト希望を入力し申請を作成するフォームです。 |
| Views/ShiftTimeMasterForm.Designer.cs | 勤務時間マスター編集フォームのデザインコードです。 |
| Views/ShiftTimeMasterForm.cs | 勤務時間マスタを編集し色設定も扱うフォームです。 |
| Views/SkillGroupMasterForm.Designer.cs | スキルグループ編集フォームのデザインコードです。 |
| Views/SkillGroupMasterForm.cs | スキルグループの一覧を編集するフォームです。 |

## ShiftPlanner.Tests プロジェクト

| ファイル | 一言要約 |
| --- | --- |
| Tests/ShiftAnalyzerTests.cs | ShiftAnalyzerの集計機能を検証する単体テストです。 |
| Tests/ShuffleTests.cs | Shuffleメソッドが要素数を維持することを確認するテストです。 |
