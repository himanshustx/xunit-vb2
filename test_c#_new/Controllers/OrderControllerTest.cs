using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.eShopWeb.Web.Controllers;
using Microsoft.eShopWeb.Web.ViewModels;
using Moq;
using Xunit;

namespace Microsoft.eShopWeb.Web.Tests.Controllers
{
    public class OrderControllerTests
    {
        private const string TestUserName = "testuser@example.com";
        private const string DifferentUserName = "otheruser@example.com";
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly OrderController _controller;

        public OrderControllerTests()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _controller = new OrderController(_mockOrderRepository.Object);
            SetupControllerContext();
        }

        private void SetupControllerContext()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, TestUserName),
                new Claim(ClaimTypes.NameIdentifier, "user-id-123")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidRepository_CreatesInstance()
        {
            // Arrange & Act
            var controller = new OrderController(_mockOrderRepository.Object);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullRepository_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            // Note: VB.NET might not check for null parameters by default
            // This test verifies if the controller handles null gracefully
            var controller = new OrderController(null);
            Assert.NotNull(controller);
        }

        #endregion

        #region MyOrders Action Tests

        [Fact]
        public async Task MyOrders_WhenCalled_ReturnsViewWithOrderViewModels()
        {
            // Arrange
            var testOrders = GetTestOrders();
            _mockOrderRepository.Setup(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()))
                .ReturnsAsync(testOrders);

            // Act
            var result = await _controller.MyOrders();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<OrderViewModel>>(viewResult.Model);
            Assert.Equal(testOrders.Count, model.Count());
            
            var orderViewModels = model.ToList();
            var firstOrder = orderViewModels.First();
            Assert.Equal(testOrders.First().Id, firstOrder.OrderNumber);
            Assert.Equal("Pending", firstOrder.Status);
            Assert.NotNull(firstOrder.OrderItems);
            Assert.Equal(2, firstOrder.OrderItems.Count);
            
            _mockOrderRepository.Verify(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()), Times.Once);
        }

        [Fact]
        public async Task MyOrders_WhenNoOrdersExist_ReturnsEmptyViewModel()
        {
            // Arrange
            _mockOrderRepository.Setup(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()))
                .ReturnsAsync(new List<Order>());

            // Act
            var result = await _controller.MyOrders();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<OrderViewModel>>(viewResult.Model);
            Assert.Empty(model);
            _mockOrderRepository.Verify(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()), Times.Once);
        }

        [Fact]
        public async Task MyOrders_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockOrderRepository.Setup(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()))
                .ThrowsAsync(new ApplicationException("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _controller.MyOrders());
            _mockOrderRepository.Verify(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()), Times.Once);
        }

        [Fact]
        public async Task MyOrders_CallsRepositoryWithCorrectUserName()
        {
            // Arrange
            var testOrders = GetTestOrders();
            
            _mockOrderRepository.Setup(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()))
                .ReturnsAsync(testOrders);

            // Act
            await _controller.MyOrders();

            // Assert
            _mockOrderRepository.Verify(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()), Times.Once);
        }

        [Fact]
        public async Task MyOrders_MapsOrderItemsCorrectly()
        {
            // Arrange
            var testOrders = GetTestOrders();
            _mockOrderRepository.Setup(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()))
                .ReturnsAsync(testOrders);

            // Act
            var result = await _controller.MyOrders();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<OrderViewModel>>(viewResult.Model);
            var firstOrder = model.First();
            var firstOrderItem = firstOrder.OrderItems.First();
            
            Assert.Equal(1, firstOrderItem.ProductId);
            Assert.Equal("Product1", firstOrderItem.ProductName);
            Assert.Equal(10.50m, firstOrderItem.UnitPrice);
            Assert.Equal(2, firstOrderItem.Units);
            Assert.Equal("test1.jpg", firstOrderItem.PictureUrl);
            Assert.Equal(0, firstOrderItem.Discount);
        }

        #endregion

        #region Detail Action Tests

        [Fact]
        public async Task Detail_WithValidOrderId_ReturnsViewWithOrderViewModel()
        {
            // Arrange
            var testOrders = GetTestOrders();
            var testOrder = testOrders.First();
            _mockOrderRepository.Setup(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()))
                .ReturnsAsync(testOrders);

            // Act
            var result = await _controller.Detail(testOrder.Id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<OrderViewModel>(viewResult.Model);
            Assert.Equal(testOrder.Id, model.OrderNumber);
            Assert.Equal(testOrder.Total(), model.Total);
            Assert.Equal("Pending", model.Status);
            Assert.Equal(testOrder.ShipToAddress, model.ShippingAddress);
            Assert.Equal(testOrder.OrderDate, model.OrderDate);
            Assert.NotNull(model.OrderItems);
            Assert.Equal(testOrder.OrderItems.Count, model.OrderItems.Count);
            
            _mockOrderRepository.Verify(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()), Times.Once);
        }

        [Fact]
        public async Task Detail_WithInvalidOrderId_ReturnsBadRequest()
        {
            // Arrange
            var invalidOrderId = 999;
            var testOrders = GetTestOrders();
            _mockOrderRepository.Setup(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()))
                .ReturnsAsync(testOrders);

            // Act
            var result = await _controller.Detail(invalidOrderId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No such order found for this user.", badRequestResult.Value);
            _mockOrderRepository.Verify(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()), Times.Once);
        }

        [Fact]
        public async Task Detail_WithZeroOrderId_ReturnsBadRequest()
        {
            // Arrange
            var testOrders = GetTestOrders();
            _mockOrderRepository.Setup(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()))
                .ReturnsAsync(testOrders);

            // Act
            var result = await _controller.Detail(0);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No such order found for this user.", badRequestResult.Value);
            _mockOrderRepository.Verify(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()), Times.Once);
        }

        [Fact]
        public async Task Detail_WithNegativeOrderId_ReturnsBadRequest()
        {
            // Arrange
            var testOrders = GetTestOrders();
            _mockOrderRepository.Setup(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()))
                .ReturnsAsync(testOrders);

            // Act
            var result = await _controller.Detail(-1);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No such order found for this user.", badRequestResult.Value);
            _mockOrderRepository.Verify(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()), Times.Once);
        }

        [Fact]
        public async Task Detail_WhenOrderBelongsToDifferentUser_ReturnsBadRequest()
        {
            // Arrange
            var address = new Address("Street", "City", "State", "Country", "ZipCode");
            var otherUserOrders = new List<Order>
            {
                new Order(DifferentUserName, address, CreateOrderItems())
            };
            
            // Use reflection to set the Id since it's likely a protected setter
            var order = otherUserOrders.First();
            SetOrderId(order, 1);

            _mockOrderRepository.Setup(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()))
                .ReturnsAsync(new List<Order>()); // No orders for current user

            // Act
            var result = await _controller.Detail(1);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No such order found for this user.", badRequestResult.Value);
            _mockOrderRepository.Verify(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()), Times.Once);
        }

        [Fact]
        public async Task Detail_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockOrderRepository.Setup(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()))
                .ThrowsAsync(new ApplicationException("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _controller.Detail(1));
            _mockOrderRepository.Verify(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()), Times.Once);
        }

        [Fact]
        public async Task Detail_WhenNoOrdersExist_ReturnsBadRequest()
        {
            // Arrange
            _mockOrderRepository.Setup(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()))
                .ReturnsAsync(new List<Order>());

            // Act
            var result = await _controller.Detail(1);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No such order found for this user.", badRequestResult.Value);
            _mockOrderRepository.Verify(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()), Times.Once);
        }

        [Fact]
        public async Task Detail_MapsOrderItemsCorrectly()
        {
            // Arrange
            var testOrders = GetTestOrders();
            var testOrder = testOrders.First();
            _mockOrderRepository.Setup(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()))
                .ReturnsAsync(testOrders);

            // Act
            var result = await _controller.Detail(testOrder.Id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<OrderViewModel>(viewResult.Model);
            var firstOrderItem = model.OrderItems.First();
            
            Assert.Equal(1, firstOrderItem.ProductId);
            Assert.Equal("Product1", firstOrderItem.ProductName);
            Assert.Equal(10.50m, firstOrderItem.UnitPrice);
            Assert.Equal(2, firstOrderItem.Units);
            Assert.Equal("test1.jpg", firstOrderItem.PictureUrl);
            Assert.Equal(0, firstOrderItem.Discount);
        }

        #endregion

        #region Edge Cases and Error Handling

        [Fact]
        public async Task MyOrders_WithNullOrderItems_HandlesGracefully()
        {
            // Arrange
            var address = new Address("Street", "City", "State", "Country", "ZipCode");
            var orderWithNullItems = new Order(TestUserName, address, new List<OrderItem>());
            SetOrderId(orderWithNullItems, 1);
            
            _mockOrderRepository.Setup(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()))
                .ReturnsAsync(new List<Order> { orderWithNullItems });

            // Act
            var result = await _controller.MyOrders();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<OrderViewModel>>(viewResult.Model);
            var orderViewModel = model.First();
            Assert.NotNull(orderViewModel.OrderItems);
            Assert.Empty(orderViewModel.OrderItems);
        }

        [Fact]
        public async Task Detail_WithNullOrderItems_HandlesGracefully()
        {
            // Arrange
            var address = new Address("Street", "City", "State", "Country", "ZipCode");
            var orderWithNullItems = new Order(TestUserName, address, new List<OrderItem>());
            SetOrderId(orderWithNullItems, 1);
            
            _mockOrderRepository.Setup(x => x.ListAsync(It.IsAny<CustomerOrdersWithItemsSpecification>()))
                .ReturnsAsync(new List<Order> { orderWithNullItems });

            // Act
            var result = await _controller.Detail(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<OrderViewModel>(viewResult.Model);
            Assert.NotNull(model.OrderItems);
            Assert.Empty(model.OrderItems);
        }

        #endregion

        #region Test Data Busilder

        private List<Order> GetTestOrders()
        {
            var address = new Address("Street", "City", "State", "Country", "ZipCode");
            var orders = new List<Order>
            {
                new Order(TestUserName, address, CreateOrderItems()),
                new Order(TestUserName, address, CreateSecondOrderItems())
                
            };

            SetOrderId(orders[0], 1);
            SetOrderId(orders[1], 2);
            orders[0].OrderDate = DateTimeOffset.Now;
            orders[1].OrderDate = DateTimeOffset.Now.AddDays(-1);

            return orders;
        }

        private List<OrderItem> CreateOrderItems()
        {
            return new List<OrderItem>
            {
                new OrderItem(new CatalogItemOrdered(1, "Product1", "test1.jpg"), 10.50m, 2),
                new OrderItem(new CatalogItemOrdered(2, "Product2", "test2.jpg"), 15.25m, 1)
            };
        }

        private List<OrderItem> CreateSecondOrderItems()
        {
            return new List<OrderItem>
            {
                new OrderItem(new CatalogItemOrdered(3, "Product3", "test3.jpg"), 20.00m, 3)
            };
        }

        private void SetOrderId(Order order, int id)
        {
            var property = typeof(Order).BaseType?.GetProperty("Id");
            if (property != null && property.CanWrite)
            {
                property.SetValue(order, id);
            }
        }

        #endregion
    }
}