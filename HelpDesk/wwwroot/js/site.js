document.addEventListener("DOMContentLoaded", function () {
    markActiveMenu();
    addClockToFooter();
    addScrollToTopButton();
    setupPasswordToggles();

    // Yalnızca BAŞARI bildirimleri bir süre sonra otomatik kapanır.
    // Hata/uyarı mesajları, önemli geri bildirim kaybolmasın diye kullanıcı
    // kapatana kadar (alert-dismissible kapatma butonuyla) görünür kalır.
    var basariAlerts = document.querySelectorAll(".alert-success");

    basariAlerts.forEach(function (alert) {
        setTimeout(function () {
            alert.classList.remove("show");
            setTimeout(function () {
                alert.style.display = "none";
            }, 300);
        }, 4000);
    });

    var nameInputs = document.querySelectorAll("input[name='AdSoyad']");

    nameInputs.forEach(function (input) {
        input.addEventListener("blur", function () {
            var value = input.value.trim().toLowerCase();

            if (value.length === 0) {
                return;
            }

            var formattedValue = value
                .split(" ")
                .filter(function (word) {
                    return word.length > 0;
                })
                .map(function (word) {
                    return word.charAt(0).toUpperCase() + word.slice(1);
                })
                .join(" ");

            input.value = formattedValue;
        });
    });

    var titleInput = document.querySelector("input[name='Baslik']");

    if (titleInput) {
        titleInput.addEventListener("input", function () {
            var text = titleInput.value;

            if (text.length > 0) {
                titleInput.value = text.charAt(0).toUpperCase() + text.slice(1);
            }
        });
    }

    var textAreas = document.querySelectorAll("textarea");

    textAreas.forEach(function (textArea) {
        var counter = document.createElement("small");
        counter.style.display = "block";
        counter.style.marginTop = "5px";
        counter.style.color = "#6c757d";
        textArea.parentNode.appendChild(counter);

        function updateCounter() {
            counter.textContent = "Karakter sayisi: " + textArea.value.length;
        }

        textArea.addEventListener("input", updateCounter);
        updateCounter();
    });
});

function markActiveMenu() {
    var currentPath = window.location.pathname.toLowerCase();
    var navLinks = document.querySelectorAll(".navbar .nav-link");

    navLinks.forEach(function (link) {
        var linkPath = new URL(link.href).pathname.toLowerCase();

        if (currentPath === linkPath) {
            link.classList.add("active");
            link.style.fontWeight = "700";
            link.style.textDecoration = "underline";
        }
    });
}

function addClockToFooter() {
    var footer = document.querySelector(".footer .container");

    if (!footer) {
        return;
    }

    var clock = document.createElement("span");
    clock.className = "text-muted small";
    footer.appendChild(clock);

    function updateClock() {
        var now = new Date();
        clock.textContent = "Tarih/Saat: " + now.toLocaleString("tr-TR");
    }

    updateClock();
    setInterval(updateClock, 1000);
}

function addScrollToTopButton() {
    var button = document.createElement("button");
    button.textContent = "Yukari";
    button.type = "button";
    button.style.position = "fixed";
    button.style.right = "20px";
    button.style.bottom = "20px";
    button.style.padding = "10px 14px";
    button.style.border = "none";
    button.style.borderRadius = "8px";
    button.style.backgroundColor = "#0d6efd";
    button.style.color = "white";
    button.style.cursor = "pointer";
    button.style.display = "none";
    button.style.zIndex = "9999";

    document.body.appendChild(button);

    window.addEventListener("scroll", function () {
        if (window.scrollY > 200) {
            button.style.display = "block";
        } else {
            button.style.display = "none";
        }
    });

    button.addEventListener("click", function () {
        window.scrollTo({
            top: 0,
            behavior: "smooth"
        });
    });
}

function setupPasswordToggles() {
    var passwordInputs = document.querySelectorAll("input[type='password']");

    passwordInputs.forEach(function (input) {
        if (input.dataset.toggleAdded === "true") {
            return;
        }

        var toggleButton = document.createElement("button");
        toggleButton.type = "button";
        toggleButton.textContent = "Goster";
        toggleButton.style.marginTop = "6px";
        toggleButton.style.border = "none";
        toggleButton.style.background = "transparent";
        toggleButton.style.color = "#0d6efd";

        toggleButton.addEventListener("click", function () {
            if (input.type === "password") {
                input.type = "text";
                toggleButton.textContent = "Gizle";
            } else {
                input.type = "password";
                toggleButton.textContent = "Goster";
            }
        });

        input.insertAdjacentElement("afterend", toggleButton);
        input.dataset.toggleAdded = "true";
    });
}
