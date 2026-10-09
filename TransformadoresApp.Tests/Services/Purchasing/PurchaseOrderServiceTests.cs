using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Models.Catalogs;
using TransformadoresApp.Models.Purchasing;
using TransformadoresApp.Services.Purchasing;
using TransformadoresApp.Tests.Infrastructure;

namespace TransformadoresApp.Tests.Services.Purchasing
{
    public class PurchaseOrderServiceTests
    {
        [Fact]
        public async Task CreateAsync_CreatesPurchaseOrder_WhenDataIsValid()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var supplier = await CreateSupplierAsync(context);

            var purchaseOrder = new PurchaseOrder
            {
                Number = " OC-001 ",
                SupplierId = supplier.Id,
                OrderDate = DateOnly.FromDateTime(DateTime.Today),
                Notes = "Orden de prueba"
            };

            var service = new PurchaseOrderService(context);

            await service.CreateAsync(purchaseOrder);

            var createdOrder = await context.PurchaseOrders.SingleAsync();

            Assert.Equal("OC-001", createdOrder.Number);
            Assert.Equal(supplier.Id, createdOrder.SupplierId);
            Assert.Equal(PurchaseOrderStatus.Draft, createdOrder.Status);
            Assert.True(createdOrder.IsActive);
            Assert.Equal("Orden de prueba", createdOrder.Notes);
        }

        [Fact]
        public async Task CreateAsync_Throws_WhenNumberAlreadyExists()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var supplier = await CreateSupplierAsync(context);

            var existingOrder = new PurchaseOrder
            {
                Number = "OC-002",
                SupplierId = supplier.Id,
                OrderDate = DateOnly.FromDateTime(DateTime.Today),
                Status = PurchaseOrderStatus.Draft,
                IsActive = true
            };

            context.PurchaseOrders.Add(existingOrder);
            await context.SaveChangesAsync();

            var service = new PurchaseOrderService(context);

            var newOrder = new PurchaseOrder
            {
                Number = " OC-002 ",
                SupplierId = supplier.Id,
                OrderDate = DateOnly.FromDateTime(DateTime.Today)
            };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.CreateAsync(newOrder));

            Assert.Equal("Ya existe una orden de compra con ese número.", exception.Message);

            Assert.Single(context.PurchaseOrders);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesPurchaseOrder_WhenOrderIsDraft()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var firstSupplier = await CreateSupplierAsync(
                context,
                "SUP-001",
                "Proveedor original");

            var secondSupplier = await CreateSupplierAsync(
                context,
                "SUP-002",
                "Proveedor modificado");

            var existingOrder = new PurchaseOrder
            {
                Number = "OC-003",
                SupplierId = firstSupplier.Id,
                OrderDate = DateOnly.FromDateTime(DateTime.Today),
                Status = PurchaseOrderStatus.Draft,
                IsActive = true,
                Notes = "Original"
            };

            context.PurchaseOrders.Add(existingOrder);
            await context.SaveChangesAsync();

            var service = new PurchaseOrderService(context);

            var updatedOrder = new PurchaseOrder
            {
                Id = existingOrder.Id,
                Number = " OC-003-MOD ",
                SupplierId = secondSupplier.Id,
                OrderDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                ExpectedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
                Notes = "Modificada"
            };

            await service.UpdateAsync(existingOrder.Id, updatedOrder);

            var order = await context.PurchaseOrders.SingleAsync();

            Assert.Equal("OC-003-MOD", order.Number);
            Assert.Equal(secondSupplier.Id, order.SupplierId);
            Assert.Equal(DateOnly.FromDateTime(DateTime.Today.AddDays(1)), order.OrderDate);
            Assert.Equal(DateOnly.FromDateTime(DateTime.Today.AddDays(7)), order.ExpectedDate);
            Assert.Equal("Modificada", order.Notes);
            Assert.Equal(PurchaseOrderStatus.Draft, order.Status);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenIdsDoNotMatch()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var service = new PurchaseOrderService(context);

            var purchaseOrder = new PurchaseOrder
            {
                Id = 2,
                Number = "OC-004",
                SupplierId = 1,
                OrderDate = DateOnly.FromDateTime(DateTime.Today)
            };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.UpdateAsync(1, purchaseOrder));

            Assert.Equal("La orden de compra indicada no coincide.", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenOrderDoesNotExist()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var service = new PurchaseOrderService(context);

            var purchaseOrder = new PurchaseOrder
            {
                Id = 999,
                Number = "OC-005",
                SupplierId = 1,
                OrderDate = DateOnly.FromDateTime(DateTime.Today)
            };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.UpdateAsync(999, purchaseOrder));

            Assert.Equal("La orden de compra no existe.", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenOrderIsInactive()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var supplier = await CreateSupplierAsync(context);

            var existingOrder = new PurchaseOrder
            {
                Number = "OC-006",
                SupplierId = supplier.Id,
                OrderDate = DateOnly.FromDateTime(DateTime.Today),
                Status = PurchaseOrderStatus.Draft,
                IsActive = false
            };

            context.PurchaseOrders.Add(existingOrder);
            await context.SaveChangesAsync();

            var service = new PurchaseOrderService(context);

            var updatedOrder = new PurchaseOrder
            {
                Id = existingOrder.Id,
                Number = "OC-006-MOD",
                SupplierId = supplier.Id,
                OrderDate = DateOnly.FromDateTime(DateTime.Today)
            };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.UpdateAsync(existingOrder.Id, updatedOrder));

            Assert.Equal("No es posible modificar una orden de compra desactivada.", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenOrderIsNotDraft()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var supplier = await CreateSupplierAsync(context);

            var existingOrder = new PurchaseOrder
            {
                Number = "OC-007",
                SupplierId = supplier.Id,
                OrderDate = DateOnly.FromDateTime(DateTime.Today),
                Status = PurchaseOrderStatus.Pending,
                IsActive = true
            };

            context.PurchaseOrders.Add(existingOrder);
            await context.SaveChangesAsync();

            var service = new PurchaseOrderService(context);

            var updatedOrder = new PurchaseOrder
            {
                Id = existingOrder.Id,
                Number = "OC-007-MOD",
                SupplierId = supplier.Id,
                OrderDate = DateOnly.FromDateTime(DateTime.Today)
            };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.UpdateAsync(existingOrder.Id, updatedOrder));

            Assert.Equal("Solo se pueden modificar órdenes de compra en estado borrador.", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenNewNumberAlreadyExists()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var supplier = await CreateSupplierAsync(context);

            var firstOrder = new PurchaseOrder
            {
                Number = "OC-008",
                SupplierId = supplier.Id,
                OrderDate = DateOnly.FromDateTime(DateTime.Today),
                Status = PurchaseOrderStatus.Draft,
                IsActive = true
            };

            var secondOrder = new PurchaseOrder
            {
                Number = "OC-009",
                SupplierId = supplier.Id,
                OrderDate = DateOnly.FromDateTime(DateTime.Today),
                Status = PurchaseOrderStatus.Draft,
                IsActive = true
            };

            context.PurchaseOrders.AddRange(firstOrder, secondOrder);
            await context.SaveChangesAsync();

            var service = new PurchaseOrderService(context);

            var updatedOrder = new PurchaseOrder
            {
                Id = secondOrder.Id,
                Number = " OC-008 ",
                SupplierId = supplier.Id,
                OrderDate = DateOnly.FromDateTime(DateTime.Today)
            };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.UpdateAsync(secondOrder.Id, updatedOrder));

            Assert.Equal("Ya existe una orden de compra con ese número.", exception.Message);

            var order = await context.PurchaseOrders.SingleAsync(po => po.Id == secondOrder.Id);

            Assert.Equal("OC-009", order.Number);
        }

        [Fact]
        public async Task ConfirmAsync_ConfirmsPurchaseOrder_WhenDraftHasItems()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var supplier = await CreateSupplierAsync(context);
            var item = await CreateItemAsync(context);

            var purchaseOrder = await CreatePurchaseOrderAsync(context, supplier.Id, "OC-101", PurchaseOrderStatus.Draft, true);

            await CreatePurchaseOrderItemAsync(context, purchaseOrder.Id, item.Id, 10);

            var service = new PurchaseOrderService(context);

            await service.ConfirmAsync(purchaseOrder.Id);

            var order = await context.PurchaseOrders.SingleAsync(po => po.Id == purchaseOrder.Id);

            Assert.Equal(PurchaseOrderStatus.Pending, order.Status);
        }

        [Fact]
        public async Task ConfirmAsync_Throws_WhenOrderDoesNotExist()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var service = new PurchaseOrderService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.ConfirmAsync(999));

            Assert.Equal("La orden de compra no existe.", exception.Message);
        }

        [Fact]
        public async Task ConfirmAsync_Throws_WhenOrderIsInactive()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var supplier = await CreateSupplierAsync(context);

            var purchaseOrder = await CreatePurchaseOrderAsync(context, supplier.Id, "OC-102", PurchaseOrderStatus.Draft, false);

            var service = new PurchaseOrderService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.ConfirmAsync(purchaseOrder.Id));

            Assert.Equal("No es posible confirmar una orden de compra desactivada.", exception.Message);
        }

        [Fact]
        public async Task ConfirmAsync_Throws_WhenOrderIsNotDraft()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var supplier = await CreateSupplierAsync(context);

            var purchaseOrder = await CreatePurchaseOrderAsync(context, supplier.Id, "OC-103", PurchaseOrderStatus.Pending, true);

            var service = new PurchaseOrderService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.ConfirmAsync(purchaseOrder.Id));

            Assert.Equal("La orden de compra ya fue confirmada o no se encuentra en estado borrador.", exception.Message);
        }

        [Fact]
        public async Task ConfirmAsync_Throws_WhenOrderHasNoItems()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var supplier = await CreateSupplierAsync(context);

            var purchaseOrder = await CreatePurchaseOrderAsync(context, supplier.Id, "OC-104", PurchaseOrderStatus.Draft, true);

            var service = new PurchaseOrderService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.ConfirmAsync(purchaseOrder.Id));

            Assert.Equal("No es posible confirmar una orden de compra sin items.", exception.Message);
        }

        [Fact]
        public async Task CancelAsync_CancelsDraftOrder()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var supplier = await CreateSupplierAsync(context);

            var purchaseOrder = await CreatePurchaseOrderAsync(context, supplier.Id, "OC-201", PurchaseOrderStatus.Draft, true);

            var service = new PurchaseOrderService(context);

            await service.CancelAsync(purchaseOrder.Id);

            var order = await context.PurchaseOrders.SingleAsync(po => po.Id == purchaseOrder.Id);

            Assert.Equal(PurchaseOrderStatus.Cancelled, order.Status);
        }

        [Fact]
        public async Task CancelAsync_CancelsPendingOrder()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var supplier = await CreateSupplierAsync(context);

            var purchaseOrder = await CreatePurchaseOrderAsync(context, supplier.Id, "OC-202", PurchaseOrderStatus.Pending, true);

            var service = new PurchaseOrderService(context);

            await service.CancelAsync(purchaseOrder.Id);

            var order = await context.PurchaseOrders.SingleAsync(po => po.Id == purchaseOrder.Id);

            Assert.Equal(PurchaseOrderStatus.Cancelled, order.Status);
        }

        [Fact]
        public async Task CancelAsync_Throws_WhenOrderDoesNotExist()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var service = new PurchaseOrderService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.CancelAsync(999));

            Assert.Equal("La orden de compra no existe.", exception.Message);
        }

        [Fact]
        public async Task CancelAsync_Throws_WhenOrderIsInactive()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var supplier = await CreateSupplierAsync(context);

            var purchaseOrder = await CreatePurchaseOrderAsync(context, supplier.Id, "OC-203", PurchaseOrderStatus.Draft, false);

            var service = new PurchaseOrderService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.CancelAsync(purchaseOrder.Id));

            Assert.Equal("No es posible cancelar una orden de compra desactivada.", exception.Message);
        }

        [Fact]
        public async Task CancelAsync_Throws_WhenOrderIsNotDraftOrPending()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var supplier = await CreateSupplierAsync(context);

            var purchaseOrder = await CreatePurchaseOrderAsync(context, supplier.Id, "OC-204", PurchaseOrderStatus.Completed, true);

            var service = new PurchaseOrderService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.CancelAsync(purchaseOrder.Id));

            Assert.Equal("Solo se pueden cancelar órdenes de compra en estado borrador o pendiente.", exception.Message);
        }

        [Fact]
        public async Task DeactivateAsync_DeactivatesActiveOrder()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var supplier = await CreateSupplierAsync(context);

            var purchaseOrder = await CreatePurchaseOrderAsync(context, supplier.Id, "OC-301", PurchaseOrderStatus.Draft, true);

            var service = new PurchaseOrderService(context);

            await service.DeactivateAsync(purchaseOrder.Id);

            var order = await context.PurchaseOrders.SingleAsync(po => po.Id == purchaseOrder.Id);

            Assert.False(order.IsActive);
        }

        [Fact]
        public async Task DeactivateAsync_Throws_WhenOrderDoesNotExist()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var service = new PurchaseOrderService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.DeactivateAsync(999));

            Assert.Equal("La orden de compra no existe.", exception.Message);
        }

        [Fact]
        public async Task DeactivateAsync_Throws_WhenOrderIsAlreadyInactive()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var supplier = await CreateSupplierAsync(context);

            var purchaseOrder = await CreatePurchaseOrderAsync(context, supplier.Id, "OC-302", PurchaseOrderStatus.Draft, false);

            var service = new PurchaseOrderService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.DeactivateAsync(purchaseOrder.Id));

            Assert.Equal("La orden ya se encuentra desactivada.", exception.Message);
        }

        [Fact]
        public async Task RestoreAsync_RestoresInactiveOrder()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var supplier = await CreateSupplierAsync(context);

            var purchaseOrder = await CreatePurchaseOrderAsync(context, supplier.Id, "OC-401", PurchaseOrderStatus.Draft, false);

            var service = new PurchaseOrderService(context);

            await service.RestoreAsync(purchaseOrder.Id);

            var order = await context.PurchaseOrders.SingleAsync(po => po.Id == purchaseOrder.Id);

            Assert.True(order.IsActive);
        }

        [Fact]
        public async Task RestoreAsync_Throws_WhenOrderDoesNotExist()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var service = new PurchaseOrderService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.RestoreAsync(999));

            Assert.Equal("La orden de compra no existe.", exception.Message);
        }

        [Fact]
        public async Task RestoreAsync_Throws_WhenOrderIsAlreadyActive()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var supplier = await CreateSupplierAsync(context);

            var purchaseOrder = await CreatePurchaseOrderAsync(context, supplier.Id, "OC-402", PurchaseOrderStatus.Draft, true);

            var service = new PurchaseOrderService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.RestoreAsync(purchaseOrder.Id));

            Assert.Equal("La orden ya se encuentra activa.", exception.Message);
        }

        [Fact]
        public async Task UpdateStatusAsync_KeepsDraftStatus()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var supplier = await CreateSupplierAsync(context);

            var purchaseOrder = await CreatePurchaseOrderAsync(context, supplier.Id, "OC-501", PurchaseOrderStatus.Draft, true);

            var service = new PurchaseOrderService(context);

            await service.UpdateStatusAsync(purchaseOrder.Id);

            var order = await context.PurchaseOrders.SingleAsync(po => po.Id == purchaseOrder.Id);

            Assert.Equal(PurchaseOrderStatus.Draft, order.Status);
        }

        [Fact]
        public async Task UpdateStatusAsync_KeepsCancelledStatus()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var supplier = await CreateSupplierAsync(context);

            var purchaseOrder = await CreatePurchaseOrderAsync(context, supplier.Id, "OC-502", PurchaseOrderStatus.Cancelled, true);

            var service = new PurchaseOrderService(context);

            await service.UpdateStatusAsync(purchaseOrder.Id);

            var order = await context.PurchaseOrders.SingleAsync(po => po.Id == purchaseOrder.Id);

            Assert.Equal(PurchaseOrderStatus.Cancelled, order.Status);
        }

        [Fact]
        public async Task UpdateStatusAsync_Throws_WhenOrderDoesNotExist()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var service = new PurchaseOrderService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>( () => service.UpdateStatusAsync(999));

            Assert.Equal("La orden de compra no existe.", exception.Message);
        }

        [Fact]
        public async Task UpdateStatusAsync_SetsDraft_WhenOrderHasNoItems()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var supplier = await CreateSupplierAsync(context);

            var purchaseOrder = await CreatePurchaseOrderAsync(context, supplier.Id, "OC-503", PurchaseOrderStatus.Pending, true);

            var service = new PurchaseOrderService(context);

            await service.UpdateStatusAsync(purchaseOrder.Id);

            var order = await context.PurchaseOrders.SingleAsync(po => po.Id == purchaseOrder.Id);

            Assert.Equal(PurchaseOrderStatus.Draft, order.Status);
        }

        [Fact]
        public async Task UpdateStatusAsync_SetsCompleted_WhenAllItemsAreReceived()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var supplier = await CreateSupplierAsync(context);
            var item = await CreateItemAsync(context);

            var purchaseOrder = await CreatePurchaseOrderAsync(context, supplier.Id, "OC-504", PurchaseOrderStatus.Pending, true);

            await CreatePurchaseOrderItemAsync(context, purchaseOrder.Id, item.Id, 10, 10);

            var service = new PurchaseOrderService(context);

            await service.UpdateStatusAsync(purchaseOrder.Id);

            var order = await context.PurchaseOrders.SingleAsync(po => po.Id == purchaseOrder.Id);

            Assert.Equal(PurchaseOrderStatus.Completed, order.Status);
        }

        [Fact]
        public async Task UpdateStatusAsync_SetsPending_WhenNoItemsAreReceived()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var supplier = await CreateSupplierAsync(context);
            var item = await CreateItemAsync(context);

            var purchaseOrder = await CreatePurchaseOrderAsync(context, supplier.Id, "OC-505", PurchaseOrderStatus.Pending, true);

            await CreatePurchaseOrderItemAsync(context, purchaseOrder.Id, item.Id, 10, 0);

            var service = new PurchaseOrderService(context);

            await service.UpdateStatusAsync(purchaseOrder.Id);

            var order = await context.PurchaseOrders.SingleAsync(po => po.Id == purchaseOrder.Id);

            Assert.Equal(PurchaseOrderStatus.Pending, order.Status);
        }

        [Fact]
        public async Task UpdateStatusAsync_SetsPartiallyReceived_WhenSomeQuantityIsReceived()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();

            var supplier = await CreateSupplierAsync(context);
            var item = await CreateItemAsync(context);

            var purchaseOrder = await CreatePurchaseOrderAsync(context, supplier.Id, "OC-506", PurchaseOrderStatus.Pending, true);

            await CreatePurchaseOrderItemAsync(context, purchaseOrder.Id, item.Id, 10, 5);

            var service = new PurchaseOrderService(context);

            await service.UpdateStatusAsync(purchaseOrder.Id);

            var order = await context.PurchaseOrders.SingleAsync(po => po.Id == purchaseOrder.Id);

            Assert.Equal(PurchaseOrderStatus.PartiallyReceived, order.Status);
        }

        private static async Task<Supplier> CreateSupplierAsync(DbContext context, string code = "SUP-001", string businessName = "Proveedor de prueba")
        {
            var supplier = new Supplier
            {
                Code = code,
                BusinessName = businessName
            };

            context.Add(supplier);
            await context.SaveChangesAsync();

            return supplier;
        }

        private static async Task<Item> CreateItemAsync(DbContext context)
        {
            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true,
                ItemType = ItemType.RawMaterial
            };

            context.Add(category);
            await context.SaveChangesAsync();

            var item = new Item
            {
                Code = $"ITEM-{Guid.NewGuid():N}"[..12],
                Name = "Item de prueba",
                CategoryId = category.Id,
                Cost = 100,
                Price = 150,
                MinimumStock = 0,
                IsActive = true
            };

            context.Add(item);
            await context.SaveChangesAsync();

            return item;
        }

        private static async Task<PurchaseOrder> CreatePurchaseOrderAsync(DbContext context, int supplierId, string number, PurchaseOrderStatus status, bool isActive)
        {
            var purchaseOrder = new PurchaseOrder
            {
                Number = number,
                SupplierId = supplierId,
                OrderDate = DateOnly.FromDateTime(DateTime.Today),
                Status = status,
                IsActive = isActive
            };

            context.Add(purchaseOrder);
            await context.SaveChangesAsync();

            return purchaseOrder;
        }

        private static async Task<PurchaseOrderItem> CreatePurchaseOrderItemAsync(DbContext context, int purchaseOrderId, int itemId, decimal quantity, decimal receivedQuantity = 0)
        {
            var purchaseOrderItem = new PurchaseOrderItem
            {
                PurchaseOrderId = purchaseOrderId,
                ItemId = itemId,
                Quantity = quantity,
                ReceivedQuantity = receivedQuantity,
                UnitPrice = 100
            };

            context.Add(purchaseOrderItem);
            await context.SaveChangesAsync();

            return purchaseOrderItem;
        }
    }
}