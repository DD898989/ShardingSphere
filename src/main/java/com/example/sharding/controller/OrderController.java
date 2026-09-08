package com.example.sharding.controller;

import com.example.sharding.entity.Order;
import com.example.sharding.entity.OrderItem;
import com.example.sharding.repository.OrderRepository;
import com.example.sharding.repository.OrderItemRepository;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.transaction.annotation.Transactional;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/orders")
public class OrderController {

    @Autowired
    private OrderRepository orderRepository;

    @Autowired
    private OrderItemRepository orderItemRepository;

    @PostMapping
    @Transactional
    public OrderDTO createOrder(@RequestBody OrderCreationRequest request) {
        Order order = new Order();
        order.orderId = request.orderId;
        order.userId = request.userId;
        order.status = request.status;
        order = orderRepository.save(order);

        for (ItemRequest itemReq : request.items) {
            OrderItem item = new OrderItem();
            item.itemId = itemReq.itemId;
            item.orderId = order.orderId;
            item.userId = order.userId;
            item.productName = itemReq.productName;
            item.price = itemReq.price;
            orderItemRepository.save(item);
        }

        List<OrderItem> items = orderItemRepository.findByOrderId(order.orderId);
        return new OrderDTO(order, items);
    }

    @GetMapping("/{id}")
    public OrderDTO getOrderById(@PathVariable Long id) {
        Order order = orderRepository.findById(id).orElse(null);
        List<OrderItem> items = orderItemRepository.findByOrderId(id);
        return new OrderDTO(order, items);
    }

    @PutMapping("/{id}")
    @Transactional
    public OrderDTO updateOrder(@PathVariable Long id, @RequestParam String status) {
        Order order = orderRepository.findById(id).orElse(null);
        order.status = status;
        orderRepository.save(order);
        List<OrderItem> items = orderItemRepository.findByOrderId(id);
        return new OrderDTO(order, items);
    }

    public static class OrderCreationRequest {
        public Long orderId;
        public Integer userId;
        public String status;
        public List<ItemRequest> items;
    }

    public static class ItemRequest {
        public Long itemId;
        public String productName;
        public Integer price;
    }

    public static class OrderDTO {
        public Order order;
        public List<OrderItem> items;
        public OrderDTO(Order order, List<OrderItem> items) {
            this.order = order;
            this.items = items;
        }
    }
}
