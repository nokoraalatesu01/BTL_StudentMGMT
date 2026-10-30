/**
 * Portal Layout Interaction Script
 * Quản lý ẩn/hiện Sidebar (Taskbar điều hướng) cho Sinh viên, Giảng viên, Quản trị viên
 */
(function () {
    'use strict';

    function initPortalSidebar() {
        var sidebar = document.getElementById('portal-sidebar');
        var scrim = document.querySelector('.sidebar-scrim');
        var toggleButtons = document.querySelectorAll('.sidebar-toggle');

        var isMobile = function () {
            return window.innerWidth <= 768;
        };

        function updateAriaState(isOpen) {
            toggleButtons.forEach(function (btn) {
                btn.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
            });
        }

        // Khởi tạo trạng thái ban đầu từ localStorage (chỉ áp dụng trên desktop)
        try {
            var storedCollapsed = localStorage.getItem('portal_sidebar_collapsed');
            if (storedCollapsed === 'true' && !isMobile()) {
                document.documentElement.classList.add('sidebar-collapsed');
                document.body.classList.add('sidebar-collapsed');
                updateAriaState(false);
            } else if (!isMobile()) {
                document.documentElement.classList.remove('sidebar-collapsed');
                document.body.classList.remove('sidebar-collapsed');
                updateAriaState(true);
            }
        } catch (e) {
            // LocalStorage might be disabled
        }

        function toggleSidebar() {
            if (isMobile()) {
                var isOpen = document.body.classList.toggle('sidebar-mobile-open');
                updateAriaState(isOpen);
            } else {
                var isCollapsed = document.body.classList.toggle('sidebar-collapsed');
                if (isCollapsed) {
                    document.documentElement.classList.add('sidebar-collapsed');
                } else {
                    document.documentElement.classList.remove('sidebar-collapsed');
                }
                try {
                    localStorage.setItem('portal_sidebar_collapsed', isCollapsed ? 'true' : 'false');
                } catch (e) { }
                updateAriaState(!isCollapsed);
            }
        }

        function closeMobileSidebar() {
            if (document.body.classList.contains('sidebar-mobile-open')) {
                document.body.classList.remove('sidebar-mobile-open');
                updateAriaState(false);
            }
        }

        // Lắng nghe sự kiện click trên tất cả nút toggle (cả trong taskbar lẫn topbar)
        toggleButtons.forEach(function (btn) {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                toggleSidebar();
            });
        });

        // Click vào lớp phủ mờ (scrim) trên thiết bị di động để đóng sidebar
        if (scrim) {
            scrim.addEventListener('click', function () {
                closeMobileSidebar();
            });
        }

        // Phím tắt: Esc để đóng menu mobile, Ctrl+B hoặc Alt+M để toggle
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                closeMobileSidebar();
            } else if ((e.ctrlKey && (e.key === 'b' || e.key === 'B')) || (e.altKey && (e.key === 'm' || e.key === 'M'))) {
                e.preventDefault();
                toggleSidebar();
            }
        });

        // Xử lý khi thay đổi kích thước cửa sổ
        var resizeTimer;
        window.addEventListener('resize', function () {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(function () {
                if (!isMobile()) {
                    closeMobileSidebar();
                    try {
                        var isCollapsed = localStorage.getItem('portal_sidebar_collapsed') === 'true';
                        if (isCollapsed) {
                            document.documentElement.classList.add('sidebar-collapsed');
                            document.body.classList.add('sidebar-collapsed');
                            updateAriaState(false);
                        } else {
                            document.documentElement.classList.remove('sidebar-collapsed');
                            document.body.classList.remove('sidebar-collapsed');
                            updateAriaState(true);
                        }
                    } catch (e) { }
                } else {
                    document.documentElement.classList.remove('sidebar-collapsed');
                    document.body.classList.remove('sidebar-collapsed');
                }
            }, 100);
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initPortalSidebar);
    } else {
        initPortalSidebar();
    }
})();
