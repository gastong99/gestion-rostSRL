// =========================
// CARGAR CATEGORÍAS
// =========================
async function loadCategories(selectId, selectedId = null) {

    const response = await fetch('/Items/GetCategoriesList');

    if (!response.ok)
        return;

    const categories = await response.json();

    const select = document.getElementById(selectId);

    if (!select)
        return;

    select.innerHTML = '<option value="">Seleccionar...</option>';

    categories.forEach(category => {

        const option = document.createElement('option');

        option.value = category.id;
        option.textContent = category.name;

        if (selectedId && category.id === selectedId)
            option.selected = true;

        select.appendChild(option);

    });

}

// =========================
// CARGAR UNIDADES
// =========================
async function loadUnitOfMeasures(selectId, selectedId = null) {

    const response = await fetch('/Items/GetUnitOfMeasuresList');

    if (!response.ok)
        return;

    const units = await response.json();

    const select = document.getElementById(selectId);

    if (!select)
        return;

    select.innerHTML = '<option value="">Seleccionar...</option>';

    units.forEach(unit => {

        const option = document.createElement('option');

        option.value = unit.unitOfMeasureId;
        option.textContent = unit.abbreviation;

        if (selectedId && unit.unitOfMeasureId === selectedId)
            option.selected = true;

        select.appendChild(option);

    });

}

function openCreateModal() {

    resetCreateForm();

    loadCategories("create_CategoryId");

    loadUnitOfMeasures("create_UnitOfMeasureId");

    const modal =
        new bootstrap.Modal(document.getElementById("createItemModal"));

    modal.show();
}
function openEditModal(item) {

    resetEditForm();

    document.getElementById("editItemModalTitle").textContent =
        `Editando '${item.name}'`;

    document.getElementById("edit_Id").value = item.id;
    document.getElementById("edit_Code").value = item.code;
    document.getElementById("edit_Name").value = item.name;
    document.getElementById("edit_Description").value = item.description ?? "";
    document.getElementById("edit_Cost").value = item.cost;
    document.getElementById("edit_Price").value = item.price;
    document.getElementById("edit_MinimumStock").value = item.minimumStock;

    loadCategories("edit_CategoryId", item.categoryId);

    loadUnitOfMeasures(
        "edit_UnitOfMeasureId",
        item.unitOfMeasureId
    );

    document.getElementById("editItemForm").action =
        `/Items/Edit/${item.id}`;

    new bootstrap.Modal(
        document.getElementById("editItemModal")
    ).show();

}
function openDeactivateModal(item) {

    document.getElementById("deactivateItemCode").textContent = item.code;

    document.getElementById("deactivateItemName").textContent = item.name;

    document.getElementById("deactivateItemForm").action =
        `/Items/Deactivate/${item.id}`;

    new bootstrap.Modal(
        document.getElementById("deactivateItemModal")
    ).show();

}
function openRestoreModal(item) {

    document.getElementById("restoreItemCode").textContent = item.code;

    document.getElementById("restoreItemName").textContent = item.name;

    document.getElementById("restoreItemForm").action =
        `/Items/Restore/${item.id}`;

    new bootstrap.Modal(
        document.getElementById("restoreItemModal")
    ).show();

}
function resetCreateForm() {

    const form =
        document.querySelector("#createItemModal form");

    if (form)
        form.reset();

}

function resetEditForm() {

    const form =
        document.getElementById("editItemForm");

    if (form)
        form.reset();

}