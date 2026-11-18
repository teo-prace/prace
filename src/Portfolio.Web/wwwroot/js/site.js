// Tema claro/escuro, paleta/logo de teste (/brand) e menu do celular. Sem dependências.
(function () {
    var root = document.documentElement;
    // Valores vindos do servidor (appsettings), usados quando nada foi escolhido.
    var defaults = { palette: root.getAttribute("data-default-palette"), logo: root.getAttribute("data-default-logo") };

    function saved(key) {
        try { return localStorage.getItem(key); } catch (e) { return null; }
    }

    function store(key, value) {
        try {
            if (value) localStorage.setItem(key, value); else localStorage.removeItem(key);
        } catch (e) { }
    }

    function applyTheme(theme) {
        var t = theme === "light" ? "light" : "dark";
        root.setAttribute("data-theme", t);
        document.querySelectorAll("[data-theme-toggle]").forEach(function (btn) {
            btn.setAttribute("aria-pressed", t === "light" ? "true" : "false");
        });
        var meta = document.querySelector('meta[name="theme-color"]');
        if (meta) meta.setAttribute("content", t === "light" ? "#F2F0EC" : "#0B0F12");
    }

    function applyBrand() {
        // Escolhas de paleta/logo são da fase de testes (/brand); em produção vale o appsettings.
        if (!root.hasAttribute("data-brand-lab")) return;
        ["palette", "logo"].forEach(function (key) {
            var value = saved(key) || defaults[key];
            if (value) root.setAttribute("data-" + key, value);
            document.querySelectorAll("[data-set-" + key + "]").forEach(function (btn) {
                btn.setAttribute("aria-pressed", btn.getAttribute("data-set-" + key) === value ? "true" : "false");
            });
        });
    }

    function applyAll() {
        applyTheme(saved("theme"));
        applyBrand();
    }

    document.addEventListener("click", function (e) {
        var toggle = e.target.closest("[data-theme-toggle]");
        if (toggle) {
            var next = root.getAttribute("data-theme") === "light" ? "dark" : "light";
            store("theme", next);
            applyTheme(next);
            return;
        }

        var choice = e.target.closest("[data-set-palette], [data-set-logo]");
        if (choice) {
            var key = choice.hasAttribute("data-set-palette") ? "palette" : "logo";
            store(key, choice.getAttribute("data-set-" + key));
            applyBrand();
            return;
        }

        if (e.target.closest("[data-brand-reset]")) {
            store("palette", null);
            store("logo", null);
            applyBrand();
            return;
        }

        // Fecha o menu do celular ao clicar em um link dele.
        var link = e.target.closest(".mobile-menu a");
        if (link) {
            var menu = link.closest("details");
            if (menu) menu.removeAttribute("open");
        }
    });

    applyAll();

    // A navegação do Blazor troca a página sem recarregar: reaplica as escolhas depois de cada troca.
    if (window.Blazor && Blazor.addEventListener) {
        Blazor.addEventListener("enhancedload", applyAll);
    }
})();
