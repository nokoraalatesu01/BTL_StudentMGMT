/**
 * UTC Portal Custom Combobox / Select Enhancer
 * Tự động biến đổi tất cả thẻ <select class="form-select"> thành combobox hiện đại:
 * - Menu thả xuống bo tròn, bóng đổ mịn, có icon checkmark
 * - Hỗ trợ tìm kiếm nhanh nếu danh sách có từ 6 lựa chọn trở lên
 * - Điều hướng bàn phím mượt mà (Phím mũi tên Lên/Xuống, Enter để chọn, Escape để đóng)
 * - Giữ nguyên 100% logic form submit, validation, sự kiện change/input
 * - Tự động đồng bộ khi thay đổi bằng JavaScript hoặc AJAX (MutationObserver)
 */
(function () {
    'use strict';

    function initPortalCustomSelects(root) {
        root = root || document;
        var selects = root.querySelectorAll('select.form-select:not(.no-custom-select):not([multiple])');

        selects.forEach(function (select) {
            if (select.dataset.portalSelectInit === 'true') {
                if (select._portalSelectRefresh) {
                    select._portalSelectRefresh();
                }
                return;
            }

            select.dataset.portalSelectInit = 'true';

            // 1. Tạo Wrapper
            var wrapper = document.createElement('div');
            wrapper.className = 'portal-select-wrapper';

            // Sao chép các thuộc tính kích thước hoặc class phù hợp từ thẻ select gốc
            if (select.style.width) {
                wrapper.style.width = select.style.width;
            } else if (select.classList.contains('w-100') || (select.parentElement && (select.parentElement.style.flex || select.parentElement.classList.contains('flex-fill')))) {
                wrapper.style.width = '100%';
                wrapper.style.flex = '1 1 auto';
                wrapper.classList.add('w-100');
            } else {
                wrapper.style.width = '100%';
            }

            if (select.style.minWidth) wrapper.style.minWidth = select.style.minWidth;
            if (select.style.maxWidth) wrapper.style.maxWidth = select.style.maxWidth;
            if (select.style.flex) wrapper.style.flex = select.style.flex;

            // 2. Ẩn select gốc an toàn
            select.classList.add('portal-select-native');
            select.parentNode.insertBefore(wrapper, select);
            wrapper.appendChild(select);

            // 3. Trigger button (Hiển thị giá trị đang chọn và mũi tên)
            var trigger = document.createElement('button');
            trigger.type = 'button';
            trigger.className = 'portal-select-trigger';
            trigger.setAttribute('aria-haspopup', 'listbox');
            trigger.setAttribute('aria-expanded', 'false');

            // Kế thừa các thuộc tính hiển thị (như padding cho icon absolute)
            if (select.style.paddingLeft) trigger.style.paddingLeft = select.style.paddingLeft;
            if (select.style.paddingRight) trigger.style.paddingRight = select.style.paddingRight;
            if (select.style.fontSize) trigger.style.fontSize = select.style.fontSize;
            if (select.style.borderColor) trigger.style.borderColor = select.style.borderColor;
            if (select.style.color) trigger.style.color = select.style.color;

            var label = document.createElement('span');
            label.className = 'portal-select-label';

            var arrow = document.createElement('span');
            arrow.className = 'portal-select-arrow';
            arrow.innerHTML = '<i class="bi bi-chevron-down"></i>';

            trigger.appendChild(label);
            trigger.appendChild(arrow);
            wrapper.appendChild(trigger);

            // 4. Dropdown menu nổi
            var dropdown = document.createElement('div');
            dropdown.className = 'portal-select-dropdown';
            dropdown.setAttribute('role', 'listbox');

            var searchInput = null;
            var optionsContainer = document.createElement('div');
            optionsContainer.className = 'portal-select-options';

            var emptyMsg = document.createElement('div');
            emptyMsg.className = 'portal-select-empty';
            emptyMsg.innerText = 'Không tìm thấy kết quả';
            emptyMsg.style.display = 'none';

            dropdown.appendChild(optionsContainer);
            dropdown.appendChild(emptyMsg);
            wrapper.appendChild(dropdown);

            // 5. Hàm dựng lại danh sách Options
            function buildOptions() {
                optionsContainer.innerHTML = '';
                var opts = Array.from(select.options);

                // Nếu có trên 6 lựa chọn thì hiển thị ô tìm kiếm nhanh
                if (opts.length >= 6) {
                    if (!searchInput) {
                        var searchWrap = document.createElement('div');
                        searchWrap.className = 'portal-select-search-wrap';
                        searchInput = document.createElement('input');
                        searchInput.type = 'text';
                        searchInput.className = 'portal-select-search-input';
                        searchInput.placeholder = 'Tìm kiếm nhanh...';
                        searchInput.autocomplete = 'off';
                        searchWrap.appendChild(searchInput);
                        dropdown.insertBefore(searchWrap, dropdown.firstChild);

                        searchInput.addEventListener('input', function () {
                            var q = searchInput.value.trim().toLowerCase();
                            var visibleCount = 0;
                            var items = optionsContainer.querySelectorAll('.portal-select-option');
                            items.forEach(function (item) {
                                var text = item.textContent.toLowerCase();
                                if (!q || text.indexOf(q) !== -1) {
                                    item.style.display = 'flex';
                                    visibleCount++;
                                } else {
                                    item.style.display = 'none';
                                }
                            });
                            emptyMsg.style.display = visibleCount === 0 ? 'block' : 'none';
                        });

                        searchWrap.addEventListener('click', function (e) {
                            e.stopPropagation();
                        });
                    }
                } else if (searchInput && searchInput.parentNode) {
                    searchInput.parentNode.remove();
                    searchInput = null;
                }

                var selectedOpt = select.options[select.selectedIndex] || opts[0];
                updateTriggerLabel(selectedOpt);

                opts.forEach(function (opt, idx) {
                    var item = document.createElement('div');
                    item.className = 'portal-select-option';
                    item.setAttribute('role', 'option');
                    item.dataset.value = opt.value;
                    item.dataset.index = idx;

                    if (opt.value === '' || opt.text.trim().startsWith('--')) {
                        item.classList.add('is-placeholder');
                    }

                    if (opt.selected) {
                        item.classList.add('is-selected');
                        item.setAttribute('aria-selected', 'true');
                    } else {
                        item.setAttribute('aria-selected', 'false');
                    }

                    if (opt.disabled) {
                        item.classList.add('is-disabled');
                    }

                    var textSpan = document.createElement('span');
                    textSpan.textContent = opt.text;

                    var checkSpan = document.createElement('span');
                    checkSpan.className = 'portal-option-check';
                    checkSpan.innerHTML = '<i class="bi bi-check2"></i>';

                    item.appendChild(textSpan);
                    item.appendChild(checkSpan);

                    item.addEventListener('click', function (e) {
                        e.stopPropagation();
                        if (opt.disabled) return;
                        selectOption(opt, item);
                    });

                    optionsContainer.appendChild(item);
                });
            }

            function selectOption(opt, itemEl) {
                select.selectedIndex = parseInt(itemEl.dataset.index, 10);
                select.value = opt.value;
                updateTriggerLabel(opt);

                optionsContainer.querySelectorAll('.portal-select-option').forEach(function (el) {
                    el.classList.remove('is-selected');
                    el.classList.remove('is-highlighted');
                    el.setAttribute('aria-selected', 'false');
                });
                itemEl.classList.add('is-selected');
                itemEl.setAttribute('aria-selected', 'true');

                closeDropdown();

                select.dispatchEvent(new Event('change', { bubbles: true }));
                select.dispatchEvent(new Event('input', { bubbles: true }));
            }

            function updateTriggerLabel(opt) {
                if (!opt) {
                    label.textContent = '';
                    return;
                }
                label.textContent = opt.text;
                if (opt.value === '' || opt.text.trim().startsWith('--')) {
                    label.classList.add('is-placeholder');
                } else {
                    label.classList.remove('is-placeholder');
                }
            }

            function openDropdown() {
                // Đóng tất cả combobox khác đang mở
                document.querySelectorAll('.portal-select-wrapper.is-open').forEach(function (other) {
                    if (other !== wrapper) other.classList.remove('is-open');
                });

                // Kiểm tra khoảng cách dưới để quyết định mở lên hay mở xuống
                var rect = trigger.getBoundingClientRect();
                var spaceBelow = window.innerHeight - rect.bottom;
                if (spaceBelow < 280 && rect.top > spaceBelow) {
                    dropdown.classList.add('open-up');
                } else {
                    dropdown.classList.remove('open-up');
                }

                wrapper.classList.add('is-open');
                trigger.setAttribute('aria-expanded', 'true');

                if (searchInput) {
                    searchInput.value = '';
                    searchInput.dispatchEvent(new Event('input'));
                    setTimeout(function () {
                        searchInput.focus();
                    }, 50);
                }

                var activeItem = optionsContainer.querySelector('.portal-select-option.is-selected');
                if (activeItem) {
                    activeItem.scrollIntoView({ block: 'nearest' });
                }
            }

            function closeDropdown() {
                wrapper.classList.remove('is-open');
                trigger.setAttribute('aria-expanded', 'false');
                optionsContainer.querySelectorAll('.portal-select-option.is-highlighted').forEach(function (el) {
                    el.classList.remove('is-highlighted');
                });
            }

            // Click Trigger
            trigger.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                if (wrapper.classList.contains('is-open')) {
                    closeDropdown();
                } else {
                    openDropdown();
                }
            });

            // Điều hướng bàn phím trên wrapper
            wrapper.addEventListener('keydown', function (e) {
                var isOpen = wrapper.classList.contains('is-open');

                if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
                    e.preventDefault();
                    if (!isOpen) {
                        openDropdown();
                        return;
                    }

                    var visibleItems = Array.from(optionsContainer.querySelectorAll('.portal-select-option')).filter(function (el) {
                        return el.style.display !== 'none' && !el.classList.contains('is-disabled');
                    });
                    if (visibleItems.length === 0) return;

                    var currentHighlighted = optionsContainer.querySelector('.portal-select-option.is-highlighted')
                        || optionsContainer.querySelector('.portal-select-option.is-selected');
                    var currentIndex = visibleItems.indexOf(currentHighlighted);

                    if (e.key === 'ArrowDown') {
                        currentIndex = (currentIndex + 1) % visibleItems.length;
                    } else {
                        currentIndex = (currentIndex - 1 + visibleItems.length) % visibleItems.length;
                    }

                    visibleItems.forEach(function (el) { el.classList.remove('is-highlighted'); });
                    var nextItem = visibleItems[currentIndex];
                    if (nextItem) {
                        nextItem.classList.add('is-highlighted');
                        nextItem.scrollIntoView({ block: 'nearest' });
                    }
                } else if (e.key === 'Enter') {
                    if (isOpen) {
                        e.preventDefault();
                        var highlighted = optionsContainer.querySelector('.portal-select-option.is-highlighted')
                            || optionsContainer.querySelector('.portal-select-option.is-selected');
                        if (highlighted) {
                            var idx = parseInt(highlighted.dataset.index, 10);
                            selectOption(select.options[idx], highlighted);
                        }
                    }
                } else if (e.key === 'Escape') {
                    if (isOpen) {
                        e.preventDefault();
                        closeDropdown();
                        trigger.focus();
                    }
                }
            });

            // Lắng nghe thay đổi từ JavaScript hoặc reset
            select.addEventListener('change', function () {
                var currentOpt = select.options[select.selectedIndex];
                updateTriggerLabel(currentOpt);
                optionsContainer.querySelectorAll('.portal-select-option').forEach(function (el) {
                    var isSel = el.dataset.value === select.value;
                    el.classList.toggle('is-selected', isSel);
                    el.setAttribute('aria-selected', isSel ? 'true' : 'false');
                });
            });

            buildOptions();
            select._portalSelectRefresh = buildOptions;

            // MutationObserver tự cập nhật khi danh sách options thay đổi động
            var observer = new MutationObserver(function () {
                buildOptions();
            });
            observer.observe(select, { childList: true, subtree: true });
        });
    }

    // Đóng dropdown khi click bên ngoài
    document.addEventListener('click', function (e) {
        if (!e.target.closest('.portal-select-wrapper')) {
            document.querySelectorAll('.portal-select-wrapper.is-open').forEach(function (w) {
                w.classList.remove('is-open');
                var trig = w.querySelector('.portal-select-trigger');
                if (trig) trig.setAttribute('aria-expanded', 'false');
            });
        }
    });

    // Tự động khôi phục khi form reset
    document.addEventListener('reset', function (e) {
        setTimeout(function () {
            var form = e.target;
            if (form && form.querySelectorAll) {
                form.querySelectorAll('select.form-select').forEach(function (s) {
                    s.dispatchEvent(new Event('change', { bubbles: true }));
                });
            }
        }, 50);
    });

    // Khởi chạy khi DOM sẵn sàng
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            initPortalCustomSelects();
        });
    } else {
        initPortalCustomSelects();
    }

    window.initPortalCustomSelects = initPortalCustomSelects;
})();
