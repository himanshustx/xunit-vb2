using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace test_c__new.Services
{
    public class BasketServiceTests
    {
        private readonly Mock<IAsyncRepository<Basket>> _mockBasketRepository;
        private readonly Mock<IAppLogger<BasketService>> _mockLogger;
        private readonly BasketService _basketService;

        public BasketServiceTests()
        {
            _mockBasketRepository = new Mock<IAsyncRepository<Basket>>();
            _mockLogger = new Mock<IAppLogger<BasketService>>();
            _basketService = new BasketService(_mockBasketRepository.Object, _mockLogger.Object);
        }

        #region AddItemToBasket Tests

        [Fact]
        public async Task AddItemToBasket_ValidParameters_CallsRepositoryMethods()
        {
            // Arrange
            var basketId = 1;
            var catalogItemId = 10;
            var price = 25.99m;
            var quantity = 2;
            var basket = CreateTestBasket(basketId, "testuser");

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);

            // Act
            await _basketService.AddItemToBasket(basketId, catalogItemId, price, quantity);

            // Assert
            _mockBasketRepository.Verify(r => r.GetByIdAsync(basketId), Times.Once);
            _mockBasketRepository.Verify(r => r.UpdateAsync(basket), Times.Once);
            Assert.Contains(basket.Items, item => item.CatalogItemId == catalogItemId);
        }

        [Fact]
        public async Task AddItemToBasket_DefaultQuantity_AddsOneItem()
        {
            // Arrange
            var basketId = 1;
            var catalogItemId = 10;
            var price = 25.99m;
            var basket = CreateTestBasket(basketId, "testuser");

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);

            // Act
            await _basketService.AddItemToBasket(basketId, catalogItemId, price);

            // Assert
            var addedItem = basket.Items.FirstOrDefault(i => i.CatalogItemId == catalogItemId);
            Assert.NotNull(addedItem);
            Assert.Equal(1, addedItem.Quantity);
        }

        [Fact]
        public async Task AddItemToBasket_ExistingItem_IncreasesQuantity()
        {
            // Arrange
            var basketId = 1;
            var catalogItemId = 10;
            var price = 25.99m;
            var quantity = 2;
            var basket = CreateTestBasket(basketId, "testuser");
            basket.AddItem(catalogItemId, price, 1); // Add initial item

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);

            // Act
            await _basketService.AddItemToBasket(basketId, catalogItemId, price, quantity);

            // Assert
            var item = basket.Items.FirstOrDefault(i => i.CatalogItemId == catalogItemId);
            Assert.NotNull(item);
            Assert.Equal(3, item.Quantity); // 1 + 2
        }

        #endregion

        #region DeleteBasketAsync Tests

        [Fact]
        public async Task DeleteBasketAsync_ValidBasketId_CallsRepositoryMethods()
        {
            // Arrange
            var basketId = 1;
            var basket = CreateTestBasket(basketId, "testuser");

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);

            // Act
            await _basketService.DeleteBasketAsync(basketId);

            // Assert
            _mockBasketRepository.Verify(r => r.GetByIdAsync(basketId), Times.Once);
            _mockBasketRepository.Verify(r => r.DeleteAsync(basket), Times.Once);
        }

        #endregion

        #region GetBasketItemCountAsync Tests

        [Fact]
        public async Task GetBasketItemCountAsync_ValidUsername_ReturnsCorrectCount()
        {
            // Arrange
            var userName = "testuser";
            var basket = CreateTestBasket(1, userName);
            basket.AddItem(1, 10.99m, 2);
            basket.AddItem(2, 15.99m, 3);

            var basketList = new List<Basket> { basket };
            _mockBasketRepository.Setup(r => r.ListAsync(It.IsAny<BasketWithItemsSpecification>()))
                .ReturnsAsync(basketList);

            // Act
            var result = await _basketService.GetBasketItemCountAsync(userName);

            // Assert
            Assert.Equal(5, result); // 2 + 3
            _mockLogger.Verify(
                l => l.LogInformation($"Basket for {userName} has 5 items."),
                Times.Once);
        }

        [Fact]
        public async Task GetBasketItemCountAsync_NoBasket_ReturnsZero()
        {
            // Arrange
            var userName = "testuser";
            var emptyBasketList = new List<Basket>();

            _mockBasketRepository.Setup(r => r.ListAsync(It.IsAny<BasketWithItemsSpecification>()))
                .ReturnsAsync(emptyBasketList);

            // Act
            var result = await _basketService.GetBasketItemCountAsync(userName);

            // Assert
            Assert.Equal(0, result);
            _mockLogger.Verify(
                l => l.LogInformation($"No basket found for {userName}"),
                Times.Once);
        }

        [Fact]
        public async Task GetBasketItemCountAsync_NullUsername_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _basketService.GetBasketItemCountAsync(null));
        }

        [Fact]
        public async Task GetBasketItemCountAsync_EmptyUsername_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _basketService.GetBasketItemCountAsync(string.Empty));
        }

        #endregion

        #region SetQuantities Tests

        [Fact]
        public async Task SetQuantities_ValidQuantities_UpdatesItemQuantities()
        {
            // Arrange
            var basketId = 1;
            var basket = CreateTestBasket(basketId, "testuser");
            basket.AddItem(1, 10.99m, 2);
            basket.AddItem(2, 15.99m, 3);

            var quantities = new Dictionary<string, int>
            {
                { basket.Items.First().Id.ToString(), 5 },
                { basket.Items.Last().Id.ToString(), 1 }
            };

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);

            // Act
            await _basketService.SetQuantities(basketId, quantities);

            // Assert
            Assert.Equal(5, basket.Items.First().Quantity);
            Assert.Equal(1, basket.Items.Last().Quantity);
            _mockBasketRepository.Verify(r => r.UpdateAsync(basket), Times.Once);
        }

        [Fact]
        public async Task SetQuantities_ZeroQuantity_RemovesEmptyItems()
        {
            // Arrange
            var basketId = 1;
            var basket = CreateTestBasket(basketId, "testuser");
            basket.AddItem(1, 10.99m, 2);
            basket.AddItem(2, 15.99m, 3);

            var quantities = new Dictionary<string, int>
            {
                { basket.Items.First().Id.ToString(), 0 },
                { basket.Items.Last().Id.ToString(), 2 }
            };

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);

            // Act
            await _basketService.SetQuantities(basketId, quantities);

            // Assert
            Assert.Single(basket.Items);
            Assert.Equal(2, basket.Items.First().Quantity);
        }

        [Fact]
        public async Task SetQuantities_NullQuantities_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _basketService.SetQuantities(1, null));
        }

        [Fact]
        public async Task SetQuantities_LogsQuantityUpdates()
        {
            // Arrange
            var basketId = 1;
            var basket = CreateTestBasket(basketId, "testuser");
            basket.AddItem(1, 10.99m, 2);

            var quantities = new Dictionary<string, int>
            {
                { basket.Items.First().Id.ToString(), 5 }
            };

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);

            // Act
            await _basketService.SetQuantities(basketId, quantities);

            // Assert
            var itemId = basket.Items.First().Id;
            _mockLogger.Verify(
                l => l.LogInformation($"Updating quantity of item ID:{itemId} to 5."),
                Times.Once);
        }

        #endregion

        #region TransferBasketAsync Tests

        [Fact]
        public async Task TransferBasketAsync_ValidParameters_TransfersBasket()
        {
            // Arrange
            var anonymousId = "anonymous123";
            var userName = "testuser";
            var basket = CreateTestBasket(1, anonymousId);

            var basketList = new List<Basket> { basket };
            _mockBasketRepository.Setup(r => r.ListAsync(It.IsAny<BasketWithItemsSpecification>()))
                .ReturnsAsync(basketList);

            // Act
            await _basketService.TransferBasketAsync(anonymousId, userName);

            // Assert
            Assert.Equal(userName, basket.BuyerId);
            _mockBasketRepository.Verify(r => r.UpdateAsync(basket), Times.Once);
        }

        [Fact]
        public async Task TransferBasketAsync_NoBasketFound_DoesNothing()
        {
            // Arrange
            var anonymousId = "anonymous123";
            var userName = "testuser";
            var emptyBasketList = new List<Basket>();

            _mockBasketRepository.Setup(r => r.ListAsync(It.IsAny<BasketWithItemsSpecification>()))
                .ReturnsAsync(emptyBasketList);

            // Act
            await _basketService.TransferBasketAsync(anonymousId, userName);

            // Assert
            _mockBasketRepository.Verify(r => r.UpdateAsync(It.IsAny<Basket>()), Times.Never);
        }

        [Fact]
        public async Task TransferBasketAsync_NullAnonymousId_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _basketService.TransferBasketAsync(null, "testuser"));
        }

        [Fact]
        public async Task TransferBasketAsync_EmptyAnonymousId_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _basketService.TransferBasketAsync(string.Empty, "testuser"));
        }

        [Fact]
        public async Task TransferBasketAsync_NullUserName_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _basketService.TransferBasketAsync("anonymous123", null));
        }

        [Fact]
        public async Task TransferBasketAsync_EmptyUserName_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _basketService.TransferBasketAsync("anonymous123", string.Empty));
        }

        #endregion

        #region Additional Edge Case Tests

        [Fact]
        public async Task AddItemToBasket_ZeroPrice_AddsItemWithZeroPrice()
        {
            // Arrange
            var basketId = 1;
            var catalogItemId = 10;
            var price = 0m;
            var quantity = 1;
            var basket = CreateTestBasket(basketId, "testuser");

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);

            // Act
            await _basketService.AddItemToBasket(basketId, catalogItemId, price, quantity);

            // Assert
            Assert.Contains(basket.Items, item => item.CatalogItemId == catalogItemId && item.UnitPrice == 0m);
        }

        [Fact]
        public async Task AddItemToBasket_NegativePrice_AddsItemWithNegativePrice()
        {
            // Arrange
            var basketId = 1;
            var catalogItemId = 10;
            var price = -5.99m;
            var quantity = 1;
            var basket = CreateTestBasket(basketId, "testuser");

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);

            // Act
            await _basketService.AddItemToBasket(basketId, catalogItemId, price, quantity);

            // Assert
            Assert.Contains(basket.Items, item => item.CatalogItemId == catalogItemId && item.UnitPrice == -5.99m);
        }

        [Fact]
        public async Task AddItemToBasket_ZeroQuantity_AddsItemWithZeroQuantity()
        {
            // Arrange
            var basketId = 1;
            var catalogItemId = 10;
            var price = 25.99m;
            var quantity = 0;
            var basket = CreateTestBasket(basketId, "testuser");

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);

            // Act
            await _basketService.AddItemToBasket(basketId, catalogItemId, price, quantity);

            // Assert
            Assert.Contains(basket.Items, item => item.CatalogItemId == catalogItemId && item.Quantity == 0);
        }

        [Fact]
        public async Task AddItemToBasket_LargeQuantity_AddsItemWithLargeQuantity()
        {
            // Arrange
            var basketId = 1;
            var catalogItemId = 10;
            var price = 25.99m;
            var quantity = 1000;
            var basket = CreateTestBasket(basketId, "testuser");

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);

            // Act
            await _basketService.AddItemToBasket(basketId, catalogItemId, price, quantity);

            // Assert
            Assert.Contains(basket.Items, item => item.CatalogItemId == catalogItemId && item.Quantity == 1000);
        }

        [Fact]
        public async Task GetBasketItemCountAsync_EmptyBasket_ReturnsZero()
        {
            // Arrange
            var userName = "testuser";
            var basket = CreateTestBasket(1, userName);
            var basketList = new List<Basket> { basket };

            _mockBasketRepository.Setup(r => r.ListAsync(It.IsAny<BasketWithItemsSpecification>()))
                .ReturnsAsync(basketList);

            // Act
            var result = await _basketService.GetBasketItemCountAsync(userName);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task SetQuantities_EmptyDictionary_DoesNotUpdateItems()
        {
            // Arrange
            var basketId = 1;
            var basket = CreateTestBasket(basketId, "testuser");
            basket.AddItem(1, 10.99m, 2);
            var originalQuantity = basket.Items.First().Quantity;

            var quantities = new Dictionary<string, int>();

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);

            // Act
            await _basketService.SetQuantities(basketId, quantities);

            // Assert
            Assert.Equal(originalQuantity, basket.Items.First().Quantity);
            _mockBasketRepository.Verify(r => r.UpdateAsync(basket), Times.Once);
        }

        [Fact]
        public async Task SetQuantities_ItemIdNotInDictionary_LeavesQuantityUnchanged()
        {
            // Arrange
            var basketId = 1;
            var basket = CreateTestBasket(basketId, "testuser");
            basket.AddItem(1, 10.99m, 2);
            basket.AddItem(2, 15.99m, 3);

            var quantities = new Dictionary<string, int>
            {
                { "999", 5 } // Non-existent item ID
            };

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);

            // Act
            await _basketService.SetQuantities(basketId, quantities);

            // Assert
            Assert.Equal(2, basket.Items.First().Quantity);
            Assert.Equal(3, basket.Items.Last().Quantity);
        }

        [Fact]
        public async Task SetQuantities_WithNullLogger_DoesNotThrow()
        {
            // Arrange
            var basketService = new BasketService(_mockBasketRepository.Object, null);
            var basketId = 1;
            var basket = CreateTestBasket(basketId, "testuser");
            basket.AddItem(1, 10.99m, 2);

            var quantities = new Dictionary<string, int>
            {
                { basket.Items.First().Id.ToString(), 5 }
            };

            _mockBasketRepository.Setup(r => r.GetByIdAsync(basketId))
                .ReturnsAsync(basket);

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => basketService.SetQuantities(basketId, quantities));
            Assert.Null(exception);
        }

        [Fact]
        public async Task TransferBasketAsync_WhitespaceUserName_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _basketService.TransferBasketAsync("anonymous123", "   "));
        }

        [Fact]
        public async Task TransferBasketAsync_WhitespaceAnonymousId_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _basketService.TransferBasketAsync("   ", "testuser"));
        }

        [Fact]
        public async Task GetBasketItemCountAsync_WhitespaceUserName_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _basketService.GetBasketItemCountAsync("   "));
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

        #endregion
    }
}
