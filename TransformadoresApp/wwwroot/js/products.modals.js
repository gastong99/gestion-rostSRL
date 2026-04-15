function openDeleteModal(id, name) {
    document.getElementById('deleteProductName').innerText = name;
    document.getElementById('deleteProductForm').action = `/Products/DeleteConfirmed/${id}`;
    const modal = new bootstrap.Modal(
        document.getElementById('deleteProductModal')
    );
    modal.show();
}

function openCreateModal() {
    resetCreateForm();

    const modal = new bootstrap.Modal(
        document.getElementById("createProductModal")
    );
    modal.show();
}

function openEditModal(product) {
    resetEditForm();

    document.getElementById("editModalTitle").textContent = `Modificando '${product.name}'`;

    document.getElementById("edit_id_route").value = product.productId;
    document.getElementById("edit_ProductId").value = product.productId;
    document.getElementById("edit_Code").value = product.code || "";
    document.getElementById("edit_Name").value = product.name || "";
    document.getElementById("edit_PotenciaKVA").value = product.potenciaKVA !== null ? product.potenciaKVA : "";
    document.getElementById("edit_Ucc").value = product.ucc !== null ? product.ucc : "";
    document.getElementById("edit_PerdidasPo").value = product.perdidasPo !== null ? product.perdidasPo : "";
    document.getElementById("edit_PerdidasPcc").value = product.perdidasPcc !== null ? product.perdidasPcc : "";
    document.getElementById("edit_Largo").value = product.largo !== null ? product.largo : "";
    document.getElementById("edit_Ancho").value = product.ancho !== null ? product.ancho : "";
    document.getElementById("edit_Alto").value = product.alto !== null ? product.alto : "";
    document.getElementById("edit_Diametro").value = product.diametro !== null ? product.diametro : "";
    document.getElementById("edit_Peso").value = product.peso !== null ? product.peso : "";

    const form = document.getElementById("editProductForm");
    form.action = `/Products/Edit/${product.productId}`;

    const modal = new bootstrap.Modal(
        document.getElementById("editProductModal")
    );
    modal.show();

    // Inicializar validación jQuery después de mostrar el modal
    setTimeout(() => {
        if (typeof $.validator !== 'undefined') {
            $(form).removeData("validator");
            $(form).removeData("unobtrusiveValidation");
            $.validator.unobtrusive.parse(form);
        }
    }, 100);
}

function resetCreateForm() {
    const form = document.querySelector('#createProductModal form');
    if (form) {
        form.reset();

        // Limpiar validación jQuery
        if (typeof $.validator !== 'undefined') {
            $(form).validate().resetForm();
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
    const form = document.getElementById("editProductForm");
    if (form) {
        form.reset();

        document.getElementById("editModalTitle").textContent = "Modificar Transformador";

        // Limpiar validación jQuery
        if (typeof $.validator !== 'undefined') {
            const validator = $(form).validate();
            if (validator) {
                validator.resetForm();
            }
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

// Limpiar formularios al cerrar modales
document.addEventListener('DOMContentLoaded', function () {
    const createModal = document.getElementById('createProductModal');
    const editModal = document.getElementById('editProductModal');

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