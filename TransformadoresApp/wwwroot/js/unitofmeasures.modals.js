// ===== MODAL CREATE =====
function openCreateModal() {
    resetCreateForm();

    const modal = new bootstrap.Modal(
        document.getElementById("createUnitModal")
    );
    modal.show();
}

// ===== MODAL EDIT =====
function openEditModal(unit) {
    resetEditForm();

    document.getElementById('editUnitModalTitle').textContent =
        `Editando unidad '${unit.name}'`;

    document.getElementById('edit_UnitOfMeasureId').value = unit.unitOfMeasureId;
    document.getElementById('edit_Name').value = unit.name || '';
    document.getElementById('edit_Abbreviation').value = unit.abbreviation || '';

    const form = document.getElementById('editUnitForm');
    form.action = `/UnitOfMeasures/Edit/${unit.unitOfMeasureId}`;

    const modal = new bootstrap.Modal(
        document.getElementById('editUnitModal')
    );
    modal.show();

    setTimeout(() => {
        if (typeof $.validator !== 'undefined') {
            $(form).removeData("validator");
            $(form).removeData("unobtrusiveValidation");
            $.validator.unobtrusive.parse(form);
        }
    }, 100);
}

// ===== MODAL DELETE =====
async function openDeleteModal(unitId, name) {
    // Establecer nombre
    document.getElementById('deleteUnitName').textContent = name;

    // Configurar acción del formulario
    document.getElementById('deleteUnitForm').action = `/UnitOfMeasures/DeleteConfirmed/${unitId}`;

    // Verificar si la unidad está en uso
    try {
        const response = await fetch(`/UnitOfMeasures/CheckUnitUsage?unitId=${unitId}`);
        if (response.ok) {
            const data = await response.json();

            const errorDiv = document.getElementById('deleteUnitError');
            const confirmDiv = document.getElementById('deleteUnitConfirm');
            const submitBtn = document.getElementById('deleteUnitSubmit');
            const cancelText = document.getElementById('deleteUnitCancelText');
            const materialsCount = document.getElementById('deleteUnitMaterialsCount');
            const materialsList = document.getElementById('deleteUnitMaterialsList');

            if (data.isUsed && data.materials && data.materials.length > 0) {
                // Unidad está en uso - mostrar error
                errorDiv.classList.remove('d-none');
                confirmDiv.classList.add('d-none');
                submitBtn.classList.add('d-none');
                cancelText.textContent = 'Cerrar';

                // Mostrar cantidad
                materialsCount.textContent = data.materialsCount;

                // Llenar lista de materiales
                materialsList.innerHTML = '';
                data.materials.forEach(materialName => {
                    const li = document.createElement('li');
                    li.textContent = materialName;
                    materialsList.appendChild(li);
                });
            } else {
                // Unidad NO está en uso - permitir eliminar
                errorDiv.classList.add('d-none');
                confirmDiv.classList.remove('d-none');
                submitBtn.classList.remove('d-none');
                cancelText.textContent = 'Cancelar';
            }
        }
    } catch (error) {
        console.error('Error verificando uso de la unidad:', error);
    }

    // Mostrar modal
    const modal = new bootstrap.Modal(
        document.getElementById('deleteUnitModal')
    );
    modal.show();
}

// ===== FUNCIONES DE LIMPIEZA =====
function resetCreateForm() {
    const form = document.querySelector('#createUnitModal form');
    if (form) {
        form.reset();

        // Limpiar validación jQuery
        if (typeof $.validator !== 'undefined') {
            const validator = $(form).validate();
            if (validator) validator.resetForm();
        }

        // Limpiar mensajes de error
        form.querySelectorAll('.text-danger, .field-validation-error').forEach(span => {
            span.textContent = '';
            span.classList.remove('field-validation-error');
            span.classList.add('field-validation-valid');
        });

        // Limpiar clases de validación
        form.querySelectorAll('.is-invalid, .input-validation-error').forEach(input => {
            input.classList.remove('is-invalid', 'input-validation-error');
        });
    }
}

function resetEditForm() {
    const form = document.getElementById('editUnitForm');
    if (form) {
        form.reset();

        // Restaurar título por defecto
        document.getElementById('editUnitModalTitle').textContent = 'Editar Unidad de Medida';

        // Limpiar validación jQuery
        if (typeof $.validator !== 'undefined') {
            const validator = $(form).validate();
            if (validator) validator.resetForm();
        }

        // Limpiar mensajes de error
        form.querySelectorAll('.text-danger, .field-validation-error').forEach(span => {
            span.textContent = '';
            span.classList.remove('field-validation-error');
            span.classList.add('field-validation-valid');
        });

        // Limpiar clases de validación
        form.querySelectorAll('.is-invalid, .input-validation-error').forEach(input => {
            input.classList.remove('is-invalid', 'input-validation-error');
        });
    }
}

// ===== LIMPIAR AL CERRAR MODALES =====
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