/**
 * Login Page Interaction & Animation Engine
 * BTL_StudentMGMT - Trường Đại học Giao thông Vận tải (UTC)
 */
(function () {
    'use strict';

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', onReady);
    } else {
        onReady();
    }

    function onReady() {
        initPasswordToggle();
        initLoginAnimations();
    }

    /**
     * 1. Xử lý bật/tắt hiển thị mật khẩu
     */
    function initPasswordToggle() {
        var toggleButtons = document.querySelectorAll('.password-toggle');

        toggleButtons.forEach(function (button) {
            button.addEventListener('click', function (e) {
                e.preventDefault();

                var targetId = button.getAttribute('aria-controls') || 'Password';
                var input = document.getElementById(targetId) || button.closest('.input-wrap').querySelector('input');
                if (!input) return;

                var isShowing = input.type === 'text';
                var nextType = isShowing ? 'password' : 'text';

                // Thay đổi thuộc tính type của trường input mật khẩu
                input.type = nextType;

                // Cập nhật trạng thái nút và trợ năng (accessibility)
                button.setAttribute('aria-pressed', (!isShowing).toString());
                var label = isShowing ? 'Hiện mật khẩu' : 'Ẩn mật khẩu';
                button.setAttribute('aria-label', label);
                button.setAttribute('title', label);

                // Cập nhật icon mắt (hiển thị / ẩn)
                var eyeOpen = button.querySelector('.eye-open');
                var eyeClosed = button.querySelector('.eye-closed');

                if (eyeOpen && eyeClosed) {
                    if (isShowing) {
                        eyeOpen.style.display = 'block';
                        eyeClosed.style.display = 'none';
                    } else {
                        eyeOpen.style.display = 'none';
                        eyeClosed.style.display = 'block';
                    }
                }

                button.classList.toggle('active', !isShowing);

                // Giữ vị trí con trỏ ở cuối chuỗi văn bản
                try {
                    input.focus();
                    var len = input.value.length;
                    input.setSelectionRange(len, len);
                } catch (err) {}
            });
        });
    }

    /**
     * 2. Hoạt cảnh mở đầu (GSAP Entrance Animations)
     */
    function initLoginAnimations() {
        if (!window.gsap) return;

        var mm = gsap.matchMedia();
        mm.add("(prefers-reduced-motion: no-preference)", function () {
            var tl = gsap.timeline({ defaults: { duration: 0.5, ease: "power2.out" } });

            if (document.querySelector('.welcome-copy')) {
                tl.from('.brand-utc', { autoAlpha: 0, y: -12, duration: 0.45 })
                  .from('.welcome-copy .eyebrow', { autoAlpha: 0, y: 10, duration: 0.35 }, "-=0.2")
                  .from('.welcome-copy h1', { autoAlpha: 0, y: 12, duration: 0.45 }, "-=0.2")
                  .from('.welcome-description', { autoAlpha: 0, y: 10, duration: 0.35 }, "-=0.2")
                  .from('.welcome-note', { autoAlpha: 0, y: 12, duration: 0.45 }, "-=0.2");
            }

            if (document.querySelector('.login-card')) {
                gsap.from('.login-card', {
                    autoAlpha: 0,
                    y: 18,
                    duration: 0.55,
                    ease: "power2.out",
                    delay: 0.08
                });
            }
        });
    }
})();
