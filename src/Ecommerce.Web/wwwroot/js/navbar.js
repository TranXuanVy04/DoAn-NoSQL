(() => {
    const header = document.querySelector('.site-header');
    const toggle = document.querySelector('.menu-toggle');
    const navigation = document.getElementById('site-navigation');
    if (!header || !toggle || !navigation) return;

    // Không có JavaScript thì các liên kết vẫn hiện và dùng được.
    header.classList.add('menu-ready');
    toggle.hidden = false;

    function setMenu(open) {
        navigation.classList.toggle('is-open', open);
        toggle.setAttribute('aria-expanded', String(open));
    }

    toggle.addEventListener('click', () => {
        setMenu(toggle.getAttribute('aria-expanded') !== 'true');
    });

    header.addEventListener('keydown', event => {
        if (event.key !== 'Escape') return;
        const account = header.querySelector('.account-menu[open]');
        if (account) {
            account.open = false;
            account.querySelector('summary').focus();
            return;
        }
        if (toggle.getAttribute('aria-expanded') === 'true') {
            setMenu(false);
            toggle.focus();
        }
    });
})();
