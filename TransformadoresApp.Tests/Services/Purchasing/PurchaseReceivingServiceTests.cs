using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;
using TransformadoresApp.Models.Catalogs;
using TransformadoresApp.Models.Inventory;
using TransformadoresApp.Models.Purchasing;
using TransformadoresApp.Services.Interfaces;
using TransformadoresApp.Services.Inventory;
using TransformadoresApp.Services.Purchasing;
using TransformadoresApp.Tests.Infrastructure;
using Xunit;

namespace TransformadoresApp.Tests.Services.Purchasing
{
    public class PurchaseReceivingServiceTests
    {
        // RECEIVE - SUCCESS
        [Fact]
        public async Task ReceiveAsync_ReceivesPartialQuantity_UpdatesStockMovementAndOrderStatus()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var order = await CreatePurchaseOrderAsync(context);
            var item = await CreateItemAsync(context);
            var warehouse = await CreateWarehouseAsync(context);
            var orderItem = await CreatePurchaseOrderItemAsync(context, order.Id, item.Id, quantity: 10);

            var service = CreateService(context);

            var returnedOrderId = await service.ReceiveAsync(orderItem.Id, warehouse.Id, 4);

            Assert.Equal(order.Id, returnedOrderId);

            var savedOrderItem = await context.PurchaseOrderItems.SingleAsync(i => i.Id == orderItem.Id);
            var savedOrder = await context.PurchaseOrders.SingleAsync(o => o.Id == order.Id);
            var stock = await context.ItemStocks.SingleAsync(s => s.ItemId == item.Id && s.WarehouseId == warehouse.Id);
            var movement = await context.StockMovements.SingleAsync();

            Assert.Equal(4, savedOrderItem.ReceivedQuantity);
            Assert.Equal(PurchaseOrderStatus.PartiallyReceived, savedOrder.Status);
            Assert.Equal(4, stock.Quantity);
            Assert.Equal(0, stock.ReservedQuantity);
            Assert.Equal(MovementType.Purchase, movement.MovementType);
            Assert.Equal(4, movement.Quantity);
            Assert.Equal(item.Id, movement.ItemId);
            Assert.Equal(warehouse.Id, movement.WarehouseId);
            Assert.Equal($"Recepción OC {order.Number}", movement.Notes);
        }

        [Fact]
        public async Task ReceiveAsync_ReceivesFullQuantity_UpdatesOrderStatusToCompleted()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var order = await CreatePurchaseOrderAsync(context);
            var item = await CreateItemAsync(context);
            var warehouse = await CreateWarehouseAsync(context);
            var orderItem = await CreatePurchaseOrderItemAsync(context, order.Id, item.Id, quantity: 8);

            var service = CreateService(context);

            await service.ReceiveAsync(orderItem.Id, warehouse.Id, 8);

            var savedOrder = await context.PurchaseOrders.SingleAsync(o => o.Id == order.Id);
            var savedOrderItem = await context.PurchaseOrderItems.SingleAsync(i => i.Id == orderItem.Id);
            var stock = await context.ItemStocks.SingleAsync();
            var movement = await context.StockMovements.SingleAsync();

            Assert.Equal(PurchaseOrderStatus.Completed, savedOrder.Status);
            Assert.Equal(8, savedOrderItem.ReceivedQuantity);
            Assert.Equal(8, stock.Quantity);
            Assert.Equal(8, movement.Quantity);
        }

        [Fact]
        public async Task ReceiveAsync_ReceivesInMultipleBatches_AccumulatesStockAndReceivedQuantity()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var order = await CreatePurchaseOrderAsync(context);
            var item = await CreateItemAsync(context);
            var warehouse = await CreateWarehouseAsync(context);
            var orderItem = await CreatePurchaseOrderItemAsync(context, order.Id, item.Id, quantity: 10);

            var service = CreateService(context);

            await service.ReceiveAsync(orderItem.Id, warehouse.Id, 3);
            await service.ReceiveAsync(orderItem.Id, warehouse.Id, 2);
            await service.ReceiveAsync(orderItem.Id, warehouse.Id, 5);

            var savedOrder = await context.PurchaseOrders.SingleAsync(o => o.Id == order.Id);
            var savedOrderItem = await context.PurchaseOrderItems.SingleAsync(i => i.Id == orderItem.Id);
            var stock = await context.ItemStocks.SingleAsync();
            var movements = await context.StockMovements.OrderBy(m => m.Id).ToListAsync();

            Assert.Equal(PurchaseOrderStatus.Completed, savedOrder.Status);
            Assert.Equal(10, savedOrderItem.ReceivedQuantity);
            Assert.Equal(10, stock.Quantity);
            Assert.Equal(3, movements.Count);
            Assert.Equal(new decimal[] { 3, 2, 5 }, movements.Select(m => m.Quantity).ToArray());
        }

        [Fact]
        public async Task ReceiveAsync_IncreasesExistingStockInsteadOfCreatingAnotherStockRecord()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var order = await CreatePurchaseOrderAsync(context);
            var item = await CreateItemAsync(context);
            var warehouse = await CreateWarehouseAsync(context);
            var orderItem = await CreatePurchaseOrderItemAsync(context, order.Id, item.Id, quantity: 10);

            context.ItemStocks.Add(new ItemStock
            {
                ItemId = item.Id,
                WarehouseId = warehouse.Id,
                Quantity = 7,
                ReservedQuantity = 2
            });
            await context.SaveChangesAsync();

            var service = CreateService(context);

            await service.ReceiveAsync(orderItem.Id, warehouse.Id, 3);

            var stocks = await context.ItemStocks.Where(s => s.ItemId == item.Id && s.WarehouseId == warehouse.Id).ToListAsync();

            Assert.Single(stocks);
            Assert.Equal(10, stocks[0].Quantity);
            Assert.Equal(2, stocks[0].ReservedQuantity);
            Assert.Equal(8, stocks[0].AvailableQuantity);
        }

        // RECEIVE - IDENTIFIERS AND EXISTENCE
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task ReceiveAsync_Throws_WhenPurchaseOrderItemIdIsInvalid(int purchaseOrderItemId)
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.ReceiveAsync(purchaseOrderItemId, 1, 1));

            Assert.Equal("El item de la orden de compra indicado no es válido.", exception.Message);
        }

        [Fact]
        public async Task ReceiveAsync_Throws_WhenPurchaseOrderItemDoesNotExist()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.ReceiveAsync(999, 1, 1));

            Assert.Equal("El item de la orden de compra no existe.", exception.Message);
        }

        // RECEIVE - PURCHASE ORDER VALIDATIONS
        [Fact]
        public async Task ReceiveAsync_Throws_WhenPurchaseOrderIsInactive()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var order = await CreatePurchaseOrderAsync(context, isActive: false);
            var item = await CreateItemAsync(context);
            var warehouse = await CreateWarehouseAsync(context);
            var orderItem = await CreatePurchaseOrderItemAsync(context, order.Id, item.Id);
            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.ReceiveAsync(orderItem.Id, warehouse.Id, 1));

            Assert.Equal("No es posible recibir mercadería de una orden de compra desactivada.", exception.Message);
        }

        [Fact]
        public async Task ReceiveAsync_Throws_WhenPurchaseOrderIsDraft()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var order = await CreatePurchaseOrderAsync(context, status: PurchaseOrderStatus.Draft);
            var item = await CreateItemAsync(context);
            var warehouse = await CreateWarehouseAsync(context);
            var orderItem = await CreatePurchaseOrderItemAsync(context, order.Id, item.Id);
            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.ReceiveAsync(orderItem.Id, warehouse.Id, 1));

            Assert.Equal("Debe confirmar la orden antes de recibir mercadería.", exception.Message);
        }

        [Fact]
        public async Task ReceiveAsync_Throws_WhenPurchaseOrderIsCompleted()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var order = await CreatePurchaseOrderAsync(context, status: PurchaseOrderStatus.Completed);
            var item = await CreateItemAsync(context);
            var warehouse = await CreateWarehouseAsync(context);
            var orderItem = await CreatePurchaseOrderItemAsync(context, order.Id, item.Id);
            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.ReceiveAsync(orderItem.Id, warehouse.Id, 1));

            Assert.Equal("La orden ya fue recibida completamente.", exception.Message);
        }

        [Fact]
        public async Task ReceiveAsync_Throws_WhenPurchaseOrderIsCancelled()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var order = await CreatePurchaseOrderAsync(context, status: PurchaseOrderStatus.Cancelled);
            var item = await CreateItemAsync(context);
            var warehouse = await CreateWarehouseAsync(context);
            var orderItem = await CreatePurchaseOrderItemAsync(context, order.Id, item.Id);
            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.ReceiveAsync(orderItem.Id, warehouse.Id, 1));

            Assert.Equal("No es posible recibir mercadería de una orden cancelada.", exception.Message);
        }

        [Fact]
        public async Task ReceiveAsync_Throws_WhenPurchaseOrderHasAnUnsupportedStatus()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var order = await CreatePurchaseOrderAsync(context, status: (PurchaseOrderStatus)99);
            var item = await CreateItemAsync(context);
            var warehouse = await CreateWarehouseAsync(context);
            var orderItem = await CreatePurchaseOrderItemAsync(context, order.Id, item.Id);
            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.ReceiveAsync(orderItem.Id, warehouse.Id, 1));

            Assert.Equal("La orden de compra no se encuentra en un estado válido para recibir mercadería.", exception.Message);
        }

        // RECEIVE - WAREHOUSE VALIDATIONS
        [Fact]
        public async Task ReceiveAsync_Throws_WhenWarehouseIdIsZero()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var order = await CreatePurchaseOrderAsync(context);
            var item = await CreateItemAsync(context);
            var orderItem = await CreatePurchaseOrderItemAsync(context, order.Id, item.Id);
            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.ReceiveAsync(orderItem.Id, 0, 1));

            Assert.Equal("Debe seleccionar un depósito.", exception.Message);
        }

        [Fact]
        public async Task ReceiveAsync_Throws_WhenWarehouseDoesNotExist()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var order = await CreatePurchaseOrderAsync(context);
            var item = await CreateItemAsync(context);
            var orderItem = await CreatePurchaseOrderItemAsync(context, order.Id, item.Id);
            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.ReceiveAsync(orderItem.Id, 999, 1));

            Assert.Equal("El depósito seleccionado no es válido.", exception.Message);
        }

        [Fact]
        public async Task ReceiveAsync_Throws_WhenWarehouseIsInactive()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var order = await CreatePurchaseOrderAsync(context);
            var item = await CreateItemAsync(context);
            var warehouse = await CreateWarehouseAsync(context, isActive: false);
            var orderItem = await CreatePurchaseOrderItemAsync(context, order.Id, item.Id);
            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.ReceiveAsync(orderItem.Id, warehouse.Id, 1));

            Assert.Equal("El depósito seleccionado no es válido.", exception.Message);
        }

        // RECEIVE - QUANTITY VALIDATIONS
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task ReceiveAsync_Throws_WhenQuantityIsNotPositive(decimal quantity)
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var order = await CreatePurchaseOrderAsync(context);
            var item = await CreateItemAsync(context);
            var warehouse = await CreateWarehouseAsync(context);
            var orderItem = await CreatePurchaseOrderItemAsync(context, order.Id, item.Id);
            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.ReceiveAsync(orderItem.Id, warehouse.Id, quantity));

            Assert.Equal("La cantidad a recibir debe ser mayor que cero.", exception.Message);
        }

        [Fact]
        public async Task ReceiveAsync_Throws_WhenQuantityExceedsPendingQuantity()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var order = await CreatePurchaseOrderAsync(context);
            var item = await CreateItemAsync(context);
            var warehouse = await CreateWarehouseAsync(context);
            var orderItem = await CreatePurchaseOrderItemAsync(context, order.Id, item.Id, quantity: 10, receivedQuantity: 4);
            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.ReceiveAsync(orderItem.Id, warehouse.Id, 7));

            Assert.Equal("La cantidad ingresada supera la cantidad pendiente de recibir.", exception.Message);
        }

        [Fact]
        public async Task ReceiveAsync_Throws_WhenOrderItemIsAlreadyFullyReceived()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var order = await CreatePurchaseOrderAsync(context);
            var item = await CreateItemAsync(context);
            var warehouse = await CreateWarehouseAsync(context);
            var orderItem = await CreatePurchaseOrderItemAsync(context, order.Id, item.Id, quantity: 5, receivedQuantity: 5);
            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.ReceiveAsync(orderItem.Id, warehouse.Id, 1));

            Assert.Equal("Este item ya fue recibido completamente.", exception.Message);
        }

        [Fact]
        public async Task ReceiveAsync_Throws_WhenCatalogItemIsInactive()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var order = await CreatePurchaseOrderAsync(context);
            var item = await CreateItemAsync(context);
            var warehouse = await CreateWarehouseAsync(context);
            var orderItem = await CreatePurchaseOrderItemAsync(context, order.Id, item.Id);

            item.IsActive = false;
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.ReceiveAsync(orderItem.Id, warehouse.Id, 1));

            Assert.Equal("El item seleccionado no existe o está inactivo.", exception.Message);
        }

        // RECEIVE - TRANSACTION ROLLBACK
        [Fact]
        public async Task ReceiveAsync_RollsBackStockChanges_WhenMovementRegistrationFails()
        {
            using var factory = new TestDbContextFactory();
            int orderId;
            int orderItemId;
            int itemId;
            int warehouseId;

            using (var arrangeContext = factory.CreateContext())
            {
                var order = await CreatePurchaseOrderAsync(arrangeContext);
                var item = await CreateItemAsync(arrangeContext);
                var warehouse = await CreateWarehouseAsync(arrangeContext);
                var orderItem = await CreatePurchaseOrderItemAsync(arrangeContext, order.Id, item.Id, quantity: 10);

                orderId = order.Id;
                orderItemId = orderItem.Id;
                itemId = item.Id;
                warehouseId = warehouse.Id;
            }

            using (var actionContext = factory.CreateContext())
            {
                var failingMovementService = new FailingStockMovementService(actionContext);
                var stockService = new StockService(actionContext, failingMovementService);
                var purchaseOrderService = new PurchaseOrderService(actionContext);
                var receivingService = new PurchaseReceivingService(actionContext, stockService, failingMovementService, purchaseOrderService);

                var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => receivingService.ReceiveAsync(orderItemId, warehouseId, 4));

                Assert.Equal("Fallo simulado al registrar el movimiento.", exception.Message);
            }

            using (var assertContext = factory.CreateContext())
            {
                Assert.False(await assertContext.ItemStocks.AnyAsync( s => s.ItemId == itemId && s.WarehouseId == warehouseId));
                Assert.False(await assertContext.StockMovements.AnyAsync( m => m.ItemId == itemId && m.WarehouseId == warehouseId));

                var savedOrderItem = await assertContext.PurchaseOrderItems.SingleAsync(i => i.Id == orderItemId);
                var savedOrder = await assertContext.PurchaseOrders.SingleAsync(o => o.Id == orderId);

                Assert.Equal(0, savedOrderItem.ReceivedQuantity);
                Assert.Equal(PurchaseOrderStatus.Pending, savedOrder.Status);
            }
        }

        // HELPERS
        private static PurchaseReceivingService CreateService(ApplicationDbContext context)
        {
            var stockMovementService = new StockMovementService(context);
            var stockService = new StockService(context, stockMovementService);
            var purchaseOrderService = new PurchaseOrderService(context);

            return new PurchaseReceivingService(context, stockService, stockMovementService, purchaseOrderService);
        }

        private static async Task<Supplier> CreateSupplierAsync(ApplicationDbContext context)
        {
            var supplier = new Supplier
            {
                Code = $"S{Guid.NewGuid():N}"[..12],
                BusinessName = "Proveedor de prueba"
            };

            context.Suppliers.Add(supplier);
            await context.SaveChangesAsync();

            return supplier;
        }

        private static async Task<PurchaseOrder> CreatePurchaseOrderAsync(ApplicationDbContext context, PurchaseOrderStatus status = PurchaseOrderStatus.Pending, bool isActive = true)
        {
            var supplier = await CreateSupplierAsync(context);

            var order = new PurchaseOrder
            {
                Number = $"PO-{Guid.NewGuid():N}"[..12],
                SupplierId = supplier.Id,
                OrderDate = DateOnly.FromDateTime(DateTime.Today),
                Status = status,
                IsActive = isActive
            };

            context.PurchaseOrders.Add(order);
            await context.SaveChangesAsync();

            return order;
        }

        private static async Task<Item> CreateItemAsync(ApplicationDbContext context, bool isActive = true)
        {
            var category = new Category
            {
                Name = $"Categoría {Guid.NewGuid():N}"[..20],
                IsActive = true,
                ItemType = ItemType.RawMaterial
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var item = new Item
            {
                Code = $"I{Guid.NewGuid():N}"[..12],
                Name = "Artículo de prueba",
                CategoryId = category.Id,
                Cost = 100,
                Price = 150,
                MinimumStock = 0,
                IsActive = isActive
            };

            context.Items.Add(item);
            await context.SaveChangesAsync();

            return item;
        }

        private static async Task<Warehouse> CreateWarehouseAsync(ApplicationDbContext context, bool isActive = true)
        {
            var warehouse = new Warehouse
            {
                Code = $"W{Guid.NewGuid():N}"[..12],
                Name = "Depósito de prueba",
                Description = "Depósito para pruebas",
                IsActive = isActive
            };

            context.Warehouses.Add(warehouse);
            await context.SaveChangesAsync();

            return warehouse;
        }

        private static async Task<PurchaseOrderItem> CreatePurchaseOrderItemAsync(ApplicationDbContext context, int purchaseOrderId, int itemId, decimal quantity = 10, decimal receivedQuantity = 0)
        {
            var orderItem = new PurchaseOrderItem
            {
                PurchaseOrderId = purchaseOrderId,
                ItemId = itemId,
                Quantity = quantity,
                ReceivedQuantity = receivedQuantity,
                UnitPrice = 100
            };

            context.PurchaseOrderItems.Add(orderItem);
            await context.SaveChangesAsync();

            return orderItem;
        }

        private sealed class FailingStockMovementService : IStockMovementService
        {
            private readonly ApplicationDbContext _context;

            public FailingStockMovementService(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task RegisterMovementAsync(int itemId, int warehouseId, MovementType movementType, decimal quantity, string? notes = null)
            {
                await _context.SaveChangesAsync();
                throw new InvalidOperationException("Fallo simulado al registrar el movimiento.");
            }
        }
    }
}