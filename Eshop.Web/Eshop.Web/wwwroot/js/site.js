document.addEventListener('DOMContentLoaded', function () {
    loadCartCount();

    document.querySelectorAll('.add-to-cart-btn').forEach(function (btn) {
        btn.addEventListener('click', function () {
            addToCart(this.dataset.productId, this.dataset.productName, this);
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
    } else {
        badge.style.display = 'none';
    }
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
