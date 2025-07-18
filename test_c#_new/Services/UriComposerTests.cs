using Microsoft.eShopWeb.ApplicationCore;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Xunit;

namespace test_c__new.Services
{
    public class UriComposerTests
    {
        #region ComposePicUri Tests

        [Fact]
        public void ComposePicUri_ValidTemplate_ReplacesBaseUrl()
        {
            // Arrange
            var catalogSettings = new CatalogSettings
            {
                CatalogBaseUrl = "https://mycdn.example.com"
            };
            var uriComposer = new UriComposer(catalogSettings);
            var uriTemplate = "http://catalogbaseurltobereplaced/images/products/1.png";

            // Act
            var result = uriComposer.ComposePicUri(uriTemplate);

            // Assert
            Assert.Equal("https://mycdn.example.com/images/products/1.png", result);
        }

        [Fact]
        public void ComposePicUri_TemplateWithoutPlaceholder_ReturnsOriginal()
        {
            // Arrange
            var catalogSettings = new CatalogSettings
            {
                CatalogBaseUrl = "https://mycdn.example.com"
            };
            var uriComposer = new UriComposer(catalogSettings);
            var uriTemplate = "https://external.example.com/image.jpg";

            // Act
            var result = uriComposer.ComposePicUri(uriTemplate);

            // Assert
            Assert.Equal("https://external.example.com/image.jpg", result);
        }

        [Fact]
        public void ComposePicUri_EmptyTemplate_ReturnsEmpty()
        {
            // Arrange
            var catalogSettings = new CatalogSettings
            {
                CatalogBaseUrl = "https://mycdn.example.com"
            };
            var uriComposer = new UriComposer(catalogSettings);
            var uriTemplate = "";

            // Act
            var result = uriComposer.ComposePicUri(uriTemplate);

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void ComposePicUri_NullTemplate_ReturnsNull()
        {
            // Arrange
            var catalogSettings = new CatalogSettings
            {
                CatalogBaseUrl = "https://mycdn.example.com"
            };
            var uriComposer = new UriComposer(catalogSettings);

            // Act
            var result = uriComposer.ComposePicUri(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ComposePicUri_MultiplePlaceholders_ReplacesAll()
        {
            // Arrange
            var catalogSettings = new CatalogSettings
            {
                CatalogBaseUrl = "https://mycdn.example.com"
            };
            var uriComposer = new UriComposer(catalogSettings);
            var uriTemplate = "http://catalogbaseurltobereplaced/folder/http://catalogbaseurltobereplaced/image.png";

            // Act
            var result = uriComposer.ComposePicUri(uriTemplate);

            // Assert
            Assert.Equal("https://mycdn.example.com/folder/https://mycdn.example.com/image.png", result);
        }

        [Fact]
        public void ComposePicUri_EmptyBaseUrl_ReplacesWithEmpty()
        {
            // Arrange
            var catalogSettings = new CatalogSettings
            {
                CatalogBaseUrl = ""
            };
            var uriComposer = new UriComposer(catalogSettings);
            var uriTemplate = "http://catalogbaseurltobereplaced/images/products/1.png";

            // Act
            var result = uriComposer.ComposePicUri(uriTemplate);

            // Assert
            Assert.Equal("/images/products/1.png", result);
        }

        [Fact]
        public void ComposePicUri_NullBaseUrl_ReplacesWithEmpty()
        {
            // Arrange
            var catalogSettings = new CatalogSettings
            {
                CatalogBaseUrl = null
            };
            var uriComposer = new UriComposer(catalogSettings);
            var uriTemplate = "http://catalogbaseurltobereplaced/images/products/1.png";

            // Act
            var result = uriComposer.ComposePicUri(uriTemplate);

            // Assert
            Assert.Equal("/images/products/1.png", result);
        }

        [Fact]
        public void ComposePicUri_BaseUrlWithTrailingSlash_HandlesCorrectly()
        {
            // Arrange
            var catalogSettings = new CatalogSettings
            {
                CatalogBaseUrl = "https://mycdn.example.com/"
            };
            var uriComposer = new UriComposer(catalogSettings);
            var uriTemplate = "http://catalogbaseurltobereplaced/images/products/1.png";

            // Act
            var result = uriComposer.ComposePicUri(uriTemplate);

            // Assert
            Assert.Equal("https://mycdn.example.com//images/products/1.png", result);
        }

        [Fact]
        public void ComposePicUri_CaseSensitivePlaceholder_OnlyReplacesExactMatch()
        {
            // Arrange
            var catalogSettings = new CatalogSettings
            {
                CatalogBaseUrl = "https://mycdn.example.com"
            };
            var uriComposer = new UriComposer(catalogSettings);
            var uriTemplate = "HTTP://CATALOGBASEURLTOBEREPLACED/images/products/1.png";

            // Act
            var result = uriComposer.ComposePicUri(uriTemplate);

            // Assert
            Assert.Equal("HTTP://CATALOGBASEURLTOBEREPLACED/images/products/1.png", result);
        }

        [Fact]
        public void ComposePicUri_PlaceholderAtEnd_ReplacesCorrectly()
        {
            // Arrange
            var catalogSettings = new CatalogSettings
            {
                CatalogBaseUrl = "https://mycdn.example.com"
            };
            var uriComposer = new UriComposer(catalogSettings);
            var uriTemplate = "http://catalogbaseurltobereplaced";

            // Act
            var result = uriComposer.ComposePicUri(uriTemplate);

            // Assert
            Assert.Equal("https://mycdn.example.com", result);
        }

        [Fact]
        public void ComposePicUri_PlaceholderAtBeginning_ReplacesCorrectly()
        {
            // Arrange
            var catalogSettings = new CatalogSettings
            {
                CatalogBaseUrl = "https://mycdn.example.com"
            };
            var uriComposer = new UriComposer(catalogSettings);
            var uriTemplate = "http://catalogbaseurltobereplaced/path/to/image.jpg";

            // Act
            var result = uriComposer.ComposePicUri(uriTemplate);

            // Assert
            Assert.Equal("https://mycdn.example.com/path/to/image.jpg", result);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_ValidCatalogSettings_DoesNotThrow()
        {
            // Arrange
            var catalogSettings = new CatalogSettings
            {
                CatalogBaseUrl = "https://example.com"
            };

            // Act & Assert
            var exception = Record.Exception(() => new UriComposer(catalogSettings));
            Assert.Null(exception);
        }

        [Fact]
        public void Constructor_NullCatalogSettings_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => new UriComposer(null));
        }

        #endregion
    }
}
