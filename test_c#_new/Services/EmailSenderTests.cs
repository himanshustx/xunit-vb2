using Microsoft.eShopWeb.Infrastructure.Services;
using System.Threading.Tasks;
using Xunit;

namespace test_c__new.Services
{
    public class EmailSenderTests
    {
        private readonly EmailSender _emailSender;

        public EmailSenderTests()
        {
            _emailSender = new EmailSender();
        }

        #region SendEmailAsync Tests

        [Fact]
        public async Task SendEmailAsync_ValidParameters_CompletesSuccessfully()
        {
            // Arrange
            var email = "test@example.com";
            var subject = "Test Subject";
            var message = "Test message content";

            // Act & Assert
            // Should not throw any exception
            await _emailSender.SendEmailAsync(email, subject, message);
        }

        [Fact]
        public async Task SendEmailAsync_EmptyEmail_CompletesSuccessfully()
        {
            // Arrange
            var email = "";
            var subject = "Test Subject";
            var message = "Test message content";

            // Act & Assert
            // Should not throw any exception
            await _emailSender.SendEmailAsync(email, subject, message);
        }

        [Fact]
        public async Task SendEmailAsync_NullEmail_CompletesSuccessfully()
        {
            // Arrange
            string email = null;
            var subject = "Test Subject";
            var message = "Test message content";

            // Act & Assert
            // Should not throw any exception
            await _emailSender.SendEmailAsync(email, subject, message);
        }

        [Fact]
        public async Task SendEmailAsync_EmptySubject_CompletesSuccessfully()
        {
            // Arrange
            var email = "test@example.com";
            var subject = "";
            var message = "Test message content";

            // Act & Assert
            // Should not throw any exception
            await _emailSender.SendEmailAsync(email, subject, message);
        }

        [Fact]
        public async Task SendEmailAsync_NullSubject_CompletesSuccessfully()
        {
            // Arrange
            var email = "test@example.com";
            string subject = null;
            var message = "Test message content";

            // Act & Assert
            // Should not throw any exception
            await _emailSender.SendEmailAsync(email, subject, message);
        }

        [Fact]
        public async Task SendEmailAsync_EmptyMessage_CompletesSuccessfully()
        {
            // Arrange
            var email = "test@example.com";
            var subject = "Test Subject";
            var message = "";

            // Act & Assert
            // Should not throw any exception
            await _emailSender.SendEmailAsync(email, subject, message);
        }

        [Fact]
        public async Task SendEmailAsync_NullMessage_CompletesSuccessfully()
        {
            // Arrange
            var email = "test@example.com";
            var subject = "Test Subject";
            string message = null;

            // Act & Assert
            // Should not throw any exception
            await _emailSender.SendEmailAsync(email, subject, message);
        }

        [Fact]
        public async Task SendEmailAsync_AllNullParameters_CompletesSuccessfully()
        {
            // Arrange
            string email = null;
            string subject = null;
            string message = null;

            // Act & Assert
            // Should not throw any exception
            await _emailSender.SendEmailAsync(email, subject, message);
        }

        [Fact]
        public async Task SendEmailAsync_AllEmptyParameters_CompletesSuccessfully()
        {
            // Arrange
            var email = "";
            var subject = "";
            var message = "";

            // Act & Assert
            // Should not throw any exception
            await _emailSender.SendEmailAsync(email, subject, message);
        }

        [Fact]
        public async Task SendEmailAsync_LongEmail_CompletesSuccessfully()
        {
            // Arrange
            var email = "very.long.email.address.that.might.exceed.normal.length@example-domain-with-very-long-name.com";
            var subject = "Test Subject";
            var message = "Test message content";

            // Act & Assert
            // Should not throw any exception
            await _emailSender.SendEmailAsync(email, subject, message);
        }

        [Fact]
        public async Task SendEmailAsync_LongSubject_CompletesSuccessfully()
        {
            // Arrange
            var email = "test@example.com";
            var subject = new string('A', 1000); // Very long subject
            var message = "Test message content";

            // Act & Assert
            // Should not throw any exception
            await _emailSender.SendEmailAsync(email, subject, message);
        }

        [Fact]
        public async Task SendEmailAsync_LongMessage_CompletesSuccessfully()
        {
            // Arrange
            var email = "test@example.com";
            var subject = "Test Subject";
            var message = new string('B', 10000); // Very long message

            // Act & Assert
            // Should not throw any exception
            await _emailSender.SendEmailAsync(email, subject, message);
        }

        [Fact]
        public async Task SendEmailAsync_SpecialCharactersInEmail_CompletesSuccessfully()
        {
            // Arrange
            var email = "test+special@example-domain.co.uk";
            var subject = "Test Subject";
            var message = "Test message content";

            // Act & Assert
            // Should not throw any exception
            await _emailSender.SendEmailAsync(email, subject, message);
        }

        [Fact]
        public async Task SendEmailAsync_SpecialCharactersInSubject_CompletesSuccessfully()
        {
            // Arrange
            var email = "test@example.com";
            var subject = "Test Subject with special chars: !@#$%^&*()";
            var message = "Test message content";

            // Act & Assert
            // Should not throw any exception
            await _emailSender.SendEmailAsync(email, subject, message);
        }

        [Fact]
        public async Task SendEmailAsync_SpecialCharactersInMessage_CompletesSuccessfully()
        {
            // Arrange
            var email = "test@example.com";
            var subject = "Test Subject";
            var message = "Test message with special chars: !@#$%^&*() and unicode: ñáéíóú";

            // Act & Assert
            // Should not throw any exception
            await _emailSender.SendEmailAsync(email, subject, message);
        }

        [Fact]
        public async Task SendEmailAsync_ReturnsCompletedTask()
        {
            // Arrange
            var email = "test@example.com";
            var subject = "Test Subject";
            var message = "Test message content";

            // Act
            var result = _emailSender.SendEmailAsync(email, subject, message);

            // Assert
            Assert.True(result.IsCompleted);
            await result; // Ensure it completes without exception
        }

        #endregion

        #region Multiple Calls Tests

        [Fact]
        public async Task SendEmailAsync_MultipleCalls_AllCompleteSuccessfully()
        {
            // Arrange
            var email1 = "test1@example.com";
            var email2 = "test2@example.com";
            var email3 = "test3@example.com";
            var subject = "Test Subject";
            var message = "Test message content";

            // Act & Assert
            await _emailSender.SendEmailAsync(email1, subject, message);
            await _emailSender.SendEmailAsync(email2, subject, message);
            await _emailSender.SendEmailAsync(email3, subject, message);
        }

        [Fact]
        public async Task SendEmailAsync_ConcurrentCalls_AllCompleteSuccessfully()
        {
            // Arrange
            var tasks = new Task[10];
            var subject = "Test Subject";
            var message = "Test message content";

            // Act
            for (int i = 0; i < 10; i++)
            {
                var email = $"test{i}@example.com";
                tasks[i] = _emailSender.SendEmailAsync(email, subject, message);
            }

            // Assert
            await Task.WhenAll(tasks);
        }

        #endregion
    }
}
