using Microsoft.eShopWeb.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;

namespace test_c__new.Services
{
    public class LoggerAdapterTests
    {
        private readonly Mock<ILoggerFactory> _mockLoggerFactory;
        private readonly Mock<ILogger<TestClass>> _mockLogger;
        private readonly LoggerAdapter<TestClass> _loggerAdapter;

        public LoggerAdapterTests()
        {
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockLogger = new Mock<ILogger<TestClass>>();
            
            _mockLoggerFactory.Setup(f => f.CreateLogger<TestClass>())
                .Returns(_mockLogger.Object);

            _loggerAdapter = new LoggerAdapter<TestClass>(_mockLoggerFactory.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ValidLoggerFactory_CreatesInstance()
        {
            // Act & Assert
            Assert.NotNull(_loggerAdapter);
            _mockLoggerFactory.Verify(f => f.CreateLogger<TestClass>(), Times.Once);
        }

        [Fact]
        public void Constructor_NullLoggerFactory_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new LoggerAdapter<TestClass>(null));
        }

        #endregion

        #region LogInformation Tests

        [Fact]
        public void LogInformation_SimpleMessage_CallsUnderlyingLogger()
        {
            // Arrange
            var message = "Test information message";

            // Act
            _loggerAdapter.LogInformation(message);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformation_MessageWithArguments_CallsUnderlyingLoggerWithArgs()
        {
            // Arrange
            var message = "Test message with {arg1} and {arg2}";
            var arg1 = "value1";
            var arg2 = 42;

            // Act
            _loggerAdapter.LogInformation(message, arg1, arg2);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformation_NullMessage_CallsUnderlyingLogger()
        {
            // Act
            _loggerAdapter.LogInformation(null);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformation_EmptyMessage_CallsUnderlyingLogger()
        {
            // Arrange
            var message = string.Empty;

            // Act
            _loggerAdapter.LogInformation(message);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformation_MessageWithNullArguments_CallsUnderlyingLogger()
        {
            // Arrange
            var message = "Test message with null args";

            // Act
            _loggerAdapter.LogInformation(message, (object[])null);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformation_MessageWithMixedArguments_CallsUnderlyingLogger()
        {
            // Arrange
            var message = "Test with {str}, {num}, {bool}, {date}";
            var args = new object[] { "string", 123, true, DateTime.Now };

            // Act
            _loggerAdapter.LogInformation(message, args);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region LogWarning Tests

        [Fact]
        public void LogWarning_SimpleMessage_CallsUnderlyingLogger()
        {
            // Arrange
            var message = "Test warning message";

            // Act
            _loggerAdapter.LogWarning(message);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_MessageWithArguments_CallsUnderlyingLoggerWithArgs()
        {
            // Arrange
            var message = "Warning: {operation} failed with {errorCode}";
            var operation = "DatabaseUpdate";
            var errorCode = 500;

            // Act
            _loggerAdapter.LogWarning(message, operation, errorCode);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_NullMessage_CallsUnderlyingLogger()
        {
            // Act
            _loggerAdapter.LogWarning(null);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_EmptyMessage_CallsUnderlyingLogger()
        {
            // Arrange
            var message = string.Empty;

            // Act
            _loggerAdapter.LogWarning(message);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_MessageWithComplexObjects_CallsUnderlyingLogger()
        {
            // Arrange
            var message = "Complex warning: {user} attempted {action}";
            var user = new { Id = 123, Name = "TestUser" };
            var action = new { Type = "DeleteAll", Target = "Database" };

            // Act
            _loggerAdapter.LogWarning(message, user, action);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public void LogInformation_VeryLongMessage_CallsUnderlyingLogger()
        {
            // Arrange
            var longMessage = new string('A', 10000); // Very long message

            // Act
            _loggerAdapter.LogInformation(longMessage);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(longMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_VeryLongMessage_CallsUnderlyingLogger()
        {
            // Arrange
            var longMessage = new string('B', 10000); // Very long message

            // Act
            _loggerAdapter.LogWarning(longMessage);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(longMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformation_MessageWithSpecialCharacters_CallsUnderlyingLogger()
        {
            // Arrange
            var message = "Special chars: àáâãäåæçèéêë ñòóôõö ùúûü ýÿ 中文 العربية";

            // Act
            _loggerAdapter.LogInformation(message);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_MessageWithSpecialCharacters_CallsUnderlyingLogger()
        {
            // Arrange
            var message = "Warning with émojis: 🚀🔥💯 and symbols: @#$%^&*()";

            // Act
            _loggerAdapter.LogWarning(message);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformation_ConcurrentCalls_AllCallsHandled()
        {
            // Arrange
            var tasks = new Task[10];

            // Act
            for (int i = 0; i < 10; i++)
            {
                var messageIndex = i;
                tasks[i] = Task.Run(() => _loggerAdapter.LogInformation($"Concurrent message {messageIndex}"));
            }

            Task.WaitAll(tasks);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(10));
        }

        [Fact]
        public void LogWarning_ConcurrentCalls_AllCallsHandled()
        {
            // Arrange
            var tasks = new Task[10];

            // Act
            for (int i = 0; i < 10; i++)
            {
                var messageIndex = i;
                tasks[i] = Task.Run(() => _loggerAdapter.LogWarning($"Concurrent warning {messageIndex}"));
            }

            Task.WaitAll(tasks);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(10));
        }

        [Fact]
        public void LogInformation_WithLargeNumberOfArguments_CallsUnderlyingLogger()
        {
            // Arrange
            var message = "Message with many args: {0} {1} {2} {3} {4} {5} {6} {7} {8} {9}";
            var args = new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            // Act
            _loggerAdapter.LogInformation(message, args);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_WithLargeNumberOfArguments_CallsUnderlyingLogger()
        {
            // Arrange
            var message = "Warning with many args: {0} {1} {2} {3} {4} {5} {6} {7} {8} {9}";
            var args = new object[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j" };

            // Act
            _loggerAdapter.LogWarning(message, args);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void LoggerAdapter_DifferentGenericTypes_CreatesSeparateInstances()
        {
            // Arrange
            var mockLoggerFactory2 = new Mock<ILoggerFactory>();
            var mockLogger2 = new Mock<ILogger<AnotherTestClass>>();
            
            mockLoggerFactory2.Setup(f => f.CreateLogger<AnotherTestClass>())
                .Returns(mockLogger2.Object);

            // Act
            var adapter1 = new LoggerAdapter<TestClass>(_mockLoggerFactory.Object);
            var adapter2 = new LoggerAdapter<AnotherTestClass>(mockLoggerFactory2.Object);

            adapter1.LogInformation("Test message 1");
            adapter2.LogInformation("Test message 2");

            // Assert
            _mockLoggerFactory.Verify(f => f.CreateLogger<TestClass>(), Times.Once);
            mockLoggerFactory2.Verify(f => f.CreateLogger<AnotherTestClass>(), Times.Once);

            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Test message 1")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            mockLogger2.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Test message 2")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region Helper Classes

        private class TestClass
        {
            // Test class for generic logger
        }

        private class AnotherTestClass
        {
            // Another test class for testing different generic types
        }

        #endregion
    }
}
