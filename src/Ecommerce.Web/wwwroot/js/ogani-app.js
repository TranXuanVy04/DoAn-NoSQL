(() => {
    const optionInputs = [...document.querySelectorAll('[data-variant-option]')];
    if (optionInputs.length) {
        const variants = [...document.querySelectorAll('#variant-data option')].map(option => ({
            id: option.value, options: JSON.parse(option.dataset.options),
            price: Number(option.dataset.price), stock: Number(option.dataset.stock), sku: option.dataset.sku
        }));
        const initialPrice = document.getElementById('selected-price').textContent;
        const initialSku = document.getElementById('selected-sku').textContent;
        const groups = [...document.querySelectorAll('[data-option-group]')].map(group => group.dataset.optionGroup);
        function updateAvailability() {
            const selected = {};
            groups.forEach(key => {
                const inputs = optionInputs.filter(input => input.dataset.variantOption === key);
                inputs.forEach(input => {
                    input.disabled = !variants.some(variant => variant.stock > 0
                        && variant.options[key] === input.value
                        && Object.entries(selected).every(([name, value]) => variant.options[name] === value));
                    if (input.disabled) input.checked = false;
                });
                const checked = inputs.find(input => input.checked);
                if (checked) selected[key] = checked.value;
            });
        }
        function updateVariant() {
            updateAvailability();
            const selected = Object.fromEntries(optionInputs.filter(input => input.checked).map(input => [input.dataset.variantOption, input.value]));
            document.querySelectorAll('[data-selected-option]').forEach(label => {
                label.textContent = selected[label.dataset.selectedOption] || '';
            });
            const complete = Object.keys(selected).length === document.querySelectorAll('[data-option-group]').length;
            const variant = complete ? variants.find(v => Object.keys(v.options).length === Object.keys(selected).length && Object.entries(selected).every(([key, value]) => v.options[key] === value)) : null;
            document.getElementById('selected-variant').value = variant?.id || '';
            document.getElementById('selected-price').textContent = variant ? variant.price.toLocaleString('vi-VN') + ' VND' : initialPrice;
            document.getElementById('selected-sku').textContent = variant?.sku || initialSku;
            document.getElementById('selected-stock').textContent = variant ? (variant.stock > 0 ? 'Còn ' + variant.stock + ' sản phẩm' : 'Tạm hết hàng') : complete ? 'Không có phiên bản cho lựa chọn này' : 'Chọn đầy đủ các thuộc tính';
            const quantity = document.querySelector('#product-add-form input[name="quantity"]');
            quantity.max = variant ? Math.max(1, Math.min(99, variant.stock)) : 99;
            quantity.value = Math.max(1, Math.min(Number(quantity.value) || 1, Number(quantity.max)));
            const buy = document.getElementById('product-buy');
            buy.disabled = !variant || variant.stock <= 0;
            buy.textContent = variant ? (variant.stock > 0 ? 'THÊM VÀO GIỎ' : 'TẠM HẾT HÀNG') : complete ? 'KHÔNG CÓ PHIÊN BẢN' : 'CHỌN PHIÊN BẢN';
        }
        optionInputs.forEach(input => input.addEventListener('change', () => {
            const groupIndex = groups.indexOf(input.dataset.variantOption);
            optionInputs.forEach(other => {
                if (groups.indexOf(other.dataset.variantOption) > groupIndex) other.checked = false;
            });
            updateVariant();
        }));
        updateVariant();
        document.getElementById('product-add-form').addEventListener('submit', event => {
            if (!document.getElementById('selected-variant').value) event.preventDefault();
        });
    }
    const filterDrawer = document.getElementById('product-filters');
    const filterToggle = document.querySelector('.ec-filter-toggle');
    filterToggle?.addEventListener('click', () => {
        filterDrawer.showModal();
        document.body.classList.add('ec-filter-open');
        filterToggle.setAttribute('aria-expanded', 'true');
    });
    document.querySelector('.ec-filter-close')?.addEventListener('click', () => filterDrawer.close());
    filterDrawer?.addEventListener('click', event => {
        if (event.target !== filterDrawer) return;
        const bounds = filterDrawer.getBoundingClientRect();
        if (event.clientX < bounds.left || event.clientX > bounds.right || event.clientY < bounds.top || event.clientY > bounds.bottom) filterDrawer.close();
    });
    filterDrawer?.addEventListener('close', () => {
        document.body.classList.remove('ec-filter-open');
        filterToggle.setAttribute('aria-expanded', 'false');
        filterToggle.focus();
    });
    document.getElementById('page-sort')?.addEventListener('change', event => {
        const grid = document.querySelector('.ec-product-grid');
        const cards = [...grid.children];
        cards.forEach((card, index) => { card.dataset.originalOrder ??= index; });
        cards.sort((a, b) => event.target.value === 'default'
            ? Number(a.dataset.originalOrder) - Number(b.dataset.originalOrder)
            : a.dataset.name.localeCompare(b.dataset.name, 'vi') * (event.target.value === 'desc' ? -1 : 1));
        cards.forEach(card => grid.appendChild(card));
    });
    document.querySelectorAll('[data-search-query]').forEach(input => {
        function validateQuery() {
            input.setCustomValidity(input.value.trim() ? '' : 'Vui lòng nhập từ khóa tìm kiếm.');
        }
        input.addEventListener('input', validateQuery);
        input.addEventListener('invalid', validateQuery);
        input.form?.addEventListener('submit', event => {
            input.value = input.value.trim();
            validateQuery();
            if (!input.reportValidity()) event.preventDefault();
        });
        validateQuery();
    });
    const toast = document.querySelector('.ec-toast');
    let toastTimer;
    function notify(message) {
        if (!toast) return;
        toast.textContent = message;
        toast.hidden = false;
        clearTimeout(toastTimer);
        toastTimer = setTimeout(() => { toast.hidden = true; }, 4500);
    }
    document.querySelectorAll('[data-demo-form]').forEach(form => {
        form.addEventListener('submit', event => {
            event.preventDefault();
            notify(form.dataset.demoForm);
        });
    });
    document.querySelectorAll('[data-demo-action]').forEach(button => {
        button.addEventListener('click', () => notify(button.dataset.demoAction));
    });
    document.querySelectorAll('[data-quantity-change]').forEach(button => {
        button.addEventListener('click', () => {
            const input = button.closest('.ec-quantity').querySelector('input');
            const min = Number(input.min || 1), max = Number(input.max || 99);
            input.value = String(Math.min(max, Math.max(min, (Number(input.value) || min) + Number(button.dataset.quantityChange))));
            input.dispatchEvent(new Event('change', { bubbles: true }));
        });
    });
    function closeMenu() {
        document.querySelector('.humberger__menu__wrapper')?.classList.remove('show__humberger__menu__wrapper');
        document.querySelector('.humberger__menu__overlay')?.classList.remove('active');
        document.body.classList.remove('over_hid');
        document.querySelector('.humberger__open')?.setAttribute('aria-expanded', 'false');
    }
    document.querySelector('.ec-mobile-close')?.addEventListener('click', closeMenu);
    document.querySelector('.humberger__menu__overlay')?.addEventListener('click', closeMenu);
    document.querySelector('.humberger__open')?.addEventListener('click', () => {
        document.querySelector('.humberger__open').setAttribute('aria-expanded', 'true');
        document.querySelector('.ec-mobile-close')?.focus();
    });
    document.addEventListener('keydown', event => {
        if (event.key === 'Escape' && document.body.classList.contains('over_hid')) { closeMenu(); document.querySelector('.humberger__open')?.focus(); }
    });
    if (window.matchMedia('(max-width: 991px)').matches) {
        document.querySelector('.hero__categories__all')?.setAttribute('aria-expanded', 'false');
    }
    document.querySelector('.hero__categories__all')?.addEventListener('click', event => {
        const button = event.currentTarget;
        button.setAttribute('aria-expanded', String(button.getAttribute('aria-expanded') !== 'true'));
    });
})();
