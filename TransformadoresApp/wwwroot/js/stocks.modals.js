document.addEventListener("DOMContentLoaded", async () => {

    await loadItems();
    await loadWarehouses();

    calculateNewStock();

    document.getElementById("edit_Adjustment")?.addEventListener("input", calculateNewStock);
});

async function loadItems() {

    const response = await fetch("/Stocks/GetItemsList");
    const items = await response.json();

    const createSelect = document.getElementById("create_ItemId");

    const editSelect = document.getElementById("edit_ItemId");

    if (createSelect) {
        createSelect.innerHTML = `<option value="">Seleccione un item...</option>`;

        items.forEach(item => {
            createSelect.innerHTML += `
                <option value="${item.id}">
                    ${item.code} - ${item.name}
                </option>`;
        });
    }

    if (editSelect) {
        editSelect.innerHTML = `<option value="">Seleccione un item...</option>`;

        items.forEach(item => {
            editSelect.innerHTML += `
                <option value="${item.id}">
                    ${item.code} - ${item.name}
                </option>`;
        });
    }
}

async function loadWarehouses() {

    const response = await fetch("/Stocks/GetWarehousesList");

    const warehouses = await response.json();

    const createSelect = document.getElementById("create_WarehouseId");

    const editSelect = document.getElementById("edit_WarehouseId");

    if (createSelect) {
        createSelect.innerHTML = `<option value="">Seleccione un depósito...</option>`;

        warehouses.forEach(warehouse => {
            createSelect.innerHTML += `
                <option value="${warehouse.id}">
                    ${warehouse.code} - ${warehouse.name}
                </option>`;
        });
    }

    if (editSelect) {
        editSelect.innerHTML = `<option value="">Seleccione un depósito...</option>`;

        warehouses.forEach(warehouse => {
            editSelect.innerHTML += `
                <option value="${warehouse.id}">
                    ${warehouse.code} - ${warehouse.name}
                </option>`;
        });
    }
}

function openEditStockModal(stock) {

    document.getElementById("edit_Id").value = stock.id;

    document.getElementById("edit_ItemId").value = stock.itemId;

    document.getElementById("edit_WarehouseId").value = stock.warehouseId;

    document.getElementById("edit_CurrentQuantity").value = Number(stock.quantity).toFixed(2);

    document.getElementById("edit_Adjustment").value = "";

    document.getElementById("edit_ReservedQuantity").value = Number(stock.reservedQuantity).toFixed(2);

    const newStock = document.getElementById("edit_NewQuantity");

    if (newStock) {
        newStock.classList.remove("is-valid");
        newStock.classList.remove("is-invalid");
    }

    const validation = document.getElementById("edit_AdjustmentValidation");

    if (validation) {
        validation.style.display = "none";
    }

    const submitButton = document.querySelector("#editStockForm button[type='submit']");

    if (submitButton) {
        submitButton.disabled = false;
    }

    calculateNewStock();

    const form = document.getElementById("editStockForm");

    form.action = `/Stocks/Edit/${stock.id}`;

    const modal = new bootstrap.Modal(document.getElementById("editStockModal"));

    modal.show();
}

function calculateNewStock() {

    const current = parseFloat(document.getElementById("edit_CurrentQuantity")?.value || 0);

    const adjustment = parseFloat(document.getElementById("edit_Adjustment")?.value || 0);

    const newQuantity = current + adjustment;

    const newStock = document.getElementById("edit_NewQuantity");

    if (newStock) {
        newStock.value = newQuantity.toFixed(2);

        const validation = document.getElementById("edit_AdjustmentValidation");

        const submitButton = document.querySelector("#editStockForm button[type='submit']");

        if (newQuantity < 0) {

            newStock.classList.add("is-invalid");
            newStock.classList.remove("is-valid");

            if (validation) validation.style.display = "block";

            if (submitButton) submitButton.disabled = true;
        }
        else {
            newStock.classList.remove("is-invalid");
            newStock.classList.add("is-valid");

            if (validation) validation.style.display = "none";

            if (submitButton) submitButton.disabled = false;
        }
    }
}

function openCreateStockModal() {

    const itemSelect = document.getElementById("create_ItemId");

    const warehouseSelect = document.getElementById("create_WarehouseId");

    const quantityInput = document.getElementById("create_Quantity");

    if (itemSelect) itemSelect.value = "";

    if (warehouseSelect) warehouseSelect.value = "";

    if (quantityInput) quantityInput.value = "";

    const modal = new bootstrap.Modal(document.getElementById("createStockModal"));

    modal.show();
}