# ShardingSphere + Spring Boot & BDD 整合測試專案

本專案展示如何結合 **ShardingSphere-JDBC**、**Spring Boot**、**JPA** 與 **BDD** 進行分庫分表整合與自動化白箱測試。

---

## 🚀 專案說明

本專案為 **DEMO 專案**，您可以直接閱讀 `testCase/Features` 中的 `*.feature` 檔案來瞭解本專案成果與各場景的分片路由設計。

---

## 📝 BDD 測試場景與路由架構示意圖

本測試套件採用「**API 寫入 ➡️ 物理庫直連驗證**」的白箱模型，確保資料真實路由至目標位置。您可以透過以下各測試場景與其對應的架構示意圖瞭解各項分片路由規則：

### 1. 訂單分片與綁定表 🔗
* **Feature 連結**：[OrderSharding.feature](testCase/Features/OrderSharding.feature)
* **路由規則**：
  * 分庫：`user_id % 2` ➡️ `ds_0` (偶數) / `ds_1` (奇數)
  * 分表：`order_id % 2` ➡️ `order_0` (偶數) / `order_1` (奇數)
* **架構路由示意圖**：
```mermaid
graph TD
    Client[客戶端 API 請求] --> |Order 實體| SS[ShardingSphere-JDBC]
    SS --> |"user_id % 2 == 0"| DS0[(ds_0 物理庫)]
    SS --> |"user_id % 2 == 1"| DS1[(ds_1 物理庫)]

    DS0 --> |"order_id % 2 == 0"| DS0_T0[order_0 / order_item_0]
    DS0 --> |"order_id % 2 == 1"| DS0_T1[order_1 / order_item_1]

    DS1 --> |"order_id % 2 == 0"| DS1_T0[order_0 / order_item_0]
    DS1 --> |"order_id % 2 == 1"| DS1_T1[order_1 / order_item_1]
```

### 2. 文章分片與 JPA 二級表 📝
* **Feature 連結**：[ArticleSharding.feature](testCase/Features/ArticleSharding.feature)
* **路由規則**：
  * 分庫：`user_id % 2` ➡️ `ds_0` (偶數) / `ds_1` (奇數)
  * 分表：`article_id % 2` ➡️ `article_0` (偶數) / `article_1` (奇數)
* **架構路由示意圖**：
```mermaid
graph TD
    Client[客戶端 API 請求] --> |Article 實體| SS[ShardingSphere-JDBC]
    SS --> |"user_id % 2 == 0"| DS0[(ds_0 物理庫)]
    SS --> |"user_id % 2 == 1"| DS1[(ds_1 物理庫)]

    DS0 --> |"article_id % 2 == 0"| DS0_T0[article_0 / article_content_0]
    DS0 --> |"article_id % 2 == 1"| DS0_T1[article_1 / article_content_1]

    DS1 --> |"article_id % 2 == 0"| DS1_T0[article_0 / article_content_0]
    DS1 --> |"article_id % 2 == 1"| DS1_T1[article_1 / article_content_1]
```

### 3. 時間範圍與間隔分片 ⏳
* **Feature 連結**：[TimeRecordSharding.feature](testCase/Features/TimeRecordSharding.feature)
* **路由規則**：
  * 分庫（年份）：`2025` ➡️ `ds_2025` / `2026` ➡️ `ds_2026`
  * 分表（月份）：`yyyyMM` 格式按月分切（例如：`time_record_202505`、`time_record_202608`）
* **架構路由示意圖**：
```mermaid
graph TD
    Client[客戶端 API 請求] --> |TimeRecord 實體| SS[ShardingSphere-JDBC]
    SS --> |"年份為 2025"| DS2025[(ds_2025 物理庫)]
    SS --> |"年份為 2026"| DS2026[(ds_2026 物理庫)]

    DS2025 --> |"按月路由 (如 5 月)"| DS2025_M5[time_record_202505]
    DS2025 --> |"按月路由 (如 12 月)"| DS2025_M12[time_record_202512]

    DS2026 --> |"按月路由 (如 8 月)"| DS2026_M8[time_record_202608]
```

### 4. 全域雪花 ID 驗證 ❄️
* **Feature 連結**：[SnowflakeId.feature](testCase/Features/SnowflakeId.feature)
* **路由規則**：
  * 主鍵由 ShardingSphere 內建 Snowflake 算法自動生成，並直接路由存儲至預設物理庫 `ds_mydefault` 的 `my_snowflake` 表。
* **架構路由示意圖**：
```mermaid
graph TD
    Client[客戶端 API 請求] --> |MySnowflake 實體| SS[ShardingSphere-JDBC]
    SS --> |"1. 產生全域唯一 ID"| Generator[Snowflake ID 產生器]
    SS --> |"2. 寫入預設庫"| DS_DEF[(ds_mydefault 物理庫)]
    DS_DEF --> Table[my_snowflake]
```

### 5. 廣播表多庫同步 📢
* **Feature 連結**：[BroadcastTable.feature](testCase/Features/BroadcastTable.feature)
* **路由規則**：
  * 對廣播表 `country` 的任何寫入都會被自動同步廣播寫入所有物理資料庫。
* **架構路由示意圖**：
```mermaid
graph TD
    Client[客戶端 API 請求] --> |Country 實體| SS[ShardingSphere-JDBC]
    SS --> |"同步寫入"| Broadcast{廣播機制}
    Broadcast --> DS0[(ds_0 物理庫)]
    Broadcast --> DS1[(ds_1 物理庫)]
    Broadcast --> DS2025[(ds_2025 物理庫)]
    Broadcast --> DS2026[(ds_2026 物理庫)]
    Broadcast --> DS_DEF[(ds_mydefault 物理庫)]
```


