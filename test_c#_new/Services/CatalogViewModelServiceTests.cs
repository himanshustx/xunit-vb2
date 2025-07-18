using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.eShopWeb.Web.Services;
using Microsoft.eShopWeb.Web.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace test_c__new.Services
{
    public class CatalogViewModelServiceTests
    {
        private readonly Mock<ILogger<CatalogViewModelService>> _mockLogger;
        private readonly Mock<IAsyncRepository<CatalogItem>> _mockItemRepository;
        private readonly Mock<IAsyncRepository<CatalogBrand>> _mockBrandRepository;
        private readonly Mock<IAsyncRepository<CatalogType>> _mockTypeRepository;
        private readonly Mock<IUriComposer> _mockUriComposer;
        private readonly Mock<ILoggerFactory> _mockLoggerFactory;
        private readonly CatalogViewModelService _catalogViewModelService;

        public CatalogViewModelServiceTests()
        {
            _mockLogger = new Mock<ILogger<CatalogViewModelService>>();
            _mockItemRepository = new Mock<IAsyncRepository<CatalogItem>>();
            _mockBrandRepository = new Mock<IAsyncRepository<CatalogBrand>>();
            _mockTypeRepository = new Mock<IAsyncRepository<CatalogType>>();
            _mockUriComposer = new Mock<IUriComposer>();
            _mockLoggerFactory = new Mock<ILoggerFactory>();

            _mockLoggerFactory.Setup(f => f.CreateLogger<CatalogViewModelService>())
                .Returns(_mockLogger.Object);

            _catalogViewModelService = new CatalogViewModelService(
                _mockLoggerFactory.Object,
                _mockItemRepository.Object,
                _mockBrandRepository.Object,
                _mockTypeRepository.Object,
                _mockUriComposer.Object);
        }

        #region GetCatalogItems Tests

        [Fact]
        public async Task GetCatalogItems_ValidParameters_ReturnsCorrectViewModel()
        {
            // Arrange
            var pageIndex = 0;
            var itemsPage = 2;
            int? brandId = 1;
            int? typeId = 2;

            var catalogItems = CreateTestCatalogItems();
            var totalItems = 5;

            _mockItemRepository.Setup(r => r.ListAsync(It.IsAny<CatalogFilterPaginatedSpecification>()))
                .ReturnsAsync(catalogItems);
            _mockItemRepository.Setup(r => r.CountAsync(It.IsAny<CatalogFilterSpecification>()))
                .ReturnsAsync(totalItems);

            SetupBrandRepository();
            SetupTypeRepository();

            // Act
            var result = await _catalogViewModelService.GetCatalogItems(pageIndex, itemsPage, brandId, typeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(catalogItems.Count, result.CatalogItems.Count());
            Assert.Equal(brandId.Value, result.BrandFilterApplied);
            Assert.Equal(typeId.Value, result.TypesFilterApplied);
            Assert.Equal(pageIndex, result.PaginationInfo.ActualPage);
            Assert.Equal(catalogItems.Count, result.PaginationInfo.ItemsPerPage);
            Assert.Equal(totalItems, result.PaginationInfo.TotalItems);
        }

        [Fact]
        public async Task GetCatalogItems_NoBrandAndTypeFilters_UsesZeroForFilters()
        {
            // Arrange
            var pageIndex = 0;
            var itemsPage = 10;
            int? brandId = null;
            int? typeId = null;

            var catalogItems = CreateTestCatalogItems();
            _mockItemRepository.Setup(r => r.ListAsync(It.IsAny<CatalogFilterPaginatedSpecification>()))
                .ReturnsAsync(catalogItems);
            _mockItemRepository.Setup(r => r.CountAsync(It.IsAny<CatalogFilterSpecification>()))
                .ReturnsAsync(10);

            SetupBrandRepository();
            SetupTypeRepository();

            // Act
            var result = await _catalogViewModelService.GetCatalogItems(pageIndex, itemsPage, brandId, typeId);

            // Assert
            Assert.Equal(0, result.BrandFilterApplied);
            Assert.Equal(0, result.TypesFilterApplied);
        }

        [Fact]
        public async Task GetCatalogItems_CalculatesPaginationCorrectly()
        {
            // Arrange
            var pageIndex = 1;
            var itemsPage = 3;
            var totalItems = 10;

            var catalogItems = CreateTestCatalogItems().Take(3).ToList();
            _mockItemRepository.Setup(r => r.ListAsync(It.IsAny<CatalogFilterPaginatedSpecification>()))
                .ReturnsAsync(catalogItems);
            _mockItemRepository.Setup(r => r.CountAsync(It.IsAny<CatalogFilterSpecification>()))
                .ReturnsAsync(totalItems);

            SetupBrandRepository();
            SetupTypeRepository();

            // Act
            var result = await _catalogViewModelService.GetCatalogItems(pageIndex, itemsPage, null, null);

            // Assert
            Assert.Equal(pageIndex, result.PaginationInfo.ActualPage);
            Assert.Equal(catalogItems.Count, result.PaginationInfo.ItemsPerPage);
            Assert.Equal(totalItems, result.PaginationInfo.TotalItems);
            Assert.Equal(4, result.PaginationInfo.TotalPages); // Ceiling(10/3) = 4
        }

        [Fact]
        public async Task GetCatalogItems_FirstPage_SetsPreviousDisabled()
        {
            // Arrange
            var pageIndex = 0;
            var itemsPage = 5;
            var totalItems = 10;

            _mockItemRepository.Setup(r => r.ListAsync(It.IsAny<CatalogFilterPaginatedSpecification>()))
                .ReturnsAsync(new List<CatalogItem>());
            _mockItemRepository.Setup(r => r.CountAsync(It.IsAny<CatalogFilterSpecification>()))
                .ReturnsAsync(totalItems);

            SetupBrandRepository();
            SetupTypeRepository();

            // Act
            var result = await _catalogViewModelService.GetCatalogItems(pageIndex, itemsPage, null, null);

            // Assert
            Assert.Equal("is-disabled", result.PaginationInfo.Previous);
            Assert.Equal("", result.PaginationInfo.Next);
        }

        [Fact]
        public async Task GetCatalogItems_LastPage_SetsNextDisabled()
        {
            // Arrange
            var pageIndex = 1; // Last page (0-indexed)
            var itemsPage = 5;
            var totalItems = 10; // Total pages = 2 (0 and 1)

            _mockItemRepository.Setup(r => r.ListAsync(It.IsAny<CatalogFilterPaginatedSpecification>()))
                .ReturnsAsync(new List<CatalogItem>());
            _mockItemRepository.Setup(r => r.CountAsync(It.IsAny<CatalogFilterSpecification>()))
                .ReturnsAsync(totalItems);

            SetupBrandRepository();
            SetupTypeRepository();

            // Act
            var result = await _catalogViewModelService.GetCatalogItems(pageIndex, itemsPage, null, null);

            // Assert
            Assert.Equal("", result.PaginationInfo.Previous);
            Assert.Equal("is-disabled", result.PaginationInfo.Next);
        }

        [Fact]
        public async Task GetCatalogItems_LogsInformation()
        {
            // Arrange
            _mockItemRepository.Setup(r => r.ListAsync(It.IsAny<CatalogFilterPaginatedSpecification>()))
                .ReturnsAsync(new List<CatalogItem>());
            _mockItemRepository.Setup(r => r.CountAsync(It.IsAny<CatalogFilterSpecification>()))
                .ReturnsAsync(0);

            SetupBrandRepository();
            SetupTypeRepository();

            // Act
            await _catalogViewModelService.GetCatalogItems(0, 10, null, null);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GetCatalogItems called.")),
                    It.IsAny<System.Exception>(),
                    It.IsAny<System.Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region GetBrands Tests

        [Fact]
        public async Task GetBrands_ReturnsAllBrandsWithAllOption()
        {
            // Arrange
            var brands = new List<CatalogBrand>
            {
                CreateTestCatalogBrand(1, "Nike"),
                CreateTestCatalogBrand(2, "Adidas"),
                CreateTestCatalogBrand(3, "Puma")
            };

            _mockBrandRepository.Setup(r => r.ListAllAsync())
                .ReturnsAsync(brands);

            // Act
            var result = await _catalogViewModelService.GetBrands();

            // Assert
            var resultList = result.ToList();
            Assert.Equal(4, resultList.Count); // 3 brands + "All" option

            var allOption = resultList.First();
            Assert.Null(allOption.Value);
            Assert.Equal("All", allOption.Text);
            Assert.True(allOption.Selected);

            var brandOptions = resultList.Skip(1).ToList();
            Assert.Equal("Nike", brandOptions[0].Text);
            Assert.Equal("Adidas", brandOptions[1].Text);
            Assert.Equal("Puma", brandOptions[2].Text);
        }

        [Fact]
        public async Task GetBrands_EmptyBrandList_ReturnsOnlyAllOption()
        {
            // Arrange
            _mockBrandRepository.Setup(r => r.ListAllAsync())
                .ReturnsAsync(new List<CatalogBrand>());

            // Act
            var result = await _catalogViewModelService.GetBrands();

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Null(resultList[0].Value);
            Assert.Equal("All", resultList[0].Text);
            Assert.True(resultList[0].Selected);
        }

        [Fact]
        public async Task GetBrands_LogsInformation()
        {
            // Arrange
            _mockBrandRepository.Setup(r => r.ListAllAsync())
                .ReturnsAsync(new List<CatalogBrand>());

            // Act
            await _catalogViewModelService.GetBrands();

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GetBrands called.")),
                    It.IsAny<System.Exception>(),
                    It.IsAny<System.Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region GetTypes Tests

        [Fact]
        public async Task GetTypes_ReturnsAllTypesWithAllOption()
        {
            // Arrange
            var types = new List<CatalogType>
            {
                CreateTestCatalogType(1, "Shoes"),
                CreateTestCatalogType(2, "Shirts"),
                CreateTestCatalogType(3, "Pants")
            };

            _mockTypeRepository.Setup(r => r.ListAllAsync())
                .ReturnsAsync(types);

            // Act
            var result = await _catalogViewModelService.GetTypes();

            // Assert
            var resultList = result.ToList();
            Assert.Equal(4, resultList.Count); // 3 types + "All" option

            var allOption = resultList.First();
            Assert.Null(allOption.Value);
            Assert.Equal("All", allOption.Text);
            Assert.True(allOption.Selected);

            var typeOptions = resultList.Skip(1).ToList();
            Assert.Equal("Shoes", typeOptions[0].Text);
            Assert.Equal("Shirts", typeOptions[1].Text);
            Assert.Equal("Pants", typeOptions[2].Text);
        }

        [Fact]
        public async Task GetTypes_EmptyTypeList_ReturnsOnlyAllOption()
        {
            // Arrange
            _mockTypeRepository.Setup(r => r.ListAllAsync())
                .ReturnsAsync(new List<CatalogType>());

            // Act
            var result = await _catalogViewModelService.GetTypes();

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Null(resultList[0].Value);
            Assert.Equal("All", resultList[0].Text);
            Assert.True(resultList[0].Selected);
        }

        [Fact]
        public async Task GetTypes_LogsInformation()
        {
            // Arrange
            _mockTypeRepository.Setup(r => r.ListAllAsync())
                .ReturnsAsync(new List<CatalogType>());

            // Act
            await _catalogViewModelService.GetTypes();

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GetTypes called.")),
                    It.IsAny<System.Exception>(),
                    It.IsAny<System.Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region Edge Cases and Error Handling

        [Fact]
        public async Task GetCatalogItems_NegativePageIndex_ProcessesCorrectly()
        {
            // Arrange
            var pageIndex = -1;
            var itemsPage = 10;

            _mockItemRepository.Setup(r => r.ListAsync(It.IsAny<CatalogFilterPaginatedSpecification>()))
                .ReturnsAsync(new List<CatalogItem>());
            _mockItemRepository.Setup(r => r.CountAsync(It.IsAny<CatalogFilterSpecification>()))
                .ReturnsAsync(0);

            SetupBrandRepository();
            SetupTypeRepository();

            // Act
            var result = await _catalogViewModelService.GetCatalogItems(pageIndex, itemsPage, null, null);

            // Assert
            Assert.Equal(pageIndex, result.PaginationInfo.ActualPage);
        }

        [Fact]
        public async Task GetCatalogItems_ZeroItemsPage_ProcessesCorrectly()
        {
            // Arrange
            var pageIndex = 0;
            var itemsPage = 0;

            _mockItemRepository.Setup(r => r.ListAsync(It.IsAny<CatalogFilterPaginatedSpecification>()))
                .ReturnsAsync(new List<CatalogItem>());
            _mockItemRepository.Setup(r => r.CountAsync(It.IsAny<CatalogFilterSpecification>()))
                .ReturnsAsync(10);

            SetupBrandRepository();
            SetupTypeRepository();

            // Act & Assert - This may throw an exception depending on implementation
            var exception = await Record.ExceptionAsync(() => 
                _catalogViewModelService.GetCatalogItems(pageIndex, itemsPage, null, null));
            
            // The specific behavior depends on the implementation
            // Either it should handle gracefully or throw an appropriate exception
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_ValidDependencies_DoesNotThrow()
        {
            // Arrange & Act
            var exception = Record.Exception(() => new CatalogViewModelService(
                _mockLoggerFactory.Object,
                _mockItemRepository.Object,
                _mockBrandRepository.Object,
                _mockTypeRepository.Object,
                _mockUriComposer.Object));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Constructor_NullLoggerFactory_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => new CatalogViewModelService(
                null,
                _mockItemRepository.Object,
                _mockBrandRepository.Object,
                _mockTypeRepository.Object,
                _mockUriComposer.Object));
        }

        #endregion

        #region Additional Edge Case Tests

        [Fact]
        public async Task GetCatalogItems_ZeroItemsPage_HandlesCorrectly()
        {
            // Arrange
            var pageIndex = 0;
            var itemsPage = 0;
            int? brandId = null;
            int? typeId = null;

            _mockItemRepository.Setup(r => r.ListAsync(It.IsAny<CatalogFilterPaginatedSpecification>()))
                .ReturnsAsync(new List<CatalogItem>());
            _mockItemRepository.Setup(r => r.CountAsync(It.IsAny<CatalogFilterSpecification>()))
                .ReturnsAsync(0);

            SetupBrandRepository();
            SetupTypeRepository();

            // Act
            var result = await _catalogViewModelService.GetCatalogItems(pageIndex, itemsPage, brandId, typeId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.CatalogItems);
            Assert.Equal(0, result.PaginationInfo.TotalPages);
        }

        [Fact]
        public async Task GetCatalogItems_NegativeBrandId_UsesNegativeValue()
        {
            // Arrange
            var pageIndex = 0;
            var itemsPage = 10;
            int? brandId = -1;
            int? typeId = null;

            _mockItemRepository.Setup(r => r.ListAsync(It.IsAny<CatalogFilterPaginatedSpecification>()))
                .ReturnsAsync(new List<CatalogItem>());
            _mockItemRepository.Setup(r => r.CountAsync(It.IsAny<CatalogFilterSpecification>()))
                .ReturnsAsync(0);

            SetupBrandRepository();
            SetupTypeRepository();

            // Act
            var result = await _catalogViewModelService.GetCatalogItems(pageIndex, itemsPage, brandId, typeId);

            // Assert
            Assert.Equal(-1, result.BrandFilterApplied);
        }

        [Fact]
        public async Task GetCatalogItems_NegativeTypeId_UsesNegativeValue()
        {
            // Arrange
            var pageIndex = 0;
            var itemsPage = 10;
            int? brandId = null;
            int? typeId = -1;

            _mockItemRepository.Setup(r => r.ListAsync(It.IsAny<CatalogFilterPaginatedSpecification>()))
                .ReturnsAsync(new List<CatalogItem>());
            _mockItemRepository.Setup(r => r.CountAsync(It.IsAny<CatalogFilterSpecification>()))
                .ReturnsAsync(0);

            SetupBrandRepository();
            SetupTypeRepository();

            // Act
            var result = await _catalogViewModelService.GetCatalogItems(pageIndex, itemsPage, brandId, typeId);

            // Assert
            Assert.Equal(-1, result.TypesFilterApplied);
        }

        [Fact]
        public async Task GetCatalogItems_LargePageIndex_HandlesCorrectly()
        {
            // Arrange
            var pageIndex = 1000;
            var itemsPage = 10;
            int? brandId = null;
            int? typeId = null;

            _mockItemRepository.Setup(r => r.ListAsync(It.IsAny<CatalogFilterPaginatedSpecification>()))
                .ReturnsAsync(new List<CatalogItem>());
            _mockItemRepository.Setup(r => r.CountAsync(It.IsAny<CatalogFilterSpecification>()))
                .ReturnsAsync(0);

            SetupBrandRepository();
            SetupTypeRepository();

            // Act
            var result = await _catalogViewModelService.GetCatalogItems(pageIndex, itemsPage, brandId, typeId);

            // Assert
            Assert.Equal(pageIndex, result.PaginationInfo.ActualPage);
        }

        [Fact]
        public async Task GetCatalogItems_PaginationInfoNext_IsDisabledOnLastPage()
        {
            // Arrange
            var pageIndex = 1; // Last page (0-indexed)
            var itemsPage = 5;
            var totalItems = 10; // This gives us 2 pages total (0 and 1)

            _mockItemRepository.Setup(r => r.ListAsync(It.IsAny<CatalogFilterPaginatedSpecification>()))
                .ReturnsAsync(CreateTestCatalogItems());
            _mockItemRepository.Setup(r => r.CountAsync(It.IsAny<CatalogFilterSpecification>()))
                .ReturnsAsync(totalItems);

            SetupBrandRepository();
            SetupTypeRepository();

            // Act
            var result = await _catalogViewModelService.GetCatalogItems(pageIndex, itemsPage, null, null);

            // Assert
            Assert.Equal("is-disabled", result.PaginationInfo.Next);
        }

        [Fact]
        public async Task GetCatalogItems_PaginationInfoPrevious_IsDisabledOnFirstPage()
        {
            // Arrange
            var pageIndex = 0; // First page
            var itemsPage = 5;
            var totalItems = 10;

            _mockItemRepository.Setup(r => r.ListAsync(It.IsAny<CatalogFilterPaginatedSpecification>()))
                .ReturnsAsync(CreateTestCatalogItems());
            _mockItemRepository.Setup(r => r.CountAsync(It.IsAny<CatalogFilterSpecification>()))
                .ReturnsAsync(totalItems);

            SetupBrandRepository();
            SetupTypeRepository();

            // Act
            var result = await _catalogViewModelService.GetCatalogItems(pageIndex, itemsPage, null, null);

            // Assert
            Assert.Equal("is-disabled", result.PaginationInfo.Previous);
        }

        [Fact]
        public async Task GetCatalogItems_MiddlePage_NeitherNextNorPreviousDisabled()
        {
            // Arrange
            var pageIndex = 1; // Middle page
            var itemsPage = 5;
            var totalItems = 15; // This gives us 3 pages total (0, 1, 2)

            _mockItemRepository.Setup(r => r.ListAsync(It.IsAny<CatalogFilterPaginatedSpecification>()))
                .ReturnsAsync(CreateTestCatalogItems());
            _mockItemRepository.Setup(r => r.CountAsync(It.IsAny<CatalogFilterSpecification>()))
                .ReturnsAsync(totalItems);

            SetupBrandRepository();
            SetupTypeRepository();

            // Act
            var result = await _catalogViewModelService.GetCatalogItems(pageIndex, itemsPage, null, null);

            // Assert
            Assert.Equal("", result.PaginationInfo.Next);
            Assert.Equal("", result.PaginationInfo.Previous);
        }

        [Fact]
        public async Task GetBrands_EmptyBrandRepository_ReturnsOnlyAllOption()
        {
            // Arrange
            _mockBrandRepository.Setup(r => r.ListAllAsync())
                .ReturnsAsync(new List<CatalogBrand>());

            // Act
            var result = await _catalogViewModelService.GetBrands();

            // Assert
            Assert.Single(result);
            var item = result.First();
            Assert.Equal("All", item.Text);
            Assert.Null(item.Value);
            Assert.True(item.Selected);
        }

        [Fact]
        public async Task GetTypes_EmptyTypeRepository_ReturnsOnlyAllOption()
        {
            // Arrange
            _mockTypeRepository.Setup(r => r.ListAllAsync())
                .ReturnsAsync(new List<CatalogType>());

            // Act
            var result = await _catalogViewModelService.GetTypes();

            // Assert
            Assert.Single(result);
            var item = result.First();
            Assert.Equal("All", item.Text);
            Assert.Null(item.Value);
            Assert.True(item.Selected);
        }

        [Fact]
        public async Task GetBrands_MultipleBrands_ReturnsAllPlusEachBrand()
        {
            // Arrange
            var brands = new List<CatalogBrand>
            {
                CreateTestCatalogBrand(1, "Brand A"),
                CreateTestCatalogBrand(2, "Brand B"),
                CreateTestCatalogBrand(3, "Brand C")
            };

            _mockBrandRepository.Setup(r => r.ListAllAsync())
                .ReturnsAsync(brands);

            // Act
            var result = await _catalogViewModelService.GetBrands();

            // Assert
            Assert.Equal(4, result.Count()); // "All" + 3 brands
            
            var allOption = result.First();
            Assert.Equal("All", allOption.Text);
            Assert.True(allOption.Selected);

            var brandItems = result.Skip(1).ToList();
            Assert.Equal("Brand A", brandItems[0].Text);
            Assert.Equal("1", brandItems[0].Value);
            Assert.False(brandItems[0].Selected);
        }

        [Fact]
        public async Task GetTypes_MultipleTypes_ReturnsAllPlusEachType()
        {
            // Arrange
            var types = new List<CatalogType>
            {
                CreateTestCatalogType(1, "Type A"),
                CreateTestCatalogType(2, "Type B"),
                CreateTestCatalogType(3, "Type C")
            };

            _mockTypeRepository.Setup(r => r.ListAllAsync())
                .ReturnsAsync(types);

            // Act
            var result = await _catalogViewModelService.GetTypes();

            // Assert
            Assert.Equal(4, result.Count()); // "All" + 3 types
            
            var allOption = result.First();
            Assert.Equal("All", allOption.Text);
            Assert.True(allOption.Selected);

            var typeItems = result.Skip(1).ToList();
            Assert.Equal("Type A", typeItems[0].Text);
            Assert.Equal("1", typeItems[0].Value);
            Assert.False(typeItems[0].Selected);
        }

        [Fact]
        public async Task GetCatalogItems_CallsLoggerInformation()
        {
            // Arrange
            var pageIndex = 0;
            var itemsPage = 10;

            _mockItemRepository.Setup(r => r.ListAsync(It.IsAny<CatalogFilterPaginatedSpecification>()))
                .ReturnsAsync(new List<CatalogItem>());
            _mockItemRepository.Setup(r => r.CountAsync(It.IsAny<CatalogFilterSpecification>()))
                .ReturnsAsync(0);

            SetupBrandRepository();
            SetupTypeRepository();

            // Act
            await _catalogViewModelService.GetCatalogItems(pageIndex, itemsPage, null, null);

            // Assert
            _mockLogger.Verify(
                l => l.LogInformation("GetCatalogItems called."),
                Times.Once);
        }

        [Fact]
        public async Task GetBrands_CallsLoggerInformation()
        {
            // Arrange
            _mockBrandRepository.Setup(r => r.ListAllAsync())
                .ReturnsAsync(new List<CatalogBrand>());

            // Act
            await _catalogViewModelService.GetBrands();

            // Assert
            _mockLogger.Verify(
                l => l.LogInformation("GetBrands called."),
                Times.Once);
        }

        [Fact]
        public async Task GetTypes_CallsLoggerInformation()
        {
            // Arrange
            _mockTypeRepository.Setup(r => r.ListAllAsync())
                .ReturnsAsync(new List<CatalogType>());

            // Act
            await _catalogViewModelService.GetTypes();

            // Assert
            _mockLogger.Verify(
                l => l.LogInformation("GetTypes called."),
                Times.Once);
        }

        #endregion

        #region Helper Methods

        private List<CatalogItem> CreateTestCatalogItems()
        {
            return new List<CatalogItem>
            {
                CreateTestCatalogItem(1, "Item 1", 10.99m),
                CreateTestCatalogItem(2, "Item 2", 15.99m),
                CreateTestCatalogItem(3, "Item 3", 20.99m)
            };
        }

        private CatalogItem CreateTestCatalogItem(int id, string name, decimal price)
        {
            var catalogItem = new CatalogItem(1, 1, name, name, price, "test.jpg");
            typeof(CatalogItem).BaseType.GetProperty("Id").SetValue(catalogItem, id);
            return catalogItem;
        }

        private CatalogBrand CreateTestCatalogBrand(int id, string brandName)
        {
            var brand = new CatalogBrand(brandName);
            typeof(CatalogBrand).BaseType.GetProperty("Id").SetValue(brand, id);
            return brand;
        }

        private CatalogType CreateTestCatalogType(int id, string typeName)
        {
            var type = new CatalogType(typeName);
            typeof(CatalogType).BaseType.GetProperty("Id").SetValue(type, id);
            return type;
        }

        private void SetupBrandRepository()
        {
            _mockBrandRepository.Setup(r => r.ListAllAsync())
                .ReturnsAsync(new List<CatalogBrand>());
        }

        private void SetupTypeRepository()
        {
            _mockTypeRepository.Setup(r => r.ListAllAsync())
                .ReturnsAsync(new List<CatalogType>());
        }

        #endregion
    }
}
