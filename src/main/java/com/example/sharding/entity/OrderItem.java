package com.example.sharding.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.Table;
import jakarta.persistence.Id;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;

/*
   1. Hibernate 預設命名規則不一致：
      * 在 JPA 中，實體類別為 OrderItem。依據 Hibernate 預設的命名規則，它會自動將 CamelCase
        對應至不含底線的表名 orderitem。
   2. ShardingSphere 無法辨識邏輯表：
      * 在我們的 sharding-config.yaml 中，配置的邏輯表名稱是含有底線的 order_item。
      * 由於 orderitem 匹配不到 order_item 規則，ShardingSphere 將其視為一個普通非分片單表（Single
        Table）。
   3. 退化路由（Fallback Routing）：
      * 針對未配置分片規則的普通單表，ShardingSphere
        會將其自動路由到預設或最後配置的單獨數據庫（在此處為 ds_2026），所以才在 ds_2026 中產生了單表
        orderitem。

  ---

  🛠️ 解決方案與修復步驟：

      在 OrderItem.java 頂部明確加上 @Table(name = "order_item") 註解，強制 Hibernate
  使用帶有底線的表名。
*/
@Entity
@Table(name = "order_item")
public class OrderItem {

    @Id
    public Long itemId;
    public Long orderId;
    public Integer userId;
    public String productName;
    public Integer price;
}
