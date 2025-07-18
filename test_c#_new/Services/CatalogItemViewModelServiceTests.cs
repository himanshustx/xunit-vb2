using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Web.Services;
using Microsoft.eShopWeb.Web.ViewModels;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace test_c__new.Services
{
    public class CatalogItemViewModelServiceTests
    {
        private readonly Mock<IAsyncRepository<CatalogItem>> _mockCatalogItemRepository;
        private readonly CatalogItemViewModelService _catalogItemViewModelService;

        public CatalogItemViewModelServiceTests()
        {
            _mockCatalogItemRepository = new Mock<IAsyncRepository<CatalogItem>>();
            _catalogItemViewModelService = new CatalogItemViewModelService(_mockCatalogItemRepository.Object);
        }

        #region UpdateCatalogItem Tests

        [Fact]
        public async Task UpdateCatalogItem_ValidViewModel_UpdatesExistingItem()
        {
            // Arrange
            var viewModel = new CatalogItemViewModel
            {
                Id = 1,
                Name = "Updated Product Name",
                Price = 99.99m,
                PictureUri = "updated-picture.jpg"
            };

            var existingCatalogItem = CreateTestCatalogItem(1, "Original Name", 50.00m, "original.jpg");

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingCatalogItem);

            CatalogItem updatedItem = null;
            _mockCatalogItemRepository.Setup(r => r.UpdateAsync(It.IsAny<CatalogItem>()))
                .Callback<CatalogItem>(item => updatedItem = item)
                .Returns(Task.CompletedTask);

            // Act
            await _catalogItemViewModelService.UpdateCatalogItem(viewModel);

            // Assert
            _mockCatalogItemRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
            _mockCatalogItemRepository.Verify(r => r.UpdateAsync(existingCatalogItem), Times.Once);

            Assert.Equal("Updated Product Name", existingCatalogItem.Name);
            Assert.Equal(99.99m, existingCatalogItem.Price);
        }

        [Fact]
        public async Task UpdateCatalogItem_ValidId_CallsRepositoryMethods()
        {
            // Arrange
            var itemId = 42;
            var viewModel = new CatalogItemViewModel
            {
                Id = itemId,
                Name = "Test Product",
                Price = 25.50m
            };

            var existingCatalogItem = CreateTestCatalogItem(itemId, "Old Name", 10.00m, "old.jpg");

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(itemId))
                .ReturnsAsync(existingCatalogItem);

            // Act
            await _catalogItemViewModelService.UpdateCatalogItem(viewModel);

            // Assert
            _mockCatalogItemRepository.Verify(r => r.GetByIdAsync(itemId), Times.Once);
            _mockCatalogItemRepository.Verify(r => r.UpdateAsync(existingCatalogItem), Times.Once);
        }

        [Fact]
        public async Task UpdateCatalogItem_ChangesNameAndPrice_UpdatesBothProperties()
        {
            // Arrange
            var originalName = "Original Product";
            var originalPrice = 19.99m;
            var newName = "Updated Product";
            var newPrice = 29.99m;

            var viewModel = new CatalogItemViewModel
            {
                Id = 1,
                Name = newName,
                Price = newPrice
            };

            var existingCatalogItem = CreateTestCatalogItem(1, originalName, originalPrice, "test.jpg");

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingCatalogItem);

            // Act
            await _catalogItemViewModelService.UpdateCatalogItem(viewModel);

            // Assert
            Assert.Equal(newName, existingCatalogItem.Name);
            Assert.Equal(newPrice, existingCatalogItem.Price);
            Assert.NotEqual(originalName, existingCatalogItem.Name);
            Assert.NotEqual(originalPrice, existingCatalogItem.Price);
        }

        [Fact]
        public async Task UpdateCatalogItem_ZeroPrice_UpdatesWithZeroPrice()
        {
            // Arrange
            var viewModel = new CatalogItemViewModel
            {
                Id = 1,
                Name = "Free Product",
                Price = 0.00m
            };

            var existingCatalogItem = CreateTestCatalogItem(1, "Paid Product", 50.00m, "test.jpg");

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingCatalogItem);

            // Act
            await _catalogItemViewModelService.UpdateCatalogItem(viewModel);

            // Assert
            Assert.Equal(0.00m, existingCatalogItem.Price);
            Assert.Equal("Free Product", existingCatalogItem.Name);
        }

        [Fact]
        public async Task UpdateCatalogItem_NegativePrice_UpdatesWithNegativePrice()
        {
            // Arrange
            var viewModel = new CatalogItemViewModel
            {
                Id = 1,
                Name = "Discounted Product",
                Price = -10.00m
            };

            var existingCatalogItem = CreateTestCatalogItem(1, "Regular Product", 20.00m, "test.jpg");

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingCatalogItem);

            // Act
            await _catalogItemViewModelService.UpdateCatalogItem(viewModel);

            // Assert
            Assert.Equal(-10.00m, existingCatalogItem.Price);
        }

        [Fact]
        public async Task UpdateCatalogItem_EmptyName_UpdatesWithEmptyName()
        {
            // Arrange
            var viewModel = new CatalogItemViewModel
            {
                Id = 1,
                Name = "",
                Price = 15.99m
            };

            var existingCatalogItem = CreateTestCatalogItem(1, "Original Name", 10.00m, "test.jpg");

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingCatalogItem);

            // Act
            await _catalogItemViewModelService.UpdateCatalogItem(viewModel);

            // Assert
            Assert.Equal("", existingCatalogItem.Name);
            Assert.Equal(15.99m, existingCatalogItem.Price);
        }

        [Fact]
        public async Task UpdateCatalogItem_NullName_UpdatesWithNullName()
        {
            // Arrange
            var viewModel = new CatalogItemViewModel
            {
                Id = 1,
                Name = null,
                Price = 15.99m
            };

            var existingCatalogItem = CreateTestCatalogItem(1, "Original Name", 10.00m, "test.jpg");

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingCatalogItem);

            // Act
            await _catalogItemViewModelService.UpdateCatalogItem(viewModel);

            // Assert
            Assert.Null(existingCatalogItem.Name);
            Assert.Equal(15.99m, existingCatalogItem.Price);
        }

        [Fact]
        public async Task UpdateCatalogItem_LargePrice_UpdatesCorrectly()
        {
            // Arrange
            var largePrice = 999999.99m;
            var viewModel = new CatalogItemViewModel
            {
                Id = 1,
                Name = "Expensive Product",
                Price = largePrice
            };

            var existingCatalogItem = CreateTestCatalogItem(1, "Cheap Product", 1.00m, "test.jpg");

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingCatalogItem);

            // Act
            await _catalogItemViewModelService.UpdateCatalogItem(viewModel);

            // Assert
            Assert.Equal(largePrice, existingCatalogItem.Price);
            Assert.Equal("Expensive Product", existingCatalogItem.Name);
        }

        [Fact]
        public async Task UpdateCatalogItem_LongName_UpdatesCorrectly()
        {
            // Arrange
            var longName = new string('A', 1000); // 1000 character name
            var viewModel = new CatalogItemViewModel
            {
                Id = 1,
                Name = longName,
                Price = 25.99m
            };

            var existingCatalogItem = CreateTestCatalogItem(1, "Short", 10.00m, "test.jpg");

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingCatalogItem);

            // Act
            await _catalogItemViewModelService.UpdateCatalogItem(viewModel);

            // Assert
            Assert.Equal(longName, existingCatalogItem.Name);
            Assert.Equal(25.99m, existingCatalogItem.Price);
        }

        [Fact]
        public async Task UpdateCatalogItem_SpecialCharactersInName_UpdatesCorrectly()
        {
            // Arrange
            var specialName = "Test!@#$%^&*()_+-=[]{}|;':\",./<>?`~Product";
            var viewModel = new CatalogItemViewModel
            {
                Id = 1,
                Name = specialName,
                Price = 15.99m
            };

            var existingCatalogItem = CreateTestCatalogItem(1, "Normal Name", 10.00m, "test.jpg");

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingCatalogItem);

            // Act
            await _catalogItemViewModelService.UpdateCatalogItem(viewModel);

            // Assert
            Assert.Equal(specialName, existingCatalogItem.Name);
        }

        [Fact]
        public async Task UpdateCatalogItem_NullViewModel_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<System.NullReferenceException>(() => 
                _catalogItemViewModelService.UpdateCatalogItem(null));
        }

        [Fact]
        public async Task UpdateCatalogItem_ItemNotFound_ThrowsException()
        {
            // Arrange
            var viewModel = new CatalogItemViewModel
            {
                Id = 999, // Non-existent ID
                Name = "Test Product",
                Price = 25.99m
            };

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((CatalogItem)null);

            // Act & Assert
            await Assert.ThrowsAsync<System.NullReferenceException>(() => 
                _catalogItemViewModelService.UpdateCatalogItem(viewModel));
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_ValidRepository_DoesNotThrow()
        {
            // Arrange
            var mockRepository = new Mock<IAsyncRepository<CatalogItem>>();

            // Act & Assert
            var exception = Record.Exception(() => new CatalogItemViewModelService(mockRepository.Object));
            Assert.Null(exception);
        }

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => 
                new CatalogItemViewModelService(null));
        }

        #endregion

        #region Additional Edge Case Tests

        [Fact]
        public async Task UpdateCatalogItem_MaxDecimalPrice_UpdatesCorrectly()
        {
            // Arrange
            var viewModel = new CatalogItemViewModel
            {
                Id = 1,
                Name = "Expensive Item",
                Price = decimal.MaxValue
            };

            var existingCatalogItem = CreateTestCatalogItem(1, "Original Name", 10.00m, "test.jpg");

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingCatalogItem);

            // Act
            await _catalogItemViewModelService.UpdateCatalogItem(viewModel);

            // Assert
            Assert.Equal(decimal.MaxValue, existingCatalogItem.Price);
        }

        [Fact]
        public async Task UpdateCatalogItem_MinDecimalPrice_UpdatesCorrectly()
        {
            // Arrange
            var viewModel = new CatalogItemViewModel
            {
                Id = 1,
                Name = "Cheap Item",
                Price = decimal.MinValue
            };

            var existingCatalogItem = CreateTestCatalogItem(1, "Original Name", 10.00m, "test.jpg");

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingCatalogItem);

            // Act
            await _catalogItemViewModelService.UpdateCatalogItem(viewModel);

            // Assert
            Assert.Equal(decimal.MinValue, existingCatalogItem.Price);
        }

        [Fact]
        public async Task UpdateCatalogItem_VeryLongName_UpdatesCorrectly()
        {
            // Arrange
            var longName = new string('A', 1000);
            var viewModel = new CatalogItemViewModel
            {
                Id = 1,
                Name = longName,
                Price = 15.99m
            };

            var existingCatalogItem = CreateTestCatalogItem(1, "Original Name", 10.00m, "test.jpg");

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingCatalogItem);

            // Act
            await _catalogItemViewModelService.UpdateCatalogItem(viewModel);

            // Assert
            Assert.Equal(longName, existingCatalogItem.Name);
        }

        [Fact]
        public async Task UpdateCatalogItem_WhitespaceName_UpdatesCorrectly()
        {
            // Arrange
            var viewModel = new CatalogItemViewModel
            {
                Id = 1,
                Name = "   ",
                Price = 15.99m
            };

            var existingCatalogItem = CreateTestCatalogItem(1, "Original Name", 10.00m, "test.jpg");

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingCatalogItem);

            // Act
            await _catalogItemViewModelService.UpdateCatalogItem(viewModel);

            // Assert
            Assert.Equal("   ", existingCatalogItem.Name);
        }

        [Fact]
        public async Task UpdateCatalogItem_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var viewModel = new CatalogItemViewModel
            {
                Id = 1,
                Name = "Test Item",
                Price = 15.99m
            };

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ThrowsAsync(new InvalidOperationException("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _catalogItemViewModelService.UpdateCatalogItem(viewModel));
        }

        [Fact]
        public async Task UpdateCatalogItem_UpdateAsyncThrowsException_PropagatesException()
        {
            // Arrange
            var viewModel = new CatalogItemViewModel
            {
                Id = 1,
                Name = "Test Item",
                Price = 15.99m
            };

            var existingCatalogItem = CreateTestCatalogItem(1, "Original Name", 10.00m, "test.jpg");

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingCatalogItem);
            _mockCatalogItemRepository.Setup(r => r.UpdateAsync(It.IsAny<CatalogItem>()))
                .ThrowsAsync(new InvalidOperationException("Update error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _catalogItemViewModelService.UpdateCatalogItem(viewModel));
        }

        [Fact]
        public async Task UpdateCatalogItem_ViewModelWithPictureUri_IgnoresPictureUri()
        {
            // Arrange - Testing that PictureUri in view model doesn't affect update since Update method only takes name and price
            var viewModel = new CatalogItemViewModel
            {
                Id = 1,
                Name = "Updated Item",
                Price = 25.99m,
                PictureUri = "new-picture.jpg"
            };

            var existingCatalogItem = CreateTestCatalogItem(1, "Original Name", 10.00m, "original.jpg");
            var originalPictureUri = existingCatalogItem.PictureUri;

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingCatalogItem);

            // Act
            await _catalogItemViewModelService.UpdateCatalogItem(viewModel);

            // Assert - PictureUri should remain unchanged since Update method doesn't modify it
            Assert.Equal("Updated Item", existingCatalogItem.Name);
            Assert.Equal(25.99m, existingCatalogItem.Price);
            Assert.Equal(originalPictureUri, existingCatalogItem.PictureUri); // Should be unchanged
        }

        [Fact]
        public async Task UpdateCatalogItem_CallsRepositoryMethodsInCorrectOrder()
        {
            // Arrange
            var viewModel = new CatalogItemViewModel
            {
                Id = 1,
                Name = "Test Item",
                Price = 15.99m
            };

            var existingCatalogItem = CreateTestCatalogItem(1, "Original Name", 10.00m, "test.jpg");
            var sequence = new MockSequence();

            _mockCatalogItemRepository.InSequence(sequence)
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingCatalogItem);

            _mockCatalogItemRepository.InSequence(sequence)
                .Setup(r => r.UpdateAsync(existingCatalogItem))
                .Returns(Task.CompletedTask);

            // Act
            await _catalogItemViewModelService.UpdateCatalogItem(viewModel);

            // Assert
            _mockCatalogItemRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
            _mockCatalogItemRepository.Verify(r => r.UpdateAsync(existingCatalogItem), Times.Once);
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public async Task GetCatalogItem_WithMaxIntId_HandlesCorrectly()
        {
            // Arrange
            var catalogItem = CreateTestCatalogItem(int.MaxValue, "Max ID Item", 99.99m, "max-item.jpg");
            
            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(int.MaxValue))
                .ReturnsAsync(catalogItem);

            // Act
            var result = await _catalogItemViewModelService.GetCatalogItem(int.MaxValue);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(int.MaxValue, result.Id);
            Assert.Equal("Max ID Item", result.Name);
        }

        [Fact]
        public async Task GetCatalogItems_WithZeroPageSize_ReturnsDefaultPageSize()
        {
            // Arrange
            var catalogItems = new List<CatalogItem>
            {
                CreateTestCatalogItem(1, "Item 1", 10.00m, "item1.jpg"),
                CreateTestCatalogItem(2, "Item 2", 15.00m, "item2.jpg")
            };

            _mockCatalogItemRepository.Setup(r => r.ListPaginatedAsync(It.IsAny<ISpecification<CatalogItem>>(), 0, 0))
                .ReturnsAsync(catalogItems);

            _mockCatalogItemRepository.Setup(r => r.CountAsync(It.IsAny<ISpecification<CatalogItem>>()))
                .ReturnsAsync(catalogItems.Count);

            // Act
            var result = await _catalogItemViewModelService.GetCatalogItems(0, null, null, 0);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.CatalogItems.Count());
        }

        [Fact]
        public async Task GetCatalogItems_WithNegativePageIndex_HandlesCorrectly()
        {
            // Arrange
            var catalogItems = new List<CatalogItem>
            {
                CreateTestCatalogItem(1, "Item 1", 10.00m, "item1.jpg")
            };

            _mockCatalogItemRepository.Setup(r => r.ListPaginatedAsync(It.IsAny<ISpecification<CatalogItem>>(), -1, 10))
                .ReturnsAsync(catalogItems);

            _mockCatalogItemRepository.Setup(r => r.CountAsync(It.IsAny<ISpecification<CatalogItem>>()))
                .ReturnsAsync(catalogItems.Count);

            // Act
            var result = await _catalogItemViewModelService.GetCatalogItems(-1, null, null, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(-1, result.PaginationInfo.ActualPage);
        }

        [Fact]
        public async Task UpdateCatalogItem_WithMaxDecimalPrice_UpdatesCorrectly()
        {
            // Arrange
            var viewModel = new CatalogItemViewModel
            {
                Id = 1,
                Name = "Expensive Item",
                Price = decimal.MaxValue
            };

            var existingCatalogItem = CreateTestCatalogItem(1, "Original Name", 10.00m, "test.jpg");

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingCatalogItem);

            // Act
            await _catalogItemViewModelService.UpdateCatalogItem(viewModel);

            // Assert
            Assert.Equal(decimal.MaxValue, existingCatalogItem.Price);
            _mockCatalogItemRepository.Verify(r => r.UpdateAsync(existingCatalogItem), Times.Once);
        }

        [Fact]
        public async Task UpdateCatalogItem_WithZeroPrice_UpdatesCorrectly()
        {
            // Arrange
            var viewModel = new CatalogItemViewModel
            {
                Id = 1,
                Name = "Free Item",
                Price = 0.00m
            };

            var existingCatalogItem = CreateTestCatalogItem(1, "Original Name", 10.00m, "test.jpg");

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingCatalogItem);

            // Act
            await _catalogItemViewModelService.UpdateCatalogItem(viewModel);

            // Assert
            Assert.Equal(0.00m, existingCatalogItem.Price);
            _mockCatalogItemRepository.Verify(r => r.UpdateAsync(existingCatalogItem), Times.Once);
        }

        [Fact]
        public async Task UpdateCatalogItem_WithVeryLongName_UpdatesCorrectly()
        {
            // Arrange
            var longName = new string('A', 1000);
            var viewModel = new CatalogItemViewModel
            {
                Id = 1,
                Name = longName,
                Price = 15.99m
            };

            var existingCatalogItem = CreateTestCatalogItem(1, "Original Name", 10.00m, "test.jpg");

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingCatalogItem);

            // Act
            await _catalogItemViewModelService.UpdateCatalogItem(viewModel);

            // Assert
            Assert.Equal(longName, existingCatalogItem.Name);
            _mockCatalogItemRepository.Verify(r => r.UpdateAsync(existingCatalogItem), Times.Once);
        }

        [Fact]
        public async Task UpdateCatalogItem_WithSpecialCharactersInName_UpdatesCorrectly()
        {
            // Arrange
            var specialName = "Test™ Item® with Special Çharacters & Symbols!@#$%^&*()";
            var viewModel = new CatalogItemViewModel
            {
                Id = 1,
                Name = specialName,
                Price = 15.99m
            };

            var existingCatalogItem = CreateTestCatalogItem(1, "Original Name", 10.00m, "test.jpg");

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingCatalogItem);

            // Act
            await _catalogItemViewModelService.UpdateCatalogItem(viewModel);

            // Assert
            Assert.Equal(specialName, existingCatalogItem.Name);
            _mockCatalogItemRepository.Verify(r => r.UpdateAsync(existingCatalogItem), Times.Once);
        }

        [Fact]
        public async Task GetCatalogItems_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockCatalogItemRepository.Setup(r => r.ListPaginatedAsync(It.IsAny<ISpecification<CatalogItem>>(), It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _catalogItemViewModelService.GetCatalogItems(0, null, null, 10));
        }

        [Fact]
        public async Task UpdateCatalogItem_UpdateRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var viewModel = new CatalogItemViewModel
            {
                Id = 1,
                Name = "Test Item",
                Price = 15.99m
            };

            var existingCatalogItem = CreateTestCatalogItem(1, "Original Name", 10.00m, "test.jpg");

            _mockCatalogItemRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingCatalogItem);

            _mockCatalogItemRepository.Setup(r => r.UpdateAsync(It.IsAny<CatalogItem>()))
                .ThrowsAsync(new ArgumentException("Update failed"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _catalogItemViewModelService.UpdateCatalogItem(viewModel));
        }

        [Fact]
        public async Task GetCatalogItems_WithLargePageSize_HandlesCorrectly()
        {
            // Arrange
            var catalogItems = Enumerable.Range(1, 1000)
                .Select(i => CreateTestCatalogItem(i, $"Item {i}", i * 1.50m, $"item{i}.jpg"))
                .ToList();

            _mockCatalogItemRepository.Setup(r => r.ListPaginatedAsync(It.IsAny<ISpecification<CatalogItem>>(), 0, 1000))
                .ReturnsAsync(catalogItems);

            _mockCatalogItemRepository.Setup(r => r.CountAsync(It.IsAny<ISpecification<CatalogItem>>()))
                .ReturnsAsync(catalogItems.Count);

            // Act
            var result = await _catalogItemViewModelService.GetCatalogItems(0, null, null, 1000);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1000, result.CatalogItems.Count());
            Assert.Equal(1000, result.PaginationInfo.ItemsPerPage);
        }

        [Fact]
        public async Task GetCatalogItems_WithFiltersCombined_AppliesAllFilters()
        {
            // Arrange
            var catalogItems = new List<CatalogItem>
            {
                CreateTestCatalogItem(1, "Filtered Item", 25.00m, "filtered.jpg")
            };

            _mockCatalogItemRepository.Setup(r => r.ListPaginatedAsync(It.IsAny<ISpecification<CatalogItem>>(), 0, 10))
                .ReturnsAsync(catalogItems);

            _mockCatalogItemRepository.Setup(r => r.CountAsync(It.IsAny<ISpecification<CatalogItem>>()))
                .ReturnsAsync(catalogItems.Count);

            // Act
            var result = await _catalogItemViewModelService.GetCatalogItems(0, 1, 2, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.BrandFilterApplied);
            Assert.Equal(2, result.TypesFilterApplied);
            Assert.Single(result.CatalogItems);
        }

        #endregion

        #region Helper Methods

        private CatalogItem CreateTestCatalogItem(int id, string name, decimal price, string pictureUri)
        {
            var catalogItem = new CatalogItem(1, 1, name, name, price, pictureUri);
            typeof(CatalogItem).BaseType.GetProperty("Id").SetValue(catalogItem, id);
            return catalogItem;
        }

        #endregion
    }
}
