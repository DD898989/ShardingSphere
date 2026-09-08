package com.example.sharding.controller;

import com.example.sharding.entity.Article;
import com.example.sharding.repository.ArticleRepository;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/articles")
public class ArticleController {

    @Autowired
    private ArticleRepository articleRepository;

    @PostMapping
    public Article createArticle(@RequestBody Article article) {
        article.contentUserId = article.userId;
        return articleRepository.save(article);
    }

    @GetMapping("/{id}")
    public Article getArticleById(@PathVariable Long id) {
        return articleRepository.findById(id).orElse(null);
    }

    @PutMapping("/{id}")
    public Article updateArticle(@PathVariable Long id, @RequestParam String title, @RequestParam String content) {
        Article article = articleRepository.findById(id).orElse(null);
        article.title = title;
        article.content = content;
        article.contentUserId = article.userId;
        return articleRepository.save(article);
    }

    @GetMapping
    public List<Article> getAllArticles() {
        return articleRepository.findAll();
    }
}
