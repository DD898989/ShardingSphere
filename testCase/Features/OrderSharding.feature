# language: zh-CN
功能: 綁定表 Order 與 OrderItem 整合測試 (批量寫入後批量驗證模式)

  在系統中，訂單（Order）與訂單明細（OrderItem）是綁定表關係，使用手動指定的 ID：
  1. 數據庫分片：根據 user_id % 2 儲存至 ds_0 (偶數) 還是 ds_1 (奇數)。
  2. 訂單表分片：根據 order_id % 2 儲存至 order_0 還是 order_1。
  3. 明細表分片：根據 order_id % 2 儲存至 order_item_0 還是 order_item_1。

  场景: 批量創建多個綁定訂單與多筆明細，並一次性驗證所有不同物理庫物理表的多筆記錄
    当 我發送 POST 請求至 "/api/orders" 帶有以下 JSON 內容:
      """
      {
        "orderId": 500,
        "userId": 10,
        "status": "PENDING",
        "items": [
          {
            "itemId": 501,
            "productName": "Keyboard",
            "price": 99
          },
          {
            "itemId": 502,
            "productName": "Mouse",
            "price": 49
          }
        ]
      }
      """
    那么 響應狀態碼應為 200

    当 我發送 POST 請求至 "/api/orders" 帶有以下 JSON 內容:
      """
      {
        "orderId": 511,
        "userId": 11,
        "status": "PENDING",
        "items": [
          {
            "itemId": 512,
            "productName": "Monitor",
            "price": 299
          },
          {
            "itemId": 513,
            "productName": "Cable",
            "price": 15
          }
        ]
      }
      """
    那么 響應狀態碼應為 200

    当 我發送 POST 請求至 "/api/orders" 帶有以下 JSON 內容:
      """
      {
        "orderId": 521,
        "userId": 12,
        "status": "PENDING",
        "items": [
          {
            "itemId": 522,
            "productName": "Cable",
            "price": 10
          },
          {
            "itemId": 523,
            "productName": "Adapter",
            "price": 25
          }
        ]
      }
      """
    那么 響應狀態碼應為 200

    当 我發送 POST 請求至 "/api/orders" 帶有以下 JSON 內容:
      """
      {
        "orderId": 530,
        "userId": 13,
        "status": "PENDING",
        "items": [
          {
            "itemId": 531,
            "productName": "Headset",
            "price": 79
          },
          {
            "itemId": 532,
            "productName": "Stand",
            "price": 19
          }
        ]
      }
      """
    那么 響應狀態碼應為 200

    而且 應該在底層 MySQL 中驗證以下分片記錄符合配置:
      | 數據庫 | 數據表   | 主鍵欄位  | 主鍵值 | user_id | status  |
      | ds_0   | order_0 | order_id  | 500    | 10      | PENDING |
      | ds_1   | order_1 | order_id  | 511    | 11      | PENDING |
      | ds_0   | order_1 | order_id  | 521    | 12      | PENDING |
      | ds_1   | order_0 | order_id  | 530    | 13      | PENDING |

    而且 應該在底層 MySQL 中驗證以下分片記錄符合配置:
      | 數據庫 | 數據表        | 主鍵欄位 | 主鍵值 | order_id | user_id | product_name | price |
      | ds_0   | order_item_0 | item_id  | 501    | 500      | 10      | Keyboard     | 99    |
      | ds_0   | order_item_0 | item_id  | 502    | 500      | 10      | Mouse        | 49    |
      | ds_1   | order_item_1 | item_id  | 512    | 511      | 11      | Monitor      | 299   |
      | ds_1   | order_item_1 | item_id  | 513    | 511      | 11      | Cable        | 15    |
      | ds_0   | order_item_1 | item_id  | 522    | 521      | 12      | Cable        | 10    |
      | ds_0   | order_item_1 | item_id  | 523    | 521      | 12      | Adapter      | 25    |
      | ds_1   | order_item_0 | item_id  | 531    | 530      | 13      | Headset      | 79    |
      | ds_1   | order_item_0 | item_id  | 532    | 530      | 13      | Stand        | 19    |
