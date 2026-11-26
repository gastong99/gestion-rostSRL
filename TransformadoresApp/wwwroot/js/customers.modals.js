
// =====================
// HELPERS
// =====================

function getElement(id) {
    return document.getElementById(id);
}

function setValue(id, value) {
    const element = getElement(id);

    if (!element) return;

    // Delegar los campos telefónicos a intl-tel-input.
    if (
        id.endsWith("_Phone") &&
        window.PhoneInput
    ) {
        window.PhoneInput.setValue(id, value);
        return;
    }

    // Delegar los campos con máscara a IMask.
    if (
        element.dataset.mask &&
        window.FormMasks
    ) {
        window.FormMasks.setValue(id, value);
        return;
    }

    element.value = value ?? "";
}

function setRequired(id, required) {
    const element = getElement(id);

    if (element) {
        element.required = required;
    }
}


// =====================
// FORM INPUTS RESET
// =====================

function resetCustomerFormInputs(form, phoneInputId) {
    if (!form) return;

    // Actualizar las máscaras después del reset nativo.
    if (window.FormMasks) {
        window.FormMasks.reset(form);
    }

    // Reiniciar el teléfono y seleccionar Argentina.
    if (window.PhoneInput && phoneInputId) {
        window.PhoneInput.reset(phoneInputId);
    }
}


// =====================
// CREATE
// =====================

function resetCreateForm() {
    const form = getElement("createCustomerForm");

    if (form) {
        if (window.FormValidation) {
            FormValidation.reset(form);
        } else {
            form.reset();
        }

        resetCustomerFormInputs(
            form,
            "create_Phone"
        );
    }

    setValue("create_CustomerType", "1");

    toggleCreateCustomerFields();
}

function toggleCreateCustomerFields() {
    const customerType = getElement("create_CustomerType")?.value;

    const personFields = getElement("create_personFields");
    const companyFields = getElement("create_companyFields");

    const isPerson = customerType === "1";
    const isCompany = customerType === "2";

    if (personFields) {
        personFields.hidden = !isPerson;
    }

    if (companyFields) {
        companyFields.hidden = !isCompany;
    }

    setRequired("create_FirstName", isPerson);
    setRequired("create_LastName", isPerson);
    setRequired("create_BusinessName", isCompany);
}

function openCreateModal() {
    resetCreateForm();

    const modalElement = getElement("createCustomerModal");

    if (modalElement) {
        bootstrap.Modal.getOrCreateInstance(modalElement).show();
    }
}


// =====================
// EDIT
// =====================

function resetEditForm() {
    const form = getElement("editCustomerForm");

    if (form) {
        if (window.FormValidation) {
            FormValidation.reset(form);
        } else {
            form.reset();
        }

        resetCustomerFormInputs(
            form,
            "edit_Phone"
        );
    }

    toggleEditCustomerFields();
}

function toggleEditCustomerFields() {
    const customerType = getElement("edit_CustomerType")?.value;

    const personFields = getElement("edit_personFields");
    const companyFields = getElement("edit_companyFields");

    const isPerson = customerType === "1";
    const isCompany = customerType === "2";

    if (personFields) {
        personFields.hidden = !isPerson;
    }

    if (companyFields) {
        companyFields.hidden = !isCompany;
    }

    setRequired("edit_FirstName", isPerson);
    setRequired("edit_LastName", isPerson);
    setRequired("edit_BusinessName", isCompany);
}

function openEditModal(customer) {

    if (!customer) {
        console.error("No se encontraron los datos del cliente.");
        return;
    }

    resetEditForm();

    const fields = [
        "Id",
        "Code",
        "CustomerType",
        "FirstName",
        "LastName",
        "DocumentNumber",
        "BusinessName",
        "FantasyName",
        "ContactName",
        "TaxId",
        "Email",
        "Phone",
        "Address",
        "City",
        "Province",
        "Country",
        "Notes"
    ];

    fields.forEach(field => {
        const value = customer[field.charAt(0).toLowerCase() + field.slice(1)];

        setValue(`edit_${field}`, value);
    });

    toggleEditCustomerFields();

    const form = getElement("editCustomerForm");

    if (form) {
        form.action = `/Customers/Edit/${customer.id}`;
    }

    const title = getElement("editCustomerModalTitle");

    if (title) {
        title.textContent = "Editar cliente";
    }

    const modalElement = getElement("editCustomerModal");

    if (modalElement) {
        bootstrap.Modal.getOrCreateInstance(modalElement).show();
    }
}


// =====================
// DEACTIVATE
// =====================

function openDeactivateModal(id, code, name) {
    setValue("deactivateCustomerId", id);

    const codeElement = getElement("deactivateCustomerCode");
    const nameElement = getElement("deactivateCustomerName");

    if (codeElement) {
        codeElement.textContent = code ?? "";
    }

    if (nameElement) {
        nameElement.textContent = name ?? "";
    }

    const form = getElement("deactivateCustomerForm");

    if (form) {
        form.action = `/Customers/Deactivate/${id}`;
    }

    const modalElement = getElement("deactivateCustomerModal");

    if (modalElement) {
        bootstrap.Modal.getOrCreateInstance(modalElement).show();
    }
}


// =====================
// RESTORE
// =====================

function openRestoreModal(id, code, name) {
    setValue("restoreCustomerId", id);

    const codeElement = getElement("restoreCustomerCode");
    const nameElement = getElement("restoreCustomerName");

    if (codeElement) {
        codeElement.textContent = code ?? "";
    }

    if (nameElement) {
        nameElement.textContent = name ?? "";
    }

    const form = getElement("restoreCustomerForm");

    if (form) {
        form.action = `/Customers/Restore/${id}`;
    }

    const modalElement = getElement("restoreCustomerModal");

    if (modalElement) {
        bootstrap.Modal.getOrCreateInstance(modalElement).show();
    }
}


// =====================
// MODAL CLEANUP
// =====================

(function () {

    function initializeEditModalCleanup() {
        const modal = getElement("editCustomerModal");

        if (!modal || modal.dataset.cleanupInitialized === "true") {
            return;
        }

        modal.dataset.cleanupInitialized = "true";

        modal.addEventListener("hidden.bs.modal", function () {
            resetEditForm();
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener(
            "DOMContentLoaded",
            initializeEditModalCleanup
        );
    } else {
        initializeEditModalCleanup();
    }

})();

// =====================
// SERVER VALIDATION
// =====================

function openCustomerValidationModal() {
    const validationState = getElement("customerValidationState");

    if (!validationState) {
        return;
    }

    const modalType = validationState.dataset.modal;

    if (modalType !== "create" && modalType !== "edit") {
        return;
    }

    const isCreate = modalType === "create";

    const modalId = isCreate
        ? "createCustomerModal"
        : "editCustomerModal";

    const formId = isCreate
        ? "createCustomerForm"
        : "editCustomerForm";

    const modalElement = getElement(modalId);
    const form = getElement(formId);

    if (!modalElement || !form) {
        return;
    }

    // Mostrar los campos correspondientes al tipo de cliente.
    if (isCreate) {
        toggleCreateCustomerFields();
    } else {
        toggleEditCustomerFields();

        const customerId = getElement("edit_Id")?.value;

        if (customerId) {
            form.action = `/Customers/Edit/${customerId}`;
        }
    }

    // Marcar los campos que tienen errores del servidor.
    const validationMessages = form.querySelectorAll(
        "[data-valmsg-for]"
    );

    validationMessages.forEach(function (messageElement) {
        const fieldName = messageElement.dataset.valmsgFor;

        if (!fieldName || !messageElement.textContent.trim()) {
            return;
        }

        const field = Array.from(form.elements).find(function (element) {
            return element.name === fieldName;
        });

        if (field) {
            field.classList.add("is-invalid");
            field.setAttribute("aria-invalid", "true");
        }
    });

    bootstrap.Modal
        .getOrCreateInstance(modalElement)
        .show();
}

if (document.readyState === "loading") {
    document.addEventListener(
        "DOMContentLoaded",
        openCustomerValidationModal
    );
} else {
    openCustomerValidationModal();
}