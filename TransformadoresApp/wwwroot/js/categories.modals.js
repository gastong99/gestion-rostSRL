// ===== CARGAR CATEGORÍAS =====
async function loadCategories(selectId, selectedCategoryId = null) {
    try {
        const response = await fetch('/Categories/GetCategoriesList');

        if (!response.ok)
            throw new Error('Error al cargar categorías');

        const categories = await response.json();

        const select = document.getElementById(selectId);

        if (!select) return;

        // Limpiar opciones
        select.innerHTML =
            '<option value="">Sin categoría padre</option>';

        categories.forEach(category => {

            const option = document.createElement('option');

            option.value = category.id;
            option.textContent = category.name;

            if (
                selectedCategoryId &&
                category.id === selectedCategoryId
            ) {
                option.selected = true;
            }

            select.appendChild(option);
        });

    } catch (error) {
        console.error(
            'Error cargando categorías:',
            error
        );
    }
}

// ===== MODAL CREATE =====
function openCreateModal() {

    resetCreateForm();

    loadCategories('create_ParentId');

    const modal = new bootstrap.Modal(
        document.getElementById('createCategoryModal')
    );

    modal.show();
}

// ===== MODAL EDIT =====
function openEditModal(category) {

    resetEditForm();

    document.getElementById(
        'editCategoryModalTitle'
    ).textContent =
        `Editando categoría '${category.name}'`;

    document.getElementById(
        'edit_Id'
    ).value = category.id;

    document.getElementById(
        'edit_Name'
    ).value = category.name || '';

    loadCategories(
        'edit_ParentId',
        category.parentId
    );

    const form =
        document.getElementById(
            'editCategoryForm'
        );

    form.action =
        `/Categories/Edit/${category.id}`;

    const modal = new bootstrap.Modal(
        document.getElementById(
            'editCategoryModal'
        )
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
function openDeleteModal(
    categoryId,
    name,
    parentName
) {

    document.getElementById(
        'deleteCategoryName'
    ).textContent = name;

    document.getElementById(
        'deleteCategoryParent'
    ).textContent =
        parentName || '-';

    document.getElementById(
        'deleteCategoryForm'
    ).action =
        `/Categories/DeleteConfirmed/${categoryId}`;

    document.getElementById(
        'deleteCategoryError'
    ).classList.add('d-none');

    document.getElementById(
        'deleteCategoryConfirm'
    ).classList.remove('d-none');

    document.getElementById(
        'deleteCategorySubmit'
    ).classList.remove('d-none');

    document.getElementById(
        'deleteCategoryCancelText'
    ).textContent = 'Cancelar';

    const modal = new bootstrap.Modal(
        document.getElementById(
            'deleteCategoryModal'
        )
    );

    modal.show();
}

// ===== LIMPIAR CREATE =====
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

        if (validator)
            validator.resetForm();
    }

    form.querySelectorAll(
        '.text-danger, .field-validation-error'
    ).forEach(span => {

        span.textContent = '';

        span.classList.remove(
            'field-validation-error'
        );

        span.classList.add(
            'field-validation-valid'
        );
    });

    form.querySelectorAll(
        '.is-invalid, .input-validation-error'
    ).forEach(input => {

        input.classList.remove(
            'is-invalid',
            'input-validation-error'
        );
    });
}

// ===== LIMPIAR EDIT =====
function resetEditForm() {

    const form =
        document.getElementById(
            'editCategoryForm'
        );

    if (!form) return;

    form.reset();

    document.getElementById(
        'editCategoryModalTitle'
    ).textContent =
        'Editar Categoría';

    if (typeof $.validator !== 'undefined') {

        const validator =
            $(form).validate();

        if (validator)
            validator.resetForm();
    }

    form.querySelectorAll(
        '.text-danger, .field-validation-error'
    ).forEach(span => {

        span.textContent = '';

        span.classList.remove(
            'field-validation-error'
        );

        span.classList.add(
            'field-validation-valid'
        );
    });

    form.querySelectorAll(
        '.is-invalid, .input-validation-error'
    ).forEach(input => {

        input.classList.remove(
            'is-invalid',
            'input-validation-error'
        );
    });
}

// ===== LIMPIAR AL CERRAR =====
document.addEventListener(
    'DOMContentLoaded',
    function () {

        const createModal =
            document.getElementById(
                'createCategoryModal'
            );

        const editModal =
            document.getElementById(
                'editCategoryModal'
            );

        if (createModal) {

            createModal.addEventListener(
                'hidden.bs.modal',
                function () {
                    resetCreateForm();
                }
            );
        }

        if (editModal) {

            editModal.addEventListener(
                'hidden.bs.modal',
                function () {
                    resetEditForm();
                }
            );
        }
    }
);
