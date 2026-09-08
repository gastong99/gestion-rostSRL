document.addEventListener("DOMContentLoaded", function () {

    const itemSelect = document.getElementById("transferItemId");
    const sourceWarehouseSelect = document.getElementById("transferSourceWarehouseId");
    const destinationWarehouseSelect = document.getElementById("transferDestinationWarehouseId");

    const availableStockInput = document.getElementById("transferAvailableStock");
    const quantityInput = document.getElementById("transferQuantity");
    const submitButton = document.getElementById("transferSubmitButton");
    const stockHelp = document.getElementById("transferStockHelp");

    if (!itemSelect || !sourceWarehouseSelect || !destinationWarehouseSelect || !availableStockInput || !quantityInput || !submitButton) return;

    async function updateAvailableStock() {

        const itemId = itemSelect.value;
        const warehouseId = sourceWarehouseSelect.value;

        availableStockInput.value = "-";
        quantityInput.removeAttribute("max");

        submitButton.disabled = true;

        if (!itemId || !warehouseId) {
            stockHelp.textContent = "Seleccione un item y un depósito de origen.";
            return;
        }

        try {
            const response = await fetch(`/Stocks/GetAvailableStock?itemId=${itemId}&warehouseId=${warehouseId}`);

            if (!response.ok) {
                throw new Error("No se pudo consultar el stock.");
            }

            const data = await response.json();

            if (!data.success) {
                stockHelp.textContent = data.message || "No se pudo consultar el stock.";
                return;
            }

            const availableQuantity = Number(data.availableQuantity);

            availableStockInput.value =
                availableQuantity.toLocaleString("es-AR", {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                });

            quantityInput.max = availableQuantity;

            if (availableQuantity <= 0) {
                stockHelp.textContent = "No hay stock disponible para transferir desde este depósito.";
                return;
            }

            stockHelp.textContent =
                "Stock disponible para transferir: " +
                availableQuantity.toLocaleString("es-AR", {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                });

            validateForm();

        }
        catch (error) {
            console.error(error);
            stockHelp.textContent = "No se pudo consultar el stock disponible.";
        }
    }

    function validateForm() {

        const itemId = itemSelect.value;
        const sourceWarehouseId = sourceWarehouseSelect.value;
        const destinationWarehouseId = destinationWarehouseSelect.value;

        const availableQuantity = Number(quantityInput.max || 0);

        const quantity = Number(quantityInput.value);

        const validWarehouses = sourceWarehouseId && destinationWarehouseId && sourceWarehouseId !== destinationWarehouseId;

        const validQuantity = quantity > 0 && quantity <= availableQuantity;

        submitButton.disabled = !itemId || !validWarehouses || !validQuantity;
    }

    itemSelect.addEventListener("change", function () {
        quantityInput.value = "";
        updateAvailableStock();
    });

    sourceWarehouseSelect.addEventListener("change", function () {
        quantityInput.value = "";
        updateAvailableStock();
    });

    destinationWarehouseSelect.addEventListener("change", function () {
        validateForm();
    });

    quantityInput.addEventListener("input", function () {
        validateForm();
    });

    const transferModal = document.getElementById("transferStockModal");

    if (transferModal) {

        transferModal.addEventListener("hidden.bs.modal", function () {

            itemSelect.value = "";
            sourceWarehouseSelect.value = "";
            destinationWarehouseSelect.value = "";

            availableStockInput.value = "-";

            quantityInput.value = "";
            quantityInput.removeAttribute("max");

            stockHelp.textContent = "Seleccione un item y un depósito de origen.";

            submitButton.disabled = true;
        });
    }
});