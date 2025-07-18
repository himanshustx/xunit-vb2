using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace test_c__new.Services
{
    public class OrderRepositoryTests : IDisposable
    {
        private readonly CatalogContext _context;
        private readonly OrderRepository _orderRepository;

        public OrderRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<CatalogContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CatalogContext(options);
            _orderRepository = new OrderRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        #region GetByIdWithItemsAsync Tests

        [Fact]
        public async Task GetByIdWithItemsAsync_ExistingOrderWithItems_ReturnsOrderWithItems()
        {
            // Arrange
            var address = CreateTestAddress();
            var orderItems = new List<OrderItem>
            {
                CreateTestOrderItem(1, "Item 1", 10.99m, 1),
                CreateTestOrderItem(2, "Item 2", 15.99m, 2)
            };

            var order = new Order("testuser@example.com", address, orderItems);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var orderId = order.Id;

            // Act
            var result = await _orderRepository.GetByIdWithItemsAsync(orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderId, result.Id);
            Assert.Equal("testuser@example.com", result.BuyerId);
            Assert.NotNull(result.OrderItems);
            Assert.Equal(2, result.OrderItems.Count);

            var orderItemsList = result.OrderItems.ToList();
            Assert.Equal("Item 1", orderItemsList[0].ItemOrdered.ProductName);
            Assert.Equal("Item 2", orderItemsList[1].ItemOrdered.ProductName);
            Assert.Equal(10.99m, orderItemsList[0].UnitPrice);
            Assert.Equal(15.99m, orderItemsList[1].UnitPrice);
        }

        [Fact]
        public async Task GetByIdWithItemsAsync_ExistingOrderWithoutItems_ReturnsOrderWithEmptyItems()
        {
            // Arrange
            var address = CreateTestAddress();
            var order = new Order("testuser@example.com", address, new List<OrderItem>());
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var orderId = order.Id;

            // Act
            var result = await _orderRepository.GetByIdWithItemsAsync(orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderId, result.Id);
            Assert.Equal("testuser@example.com", result.BuyerId);
            Assert.NotNull(result.OrderItems);
            Assert.Empty(result.OrderItems);
        }

        [Fact]
        public async Task GetByIdWithItemsAsync_NonExistingOrder_ReturnsNull()
        {
            // Act
            var result = await _orderRepository.GetByIdWithItemsAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdWithItemsAsync_ZeroId_ReturnsNull()
        {
            // Act
            var result = await _orderRepository.GetByIdWithItemsAsync(0);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdWithItemsAsync_NegativeId_ReturnsNull()
        {
            // Act
            var result = await _orderRepository.GetByIdWithItemsAsync(-1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdWithItemsAsync_OrderWithManyItems_ReturnsAllItems()
        {
            // Arrange
            var address = CreateTestAddress();
            var orderItems = Enumerable.Range(1, 10)
                .Select(i => CreateTestOrderItem(i, $"Item {i}", i * 5.99m, i))
                .ToList();

            var order = new Order("testuser@example.com", address, orderItems);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var orderId = order.Id;

            // Act
            var result = await _orderRepository.GetByIdWithItemsAsync(orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderId, result.Id);
            Assert.NotNull(result.OrderItems);
            Assert.Equal(10, result.OrderItems.Count);

            foreach (var item in result.OrderItems)
            {
                Assert.NotNull(item.ItemOrdered);
                Assert.NotEmpty(item.ItemOrdered.ProductName);
                Assert.True(item.UnitPrice > 0);
            }
        }

        #endregion

        #region Integration with Base Repository Tests

        [Fact]
        public async Task AddAsync_ValidOrder_AddsSuccessfully()
        {
            // Arrange
            var address = CreateTestAddress();
            var orderItems = new List<OrderItem>
            {
                CreateTestOrderItem(1, "Test Item", 25.99m, 1)
            };

            var order = new Order("testuser@example.com", address, orderItems);

            // Act
            await _orderRepository.AddAsync(order);

            // Assert
            var savedOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.BuyerId == "testuser@example.com");

            Assert.NotNull(savedOrder);
            Assert.Equal("testuser@example.com", savedOrder.BuyerId);
            Assert.Single(savedOrder.OrderItems);
        }

        [Fact]
        public async Task UpdateAsync_ExistingOrder_UpdatesSuccessfully()
        {
            // Arrange
            var address = CreateTestAddress();
            var orderItems = new List<OrderItem>
            {
                CreateTestOrderItem(1, "Original Item", 10.99m, 1)
            };

            var order = new Order("original@example.com", address, orderItems);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Modify the order (note: in real scenarios, you'd have methods to update the order)
            var existingOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstAsync(o => o.Id == order.Id);

            // Act
            await _orderRepository.UpdateAsync(existingOrder);

            // Assert
            var updatedOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstAsync(o => o.Id == order.Id);

            Assert.NotNull(updatedOrder);
            Assert.Equal(order.Id, updatedOrder.Id);
        }

        [Fact]
        public async Task DeleteAsync_ExistingOrder_DeletesSuccessfully()
        {
            // Arrange
            var address = CreateTestAddress();
            var orderItems = new List<OrderItem>
            {
                CreateTestOrderItem(1, "Item to Delete", 15.99m, 1)
            };

            var order = new Order("delete@example.com", address, orderItems);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var orderId = order.Id;

            // Act
            await _orderRepository.DeleteAsync(order);

            // Assert
            var deletedOrder = await _context.Orders.FindAsync(orderId);
            Assert.Null(deletedOrder);
        }

        [Fact]
        public async Task ListAllAsync_MultipleOrders_ReturnsAllOrders()
        {
            // Arrange
            var address = CreateTestAddress();
            var orders = new[]
            {
                new Order("user1@example.com", address, new List<OrderItem> { CreateTestOrderItem(1, "Item 1", 10.99m, 1) }),
                new Order("user2@example.com", address, new List<OrderItem> { CreateTestOrderItem(2, "Item 2", 15.99m, 1) }),
                new Order("user3@example.com", address, new List<OrderItem> { CreateTestOrderItem(3, "Item 3", 20.99m, 1) })
            };

            _context.Orders.AddRange(orders);
            await _context.SaveChangesAsync();

            // Act
            var result = await _orderRepository.ListAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            Assert.Contains(result, o => o.BuyerId == "user1@example.com");
            Assert.Contains(result, o => o.BuyerId == "user2@example.com");
            Assert.Contains(result, o => o.BuyerId == "user3@example.com");
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public async Task GetByIdWithItemsAsync_OrderWithMaxIntId_HandlesCorrectly()
        {
            // Arrange
            var address = CreateTestAddress();
            var orderItems = new List<OrderItem>
            {
                CreateTestOrderItem(1, "Max ID Item", 99.99m, 1)
            };

            var order = new Order("maxid@example.com", address, orderItems);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Act
            var result = await _orderRepository.GetByIdWithItemsAsync(int.MaxValue);

            // Assert
            Assert.Null(result); // Should not find an order with max int ID
        }

        [Fact]
        public async Task GetByIdWithItemsAsync_OrderWithSpecialCharactersInBuyerId_ReturnsCorrectly()
        {
            // Arrange
            var address = CreateTestAddress();
            var orderItems = new List<OrderItem>
            {
                CreateTestOrderItem(1, "Special Item", 25.99m, 1)
            };

            var specialBuyerId = "special+user@domain-name.co.uk";
            var order = new Order(specialBuyerId, address, orderItems);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var orderId = order.Id;

            // Act
            var result = await _orderRepository.GetByIdWithItemsAsync(orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(specialBuyerId, result.BuyerId);
            Assert.Single(result.OrderItems);
        }

        [Fact]
        public async Task GetByIdWithItemsAsync_OrderWithLargeNumberOfItems_ReturnsAllItems()
        {
            // Arrange
            var address = CreateTestAddress();
            var orderItems = Enumerable.Range(1, 100)
                .Select(i => CreateTestOrderItem(i, $"Bulk Item {i}", i * 1.99m, 1))
                .ToList();

            var order = new Order("bulk@example.com", address, orderItems);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var orderId = order.Id;

            // Act
            var result = await _orderRepository.GetByIdWithItemsAsync(orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderId, result.Id);
            Assert.Equal(100, result.OrderItems.Count);

            // Verify all items are loaded with their details
            foreach (var item in result.OrderItems)
            {
                Assert.NotNull(item.ItemOrdered);
                Assert.NotEmpty(item.ItemOrdered.ProductName);
                Assert.True(item.UnitPrice > 0);
            }
        }

        [Fact]
        public async Task GetByIdWithItemsAsync_OrderWithItemsHavingDifferentPrices_LoadsCorrectPrices()
        {
            // Arrange
            var address = CreateTestAddress();
            var orderItems = new List<OrderItem>
            {
                CreateTestOrderItem(1, "Cheap Item", 0.01m, 1),
                CreateTestOrderItem(2, "Expensive Item", 999.99m, 1),
                CreateTestOrderItem(3, "Free Item", 0.00m, 1)
            };

            var order = new Order("prices@example.com", address, orderItems);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var orderId = order.Id;

            // Act
            var result = await _orderRepository.GetByIdWithItemsAsync(orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.OrderItems.Count);

            var itemsList = result.OrderItems.ToList();
            Assert.Contains(itemsList, item => item.UnitPrice == 0.01m);
            Assert.Contains(itemsList, item => item.UnitPrice == 999.99m);
            Assert.Contains(itemsList, item => item.UnitPrice == 0.00m);
        }

        [Fact]
        public async Task GetByIdWithItemsAsync_ConcurrentAccess_HandlesCorrectly()
        {
            // Arrange
            var address = CreateTestAddress();
            var orderItems = new List<OrderItem>
            {
                CreateTestOrderItem(1, "Concurrent Item", 15.99m, 1)
            };

            var order = new Order("concurrent@example.com", address, orderItems);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var orderId = order.Id;

            // Act - Simulate concurrent access
            var task1 = _orderRepository.GetByIdWithItemsAsync(orderId);
            var task2 = _orderRepository.GetByIdWithItemsAsync(orderId);
            var task3 = _orderRepository.GetByIdWithItemsAsync(orderId);

            var results = await Task.WhenAll(task1, task2, task3);

            // Assert
            Assert.All(results, result =>
            {
                Assert.NotNull(result);
                Assert.Equal(orderId, result.Id);
                Assert.Single(result.OrderItems);
            });
        }

        #endregion

        #region Helper Methods

        private Address CreateTestAddress()
        {
            return new Address("123 Test St", "Test City", "TS", "Test Country", "12345");
        }

        private OrderItem CreateTestOrderItem(int catalogItemId, string productName, decimal unitPrice, int units)
        {
            var itemOrdered = new CatalogItemOrdered(catalogItemId, productName, $"pic{catalogItemId}.jpg");
            return new OrderItem(itemOrdered, unitPrice, units);
        }

        #endregion
    }
}
