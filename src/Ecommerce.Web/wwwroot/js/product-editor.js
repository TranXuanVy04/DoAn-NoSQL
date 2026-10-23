(() => {
    const editor = document.getElementById('product-editor');
    if (!editor) return;
    const checkbox = document.getElementById('has-variants');
    const simple = document.getElementById('simple-product-fields');
    const variants = document.getElementById('variant-editor');
    const rows = document.getElementById('variant-rows');
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
