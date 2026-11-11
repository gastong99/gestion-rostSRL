
// =====================
// MODAL CREATE
// =====================

function openCreateModal() {
    resetCreateForm();

    const modal = new bootstrap.Modal(document.getElementById("createUnitModal"));
    modal.show();
}

// =====================
// MODAL EDIT
// =====================

function openEditModal(unit) {
    resetEditForm();

    document.getElementById('editUnitModalTitle').textContent =
        `Editando unidad '${unit.name}'`;

    document.getElementById('edit_UnitOfMeasureId').value = unit.unitOfMeasureId;
    document.getElementById('edit_Name').value = unit.name || '';
    document.getElementById('edit_Abbreviation').value = unit.abbreviation || '';

    const form = document.getElementById('editUnitForm');
    form.action = `/UnitOfMeasures/Edit/${unit.unitOfMeasureId}`;

    const modal = new bootstrap.Modal(document.getElementById('editUnitModal'));
    modal.show();

    setTimeout(() => {
        if (typeof $.validator !== 'undefined') {
            $(form).removeData("validator");
            $(form).removeData("unobtrusiveValidation");
            $.validator.unobtrusive.parse(form);
        }
    }, 100);
}

// =====================
// MODAL DELETE
// =====================

async function openDeleteModal(unitId, name) {

    const errorDiv = document.getElementById('deleteUnitError');
    const confirmDiv = document.getElementById('deleteUnitConfirm');
    const submitBtn = document.getElementById('deleteUnitSubmit');
    const cancelText = document.getElementById('deleteUnitCancelText');
    const itemsCount = document.getElementById('deleteUnitItemsCount');
    const itemsList = document.getElementById('deleteUnitItemsList');

    // Restablecer estado del modal
    errorDiv.classList.add('d-none');
    confirmDiv.classList.remove('d-none');
    submitBtn.classList.remove('d-none');
    submitBtn.disabled = true;
    cancelText.textContent = 'Cancelar';

    itemsCount.textContent = '';
    itemsList.innerHTML = '';

    document.getElementById('deleteUnitName').textContent = name;
    document.getElementById('deleteUnitForm').action =
        `/UnitOfMeasures/DeleteConfirmed/${unitId}`;

    try {
        const response = await fetch(
            `/UnitOfMeasures/CheckUnitUsage?unitId=${unitId}`
        );

        if (!response.ok) {
            throw new Error('No se pudo verificar el uso de la unidad.');
        }

        const data = await response.json();

        if (data.isUsed && data.items && data.items.length > 0) {

            // La unidad está en uso: impedir eliminación
            errorDiv.classList.remove('d-none');
            confirmDiv.classList.add('d-none');
            submitBtn.classList.add('d-none');
            cancelText.textContent = 'Cerrar';

            itemsCount.textContent = data.itemsCount;

            data.items.forEach(itemName => {
                const li = document.createElement('li');
                li.textContent = itemName;
                itemsList.appendChild(li);
            });
        }
        else {
            // La unidad no está en uso: permitir eliminación
            errorDiv.classList.add('d-none');
            confirmDiv.classList.remove('d-none');
            submitBtn.classList.remove('d-none');
            submitBtn.disabled = false;
            cancelText.textContent = 'Cancelar';
        }
    }
    catch (error) {
        console.error('Error verificando uso de la unidad:', error);

        // Ante un error de verificación, no permitir eliminar
        errorDiv.classList.remove('d-none');
        confirmDiv.classList.add('d-none');
        submitBtn.classList.add('d-none');
        cancelText.textContent = 'Cerrar';

        errorDiv.querySelector('strong').textContent =
            'No se pudo verificar el uso de la unidad.';

        const message = document.createElement('p');
        message.className = 'mb-0 mt-2';
        message.textContent =
            'Intente nuevamente. No se habilitó la eliminación por seguridad.';

        itemsList.innerHTML = '';
        itemsList.appendChild(message);
    }

    // Mostrar modal
    const modal = new bootstrap.Modal(
        document.getElementById('deleteUnitModal')
    );

    modal.show();
}

// =====================
// FORM RESET
// =====================

function resetCreateForm() {
    const form = document.querySelector('#createUnitModal form');

    if (form) {
        form.reset();

        if (typeof $.validator !== 'undefined') {
            const validator = $(form).validate();
            if (validator) validator.resetForm();
        }

        form.querySelectorAll('.text-danger, .field-validation-error')
            .forEach(span => {
                span.textContent = '';
                span.classList.remove('field-validation-error');
                span.classList.add('field-validation-valid');
            });

        form.querySelectorAll('.is-invalid, .input-validation-error')
            .forEach(input => {
                input.classList.remove('is-invalid', 'input-validation-error');
            });
    }
}

function resetEditForm() {
    const form = document.getElementById('editUnitForm');

    if (form) {
        form.reset();

        document.getElementById('editUnitModalTitle').textContent =
            'Editar Unidad de Medida';

        if (typeof $.validator !== 'undefined') {
            const validator = $(form).validate();
            if (validator) validator.resetForm();
        }

        form.querySelectorAll('.text-danger, .field-validation-error')
            .forEach(span => {
                span.textContent = '';
                span.classList.remove('field-validation-error');
                span.classList.add('field-validation-valid');
            });

        form.querySelectorAll('.is-invalid, .input-validation-error')
            .forEach(input => {
                input.classList.remove('is-invalid', 'input-validation-error');
            });
    }
}

// =====================
// CLEANUP ON MODAL CLOSE
// =====================

document.addEventListener('DOMContentLoaded', function () {

    const createModal = document.getElementById('createUnitModal');
    const editModal = document.getElementById('editUnitModal');

    if (createModal) {
        createModal.addEventListener('hidden.bs.modal', function () {
            resetCreateForm();
        });
    }

    if (editModal) {
        editModal.addEventListener('hidden.bs.modal', function () {
            resetEditForm();
        });
    }
});
