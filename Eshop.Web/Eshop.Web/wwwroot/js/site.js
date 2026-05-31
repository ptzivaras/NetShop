document.addEventListener('DOMContentLoaded', function () {
    loadCartCount();
    initNavbarScroll();
    initCountUp();

    document.querySelectorAll('.add-to-cart-btn').forEach(function (btn) {
        btn.addEventListener('click', function () {
            addToCart(this.dataset.productId, this.dataset.productName, this);
        });
    });

    document.querySelectorAll('.heart-btn').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var span = this.querySelector('span') || this;
            span.classList.remove('heart-pulse');
            void span.offsetWidth;
            span.classList.add('heart-pulse');
        });
    });
});

function loadCartCount() {
    fetch('/Cart/Count')
        .then(function (r) { return r.ok ? r.json() : null; })
        .then(function (data) {
            if (data) updateCartBadge(data.count);
        })
        .catch(function () {});
}

function updateCartBadge(count) {
    var badge = document.getElementById('cart-badge');
    if (!badge) return;
    if (count > 0) {
        badge.textContent = count;
        badge.style.display = 'inline';
        badge.classList.remove('badge-bounce');
        void badge.offsetWidth;
        badge.classList.add('badge-bounce');
    } else {
        badge.style.display = 'none';
    }
}

function initNavbarScroll() {
    var navbar = document.querySelector('.navbar');
    if (!navbar) return;
    window.addEventListener('scroll', function () {
        navbar.classList.toggle('navbar-scrolled', window.scrollY > 10);
    }, { passive: true });
}

function initCountUp() {
    document.querySelectorAll('.stat-number[data-target]').forEach(function (el) {
        var target = parseFloat(el.dataset.target);
        var isDecimal = el.dataset.decimal === 'true';
        var prefix = el.dataset.prefix || '';
        var duration = 800;
        var start = performance.now();

        function step(now) {
            var progress = Math.min((now - start) / duration, 1);
            var eased = 1 - Math.pow(1 - progress, 3);
            var value = target * eased;
            el.textContent = prefix + (isDecimal ? value.toFixed(2) : Math.floor(value).toLocaleString());
            if (progress < 1) requestAnimationFrame(step);
        }
        requestAnimationFrame(step);
    });
}

function addToCart(productId, productName, btn) {
    var token = document.querySelector('meta[name="csrf-token"]')?.getAttribute('content');

    btn.disabled = true;

    fetch('/Cart/Add', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token
        },
        body: 'productId=' + encodeURIComponent(productId)
    })
    .then(function (r) {
        if (r.status === 401) {
            window.location.href = '/Identity/Account/Login';
            return null;
        }
        return r.json();
    })
    .then(function (data) {
        btn.disabled = false;
        if (!data) return;

        var toastEl = document.getElementById('addToCartToast');
        document.getElementById('toast-product-name').textContent = productName;
        new bootstrap.Toast(toastEl).show();

        loadCartCount();
    })
    .catch(function () {
        btn.disabled = false;
    });
}
