package com.example.sharding.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.Id;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Table;
import org.hibernate.annotations.DynamicUpdate;
import java.time.LocalDateTime;

@Entity
@DynamicUpdate
public class TimeRecord {

    @Id
    public Long id;
    public LocalDateTime myTime;
    public String data;
}
