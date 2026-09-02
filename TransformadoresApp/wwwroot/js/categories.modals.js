// CARGAR CATEGORÍAS
async function loadCategories(selectId, selectedCategoryId = null, excludeId = null) {
    try
    {
        let url = "/Categories/GetCategoriesList";

        if (excludeId !== null) url += `?excludeId=${excludeId}`;

        const response = await fetch(url);

        // Excluir la categoría actual
        if (currentCategoryId) {
            categories = categories.filter(c => c.id !== currentCategoryId);
        }

        // Mostrar sólo categorías activas
        categories = categories.filter(c => c.isActive);

        // Filtrar por tipo (si se especificó)
        if (itemType !== null && itemType !== undefined) {
            categories = categories.filter(c => c.itemType === itemType);
        }

        const select = document.getElementById(selectId);

        if (!select) return;

        select.innerHTML =
            '<option value="">Sin categoría padre</option>';

        categories.forEach(category => {

            const option = document.createElement('option');

            option.value = category.id;
            option.textContent = category.name;

            if (selectedCategoryId === category.id) {
                option.selected = true;
            }

            select.appendChild(option);
        });

    }
    catch (error) {
        console.error('Error cargando categorías:', error);
    }
}

// CARGAR TIPOS DE ITEM
async function loadItemTypes(selectId, selectedId = null) {

    try
    {
        const response = await fetch('/Categories/GetItemTypesList');

        if (!response.ok) throw new Error('No se pudieron cargar los tipos.');

        const itemTypes = await response.json();

        const select = document.getElementById(selectId);

        if (!select) return;

        select.innerHTML =
            '<option value="">Seleccionar...</option>';

        itemTypes.forEach(type => {

            const option = document.createElement('option');

            option.value = type.id;
            option.textContent = type.name;

            if (selectedId === type.id) {
                option.selected = true;
            }

            select.appendChild(option);
        });
    }
    catch (error) {
        console.error('Error cargando tipos:', error);
    }
}

// MODAL CREATE
async function openCreateModal() {

    resetCreateForm();

    await loadItemTypes('create_ItemType');

    await loadCategories('create_ParentId');

    const modal =
        new bootstrap.Modal(
            document.getElementById('createCategoryModal')
        );

    modal.show();
}

// MODAL EDIT
async function openEditModal(category) {

    resetEditForm();

    document.getElementById('editCategoryModalTitle').textContent = `Editando categoría '${category.name}'`;

    document.getElementById('edit_Id').value = category.id;

    document.getElementById('edit_Name').value = category.name;

    await loadItemTypes('edit_ItemType', category.itemType);

    await loadCategories("edit_ParentId", category.parentId, category.id);

    document.getElementById('editCategoryForm').action = `/Categories/Edit/${category.id}`;

    const modal =
        new bootstrap.Modal(
            document.getElementById(
                'editCategoryModal'
            )
        );

    modal.show();

    setTimeout(() => {

        if (typeof $.validator !== 'undefined') {

            const form = document.getElementById('editCategoryForm');

            $(form).removeData('validator');
            $(form).removeData('unobtrusiveValidation');

            $.validator.unobtrusive.parse(form);
        }
    }, 100);
}

// MODAL DELETE
function openDeleteModal(categoryId, name, parentName) {

    document.getElementById('deleteCategoryName').textContent = name;

    document.getElementById('deleteCategoryParent').textContent = parentName || '-';

    document.getElementById('deleteCategoryForm').action = `/Categories/DeleteConfirmed/${categoryId}`;

    document.getElementById('deleteCategoryError').classList.add('d-none');

    document.getElementById('deleteCategoryConfirm').classList.remove('d-none');

    document.getElementById('deleteCategorySubmit').classList.remove('d-none');

    document.getElementById('deleteCategoryCancelText').textContent = 'Cancelar';

    const modal =
        new bootstrap.Modal(
            document.getElementById(
                'deleteCategoryModal'
            )
        );

    modal.show();
}

// MODAL RESTORE
function openRestoreModal(categoryId, name) {

    document.getElementById("restoreCategoryName").textContent = name;

    document.getElementById("restoreCategoryForm").action = `/Categories/Restore/${categoryId}`;

    const modal = new bootstrap.Modal(
        document.getElementById("restoreCategoryModal")
    );

    modal.show();
}

// RESET CREATE
function resetCreateForm() {

    const form =
        document.querySelector(
            '#createCategoryModal form'
        );

    if (!form) return;

    form.reset();

    if (typeof $.validator !== 'undefined') {

        const validator =
            $(form).validate();

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

// RESET EDIT
function resetEditForm() {

    const form = document.getElementById('editCategoryForm');

    if (!form) return;

    form.reset();

    document.getElementById('editCategoryModalTitle').textContent = 'Editar Categoría';

    if (typeof $.validator !== 'undefined') {

        const validator =
            $(form).validate();

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
            input.classList.remove(
                'is-invalid',
                'input-validation-error'
            );
        });
}

// LIMPIAR AL CERRAR MODALES
document.addEventListener('DOMContentLoaded',
    function () {

        const createModal = document.getElementById('createCategoryModal');

        const editModal = document.getElementById('editCategoryModal');

        if (createModal) {

            createModal.addEventListener('hidden.bs.modal',
                function () {
                    resetCreateForm();
                }
            );
        }

        if (editModal) {

            editModal.addEventListener('hidden.bs.modal',
                function () {
                    resetEditForm();
                }
            );
        }
    }
);