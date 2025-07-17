
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.eShopWeb.Web.Controllers.Api;
using Microsoft.eShopWeb.Web.Services;
using Microsoft.eShopWeb.Web.ViewModels;
using Moq;
using Xunit;

namespace Microsoft.eShopWeb.Web.Tests.Controllers.Api
{
    public class CatalogControllerTests
    {
        private readonly Mock<ICatalogViewModelService> _mockCatalogViewModelService;
        private readonly CatalogController _controller;

        public CatalogControllerTests()
        {
            _mockCatalogViewModelService = new Mock<ICatalogViewModelService>();
            _controller = new CatalogController(_mockCatalogViewModelService.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidService_CreatesInstance()
        {
            // Arrange & Act
            var controller = new CatalogController(_mockCatalogViewModelService.Object);

            // Assert
            Assert.NotNull(controller);
            Assert.IsAssignableFrom<BaseApiController>(controller);
        }

        [Fact]
        public void Constructor_WithNullService_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new CatalogController(null));
        }

        #endregion

        #region List Action Tests

        [Fact]
        public async Task List_WithValidParameters_ReturnsOkWithCatalogModel()
        {
            // Arrange
            var expectedCatalogModel = CreateTestCatalogIndexViewModel();
            _mockCatalogViewModelService.Setup(x => x.GetCatalogItems(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(expectedCatalogModel);

            // Act
            var result = await _controller.List(brandFilterApplied: 1, typesFilterApplied: 2, page: 1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<CatalogIndexViewModel>(okResult.Value);
            Assert.Equal(expectedCatalogModel, model);
            _mockCatalogViewModelService.Verify(x => x.GetCatalogItems(1, 10, 1, 2), Times.Once);
        }

        [Fact]
        public async Task List_WithNullParameters_UsesDefaultValues()
        {
            // Arrange
            var expectedCatalogModel = CreateTestCatalogIndexViewModel();
            _mockCatalogViewModelService.Setup(x => x.GetCatalogItems(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(expectedCatalogModel);

            // Act
            var result = await _controller.List(brandFilterApplied: null, typesFilterApplied: null, page: null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<CatalogIndexViewModel>(okResult.Value);
            Assert.Equal(expectedCatalogModel, model);
            _mockCatalogViewModelService.Verify(x => x.GetCatalogItems(0, 10, null, null), Times.Once);
        }

        [Fact]
        public async Task List_WithZeroPage_UsesZeroAsPageIndex()
        {
            // Arrange
            var expectedCatalogModel = CreateTestCatalogIndexViewModel();
            _mockCatalogViewModelService.Setup(x => x.GetCatalogItems(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(expectedCatalogModel);

            // Act
            var result = await _controller.List(brandFilterApplied: null, typesFilterApplied: null, page: 0);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockCatalogViewModelService.Verify(x => x.GetCatalogItems(0, 10, null, null), Times.Once);
        }

        [Fact]
        public async Task List_WithNegativePage_UsesNegativePageIndex()
        {
            // Arrange
            var expectedCatalogModel = CreateTestCatalogIndexViewModel();
            _mockCatalogViewModelService.Setup(x => x.GetCatalogItems(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(expectedCatalogModel);

            // Act
            var result = await _controller.List(brandFilterApplied: null, typesFilterApplied: null, page: -1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockCatalogViewModelService.Verify(x => x.GetCatalogItems(-1, 10, null, null), Times.Once);
        }

        [Fact]
        public async Task List_WithLargePageNumber_PassesCorrectPageIndex()
        {
            // Arrange
            var expectedCatalogModel = CreateTestCatalogIndexViewModel();
            _mockCatalogViewModelService.Setup(x => x.GetCatalogItems(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(expectedCatalogModel);

            // Act
            var result = await _controller.List(brandFilterApplied: null, typesFilterApplied: null, page: 1000);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockCatalogViewModelService.Verify(x => x.GetCatalogItems(1000, 10, null, null), Times.Once);
        }

        [Fact]
        public async Task List_WithAllFilters_PassesAllParameters()
        {
            // Arrange
            var expectedCatalogModel = CreateTestCatalogIndexViewModel();
            _mockCatalogViewModelService.Setup(x => x.GetCatalogItems(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(expectedCatalogModel);

            // Act
            var result = await _controller.List(brandFilterApplied: 5, typesFilterApplied: 3, page: 2);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockCatalogViewModelService.Verify(x => x.GetCatalogItems(2, 10, 5, 3), Times.Once);
        }

        [Fact]
        public async Task List_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _mockCatalogViewModelService.Setup(x => x.GetCatalogItems(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ThrowsAsync(new ApplicationException("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _controller.List(null, null, null));
            _mockCatalogViewModelService.Verify(x => x.GetCatalogItems(0, 10, null, null), Times.Once);
        }

        [Fact]
        public async Task List_WithEmptyResults_ReturnsOkWithEmptyModel()
        {
            // Arrange
            var emptyCatalogModel = new CatalogIndexViewModel
            {
                CatalogItems = new List<CatalogItemViewModel>(),
                Brands = new List<SelectListItem>(),
                Types = new List<SelectListItem>(),
                BrandFilterApplied = null,
                TypesFilterApplied = null,
                PaginationInfo = new PaginationInfoViewModel()
            };

            _mockCatalogViewModelService.Setup(x => x.GetCatalogItems(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(emptyCatalogModel);

            // Act
            var result = await _controller.List(null, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<CatalogIndexViewModel>(okResult.Value);
            Assert.Empty(model.CatalogItems);
            Assert.Empty(model.Brands);
            Assert.Empty(model.Types);
        }

        [Fact]
        public async Task List_UsesCorrectItemsPerPage()
        {
            // Arrange
            var expectedCatalogModel = CreateTestCatalogIndexViewModel();
            _mockCatalogViewModelService.Setup(x => x.GetCatalogItems(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(expectedCatalogModel);

            // Act
            var result = await _controller.List(null, null, null);

            // Assert
            _mockCatalogViewModelService.Verify(x => x.GetCatalogItems(0, 10, null, null), Times.Once);
        }

        [Fact]
        public async Task List_WithNegativeFilters_PassesNegativeValues()
        {
            // Arrange
            var expectedCatalogModel = CreateTestCatalogIndexViewModel();
            _mockCatalogViewModelService.Setup(x => x.GetCatalogItems(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(expectedCatalogModel);

            // Act
            var result = await _controller.List(brandFilterApplied: -1, typesFilterApplied: -2, page: 1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockCatalogViewModelService.Verify(x => x.GetCatalogItems(1, 10, -1, -2), Times.Once);
        }

        [Fact]
        public async Task List_WithMaxIntValues_HandlesLargeValues()
        {
            // Arrange
            var expectedCatalogModel = CreateTestCatalogIndexViewModel();
            _mockCatalogViewModelService.Setup(x => x.GetCatalogItems(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(expectedCatalogModel);

            // Act
            var result = await _controller.List(brandFilterApplied: int.MaxValue, typesFilterApplied: int.MaxValue, page: int.MaxValue);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockCatalogViewModelService.Verify(x => x.GetCatalogItems(int.MaxValue, 10, int.MaxValue, int.MaxValue), Times.Once);
        }

        [Fact]
        public async Task List_WithMinIntValues_HandlesSmallValues()
        {
            // Arrange
            var expectedCatalogModel = CreateTestCatalogIndexViewModel();
            _mockCatalogViewModelService.Setup(x => x.GetCatalogItems(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(expectedCatalogModel);

            // Act
            var result = await _controller.List(brandFilterApplied: int.MinValue, typesFilterApplied: int.MinValue, page: int.MinValue);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockCatalogViewModelService.Verify(x => x.GetCatalogItems(int.MinValue, 10, int.MinValue, int.MinValue), Times.Once);
        }

        #endregion

        #region Attribute Tests

        [Fact]
        public void List_HasHttpGetAttribute()
        {
            // Arrange
            var method = typeof(CatalogController).GetMethod("List");

            // Act
            var httpGetAttribute = method.GetCustomAttributes(typeof(HttpGetAttribute), false)
                .FirstOrDefault() as HttpGetAttribute;

            // Assert
            Assert.NotNull(httpGetAttribute);
        }

        [Fact]
        public void CatalogController_InheritsFromBaseApiController()
        {
            // Arrange
            var controllerType = typeof(CatalogController);

            // Act & Assert
            Assert.True(controllerType.IsSubclassOf(typeof(BaseApiController)));
        }

        #endregion

        #region Service Integration Tests

        [Fact]
        public async Task List_CallsServiceOncePerRequest()
        {
            // Arrange
            var expectedCatalogModel = CreateTestCatalogIndexViewModel();
            _mockCatalogViewModelService.Setup(x => x.GetCatalogItems(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(expectedCatalogModel);

            // Act
            await _controller.List(1, 2, 1);
            await _controller.List(1, 2, 1);

            // Assert
            _mockCatalogViewModelService.Verify(x => x.GetCatalogItems(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()), Times.Exactly(2));
        }

        [Fact]
        public async Task List_WithDifferentParameters_CallsServiceWithCorrectParameters()
        {
            // Arrange
            var expectedCatalogModel = CreateTestCatalogIndexViewModel();
            _mockCatalogViewModelService.Setup(x => x.GetCatalogItems(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(expectedCatalogModel);

            // Act
            await _controller.List(1, 2, 0);
            await _controller.List(3, 4, 1);

            // Assert
            _mockCatalogViewModelService.Verify(x => x.GetCatalogItems(0, 10, 1, 2), Times.Once);
            _mockCatalogViewModelService.Verify(x => x.GetCatalogItems(1, 10, 3, 4), Times.Once);
        }

        [Fact]
        public async Task List_ReturnsJsonSerializableModel()
        {
            // Arrange
            var catalogModel = CreateTestCatalogIndexViewModel();
            _mockCatalogViewModelService.Setup(x => x.GetCatalogItems(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(catalogModel);

            // Act
            var result = await _controller.List(null, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<CatalogIndexViewModel>(okResult.Value);
            
            // Verify the model has all expected properties
            Assert.NotNull(model.CatalogItems);
            Assert.NotNull(model.Brands);
            Assert.NotNull(model.Types);
            Assert.NotNull(model.PaginationInfo);
        }

        #endregion

        #region Test Data Builder

        private CatalogIndexViewModel CreateTestCatalogIndexViewModel()
        {
            return new CatalogIndexViewModel
            {
                CatalogItems = new List<CatalogItemViewModel>
                {
                    new CatalogItemViewModel
                    {
                        Id = 1,
                        Name = "Test Product 1",
                        Price = 10.99m,
                        PictureUri = "test1.jpg"
                    },
                    new CatalogItemViewModel
                    {
                        Id = 2,
                        Name = "Test Product 2",
                        Price = 20.99m,
                        PictureUri = "test2.jpg"
                    }
                },
                Brands = new List<SelectListItem>
                {
                    new SelectListItem { Value = "1", Text = "Brand 1" },
                    new SelectListItem { Value = "2", Text = "Brand 2" }
                },
                Types = new List<SelectListItem>
                {
                    new SelectListItem { Value = "1", Text = "Type 1" },
                    new SelectListItem { Value = "2", Text = "Type 2" }
                },
                BrandFilterApplied = 1,
                TypesFilterApplied = 2,
                PaginationInfo = new PaginationInfoViewModel
                {
                    ActualPage = 1,
                    ItemsPerPage = 10,
                    TotalItems = 2,
                    TotalPages = 1
                }
            };
        }

        #endregion
    }
}
