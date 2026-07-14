document.addEventListener("DOMContentLoaded", async () => {

    await loadItems();
    await loadWarehouses();

    calculateAvailableStock();

    document
        .getElementById("edit_Quantity")
        ?.addEventListener("input", calculateAvailableStock);

    document
        .getElementById("edit_ReservedQuantity")
        ?.addEventListener("input", calculateAvailableStock);
});

async function loadItems() {

    const response = await fetch("/Stocks/GetItemsList");
    const items = await response.json();

    const createSelect =
        document.getElementById("create_ItemId");

    const editSelect =
        document.getElementById("edit_ItemId");

    if (createSelect) {
        createSelect.innerHTML =
            `<option value="">Seleccione un item...</option>`;

        items.forEach(item => {
            createSelect.innerHTML += `
                <option value="${item.id}">
                    ${item.code} - ${item.name}
                </option>`;
        });
    }

    if (editSelect) {
        editSelect.innerHTML =
            `<option value="">Seleccione un item...</option>`;

        items.forEach(item => {
            editSelect.innerHTML += `
                <option value="${item.id}">
                    ${item.code} - ${item.name}
                </option>`;
        });
    }
}

async function loadWarehouses() {

    const response =
        await fetch("/Stocks/GetWarehousesList");

    const warehouses =
        await response.json();

    const createSelect =
        document.getElementById("create_WarehouseId");

    const editSelect =
        document.getElementById("edit_WarehouseId");

    if (createSelect) {
        createSelect.innerHTML =
            `<option value="">Seleccione un depósito...</option>`;

        warehouses.forEach(warehouse => {
            createSelect.innerHTML += `
                <option value="${warehouse.id}">
                    ${warehouse.code} - ${warehouse.name}
                </option>`;
        });
    }

    if (editSelect) {
        editSelect.innerHTML =
            `<option value="">Seleccione un depósito...</option>`;

        warehouses.forEach(warehouse => {
            editSelect.innerHTML += `
                <option value="${warehouse.id}">
                    ${warehouse.code} - ${warehouse.name}
                </option>`;
        });
    }
}

function openEditStockModal(stock) {

    document.getElementById("edit_Id").value =
        stock.id;

    document.getElementById("edit_ItemId").value =
        stock.itemId;

    document.getElementById("edit_WarehouseId").value =
        stock.warehouseId;

    document.getElementById("edit_Quantity").value =
        stock.quantity;

    document.getElementById("edit_ReservedQuantity").value =
        stock.reservedQuantity;

    calculateAvailableStock();

    const form =
        document.getElementById("editStockForm");

    form.action =
        `/Stocks/Edit/${stock.id}`;

    const modal =
        new bootstrap.Modal(
            document.getElementById("editStockModal"));

    modal.show();
}

function calculateAvailableStock() {

    const quantity =
        parseFloat(
            document.getElementById("edit_Quantity")?.value || 0);

    const reserved =
        parseFloat(
            document.getElementById("edit_ReservedQuantity")?.value || 0);

    const available = quantity - reserved;

    const availableInput =
        document.getElementById("edit_AvailableQuantity");

    if (availableInput) {
        availableInput.value = available.toFixed(2);
    }
}
function openCreateStockModal() {

    const itemSelect =
        document.getElementById("create_ItemId");

    const warehouseSelect =
        document.getElementById("create_WarehouseId");

    const quantityInput =
        document.getElementById("create_Quantity");

    if (itemSelect) itemSelect.value = "";

    if (warehouseSelect) warehouseSelect.value = "";

    if (quantityInput) quantityInput.value = "";

    const modal = new bootstrap.Modal(document.getElementById("createStockModal"));

    modal.show();
}