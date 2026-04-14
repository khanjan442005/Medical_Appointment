document.addEventListener("DOMContentLoaded", () => {
    document.documentElement.classList.add("js-ready");
    initializeNavToggles();

    const tiltCards = document.querySelectorAll("[data-tilt]");

    tiltCards.forEach((card) => {
        let rafId = 0;

        const resetTilt = () => {
            card.style.transform = "";
        };

        card.addEventListener("mousemove", (event) => {
            const bounds = card.getBoundingClientRect();
            const px = (event.clientX - bounds.left) / bounds.width;
            const py = (event.clientY - bounds.top) / bounds.height;
            const rotateY = (px - 0.5) * 8;
            const rotateX = (0.5 - py) * 6;

            cancelAnimationFrame(rafId);
            rafId = requestAnimationFrame(() => {
                card.style.transform =
                    `perspective(1400px) rotateX(${rotateX}deg) rotateY(${rotateY}deg) translateY(-3px)`;
            });
        });

        card.addEventListener("mouseleave", () => {
            cancelAnimationFrame(rafId);
            resetTilt();
        });
    });

    const orbitNodes = document.querySelectorAll("[data-orbit]");

    if (orbitNodes.length > 0) {
        document.addEventListener("mousemove", (event) => {
            const x = (event.clientX / window.innerWidth - 0.5) * 2;
            const y = (event.clientY / window.innerHeight - 0.5) * 2;

            orbitNodes.forEach((node, index) => {
                const distance = (index + 1) * 7;
                node.style.transform =
                    `translate3d(${x * distance}px, ${y * distance}px, 0)`;
            });
        });
    }

    initializeAuthValidation();
    initializeBookingFlow();
    initializePaymentFlow();
});

function initializeNavToggles() {
    const toggles = document.querySelectorAll("[data-nav-toggle]");

    if (toggles.length === 0) {
        return;
    }

    const resetToggles = () => {
        if (window.innerWidth < 1200) {
            return;
        }

        toggles.forEach((toggle) => {
            const panelId = toggle.dataset.navTarget;
            const panel = panelId ? document.getElementById(panelId) : null;

            toggle.classList.remove("is-active");
            toggle.setAttribute("aria-expanded", "false");

            if (panel) {
                panel.classList.remove("is-open");
            }
        });
    };

    toggles.forEach((toggle) => {
        const panelId = toggle.dataset.navTarget;
        const panel = panelId ? document.getElementById(panelId) : null;

        if (!panel) {
            return;
        }

        toggle.addEventListener("click", () => {
            const isOpen = panel.classList.toggle("is-open");
            toggle.classList.toggle("is-active", isOpen);
            toggle.setAttribute("aria-expanded", isOpen ? "true" : "false");
        });
    });

    window.addEventListener("resize", resetToggles);
    resetToggles();
}

function initializeAuthValidation() {
    const authForms = document.querySelectorAll(".auth-form-stack");
    if (authForms.length === 0) {
        return;
    }

    const passwordRuleLabels = [
        { key: "length", label: "8 to 20 characters" },
        { key: "upper", label: "At least one uppercase letter" },
        { key: "lower", label: "At least one lowercase letter" },
        { key: "number", label: "At least one number" },
        { key: "special", label: "At least one special character" }
    ];

    const emailPattern = /^\S+@\S+\.\S+$/;

    const getFieldName = (input) => {
        const name = input.getAttribute("name") ?? "";
        const segments = name.split(".");
        return segments[segments.length - 1];
    };

    const getMessageNode = (input) => {
        const form = input.form;
        if (!form) {
            return null;
        }

        const fieldName = getFieldName(input);
        return form.querySelector(`[data-valmsg-for="${fieldName}"]`);
    };

    const setMessage = (input, message) => {
        const messageNode = getMessageNode(input);
        if (messageNode) {
            messageNode.textContent = message;
        }

        input.classList.toggle("is-invalid", message.length > 0);
        input.classList.toggle("is-valid", message.length === 0 && input.value.trim().length > 0);
    };

    const evaluatePasswordRules = (value) => ({
        length: value.length >= 8 && value.length <= 20,
        upper: /[A-Z]/.test(value),
        lower: /[a-z]/.test(value),
        number: /\d/.test(value),
        special: /[^A-Za-z0-9\s]/.test(value)
    });

    const getPasswordMessage = (input, forceRequired) => {
        const value = input.value;
        const requiredMessage = input.dataset.requiredMessage || "Password is required.";

        if (value.length === 0) {
            return forceRequired ? requiredMessage : "";
        }

        if (value.length < 8) {
            return "Password must be at least 8 characters.";
        }

        if (value.length > 20) {
            return "Password must not exceed 20 characters.";
        }

        const rules = evaluatePasswordRules(value);
        if (!rules.upper) {
            return "Password must include at least one uppercase letter.";
        }

        if (!rules.lower) {
            return "Password must include at least one lowercase letter.";
        }

        if (!rules.number) {
            return "Password must include at least one number.";
        }

        if (!rules.special) {
            return "Password must include at least one special character (@, #, $, etc.).";
        }

        return "";
    };

    const getEmailMessage = (input, forceRequired) => {
        const value = input.value;
        if (value.trim().length === 0) {
            return forceRequired ? "Email is required." : "";
        }

        if (/\s/.test(value)) {
            return "Email must not contain spaces.";
        }

        if (!emailPattern.test(value)) {
            return "Enter a valid email address.";
        }

        return "";
    };

    const getConfirmPasswordMessage = (input, passwordInput, forceRequired) => {
        const value = input.value;
        const requiredMessage = input.dataset.requiredMessage || "Confirm password is required.";

        if (value.length === 0) {
            return forceRequired ? requiredMessage : "";
        }

        return value === passwordInput.value ? "" : "Passwords do not match.";
    };

    const enhancePasswordToggle = (input) => {
        if (!input || input.parentElement?.classList.contains("password-input-shell")) {
            return;
        }

        const wrapper = document.createElement("div");
        wrapper.className = "password-input-shell";
        input.parentNode.insertBefore(wrapper, input);
        wrapper.appendChild(input);

        const toggle = document.createElement("button");
        toggle.type = "button";
        toggle.className = "password-toggle";
        toggle.textContent = "Show";
        toggle.setAttribute("aria-label", "Show password");
        wrapper.appendChild(toggle);

        toggle.addEventListener("click", () => {
            const isPassword = input.type === "password";
            input.type = isPassword ? "text" : "password";
            toggle.textContent = isPassword ? "Hide" : "Show";
            toggle.setAttribute("aria-label", isPassword ? "Hide password" : "Show password");
        });
    };

    const enhancePasswordGuidance = (input) => {
        if (!input || input.dataset.passwordAssistReady === "true") {
            return;
        }

        const assist = document.createElement("div");
        assist.className = "password-assist";

        const strength = document.createElement("div");
        strength.className = "password-strength";
        strength.textContent = "Strength: Weak";
        assist.appendChild(strength);

        const list = document.createElement("ul");
        list.className = "password-rule-list";

        passwordRuleLabels.forEach((rule) => {
            const item = document.createElement("li");
            item.className = "password-rule";
            item.dataset.rule = rule.key;
            item.textContent = rule.label;
            list.appendChild(item);
        });

        assist.appendChild(list);

        const messageNode = getMessageNode(input);
        const container = input.closest("div");
        if (!container) {
            return;
        }

        if (messageNode) {
            messageNode.insertAdjacentElement("afterend", assist);
        } else {
            container.appendChild(assist);
        }

        input.dataset.passwordAssistReady = "true";
    };

    const updatePasswordAssist = (input) => {
        const container = input.closest("div");
        if (!container) {
            return;
        }

        const assist = container.querySelector(".password-assist");
        if (!assist) {
            return;
        }

        const rules = evaluatePasswordRules(input.value);
        let completed = 0;

        assist.querySelectorAll(".password-rule").forEach((item) => {
            const isValid = Boolean(rules[item.dataset.rule]);
            item.classList.toggle("is-valid", isValid);
            if (isValid) {
                completed += 1;
            }
        });

        const strengthNode = assist.querySelector(".password-strength");
        if (!strengthNode) {
            return;
        }

        let label = "Weak";
        if (completed >= 5) {
            label = "Strong";
        } else if (completed >= 3) {
            label = "Medium";
        }

        strengthNode.textContent = `Strength: ${label}`;
        strengthNode.dataset.strength = label.toLowerCase();
    };

    authForms.forEach((form) => {
        form.setAttribute("novalidate", "novalidate");

        const strictEmailInputs = form.querySelectorAll("input[data-strict-email='true']");
        const strongPasswordInput = form.querySelector("input[data-password-policy='strong']");
        const confirmPasswordInput = form.querySelector("input[data-confirm-password='true']");
        const passwordInputs = form.querySelectorAll("input[type='password']");

        passwordInputs.forEach((input) => {
            enhancePasswordToggle(input);
        });

        if (strongPasswordInput) {
            enhancePasswordGuidance(strongPasswordInput);
            updatePasswordAssist(strongPasswordInput);
        }

        strictEmailInputs.forEach((input) => {
            input.addEventListener("input", () => {
                setMessage(input, getEmailMessage(input, false));
            });

            input.addEventListener("blur", () => {
                setMessage(input, getEmailMessage(input, true));
            });
        });

        if (strongPasswordInput) {
            strongPasswordInput.addEventListener("input", () => {
                updatePasswordAssist(strongPasswordInput);
                setMessage(strongPasswordInput, getPasswordMessage(strongPasswordInput, false));

                if (confirmPasswordInput) {
                    setMessage(confirmPasswordInput, getConfirmPasswordMessage(confirmPasswordInput, strongPasswordInput, false));
                }
            });

            strongPasswordInput.addEventListener("blur", () => {
                updatePasswordAssist(strongPasswordInput);
                setMessage(strongPasswordInput, getPasswordMessage(strongPasswordInput, true));
            });
        }

        if (confirmPasswordInput && strongPasswordInput) {
            confirmPasswordInput.addEventListener("input", () => {
                setMessage(confirmPasswordInput, getConfirmPasswordMessage(confirmPasswordInput, strongPasswordInput, false));
            });

            confirmPasswordInput.addEventListener("blur", () => {
                setMessage(confirmPasswordInput, getConfirmPasswordMessage(confirmPasswordInput, strongPasswordInput, true));
            });
        }

        form.addEventListener("submit", (event) => {
            let firstInvalidField = null;

            strictEmailInputs.forEach((input) => {
                const message = getEmailMessage(input, true);
                setMessage(input, message);
                if (!firstInvalidField && message.length > 0) {
                    firstInvalidField = input;
                }
            });

            if (strongPasswordInput) {
                const passwordMessage = getPasswordMessage(strongPasswordInput, true);
                updatePasswordAssist(strongPasswordInput);
                setMessage(strongPasswordInput, passwordMessage);
                if (!firstInvalidField && passwordMessage.length > 0) {
                    firstInvalidField = strongPasswordInput;
                }
            }

            if (confirmPasswordInput && strongPasswordInput) {
                const confirmMessage = getConfirmPasswordMessage(confirmPasswordInput, strongPasswordInput, true);
                setMessage(confirmPasswordInput, confirmMessage);
                if (!firstInvalidField && confirmMessage.length > 0) {
                    firstInvalidField = confirmPasswordInput;
                }
            }

            if (firstInvalidField) {
                event.preventDefault();
                firstInvalidField.focus();
            }
        });
    });
}

function initializeBookingFlow() {
    const bookingForm = document.querySelector("[data-booking-form]");
    if (!bookingForm) {
        return;
    }

    const dateInput = bookingForm.querySelector("[data-booking-date]");
    const slotChoices = Array.from(bookingForm.querySelectorAll("[data-slot-choice]"));
    const messageNode = bookingForm.querySelector("[data-booking-slot-message]");

    if (!dateInput || slotChoices.length === 0 || !messageNode) {
        return;
    }

    const dayLabels = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];

    const resetSelection = (choice) => {
        const input = choice.querySelector("input[type='radio']");
        if (input) {
            input.checked = false;
        }
    };

    const updateSlots = () => {
        if (!dateInput.value) {
            slotChoices.forEach((choice) => {
                choice.hidden = false;
            });

            messageNode.textContent = "Pick a date to see which time slots are valid for that day.";
            return;
        }

        const selectedDate = new Date(`${dateInput.value}T00:00:00`);
        const dayLabel = Number.isNaN(selectedDate.getTime()) ? "" : dayLabels[selectedDate.getDay()];
        let visibleCount = 0;

        slotChoices.forEach((choice) => {
            const days = (choice.dataset.slotDays ?? "").split("|").filter(Boolean);
            const isVisible = dayLabel.length > 0 && days.includes(dayLabel);
            choice.hidden = !isVisible;

            if (!isVisible) {
                resetSelection(choice);
                return;
            }

            visibleCount += 1;
        });

        if (!dayLabel) {
            messageNode.textContent = "Choose a valid appointment date.";
            return;
        }

        if (visibleCount === 0) {
            messageNode.textContent = `No slots are available on ${dayLabel}. Choose another date from the doctor's saved schedule.`;
            return;
        }

        messageNode.textContent = `${visibleCount} slot(s) available on ${dayLabel}.`;
    };

    dateInput.addEventListener("change", updateSlots);
    updateSlots();
}

function initializePaymentFlow() {
    const paymentForm = document.querySelector("[data-payment-form]");
    if (!paymentForm) {
        return;
    }

    const methodInputs = Array.from(paymentForm.querySelectorAll("[data-payment-method]"));
    const cardFields = paymentForm.querySelector("[data-card-fields]");
    const noteNode = paymentForm.querySelector("[data-payment-note]");

    if (methodInputs.length === 0 || !cardFields || !noteNode) {
        return;
    }

    const cardInputs = Array.from(cardFields.querySelectorAll("input"));

    const updatePaymentView = () => {
        const selectedInput = methodInputs.find((input) => input.checked);
        const selectedMethod = selectedInput ? selectedInput.value : "";
        const isCard = selectedMethod === "Card";

        cardFields.hidden = !isCard;
        cardInputs.forEach((input) => {
            input.disabled = !isCard;
        });

        if (selectedMethod === "UPI") {
            noteNode.textContent = "UPI payments confirm instantly and do not require extra card details.";
            return;
        }

        if (selectedMethod === "Net Banking") {
            noteNode.textContent = "Net banking confirms directly after the payment request is submitted.";
            return;
        }

        if (selectedMethod === "Cash at clinic") {
            noteNode.textContent = "Your booking will be confirmed now, and you can pay at the clinic during the visit.";
            return;
        }

        noteNode.textContent = "Enter your card details to confirm the appointment payment.";
    };

    methodInputs.forEach((input) => {
        input.addEventListener("change", updatePaymentView);
    });

    updatePaymentView();
}
