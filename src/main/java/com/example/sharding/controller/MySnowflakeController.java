package com.example.sharding.controller;

import com.example.sharding.entity.MySnowflake;
import com.example.sharding.repository.MySnowflakeRepository;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/snowflakes")
public class MySnowflakeController {

    @Autowired
    private MySnowflakeRepository mySnowflakeRepository;

    @PostMapping
    public MySnowflake createSnowflake(@RequestBody MySnowflake snowflake) {
        // ID is left null so ShardingSphere Snowflake key generator can generate it.
        snowflake.id = null;
        return mySnowflakeRepository.save(snowflake);
    }

    @GetMapping("/{id}")
    public MySnowflake getSnowflakeById(@PathVariable Long id) {
        return mySnowflakeRepository.findById(id).orElse(null);
    }

    @GetMapping
    public List<MySnowflake> getAllSnowflakes() {
        return mySnowflakeRepository.findAll();
    }
}
