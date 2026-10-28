(() => {
    const notificationDuration = 3000;
    document.querySelectorAll('[data-transient-notification]').forEach((container) => {
        const dismiss = () => container.remove();
        const timer = window.setTimeout(dismiss, notificationDuration);
        container.querySelector('[data-bs-dismiss="alert"]')?.addEventListener('click', (event) => {
            event.preventDefault();
            event.stopPropagation();
            window.clearTimeout(timer);
            dismiss();
        });
    });

    // Dùng thông báo tiếng Việt cho xác thực HTML, kể cả các dòng thêm động.
    document.addEventListener('invalid', (event) => {
        const field = event.target;
        if (typeof field.setCustomValidity !== 'function') return;
        field.setCustomValidity('');
        const validity = field.validity;
        if (validity.valueMissing) field.setCustomValidity('Vui lòng nhập hoặc chọn thông tin bắt buộc.');
        else if (validity.typeMismatch && field.type === 'email') field.setCustomValidity('Email chưa đúng định dạng.');
        else if (validity.badInput) field.setCustomValidity('Vui lòng nhập một số hợp lệ.');
        else if (validity.rangeUnderflow) field.setCustomValidity(`Giá trị phải lớn hơn hoặc bằng ${field.min}.`);
        else if (validity.rangeOverflow) field.setCustomValidity(`Giá trị phải nhỏ hơn hoặc bằng ${field.max}.`);
        else if (validity.tooShort) field.setCustomValidity(`Vui lòng nhập ít nhất ${field.minLength} ký tự.`);
        else if (validity.tooLong) field.setCustomValidity(`Không được nhập quá ${field.maxLength} ký tự.`);
        else if (validity.stepMismatch) field.setCustomValidity('Giá trị nhập chưa đúng bước tăng cho phép.');
        else if (validity.patternMismatch) field.setCustomValidity('Thông tin nhập chưa đúng định dạng.');
    }, true);
    for (const eventName of ['input', 'change']) {
        document.addEventListener(eventName, (event) => {
            if (typeof event.target.setCustomValidity === 'function') event.target.setCustomValidity('');
        });
    }
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    let favoriteNotificationTimer;

    const showFavoriteNotification = (message) => {
        const notification = document.getElementById('favorite-notification');
        if (!notification) return;
        notification.textContent = message;
        notification.classList.add('is-visible');
        window.clearTimeout(favoriteNotificationTimer);
        favoriteNotificationTimer = window.setTimeout(() => {
            notification.classList.remove('is-visible');
            notification.textContent = '';
        }, notificationDuration);
    };

    document.addEventListener('click', async (event) => {
        const button = event.target.closest('[data-favorite-toggle]');
        if (!button) return;
        button.disabled = true;
        try {
            const response = await fetch('/Favorite/Toggle', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8',
                    'RequestVerificationToken': token ?? '',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: new URLSearchParams({ recipeId: button.dataset.recipeId })
            });
            if (response.status === 401) {
                const returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
                window.location.href = `/Account/Login?returnUrl=${returnUrl}`;
                return;
            }
            if (!response.ok) throw new Error('Không thể cập nhật yêu thích.');
            const result = await response.json();
            button.classList.toggle('is-favorite', result.isFavorite);
            button.setAttribute('aria-pressed', String(result.isFavorite));
            button.setAttribute('aria-label', result.isFavorite ? 'Bỏ khỏi yêu thích' : 'Lưu yêu thích');
            const detailLabel = button.querySelector('span');
            if (detailLabel) detailLabel.textContent = result.isFavorite ? 'Đã yêu thích' : 'Yêu thích';
            showFavoriteNotification(result.isFavorite ? 'Đã thêm công thức vào yêu thích.' : 'Đã bỏ công thức khỏi yêu thích.');
            if (!result.isFavorite && window.location.pathname.toLowerCase().endsWith('/favorites')) {
                button.closest('.col-sm-6')?.remove();
                const grid = document.querySelector('[data-favorites-grid]');
                if (grid && grid.children.length === 0) {
                    grid.hidden = true;
                    const empty = document.querySelector('[data-favorites-empty]');
                    if (empty) empty.hidden = false;
                }
            }
        } catch {
            showFavoriteNotification('Chưa thể cập nhật yêu thích. Vui lòng thử lại.');
        } finally {
            button.disabled = false;
        }
    });

    document.querySelectorAll('[data-add-row]').forEach((button) => {
        button.addEventListener('click', () => {
            const target = document.getElementById(button.dataset.addRow);
            if (!target) return;
            const isIngredient = target.id === 'ingredients';
            const index = target.children.length;
            const row = document.createElement('div');
            row.className = `dynamic-row ${isIngredient ? 'ingredient-row' : 'step-row'}`;
            if (isIngredient) {
                row.innerHTML = `<div><label class="visually-hidden" for="NguyenLieus_${index}__TenNguyenLieu">Tên nguyên liệu</label><input id="NguyenLieus_${index}__TenNguyenLieu" name="NguyenLieus[${index}].TenNguyenLieu" class="form-control" placeholder="Tên nguyên liệu" maxlength="120" required /></div><div><label class="visually-hidden" for="NguyenLieus_${index}__DinhLuong">Định lượng</label><input id="NguyenLieus_${index}__DinhLuong" name="NguyenLieus[${index}].DinhLuong" class="form-control" placeholder="Ví dụ: 200 g" maxlength="100" required /></div><button class="remove-row" type="button" aria-label="Xóa nguyên liệu">×</button>`;
            } else {
                row.innerHTML = `<span class="step-number">${String(index + 1).padStart(2, '0')}</span><div><label class="visually-hidden" for="BuocThucHiens_${index}__NoiDung">Nội dung bước ${index + 1}</label><textarea id="BuocThucHiens_${index}__NoiDung" name="BuocThucHiens[${index}].NoiDung" rows="2" class="form-control" placeholder="Mô tả bước ${index + 1}..." required></textarea></div><button class="remove-row" type="button" aria-label="Xóa bước">×</button>`;
            }
            target.appendChild(row);
        });
    });

    document.addEventListener('click', (event) => {
        const remove = event.target.closest('.remove-row');
        if (!remove) return;
        const list = remove.closest('.dynamic-rows');
        if (!list || list.children.length <= 1) return;
        remove.closest('.dynamic-row').remove();
        [...list.children].forEach((row, index) => {
            row.querySelectorAll('input, textarea').forEach((input) => {
                input.name = input.name.replace(/\[\d+\]/, `[${index}]`);
                input.id = input.id.replace(/_\d+__/, `_${index}__`);
            });
            row.querySelectorAll('label').forEach((label, labelIndex) => {
                const input = row.querySelectorAll('input, textarea')[labelIndex];
                if (input) label.htmlFor = input.id;
            });
            row.querySelectorAll('[data-valmsg-for]').forEach((message) => {
                message.dataset.valmsgFor = message.dataset.valmsgFor.replace(/\[\d+\]/, `[${index}]`);
            });
            const number = row.querySelector('.step-number');
            if (number && list.id === 'steps') number.textContent = String(index + 1).padStart(2, '0');
        });
    });
})();
