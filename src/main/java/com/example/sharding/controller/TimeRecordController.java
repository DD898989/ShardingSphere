package com.example.sharding.controller;

import com.example.sharding.entity.TimeRecord;
import com.example.sharding.repository.TimeRecordRepository;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.format.annotation.DateTimeFormat;
import org.springframework.web.bind.annotation.*;

import java.time.LocalDateTime;
import java.util.List;

@RestController
@RequestMapping("/api/records")
public class TimeRecordController {

    @Autowired
    private TimeRecordRepository timeRecordRepository;

    @PostMapping
    public TimeRecord createRecord(
            @RequestParam(required = false) Long id,
            @RequestParam @DateTimeFormat(pattern = "yyyy-MM-dd HH:mm:ss") LocalDateTime myTime,
            @RequestParam String data) {
        TimeRecord record = new TimeRecord();
        if (id != null) {
            record.id = id;
        } else {
            record.id = System.nanoTime() & 0x7fffffffffffffffL;
        }
        record.myTime = myTime;
        record.data = data;
        return timeRecordRepository.save(record);
    }

    @PutMapping("/{id}")
    public TimeRecord updateRecord(
            @PathVariable Long id,
            @RequestParam @DateTimeFormat(pattern = "yyyy-MM-dd HH:mm:ss") LocalDateTime myTime,
            @RequestParam String data) {
        TimeRecord record = timeRecordRepository.findById(id).orElse(null);
        record.myTime = myTime;
        record.data = data;
        return timeRecordRepository.save(record);
    }

    @GetMapping
    public List<TimeRecord> getAllRecords() {
        return timeRecordRepository.findAll();
    }
}
