# language: zh-CN
功能: Snowflake 全域唯一 ID 整合測試

  在系統中：
  Snowflake 全域唯一 ID：`MySnowflake` (表名 `my_snowflake`) 的主鍵由 ShardingSphere 的 Snowflake 算法自動生成，且儲存於 `ds_mydefault` 資料庫中。

  场景: 驗證 Snowflake 主鍵自動生成並寫入預設資料庫
    当 我發送 POST 請求至 "/api/snowflakes" 帶有以下 JSON 內容:
      """
      {
        "name": "Snowflake Demo Item"
      }
      """
    那么 響應狀態碼應為 200
    而且 解析響應並將欄位 "id" 儲存為 "LastSnowflakeId"

    而且 應該在底層的 MySQL 數據庫 "ds_mydefault" 的表 "my_snowflake" 中找到主鍵 "id" 為 "{LastSnowflakeId}" 的記錄，且滿足以下欄位值:
      | 欄位 | 預期值 |
      | name | Snowflake Demo Item |
