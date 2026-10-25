
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;
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
            var cancellationToken = TestContext.Current.CancellationToken;

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
            await context.SaveChangesAsync(cancellationToken);

            var service = new StockMovementService(context);
            var movementType = Enum.GetValues<MovementType>().First();

            await service.RegisterMovementAsync(
                item.Id,
                warehouse.Id,
                movementType,
                10m,
                "Movimiento de prueba",
                cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            var movement = await context.StockMovements
                .SingleAsync(cancellationToken);

            Assert.Equal(item.Id, movement.ItemId);
            Assert.Equal(warehouse.Id, movement.WarehouseId);
            Assert.Equal(movementType, movement.MovementType);
            Assert.Equal(10m, movement.Quantity);
            Assert.Equal("Movimiento de prueba", movement.Notes);
        }

        [Fact]
        public async Task RegisterMovementAsync_Throws_WhenItemIdIsInvalid()
        {
            var cancellationToken = TestContext.Current.CancellationToken;

            using var factory = new TestDbContextFactory();
            await using var context = factory.CreateContext();

            var service = new StockMovementService(context);
            var movementType = Enum.GetValues<MovementType>().First();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.RegisterMovementAsync(
                    0, 1, movementType, 10m,
                    cancellationToken: cancellationToken));

            Assert.Equal(
                "Debe seleccionar un item válido.",
                exception.Message);
        }

        [Fact]
        public async Task RegisterMovementAsync_Throws_WhenWarehouseIdIsInvalid()
        {
            var cancellationToken = TestContext.Current.CancellationToken;

            using var factory = new TestDbContextFactory();
            await using var context = factory.CreateContext();

            var service = new StockMovementService(context);
            var movementType = Enum.GetValues<MovementType>().First();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.RegisterMovementAsync(
                    1, 0, movementType, 10m,
                    cancellationToken: cancellationToken));

            Assert.Equal(
                "Debe seleccionar un depósito válido.",
                exception.Message);
        }

        [Fact]
        public async Task RegisterMovementAsync_Throws_WhenQuantityIsZero()
        {
            var cancellationToken = TestContext.Current.CancellationToken;

            using var factory = new TestDbContextFactory();
            await using var context = factory.CreateContext();

            var service = new StockMovementService(context);
            var movementType = Enum.GetValues<MovementType>().First();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.RegisterMovementAsync(
                    1, 1, movementType, 0m,
                    cancellationToken: cancellationToken));

            Assert.Equal(
                "La cantidad del movimiento no puede ser cero.",
                exception.Message);
        }

        [Fact]
        public async Task RegisterMovementAsync_Throws_WhenMovementTypeIsInvalid()
        {
            var cancellationToken = TestContext.Current.CancellationToken;

            using var factory = new TestDbContextFactory();
            await using var context = factory.CreateContext();

            var service = new StockMovementService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.RegisterMovementAsync(
                    1, 1, (MovementType)999, 10m,
                    cancellationToken: cancellationToken));

            Assert.Equal(
                "El tipo de movimiento indicado no es válido.",
                exception.Message);
        }

        [Fact]
        public async Task RegisterMovementAsync_Throws_WhenItemDoesNotExist()
        {
            var cancellationToken = TestContext.Current.CancellationToken;

            using var factory = new TestDbContextFactory();
            await using var context = factory.CreateContext();

            var warehouse = new Warehouse
            {
                Code = "DEP-001",
                Name = "Depósito de prueba",
                IsActive = true
            };

            context.Warehouses.Add(warehouse);
            await context.SaveChangesAsync(cancellationToken);

            var service = new StockMovementService(context);
            var movementType = Enum.GetValues<MovementType>().First();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.RegisterMovementAsync(
                    999, warehouse.Id, movementType, 10m,
                    cancellationToken: cancellationToken));

            Assert.Equal(
                "El item seleccionado no existe o está inactivo.",
                exception.Message);
        }

        [Fact]
        public async Task RegisterMovementAsync_Throws_WhenItemIsInactive()
        {
            var cancellationToken = TestContext.Current.CancellationToken;

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
            await context.SaveChangesAsync(cancellationToken);

            var service = new StockMovementService(context);
            var movementType = Enum.GetValues<MovementType>().First();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.RegisterMovementAsync(
                    item.Id, warehouse.Id, movementType, 10m,
                    cancellationToken: cancellationToken));

            Assert.Equal(
                "El item seleccionado no existe o está inactivo.",
                exception.Message);
        }

        [Fact]
        public async Task RegisterMovementAsync_Throws_WhenWarehouseDoesNotExist()
        {
            var cancellationToken = TestContext.Current.CancellationToken;

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
            await context.SaveChangesAsync(cancellationToken);

            var service = new StockMovementService(context);
            var movementType = Enum.GetValues<MovementType>().First();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.RegisterMovementAsync(
                    item.Id, 999, movementType, 10m,
                    cancellationToken: cancellationToken));

            Assert.Equal(
                "El depósito seleccionado no existe o está inactivo.",
                exception.Message);
        }

        [Fact]
        public async Task RegisterMovementAsync_Throws_WhenWarehouseIsInactive()
        {
            var cancellationToken = TestContext.Current.CancellationToken;

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
            await context.SaveChangesAsync(cancellationToken);

            var service = new StockMovementService(context);
            var movementType = Enum.GetValues<MovementType>().First();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.RegisterMovementAsync(
                    item.Id, warehouse.Id, movementType, 10m,
                    cancellationToken: cancellationToken));

            Assert.Equal(
                "El depósito seleccionado no existe o está inactivo.",
                exception.Message);
        }

        [Fact]
        public async Task RegisterMovementAsync_TrimsNotes_WhenNotesContainWhitespace()
        {
            var cancellationToken = TestContext.Current.CancellationToken;

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
            await context.SaveChangesAsync(cancellationToken);

            var service = new StockMovementService(context);
            var movementType = Enum.GetValues<MovementType>().First();

            await service.RegisterMovementAsync(
                item.Id,
                warehouse.Id,
                movementType,
                10m,
                "  Nota de prueba  ",
                cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            var movement = await context.StockMovements
                .SingleAsync(cancellationToken);

            Assert.Equal("Nota de prueba", movement.Notes);
        }

        private static async Task<Category> CreateCategoryAsync(
            DbContext context)
        {
            var cancellationToken = TestContext.Current.CancellationToken;

            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            context.Set<Category>().Add(category);
            await context.SaveChangesAsync(cancellationToken);

            return category;
        }
    }
}