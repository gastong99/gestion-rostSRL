(function() {
    "use strict";

    // =====================
    // CLEAR
    // =====================

    function clear(form) {
        if (!form) {
            return;
        }

        // Limpiar el estado interno de jQuery Validation.
        if (window.jQuery && window.jQuery.validator) {
            const validator = window.jQuery(form).data("validator");

            if (validator) {
                validator.resetForm();
            }
        }

        // Limpiar los estilos de error de los campos.
        form.querySelectorAll(
            ".input-validation-error, .is-invalid"
        ).forEach(element => {
            element.classList.remove(
                "input-validation-error",
                "is-invalid"
            );

            element.removeAttribute("aria-invalid");
        });

        // Limpiar los mensajes de validación.
        form.querySelectorAll("[data-valmsg-for]").forEach(element => {
            element.textContent = "";

            element.classList.remove("field-validation-error");
            element.classList.add("field-validation-valid");
        });
    }

    // =====================
    // RESET
    // =====================

    function reset(form) {
        if (!form) {
            return;
        }

        form.reset();

        clear(form);
    }

    // =====================
    // PUBLIC API
    // =====================

    window.FormValidation = {
        clear,
        reset
    };

})();