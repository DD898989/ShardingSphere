# language: zh-CN
功能: 廣播表 Country 整合測試

  在系統中，廣播表 (Broadcast Table)：`Country` (表名 `country`) 數據會自動廣播同步寫入所有物理資料庫。

  场景: 驗證廣播表自動寫入所有物理庫
    当 我發送 POST 請求至 "/api/countries" 帶有以下 JSON 內容:
      """
      {
        "id": 886,
        "name": "Taiwan",
        "code": "TW"
      }
      """
    那么 響應狀態碼應為 200

    而且 應該在底層的 MySQL 數據庫 "ds_0" 的表 "country" 中找到主鍵 "id" 為 "886" 的記錄，且滿足以下欄位值:
      | 欄位 | 預期值 |
      | name | Taiwan |
      | code | TW     |

    而且 應該在底層的 MySQL 數據庫 "ds_1" 的表 "country" 中找到主鍵 "id" 為 "886" 的記錄，且滿足以下欄位值:
      | 欄位 | 預期值 |
      | name | Taiwan |
      | code | TW     |

    而且 應該在底層的 MySQL 數據庫 "ds_2025" 的表 "country" 中找到主鍵 "id" 為 "886" 的記錄，且滿足以下欄位值:
      | 欄位 | 預期值 |
      | name | Taiwan |
      | code | TW     |

    而且 應該在底層的 MySQL 數據庫 "ds_2026" 的表 "country" 中找到主鍵 "id" 為 "886" 的記錄，且滿足以下欄位值:
      | 欄位 | 預期值 |
      | name | Taiwan |
      | code | TW     |

    而且 應該在底層的 MySQL 數據庫 "ds_mydefault" 的表 "country" 中找到主鍵 "id" 為 "886" 的記錄，且滿足以下欄位值:
      | 欄位 | 預期值 |
      | name | Taiwan |
      | code | TW     |
