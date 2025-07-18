using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace test_c__new.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IAsyncRepository<Basket>> _mockBasketRepository;
        private readonly Mock<IAsyncRepository<CatalogItem>> _mockItemRepository;
        private readonly Mock<IAsyncRepository<Order>> _mockOrderRepository;
        private readonly Mock<IUriComposer> _mockUriComposer;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _mockBasketRepository = new Mock<IAsyncRepository<Basket>>();
            _mockItemRepository = new Mock<IAsyncRepository<CatalogItem>>();
            _mockOrderRepository = new Mock<IAsyncRepository<Order>>();
            _mockUriComposer = new Mock<IUriComposer>();
            
            _orderService = new OrderService(
                _mockBasketRepository.Object,
                _mockItemRepository.Object,
                _mockOrderRepository.Object,
                _mockUriComposer.Object);
        }

        #region CreateOrderAsync Tests

        [Fact]
        public async Task CreateOrderAsync_ValidBasketAndAddress_CreatesOrder()
        {
            // Arrange
            var basketId = 1;
            var buyerId = "testbuyer";
            var shippingAddress = CreateTestAddress();
            var basket = CreateTestBasket(basketId, buyerId);
            var catalogItem1 = CreateTestCatalogItem(1, "Item 1", 10.99m, "pic1.jpg");
            var catalogItem2 = CreateTestCatalogItem(2, "Item 2", 15.99m, "pic2.jpg");

            basket.AddItem(1, 10.99m, 2);
            basket.AddItem(2, 15.99m, 1);

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);

            _mockItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(catalogItem1);
            _mockItemRepository.Setup(r => r.GetByIdAsync(2))
                .ReturnsAsync(catalogItem2);

            _mockUriComposer.Setup(u => u.ComposePicUri("pic1.jpg"))
                .Returns("http://test.com/pic1.jpg");
            _mockUriComposer.Setup(u => u.ComposePicUri("pic2.jpg"))
                .Returns("http://test.com/pic2.jpg");

            Order capturedOrder = null;
            _mockOrderRepository.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .Callback<Order>(order => capturedOrder = order)
                .Returns(Task.CompletedTask);

            // Act
            await _orderService.CreateOrderAsync(basketId, shippingAddress);

            // Assert
            _mockBasketRepository.Verify(r => r.GetByIdAsync(basketId), Times.Once);
            _mockItemRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
            _mockItemRepository.Verify(r => r.GetByIdAsync(2), Times.Once);
            _mockOrderRepository.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);

            Assert.NotNull(capturedOrder);
            Assert.Equal(buyerId, capturedOrder.BuyerId);
            Assert.Equal(shippingAddress, capturedOrder.ShipToAddress);
            Assert.Equal(2, capturedOrder.OrderItems.Count);
        }

        [Fact]
        public async Task CreateOrderAsync_ValidBasket_CreatesCorrectOrderItems()
        {
            // Arrange
            var basketId = 1;
            var buyerId = "testbuyer";
            var shippingAddress = CreateTestAddress();
            var basket = CreateTestBasket(basketId, buyerId);
            var catalogItem = CreateTestCatalogItem(1, "Test Item", 25.50m, "test.jpg");

            basket.AddItem(1, 25.50m, 3);

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);

            _mockItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(catalogItem);

            _mockUriComposer.Setup(u => u.ComposePicUri("test.jpg"))
                .Returns("http://test.com/test.jpg");

            Order capturedOrder = null;
            _mockOrderRepository.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .Callback<Order>(order => capturedOrder = order)
                .Returns(Task.CompletedTask);

            // Act
            await _orderService.CreateOrderAsync(basketId, shippingAddress);

            // Assert
            Assert.NotNull(capturedOrder);
            Assert.Single(capturedOrder.OrderItems);

            var orderItem = capturedOrder.OrderItems.First();
            Assert.Equal(25.50m, orderItem.UnitPrice);
            Assert.Equal(3, orderItem.Units);
            Assert.Equal(1, orderItem.ItemOrdered.CatalogItemId);
            Assert.Equal("Test Item", orderItem.ItemOrdered.ProductName);
            Assert.Equal("http://test.com/test.jpg", orderItem.ItemOrdered.PictureUri);
        }

        [Fact]
        public async Task CreateOrderAsync_EmptyBasket_CreatesOrderWithNoItems()
        {
            // Arrange
            var basketId = 1;
            var buyerId = "testbuyer";
            var shippingAddress = CreateTestAddress();
            var basket = CreateTestBasket(basketId, buyerId);

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);

            Order capturedOrder = null;
            _mockOrderRepository.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .Callback<Order>(order => capturedOrder = order)
                .Returns(Task.CompletedTask);

            // Act
            await _orderService.CreateOrderAsync(basketId, shippingAddress);

            // Assert
            Assert.NotNull(capturedOrder);
            Assert.Empty(capturedOrder.OrderItems);
            Assert.Equal(buyerId, capturedOrder.BuyerId);
        }

        [Fact]
        public async Task CreateOrderAsync_MultipleItemsSameCatalog_CreatesMultipleOrderItems()
        {
            // Arrange
            var basketId = 1;
            var buyerId = "testbuyer";
            var shippingAddress = CreateTestAddress();
            var basket = CreateTestBasket(basketId, buyerId);
            var catalogItem1 = CreateTestCatalogItem(1, "Item 1", 10.00m, "pic1.jpg");
            var catalogItem2 = CreateTestCatalogItem(2, "Item 2", 20.00m, "pic2.jpg");
            var catalogItem3 = CreateTestCatalogItem(3, "Item 3", 30.00m, "pic3.jpg");

            basket.AddItem(1, 10.00m, 1);
            basket.AddItem(2, 20.00m, 2);
            basket.AddItem(3, 30.00m, 3);

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);

            _mockItemRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(catalogItem1);
            _mockItemRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(catalogItem2);
            _mockItemRepository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(catalogItem3);

            _mockUriComposer.Setup(u => u.ComposePicUri(It.IsAny<string>()))
                .Returns<string>(uri => $"http://test.com/{uri}");

            Order capturedOrder = null;
            _mockOrderRepository.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .Callback<Order>(order => capturedOrder = order)
                .Returns(Task.CompletedTask);

            // Act
            await _orderService.CreateOrderAsync(basketId, shippingAddress);

            // Assert
            Assert.NotNull(capturedOrder);
            Assert.Equal(3, capturedOrder.OrderItems.Count);

            var orderItems = capturedOrder.OrderItems.ToList();
            
            // Verify first item
            Assert.Equal(1, orderItems[0].ItemOrdered.CatalogItemId);
            Assert.Equal("Item 1", orderItems[0].ItemOrdered.ProductName);
            Assert.Equal(10.00m, orderItems[0].UnitPrice);
            Assert.Equal(1, orderItems[0].Units);

            // Verify second item
            Assert.Equal(2, orderItems[1].ItemOrdered.CatalogItemId);
            Assert.Equal("Item 2", orderItems[1].ItemOrdered.ProductName);
            Assert.Equal(20.00m, orderItems[1].UnitPrice);
            Assert.Equal(2, orderItems[1].Units);

            // Verify third item
            Assert.Equal(3, orderItems[2].ItemOrdered.CatalogItemId);
            Assert.Equal("Item 3", orderItems[2].ItemOrdered.ProductName);
            Assert.Equal(30.00m, orderItems[2].UnitPrice);
            Assert.Equal(3, orderItems[2].Units);
        }

        [Fact]
        public async Task CreateOrderAsync_UriComposerCalled_ForEachItem()
        {
            // Arrange
            var basketId = 1;
            var buyerId = "testbuyer";
            var shippingAddress = CreateTestAddress();
            var basket = CreateTestBasket(basketId, buyerId);
            var catalogItem1 = CreateTestCatalogItem(1, "Item 1", 10.00m, "pic1.jpg");
            var catalogItem2 = CreateTestCatalogItem(2, "Item 2", 20.00m, "pic2.jpg");

            basket.AddItem(1, 10.00m, 1);
            basket.AddItem(2, 20.00m, 1);

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);

            _mockItemRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(catalogItem1);
            _mockItemRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(catalogItem2);

            _mockUriComposer.Setup(u => u.ComposePicUri("pic1.jpg"))
                .Returns("http://test.com/pic1.jpg");
            _mockUriComposer.Setup(u => u.ComposePicUri("pic2.jpg"))
                .Returns("http://test.com/pic2.jpg");

            // Act
            await _orderService.CreateOrderAsync(basketId, shippingAddress);

            // Assert
            _mockUriComposer.Verify(u => u.ComposePicUri("pic1.jpg"), Times.Once);
            _mockUriComposer.Verify(u => u.ComposePicUri("pic2.jpg"), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_NullBasket_ThrowsException()
        {
            // Arrange
            var basketId = 1;
            var shippingAddress = CreateTestAddress();

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync((Basket)null);

            // Act & Assert
            await Assert.ThrowsAsync<System.ArgumentNullException>(() => 
                _orderService.CreateOrderAsync(basketId, shippingAddress));
        }

        #endregion

        #region Additional Edge Case Tests

        [Fact]
        public async Task CreateOrderAsync_BasketWithZeroPriceItems_CreatesOrderCorrectly()
        {
            // Arrange
            var basketId = 1;
            var buyerId = "testbuyer";
            var shippingAddress = CreateTestAddress();
            var basket = CreateTestBasket(basketId, buyerId);
            var catalogItem = CreateTestCatalogItem(1, "Free Item", 0.00m, "free.jpg");

            basket.AddItem(1, 0.00m, 1);

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);
            _mockItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(catalogItem);
            _mockUriComposer.Setup(u => u.ComposePicUri("free.jpg"))
                .Returns("http://test.com/free.jpg");

            Order capturedOrder = null;
            _mockOrderRepository.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .Callback<Order>(order => capturedOrder = order)
                .Returns(Task.CompletedTask);

            // Act
            await _orderService.CreateOrderAsync(basketId, shippingAddress);

            // Assert
            Assert.NotNull(capturedOrder);
            Assert.Single(capturedOrder.OrderItems);
            Assert.Equal(0.00m, capturedOrder.OrderItems.First().UnitPrice);
        }

        [Fact]
        public async Task CreateOrderAsync_BasketWithLargeQuantity_CreatesOrderCorrectly()
        {
            // Arrange
            var basketId = 1;
            var buyerId = "testbuyer";
            var shippingAddress = CreateTestAddress();
            var basket = CreateTestBasket(basketId, buyerId);
            var catalogItem = CreateTestCatalogItem(1, "Bulk Item", 5.99m, "bulk.jpg");

            basket.AddItem(1, 5.99m, 1000);

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);
            _mockItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(catalogItem);
            _mockUriComposer.Setup(u => u.ComposePicUri("bulk.jpg"))
                .Returns("http://test.com/bulk.jpg");

            Order capturedOrder = null;
            _mockOrderRepository.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .Callback<Order>(order => capturedOrder = order)
                .Returns(Task.CompletedTask);

            // Act
            await _orderService.CreateOrderAsync(basketId, shippingAddress);

            // Assert
            Assert.NotNull(capturedOrder);
            Assert.Single(capturedOrder.OrderItems);
            Assert.Equal(1000, capturedOrder.OrderItems.First().Units);
        }

        [Fact]
        public async Task CreateOrderAsync_CatalogItemWithNullPictureUri_HandlesCorrectly()
        {
            // Arrange
            var basketId = 1;
            var buyerId = "testbuyer";
            var shippingAddress = CreateTestAddress();
            var basket = CreateTestBasket(basketId, buyerId);
            var catalogItem = CreateTestCatalogItem(1, "No Picture Item", 15.99m, null);

            basket.AddItem(1, 15.99m, 1);

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);
            _mockItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(catalogItem);
            _mockUriComposer.Setup(u => u.ComposePicUri(null))
                .Returns("http://test.com/default.jpg");

            Order capturedOrder = null;
            _mockOrderRepository.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .Callback<Order>(order => capturedOrder = order)
                .Returns(Task.CompletedTask);

            // Act
            await _orderService.CreateOrderAsync(basketId, shippingAddress);

            // Assert
            Assert.NotNull(capturedOrder);
            _mockUriComposer.Verify(u => u.ComposePicUri(null), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_CatalogItemWithEmptyPictureUri_HandlesCorrectly()
        {
            // Arrange
            var basketId = 1;
            var buyerId = "testbuyer";
            var shippingAddress = CreateTestAddress();
            var basket = CreateTestBasket(basketId, buyerId);
            var catalogItem = CreateTestCatalogItem(1, "Empty Picture Item", 15.99m, "");

            basket.AddItem(1, 15.99m, 1);

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);
            _mockItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(catalogItem);
            _mockUriComposer.Setup(u => u.ComposePicUri(""))
                .Returns("http://test.com/default.jpg");

            // Act
            await _orderService.CreateOrderAsync(basketId, shippingAddress);

            // Assert
            _mockUriComposer.Verify(u => u.ComposePicUri(""), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_UriComposerReturnsNull_HandlesCorrectly()
        {
            // Arrange
            var basketId = 1;
            var buyerId = "testbuyer";
            var shippingAddress = CreateTestAddress();
            var basket = CreateTestBasket(basketId, buyerId);
            var catalogItem = CreateTestCatalogItem(1, "Item", 15.99m, "pic.jpg");

            basket.AddItem(1, 15.99m, 1);

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);
            _mockItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(catalogItem);
            _mockUriComposer.Setup(u => u.ComposePicUri("pic.jpg"))
                .Returns((string)null);

            Order capturedOrder = null;
            _mockOrderRepository.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .Callback<Order>(order => capturedOrder = order)
                .Returns(Task.CompletedTask);

            // Act
            await _orderService.CreateOrderAsync(basketId, shippingAddress);

            // Assert
            Assert.NotNull(capturedOrder);
            Assert.Single(capturedOrder.OrderItems);
            Assert.Null(capturedOrder.OrderItems.First().ItemOrdered.PictureUri);
        }

        [Fact]
        public async Task CreateOrderAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var basketId = 1;
            var shippingAddress = CreateTestAddress();

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ThrowsAsync(new InvalidOperationException("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _orderService.CreateOrderAsync(basketId, shippingAddress));
        }

        [Fact]
        public async Task CreateOrderAsync_ItemRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var basketId = 1;
            var buyerId = "testbuyer";
            var shippingAddress = CreateTestAddress();
            var basket = CreateTestBasket(basketId, buyerId);

            basket.AddItem(1, 15.99m, 1);

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);
            _mockItemRepository.Setup(r => r.GetByIdAsync(1))
                .ThrowsAsync(new InvalidOperationException("Item repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _orderService.CreateOrderAsync(basketId, shippingAddress));
        }

        [Fact]
        public async Task CreateOrderAsync_OrderRepositoryAddAsyncThrowsException_PropagatesException()
        {
            // Arrange
            var basketId = 1;
            var buyerId = "testbuyer";
            var shippingAddress = CreateTestAddress();
            var basket = CreateTestBasket(basketId, buyerId);
            var catalogItem = CreateTestCatalogItem(1, "Item", 15.99m, "pic.jpg");

            basket.AddItem(1, 15.99m, 1);

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);
            _mockItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(catalogItem);
            _mockUriComposer.Setup(u => u.ComposePicUri("pic.jpg"))
                .Returns("http://test.com/pic.jpg");
            _mockOrderRepository.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .ThrowsAsync(new InvalidOperationException("Order repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _orderService.CreateOrderAsync(basketId, shippingAddress));
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public async Task CreateOrderAsync_WithZeroPriceItems_CreatesOrderSuccessfully()
        {
            // Arrange
            var basketId = 1;
            var shippingAddress = CreateTestAddress();
            var basket = CreateTestBasket(basketId, "testuser@example.com");

            var catalogItem = CreateTestCatalogItem(1, "Free Item", 0.00m, "free-item.jpg");
            basket.AddItem(catalogItem.Id, 0.00m, 1);

            _mockBasketRepository.Setup(r => r.GetByIdWithItemsAsync(basketId))
                .ReturnsAsync(basket);

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(catalogItem);

            Order capturedOrder = null;
            _mockOrderRepository.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .Callback<Order>(order => capturedOrder = order)
                .Returns(Task.CompletedTask);

            // Act
            await _orderService.CreateOrderAsync(basketId, shippingAddress);

            // Assert
            Assert.NotNull(capturedOrder);
            Assert.Equal(0.00m, capturedOrder.Total());
            Assert.Single(capturedOrder.OrderItems);
            Assert.Equal(0.00m, capturedOrder.OrderItems.First().UnitPrice);
        }

        [Fact]
        public async Task CreateOrderAsync_WithLargeQuantity_CreatesOrderSuccessfully()
        {
            // Arrange
            var basketId = 1;
            var shippingAddress = CreateTestAddress();
            var basket = CreateTestBasket(basketId, "testuser@example.com");

            var catalogItem = CreateTestCatalogItem(1, "Bulk Item", 1.00m, "bulk-item.jpg");
            basket.AddItem(catalogItem.Id, 1.00m, 1000);

            _mockBasketRepository.Setup(r => r.GetByIdWithItemsAsync(basketId))
                .ReturnsAsync(basket);

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(catalogItem);

            Order capturedOrder = null;
            _mockOrderRepository.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .Callback<Order>(order => capturedOrder = order)
                .Returns(Task.CompletedTask);

            // Act
            await _orderService.CreateOrderAsync(basketId, shippingAddress);

            // Assert
            Assert.NotNull(capturedOrder);
            Assert.Equal(1000.00m, capturedOrder.Total());
            Assert.Equal(1000, capturedOrder.OrderItems.First().Units);
        }

        [Fact]
        public async Task CreateOrderAsync_WithHighPriceItems_CreatesOrderSuccessfully()
        {
            // Arrange
            var basketId = 1;
            var shippingAddress = CreateTestAddress();
            var basket = CreateTestBasket(basketId, "testuser@example.com");

            var catalogItem = CreateTestCatalogItem(1, "Expensive Item", 999999.99m, "expensive-item.jpg");
            basket.AddItem(catalogItem.Id, 999999.99m, 1);

            _mockBasketRepository.Setup(r => r.GetByIdWithItemsAsync(basketId))
                .ReturnsAsync(basket);

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(catalogItem);

            Order capturedOrder = null;
            _mockOrderRepository.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .Callback<Order>(order => capturedOrder = order)
                .Returns(Task.CompletedTask);

            // Act
            await _orderService.CreateOrderAsync(basketId, shippingAddress);

            // Assert
            Assert.NotNull(capturedOrder);
            Assert.Equal(999999.99m, capturedOrder.Total());
            Assert.Equal(999999.99m, capturedOrder.OrderItems.First().UnitPrice);
        }

        [Fact]
        public async Task CreateOrderAsync_WithSpecialCharactersInAddress_CreatesOrderSuccessfully()
        {
            // Arrange
            var basketId = 1;
            var specialAddress = new Address("123 Rúe de là Paix", "São Paulo", "SP", "Brésil", "01234-567");
            var basket = CreateTestBasket(basketId, "testuser@example.com");

            var catalogItem = CreateTestCatalogItem(1, "Test Item", 10.00m, "test-item.jpg");
            basket.AddItem(catalogItem.Id, 10.00m, 1);

            _mockBasketRepository.Setup(r => r.GetByIdWithItemsAsync(basketId))
                .ReturnsAsync(basket);

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(catalogItem);

            Order capturedOrder = null;
            _mockOrderRepository.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .Callback<Order>(order => capturedOrder = order)
                .Returns(Task.CompletedTask);

            // Act
            await _orderService.CreateOrderAsync(basketId, specialAddress);

            // Assert
            Assert.NotNull(capturedOrder);
            Assert.Equal("123 Rúe de là Paix", capturedOrder.ShipToAddress.Street);
            Assert.Equal("São Paulo", capturedOrder.ShipToAddress.City);
            Assert.Equal("Brésil", capturedOrder.ShipToAddress.Country);
        }

        [Fact]
        public async Task CreateOrderAsync_OrderRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var basketId = 1;
            var shippingAddress = CreateTestAddress();
            var basket = CreateTestBasket(basketId, "testuser@example.com");

            var catalogItem = CreateTestCatalogItem(1, "Test Item", 10.00m, "test-item.jpg");
            basket.AddItem(catalogItem.Id, 10.00m, 1);

            _mockBasketRepository.Setup(r => r.GetByIdWithItemsAsync(basketId))
                .ReturnsAsync(basket);

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(catalogItem);

            _mockOrderRepository.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _orderService.CreateOrderAsync(basketId, shippingAddress));
        }

        [Fact]
        public async Task CreateOrderAsync_CatalogItemRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var basketId = 1;
            var shippingAddress = CreateTestAddress();
            var basket = CreateTestBasket(basketId, "testuser@example.com");

            basket.AddItem(1, 10.00m, 1);

            _mockBasketRepository.Setup(r => r.GetByIdWithItemsAsync(basketId))
                .ReturnsAsync(basket);

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ThrowsAsync(new ArgumentException("Catalog item not found"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _orderService.CreateOrderAsync(basketId, shippingAddress));
        }

        [Fact]
        public async Task CreateOrderAsync_WithMaxIntBasketId_HandlesCorrectly()
        {
            // Arrange
            var basketId = int.MaxValue;
            var shippingAddress = CreateTestAddress();
            
            _mockBasketRepository.Setup(r => r.GetByIdWithItemsAsync(basketId))
                .ReturnsAsync((Basket)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _orderService.CreateOrderAsync(basketId, shippingAddress));
        }

        [Fact]
        public async Task CreateOrderAsync_WithNegativeBasketId_HandlesCorrectly()
        {
            // Arrange
            var basketId = -1;
            var shippingAddress = CreateTestAddress();
            
            _mockBasketRepository.Setup(r => r.GetByIdWithItemsAsync(basketId))
                .ReturnsAsync((Basket)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _orderService.CreateOrderAsync(basketId, shippingAddress));
        }

        [Fact]
        public async Task CreateOrderAsync_WithVeryLongProductName_CreatesOrderSuccessfully()
        {
            // Arrange
            var basketId = 1;
            var shippingAddress = CreateTestAddress();
            var basket = CreateTestBasket(basketId, "testuser@example.com");

            var longProductName = new string('A', 1000); // Very long product name
            var catalogItem = CreateTestCatalogItem(1, longProductName, 10.00m, "test-item.jpg");
            basket.AddItem(catalogItem.Id, 10.00m, 1);

            _mockBasketRepository.Setup(r => r.GetByIdWithItemsAsync(basketId))
                .ReturnsAsync(basket);

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(catalogItem);

            Order capturedOrder = null;
            _mockOrderRepository.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .Callback<Order>(order => capturedOrder = order)
                .Returns(Task.CompletedTask);

            // Act
            await _orderService.CreateOrderAsync(basketId, shippingAddress);

            // Assert
            Assert.NotNull(capturedOrder);
            Assert.Equal(longProductName, capturedOrder.OrderItems.First().ItemOrdered.ProductName);
        }

        #endregion

        #region Helper Methods

        private Basket CreateTestBasket(int id, string buyerId)
        {
            var basket = new Basket(buyerId);
            // Using reflection to set the Id since it's typically set by the database
            typeof(Basket).BaseType.GetProperty("Id").SetValue(basket, id);
            return basket;
        }

        private CatalogItem CreateTestCatalogItem(int id, string name, decimal price, string pictureUri)
        {
            var catalogItem = new CatalogItem(1, 1, name, name, price, pictureUri);
            // Using reflection to set the Id since it's typically set by the database
            typeof(CatalogItem).BaseType.GetProperty("Id").SetValue(catalogItem, id);
            return catalogItem;
        }

        private Address CreateTestAddress()
        {
            return new Address("123 Main St", "Anytown", "ST", "USA", "12345");
        }

        #endregion
    }
}
