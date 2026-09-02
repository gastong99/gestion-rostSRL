// RESET FORMULARIOS
function resetCreateItemForm() {

    const form = document.querySelector("#createPurchaseOrderItemModal form");

    if (form) form.reset();

    document.getElementById("create_Subtotal").value = "";
}

function resetEditItemForm() {

    const form = document.getElementById("editPurchaseOrderItemForm");

    if (form) form.reset();

    const subtotal = document.getElementById("edit_Subtotal");

    if (subtotal) subtotal.value = "";
}

function resetReceiveItemForm() {

    const form = document.getElementById("receivePurchaseOrderItemForm");

    if (form) form.reset();
}

// CREAR
function openCreateItemModal(purchaseOrderId) {

    resetCreateItemForm();

    document.getElementById("create_PurchaseOrderId").value = purchaseOrderId;

    calculateSubtotal();

    const modal = new bootstrap.Modal(document.getElementById("createPurchaseOrderItemModal"));

    modal.show();
}

// EDITAR
function openEditItemModal(item) {

    resetEditItemForm();

    document.getElementById("editPurchaseOrderItemModalTitle").textContent = `Editando '${item.itemName}'`;

    document.getElementById("edit_ItemId").value = item.id;

    document.getElementById("edit_PurchaseOrderId").value = item.purchaseOrderId;

    document.getElementById("edit_CatalogItemId").value = item.itemId;

    document.getElementById("edit_ItemName").value = item.itemName;

    document.getElementById("edit_Quantity").value = item.quantity;

    document.getElementById("edit_UnitPrice").value = item.unitPrice;

    calculateEditSubtotal();

    document.getElementById("editPurchaseOrderItemForm").action = `/PurchaseOrderItems/Edit/${item.id}`;

    const modal = new bootstrap.Modal(document.getElementById("editPurchaseOrderItemModal"));

    modal.show();
}

// ELIMINAR
function openDeleteItemModal(item) {

    document.getElementById("deleteItemName").textContent = item.itemName;

    document.getElementById("deleteQuantity").textContent = item.quantity;

    document.getElementById("deletePurchaseOrderItemForm").action = `/PurchaseOrderItems/Delete/${item.id}`;

    const modal = new bootstrap.Modal(document.getElementById("deletePurchaseOrderItemModal"));

    modal.show();
}

// RECIBIR MERCADERÍA
function openReceiveItemModal(item) {

    resetReceiveItemForm();

    document.getElementById("receivePurchaseOrderItemModalTitle").textContent = `Recibir '${item.itemName}'`;
    document.getElementById("receive_ItemName").value = item.itemName;
    document.getElementById("receive_OrderedQuantity").value = item.quantity;
    document.getElementById("receive_ReceivedQuantity").value = item.receivedQuantity;
    document.getElementById("receive_PendingQuantity").value = item.pendingQuantity;
    document.getElementById("receive_Quantity").value = item.pendingQuantity;
    document.getElementById("receive_Quantity").max = item.pendingQuantity;
    document.getElementById("receivePurchaseOrderItemForm").action = `/PurchaseOrderItems/Receive/${item.id}`;

    const modal = new bootstrap.Modal(document.getElementById("receivePurchaseOrderItemModal"));

    modal.show();
}

// CALCULAR SUBTOTAL
function calculateSubtotal() {

    const quantity = parseFloat(document.getElementById("create_Quantity").value) || 0;

    const unitPrice = parseFloat(document.getElementById("create_UnitPrice").value) || 0;

    document.getElementById("create_Subtotal").value = (quantity * unitPrice).toFixed(2);
}

function calculateEditSubtotal() {

    const quantity = parseFloat(document.getElementById("edit_Quantity").value) || 0;

    const unitPrice = parseFloat(document.getElementById("edit_UnitPrice").value) || 0;

    document.getElementById("edit_Subtotal").value = (quantity * unitPrice).toFixed(2);
}

// EVENTOS
document.addEventListener("DOMContentLoaded", () => {

    const createQuantity = document.getElementById("create_Quantity");

    const createUnitPrice = document.getElementById("create_UnitPrice");

    if (createQuantity) createQuantity.addEventListener("input", calculateSubtotal);

    if (createUnitPrice) createUnitPrice.addEventListener("input", calculateSubtotal);

    const editQuantity = document.getElementById("edit_Quantity");

    const editUnitPrice = document.getElementById("edit_UnitPrice");

    if (editQuantity) editQuantity.addEventListener("input", calculateEditSubtotal);

    if (editUnitPrice) editUnitPrice.addEventListener("input", calculateEditSubtotal);
});