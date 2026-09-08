package com.example.sharding.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.Id;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import org.hibernate.annotations.DynamicUpdate;

@Entity
@DynamicUpdate
public class Order {

    @Id
    public Long orderId;
    public Integer userId;
    public String status;
}
