// RESET FORMULARIOS
function resetCreateForm() {

    const form = document.querySelector("#createPurchaseOrderModal form");

    if (form) form.reset();
}

function resetEditForm() {

    const form = document.getElementById("editPurchaseOrderForm");

    if (form) form.reset();
}

// CREAR
function openCreateModal() {

    resetCreateForm();

    const modal = new bootstrap.Modal(document.getElementById("createPurchaseOrderModal"));

    modal.show();
}

// EDITAR
function openEditModal(order) {

    resetEditForm();

    document.getElementById("editPurchaseOrderModalTitle").textContent = `Editando '${order.number}'`;
    document.getElementById("edit_Id").value = order.id;
    document.getElementById("edit_Number").value = order.number;
    document.getElementById("edit_SupplierId").value = order.supplierId;
    document.getElementById("edit_OrderDate").value = order.orderDate;
    document.getElementById("edit_ExpectedDate").value = order.expectedDate ?? "";
    document.getElementById("edit_Notes").value = order.notes ?? "";
    document.getElementById("editPurchaseOrderForm").action = `/PurchaseOrders/Edit/${order.id}`;

    const modal = new bootstrap.Modal(document.getElementById("editPurchaseOrderModal"));

    modal.show();
}

// DESACTIVAR
function openDeactivateModal(order) {

    document.getElementById("deactivatePurchaseOrderNumber").textContent = order.number;
    document.getElementById("deactivatePurchaseOrderSupplier").textContent = order.supplierName;
    document.getElementById("deactivatePurchaseOrderForm").action = `/PurchaseOrders/Deactivate/${order.id}`;

    const modal = new bootstrap.Modal(document.getElementById("deactivatePurchaseOrderModal"));

    modal.show();
}

// RESTAURAR
function openRestoreModal(order) {

    document.getElementById("restorePurchaseOrderNumber").textContent = order.number;
    document.getElementById("restorePurchaseOrderSupplier").textContent = order.supplierName;
    document.getElementById("restorePurchaseOrderForm").action = `/PurchaseOrders/Restore/${order.id}`;

    const modal = new bootstrap.Modal(document.getElementById("restorePurchaseOrderModal"));

    modal.show();
}

// CONFIRMAR ORDEN
function openConfirmPurchaseOrderModal(order) {

    document.getElementById("confirmPurchaseOrderNumber").textContent = order.number;
    document.getElementById("confirmPurchaseOrderSupplier").textContent = order.supplierName;
    document.getElementById("confirmPurchaseOrderForm").action = `/PurchaseOrders/Confirm/${order.id}`;

    const modal = new bootstrap.Modal(document.getElementById("confirmPurchaseOrderModal"));

    modal.show();
}

// CANCELAR ORDEN
function openCancelPurchaseOrderModal(order) {

    document.getElementById("cancelPurchaseOrderNumber").textContent = order.number;
    document.getElementById("cancelPurchaseOrderSupplier").textContent = order.supplierName;
    document.getElementById("cancelPurchaseOrderForm").action = `/PurchaseOrders/Cancel/${order.id}`;

    const modal = new bootstrap.Modal(document.getElementById("cancelPurchaseOrderModal"));

    modal.show();
}