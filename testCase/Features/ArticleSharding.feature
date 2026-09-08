# language: zh-CN
功能: 主從表 Article 與 ArticleContent 整合測試 (批量寫入後批量驗證模式)

  在系統中，文章（Article）主表與其內容表（ArticleContent）是 JPA 二級表（SecondaryTable）與綁定表關係：
  1. 數據庫分片：根據 user_id % 2 決定儲存至 ds_0 (偶數) 還是 ds_1 (奇數)。
  2. 文章主表分片：根據 article_id % 2 決定儲存至 article_0 還是 article_1。
  3. 文章內容表分片：根據 article_id % 2 決定儲存至 article_content_0 還是 article_content_1。

  场景: 批量創建多篇分片文章，並直連 MySQL 主表與內容表中一次性驗證多片記錄
    当 我發送 POST 請求至 "/api/articles" 帶有以下 JSON 內容:
      """
      {
        "articleId": 300,
        "title": "BDD Guide",
        "userId": 42,
        "content": "Comprehensive ShardingSphere BDD Testing"
      }
      """
    那么 響應狀態碼應為 200

    当 我發送 POST 請求至 "/api/articles" 帶有以下 JSON 內容:
      """
      {
        "articleId": 301,
        "title": "Docker Tricks",
        "userId": 43,
        "content": "Advanced Docker deployment tips"
      }
      """
    那么 響應狀態碼應為 200

    当 我發送 POST 請求至 "/api/articles" 帶有以下 JSON 內容:
      """
      {
        "articleId": 302,
        "title": "K8s Deploy",
        "userId": 45,
        "content": "Orchestrating Sharded Databases"
      }
      """
    那么 響應狀態碼應為 200

    当 我發送 POST 請求至 "/api/articles" 帶有以下 JSON 內容:
      """
      {
        "articleId": 303,
        "title": "Sharding Guide",
        "userId": 44,
        "content": "ShardingSphere Deep Dive"
      }
      """
    那么 響應狀態碼應為 200

    而且 應該在底層 MySQL 中驗證以下分片記錄符合配置:
      | 數據庫 | 數據表    | 主鍵欄位   | 主鍵值 | user_id | title          |
      | ds_0   | article_0 | article_id | 300    | 42      | BDD Guide      |
      | ds_1   | article_1 | article_id | 301    | 43      | Docker Tricks  |
      | ds_1   | article_0 | article_id | 302    | 45      | K8s Deploy     |
      | ds_0   | article_1 | article_id | 303    | 44      | Sharding Guide |

    而且 應該在底層 MySQL 中驗證以下分片記錄符合配置:
      | 數據庫 | 數據表            | 主鍵欄位   | 主鍵值 | content_user_id | content                                  |
      | ds_0   | article_content_0 | article_id | 300    | 42              | Comprehensive ShardingSphere BDD Testing |
      | ds_1   | article_content_1 | article_id | 301    | 43              | Advanced Docker deployment tips          |
      | ds_1   | article_content_0 | article_id | 302    | 45              | Orchestrating Sharded Databases          |
      | ds_0   | article_content_1 | article_id | 303    | 44              | ShardingSphere Deep Dive                 |
