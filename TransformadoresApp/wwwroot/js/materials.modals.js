// CARGAR UNIDADES DE MEDIDA
async function loadUnitOfMeasures(selectId, selectedUnitId = null) {
    try
    {
        const response = await fetch('/Materials/GetUnitOfMeasuresList');
        if (!response.ok) throw new Error('Error al cargar unidades de medida');

        const units = await response.json();
        const select = document.getElementById(selectId);

        if (!select) return;

        // Limpiar opciones actuales excepto la primera
        select.innerHTML = '<option value="">Seleccionar...</option>';

        // Agregar unidades de medida
        units.forEach(unit => {
            const option = document.createElement('option');
            option.value = unit.unitOfMeasureId;
            option.textContent = unit.abbreviation;
            if (selectedUnitId && unit.unitOfMeasureId === selectedUnitId) {
                option.selected = true;
            }
            select.appendChild(option);
        });
    }
    catch (error) {
        console.error('Error cargando unidades de medida:', error);
    }
}

// MODAL CREATE
function openCreateModal() {
    resetCreateForm();

    // Cargar unidades de medida
    loadUnitOfMeasures('create_UnitOfMeasureId');

    const modal = new bootstrap.Modal(document.getElementById("createMaterialModal"));
    modal.show();
}

// MODAL EDIT
function openEditModal(material) {
    resetEditForm();

    // Establecer título
    document.getElementById('editMaterialModalTitle').textContent = `Editando material '${material.name}'`;

    // Establecer valores
    document.getElementById('edit_MaterialId').value = material.materialId;
    document.getElementById('edit_Code').value = material.code || '';
    document.getElementById('edit_Name').value = material.name || '';

    // Cargar unidades de medida y seleccionar la actual
    loadUnitOfMeasures('edit_UnitOfMeasureId', material.unitOfMeasureId);

    // Configurar acción del formulario
    const form = document.getElementById('editMaterialForm');
    form.action = `/Materials/Edit/${material.materialId}`;

    // Mostrar modal
    const modal = new bootstrap.Modal(document.getElementById('editMaterialModal'));
    modal.show();

    // Inicializar validación jQuery
    setTimeout(() => {
        if (typeof $.validator !== 'undefined') {
            $(form).removeData("validator");
            $(form).removeData("unobtrusiveValidation");
            $.validator.unobtrusive.parse(form);
        }
    }, 100);
}

// MODAL DELETE
async function openDeleteModal(materialId, code, name, unit) {
    // Establecer datos básicos
    document.getElementById('deleteMaterialCode').textContent = code;
    document.getElementById('deleteMaterialName').textContent = name;
    document.getElementById('deleteMaterialUnit').textContent = unit;

    // Configurar acción del formulario
    document.getElementById('deleteMaterialForm').action = `/Materials/DeleteConfirmed/${materialId}`;

    // Verificar si el material está en uso
    try
    {
        const response = await fetch(`/Materials/CheckMaterialUsage?materialId=${materialId}`);
        if (response.ok) {
            const data = await response.json();

            const errorDiv = document.getElementById('deleteMaterialError');
            const confirmDiv = document.getElementById('deleteMaterialConfirm');
            const submitBtn = document.getElementById('deleteMaterialSubmit');
            const cancelText = document.getElementById('deleteMaterialCancelText');
            const usedInList = document.getElementById('deleteMaterialUsedIn');

            if (data.isUsed && data.usedIn && data.usedIn.length > 0) {
                // Material está en uso - mostrar error
                errorDiv.classList.remove('d-none');
                confirmDiv.classList.add('d-none');
                submitBtn.classList.add('d-none');
                cancelText.textContent = 'Cerrar';

                // Llenar lista de transformadores
                usedInList.innerHTML = '';
                data.usedIn.forEach(productName => {
                    const li = document.createElement('li');
                    li.textContent = productName;
                    usedInList.appendChild(li);
                });
            }
            else {
                // Material NO está en uso - permitir eliminar
                errorDiv.classList.add('d-none');
                confirmDiv.classList.remove('d-none');
                submitBtn.classList.remove('d-none');
                cancelText.textContent = 'Cancelar';
            }
        }
    }
    catch (error) {
        console.error('Error verificando uso del material:', error);
    }

    // Mostrar modal
    const modal = new bootstrap.Modal(document.getElementById('deleteMaterialModal'));
    modal.show();
}

// FUNCIONES DE LIMPIEZA
function resetCreateForm() {
    const form = document.querySelector('#createMaterialModal form');
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
    const form = document.getElementById('editMaterialForm');
    if (form) {
        form.reset();

        // Restaurar título por defecto
        document.getElementById('editMaterialModalTitle').textContent = 'Editar Material';

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

// LIMPIAR AL CERRAR MODALES
document.addEventListener('DOMContentLoaded', function () {
    const createModal = document.getElementById('createMaterialModal');
    const editModal = document.getElementById('editMaterialModal');

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