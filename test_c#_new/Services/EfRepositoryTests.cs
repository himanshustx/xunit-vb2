using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace test_c__new.Services
{
    public class EfRepositoryTests : IDisposable
    {
        private readonly CatalogContext _context;
        private readonly EfRepository<CatalogItem> _repository;

        public EfRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<CatalogContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CatalogContext(options);
            _repository = new EfRepository<CatalogItem>(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsEntity()
        {
            // Arrange
            var catalogItem = CreateTestCatalogItem(1, "Test Item", 10.99m, "test-item.jpg");
            _context.CatalogItems.Add(catalogItem);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Item", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ZeroId_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(0);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_NegativeId_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(-1);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region ListAllAsync Tests

        [Fact]
        public async Task ListAllAsync_EmptyDatabase_ReturnsEmptyList()
        {
            // Act
            var result = await _repository.ListAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ListAllAsync_WithData_ReturnsAllEntities()
        {
            // Arrange
            var items = new[]
            {
                CreateTestCatalogItem(1, "Item 1", 10.99m, "item1.jpg"),
                CreateTestCatalogItem(2, "Item 2", 15.99m, "item2.jpg"),
                CreateTestCatalogItem(3, "Item 3", 20.99m, "item3.jpg")
            };

            _context.CatalogItems.AddRange(items);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.ListAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            Assert.Contains(result, x => x.Name == "Item 1");
            Assert.Contains(result, x => x.Name == "Item 2");
            Assert.Contains(result, x => x.Name == "Item 3");
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_ValidEntity_AddsToDatabase()
        {
            // Arrange
            var catalogItem = CreateTestCatalogItem(0, "New Item", 25.99m, "new-item.jpg");

            // Act
            await _repository.AddAsync(catalogItem);

            // Assert
            var savedItem = await _context.CatalogItems.FirstOrDefaultAsync(x => x.Name == "New Item");
            Assert.NotNull(savedItem);
            Assert.Equal("New Item", savedItem.Name);
            Assert.Equal(25.99m, savedItem.Price);
        }

        [Fact]
        public async Task AddAsync_NullEntity_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.AddAsync(null));
        }

        [Fact]
        public async Task AddAsync_EntityWithDuplicateId_ThrowsException()
        {
            // Arrange
            var existingItem = CreateTestCatalogItem(1, "Existing Item", 10.99m, "existing.jpg");
            _context.CatalogItems.Add(existingItem);
            await _context.SaveChangesAsync();

            var duplicateItem = CreateTestCatalogItem(1, "Duplicate Item", 15.99m, "duplicate.jpg");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.AddAsync(duplicateItem));
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ExistingEntity_UpdatesSuccessfully()
        {
            // Arrange
            var catalogItem = CreateTestCatalogItem(1, "Original Item", 10.99m, "original.jpg");
            _context.CatalogItems.Add(catalogItem);
            await _context.SaveChangesAsync();

            // Modify the entity
            catalogItem.UpdateDetails("Updated Item", 15.99m);

            // Act
            await _repository.UpdateAsync(catalogItem);

            // Assert
            var updatedItem = await _context.CatalogItems.FindAsync(1);
            Assert.NotNull(updatedItem);
            Assert.Equal("Updated Item", updatedItem.Name);
            Assert.Equal(15.99m, updatedItem.Price);
        }

        [Fact]
        public async Task UpdateAsync_NullEntity_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.UpdateAsync(null));
        }

        [Fact]
        public async Task UpdateAsync_NonExistingEntity_ThrowsException()
        {
            // Arrange
            var nonExistingItem = CreateTestCatalogItem(999, "Non-existing Item", 10.99m, "non-existing.jpg");

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _repository.UpdateAsync(nonExistingItem));
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ExistingEntity_DeletesSuccessfully()
        {
            // Arrange
            var catalogItem = CreateTestCatalogItem(1, "Item to Delete", 10.99m, "delete-me.jpg");
            _context.CatalogItems.Add(catalogItem);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(catalogItem);

            // Assert
            var deletedItem = await _context.CatalogItems.FindAsync(1);
            Assert.Null(deletedItem);
        }

        [Fact]
        public async Task DeleteAsync_NullEntity_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.DeleteAsync(null));
        }

        #endregion

        #region ListAsync with Specification Tests

        [Fact]
        public async Task ListAsync_WithSpecification_ReturnsFilteredResults()
        {
            // Arrange
            var items = new[]
            {
                CreateTestCatalogItem(1, "Expensive Item", 100.00m, "expensive.jpg"),
                CreateTestCatalogItem(2, "Cheap Item", 5.00m, "cheap.jpg"),
                CreateTestCatalogItem(3, "Medium Item", 50.00m, "medium.jpg")
            };

            _context.CatalogItems.AddRange(items);
            await _context.SaveChangesAsync();

            var mockSpec = new Mock<ISpecification<CatalogItem>>();
            mockSpec.Setup(s => s.Criteria).Returns(x => x.Price > 10.00m);
            mockSpec.Setup(s => s.Includes).Returns(new List<System.Linq.Expressions.Expression<Func<CatalogItem, object>>>());
            mockSpec.Setup(s => s.IncludeStrings).Returns(new List<string>());

            // Act
            var result = await _repository.ListAsync(mockSpec.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, x => x.Name == "Expensive Item");
            Assert.Contains(result, x => x.Name == "Medium Item");
            Assert.DoesNotContain(result, x => x.Name == "Cheap Item");
        }

        [Fact]
        public async Task ListAsync_WithNullSpecification_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.ListAsync(null));
        }

        #endregion

        #region CountAsync Tests

        [Fact]
        public async Task CountAsync_EmptyDatabase_ReturnsZero()
        {
            // Act
            var result = await _repository.CountAsync();

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task CountAsync_WithData_ReturnsCorrectCount()
        {
            // Arrange
            var items = new[]
            {
                CreateTestCatalogItem(1, "Item 1", 10.99m, "item1.jpg"),
                CreateTestCatalogItem(2, "Item 2", 15.99m, "item2.jpg"),
                CreateTestCatalogItem(3, "Item 3", 20.99m, "item3.jpg")
            };

            _context.CatalogItems.AddRange(items);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.CountAsync();

            // Assert
            Assert.Equal(3, result);
        }

        [Fact]
        public async Task CountAsync_WithSpecification_ReturnsFilteredCount()
        {
            // Arrange
            var items = new[]
            {
                CreateTestCatalogItem(1, "Expensive Item", 100.00m, "expensive.jpg"),
                CreateTestCatalogItem(2, "Cheap Item", 5.00m, "cheap.jpg"),
                CreateTestCatalogItem(3, "Medium Item", 50.00m, "medium.jpg")
            };

            _context.CatalogItems.AddRange(items);
            await _context.SaveChangesAsync();

            var mockSpec = new Mock<ISpecification<CatalogItem>>();
            mockSpec.Setup(s => s.Criteria).Returns(x => x.Price > 10.00m);

            // Act
            var result = await _repository.CountAsync(mockSpec.Object);

            // Assert
            Assert.Equal(2, result);
        }

        #endregion

        #region ListPaginatedAsync Tests

        [Fact]
        public async Task ListPaginatedAsync_ValidPageAndSize_ReturnsCorrectPage()
        {
            // Arrange
            var items = Enumerable.Range(1, 10)
                .Select(i => CreateTestCatalogItem(i, $"Item {i}", i * 10.00m, $"item{i}.jpg"))
                .ToArray();

            _context.CatalogItems.AddRange(items);
            await _context.SaveChangesAsync();

            var mockSpec = new Mock<ISpecification<CatalogItem>>();
            mockSpec.Setup(s => s.Criteria).Returns(x => true);
            mockSpec.Setup(s => s.Includes).Returns(new List<System.Linq.Expressions.Expression<Func<CatalogItem, object>>>());
            mockSpec.Setup(s => s.IncludeStrings).Returns(new List<string>());

            // Act
            var result = await _repository.ListPaginatedAsync(mockSpec.Object, 1, 3);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            Assert.Contains(result, x => x.Name == "Item 4");
            Assert.Contains(result, x => x.Name == "Item 5");
            Assert.Contains(result, x => x.Name == "Item 6");
        }

        [Fact]
        public async Task ListPaginatedAsync_FirstPage_ReturnsFirstItems()
        {
            // Arrange
            var items = Enumerable.Range(1, 5)
                .Select(i => CreateTestCatalogItem(i, $"Item {i}", i * 10.00m, $"item{i}.jpg"))
                .ToArray();

            _context.CatalogItems.AddRange(items);
            await _context.SaveChangesAsync();

            var mockSpec = new Mock<ISpecification<CatalogItem>>();
            mockSpec.Setup(s => s.Criteria).Returns(x => true);
            mockSpec.Setup(s => s.Includes).Returns(new List<System.Linq.Expressions.Expression<Func<CatalogItem, object>>>());
            mockSpec.Setup(s => s.IncludeStrings).Returns(new List<string>());

            // Act
            var result = await _repository.ListPaginatedAsync(mockSpec.Object, 0, 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, x => x.Name == "Item 1");
            Assert.Contains(result, x => x.Name == "Item 2");
        }

        [Fact]
        public async Task ListPaginatedAsync_EmptyDatabase_ReturnsEmptyList()
        {
            // Arrange
            var mockSpec = new Mock<ISpecification<CatalogItem>>();
            mockSpec.Setup(s => s.Criteria).Returns(x => true);
            mockSpec.Setup(s => s.Includes).Returns(new List<System.Linq.Expressions.Expression<Func<CatalogItem, object>>>());
            mockSpec.Setup(s => s.IncludeStrings).Returns(new List<string>());

            // Act
            var result = await _repository.ListPaginatedAsync(mockSpec.Object, 0, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public async Task AddAsync_LargeNumberOfEntities_AddsAllSuccessfully()
        {
            // Arrange
            var items = Enumerable.Range(1, 100)
                .Select(i => CreateTestCatalogItem(0, $"Bulk Item {i}", i * 1.50m, $"bulk{i}.jpg"))
                .ToList();

            // Act
            foreach (var item in items)
            {
                await _repository.AddAsync(item);
            }

            // Assert
            var count = await _repository.CountAsync();
            Assert.Equal(100, count);
        }

        [Fact]
        public async Task UpdateAsync_ConcurrentUpdates_HandlesCorrectly()
        {
            // Arrange
            var catalogItem = CreateTestCatalogItem(1, "Concurrent Item", 10.99m, "concurrent.jpg");
            _context.CatalogItems.Add(catalogItem);
            await _context.SaveChangesAsync();

            // Get the same entity in two different contexts
            var item1 = await _repository.GetByIdAsync(1);
            var item2 = await _repository.GetByIdAsync(1);

            // Modify both
            item1.UpdateDetails("Updated by 1", 15.99m);
            item2.UpdateDetails("Updated by 2", 20.99m);

            // Act
            await _repository.UpdateAsync(item1);

            // Assert - Second update should throw concurrency exception
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _repository.UpdateAsync(item2));
        }

        [Fact]
        public async Task ListPaginatedAsync_WithZeroPageSize_ReturnsEmptyList()
        {
            // Arrange
            var items = new[]
            {
                CreateTestCatalogItem(1, "Item 1", 10.99m, "item1.jpg"),
                CreateTestCatalogItem(2, "Item 2", 15.99m, "item2.jpg")
            };

            _context.CatalogItems.AddRange(items);
            await _context.SaveChangesAsync();

            var mockSpec = new Mock<ISpecification<CatalogItem>>();
            mockSpec.Setup(s => s.Criteria).Returns(x => true);
            mockSpec.Setup(s => s.Includes).Returns(new List<System.Linq.Expressions.Expression<Func<CatalogItem, object>>>());
            mockSpec.Setup(s => s.IncludeStrings).Returns(new List<string>());

            // Act
            var result = await _repository.ListPaginatedAsync(mockSpec.Object, 0, 0);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ListPaginatedAsync_WithNegativePageIndex_HandlesCorrectly()
        {
            // Arrange
            var items = new[]
            {
                CreateTestCatalogItem(1, "Item 1", 10.99m, "item1.jpg"),
                CreateTestCatalogItem(2, "Item 2", 15.99m, "item2.jpg")
            };

            _context.CatalogItems.AddRange(items);
            await _context.SaveChangesAsync();

            var mockSpec = new Mock<ISpecification<CatalogItem>>();
            mockSpec.Setup(s => s.Criteria).Returns(x => true);
            mockSpec.Setup(s => s.Includes).Returns(new List<System.Linq.Expressions.Expression<Func<CatalogItem, object>>>());
            mockSpec.Setup(s => s.IncludeStrings).Returns(new List<string>());

            // Act
            var result = await _repository.ListPaginatedAsync(mockSpec.Object, -1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region Helper Methods

        private CatalogItem CreateTestCatalogItem(int id, string name, decimal price, string pictureUri)
        {
            var catalogItem = new CatalogItem(1, 1, name, name, price, pictureUri);
            if (id > 0)
            {
                typeof(CatalogItem).BaseType.GetProperty("Id").SetValue(catalogItem, id);
            }
            return catalogItem;
        }

        #endregion
    }
}
