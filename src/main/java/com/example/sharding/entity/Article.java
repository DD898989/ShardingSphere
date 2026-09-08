package com.example.sharding.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.Table;
import jakarta.persistence.SecondaryTable;
import jakarta.persistence.PrimaryKeyJoinColumn;
import jakarta.persistence.Id;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Column;
import org.hibernate.annotations.DynamicUpdate;

@Entity
@DynamicUpdate
@Table(name = "article")
@SecondaryTable(name = "article_content", pkJoinColumns = @PrimaryKeyJoinColumn(name = "article_id"))
public class Article {

    @Id
    public Long articleId;

    public String title;
    
    public Integer userId;

    @Column(table = "article_content")
    public Integer contentUserId;

    @Column(table = "article_content")
    public String content;
}
