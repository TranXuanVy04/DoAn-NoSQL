(() => {
    const editor = document.getElementById('product-editor');
    if (!editor) return;
    const checkbox = document.getElementById('has-variants');
    const simple = document.getElementById('simple-product-fields');
    const variants = document.getElementById('variant-editor');
    const rows = document.getElementById('variant-rows');
    const category = document.getElementById('CategoryId');
    const json = document.getElementById('AttributesJson');
    const fields = document.getElementById('attribute-fields');
    const config = JSON.parse(document.getElementById('attribute-config').textContent);
    const labels = { brand: 'Thương hiệu', warrantyMonths: 'Bảo hành (tháng)', connectivity: 'Kết nối', material: 'Chất liệu', size: 'Kích cỡ', origin: 'Xuất xứ', color: 'Màu sắc' };
    function hasVariants() { return checkbox.type === 'checkbox' ? checkbox.checked : checkbox.value.toLowerCase() === 'true'; }
    function readAttributes() {
        const value = JSON.parse(json.value);
        if (!value || Array.isArray(value) || typeof value !== 'object') throw new Error();
        return value;
    }
    function syncAttributes() {
        try {
            const value = readAttributes();
            fields.querySelectorAll('input').forEach(input => {
                const key = input.dataset.key;
                if (!input.value.trim()) delete value[key];
                else value[key] = input.type === 'number' ? Number(input.value) : input.value.trim();
            });
            if (hasVariants()) { delete value.size; delete value.color; }
            json.value = JSON.stringify(value, null, 2);
            json.setCustomValidity('');
        } catch { json.setCustomValidity('Thuộc tính phải là JSON object hợp lệ.'); }
    }
    function renderAttributes() {
        let value;
        try { value = readAttributes(); json.setCustomValidity(''); }
        catch { document.getElementById('attribute-advanced').open = true; return; }
        let root = config.categories.find(c => c.id === category.value);
        const visited = new Set();
        while (root?.parentId && !visited.has(root.id)) {
            visited.add(root.id); root = config.categories.find(c => c.id === root.parentId);
        }
        const rules = config.rules[root?.id] || {};
        const keys = new Set(Object.keys(rules));
        Object.keys(value).filter(k => k in labels).forEach(k => keys.add(k));
        if ('brand' in rules) keys.add('connectivity');
        fields.replaceChildren();
        keys.forEach(key => {
            if (hasVariants() && (key === 'size' || key === 'color')) return;
            const label = document.createElement('label'); label.textContent = labels[key] || key;
            const input = document.createElement('input'); input.dataset.key = key;
            input.type = rules[key] === 'nonNegativeNumber' || typeof value[key] === 'number' ? 'number' : 'text';
            if (input.type === 'number') { input.min = '0'; input.step = 'any'; }
            else input.maxLength = 160;
            input.required = key in rules; input.value = value[key] ?? '';
            input.addEventListener('input', syncAttributes);
            label.append(input); fields.append(label);
        });
        document.getElementById('attribute-hint').textContent = hasVariants()
            ? 'Nhập thông tin chung. Màu và kích cỡ nhập riêng ở từng biến thể.'
            : 'Nhập thông số theo danh mục. Các ô bắt buộc được kiểm tra khi lưu.';
    }
    category.addEventListener('change', renderAttributes);
    checkbox.addEventListener('change', () => { renderAttributes(); syncAttributes(); });
    json.addEventListener('change', renderAttributes);
    editor.addEventListener('submit', event => {
        syncAttributes();
        if (!json.checkValidity()) { event.preventDefault(); document.getElementById('attribute-advanced').open = true; json.reportValidity(); }
    });
    const imageFile = document.getElementById('image-file');
    const preview = document.getElementById('image-preview');
    let previewUrl;
    imageFile.addEventListener('change', () => {
        if (previewUrl) URL.revokeObjectURL(previewUrl);
        imageFile.setCustomValidity('');
        const file = imageFile.files[0];
        if (file && (file.size > 5 * 1024 * 1024 || !['image/jpeg','image/png','image/webp'].includes(file.type))) {
            imageFile.setCustomValidity('Chọn ảnh JPG, PNG hoặc WebP tối đa 5 MB.'); imageFile.reportValidity(); return;
        }
        preview.src = file ? (previewUrl = URL.createObjectURL(file)) : document.getElementById('ImageUrl').value;
        preview.hidden = !preview.getAttribute('src');
    });
    renderAttributes();
    function toggle() {
        const enabled = checkbox.type === 'checkbox' ? checkbox.checked : checkbox.value.toLowerCase() === 'true';
        simple.hidden = enabled;
        variants.hidden = !enabled;
        simple.querySelectorAll('input').forEach(input => { input.disabled = enabled; input.required = !enabled; });
        variants.querySelectorAll('input, textarea').forEach(input => { input.disabled = !enabled; });
    }
    checkbox.addEventListener('change', () => {
        toggle();
        if (checkbox.checked && rows.children.length === 0) addVariant();
    });
    function addVariant() {
        if (rows.children.length >= 100) return;
        const key = crypto.randomUUID().replaceAll('-', '');
        const html = document.getElementById('variant-row-template').innerHTML.replaceAll('__key__', key);
        rows.insertAdjacentHTML('beforeend', html);
        rows.lastElementChild.querySelector('input[required]').focus();
    }
    document.getElementById('add-variant').addEventListener('click', addVariant);
    rows.addEventListener('click', event => {
        const button = event.target.closest('[data-remove-variant]');
        if (button) button.closest('.variant-edit-row').remove();
    });
    toggle();
})();
