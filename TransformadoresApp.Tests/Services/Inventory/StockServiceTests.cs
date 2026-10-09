using TransformadoresApp.Data;
using TransformadoresApp.Models.Catalogs;
using TransformadoresApp.Models.Inventory;
using TransformadoresApp.Services.Inventory;
using TransformadoresApp.Tests.Infrastructure;

namespace TransformadoresApp.Tests.Services.Inventory
{
    public class StockServiceTests
    {
        [Fact]
        public async Task IncreaseStockAsync_CreatesStock_WhenStockDoesNotExist()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            var item = new Item
            {
                Code = "TEST-001",
                Name = "Item de prueba",
                Category = category,
                IsActive = true
            };

            var warehouse = new Warehouse
            {
                Code = "DEP-TEST",
                Name = "Depósito de prueba",
                IsActive = true
            };

            context.Categories.Add(category);
            context.Items.Add(item);
            context.Warehouses.Add(warehouse);

            await context.SaveChangesAsync();

            var movementService = new StockMovementService(context);
            var stockService = new StockService(context, movementService);

            await stockService.IncreaseStockAsync(item.Id, warehouse.Id, 10);

            await context.SaveChangesAsync();

            var stock = context.ItemStocks.Single();

            Assert.Equal(item.Id, stock.ItemId);
            Assert.Equal(warehouse.Id, stock.WarehouseId);
            Assert.Equal(10, stock.Quantity);
            Assert.Equal(0, stock.ReservedQuantity);
        }

        [Fact]
        public async Task IncreaseStockAsync_IncreasesExistingStock_WhenStockExists()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            var item = new Item
            {
                Code = "TEST-002",
                Name = "Item de prueba 2",
                Category = category,
                IsActive = true
            };

            var warehouse = new Warehouse
            {
                Code = "DEP-TEST-2",
                Name = "Depósito de prueba 2",
                IsActive = true
            };

            context.Categories.Add(category);
            context.Items.Add(item);
            context.Warehouses.Add(warehouse);

            await context.SaveChangesAsync();

            var existingStock = new ItemStock
            {
                ItemId = item.Id,
                WarehouseId = warehouse.Id,
                Quantity = 10,
                ReservedQuantity = 0
            };

            context.ItemStocks.Add(existingStock);

            await context.SaveChangesAsync();

            var movementService = new StockMovementService(context);
            var stockService = new StockService(context, movementService);

            await stockService.IncreaseStockAsync(item.Id, warehouse.Id, 5);

            await context.SaveChangesAsync();

            var stocks = context.ItemStocks.Where(s => s.ItemId == item.Id && s.WarehouseId == warehouse.Id).ToList();

            Assert.Single(stocks);
            Assert.Equal(15, stocks[0].Quantity);
            Assert.Equal(0, stocks[0].ReservedQuantity);
        }

        [Fact]
        public async Task CreateInitialStockAsync_CreatesStockAndMovement()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            var item = new Item
            {
                Code = "TEST-003",
                Name = "Item de prueba 3",
                Category = category,
                IsActive = true
            };

            var warehouse = new Warehouse
            {
                Code = "DEP-TEST-3",
                Name = "Depósito de prueba 3",
                IsActive = true
            };

            context.Categories.Add(category);
            context.Items.Add(item);
            context.Warehouses.Add(warehouse);

            await context.SaveChangesAsync();

            var movementService = new StockMovementService(context);
            var stockService = new StockService(context, movementService);

            await stockService.CreateInitialStockAsync(item.Id, warehouse.Id, 25);

            await context.SaveChangesAsync();

            var stock = context.ItemStocks.Single();

            Assert.Equal(item.Id, stock.ItemId);
            Assert.Equal(warehouse.Id, stock.WarehouseId);
            Assert.Equal(25, stock.Quantity);
            Assert.Equal(0, stock.ReservedQuantity);

            var movement = context.StockMovements.Single();

            Assert.Equal(item.Id, movement.ItemId);
            Assert.Equal(warehouse.Id, movement.WarehouseId);
            Assert.Equal(MovementType.InitialLoad, movement.MovementType);
            Assert.Equal(25, movement.Quantity);
            Assert.Equal("Carga inicial de stock", movement.Notes);
        }

        [Fact]
        public async Task CreateInitialStockAsync_ThrowsException_WhenStockAlreadyExists()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            var item = new Item
            {
                Code = "TEST-004",
                Name = "Item de prueba 4",
                Category = category,
                IsActive = true
            };

            var warehouse = new Warehouse
            {
                Code = "DEP-TEST-4",
                Name = "Depósito de prueba 4",
                IsActive = true
            };

            context.Categories.Add(category);
            context.Items.Add(item);
            context.Warehouses.Add(warehouse);

            await context.SaveChangesAsync();

            var existingStock = new ItemStock
            {
                ItemId = item.Id,
                WarehouseId = warehouse.Id,
                Quantity = 10,
                ReservedQuantity = 0
            };

            context.ItemStocks.Add(existingStock);

            await context.SaveChangesAsync();

            var movementService = new StockMovementService(context);
            var stockService = new StockService(context, movementService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => stockService.CreateInitialStockAsync(
                    item.Id,
                    warehouse.Id,
                    20));

            Assert.Equal("Ya existe stock para ese item en el depósito seleccionado.", exception.Message);

            var stocks = context.ItemStocks.Where(s => s.ItemId == item.Id && s.WarehouseId == warehouse.Id).ToList();

            Assert.Single(stocks);
            Assert.Equal(10, stocks[0].Quantity);

            Assert.Empty(context.StockMovements);
        }

        [Fact]
        public async Task AdjustStockAsync_IncreasesStockAndRegistersMovement()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            var item = new Item
            {
                Code = "TEST-005",
                Name = "Item de prueba 5",
                Category = category,
                IsActive = true
            };

            var warehouse = new Warehouse
            {
                Code = "DEP-TEST-5",
                Name = "Depósito de prueba 5",
                IsActive = true
            };

            context.Categories.Add(category);
            context.Items.Add(item);
            context.Warehouses.Add(warehouse);

            await context.SaveChangesAsync();

            var stock = new ItemStock
            {
                ItemId = item.Id,
                WarehouseId = warehouse.Id,
                Quantity = 10,
                ReservedQuantity = 2
            };

            context.ItemStocks.Add(stock);

            await context.SaveChangesAsync();

            var movementService = new StockMovementService(context);
            var stockService = new StockService(context, movementService);

            await stockService.AdjustStockAsync(
                stock.Id,
                5);

            await context.SaveChangesAsync();

            var updatedStock = context.ItemStocks.Single(s => s.Id == stock.Id);

            Assert.Equal(15, updatedStock.Quantity);
            Assert.Equal(2, updatedStock.ReservedQuantity);

            var movement = context.StockMovements.Single();

            Assert.Equal(item.Id, movement.ItemId);
            Assert.Equal(warehouse.Id, movement.WarehouseId);
            Assert.Equal(MovementType.InventoryAdjustment, movement.MovementType);
            Assert.Equal(5, movement.Quantity);
            Assert.Equal("Ajuste manual de inventario", movement.Notes);
        }

        [Fact]
        public async Task AdjustStockAsync_DecreasesStockAndRegistersMovement()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            var item = new Item
            {
                Code = "TEST-006",
                Name = "Item de prueba 6",
                Category = category,
                IsActive = true
            };

            var warehouse = new Warehouse
            {
                Code = "DEP-TEST-6",
                Name = "Depósito de prueba 6",
                IsActive = true
            };

            context.Categories.Add(category);
            context.Items.Add(item);
            context.Warehouses.Add(warehouse);

            await context.SaveChangesAsync();

            var stock = new ItemStock
            {
                ItemId = item.Id,
                WarehouseId = warehouse.Id,
                Quantity = 10,
                ReservedQuantity = 3
            };

            context.ItemStocks.Add(stock);

            await context.SaveChangesAsync();

            var movementService = new StockMovementService(context);
            var stockService = new StockService(context, movementService);

            await stockService.AdjustStockAsync(stock.Id, -4);

            await context.SaveChangesAsync();

            var updatedStock = context.ItemStocks.Single(s => s.Id == stock.Id);

            Assert.Equal(6, updatedStock.Quantity);
            Assert.Equal(3, updatedStock.ReservedQuantity);

            var movement = context.StockMovements.Single();

            Assert.Equal(item.Id, movement.ItemId);
            Assert.Equal(warehouse.Id, movement.WarehouseId);
            Assert.Equal(MovementType.InventoryAdjustment, movement.MovementType);
            Assert.Equal(-4, movement.Quantity);
            Assert.Equal("Ajuste manual de inventario", movement.Notes);
        }

        [Fact]
        public async Task AdjustStockAsync_ThrowsException_WhenAdjustmentWouldGoBelowReservedStock()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            var item = new Item
            {
                Code = "TEST-007",
                Name = "Item de prueba 7",
                Category = category,
                IsActive = true
            };

            var warehouse = new Warehouse
            {
                Code = "DEP-TEST-7",
                Name = "Depósito de prueba 7",
                IsActive = true
            };

            context.Categories.Add(category);
            context.Items.Add(item);
            context.Warehouses.Add(warehouse);

            await context.SaveChangesAsync();

            var stock = new ItemStock
            {
                ItemId = item.Id,
                WarehouseId = warehouse.Id,
                Quantity = 10,
                ReservedQuantity = 8
            };

            context.ItemStocks.Add(stock);

            await context.SaveChangesAsync();

            var movementService = new StockMovementService(context);
            var stockService = new StockService(context, movementService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => stockService.AdjustStockAsync( stock.Id, -3));

            Assert.Equal("El ajuste no puede dejar el stock por debajo de la cantidad reservada.", exception.Message);

            var updatedStock = context.ItemStocks.Single(s => s.Id == stock.Id);

            Assert.Equal(10, updatedStock.Quantity);
            Assert.Equal(8, updatedStock.ReservedQuantity);

            Assert.Empty(context.StockMovements);
        }

        [Fact]
        public async Task IncreaseStockAsync_ThrowsException_WhenQuantityIsZero()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            var item = new Item
            {
                Code = "TEST-008",
                Name = "Item de prueba",
                Category = category,
                IsActive = true
            };

            var warehouse = new Warehouse
            {
                Code = "DEP-TEST-8",
                Name = "Depósito de prueba",
                IsActive = true
            };

            context.Categories.Add(category);
            context.Items.Add(item);
            context.Warehouses.Add(warehouse);

            await context.SaveChangesAsync();

            var movementService = new StockMovementService(context);
            var stockService = new StockService(context, movementService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => stockService.IncreaseStockAsync(item.Id, warehouse.Id, 0));

            Assert.Equal("La cantidad a ingresar debe ser mayor que cero.", exception.Message);

            Assert.Empty(context.ItemStocks);
        }

        [Fact]
        public async Task IncreaseStockAsync_ThrowsException_WhenItemIdIsInvalid()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var movementService = new StockMovementService(context);
            var stockService = new StockService(context, movementService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => stockService.IncreaseStockAsync(0, 1, 10));

            Assert.Equal("El item indicado no es válido.", exception.Message);
        }

        [Fact]
        public async Task IncreaseStockAsync_ThrowsException_WhenWarehouseIdIsInvalid()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var movementService = new StockMovementService(context);
            var stockService = new StockService(context, movementService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => stockService.IncreaseStockAsync(1, 0, 10));

            Assert.Equal("El depósito indicado no es válido.", exception.Message);
        }

        [Fact]
        public async Task IncreaseStockAsync_ThrowsException_WhenItemDoesNotExist()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var warehouse = new Warehouse
            {
                Code = "DEP-TEST-9",
                Name = "Depósito de prueba",
                IsActive = true
            };

            context.Warehouses.Add(warehouse);

            await context.SaveChangesAsync();

            var movementService = new StockMovementService(context);
            var stockService = new StockService(context, movementService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => stockService.IncreaseStockAsync(999, warehouse.Id, 10));

            Assert.Equal("El item seleccionado no existe o está inactivo.", exception.Message);

            Assert.Empty(context.ItemStocks);
        }

        [Fact]
        public async Task IncreaseStockAsync_ThrowsException_WhenItemIsInactive()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            var item = new Item
            {
                Code = "TEST-010",
                Name = "Item inactivo",
                Category = category,
                IsActive = false
            };

            var warehouse = new Warehouse
            {
                Code = "DEP-TEST-10",
                Name = "Depósito de prueba",
                IsActive = true
            };

            context.Categories.Add(category);
            context.Items.Add(item);
            context.Warehouses.Add(warehouse);

            await context.SaveChangesAsync();

            var movementService = new StockMovementService(context);
            var stockService = new StockService(context, movementService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => stockService.IncreaseStockAsync(item.Id, warehouse.Id, 10));

            Assert.Equal("El item seleccionado no existe o está inactivo.", exception.Message);

            Assert.Empty(context.ItemStocks);
        }

        [Fact]
        public async Task IncreaseStockAsync_ThrowsException_WhenWarehouseDoesNotExist()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            var item = new Item
            {
                Code = "TEST-011",
                Name = "Item de prueba",
                Category = category,
                IsActive = true
            };

            context.Categories.Add(category);
            context.Items.Add(item);

            await context.SaveChangesAsync();

            var movementService = new StockMovementService(context);
            var stockService = new StockService(context, movementService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => stockService.IncreaseStockAsync(item.Id, 999, 10));

            Assert.Equal("El depósito seleccionado no existe o está inactivo.", exception.Message);

            Assert.Empty(context.ItemStocks);
        }

        [Fact]
        public async Task IncreaseStockAsync_ThrowsException_WhenWarehouseIsInactive()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            var item = new Item
            {
                Code = "TEST-012",
                Name = "Item de prueba",
                Category = category,
                IsActive = true
            };

            var warehouse = new Warehouse
            {
                Code = "DEP-TEST-12",
                Name = "Depósito inactivo",
                IsActive = false
            };

            context.Categories.Add(category);
            context.Items.Add(item);
            context.Warehouses.Add(warehouse);

            await context.SaveChangesAsync();

            var movementService = new StockMovementService(context);
            var stockService = new StockService(context, movementService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => stockService.IncreaseStockAsync(item.Id, warehouse.Id, 10));

            Assert.Equal("El depósito seleccionado no existe o está inactivo.", exception.Message);

            Assert.Empty(context.ItemStocks);
        }
    }
}