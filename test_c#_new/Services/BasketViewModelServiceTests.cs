using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.Web.Pages.Basket;
using Microsoft.eShopWeb.Web.Services;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace test_c__new.Services
{
    public class BasketViewModelServiceTests
    {
        private readonly Mock<IAsyncRepository<Basket>> _mockBasketRepository;
        private readonly Mock<IAsyncRepository<CatalogItem>> _mockItemRepository;
        private readonly Mock<IUriComposer> _mockUriComposer;
        private readonly BasketViewModelService _basketViewModelService;

        public BasketViewModelServiceTests()
        {
            _mockBasketRepository = new Mock<IAsyncRepository<Basket>>();
            _mockItemRepository = new Mock<IAsyncRepository<CatalogItem>>();
            _mockUriComposer = new Mock<IUriComposer>();
            _basketViewModelService = new BasketViewModelService(
                _mockBasketRepository.Object,
                _mockItemRepository.Object,
                _mockUriComposer.Object);
        }

        #region GetOrCreateBasketForUser Tests

        [Fact]
        public async Task GetOrCreateBasketForUser_ExistingBasket_ReturnsExistingBasketViewModel()
        {
            // Arrange
            var userName = "testuser";
            var basketId = 1;
            var basket = CreateTestBasket(basketId, userName);
            var catalogItem = CreateTestCatalogItem(1, "Test Item", 25.99m, "test.jpg");
            
            basket.AddItem(1, 25.99m, 2);

            var basketList = new List<Basket> { basket };
            _mockBasketRepository.Setup(r => r.ListAsync(It.IsAny<BasketWithItemsSpecification>()))
                .ReturnsAsync(basketList);

            _mockItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(catalogItem);

            _mockUriComposer.Setup(u => u.ComposePicUri("test.jpg"))
                .Returns("http://test.com/test.jpg");

            // Act
            var result = await _basketViewModelService.GetOrCreateBasketForUser(userName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(basketId, result.Id);
            Assert.Equal(userName, result.BuyerId);
            Assert.Single(result.Items);

            var item = result.Items.First();
            Assert.Equal(1, item.CatalogItemId);
            Assert.Equal("Test Item", item.ProductName);
            Assert.Equal(25.99m, item.UnitPrice);
            Assert.Equal(2, item.Quantity);
            Assert.Equal("http://test.com/test.jpg", item.PictureUrl);
        }

        [Fact]
        public async Task GetOrCreateBasketForUser_NoExistingBasket_CreatesNewBasket()
        {
            // Arrange
            var userName = "newuser";
            var emptyBasketList = new List<Basket>();

            _mockBasketRepository.Setup(r => r.ListAsync(It.IsAny<BasketWithItemsSpecification>()))
                .ReturnsAsync(emptyBasketList);

            var newBasket = CreateTestBasket(1, userName);
            _mockBasketRepository.Setup(r => r.AddAsync(It.IsAny<Basket>()))
                .Callback<Basket>(b => typeof(Basket).BaseType.GetProperty("Id").SetValue(b, 1))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _basketViewModelService.GetOrCreateBasketForUser(userName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userName, result.BuyerId);
            Assert.Empty(result.Items);
            _mockBasketRepository.Verify(r => r.AddAsync(It.IsAny<Basket>()), Times.Once);
        }

        [Fact]
        public async Task GetOrCreateBasketForUser_BasketWithMultipleItems_ReturnsAllItems()
        {
            // Arrange
            var userName = "testuser";
            var basketId = 1;
            var basket = CreateTestBasket(basketId, userName);
            var catalogItem1 = CreateTestCatalogItem(1, "Item 1", 10.99m, "item1.jpg");
            var catalogItem2 = CreateTestCatalogItem(2, "Item 2", 15.99m, "item2.jpg");
            var catalogItem3 = CreateTestCatalogItem(3, "Item 3", 20.99m, "item3.jpg");

            basket.AddItem(1, 10.99m, 1);
            basket.AddItem(2, 15.99m, 2);
            basket.AddItem(3, 20.99m, 3);

            var basketList = new List<Basket> { basket };
            _mockBasketRepository.Setup(r => r.ListAsync(It.IsAny<BasketWithItemsSpecification>()))
                .ReturnsAsync(basketList);

            _mockItemRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(catalogItem1);
            _mockItemRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(catalogItem2);
            _mockItemRepository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(catalogItem3);

            _mockUriComposer.Setup(u => u.ComposePicUri("item1.jpg")).Returns("http://test.com/item1.jpg");
            _mockUriComposer.Setup(u => u.ComposePicUri("item2.jpg")).Returns("http://test.com/item2.jpg");
            _mockUriComposer.Setup(u => u.ComposePicUri("item3.jpg")).Returns("http://test.com/item3.jpg");

            // Act
            var result = await _basketViewModelService.GetOrCreateBasketForUser(userName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Items.Count);

            var items = result.Items.OrderBy(i => i.CatalogItemId).ToList();
            
            Assert.Equal("Item 1", items[0].ProductName);
            Assert.Equal(1, items[0].Quantity);
            Assert.Equal(10.99m, items[0].UnitPrice);

            Assert.Equal("Item 2", items[1].ProductName);
            Assert.Equal(2, items[1].Quantity);
            Assert.Equal(15.99m, items[1].UnitPrice);

            Assert.Equal("Item 3", items[2].ProductName);
            Assert.Equal(3, items[2].Quantity);
            Assert.Equal(20.99m, items[2].UnitPrice);
        }

        [Fact]
        public async Task GetOrCreateBasketForUser_EmptyBasket_ReturnsEmptyItemsList()
        {
            // Arrange
            var userName = "testuser";
            var basketId = 1;
            var basket = CreateTestBasket(basketId, userName);

            var basketList = new List<Basket> { basket };
            _mockBasketRepository.Setup(r => r.ListAsync(It.IsAny<BasketWithItemsSpecification>()))
                .ReturnsAsync(basketList);

            // Act
            var result = await _basketViewModelService.GetOrCreateBasketForUser(userName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(basketId, result.Id);
            Assert.Equal(userName, result.BuyerId);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task GetOrCreateBasketForUser_CallsUriComposerForEachItem()
        {
            // Arrange
            var userName = "testuser";
            var basketId = 1;
            var basket = CreateTestBasket(basketId, userName);
            var catalogItem1 = CreateTestCatalogItem(1, "Item 1", 10.99m, "pic1.jpg");
            var catalogItem2 = CreateTestCatalogItem(2, "Item 2", 15.99m, "pic2.jpg");

            basket.AddItem(1, 10.99m, 1);
            basket.AddItem(2, 15.99m, 1);

            var basketList = new List<Basket> { basket };
            _mockBasketRepository.Setup(r => r.ListAsync(It.IsAny<BasketWithItemsSpecification>()))
                .ReturnsAsync(basketList);

            _mockItemRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(catalogItem1);
            _mockItemRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(catalogItem2);

            _mockUriComposer.Setup(u => u.ComposePicUri("pic1.jpg")).Returns("http://test.com/pic1.jpg");
            _mockUriComposer.Setup(u => u.ComposePicUri("pic2.jpg")).Returns("http://test.com/pic2.jpg");

            // Act
            await _basketViewModelService.GetOrCreateBasketForUser(userName);

            // Assert
            _mockUriComposer.Verify(u => u.ComposePicUri("pic1.jpg"), Times.Once);
            _mockUriComposer.Verify(u => u.ComposePicUri("pic2.jpg"), Times.Once);
        }

        [Fact]
        public async Task GetOrCreateBasketForUser_CallsItemRepositoryForEachBasketItem()
        {
            // Arrange
            var userName = "testuser";
            var basketId = 1;
            var basket = CreateTestBasket(basketId, userName);
            var catalogItem1 = CreateTestCatalogItem(1, "Item 1", 10.99m, "pic1.jpg");
            var catalogItem2 = CreateTestCatalogItem(2, "Item 2", 15.99m, "pic2.jpg");

            basket.AddItem(1, 10.99m, 1);
            basket.AddItem(2, 15.99m, 1);

            var basketList = new List<Basket> { basket };
            _mockBasketRepository.Setup(r => r.ListAsync(It.IsAny<BasketWithItemsSpecification>()))
                .ReturnsAsync(basketList);

            _mockItemRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(catalogItem1);
            _mockItemRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(catalogItem2);

            _mockUriComposer.Setup(u => u.ComposePicUri(It.IsAny<string>()))
                .Returns("http://test.com/image.jpg");

            // Act
            await _basketViewModelService.GetOrCreateBasketForUser(userName);

            // Assert
            _mockItemRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
            _mockItemRepository.Verify(r => r.GetByIdAsync(2), Times.Once);
        }

        [Fact]
        public async Task GetOrCreateBasketForUser_ValidUserName_UsesCorrectSpecification()
        {
            // Arrange
            var userName = "testuser";
            var emptyBasketList = new List<Basket>();

            BasketWithItemsSpecification capturedSpec = null;
            _mockBasketRepository.Setup(r => r.ListAsync(It.IsAny<BasketWithItemsSpecification>()))
                .Callback<BasketWithItemsSpecification>(spec => capturedSpec = spec)
                .ReturnsAsync(emptyBasketList);

            _mockBasketRepository.Setup(r => r.AddAsync(It.IsAny<Basket>()))
                .Returns(Task.CompletedTask);

            // Act
            await _basketViewModelService.GetOrCreateBasketForUser(userName);

            // Assert
            Assert.NotNull(capturedSpec);
            // Note: We can't easily verify the specification contents without access to internal properties
            // but we can verify it was called with the correct type
            _mockBasketRepository.Verify(r => r.ListAsync(It.IsAny<BasketWithItemsSpecification>()), Times.Once);
        }

        #endregion

        #region Private Method Testing Through Public Interface

        [Fact]
        public async Task GetOrCreateBasketForUser_NewBasket_SetsCorrectProperties()
        {
            // Arrange
            var userName = "newuser";
            var emptyBasketList = new List<Basket>();

            _mockBasketRepository.Setup(r => r.ListAsync(It.IsAny<BasketWithItemsSpecification>()))
                .ReturnsAsync(emptyBasketList);

            Basket capturedBasket = null;
            _mockBasketRepository.Setup(r => r.AddAsync(It.IsAny<Basket>()))
                .Callback<Basket>(b => 
                {
                    capturedBasket = b;
                    typeof(Basket).BaseType.GetProperty("Id").SetValue(b, 123);
                })
                .Returns(Task.CompletedTask);

            // Act
            var result = await _basketViewModelService.GetOrCreateBasketForUser(userName);

            // Assert
            Assert.NotNull(capturedBasket);
            Assert.Equal(userName, capturedBasket.BuyerId);
            Assert.Equal(userName, result.BuyerId);
            Assert.Equal(123, result.Id);
            Assert.Empty(result.Items);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task GetOrCreateBasketForUser_NullUserName_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<System.ArgumentNullException>(() => 
                _basketViewModelService.GetOrCreateBasketForUser(null));
        }

        [Fact]
        public async Task GetOrCreateBasketForUser_EmptyUserName_CreateBasketForEmptyString()
        {
            // Arrange
            var userName = "";
            var emptyBasketList = new List<Basket>();

            _mockBasketRepository.Setup(r => r.ListAsync(It.IsAny<BasketWithItemsSpecification>()))
                .ReturnsAsync(emptyBasketList);

            _mockBasketRepository.Setup(r => r.AddAsync(It.IsAny<Basket>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _basketViewModelService.GetOrCreateBasketForUser(userName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("", result.BuyerId);
        }

        #endregion

        #region Additional Edge Case Tests

        [Fact]
        public async Task GetOrCreateBasketForUser_EmptyUserName_CreatesBasketWithEmptyBuyerId()
        {
            // Arrange
            var userName = "";
            var emptyBasketList = new List<Basket>();

            _mockBasketRepository.Setup(r => r.ListAsync(It.IsAny<BasketWithItemsSpecification>()))
                .ReturnsAsync(emptyBasketList);

            _mockBasketRepository.Setup(r => r.AddAsync(It.IsAny<Basket>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _basketViewModelService.GetOrCreateBasketForUser(userName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("", result.BuyerId);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task GetOrCreateBasketForUser_NullUserName_CreatesBasketWithNullBuyerId()
        {
            // Arrange
            string userName = null;
            var emptyBasketList = new List<Basket>();

            _mockBasketRepository.Setup(r => r.ListAsync(It.IsAny<BasketWithItemsSpecification>()))
                .ReturnsAsync(emptyBasketList);

            _mockBasketRepository.Setup(r => r.AddAsync(It.IsAny<Basket>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _basketViewModelService.GetOrCreateBasketForUser(userName);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.BuyerId);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task GetOrCreateBasketForUser_ExistingBasketWithZeroPriceItems_ReturnsCorrectViewModel()
        {
            // Arrange
            var userName = "testuser";
            var basketId = 1;
            var basket = CreateTestBasket(basketId, userName);
            var catalogItem = CreateTestCatalogItem(1, "Free Item", 0m, "free.jpg");
            
            basket.AddItem(1, 0m, 1);

            var basketList = new List<Basket> { basket };
            _mockBasketRepository.Setup(r => r.ListAsync(It.IsAny<BasketWithItemsSpecification>()))
                .ReturnsAsync(basketList);

            _mockItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(catalogItem);

            _mockUriComposer.Setup(u => u.ComposePicUri("free.jpg"))
                .Returns("http://test.com/free.jpg");

            // Act
            var result = await _basketViewModelService.GetOrCreateBasketForUser(userName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userName, result.BuyerId);
            Assert.Single(result.Items);
            
            var item = result.Items.First();
            Assert.Equal(0m, item.UnitPrice);
            Assert.Equal("Free Item", item.ProductName);
        }

        [Fact]
        public async Task GetOrCreateBasketForUser_ExistingBasketWithLargeQuantity_ReturnsCorrectViewModel()
        {
            // Arrange
            var userName = "testuser";
            var basketId = 1;
            var basket = CreateTestBasket(basketId, userName);
            var catalogItem = CreateTestCatalogItem(1, "Bulk Item", 1.99m, "bulk.jpg");
            
            basket.AddItem(1, 1.99m, 1000);

            var basketList = new List<Basket> { basket };
            _mockBasketRepository.Setup(r => r.ListAsync(It.IsAny<BasketWithItemsSpecification>()))
                .ReturnsAsync(basketList);

            _mockItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(catalogItem);

            _mockUriComposer.Setup(u => u.ComposePicUri("bulk.jpg"))
                .Returns("http://test.com/bulk.jpg");

            // Act
            var result = await _basketViewModelService.GetOrCreateBasketForUser(userName);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            
            var item = result.Items.First();
            Assert.Equal(1000, item.Quantity);
        }

        [Fact]
        public async Task GetOrCreateBasketForUser_CatalogItemWithNullPictureUri_HandlesCorrectly()
        {
            // Arrange
            var userName = "testuser";
            var basketId = 1;
            var basket = CreateTestBasket(basketId, userName);
            var catalogItem = CreateTestCatalogItem(1, "No Picture Item", 15.99m, null);
            
            basket.AddItem(1, 15.99m, 1);

            var basketList = new List<Basket> { basket };
            _mockBasketRepository.Setup(r => r.ListAsync(It.IsAny<BasketWithItemsSpecification>()))
                .ReturnsAsync(basketList);

            _mockItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(catalogItem);

            _mockUriComposer.Setup(u => u.ComposePicUri(null))
                .Returns("http://test.com/default.jpg");

            // Act
            var result = await _basketViewModelService.GetOrCreateBasketForUser(userName);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            
            var item = result.Items.First();
            Assert.Equal("http://test.com/default.jpg", item.PictureUrl);
        }

        [Fact]
        public async Task GetOrCreateBasketForUser_CatalogItemWithEmptyPictureUri_HandlesCorrectly()
        {
            // Arrange
            var userName = "testuser";
            var basketId = 1;
            var basket = CreateTestBasket(basketId, userName);
            var catalogItem = CreateTestCatalogItem(1, "Empty Picture Item", 15.99m, "");
            
            basket.AddItem(1, 15.99m, 1);

            var basketList = new List<Basket> { basket };
            _mockBasketRepository.Setup(r => r.ListAsync(It.IsAny<BasketWithItemsSpecification>()))
                .ReturnsAsync(basketList);

            _mockItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(catalogItem);

            _mockUriComposer.Setup(u => u.ComposePicUri(""))
                .Returns("http://test.com/empty.jpg");

            // Act
            var result = await _basketViewModelService.GetOrCreateBasketForUser(userName);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            
            var item = result.Items.First();
            Assert.Equal("http://test.com/empty.jpg", item.PictureUrl);
        }

        [Fact]
        public async Task GetOrCreateBasketForUser_MultipleBasketItemsWithSameCatalogItem_ProcessesAllCorrectly()
        {
            // Arrange
            var userName = "testuser";
            var basketId = 1;
            var basket = CreateTestBasket(basketId, userName);
            var catalogItem = CreateTestCatalogItem(1, "Popular Item", 19.99m, "popular.jpg");
            
            // Add the same catalog item multiple times (though this shouldn't happen in real scenarios)
            basket.AddItem(1, 19.99m, 2);
            basket.AddItem(1, 19.99m, 3);

            var basketList = new List<Basket> { basket };
            _mockBasketRepository.Setup(r => r.ListAsync(It.IsAny<BasketWithItemsSpecification>()))
                .ReturnsAsync(basketList);

            _mockItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(catalogItem);

            _mockUriComposer.Setup(u => u.ComposePicUri("popular.jpg"))
                .Returns("http://test.com/popular.jpg");

            // Act
            var result = await _basketViewModelService.GetOrCreateBasketForUser(userName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count); // Should have 2 basket items
            
            // Both items should reference the same catalog item
            Assert.All(result.Items, item => 
            {
                Assert.Equal(1, item.CatalogItemId);
                Assert.Equal("Popular Item", item.ProductName);
                Assert.Equal("http://test.com/popular.jpg", item.PictureUrl);
            });
        }

        [Fact]
        public async Task GetOrCreateBasketForUser_UriComposerReturnsNull_HandlesCorrectly()
        {
            // Arrange
            var userName = "testuser";
            var basketId = 1;
            var basket = CreateTestBasket(basketId, userName);
            var catalogItem = CreateTestCatalogItem(1, "Item", 15.99m, "test.jpg");
            
            basket.AddItem(1, 15.99m, 1);

            var basketList = new List<Basket> { basket };
            _mockBasketRepository.Setup(r => r.ListAsync(It.IsAny<BasketWithItemsSpecification>()))
                .ReturnsAsync(basketList);

            _mockItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(catalogItem);

            _mockUriComposer.Setup(u => u.ComposePicUri("test.jpg"))
                .Returns((string)null);

            // Act
            var result = await _basketViewModelService.GetOrCreateBasketForUser(userName);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            
            var item = result.Items.First();
            Assert.Null(item.PictureUrl);
        }

        [Fact]
        public async Task GetOrCreateBasketForUser_VerifiesRepositoryCallsCorrectly()
        {
            // Arrange
            var userName = "testuser";
            var emptyBasketList = new List<Basket>();

            _mockBasketRepository.Setup(r => r.ListAsync(It.IsAny<BasketWithItemsSpecification>()))
                .ReturnsAsync(emptyBasketList);

            _mockBasketRepository.Setup(r => r.AddAsync(It.IsAny<Basket>()))
                .Returns(Task.CompletedTask);

            // Act
            await _basketViewModelService.GetOrCreateBasketForUser(userName);

            // Assert
            _mockBasketRepository.Verify(r => r.ListAsync(It.IsAny<BasketWithItemsSpecification>()), Times.Once);
            _mockBasketRepository.Verify(r => r.AddAsync(It.IsAny<Basket>()), Times.Once);
        }

        #endregion

        #region Helper Methods

        private Basket CreateTestBasket(int id, string buyerId)
        {
            var basket = new Basket(buyerId);
            typeof(Basket).BaseType.GetProperty("Id").SetValue(basket, id);
            return basket;
        }

        private CatalogItem CreateTestCatalogItem(int id, string name, decimal price, string pictureUri)
        {
            var catalogItem = new CatalogItem(1, 1, name, name, price, pictureUri);
            typeof(CatalogItem).BaseType.GetProperty("Id").SetValue(catalogItem, id);
            return catalogItem;
        }

        #endregion
    }
}
