// RESET FORMULARIOS
function resetCreateForm() {

    const form = document.querySelector("#createSupplierModal form");

    if (form) form.reset();
}

function resetEditForm() {

    const form = document.getElementById("editSupplierForm");

    if (form) form.reset();
}

// CREAR
function openCreateModal() {

    resetCreateForm();

    const modal = new bootstrap.Modal(document.getElementById("createSupplierModal"));
    modal.show();
}

// EDITAR
function openEditModal(supplier) {

    resetEditForm();

    document.getElementById("editSupplierModalTitle").textContent = `Editando '${supplier.businessName}'`;
    document.getElementById("edit_Id").value = supplier.id;
    document.getElementById("edit_Code").value = supplier.code;
    document.getElementById("edit_BusinessName").value = supplier.businessName;
    document.getElementById("edit_FantasyName").value = supplier.fantasyName ?? "";
    document.getElementById("edit_TaxId").value = supplier.taxId ?? "";
    document.getElementById("edit_Email").value = supplier.email ?? "";
    document.getElementById("edit_Phone").value = supplier.phone ?? "";
    document.getElementById("edit_Address").value = supplier.address ?? "";
    document.getElementById("edit_City").value = supplier.city ?? "";
    document.getElementById("edit_Province").value = supplier.province ?? "";
    document.getElementById("edit_Country").value = supplier.country ?? "";
    document.getElementById("edit_Notes").value = supplier.notes ?? "";
    document.getElementById("editSupplierForm").action = `/Suppliers/Edit/${supplier.id}`;

    const modal = new bootstrap.Modal(document.getElementById("editSupplierModal"));

    modal.show();
}

// DESACTIVAR
function openDeactivateModal(supplier) {

    document.getElementById("deactivateSupplierCode").textContent = supplier.code;
    document.getElementById("deactivateSupplierBusinessName").textContent = supplier.businessName;
    document.getElementById("deactivateSupplierTaxId").textContent = supplier.taxId ?? "-";
    document.getElementById("deactivateSupplierForm").action = `/Suppliers/Deactivate/${supplier.id}`;

    const modal = new bootstrap.Modal(document.getElementById("deactivateSupplierModal"));

    modal.show();
}

// RESTAURAR
function openRestoreModal(supplier) {

    document.getElementById("restoreSupplierCode").textContent = supplier.code;
    document.getElementById("restoreSupplierBusinessName").textContent = supplier.businessName;
    document.getElementById("restoreSupplierTaxId").textContent = supplier.taxId ?? "-";
    document.getElementById("restoreSupplierForm").action = `/Suppliers/Restore/${supplier.id}`;

    const modal = new bootstrap.Modal(document.getElementById("restoreSupplierModal"));

    modal.show();
}