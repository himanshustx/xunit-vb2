using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.eShopWeb.Web.Services;
using Microsoft.eShopWeb.Web.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace test_c__new.Services
{
    public class CachedCatalogViewModelServiceTests
    {
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<CatalogViewModelService> _mockCatalogViewModelService;
        private readonly CachedCatalogViewModelService _cachedCatalogViewModelService;

        public CachedCatalogViewModelServiceTests()
        {
            _mockCache = new Mock<IMemoryCache>();
            _mockCatalogViewModelService = new Mock<CatalogViewModelService>();
            _cachedCatalogViewModelService = new CachedCatalogViewModelService(
                _mockCache.Object,
                _mockCatalogViewModelService.Object);
        }

        #region GetBrands Tests

        [Fact]
        public async Task GetBrands_CacheHit_ReturnsCachedResult()
        {
            // Arrange
            var expectedBrands = CreateTestSelectListItems("Brand");
            var cacheKey = "brands";

            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.Is<object>(k => k.Equals(cacheKey)),
                It.IsAny<Func<ICacheEntry, Task<IEnumerable<SelectListItem>>>>()))
                .ReturnsAsync(expectedBrands);

            // Act
            var result = await _cachedCatalogViewModelService.GetBrands();

            // Assert
            Assert.Equal(expectedBrands, result);
            _mockCache.Verify(c => c.GetOrCreateAsync(
                cacheKey,
                It.IsAny<Func<ICacheEntry, Task<IEnumerable<SelectListItem>>>>()), Times.Once);
        }

        [Fact]
        public async Task GetBrands_CacheMiss_CallsUnderlyingServiceAndCaches()
        {
            // Arrange
            var expectedBrands = CreateTestSelectListItems("Brand");
            var cacheKey = "brands";
            var mockCacheEntry = new Mock<ICacheEntry>();

            Func<ICacheEntry, Task<IEnumerable<SelectListItem>>> factoryFunction = null;

            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.Is<object>(k => k.Equals(cacheKey)),
                It.IsAny<Func<ICacheEntry, Task<IEnumerable<SelectListItem>>>>()))
                .Callback<object, Func<ICacheEntry, Task<IEnumerable<SelectListItem>>>>((key, factory) => factoryFunction = factory)
                .Returns<object, Func<ICacheEntry, Task<IEnumerable<SelectListItem>>>>((key, factory) => factory(mockCacheEntry.Object));

            _mockCatalogViewModelService.Setup(s => s.GetBrands())
                .ReturnsAsync(expectedBrands);

            // Act
            var result = await _cachedCatalogViewModelService.GetBrands();

            // Assert
            Assert.Equal(expectedBrands, result);
            _mockCatalogViewModelService.Verify(s => s.GetBrands(), Times.Once);
            mockCacheEntry.VerifySet(e => e.SlidingExpiration = TimeSpan.FromSeconds(30), Times.Once);
        }

        [Fact]
        public async Task GetBrands_SetsCacheDuration()
        {
            // Arrange
            var expectedBrands = CreateTestSelectListItems("Brand");
            var mockCacheEntry = new Mock<ICacheEntry>();

            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.IsAny<object>(),
                It.IsAny<Func<ICacheEntry, Task<IEnumerable<SelectListItem>>>>()))
                .Returns<object, Func<ICacheEntry, Task<IEnumerable<SelectListItem>>>>((key, factory) => factory(mockCacheEntry.Object));

            _mockCatalogViewModelService.Setup(s => s.GetBrands())
                .ReturnsAsync(expectedBrands);

            // Act
            await _cachedCatalogViewModelService.GetBrands();

            // Assert
            mockCacheEntry.VerifySet(e => e.SlidingExpiration = TimeSpan.FromSeconds(30), Times.Once);
        }

        #endregion

        #region GetTypes Tests

        [Fact]
        public async Task GetTypes_CacheHit_ReturnsCachedResult()
        {
            // Arrange
            var expectedTypes = CreateTestSelectListItems("Type");
            var cacheKey = "types";

            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.Is<object>(k => k.Equals(cacheKey)),
                It.IsAny<Func<ICacheEntry, Task<IEnumerable<SelectListItem>>>>()))
                .ReturnsAsync(expectedTypes);

            // Act
            var result = await _cachedCatalogViewModelService.GetTypes();

            // Assert
            Assert.Equal(expectedTypes, result);
            _mockCache.Verify(c => c.GetOrCreateAsync(
                cacheKey,
                It.IsAny<Func<ICacheEntry, Task<IEnumerable<SelectListItem>>>>()), Times.Once);
        }

        [Fact]
        public async Task GetTypes_CacheMiss_CallsUnderlyingServiceAndCaches()
        {
            // Arrange
            var expectedTypes = CreateTestSelectListItems("Type");
            var cacheKey = "types";
            var mockCacheEntry = new Mock<ICacheEntry>();

            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.Is<object>(k => k.Equals(cacheKey)),
                It.IsAny<Func<ICacheEntry, Task<IEnumerable<SelectListItem>>>>()))
                .Returns<object, Func<ICacheEntry, Task<IEnumerable<SelectListItem>>>>((key, factory) => factory(mockCacheEntry.Object));

            _mockCatalogViewModelService.Setup(s => s.GetTypes())
                .ReturnsAsync(expectedTypes);

            // Act
            var result = await _cachedCatalogViewModelService.GetTypes();

            // Assert
            Assert.Equal(expectedTypes, result);
            _mockCatalogViewModelService.Verify(s => s.GetTypes(), Times.Once);
            mockCacheEntry.VerifySet(e => e.SlidingExpiration = TimeSpan.FromSeconds(30), Times.Once);
        }

        [Fact]
        public async Task GetTypes_SetsCacheDuration()
        {
            // Arrange
            var expectedTypes = CreateTestSelectListItems("Type");
            var mockCacheEntry = new Mock<ICacheEntry>();

            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.IsAny<object>(),
                It.IsAny<Func<ICacheEntry, Task<IEnumerable<SelectListItem>>>>()))
                .Returns<object, Func<ICacheEntry, Task<IEnumerable<SelectListItem>>>>((key, factory) => factory(mockCacheEntry.Object));

            _mockCatalogViewModelService.Setup(s => s.GetTypes())
                .ReturnsAsync(expectedTypes);

            // Act
            await _cachedCatalogViewModelService.GetTypes();

            // Assert
            mockCacheEntry.VerifySet(e => e.SlidingExpiration = TimeSpan.FromSeconds(30), Times.Once);
        }

        #endregion

        #region GetCatalogItems Tests

        [Fact]
        public async Task GetCatalogItems_CacheHit_ReturnsCachedResult()
        {
            // Arrange
            var pageIndex = 0;
            var itemsPage = 10;
            int? brandId = 1;
            int? typeId = 2;
            var expectedViewModel = CreateTestCatalogIndexViewModel();
            var expectedCacheKey = string.Format("items-{0}-{1}-{2}-{3}", pageIndex, itemsPage, brandId, typeId);

            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.Is<object>(k => k.Equals(expectedCacheKey)),
                It.IsAny<Func<ICacheEntry, Task<CatalogIndexViewModel>>>()))
                .ReturnsAsync(expectedViewModel);

            // Act
            var result = await _cachedCatalogViewModelService.GetCatalogItems(pageIndex, itemsPage, brandId, typeId);

            // Assert
            Assert.Equal(expectedViewModel, result);
            _mockCache.Verify(c => c.GetOrCreateAsync(
                expectedCacheKey,
                It.IsAny<Func<ICacheEntry, Task<CatalogIndexViewModel>>>()), Times.Once);
        }

        [Fact]
        public async Task GetCatalogItems_CacheMiss_CallsUnderlyingServiceAndCaches()
        {
            // Arrange
            var pageIndex = 1;
            var itemsPage = 5;
            int? brandId = 3;
            int? typeId = 4;
            var expectedViewModel = CreateTestCatalogIndexViewModel();
            var expectedCacheKey = string.Format("items-{0}-{1}-{2}-{3}", pageIndex, itemsPage, brandId, typeId);
            var mockCacheEntry = new Mock<ICacheEntry>();

            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.Is<object>(k => k.Equals(expectedCacheKey)),
                It.IsAny<Func<ICacheEntry, Task<CatalogIndexViewModel>>>()))
                .Returns<object, Func<ICacheEntry, Task<CatalogIndexViewModel>>>((key, factory) => factory(mockCacheEntry.Object));

            _mockCatalogViewModelService.Setup(s => s.GetCatalogItems(pageIndex, itemsPage, brandId, typeId))
                .ReturnsAsync(expectedViewModel);

            // Act
            var result = await _cachedCatalogViewModelService.GetCatalogItems(pageIndex, itemsPage, brandId, typeId);

            // Assert
            Assert.Equal(expectedViewModel, result);
            _mockCatalogViewModelService.Verify(s => s.GetCatalogItems(pageIndex, itemsPage, brandId, typeId), Times.Once);
            mockCacheEntry.VerifySet(e => e.SlidingExpiration = TimeSpan.FromSeconds(30), Times.Once);
        }

        [Fact]
        public async Task GetCatalogItems_NullBrandAndType_GeneratesCorrectCacheKey()
        {
            // Arrange
            var pageIndex = 0;
            var itemsPage = 10;
            int? brandId = null;
            int? typeId = null;
            var expectedViewModel = CreateTestCatalogIndexViewModel();
            var expectedCacheKey = string.Format("items-{0}-{1}-{2}-{3}", pageIndex, itemsPage, brandId, typeId);

            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.Is<object>(k => k.Equals(expectedCacheKey)),
                It.IsAny<Func<ICacheEntry, Task<CatalogIndexViewModel>>>()))
                .ReturnsAsync(expectedViewModel);

            // Act
            var result = await _cachedCatalogViewModelService.GetCatalogItems(pageIndex, itemsPage, brandId, typeId);

            // Assert
            _mockCache.Verify(c => c.GetOrCreateAsync(
                expectedCacheKey,
                It.IsAny<Func<ICacheEntry, Task<CatalogIndexViewModel>>>()), Times.Once);
        }

        [Fact]
        public async Task GetCatalogItems_DifferentParameters_GeneratesDifferentCacheKeys()
        {
            // Arrange
            var expectedViewModel = CreateTestCatalogIndexViewModel();
            var mockCacheEntry = new Mock<ICacheEntry>();

            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.IsAny<object>(),
                It.IsAny<Func<ICacheEntry, Task<CatalogIndexViewModel>>>()))
                .Returns<object, Func<ICacheEntry, Task<CatalogIndexViewModel>>>((key, factory) => factory(mockCacheEntry.Object));

            _mockCatalogViewModelService.Setup(s => s.GetCatalogItems(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(expectedViewModel);

            // Act
            await _cachedCatalogViewModelService.GetCatalogItems(0, 10, 1, 1);
            await _cachedCatalogViewModelService.GetCatalogItems(1, 10, 1, 1);
            await _cachedCatalogViewModelService.GetCatalogItems(0, 5, 1, 1);
            await _cachedCatalogViewModelService.GetCatalogItems(0, 10, 2, 1);
            await _cachedCatalogViewModelService.GetCatalogItems(0, 10, 1, 2);

            // Assert - Each call should use a different cache key
            _mockCache.Verify(c => c.GetOrCreateAsync(
                "items-0-10-1-1",
                It.IsAny<Func<ICacheEntry, Task<CatalogIndexViewModel>>>()), Times.Once);
            _mockCache.Verify(c => c.GetOrCreateAsync(
                "items-1-10-1-1",
                It.IsAny<Func<ICacheEntry, Task<CatalogIndexViewModel>>>()), Times.Once);
            _mockCache.Verify(c => c.GetOrCreateAsync(
                "items-0-5-1-1",
                It.IsAny<Func<ICacheEntry, Task<CatalogIndexViewModel>>>()), Times.Once);
            _mockCache.Verify(c => c.GetOrCreateAsync(
                "items-0-10-2-1",
                It.IsAny<Func<ICacheEntry, Task<CatalogIndexViewModel>>>()), Times.Once);
            _mockCache.Verify(c => c.GetOrCreateAsync(
                "items-0-10-1-2",
                It.IsAny<Func<ICacheEntry, Task<CatalogIndexViewModel>>>()), Times.Once);
        }

        [Fact]
        public async Task GetCatalogItems_SetsCacheDuration()
        {
            // Arrange
            var expectedViewModel = CreateTestCatalogIndexViewModel();
            var mockCacheEntry = new Mock<ICacheEntry>();

            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.IsAny<object>(),
                It.IsAny<Func<ICacheEntry, Task<CatalogIndexViewModel>>>()))
                .Returns<object, Func<ICacheEntry, Task<CatalogIndexViewModel>>>((key, factory) => factory(mockCacheEntry.Object));

            _mockCatalogViewModelService.Setup(s => s.GetCatalogItems(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(expectedViewModel);

            // Act
            await _cachedCatalogViewModelService.GetCatalogItems(0, 10, null, null);

            // Assert
            mockCacheEntry.VerifySet(e => e.SlidingExpiration = TimeSpan.FromSeconds(30), Times.Once);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_ValidDependencies_DoesNotThrow()
        {
            // Arrange & Act
            var exception = Record.Exception(() => new CachedCatalogViewModelService(
                _mockCache.Object,
                _mockCatalogViewModelService.Object));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Constructor_NullCache_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new CachedCatalogViewModelService(
                null,
                _mockCatalogViewModelService.Object));
        }

        [Fact]
        public void Constructor_NullCatalogViewModelService_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new CachedCatalogViewModelService(
                _mockCache.Object,
                null));
        }

        #endregion

        #region Cache Key Tests

        [Fact]
        public async Task GetCatalogItems_NegativeValues_GeneratesCorrectCacheKey()
        {
            // Arrange
            var pageIndex = -1;
            var itemsPage = -5;
            int? brandId = -10;
            int? typeId = -20;
            var expectedCacheKey = string.Format("items-{0}-{1}-{2}-{3}", pageIndex, itemsPage, brandId, typeId);
            var mockCacheEntry = new Mock<ICacheEntry>();

            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.Is<object>(k => k.Equals(expectedCacheKey)),
                It.IsAny<Func<ICacheEntry, Task<CatalogIndexViewModel>>>()))
                .Returns<object, Func<ICacheEntry, Task<CatalogIndexViewModel>>>((key, factory) => factory(mockCacheEntry.Object));

            _mockCatalogViewModelService.Setup(s => s.GetCatalogItems(pageIndex, itemsPage, brandId, typeId))
                .ReturnsAsync(CreateTestCatalogIndexViewModel());

            // Act
            await _cachedCatalogViewModelService.GetCatalogItems(pageIndex, itemsPage, brandId, typeId);

            // Assert
            _mockCache.Verify(c => c.GetOrCreateAsync(
                expectedCacheKey,
                It.IsAny<Func<ICacheEntry, Task<CatalogIndexViewModel>>>()), Times.Once);
        }

        [Fact]
        public async Task GetCatalogItems_ZeroValues_GeneratesCorrectCacheKey()
        {
            // Arrange
            var pageIndex = 0;
            var itemsPage = 0;
            int? brandId = 0;
            int? typeId = 0;
            var expectedCacheKey = "items-0-0-0-0";
            var mockCacheEntry = new Mock<ICacheEntry>();

            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.Is<object>(k => k.Equals(expectedCacheKey)),
                It.IsAny<Func<ICacheEntry, Task<CatalogIndexViewModel>>>()))
                .Returns<object, Func<ICacheEntry, Task<CatalogIndexViewModel>>>((key, factory) => factory(mockCacheEntry.Object));

            _mockCatalogViewModelService.Setup(s => s.GetCatalogItems(pageIndex, itemsPage, brandId, typeId))
                .ReturnsAsync(CreateTestCatalogIndexViewModel());

            // Act
            await _cachedCatalogViewModelService.GetCatalogItems(pageIndex, itemsPage, brandId, typeId);

            // Assert
            _mockCache.Verify(c => c.GetOrCreateAsync(
                expectedCacheKey,
                It.IsAny<Func<ICacheEntry, Task<CatalogIndexViewModel>>>()), Times.Once);
        }

        #endregion

        #region Additional Edge Case Tests

        [Fact]
        public async Task GetBrands_EmptyBrandsList_CachesEmptyResult()
        {
            // Arrange
            var expectedBrands = new List<SelectListItem>();
            var cacheKey = "brands";

            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.Is<object>(k => k.Equals(cacheKey)),
                It.IsAny<Func<ICacheEntry, Task<IEnumerable<SelectListItem>>>>()))
                .ReturnsAsync(expectedBrands);

            // Act
            var result = await _cachedCatalogViewModelService.GetBrands();

            // Assert
            Assert.Empty(result);
            _mockCache.Verify(c => c.GetOrCreateAsync(
                cacheKey,
                It.IsAny<Func<ICacheEntry, Task<IEnumerable<SelectListItem>>>>()), Times.Once);
        }

        [Fact]
        public async Task GetTypes_EmptyTypesList_CachesEmptyResult()
        {
            // Arrange
            var expectedTypes = new List<SelectListItem>();
            var cacheKey = "types";

            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.Is<object>(k => k.Equals(cacheKey)),
                It.IsAny<Func<ICacheEntry, Task<IEnumerable<SelectListItem>>>>()))
                .ReturnsAsync(expectedTypes);

            // Act
            var result = await _cachedCatalogViewModelService.GetTypes();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCatalogItems_ExtremePageValues_GeneratesCorrectCacheKey()
        {
            // Arrange
            var viewModel = CreateTestCatalogIndexViewModel();
            var expectedCacheKey = "items-1000-50-999-888";

            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.Is<object>(k => k.Equals(expectedCacheKey)),
                It.IsAny<Func<ICacheEntry, Task<CatalogIndexViewModel>>>()))
                .ReturnsAsync(viewModel);

            // Act
            var result = await _cachedCatalogViewModelService.GetCatalogItems(1000, 50, 999, 888);

            // Assert
            Assert.Equal(viewModel, result);
        }

        [Fact]
        public async Task GetCatalogItems_MaxIntValues_HandlesCorrectly()
        {
            // Arrange
            var viewModel = CreateTestCatalogIndexViewModel();
            var expectedCacheKey = $"items-{int.MaxValue}-{int.MaxValue}-{int.MaxValue}-{int.MaxValue}";

            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.Is<object>(k => k.Equals(expectedCacheKey)),
                It.IsAny<Func<ICacheEntry, Task<CatalogIndexViewModel>>>()))
                .ReturnsAsync(viewModel);

            // Act
            var result = await _cachedCatalogViewModelService.GetCatalogItems(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);

            // Assert
            Assert.Equal(viewModel, result);
        }

        [Fact]
        public async Task GetCatalogItems_MinIntValues_HandlesCorrectly()
        {
            // Arrange
            var viewModel = CreateTestCatalogIndexViewModel();
            var expectedCacheKey = $"items-{int.MinValue}-{int.MinValue}-{int.MinValue}-{int.MinValue}";

            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.Is<object>(k => k.Equals(expectedCacheKey)),
                It.IsAny<Func<ICacheEntry, Task<CatalogIndexViewModel>>>()))
                .ReturnsAsync(viewModel);

            // Act
            var result = await _cachedCatalogViewModelService.GetCatalogItems(int.MinValue, int.MinValue, int.MinValue, int.MinValue);

            // Assert
            Assert.Equal(viewModel, result);
        }

        [Fact]
        public async Task GetBrands_CacheThrowsException_PropagatesException()
        {
            // Arrange
            var cacheKey = "brands";
            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.Is<object>(k => k.Equals(cacheKey)),
                It.IsAny<Func<ICacheEntry, Task<IEnumerable<SelectListItem>>>>()))
                .ThrowsAsync(new InvalidOperationException("Cache error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _cachedCatalogViewModelService.GetBrands());
        }

        [Fact]
        public async Task GetCatalogItems_UnderlyingServiceReturnsNull_HandlesCorrectly()
        {
            // Arrange
            var cacheKey = "items-0-10--";

            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.Is<object>(k => k.Equals(cacheKey)),
                It.IsAny<Func<ICacheEntry, Task<CatalogIndexViewModel>>>()))
                .ReturnsAsync((CatalogIndexViewModel)null);

            // Act
            var result = await _cachedCatalogViewModelService.GetCatalogItems(0, 10, null, null);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region Helper Methods

        private IEnumerable<SelectListItem> CreateTestSelectListItems(string prefix)
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = $"{prefix} 1" },
                new SelectListItem { Value = "2", Text = $"{prefix} 2" },
                new SelectListItem { Value = "3", Text = $"{prefix} 3" }
            };
        }

        private CatalogIndexViewModel CreateTestCatalogIndexViewModel()
        {
            return new CatalogIndexViewModel
            {
                CatalogItems = new List<CatalogItemViewModel>
                {
                    new CatalogItemViewModel { Id = 1, Name = "Item 1", Price = 10.99m },
                    new CatalogItemViewModel { Id = 2, Name = "Item 2", Price = 15.99m }
                },
                Brands = CreateTestSelectListItems("Brand"),
                Types = CreateTestSelectListItems("Type"),
                BrandFilterApplied = 1,
                TypesFilterApplied = 2,
                PaginationInfo = new PaginationInfoViewModel
                {
                    ActualPage = 0,
                    ItemsPerPage = 10,
                    TotalItems = 20,
                    TotalPages = 2
                }
            };
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public async Task GetCatalogItems_WithNullParameters_HandlesGracefully()
        {
            // Arrange
            var expectedResult = CreateTestCatalogIndexViewModel();
            _mockCatalogViewModelService.Setup(s => s.GetCatalogItems(
                It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _cachedCatalogViewModelService.GetCatalogItems(0, null, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.CatalogItems.Count(), result.CatalogItems.Count());
        }

        [Fact]
        public async Task GetCatalogItems_WithMaxIntValues_HandlesLargeNumbers()
        {
            // Arrange
            var expectedResult = CreateTestCatalogIndexViewModel();
            _mockCatalogViewModelService.Setup(s => s.GetCatalogItems(
                It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _cachedCatalogViewModelService.GetCatalogItems(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetBrands_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _mockCatalogViewModelService.Setup(s => s.GetBrands())
                .ThrowsAsync(new InvalidOperationException("Service error"));

            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.IsAny<object>(),
                It.IsAny<Func<ICacheEntry, Task<IEnumerable<SelectListItem>>>>()))
                .Returns<object, Func<ICacheEntry, Task<IEnumerable<SelectListItem>>>>((key, factory) => factory(Mock.Of<ICacheEntry>()));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _cachedCatalogViewModelService.GetBrands());
        }

        [Fact]
        public async Task GetTypes_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _mockCatalogViewModelService.Setup(s => s.GetTypes())
                .ThrowsAsync(new ArgumentException("Invalid argument"));

            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.IsAny<object>(),
                It.IsAny<Func<ICacheEntry, Task<IEnumerable<SelectListItem>>>>()))
                .Returns<object, Func<ICacheEntry, Task<IEnumerable<SelectListItem>>>>((key, factory) => factory(Mock.Of<ICacheEntry>()));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _cachedCatalogViewModelService.GetTypes());
        }

        [Fact]
        public async Task GetCatalogItems_WithEmptyResult_CachesEmptyResult()
        {
            // Arrange
            var emptyResult = new CatalogIndexViewModel
            {
                CatalogItems = new List<CatalogItemViewModel>(),
                Brands = new List<SelectListItem>(),
                Types = new List<SelectListItem>(),
                PaginationInfo = new PaginationInfoViewModel()
            };

            _mockCatalogViewModelService.Setup(s => s.GetCatalogItems(
                It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(emptyResult);

            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.IsAny<object>(),
                It.IsAny<Func<ICacheEntry, Task<CatalogIndexViewModel>>>()))
                .Returns<object, Func<ICacheEntry, Task<CatalogIndexViewModel>>>((key, factory) => factory(mockCacheEntry.Object));

            // Act
            var result = await _cachedCatalogViewModelService.GetCatalogItems(0, 1, 1, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.CatalogItems);
            mockCacheEntry.VerifySet(e => e.SlidingExpiration = TimeSpan.FromSeconds(30), Times.Once);
        }

        [Fact]
        public async Task GetBrands_WithEmptyResult_CachesEmptyResult()
        {
            // Arrange
            var emptyBrands = new List<SelectListItem>();
            _mockCatalogViewModelService.Setup(s => s.GetBrands())
                .ReturnsAsync(emptyBrands);

            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.IsAny<object>(),
                It.IsAny<Func<ICacheEntry, Task<IEnumerable<SelectListItem>>>>()))
                .Returns<object, Func<ICacheEntry, Task<IEnumerable<SelectListItem>>>>((key, factory) => factory(mockCacheEntry.Object));

            // Act
            var result = await _cachedCatalogViewModelService.GetBrands();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTypes_WithEmptyResult_CachesEmptyResult()
        {
            // Arrange
            var emptyTypes = new List<SelectListItem>();
            _mockCatalogViewModelService.Setup(s => s.GetTypes())
                .ReturnsAsync(emptyTypes);

            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.IsAny<object>(),
                It.IsAny<Func<ICacheEntry, Task<IEnumerable<SelectListItem>>>>()))
                .Returns<object, Func<ICacheEntry, Task<IEnumerable<SelectListItem>>>>((key, factory) => factory(mockCacheEntry.Object));

            // Act
            var result = await _cachedCatalogViewModelService.GetTypes();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCatalogItems_ConcurrentRequests_HandlesProperly()
        {
            // Arrange
            var expectedResult = CreateTestCatalogIndexViewModel();
            var callCount = 0;

            _mockCatalogViewModelService.Setup(s => s.GetCatalogItems(
                It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .Returns(async () =>
                {
                    callCount++;
                    await Task.Delay(100); // Simulate async work
                    return expectedResult;
                });

            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockCache.Setup(c => c.GetOrCreateAsync(
                It.IsAny<object>(),
                It.IsAny<Func<ICacheEntry, Task<CatalogIndexViewModel>>>()))
                .Returns<object, Func<ICacheEntry, Task<CatalogIndexViewModel>>>((key, factory) => factory(mockCacheEntry.Object));

            // Act
            var tasks = Enumerable.Range(0, 3).Select(_ => 
                _cachedCatalogViewModelService.GetCatalogItems(0, 1, 1, 1));
            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.All(results, result => Assert.NotNull(result));
            Assert.Equal(3, callCount); // Each concurrent call should invoke the service
        }

        #endregion
    }
}
