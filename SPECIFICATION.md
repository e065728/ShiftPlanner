# ShiftPlanner 仕様書

## 目的

ShiftPlanner は、従業員の勤務シフトを作成・管理する Windows Forms アプリケーションです。小規模なチームを想定し、手作業によるシフト作成の負担を軽減することを目的としています。

## 主な機能

- **メンバー管理**
  - 従業員ごとの勤務可能曜日・勤務時間帯の設定
  - スキル、資格、スキルグループの登録
  - 土日勤務可否や連続勤務上限など、勤務に関する制約設定
  - 希望休の登録
- **マスター管理**
  - 勤務時間帯(ShiftTime)やスキルグループ(SkillGroup)の編集
  - 祝日マスター(CustomHoliday)の編集
- **シフト生成**
  - 勤務枠(ShiftFrame)に対しランダムまたは貪欲法による自動割当
  - メンバーの勤務希望・休み希望を考慮した割り当て
  - 最低休日日数やスキルグループ要件を満たすよう調整
- **分析・出力**
  - 月別の労働時間集計、シフトタイプ分布の取得
  - シフト表や分析結果を CSV/Excel 形式で出力
- **GUI 操作**
  - 直感的なフォーム操作で各種マスターやシフト表を編集
  - DataGridView を用いた一覧表示とセル編集

## データ保存

アプリケーション設定やメンバー情報など、各種データはユーザーの `AppData/ShiftPlanner` フォルダに JSON 形式で保存されます。起動時に自動読み込みを行い、終了時に保存します。

## 開発環境

- .NET Framework 4.8 / C# 8.0
- Windows Forms
- 必要な外部ライブラリは `packages.config` で管理

以上が現行実装の概要です。今後の機能追加や改善においては、本仕様書を参考にしてください。
