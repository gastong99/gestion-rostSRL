(function() {
    "use strict";

    const instances = new WeakMap();

    // =====================
    // GET INPUT
    // =====================

    function getInput(inputOrId) {
        if (inputOrId instanceof HTMLElement) {
            return inputOrId;
        }

        if (typeof inputOrId === "string") {
            return document.getElementById(inputOrId);
        }

        return null;
    }

    // =====================
    // CREATE MASK
    // =====================

    function createMask(input) {
        if (!input || instances.has(input)) {
            return;
        }

        const maskType = input.dataset.mask;

        let maskOptions = null;

        switch (maskType) {

            case "cuit":
                maskOptions = {
                    mask: "00-00000000-0"
                };
                break;

            default:
                console.warn(
                    `No existe una configuración para la máscara "${maskType}".`,
                    input
                );
                return;
        }

        const mask = IMask(input, maskOptions);

        instances.set(input, mask);
    }

    // =====================
    // INITIALIZE
    // =====================

    function initialize(root = document) {
        if (typeof IMask === "undefined") {
            console.error("IMask no está cargado.");
            return;
        }

        if (!root) {
            return;
        }

        // Permite inicializar directamente un input.
        if (
            root instanceof HTMLElement &&
            root.matches("[data-mask]")
        ) {
            createMask(root);
        }

        // Inicializa todos los inputs dentro del contenedor.
        const inputs = root.querySelectorAll("[data-mask]");

        inputs.forEach(input => {
            createMask(input);
        });
    }

    // =====================
    // SET VALUE
    // =====================

    function setValue(inputOrId, value) {
        const input = getInput(inputOrId);

        if (!input) {
            return;
        }

        const mask = instances.get(input);
        const newValue = value ?? "";

        if (mask) {
            mask.value = newValue;
        } else {
            input.value = newValue;
        }
    }

    // =====================
    // RESET
    // =====================

    function reset(root = document) {
        if (!root) {
            return;
        }

        const inputs = [];

        if (
            root instanceof HTMLElement &&
            root.matches("[data-mask]")
        ) {
            inputs.push(root);
        }

        inputs.push(
            ...root.querySelectorAll("[data-mask]")
        );

        inputs.forEach(input => {
            const mask = instances.get(input);

            if (mask) {
                mask.updateValue();
            }
        });
    }

    // =====================
    // PUBLIC API
    // =====================

    window.FormMasks = {
        initialize,
        setValue,
        reset
    };

    // =====================
    // DOM READY
    // =====================

    document.addEventListener("DOMContentLoaded", function() {
        initialize();
    });

})();