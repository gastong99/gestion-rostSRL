// =========================
// RESET FORMULARIOS
// =========================

function resetCreateForm() {

    const form =
        document.querySelector("#createWarehouseModal form");

    if (form)
        form.reset();

}

function resetEditForm() {

    const form =
        document.getElementById("editWarehouseForm");

    if (form)
        form.reset();

}

// =========================
// CREAR
// =========================

function openCreateModal() {

    resetCreateForm();

    const modal =
        new bootstrap.Modal(
            document.getElementById("createWarehouseModal")
        );

    modal.show();

}

// =========================
// EDITAR
// =========================

function openEditModal(warehouse) {

    resetEditForm();

    document.getElementById("editWarehouseModalTitle").textContent =
        `Editando '${warehouse.name}'`;

    document.getElementById("edit_Id").value =
        warehouse.id;

    document.getElementById("edit_Code").value =
        warehouse.code;

    document.getElementById("edit_Name").value =
        warehouse.name;

    document.getElementById("edit_Description").value =
        warehouse.description ?? "";

    document.getElementById("editWarehouseForm").action =
        `/Warehouses/Edit/${warehouse.id}`;

    const modal =
        new bootstrap.Modal(
            document.getElementById("editWarehouseModal")
        );

    modal.show();

}

// =========================
// DESACTIVAR
// =========================

function openDeactivateModal(warehouse) {

    document.getElementById("deactivateWarehouseCode").textContent =
        warehouse.code;

    document.getElementById("deactivateWarehouseName").textContent =
        warehouse.name;

    document.getElementById("deactivateWarehouseForm").action =
        `/Warehouses/Deactivate/${warehouse.id}`;

    const modal =
        new bootstrap.Modal(
            document.getElementById("deactivateWarehouseModal")
        );

    modal.show();

}

// =========================
// RESTAURAR
// =========================

function openRestoreModal(warehouse) {

    document.getElementById("restoreWarehouseCode").textContent =
        warehouse.code;

    document.getElementById("restoreWarehouseName").textContent =
        warehouse.name;

    document.getElementById("restoreWarehouseForm").action =
        `/Warehouses/Restore/${warehouse.id}`;

    const modal =
        new bootstrap.Modal(
            document.getElementById("restoreWarehouseModal")
        );

    modal.show();

}