package com.example.sharding.repository;

import com.example.sharding.entity.MySnowflake;
import org.springframework.data.jpa.repository.JpaRepository;

public interface MySnowflakeRepository extends JpaRepository<MySnowflake, Long> {
}
