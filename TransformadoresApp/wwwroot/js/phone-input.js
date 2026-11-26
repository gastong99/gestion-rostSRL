
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
    // CREATE INSTANCE
    // =====================

    function createInstance(input) {
        if (!input || instances.has(input)) {
            return;
        }

        if (typeof window.intlTelInput !== "function") {
            console.error("intl-tel-input no está cargado.");
            return;
        }

        const iti = window.intlTelInput(input, {
            initialCountry: input.dataset.initialCountry || "ar",
            numberDisplayFormat: "NATIONAL",
            placeholderNumberPolicy: "POLITE",
            loadUtils: () => import(
                "/lib/intl-tel-input/js/utils.js"
            )
        });

        instances.set(input, iti);
    }

    // =====================
    // INITIALIZE
    // =====================

    function initialize(root = document) {
        if (!root) {
            return;
        }

        if (typeof window.intlTelInput !== "function") {
            console.error("intl-tel-input no está cargado.");
            return;
        }

        // Permite inicializar directamente un input.
        if (
            root instanceof HTMLElement &&
            root.matches("[data-phone-input]")
        ) {
            createInstance(root);
        }

        // Inicializa todos los teléfonos dentro del contenedor.
        const inputs = root.querySelectorAll("[data-phone-input]");

        inputs.forEach(input => {
            createInstance(input);
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

        const iti = instances.get(input);
        const phone = value ?? "";

        if (!iti) {
            input.value = phone;
            return;
        }

        if (phone.trim() === "") {
            input.value = "";

            iti.setSelectedCountry(
                input.dataset.initialCountry || "ar"
            );

            return;
        }

        Promise.resolve(iti.promise)
            .then(() => {
                iti.setNumber(phone);
            })
            .catch(error => {
                console.error(
                    "Error al configurar el teléfono:",
                    error
                );
            });
    }

    // =====================
    // RESET
    // =====================
    function reset(inputOrId) {
        const input = getInput(inputOrId);

        if (!input) {
            return;
        }

        input.value = "";

        const iti = instances.get(input);

        if (iti) {
            iti.setSelectedCountry(
                input.dataset.initialCountry || "ar"
            );
        }
    }

    // =====================
    // PUBLIC API
    // =====================

    window.PhoneInput = {
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
