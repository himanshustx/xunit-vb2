using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopWeb.Web.Controllers.Api;
using System.Linq;
using Xunit;

namespace Microsoft.eShopWeb.Web.Tests.Controllers.Api
{
    public class BaseApiControllerTests
    {
        private readonly BaseApiController _controller;

        public BaseApiControllerTests()
        {
            _controller = new BaseApiController();
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_CreatesInstance()
        {
            // Arrange & Act
            var controller = new BaseApiController();

            // Assert
            Assert.NotNull(controller);
            Assert.IsAssignableFrom<Controller>(controller);
        }

        #endregion

        #region Attribute Tests

        [Fact]
        public void BaseApiController_HasCorrectRouteAttribute()
        {
            // Arrange
            var controllerType = typeof(BaseApiController);

            // Act
            var routeAttribute = controllerType.GetCustomAttributes(typeof(RouteAttribute), false)
                .FirstOrDefault() as RouteAttribute;

            // Assert
            Assert.NotNull(routeAttribute);
            Assert.Equal("api/[controller]/[action]", routeAttribute.Template);
        }

        [Fact]
        public void BaseApiController_HasApiControllerAttribute()
        {
            // Arrange
            var controllerType = typeof(BaseApiController);

            // Act
            var apiControllerAttribute = controllerType.GetCustomAttributes(typeof(ApiControllerAttribute), false)
                .FirstOrDefault() as ApiControllerAttribute;

            // Assert
            Assert.NotNull(apiControllerAttribute);
        }

        [Fact]
        public void BaseApiController_InheritsFromController()
        {
            // Arrange
            var controllerType = typeof(BaseApiController);

            // Act & Assert
            Assert.True(controllerType.IsSubclassOf(typeof(Controller)));
        }

        #endregion

        #region Controller Properties Tests

        [Fact]
        public void BaseApiController_HasControllerBaseProperties()
        {
            // Arrange & Act
            var controller = new BaseApiController();

            // Assert
            Assert.NotNull(controller.GetType().GetProperty("HttpContext"));
            Assert.NotNull(controller.GetType().GetProperty("Request"));
            Assert.NotNull(controller.GetType().GetProperty("Response"));
            Assert.NotNull(controller.GetType().GetProperty("User"));
        }

        [Fact]
        public void BaseApiController_CanBeInstantiatedAsController()
        {
            // Arrange & Act
            Controller controller = new BaseApiController();

            // Assert
            Assert.NotNull(controller);
            Assert.IsType<BaseApiController>(controller);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void BaseApiController_CanBeUsedAsBaseClass()
        {
            // Arrange
            var derivedController = new TestDerivedController();

            // Act & Assert
            Assert.NotNull(derivedController);
            Assert.IsAssignableFrom<BaseApiController>(derivedController);
            Assert.IsAssignableFrom<Controller>(derivedController);
        }

        [Fact]
        public void BaseApiController_InheritsControllerBehavior()
        {
            // Arrange
            var controller = new BaseApiController();

            // Act
            var okResult = controller.Ok();
            var badRequestResult = controller.BadRequest();
            var notFoundResult = controller.NotFound();

            // Assert
            Assert.IsType<OkResult>(okResult);
            Assert.IsType<BadRequestResult>(badRequestResult);
            Assert.IsType<NotFoundResult>(notFoundResult);
        }

        #endregion

        #region Helper Classes

        private class TestDerivedController : BaseApiController
        {
            public IActionResult TestAction()
            {
                return Ok("Test");
            }
        }

        #endregion
    }
}
