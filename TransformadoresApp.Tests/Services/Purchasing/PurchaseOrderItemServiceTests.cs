using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Models.Catalogs;
using TransformadoresApp.Models.Purchasing;
using TransformadoresApp.Services.Purchasing;
using TransformadoresApp.Tests.Infrastructure;
using Xunit;

namespace TransformadoresApp.Tests.Services.Purchasing
{
    public class PurchaseOrderItemServiceTests
    {
        // CREATE
        [Fact]
        public async Task CreateAsync_CreatesItemAndResetsReceivedQuantity()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var order = await CreatePurchaseOrderAsync(context, cancellationToken);
            var catalogItem = await CreateItemAsync(context, cancellationToken);
            var service = CreateService(context);

            var item = new PurchaseOrderItem
            {
                PurchaseOrderId = order.Id,
                ItemId = catalogItem.Id,
                Quantity = 10,
                ReceivedQuantity = 5,
                UnitPrice = 100
            };

            await service.CreateAsync(item, cancellationToken);

            var savedItem = await context.PurchaseOrderItems
                .SingleAsync(i => i.Id == item.Id, cancellationToken);

            Assert.Equal(0, savedItem.ReceivedQuantity);
            Assert.Equal(10, savedItem.Quantity);
            Assert.Equal(100, savedItem.UnitPrice);
        }

        [Fact]
        public async Task CreateAsync_Throws_WhenQuantityIsZero()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var service = CreateService(context);

            var item = new PurchaseOrderItem
            {
                PurchaseOrderId = 999,
                ItemId = 999,
                Quantity = 0,
                UnitPrice = 100
            };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(item, cancellationToken));

            Assert.Equal("La cantidad debe ser mayor que cero.", exception.Message);
        }

        [Fact]
        public async Task CreateAsync_Throws_WhenQuantityIsNegative()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var service = CreateService(context);

            var item = new PurchaseOrderItem
            {
                PurchaseOrderId = 999,
                ItemId = 999,
                Quantity = -1,
                UnitPrice = 100
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(item, cancellationToken));
        }

        [Fact]
        public async Task CreateAsync_Throws_WhenUnitPriceIsNegative()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var service = CreateService(context);

            var item = new PurchaseOrderItem
            {
                PurchaseOrderId = 999,
                ItemId = 999,
                Quantity = 1,
                UnitPrice = -1
            };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(item, cancellationToken));

            Assert.Equal("El precio unitario no puede ser negativo.", exception.Message);
        }

        [Fact]
        public async Task CreateAsync_Throws_WhenOrderDoesNotExist()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var catalogItem = await CreateItemAsync(context, cancellationToken);
            var service = CreateService(context);

            var item = new PurchaseOrderItem
            {
                PurchaseOrderId = 999,
                ItemId = catalogItem.Id,
                Quantity = 1,
                UnitPrice = 100
            };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(item, cancellationToken));

            Assert.Equal("La orden de compra no existe.", exception.Message);
        }

        [Fact]
        public async Task CreateAsync_Throws_WhenOrderIsInactive()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var order = await CreatePurchaseOrderAsync(
                context, cancellationToken, isActive: false);

            var catalogItem = await CreateItemAsync(context, cancellationToken);
            var service = CreateService(context);
            var item = NewOrderItem(order.Id, catalogItem.Id);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(item, cancellationToken));

            Assert.Equal(
                "No es posible agregar items a una orden de compra desactivada.",
                exception.Message);
        }

        [Fact]
        public async Task CreateAsync_Throws_WhenOrderIsNotDraft()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var order = await CreatePurchaseOrderAsync(
                context, cancellationToken, status: PurchaseOrderStatus.Pending);

            var catalogItem = await CreateItemAsync(context, cancellationToken);
            var service = CreateService(context);
            var item = NewOrderItem(order.Id, catalogItem.Id);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(item, cancellationToken));

            Assert.Equal(
                "Solo es posible agregar items a una orden en estado borrador.",
                exception.Message);
        }

        [Fact]
        public async Task CreateAsync_Throws_WhenCatalogItemDoesNotExist()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var order = await CreatePurchaseOrderAsync(context, cancellationToken);
            var service = CreateService(context);
            var item = NewOrderItem(order.Id, 999);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(item, cancellationToken));

            Assert.Equal(
                "El item seleccionado no existe o está inactivo.",
                exception.Message);
        }

        [Fact]
        public async Task CreateAsync_Throws_WhenCatalogItemIsInactive()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var order = await CreatePurchaseOrderAsync(context, cancellationToken);
            var catalogItem = await CreateItemAsync(
                context, cancellationToken, isActive: false);

            var service = CreateService(context);
            var item = NewOrderItem(order.Id, catalogItem.Id);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(item, cancellationToken));
        }

        [Fact]
        public async Task CreateAsync_Throws_WhenItemAlreadyExistsInOrder()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var order = await CreatePurchaseOrderAsync(context, cancellationToken);
            var catalogItem = await CreateItemAsync(context, cancellationToken);

            await CreatePurchaseOrderItemAsync(
                context, order.Id, catalogItem.Id, cancellationToken);

            var service = CreateService(context);
            var item = NewOrderItem(order.Id, catalogItem.Id);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(item, cancellationToken));

            Assert.Equal("Ese item ya existe en la orden.", exception.Message);
        }

        // UPDATE
        [Fact]
        public async Task UpdateAsync_UpdatesQuantityAndUnitPrice()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var order = await CreatePurchaseOrderAsync(context, cancellationToken);
            var catalogItem = await CreateItemAsync(context, cancellationToken);

            var existingItem = await CreatePurchaseOrderItemAsync(
                context, order.Id, catalogItem.Id, cancellationToken,
                quantity: 10, receivedQuantity: 0);

            var service = CreateService(context);

            await service.UpdateAsync(new PurchaseOrderItem
            {
                Id = existingItem.Id,
                PurchaseOrderId = order.Id,
                ItemId = catalogItem.Id,
                Quantity = 20,
                UnitPrice = 150
            }, cancellationToken);

            var updatedItem = await context.PurchaseOrderItems
                .SingleAsync(i => i.Id == existingItem.Id, cancellationToken);

            Assert.Equal(20, updatedItem.Quantity);
            Assert.Equal(150, updatedItem.UnitPrice);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenQuantityIsZero()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateAsync(new PurchaseOrderItem
                {
                    Id = 999,
                    Quantity = 0,
                    UnitPrice = 100
                }, cancellationToken));

            Assert.Equal("La cantidad debe ser mayor que cero.", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenUnitPriceIsNegative()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateAsync(new PurchaseOrderItem
                {
                    Id = 999,
                    Quantity = 1,
                    UnitPrice = -1
                }, cancellationToken));

            Assert.Equal("El precio unitario no puede ser negativo.", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenOrderItemDoesNotExist()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateAsync(new PurchaseOrderItem
                {
                    Id = 999,
                    PurchaseOrderId = 1,
                    ItemId = 1,
                    Quantity = 1,
                    UnitPrice = 100
                }, cancellationToken));

            Assert.Equal("El item de la orden no existe.", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenOrderIdDoesNotMatch()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var order = await CreatePurchaseOrderAsync(context, cancellationToken);
            var catalogItem = await CreateItemAsync(context, cancellationToken);

            var existingItem = await CreatePurchaseOrderItemAsync(
                context, order.Id, catalogItem.Id, cancellationToken);

            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateAsync(new PurchaseOrderItem
                {
                    Id = existingItem.Id,
                    PurchaseOrderId = order.Id + 1,
                    ItemId = catalogItem.Id,
                    Quantity = 2,
                    UnitPrice = 100
                }, cancellationToken));

            Assert.Equal(
                "La orden de compra indicada no coincide con el item.",
                exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenOrderIsInactive()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var order = await CreatePurchaseOrderAsync(
                context, cancellationToken, isActive: false);

            var catalogItem = await CreateItemAsync(context, cancellationToken);

            var existingItem = await CreatePurchaseOrderItemAsync(
                context, order.Id, catalogItem.Id, cancellationToken);

            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateAsync(new PurchaseOrderItem
                {
                    Id = existingItem.Id,
                    PurchaseOrderId = order.Id,
                    ItemId = catalogItem.Id,
                    Quantity = 2,
                    UnitPrice = 100
                }, cancellationToken));

            Assert.Equal(
                "No es posible modificar items de una orden de compra desactivada.",
                exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenOrderIsNotDraft()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var order = await CreatePurchaseOrderAsync(
                context, cancellationToken, status: PurchaseOrderStatus.Pending);

            var catalogItem = await CreateItemAsync(context, cancellationToken);

            var existingItem = await CreatePurchaseOrderItemAsync(
                context, order.Id, catalogItem.Id, cancellationToken);

            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateAsync(new PurchaseOrderItem
                {
                    Id = existingItem.Id,
                    PurchaseOrderId = order.Id,
                    ItemId = catalogItem.Id,
                    Quantity = 2,
                    UnitPrice = 100
                }, cancellationToken));

            Assert.Equal(
                "Solo es posible modificar items de una orden en estado borrador.",
                exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenCatalogItemIsInactive()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var order = await CreatePurchaseOrderAsync(context, cancellationToken);
            var catalogItem = await CreateItemAsync(context, cancellationToken);

            var existingItem = await CreatePurchaseOrderItemAsync(
                context, order.Id, catalogItem.Id, cancellationToken);

            catalogItem.IsActive = false;
            await context.SaveChangesAsync(cancellationToken);

            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateAsync(new PurchaseOrderItem
                {
                    Id = existingItem.Id,
                    PurchaseOrderId = order.Id,
                    ItemId = catalogItem.Id,
                    Quantity = 2,
                    UnitPrice = 100
                }, cancellationToken));

            Assert.Equal(
                "El item seleccionado no existe o está inactivo.",
                exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenChangingCatalogItem()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var order = await CreatePurchaseOrderAsync(context, cancellationToken);
            var originalItem = await CreateItemAsync(context, cancellationToken);
            var otherItem = await CreateItemAsync(context, cancellationToken);

            var existingItem = await CreatePurchaseOrderItemAsync(
                context, order.Id, originalItem.Id, cancellationToken);

            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateAsync(new PurchaseOrderItem
                {
                    Id = existingItem.Id,
                    PurchaseOrderId = order.Id,
                    ItemId = otherItem.Id,
                    Quantity = 2,
                    UnitPrice = 100
                }, cancellationToken));

            Assert.Equal(
                "No es posible cambiar el item de una línea existente.",
                exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenQuantityIsLessThanReceived()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var order = await CreatePurchaseOrderAsync(context, cancellationToken);
            var catalogItem = await CreateItemAsync(context, cancellationToken);

            var existingItem = await CreatePurchaseOrderItemAsync(
                context, order.Id, catalogItem.Id, cancellationToken,
                quantity: 10, receivedQuantity: 5);

            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateAsync(new PurchaseOrderItem
                {
                    Id = existingItem.Id,
                    PurchaseOrderId = order.Id,
                    ItemId = catalogItem.Id,
                    Quantity = 4,
                    UnitPrice = 100
                }, cancellationToken));

            Assert.Contains("La cantidad no puede ser menor", exception.Message);
        }

        // DELETE
        [Fact]
        public async Task DeleteAsync_DeletesItemAndReturnsPurchaseOrderId()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var order = await CreatePurchaseOrderAsync(context, cancellationToken);
            var catalogItem = await CreateItemAsync(context, cancellationToken);

            var existingItem = await CreatePurchaseOrderItemAsync(
                context, order.Id, catalogItem.Id, cancellationToken);

            var service = CreateService(context);

            var returnedOrderId = await service.DeleteAsync(
                existingItem.Id, cancellationToken);

            Assert.Equal(order.Id, returnedOrderId);

            Assert.False(await context.PurchaseOrderItems
                .AnyAsync(i => i.Id == existingItem.Id, cancellationToken));
        }

        [Fact]
        public async Task DeleteAsync_Throws_WhenOrderItemDoesNotExist()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.DeleteAsync(999, cancellationToken));

            Assert.Equal("El item de la orden no existe.", exception.Message);
        }

        [Fact]
        public async Task DeleteAsync_Throws_WhenOrderIsInactive()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var order = await CreatePurchaseOrderAsync(
                context, cancellationToken, isActive: false);

            var catalogItem = await CreateItemAsync(context, cancellationToken);

            var existingItem = await CreatePurchaseOrderItemAsync(
                context, order.Id, catalogItem.Id, cancellationToken);

            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.DeleteAsync(existingItem.Id, cancellationToken));

            Assert.Equal(
                "No es posible eliminar items de una orden de compra desactivada.",
                exception.Message);
        }

        [Fact]
        public async Task DeleteAsync_Throws_WhenOrderIsNotDraft()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var order = await CreatePurchaseOrderAsync(
                context, cancellationToken, status: PurchaseOrderStatus.Pending);

            var catalogItem = await CreateItemAsync(context, cancellationToken);

            var existingItem = await CreatePurchaseOrderItemAsync(
                context, order.Id, catalogItem.Id, cancellationToken);

            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.DeleteAsync(existingItem.Id, cancellationToken));

            Assert.Equal(
                "Solo es posible eliminar items de una orden en estado borrador.",
                exception.Message);
        }

        [Fact]
        public async Task DeleteAsync_Throws_WhenItemHasReceivedQuantity()
        {
            using var factory = new TestDbContextFactory();
            using var context = factory.CreateContext();
            var cancellationToken = TestContext.Current.CancellationToken;

            var order = await CreatePurchaseOrderAsync(context, cancellationToken);
            var catalogItem = await CreateItemAsync(context, cancellationToken);

            var existingItem = await CreatePurchaseOrderItemAsync(
                context, order.Id, catalogItem.Id, cancellationToken,
                quantity: 10, receivedQuantity: 1);

            var service = CreateService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.DeleteAsync(existingItem.Id, cancellationToken));

            Assert.Equal(
                "No es posible eliminar un item que ya tiene mercadería recibida.",
                exception.Message);
        }

        // HELPERS
        private static PurchaseOrderItemService CreateService(
            TransformadoresApp.Data.ApplicationDbContext context)
        {
            var purchaseOrderService = new PurchaseOrderService(context);
            return new PurchaseOrderItemService(context, purchaseOrderService);
        }

        private static PurchaseOrderItem NewOrderItem(int purchaseOrderId, int itemId)
        {
            return new PurchaseOrderItem
            {
                PurchaseOrderId = purchaseOrderId,
                ItemId = itemId,
                Quantity = 1,
                UnitPrice = 100
            };
        }

        private static async Task<Supplier> CreateSupplierAsync(
            TransformadoresApp.Data.ApplicationDbContext context,
            CancellationToken cancellationToken)
        {
            var supplier = new Supplier
            {
                Code = $"S{Guid.NewGuid():N}"[..12],
                BusinessName = "Proveedor de prueba"
            };

            context.Suppliers.Add(supplier);
            await context.SaveChangesAsync(cancellationToken);

            return supplier;
        }

        private static async Task<PurchaseOrder> CreatePurchaseOrderAsync(
            TransformadoresApp.Data.ApplicationDbContext context,
            CancellationToken cancellationToken,
            PurchaseOrderStatus status = PurchaseOrderStatus.Draft,
            bool isActive = true)
        {
            var supplier = await CreateSupplierAsync(context, cancellationToken);

            var order = new PurchaseOrder
            {
                Number = $"PO-{Guid.NewGuid():N}"[..12],
                SupplierId = supplier.Id,
                OrderDate = DateOnly.FromDateTime(DateTime.Today),
                Status = status,
                IsActive = isActive
            };

            context.PurchaseOrders.Add(order);
            await context.SaveChangesAsync(cancellationToken);

            return order;
        }

        private static async Task<Item> CreateItemAsync(
            TransformadoresApp.Data.ApplicationDbContext context,
            CancellationToken cancellationToken,
            bool isActive = true)
        {
            var category = new Category
            {
                Name = "Categoría de prueba",
                IsActive = true,
                ItemType = ItemType.RawMaterial
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync(cancellationToken);

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
            await context.SaveChangesAsync(cancellationToken);

            return item;
        }

        private static async Task<PurchaseOrderItem> CreatePurchaseOrderItemAsync(
            TransformadoresApp.Data.ApplicationDbContext context,
            int purchaseOrderId,
            int itemId,
            CancellationToken cancellationToken,
            decimal quantity = 10,
            decimal receivedQuantity = 0)
        {
            var item = new PurchaseOrderItem
            {
                PurchaseOrderId = purchaseOrderId,
                ItemId = itemId,
                Quantity = quantity,
                ReceivedQuantity = receivedQuantity,
                UnitPrice = 100
            };

            context.PurchaseOrderItems.Add(item);
            await context.SaveChangesAsync(cancellationToken);

            return item;
        }
    }
}