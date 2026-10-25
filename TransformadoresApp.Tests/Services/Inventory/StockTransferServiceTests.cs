
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;
using TransformadoresApp.Models.Catalogs;
using TransformadoresApp.Models.Inventory;
using TransformadoresApp.Services.Interfaces;
using TransformadoresApp.Services.Inventory;
using TransformadoresApp.Tests.Infrastructure;

namespace TransformadoresApp.Tests.Services.Inventory
{
    public class StockTransferServiceTests
    {
        [Fact]
        public async Task TransferAsync_TransfersStock_WhenDataIsValid()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            var item = new Item
            {
                Code = "TRANSFER-001",
                Name = "Item de transferencia",
                Category = category,
                IsActive = true
            };

            var sourceWarehouse = new Warehouse
            {
                Code = "DEP-ORIGEN",
                Name = "Depósito origen",
                IsActive = true
            };

            var destinationWarehouse = new Warehouse
            {
                Code = "DEP-DESTINO",
                Name = "Depósito destino",
                IsActive = true
            };

            context.Categories.Add(category);
            context.Items.Add(item);
            context.Warehouses.AddRange(sourceWarehouse, destinationWarehouse);

            await context.SaveChangesAsync(cancellationToken);

            var sourceStock = new ItemStock
            {
                ItemId = item.Id,
                WarehouseId = sourceWarehouse.Id,
                Quantity = 20,
                ReservedQuantity = 5
            };

            context.ItemStocks.Add(sourceStock);

            await context.SaveChangesAsync(cancellationToken);

            var movementService = new StockMovementService(context);
            var transferService = new StockTransferService(context, movementService);

            await transferService.TransferAsync(
                item.Id, sourceWarehouse.Id, destinationWarehouse.Id, 10,
                cancellationToken);

            var updatedSourceStock = await context.ItemStocks.SingleAsync(
                s => s.ItemId == item.Id && s.WarehouseId == sourceWarehouse.Id,
                cancellationToken);

            var updatedDestinationStock = await context.ItemStocks.SingleAsync(
                s => s.ItemId == item.Id && s.WarehouseId == destinationWarehouse.Id,
                cancellationToken);

            Assert.Equal(10, updatedSourceStock.Quantity);
            Assert.Equal(5, updatedSourceStock.ReservedQuantity);

            Assert.Equal(10, updatedDestinationStock.Quantity);
            Assert.Equal(0, updatedDestinationStock.ReservedQuantity);
        }

        [Fact]
        public async Task TransferAsync_CreatesDestinationStock_WhenStockDoesNotExist()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            var item = new Item
            {
                Code = "TRANSFER-002",
                Name = "Item de transferencia",
                Category = category,
                IsActive = true
            };

            var sourceWarehouse = new Warehouse
            {
                Code = "DEP-ORIGEN-2",
                Name = "Depósito origen 2",
                IsActive = true
            };

            var destinationWarehouse = new Warehouse
            {
                Code = "DEP-DESTINO-2",
                Name = "Depósito destino 2",
                IsActive = true
            };

            context.Categories.Add(category);
            context.Items.Add(item);
            context.Warehouses.AddRange(sourceWarehouse, destinationWarehouse);

            await context.SaveChangesAsync(cancellationToken);

            context.ItemStocks.Add(new ItemStock
            {
                ItemId = item.Id,
                WarehouseId = sourceWarehouse.Id,
                Quantity = 15,
                ReservedQuantity = 0
            });

            await context.SaveChangesAsync(cancellationToken);

            var movementService = new StockMovementService(context);
            var transferService = new StockTransferService(context, movementService);

            await transferService.TransferAsync(
                item.Id, sourceWarehouse.Id, destinationWarehouse.Id, 7,
                cancellationToken);

            var destinationStock = await context.ItemStocks.SingleAsync(
                s => s.ItemId == item.Id && s.WarehouseId == destinationWarehouse.Id,
                cancellationToken);

            Assert.Equal(7, destinationStock.Quantity);
            Assert.Equal(0, destinationStock.ReservedQuantity);
        }

        [Fact]
        public async Task TransferAsync_IncreasesExistingDestinationStock()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            var item = new Item
            {
                Code = "TRANSFER-003",
                Name = "Item de transferencia",
                Category = category,
                IsActive = true
            };

            var sourceWarehouse = new Warehouse
            {
                Code = "DEP-ORIGEN-3",
                Name = "Depósito origen 3",
                IsActive = true
            };

            var destinationWarehouse = new Warehouse
            {
                Code = "DEP-DESTINO-3",
                Name = "Depósito destino 3",
                IsActive = true
            };

            context.Categories.Add(category);
            context.Items.Add(item);
            context.Warehouses.AddRange(sourceWarehouse, destinationWarehouse);

            await context.SaveChangesAsync(cancellationToken);

            context.ItemStocks.AddRange(
                new ItemStock
                {
                    ItemId = item.Id,
                    WarehouseId = sourceWarehouse.Id,
                    Quantity = 20,
                    ReservedQuantity = 0
                },
                new ItemStock
                {
                    ItemId = item.Id,
                    WarehouseId = destinationWarehouse.Id,
                    Quantity = 5,
                    ReservedQuantity = 2
                });

            await context.SaveChangesAsync(cancellationToken);

            var movementService = new StockMovementService(context);
            var transferService = new StockTransferService(context, movementService);

            await transferService.TransferAsync(
                item.Id, sourceWarehouse.Id, destinationWarehouse.Id, 8,
                cancellationToken);

            var destinationStock = await context.ItemStocks.SingleAsync(
                s => s.ItemId == item.Id && s.WarehouseId == destinationWarehouse.Id,
                cancellationToken);

            Assert.Equal(13, destinationStock.Quantity);
            Assert.Equal(2, destinationStock.ReservedQuantity);
        }

        [Fact]
        public async Task TransferAsync_RegistersTransferMovements()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            var item = new Item
            {
                Code = "TRANSFER-004",
                Name = "Item de transferencia",
                Category = category,
                IsActive = true
            };

            var sourceWarehouse = new Warehouse
            {
                Code = "DEP-ORIGEN-4",
                Name = "Depósito origen 4",
                IsActive = true
            };

            var destinationWarehouse = new Warehouse
            {
                Code = "DEP-DESTINO-4",
                Name = "Depósito destino 4",
                IsActive = true
            };

            context.Categories.Add(category);
            context.Items.Add(item);
            context.Warehouses.AddRange(sourceWarehouse, destinationWarehouse);

            await context.SaveChangesAsync(cancellationToken);

            context.ItemStocks.Add(new ItemStock
            {
                ItemId = item.Id,
                WarehouseId = sourceWarehouse.Id,
                Quantity = 20,
                ReservedQuantity = 0
            });

            await context.SaveChangesAsync(cancellationToken);

            var movementService = new StockMovementService(context);
            var transferService = new StockTransferService(context, movementService);

            await transferService.TransferAsync(
                item.Id, sourceWarehouse.Id, destinationWarehouse.Id, 6,
                cancellationToken);

            var movements = await context.StockMovements
                .OrderBy(m => m.Id)
                .ToListAsync(cancellationToken);

            Assert.Equal(2, movements.Count);

            var transferOut = movements[0];

            Assert.Equal(item.Id, transferOut.ItemId);
            Assert.Equal(sourceWarehouse.Id, transferOut.WarehouseId);
            Assert.Equal(MovementType.TransferOut, transferOut.MovementType);
            Assert.Equal(-6, transferOut.Quantity);
            Assert.Equal($"Transferencia a depósito {destinationWarehouse.Id}", transferOut.Notes);

            var transferIn = movements[1];

            Assert.Equal(item.Id, transferIn.ItemId);
            Assert.Equal(destinationWarehouse.Id, transferIn.WarehouseId);
            Assert.Equal(MovementType.TransferIn, transferIn.MovementType);
            Assert.Equal(6, transferIn.Quantity);
            Assert.Equal($"Transferencia desde depósito {sourceWarehouse.Id}", transferIn.Notes);
        }

        [Fact]
        public async Task TransferAsync_Throws_WhenItemIdIsInvalid()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var movementService = new StockMovementService(context);
            var transferService = new StockTransferService(context, movementService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => transferService.TransferAsync(0, 1, 2, 10, cancellationToken));

            Assert.Equal("Debe seleccionar un item válido.", exception.Message);
        }

        [Fact]
        public async Task TransferAsync_Throws_WhenSourceWarehouseIdIsInvalid()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var movementService = new StockMovementService(context);
            var transferService = new StockTransferService(context, movementService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => transferService.TransferAsync(1, 0, 2, 10, cancellationToken));

            Assert.Equal("Debe seleccionar un depósito de origen válido.", exception.Message);
        }

        [Fact]
        public async Task TransferAsync_Throws_WhenDestinationWarehouseIdIsInvalid()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var movementService = new StockMovementService(context);
            var transferService = new StockTransferService(context, movementService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => transferService.TransferAsync(1, 1, 0, 10, cancellationToken));

            Assert.Equal("Debe seleccionar un depósito de destino válido.", exception.Message);
        }

        [Fact]
        public async Task TransferAsync_Throws_WhenSourceAndDestinationAreTheSame()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var movementService = new StockMovementService(context);
            var transferService = new StockTransferService(context, movementService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => transferService.TransferAsync(1, 1, 1, 10, cancellationToken));

            Assert.Equal("El depósito de origen y destino no pueden ser el mismo.", exception.Message);
        }

        [Fact]
        public async Task TransferAsync_Throws_WhenQuantityIsZero()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var movementService = new StockMovementService(context);
            var transferService = new StockTransferService(context, movementService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => transferService.TransferAsync(1, 2, 3, 0, cancellationToken));

            Assert.Equal("La cantidad a transferir debe ser mayor que cero.", exception.Message);
        }

        [Fact]
        public async Task TransferAsync_Throws_WhenQuantityIsNegative()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var movementService = new StockMovementService(context);
            var transferService = new StockTransferService(context, movementService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => transferService.TransferAsync(1, 2, 3, -5, cancellationToken));

            Assert.Equal("La cantidad a transferir debe ser mayor que cero.", exception.Message);
        }

        [Fact]
        public async Task TransferAsync_Throws_WhenItemDoesNotExist()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var sourceWarehouse = new Warehouse
            {
                Code = "DEP-ORIGEN-5",
                Name = "Depósito origen 5",
                IsActive = true
            };

            var destinationWarehouse = new Warehouse
            {
                Code = "DEP-DESTINO-5",
                Name = "Depósito destino 5",
                IsActive = true
            };

            context.Warehouses.AddRange(sourceWarehouse, destinationWarehouse);

            await context.SaveChangesAsync(cancellationToken);

            var movementService = new StockMovementService(context);
            var transferService = new StockTransferService(context, movementService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => transferService.TransferAsync(
                    999, sourceWarehouse.Id, destinationWarehouse.Id, 10, cancellationToken));

            Assert.Equal("El item seleccionado no existe o está inactivo.", exception.Message);
        }

        [Fact]
        public async Task TransferAsync_Throws_WhenItemIsInactive()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            var item = new Item
            {
                Code = "TRANSFER-006",
                Name = "Item inactivo",
                Category = category,
                IsActive = false
            };

            var sourceWarehouse = new Warehouse
            {
                Code = "DEP-ORIGEN-6",
                Name = "Depósito origen 6",
                IsActive = true
            };

            var destinationWarehouse = new Warehouse
            {
                Code = "DEP-DESTINO-6",
                Name = "Depósito destino 6",
                IsActive = true
            };

            context.Categories.Add(category);
            context.Items.Add(item);
            context.Warehouses.AddRange(sourceWarehouse, destinationWarehouse);

            await context.SaveChangesAsync(cancellationToken);

            var movementService = new StockMovementService(context);
            var transferService = new StockTransferService(context, movementService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => transferService.TransferAsync(
                    item.Id, sourceWarehouse.Id, destinationWarehouse.Id, 10, cancellationToken));

            Assert.Equal("El item seleccionado no existe o está inactivo.", exception.Message);
        }

        [Fact]
        public async Task TransferAsync_Throws_WhenSourceWarehouseDoesNotExist()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            var item = new Item
            {
                Code = "TRANSFER-007",
                Name = "Item de prueba",
                Category = category,
                IsActive = true
            };

            var destinationWarehouse = new Warehouse
            {
                Code = "DEP-DESTINO-7",
                Name = "Depósito destino 7",
                IsActive = true
            };

            context.Categories.Add(category);
            context.Items.Add(item);
            context.Warehouses.Add(destinationWarehouse);

            await context.SaveChangesAsync(cancellationToken);

            var movementService = new StockMovementService(context);
            var transferService = new StockTransferService(context, movementService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => transferService.TransferAsync(
                    item.Id, 999, destinationWarehouse.Id, 10, cancellationToken));

            Assert.Equal("El depósito de origen seleccionado no existe o está inactivo.", exception.Message);
        }

        [Fact]
        public async Task TransferAsync_Throws_WhenSourceWarehouseIsInactive()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            var item = new Item
            {
                Code = "TRANSFER-008",
                Name = "Item de prueba",
                Category = category,
                IsActive = true
            };

            var sourceWarehouse = new Warehouse
            {
                Code = "DEP-ORIGEN-8",
                Name = "Depósito origen inactivo",
                IsActive = false
            };

            var destinationWarehouse = new Warehouse
            {
                Code = "DEP-DESTINO-8",
                Name = "Depósito destino 8",
                IsActive = true
            };

            context.Categories.Add(category);
            context.Items.Add(item);
            context.Warehouses.AddRange(sourceWarehouse, destinationWarehouse);

            await context.SaveChangesAsync(cancellationToken);

            var movementService = new StockMovementService(context);
            var transferService = new StockTransferService(context, movementService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => transferService.TransferAsync(
                    item.Id, sourceWarehouse.Id, destinationWarehouse.Id, 10, cancellationToken));

            Assert.Equal("El depósito de origen seleccionado no existe o está inactivo.", exception.Message);
        }

        [Fact]
        public async Task TransferAsync_Throws_WhenDestinationWarehouseDoesNotExist()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            var item = new Item
            {
                Code = "TRANSFER-009",
                Name = "Item de prueba",
                Category = category,
                IsActive = true
            };

            var sourceWarehouse = new Warehouse
            {
                Code = "DEP-ORIGEN-9",
                Name = "Depósito origen 9",
                IsActive = true
            };

            context.Categories.Add(category);
            context.Items.Add(item);
            context.Warehouses.Add(sourceWarehouse);

            await context.SaveChangesAsync(cancellationToken);

            var movementService = new StockMovementService(context);
            var transferService = new StockTransferService(context, movementService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => transferService.TransferAsync(
                    item.Id, sourceWarehouse.Id, 999, 10, cancellationToken));

            Assert.Equal("El depósito de destino seleccionado no existe o está inactivo.", exception.Message);
        }

        [Fact]
        public async Task TransferAsync_Throws_WhenDestinationWarehouseIsInactive()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            var item = new Item
            {
                Code = "TRANSFER-010",
                Name = "Item de prueba",
                Category = category,
                IsActive = true
            };

            var sourceWarehouse = new Warehouse
            {
                Code = "DEP-ORIGEN-10",
                Name = "Depósito origen 10",
                IsActive = true
            };

            var destinationWarehouse = new Warehouse
            {
                Code = "DEP-DESTINO-10",
                Name = "Depósito destino inactivo",
                IsActive = false
            };

            context.Categories.Add(category);
            context.Items.Add(item);
            context.Warehouses.AddRange(sourceWarehouse, destinationWarehouse);

            await context.SaveChangesAsync(cancellationToken);

            var movementService = new StockMovementService(context);
            var transferService = new StockTransferService(context, movementService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => transferService.TransferAsync(
                    item.Id, sourceWarehouse.Id, destinationWarehouse.Id, 10, cancellationToken));

            Assert.Equal("El depósito de destino seleccionado no existe o está inactivo.", exception.Message);
        }

        [Fact]
        public async Task TransferAsync_Throws_WhenSourceStockDoesNotExist()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            var item = new Item
            {
                Code = "TRANSFER-011",
                Name = "Item sin stock",
                Category = category,
                IsActive = true
            };

            var sourceWarehouse = new Warehouse
            {
                Code = "DEP-ORIGEN-11",
                Name = "Depósito origen 11",
                IsActive = true
            };

            var destinationWarehouse = new Warehouse
            {
                Code = "DEP-DESTINO-11",
                Name = "Depósito destino 11",
                IsActive = true
            };

            context.Categories.Add(category);
            context.Items.Add(item);
            context.Warehouses.AddRange(sourceWarehouse, destinationWarehouse);

            await context.SaveChangesAsync(cancellationToken);

            var movementService = new StockMovementService(context);
            var transferService = new StockTransferService(context, movementService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => transferService.TransferAsync(
                    item.Id, sourceWarehouse.Id, destinationWarehouse.Id, 10, cancellationToken));

            Assert.Equal("No existe stock del item seleccionado en el depósito de origen.", exception.Message);
        }

        [Fact]
        public async Task TransferAsync_Throws_WhenQuantityExceedsAvailableStock()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            var item = new Item
            {
                Code = "TRANSFER-012",
                Name = "Item con stock insuficiente",
                Category = category,
                IsActive = true
            };

            var sourceWarehouse = new Warehouse
            {
                Code = "DEP-ORIGEN-12",
                Name = "Depósito origen 12",
                IsActive = true
            };

            var destinationWarehouse = new Warehouse
            {
                Code = "DEP-DESTINO-12",
                Name = "Depósito destino 12",
                IsActive = true
            };

            context.Categories.Add(category);
            context.Items.Add(item);
            context.Warehouses.AddRange(sourceWarehouse, destinationWarehouse);

            await context.SaveChangesAsync(cancellationToken);

            context.ItemStocks.Add(new ItemStock
            {
                ItemId = item.Id,
                WarehouseId = sourceWarehouse.Id,
                Quantity = 10,
                ReservedQuantity = 4
            });

            await context.SaveChangesAsync(cancellationToken);

            var movementService = new StockMovementService(context);
            var transferService = new StockTransferService(context, movementService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => transferService.TransferAsync(
                    item.Id, sourceWarehouse.Id, destinationWarehouse.Id, 7, cancellationToken));

            Assert.Equal("La cantidad a transferir supera el stock disponible en el depósito de origen.", exception.Message);

            var sourceStock = await context.ItemStocks.SingleAsync(
                s => s.ItemId == item.Id && s.WarehouseId == sourceWarehouse.Id,
                cancellationToken);

            Assert.Equal(10, sourceStock.Quantity);
            Assert.Equal(4, sourceStock.ReservedQuantity);

            Assert.Empty(context.StockMovements);
        }

        [Fact]
        public async Task TransferAsync_TransfersExactlyAvailableStock_WhenStockIsReserved()
        {
            using var factory = new TestDbContextFactory();
            await using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            var item = new Item
            {
                Code = "TRANSFER-013",
                Name = "Item con stock reservado",
                Category = category,
                IsActive = true
            };

            var sourceWarehouse = new Warehouse
            {
                Code = "DEP-ORIGEN-13",
                Name = "Depósito origen 13",
                IsActive = true
            };

            var destinationWarehouse = new Warehouse
            {
                Code = "DEP-DESTINO-13",
                Name = "Depósito destino 13",
                IsActive = true
            };

            context.Categories.Add(category);
            context.Items.Add(item);
            context.Warehouses.AddRange(sourceWarehouse, destinationWarehouse);

            await context.SaveChangesAsync(cancellationToken);

            context.ItemStocks.Add(new ItemStock
            {
                ItemId = item.Id,
                WarehouseId = sourceWarehouse.Id,
                Quantity = 10,
                ReservedQuantity = 4
            });

            await context.SaveChangesAsync(cancellationToken);

            var movementService = new StockMovementService(context);
            var transferService = new StockTransferService(context, movementService);

            await transferService.TransferAsync(
                item.Id, sourceWarehouse.Id, destinationWarehouse.Id, 6,
                cancellationToken);

            await using var verificationContext = factory.CreateContext();

            var updatedSourceStock = await verificationContext.ItemStocks.SingleAsync(
                s => s.ItemId == item.Id && s.WarehouseId == sourceWarehouse.Id,
                cancellationToken);

            var updatedDestinationStock = await verificationContext.ItemStocks.SingleAsync(
                s => s.ItemId == item.Id && s.WarehouseId == destinationWarehouse.Id,
                cancellationToken);

            var movements = await verificationContext.StockMovements
                .OrderBy(m => m.Id)
                .ToListAsync(cancellationToken);

            Assert.Equal(4, updatedSourceStock.Quantity);
            Assert.Equal(4, updatedSourceStock.ReservedQuantity);

            Assert.Equal(6, updatedDestinationStock.Quantity);
            Assert.Equal(0, updatedDestinationStock.ReservedQuantity);

            Assert.Equal(2, movements.Count);
            Assert.Equal(MovementType.TransferOut, movements[0].MovementType);
            Assert.Equal(-6, movements[0].Quantity);
            Assert.Equal(MovementType.TransferIn, movements[1].MovementType);
            Assert.Equal(6, movements[1].Quantity);
        }

        [Fact]
        public async Task TransferAsync_RollsBack_WhenSecondMovementRegistrationFails()
        {
            using var factory = new TestDbContextFactory();
            await using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true
            };

            var item = new Item
            {
                Code = "TRANSFER-014",
                Name = "Item para probar rollback",
                Category = category,
                IsActive = true
            };

            var sourceWarehouse = new Warehouse
            {
                Code = "DEP-ORIGEN-14",
                Name = "Depósito origen 14",
                IsActive = true
            };

            var destinationWarehouse = new Warehouse
            {
                Code = "DEP-DESTINO-14",
                Name = "Depósito destino 14",
                IsActive = true
            };

            context.Categories.Add(category);
            context.Items.Add(item);
            context.Warehouses.AddRange(sourceWarehouse, destinationWarehouse);

            await context.SaveChangesAsync(cancellationToken);

            context.ItemStocks.Add(new ItemStock
            {
                ItemId = item.Id,
                WarehouseId = sourceWarehouse.Id,
                Quantity = 10,
                ReservedQuantity = 2
            });

            await context.SaveChangesAsync(cancellationToken);

            var movementService = new FailingSecondMovementService(context);
            var transferService = new StockTransferService(context, movementService);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => transferService.TransferAsync(
                    item.Id, sourceWarehouse.Id, destinationWarehouse.Id, 5,
                    cancellationToken));

            await using var verificationContext = factory.CreateContext();

            var sourceStock = await verificationContext.ItemStocks.SingleAsync(
                s => s.ItemId == item.Id && s.WarehouseId == sourceWarehouse.Id,
                cancellationToken);

            var destinationStock = await verificationContext.ItemStocks.SingleOrDefaultAsync(
                s => s.ItemId == item.Id && s.WarehouseId == destinationWarehouse.Id,
                cancellationToken);

            var movements = await verificationContext.StockMovements
                .ToListAsync(cancellationToken);

            Assert.Equal(10, sourceStock.Quantity);
            Assert.Equal(2, sourceStock.ReservedQuantity);

            Assert.Null(destinationStock);
            Assert.Empty(movements);
        }
    }

    internal sealed class FailingSecondMovementService : IStockMovementService
    {
        private readonly StockMovementService _innerService;
        private int _registrationCount;

        public FailingSecondMovementService(ApplicationDbContext context)
        {
            _innerService = new StockMovementService(context);
        }

        public async Task RegisterMovementAsync(
            int itemId,
            int warehouseId,
            MovementType movementType,
            decimal quantity,
            string? notes = null,
            CancellationToken cancellationToken = default)
        {
            _registrationCount++;

            if (_registrationCount == 2)
            {
                throw new InvalidOperationException(
                    "Fallo simulado al registrar el segundo movimiento.");
            }

            await _innerService.RegisterMovementAsync(
                itemId,
                warehouseId,
                movementType,
                quantity,
                notes,
                cancellationToken);
        }
    }
}
