(function() {
    "use strict";

    document.addEventListener(
    "DOMContentLoaded",
    openValidationModal
);

    // =====================
    // RESET FORMULARIOS
    // =====================

    function resetCreateForm() {

    const form = document.getElementById("createSupplierForm");

    if (!form) return;

    if (window.FormValidation) {
        FormValidation.reset(form);
    } else {
        form.reset();
    }

    // Limpiar los campos de texto.
    form.querySelectorAll(
        'input[type="text"], input[type="email"], input[type="tel"], textarea'
    ).forEach(function (field) {
        field.value = "";
    });

    // Valor predeterminado para el país.
    const country = form.querySelector("#Country");

    if (country) {
        country.value = "Argentina";
    }

    // Reiniciar el teléfono y su país.
    if (window.PhoneInput) {
        PhoneInput.reset("Phone");
    }
}

    function resetEditForm() {

        const form = document.getElementById("editSupplierForm");

        if (form && window.FormValidation) {
            FormValidation.reset(form);
        } else if (form) {
            form.reset();
        }

        if (window.PhoneInput) {
            PhoneInput.reset("edit_Phone");
        }
    }


    // =====================
    // CREATE
    // =====================

    function openCreateModal() {

        resetCreateForm();

        const modal = new bootstrap.Modal(
            document.getElementById("createSupplierModal")
        );

        modal.show();
    }

    // =====================
    // EDIT
    // =====================

    function openEditModal(supplier) {

        resetEditForm();

        document.getElementById(
            "editSupplierModalTitle"
        ).textContent = `Editando '${supplier.businessName}'`;

        document.getElementById("edit_Id").value =
            supplier.id;

        document.getElementById("edit_Code").value =
            supplier.code;

        document.getElementById("edit_BusinessName").value =
            supplier.businessName;

        document.getElementById("edit_FantasyName").value =
            supplier.fantasyName ?? "";

        document.getElementById("edit_TaxId").value =
            supplier.taxId ?? "";

        document.getElementById("edit_Email").value =
            supplier.email ?? "";

        PhoneInput.setValue(
            "edit_Phone",
            supplier.phone ?? ""
        );

        document.getElementById("edit_Address").value =
            supplier.address ?? "";

        document.getElementById("edit_City").value =
            supplier.city ?? "";

        document.getElementById("edit_Province").value =
            supplier.province ?? "";

        document.getElementById("edit_Country").value =
            supplier.country ?? "";

        document.getElementById("edit_Notes").value =
            supplier.notes ?? "";

        document.getElementById(
            "editSupplierForm"
        ).action = `/Suppliers/Edit/${supplier.id}`;

        const modal = new bootstrap.Modal(
            document.getElementById("editSupplierModal")
        );

        modal.show();
    }

    // =====================
    // DEACTIVATE
    // =====================

    function openDeactivateModal(supplier) {

        document.getElementById(
            "deactivateSupplierCode"
        ).textContent = supplier.code;

        document.getElementById(
            "deactivateSupplierBusinessName"
        ).textContent = supplier.businessName;

        document.getElementById(
            "deactivateSupplierTaxId"
        ).textContent = supplier.taxId ?? "-";

        document.getElementById(
            "deactivateSupplierForm"
        ).action = `/Suppliers/Deactivate/${supplier.id}`;

        const modal = new bootstrap.Modal(
            document.getElementById("deactivateSupplierModal")
        );

        modal.show();
    }

    // =====================
    // RESTORE
    // =====================

    function openRestoreModal(supplier) {

        document.getElementById(
            "restoreSupplierCode"
        ).textContent = supplier.code;

        document.getElementById(
            "restoreSupplierBusinessName"
        ).textContent = supplier.businessName;

        document.getElementById(
            "restoreSupplierTaxId"
        ).textContent = supplier.taxId ?? "-";

        document.getElementById(
            "restoreSupplierForm"
        ).action = `/Suppliers/Restore/${supplier.id}`;

        const modal = new bootstrap.Modal(
            document.getElementById("restoreSupplierModal")
        );

        modal.show();
    }

    // =====================
// VALIDACIÓN DEL SERVIDOR
// =====================

function openValidationModal() {

    const validationState = document.getElementById(
        "supplierValidationState"
    );

    if (!validationState) return;

    const validationModal = validationState.dataset.modal;

    if (!validationModal) return;

    const isCreate = validationModal === "create";

    const modalId = isCreate
        ? "createSupplierModal"
        : "editSupplierModal";

    const formId = isCreate
        ? "createSupplierForm"
        : "editSupplierForm";

    const modalElement = document.getElementById(modalId);
    const form = document.getElementById(formId);

    if (!modalElement || !form) return;

    // Marcar los campos que tienen errores del servidor.
    form.querySelectorAll("[data-valmsg-for]").forEach(function (message) {

        if (!message.textContent.trim()) return;

        const fieldName = message.getAttribute("data-valmsg-for");

        const field = form.querySelector(
            `[name="${CSS.escape(fieldName)}"]`
        );

        if (!field) return;

        field.classList.add("is-invalid");
        field.setAttribute("aria-invalid", "true");
    });

    // Abrir el modal sin resetear el formulario.
    bootstrap.Modal.getOrCreateInstance(modalElement).show();
}

    // =====================
    // PUBLIC FUNCTIONS
    // =====================

    window.openCreateModal = openCreateModal;
    window.openEditModal = openEditModal;
    window.openDeactivateModal = openDeactivateModal;
    window.openRestoreModal = openRestoreModal;

})();