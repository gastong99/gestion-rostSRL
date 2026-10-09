using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Models.Catalogs;
using TransformadoresApp.Models.Inventory;
using TransformadoresApp.Services.Inventory;
using TransformadoresApp.Tests.Infrastructure;

namespace TransformadoresApp.Tests.Services.Inventory
{
    public class StockMovementServiceTests
    {
        [Fact]
        public async Task RegisterMovementAsync_CreatesMovement_WhenDataIsValid()
        {
            using var factory = new TestDbContextFactory();
            await using var context = factory.CreateContext();

            var category = await CreateCategoryAsync(context);

            var item = new Item
            {
                Code = "ITEM-001",
                Name = "Item de prueba",
                CategoryId = category.Id,
                IsActive = true
            };

            var warehouse = new Warehouse
            {
                Code = "DEP-001",
                Name = "Depósito de prueba",
                IsActive = true
            };

            context.Items.Add(item);
            context.Warehouses.Add(warehouse);

            await context.SaveChangesAsync();

            var service = new StockMovementService(context);

            await service.RegisterMovementAsync(item.Id, warehouse.Id, MovementType.Purchase, 10, "Compra de prueba");

            await context.SaveChangesAsync();

            var movement = await context.StockMovements.SingleAsync();

            Assert.Equal(item.Id, movement.ItemId);
            Assert.Equal(warehouse.Id, movement.WarehouseId);
            Assert.Equal(MovementType.Purchase, movement.MovementType);
            Assert.Equal(10m, movement.Quantity);
            Assert.Equal("Compra de prueba", movement.Notes);
        }

        [Fact]
        public async Task RegisterMovementAsync_Throws_WhenItemIdIsInvalid()
        {
            using var factory = new TestDbContextFactory();
            await using var context = factory.CreateContext();

            var service = new StockMovementService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterMovementAsync(0, 1, MovementType.Purchase, 10));

            Assert.Equal("Debe seleccionar un item válido.", exception.Message);
        }

        [Fact]
        public async Task RegisterMovementAsync_Throws_WhenWarehouseIdIsInvalid()
        {
            using var factory = new TestDbContextFactory();
            await using var context = factory.CreateContext();

            var service = new StockMovementService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterMovementAsync(1, 0, MovementType.Purchase, 10));

            Assert.Equal("Debe seleccionar un depósito válido.", exception.Message);
        }

        [Fact]
        public async Task RegisterMovementAsync_Throws_WhenQuantityIsZero()
        {
            using var factory = new TestDbContextFactory();
            await using var context = factory.CreateContext();

            var category = await CreateCategoryAsync(context);

            var item = new Item
            {
                Code = "ITEM-001",
                Name = "Item de prueba",
                CategoryId = category.Id,
                IsActive = true
            };

            var warehouse = new Warehouse
            {
                Code = "DEP-001",
                Name = "Depósito de prueba",
                IsActive = true
            };

            context.Items.Add(item);
            context.Warehouses.Add(warehouse);

            await context.SaveChangesAsync();

            var service = new StockMovementService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterMovementAsync(item.Id, warehouse.Id, MovementType.Purchase, 0));

            Assert.Equal("La cantidad del movimiento no puede ser cero.", exception.Message);
        }

        [Fact]
        public async Task RegisterMovementAsync_Throws_WhenItemDoesNotExist()
        {
            using var factory = new TestDbContextFactory();
            await using var context = factory.CreateContext();

            var warehouse = new Warehouse
            {
                Code = "DEP-001",
                Name = "Depósito de prueba",
                IsActive = true
            };

            context.Warehouses.Add(warehouse);

            await context.SaveChangesAsync();

            var service = new StockMovementService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterMovementAsync(999, warehouse.Id, MovementType.Purchase, 10));

            Assert.Equal("El item seleccionado no existe o está inactivo.", exception.Message);
        }

        [Fact]
        public async Task RegisterMovementAsync_Throws_WhenItemIsInactive()
        {
            using var factory = new TestDbContextFactory();
            await using var context = factory.CreateContext();

            var category = await CreateCategoryAsync(context);

            var item = new Item
            {
                Code = "ITEM-001",
                Name = "Item inactivo",
                CategoryId = category.Id,
                IsActive = false
            };

            var warehouse = new Warehouse
            {
                Code = "DEP-001",
                Name = "Depósito de prueba",
                IsActive = true
            };

            context.Items.Add(item);
            context.Warehouses.Add(warehouse);

            await context.SaveChangesAsync();

            var service = new StockMovementService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterMovementAsync(item.Id, warehouse.Id, MovementType.Purchase, 10));

            Assert.Equal("El item seleccionado no existe o está inactivo.", exception.Message);
        }

        [Fact]
        public async Task RegisterMovementAsync_Throws_WhenWarehouseDoesNotExist()
        {
            using var factory = new TestDbContextFactory();
            await using var context = factory.CreateContext();

            var category = await CreateCategoryAsync(context);

            var item = new Item
            {
                Code = "ITEM-001",
                Name = "Item de prueba",
                CategoryId = category.Id,
                IsActive = true
            };

            context.Items.Add(item);

            await context.SaveChangesAsync();

            var service = new StockMovementService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterMovementAsync(item.Id, 999, MovementType.Purchase, 10));

            Assert.Equal("El depósito seleccionado no existe o está inactivo.", exception.Message);
        }

        [Fact]
        public async Task RegisterMovementAsync_Throws_WhenWarehouseIsInactive()
        {
            using var factory = new TestDbContextFactory();
            await using var context = factory.CreateContext();

            var category = await CreateCategoryAsync(context);

            var item = new Item
            {
                Code = "ITEM-001",
                Name = "Item de prueba",
                CategoryId = category.Id,
                IsActive = true
            };

            var warehouse = new Warehouse
            {
                Code = "DEP-001",
                Name = "Depósito inactivo",
                IsActive = false
            };

            context.Items.Add(item);
            context.Warehouses.Add(warehouse);

            await context.SaveChangesAsync();

            var service = new StockMovementService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterMovementAsync(item.Id, warehouse.Id, MovementType.Purchase, 10));

            Assert.Equal("El depósito seleccionado no existe o está inactivo.", exception.Message);
        }

        [Fact]
        public async Task RegisterMovementAsync_TrimsNotes()
        {
            using var factory = new TestDbContextFactory();
            await using var context = factory.CreateContext();

            var category = await CreateCategoryAsync(context);

            var item = new Item
            {
                Code = "ITEM-001",
                Name = "Item de prueba",
                CategoryId = category.Id,
                IsActive = true
            };

            var warehouse = new Warehouse
            {
                Code = "DEP-001",
                Name = "Depósito de prueba",
                IsActive = true
            };

            context.Items.Add(item);
            context.Warehouses.Add(warehouse);

            await context.SaveChangesAsync();

            var service = new StockMovementService(context);

            await service.RegisterMovementAsync(item.Id, warehouse.Id, MovementType.Purchase, 10, "  Compra de prueba  ");

            await context.SaveChangesAsync();

            var movement = await context.StockMovements.SingleAsync();

            Assert.Equal("Compra de prueba", movement.Notes);
        }

        // HELPERS
        private static async Task<Category> CreateCategoryAsync(
            DbContext context)
        {
            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            context.Set<Category>().Add(category);

            await context.SaveChangesAsync();

            return category;
        }
    }
}