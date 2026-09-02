// VARIABLES GLOBALES
let currentOrderData = null;

// CARGAR PRODUCTOS
async function loadProducts(selectId, selectedProductId = null) {
    try
    {
        const response = await fetch('/ProductionOrders/GetProductsList');
        if (!response.ok) throw new Error('Error al cargar productos');

        const products = await response.json();
        const select = document.getElementById(selectId);

        if (!select) return;

        select.innerHTML = '<option value="">Seleccionar...</option>';

        products.forEach(product => {
            const option = document.createElement('option');
            option.value = product.productId;
            option.textContent = product.name;
            if (selectedProductId && product.productId === selectedProductId) {
                option.selected = true;
            }
            select.appendChild(option);
        });
    }
    catch (error) {
        console.error('Error cargando productos:', error);
    }
}

// PREVISUALIZAR BOM
async function previewBom(productId, quantity, contentElementId) {
    if (!productId || !quantity || quantity <= 0) {
        alert('Seleccione un producto y una cantidad válida.');
        return;
    }

    try
    {
        const response = await fetch(`/ProductionOrders/PreviewBom?productId=${productId}&quantity=${quantity}`);
        if (!response.ok) throw new Error('Error al cargar materiales');

        const html = await response.text();
        document.getElementById(contentElementId).innerHTML = html;
        document.getElementById('previewBomArea').style.display = 'block';
    }
    catch (error) {
        console.error('Error cargando BOM:', error);
        alert('No se pudo cargar la lista de materiales');
    }
}

// CAMBIAR ESTADO (AJAX)
async function changeOrderStatus(orderId, newStatus, rowElement) {
    try
    {
        const response = await fetch('/ProductionOrders/UpdateStatus', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: `id=${orderId}&newStatus=${encodeURIComponent(newStatus)}`
        });

        const result = await response.json();

        if (result.success) {
            // Actualizar el data-order con el nuevo estado
            const orderDataStr = rowElement.getAttribute('data-order');
            if (orderDataStr) {
                const orderData = JSON.parse(orderDataStr);
                orderData.status = newStatus;
                rowElement.setAttribute('data-order', JSON.stringify(orderData));
            }

            // Actualizar dropdown con nuevas opciones válidas
            updateStatusDropdown(rowElement, newStatus);

            showToast('success', result.message);
        }
        else {
            showToast('error', result.message);
            // Revertir el dropdown
            const select = rowElement.querySelector('.status-dropdown');
            if (select) {
                select.value = select.getAttribute('data-original-status');
            }
        }
    }
    catch (error) {
        console.error('Error cambiando estado:', error);
        showToast('error', 'Error al cambiar el estado');
    }
}

// Actualizar dropdown con nuevas opciones según el estado 
function updateStatusDropdown(rowElement, newStatus) {
    const select = rowElement.querySelector('.status-dropdown');
    if (!select) return;

    // Actualizar data-original-status
    select.setAttribute('data-original-status', newStatus);

    // Limpiar opciones actuales
    select.innerHTML = '';

    // Agregar nuevas opciones según el nuevo estado
    const options = [];

    switch (newStatus) {
        case 'Pendiente':
            options.push(
                { value: 'Pendiente', text: 'Pendiente', selected: true },
                { value: 'EnProceso', text: 'En Proceso', selected: false },
                { value: 'Cancelada', text: 'Cancelada', selected: false }
            );
            break;
        case 'EnProceso':
        case 'En Proceso':
            options.push(
                { value: 'EnProceso', text: 'En Proceso', selected: true },
                { value: 'Completada', text: 'Completada', selected: false },
                { value: 'Cancelada', text: 'Cancelada', selected: false }
            );
            break;
        case 'Completada':
        case 'Cancelada':
            // Si llega a estado final, reemplazar dropdown con badge
            const statusText = newStatus === 'Completada' ? 'Completada' : 'Cancelada';
            const badgeClass = newStatus === 'Completada' ? 'bg-success' : 'bg-danger';
            const tdElement = select.closest('td');
            if (tdElement) {
                tdElement.innerHTML = `<span class="badge status-badge ${badgeClass}">${statusText}</span>`;
            }
            return;
    }

    // Agregar opciones al select
    options.forEach(opt => {
        const option = document.createElement('option');
        option.value = opt.value;
        option.textContent = opt.text;
        option.selected = opt.selected;
        select.appendChild(option);
    });
}

// Actualizar badge de estado
function updateStatusBadge(rowElement, newStatus) {
    const badgeElement = rowElement.querySelector('.status-badge');
    if (!badgeElement) return;

    // Remover clases antiguas
    badgeElement.className = 'badge status-badge';

    // Agregar nueva clase según estado
    const statusText = newStatus.replace('EnProceso', 'En Proceso');
    switch (newStatus) {
        case 'Pendiente':
            badgeElement.classList.add('bg-warning', 'text-dark');
            break;
        case 'EnProceso':
        case 'En Proceso':
            badgeElement.classList.add('bg-info', 'text-dark');
            break;
        case 'Completada':
            badgeElement.classList.add('bg-success');
            break;
        case 'Cancelada':
            badgeElement.classList.add('bg-danger');
            break;
    }

    badgeElement.textContent = statusText;
}

// Toast notifications
function showToast(type, message) {
    // Crear toast dinámicamente
    const toastHtml = `
        <div class="toast align-items-center text-white bg-${type === 'success' ? 'success' : 'danger'} border-0" role="alert">
            <div class="d-flex">
                <div class="toast-body">
                    <i class="bi bi-${type === 'success' ? 'check-circle' : 'exclamation-circle'}"></i> ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `;

    // Agregar al container de toasts
    let container = document.getElementById('toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toast-container';
        container.className = 'toast-container position-fixed top-0 end-0 p-3';
        container.style.zIndex = '9999';
        document.body.appendChild(container);
    }

    const div = document.createElement('div');
    div.innerHTML = toastHtml;
    const toastElement = div.firstElementChild;
    container.appendChild(toastElement);

    const toast = new bootstrap.Toast(toastElement, { delay: 3000 });
    toast.show();

    toastElement.addEventListener('hidden.bs.toast', () => {
        toastElement.remove();
    });
}

// MODAL CREATE
function openCreateModal() {
    resetCreateForm();
    loadProducts('create_ProductId');

    // Configurar evento de previsualización
    document.getElementById('previewBomBtn').onclick = function () {
        const productId = document.getElementById('create_ProductId').value;
        const quantity = document.getElementById('create_Quantity').value;
        previewBom(productId, quantity, 'previewBomContent');
    };

    const modal = new bootstrap.Modal(document.getElementById('createOrderModal'));
    modal.show();
}

// MODAL EDIT
async function openEditModal(order) {
    resetEditForm();

    document.getElementById('editOrderModalTitle').textContent = `Modificando Orden #${order.productionOrderId}`;
    document.getElementById('edit_ProductionOrderId').value = order.productionOrderId;
    document.getElementById('edit_ProductId').value = order.productId;
    document.getElementById('edit_ProductName').value = order.product?.name || '';
    document.getElementById('edit_Quantity').value = order.quantity || '';

    // Cargar opciones de estado según estado actual
    loadStatusOptions(order.status);

    const form = document.getElementById('editOrderForm');
    form.action = `/ProductionOrders/Edit/${order.productionOrderId}`;

    const modal = new bootstrap.Modal(document.getElementById('editOrderModal'));
    modal.show();

    setTimeout(() => {
        if (typeof $.validator !== 'undefined') {
            $(form).removeData("validator");
            $(form).removeData("unobtrusiveValidation");
            $.validator.unobtrusive.parse(form);
        }
    }, 100);
}

// Cargar opciones de estado según validaciones
function loadStatusOptions(currentStatus) {
    const select = document.getElementById('edit_Status');
    select.innerHTML = '';

    const options = [];

    switch (currentStatus) {
        case 'Pendiente':
            options.push(
                { value: 'Pendiente', text: 'Pendiente' },
                { value: 'EnProceso', text: 'En Proceso' },
                { value: 'Cancelada', text: 'Cancelada' }
            );
            break;
        case 'EnProceso':
        case 'En Proceso':
            options.push(
                { value: 'EnProceso', text: 'En Proceso' },
                { value: 'Completada', text: 'Completada' },
                { value: 'Cancelada', text: 'Cancelada' }
            );
            break;
        case 'Completada':
        case 'Cancelada':
            options.push({ value: currentStatus, text: currentStatus });
            select.disabled = true;
            break;
    }

    options.forEach(opt => {
        const option = document.createElement('option');
        option.value = opt.value;
        option.textContent = opt.text;
        if (opt.value === currentStatus || (currentStatus === 'En Proceso' && opt.value === 'EnProceso')) {
            option.selected = true;
        }
        select.appendChild(option);
    });
}

// MODAL DETAILS
async function openDetailsModal(orderId) {
    try {
        const row = document.querySelector(`tr[data-order-id="${orderId}"]`);
        if (!row) {
            console.error('No se encontró la fila con order-id:', orderId);
            alert('Error: No se pudo encontrar la orden en la tabla');
            return;
        }

        // Obtener datos del atributo data-order
        const orderDataStr = row.getAttribute('data-order');
        if (!orderDataStr) {
            console.error('No se encontró data-order en la fila');
            alert('Error: Datos de la orden no disponibles');
            return;
        }

        const orderData = JSON.parse(orderDataStr);

        document.getElementById('detailsOrderModalTitle').textContent = `Detalles de Orden #${orderId}`;
        document.getElementById('details_OrderId').textContent = orderId;
        document.getElementById('details_ProductName').textContent = orderData.product?.name || '';
        document.getElementById('details_Quantity').textContent = orderData.quantity || '';

        // Formatear fecha LOCAL correctamente (sin adelantar horas)
        const orderDateStr = orderData.orderDate;
        let formattedDate = '';

        try {
            // La fecha viene en formato ISO desde el servidor
            // Extraer solo la parte visible de la tabla para evitar conversiones
            const dateCell = row.querySelector('.order-date');
            if (dateCell) {
                formattedDate = dateCell.textContent.trim();
            } else {
                // Fallback: parsear la fecha
                const orderDate = new Date(orderDateStr);
                formattedDate = orderDate.toLocaleString('es-AR', {
                    year: 'numeric',
                    month: '2-digit',
                    day: '2-digit',
                    hour: '2-digit',
                    minute: '2-digit',
                    hour12: false
                });
            }
        } catch (dateError) {
            console.error('Error formateando fecha:', dateError);
            formattedDate = orderDateStr;
        }

        document.getElementById('details_OrderDate').textContent = formattedDate;

        // Obtener estado actual
        let currentStatus = orderData.status;
        let statusDisplay = currentStatus.replace('EnProceso', 'En Proceso');

        // Determinar clase de badge según estado
        let badgeClass = 'badge ';
        switch (currentStatus) {
            case 'Pendiente':
                badgeClass += 'bg-warning text-dark';
                break;
            case 'EnProceso':
            case 'En Proceso':
                badgeClass += 'bg-info text-dark';
                statusDisplay = 'En Proceso';
                break;
            case 'Completada':
                badgeClass += 'bg-success';
                break;
            case 'Cancelada':
                badgeClass += 'bg-danger';
                break;
            default:
                badgeClass += 'bg-secondary';
        }

        document.getElementById('details_Status').innerHTML = `<span class="${badgeClass}">${statusDisplay}</span>`;

        // Cargar BOM
        const productId = orderData.productId;
        const quantity = orderData.quantity;

        try {
            const bomResponse = await fetch(`/ProductionOrders/PreviewBom?productId=${productId}&quantity=${quantity}`);
            if (bomResponse.ok) {
                const bomHtml = await bomResponse.text();
                document.getElementById('details_BomTable').innerHTML = bomHtml;
            } else {
                document.getElementById('details_BomTable').innerHTML = '<p class="text-danger">No se pudo cargar la lista de materiales</p>';
            }
        } catch (bomError) {
            console.error('Error cargando BOM:', bomError);
            document.getElementById('details_BomTable').innerHTML = '<p class="text-danger">Error al cargar materiales</p>';
        }

        // Verificar si está finalizada
        const isFinalized = currentStatus === 'Completada' || currentStatus === 'Cancelada';

        // Configurar botón de editar
        const editBtn = document.getElementById('detailsEditButton');
        if (isFinalized) {
            editBtn.classList.add('disabled');
            editBtn.onclick = null;
        } else {
            editBtn.classList.remove('disabled');
            editBtn.onclick = function () {
                const detailsModal = bootstrap.Modal.getInstance(document.getElementById('detailsOrderModal'));
                detailsModal.hide();
                setTimeout(() => {
                    openEditModal(orderData);
                }, 300);
            };
        }

        // Configurar botón de PDF
        const pdfBtn = document.getElementById('detailsPdfButton');
        pdfBtn.href = `/ProductionOrders/GeneratePdf/${orderId}`;
        if (isFinalized) {
            pdfBtn.classList.add('disabled');
            pdfBtn.onclick = (e) => e.preventDefault();
        } else {
            pdfBtn.classList.remove('disabled');
            pdfBtn.onclick = null;
        }

        const modal = new bootstrap.Modal(document.getElementById('detailsOrderModal'));
        modal.show();

    } catch (error) {
        console.error('Error cargando detalles:', error);
        alert('Error al cargar los detalles de la orden');
    }
}

// MODALES DE PAPELERA
function openMoveToTrashModal(orderId, productName) {
    document.getElementById('trash_OrderId').textContent = orderId;
    document.getElementById('trash_ProductName').textContent = productName;
    document.getElementById('moveToTrashForm').action = `/ProductionOrders/MoveToTrash/${orderId}`;

    const modal = new bootstrap.Modal(document.getElementById('moveToTrashModal'));
    modal.show();
}

function openRestoreModal(orderId, productName) {
    document.getElementById('restore_OrderId').textContent = orderId;
    document.getElementById('restore_ProductName').textContent = productName;
    document.getElementById('restoreOrderForm').action = `/ProductionOrders/Restore/${orderId}`;

    const modal = new bootstrap.Modal(document.getElementById('restoreOrderModal'));
    modal.show();
}

function openDeleteModal(orderId, productName, quantity) {
    document.getElementById('delete_OrderId').textContent = orderId;
    document.getElementById('delete_ProductName').textContent = productName;
    document.getElementById('delete_Quantity').textContent = quantity;
    document.getElementById('deleteOrderForm').action = `/ProductionOrders/DeleteConfirmed/${orderId}`;

    const modal = new bootstrap.Modal(document.getElementById('deleteOrderModal'));
    modal.show();
}

// FUNCIONES DE LIMPIEZA
function resetCreateForm() {
    const form = document.querySelector('#createOrderModal form');
    if (form) {
        form.reset();
        document.getElementById('previewBomArea').style.display = 'none';

        if (typeof $.validator !== 'undefined') {
            const validator = $(form).validate();
            if (validator) validator.resetForm();
        }

        form.querySelectorAll('.text-danger, .field-validation-error').forEach(span => {
            span.textContent = '';
            span.classList.remove('field-validation-error');
            span.classList.add('field-validation-valid');
        });

        form.querySelectorAll('.is-invalid, .input-validation-error').forEach(input => {
            input.classList.remove('is-invalid', 'input-validation-error');
        });
    }
}

function resetEditForm() {
    const form = document.getElementById('editOrderForm');
    if (form) {
        form.reset();
        document.getElementById('editOrderModalTitle').textContent = 'Modificar Orden de Producción';

        if (typeof $.validator !== 'undefined') {
            const validator = $(form).validate();
            if (validator) validator.resetForm();
        }

        form.querySelectorAll('.text-danger, .field-validation-error').forEach(span => {
            span.textContent = '';
            span.classList.remove('field-validation-error');
            span.classList.add('field-validation-valid');
        });

        form.querySelectorAll('.is-invalid, .input-validation-error').forEach(input => {
            input.classList.remove('is-invalid', 'input-validation-error');
        });
    }
}

// LIMPIAR AL CERRAR MODALES
let pendingStatusChange = null;

document.querySelectorAll('.status-dropdown').forEach(select => {
    select.addEventListener('change', function () {
        const orderId = this.getAttribute('data-order-id');
        const newStatus = this.value;
        const row = this.closest('tr');
        const statusLabels = {
            'Pendiente': 'Pendiente',
            'EnProceso': 'En Proceso',
            'Completada': 'Completada',
            'Cancelada': 'Cancelada'
        };

        pendingStatusChange = { orderId, newStatus, row, select };

        document.getElementById('status_OrderId').textContent = orderId;
        document.getElementById('status_NewStatus').textContent = statusLabels[newStatus] ?? newStatus;

        const modal = new bootstrap.Modal(document.getElementById('changeStatusModal'));
        modal.show();
    });
});

document.getElementById('confirmStatusChange').addEventListener('click', function () {
    if (!pendingStatusChange) return;
    const { orderId, newStatus, row } = pendingStatusChange;
    const modal = bootstrap.Modal.getInstance(document.getElementById('changeStatusModal'));
    modal.hide();
    changeOrderStatus(orderId, newStatus, row);
    pendingStatusChange = null;
});

document.getElementById('cancelStatusChange').addEventListener('click', function () {
    if (!pendingStatusChange) return;
    pendingStatusChange.select.value = pendingStatusChange.select.getAttribute('data-original-status');
    pendingStatusChange = null;
});

document.getElementById('changeStatusModal').addEventListener('hidden.bs.modal', function () {
    if (pendingStatusChange) {
        pendingStatusChange.select.value = pendingStatusChange.select.getAttribute('data-original-status');
        pendingStatusChange = null;
    }
});