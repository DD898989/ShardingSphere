# language: zh-CN
功能: 時間記錄時間範圍分片與 API 整合測試 (批量寫入後批量驗證模式)

  在系統中，時間記錄（TimeRecord）是根據時間範圍進行分片的：
  1. 數據庫分片：2025 年的數據儲存在 ds_2025，2026 年的數據儲存在 ds_2026。
  2. 表分片：根據月份（01 到 12），如 5 月儲存在 time_record_yyyy05，8 月儲存在 time_record_yyyy08。
  其中 id 是在測試中手動指定，不再自動生成。

  场景: 批量創建多個年份和月份的時間記錄，並一次性直連 MySQL 驗證所有分片記錄
    当 我發送 POST 請求至 "/api/records" 帶有以下參數:
      | 欄位   | 值                  |
      | id     | 601                 |
      | myTime | 2025-05-15 12:00:00 |
      | data   | Hello 2025 May      |
    那么 響應狀態碼應為 200

    当 我發送 POST 請求至 "/api/records" 帶有以下參數:
      | 欄位   | 值                  |
      | id     | 602                 |
      | myTime | 2026-08-20 15:30:00 |
      | data   | Hello 2026 August   |
    那么 響應狀態碼應為 200

    当 我發送 POST 請求至 "/api/records" 帶有以下參數:
      | 欄位   | 值                  |
      | id     | 603                 |
      | myTime | 2025-12-01 09:00:00 |
      | data   | Hello 2025 Dec      |
    那么 響應狀態碼應為 200

    当 我發送 POST 請求至 "/api/records" 帶有以下參數:
      | 欄位   | 值                  |
      | id     | 604                 |
      | myTime | 2026-01-10 18:45:00 |
      | data   | Hello 2026 Jan      |
    那么 響應狀態碼應為 200

    而且 應該在底層 MySQL 中驗證以下分片記錄符合配置:
      | 數據庫  | 數據表              | 主鍵欄位 | 主鍵值 | my_time             | data              |
      | ds_2025 | time_record_202505 | id       | 601    | 2025-05-15 12:00:00 | Hello 2025 May    |
      | ds_2026 | time_record_202608 | id       | 602    | 2026-08-20 15:30:00 | Hello 2026 August |
      | ds_2025 | time_record_202512 | id       | 603    | 2025-12-01 09:00:00 | Hello 2025 Dec    |
      | ds_2026 | time_record_202601 | id       | 604    | 2026-01-10 18:45:00 | Hello 2026 Jan    |
