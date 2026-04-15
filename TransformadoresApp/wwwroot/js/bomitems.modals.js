// ===== CARGAR MATERIALES =====
async function loadMaterials(selectId, selectedMaterialId = null) {
    try {
        const response = await fetch('/BomItems/GetMaterialsList');
        if (!response.ok) throw new Error('Error al cargar materiales');

        const materials = await response.json();
        const select = document.getElementById(selectId);

        if (!select) return;

        // Limpiar opciones actuales excepto la primera
        select.innerHTML = '<option value="">Seleccionar...</option>';

        // Agregar materiales
        materials.forEach(material => {
            const option = document.createElement('option');
            option.value = material.materialId;
            option.textContent = material.name;
            if (selectedMaterialId && material.materialId === selectedMaterialId) {
                option.selected = true;
            }
            select.appendChild(option);
        });

        // Si hay un material seleccionado, cargar su unidad
        if (selectedMaterialId) {
            await loadUnitOfMeasure(selectedMaterialId, selectId.replace('MaterialId', 'UnitDisplay'));
        }
    } catch (error) {
        console.error('Error cargando materiales:', error);
    }
}

// ===== CARGAR UNIDAD DE MEDIDA =====
async function loadUnitOfMeasure(materialId, unitDisplayId) {
    const unitDisplay = document.getElementById(unitDisplayId);
    if (!unitDisplay || !materialId) {
        if (unitDisplay) unitDisplay.value = '';
        return;
    }

    try {
        const response = await fetch(`/BomItems/GetUnitOfMeasure?materialId=${materialId}`);
        if (response.ok) {
            const data = await response.json();
            unitDisplay.value = data.unit || '';
        } else {
            unitDisplay.value = 'No disponible';
        }
    } catch (error) {
        console.error('Error cargando unidad:', error);
        unitDisplay.value = 'Error al cargar';
    }
}

// ===== MODAL CREATE =====
function openCreateBomModal(productId, productName) {
    resetCreateBomForm();

    // Establecer título
    document.getElementById('createBomModalTitle').textContent =
        `Agregar material para "${productName}"`;

    // Establecer ProductId
    document.getElementById('create_ProductId').value = productId;

    // Cargar lista de materiales
    loadMaterials('create_MaterialId');

    // Configurar evento change para el select de materiales
    const materialSelect = document.getElementById('create_MaterialId');
    materialSelect.addEventListener('change', function () {
        loadUnitOfMeasure(this.value, 'create_UnitDisplay');
    });

    // Mostrar modal
    const modal = new bootstrap.Modal(document.getElementById('createBomItemModal'));
    modal.show();
}

// ===== MODAL EDIT =====
function openEditBomModal(bomItem) {
    resetEditBomForm();

    // Establecer título
    document.getElementById('editBomModalTitle').textContent =
        `Modificar material "${bomItem.material?.name || ''}"`;

    // Establecer valores
    document.getElementById('edit_BomItemId').value = bomItem.bomItemId;
    document.getElementById('edit_ProductId').value = bomItem.productId;
    document.getElementById('edit_QuantityPerUnit').value = bomItem.quantityPerUnit || '';

    // Cargar materiales y seleccionar el actual
    loadMaterials('edit_MaterialId', bomItem.materialId);

    // Configurar acción del formulario
    const form = document.getElementById('editBomItemForm');
    form.action = `/BomItems/Edit/${bomItem.bomItemId}`;

    // Configurar evento change para el select de materiales
    const materialSelect = document.getElementById('edit_MaterialId');
    materialSelect.addEventListener('change', function () {
        loadUnitOfMeasure(this.value, 'edit_UnitDisplay');
    });

    // Mostrar modal
    const modal = new bootstrap.Modal(document.getElementById('editBomItemModal'));
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

// ===== MODAL DELETE =====
function openDeleteBomModal(bomItemId, materialName, quantity, unit) {
    document.getElementById('deleteBomMaterialName').textContent = materialName;
    document.getElementById('deleteBomQuantity').textContent = quantity;
    document.getElementById('deleteBomUnit').textContent = unit;
    document.getElementById('deleteBomItemForm').action = `/BomItems/Delete/${bomItemId}`;

    const modal = new bootstrap.Modal(document.getElementById('deleteBomItemModal'));
    modal.show();
}

// ===== FUNCIONES DE LIMPIEZA =====
function resetCreateBomForm() {
    const form = document.getElementById('createBomItemForm');
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

        // Limpiar unidad de medida
        document.getElementById('create_UnitDisplay').value = '';
    }
}

function resetEditBomForm() {
    const form = document.getElementById('editBomItemForm');
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

        // Limpiar unidad de medida
        document.getElementById('edit_UnitDisplay').value = '';
    }
}

// ===== LIMPIAR AL CERRAR MODALES =====
document.addEventListener('DOMContentLoaded', function () {
    const createModal = document.getElementById('createBomItemModal');
    const editModal = document.getElementById('editBomItemModal');

    if (createModal) {
        createModal.addEventListener('hidden.bs.modal', function () {
            resetCreateBomForm();
        });
    }

    if (editModal) {
        editModal.addEventListener('hidden.bs.modal', function () {
            resetEditBomForm();
        });
    }
});