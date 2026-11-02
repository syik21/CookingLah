(function () {
    function init() {
        document.querySelectorAll('#txtSignPass, #password, #txtNewPassword').forEach(function (input) {
            if (input.dataset.strengthReady) return;
            input.dataset.strengthReady = 'true';
            var box = document.createElement('div');
            box.style.cssText = 'width:100%;font-size:12px;line-height:1.5;margin:8px 0;color:#374151';
            var meter = document.createElement('meter');
            meter.min = 0; meter.max = 3; meter.low = 1; meter.high = 2; meter.optimum = 3;
            meter.style.cssText = 'display:block;width:100%;height:10px';
            meter.setAttribute('aria-label', 'Password strength');
            var label = document.createElement('span');
            label.id = input.id + 'Strength';
            label.setAttribute('aria-live', 'polite');
            box.append(meter, label);
            input.parentNode.insertBefore(box, input.nextSibling);
            input.setAttribute('aria-describedby', ((input.getAttribute('aria-describedby') || '') + ' ' + label.id).trim());
            function update() {
                var value = input.value;
                var valid = value.length >= 8 && /[A-Za-z]/.test(value) && /[0-9]/.test(value) && /[^\w\s]|_/.test(value) && new TextEncoder().encode(value).length <= 72;
                var score = !value ? 0 : !valid ? 1 : value.length < 12 ? 2 : 3;
                meter.value = score;
                label.textContent = (value ? ['','Weak','Moderate','Strong'][score] + '. ' : '') + 'Use 8+ characters with a letter, number and symbol; maximum 72 UTF-8 bytes.';
                input.setCustomValidity(value && !valid ? 'Use at least 8 characters including a letter, number and symbol (maximum 72 UTF-8 bytes).' : '');
            }
            input.addEventListener('input', update);
            input.addEventListener('change', update);
            if (input.form) input.form.addEventListener('submit', update);
            update();
        });
    }
    init();
    if (window.Sys && Sys.Application) Sys.Application.add_load(init);
}());
