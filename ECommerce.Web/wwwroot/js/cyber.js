// Futuristic Cyber Merchandise E-Commerce JS

document.addEventListener('DOMContentLoaded', () => {
    initCartHandlers();
    initDemoAuthFill();
    initToasts();
});

// Toast notification helper
function showCyberToast(message, type = 'success') {
    let container = document.getElementById('cyber-toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'cyber-toast-container';
        container.style.position = 'fixed';
        container.style.bottom = '24px';
        container.style.right = '24px';
        container.style.zIndex = '9999';
        container.style.display = 'flex';
        container.style.flexDirection = 'column';
        container.style.gap = '10px';
        document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    toast.className = 'glass-panel p-3 d-flex align-items-center gap-3 animate__animated animate__fadeInUp';
    toast.style.minWidth = '280px';
    toast.style.border = type === 'success' ? '1px solid #06b6d4' : '1px solid #ec4899';
    toast.style.boxShadow = type === 'success' ? '0 0 15px rgba(6, 182, 212, 0.4)' : '0 0 15px rgba(236, 72, 153, 0.4)';
    toast.style.color = '#ffffff';

    const icon = type === 'success' 
        ? '<i class="bi bi-check-circle-fill text-cyan fs-5"></i>' 
        : '<i class="bi bi-exclamation-triangle-fill text-pink fs-5"></i>';

    toast.innerHTML = `
        ${icon}
        <div class="flex-grow-1 font-heading fw-semibold small">${message}</div>
        <button type="button" class="btn-close btn-close-white small" onclick="this.parentElement.remove()"></button>
    `;

    container.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transform = 'translateY(10px)';
        toast.style.transition = 'all 0.3s ease';
        setTimeout(() => toast.remove(), 300);
    }, 3500);
}

// Quick Add To Cart AJAX
function initCartHandlers() {
    document.querySelectorAll('.btn-quick-add-cart').forEach(button => {
        button.addEventListener('click', async (e) => {
            e.preventDefault();
            const productId = button.getAttribute('data-product-id');
            const quantityInput = document.getElementById('product-quantity');
            const quantity = quantityInput ? parseInt(quantityInput.value) || 1 : 1;

            button.disabled = true;
            const originalHtml = button.innerHTML;
            button.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Beam to Cart...';

            try {
                const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
                const token = tokenElement ? tokenElement.value : '';

                const response = await fetch('/Cart/AddToCart', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                        'RequestVerificationToken': token
                    },
                    body: new URLSearchParams({
                        productId: productId,
                        quantity: quantity
                    })
                });

                if (response.ok) {
                    const data = await response.json();
                    if (data.success) {
                        showCyberToast(data.message || 'Item added to Marwan Hosny Store cart!');
                        updateCartBadges(data.totalItems);
                    } else {
                        showCyberToast(data.message || 'Could not add item to cart.', 'error');
                    }
                } else {
                    showCyberToast('Error adding item to cart. Please try again.', 'error');
                }
            } catch (err) {
                console.error(err);
                showCyberToast('Network signal interrupted.', 'error');
            } finally {
                button.disabled = false;
                button.innerHTML = originalHtml;
            }
        });
    });
}

function updateCartBadges(count) {
    document.querySelectorAll('.cart-badge-count').forEach(el => {
        el.textContent = count;
        el.style.display = count > 0 ? 'flex' : 'none';
        el.classList.add('animate__animated', 'animate__pulse');
        setTimeout(() => el.classList.remove('animate__animated', 'animate__pulse'), 1000);
    });
}

// Quick demo login fill
function initDemoAuthFill() {
    window.fillAdminCredentials = () => {
        const emailInput = document.getElementById('Input_Email') || document.querySelector('input[name="Email"]');
        const passInput = document.getElementById('Input_Password') || document.querySelector('input[name="Password"]');
        if (emailInput && passInput) {
            emailInput.value = 'admin@marwanhosny.store';
            passInput.value = 'Admin@123456';
            showCyberToast('Admin credentials loaded!');
        }
    };

    window.fillCustomerCredentials = () => {
        const emailInput = document.getElementById('Input_Email') || document.querySelector('input[name="Email"]');
        const passInput = document.getElementById('Input_Password') || document.querySelector('input[name="Password"]');
        if (emailInput && passInput) {
            emailInput.value = 'customer@marwanhosny.store';
            passInput.value = 'Customer@123456';
            showCyberToast('Customer credentials loaded!');
        }
    };
}

function initToasts() {
    // Bootstrap toast auto init if needed
}

