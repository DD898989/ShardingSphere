package com.example.sharding.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import org.hibernate.annotations.DynamicUpdate;

@Entity
@DynamicUpdate
@Table(name = "country")
public class Country {

    @Id
    public Long id;

    public String name;

    public String code;
}
