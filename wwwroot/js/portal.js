/**
 * Portal Animation & Interaction Engine
 * Sử dụng GSAP (Core, Timeline, Utils, ScrollTrigger) để tối ưu giao diện sinh động, mượt mà
 * Hỗ trợ Cổng Sinh viên, Giảng viên, Quản trị viên - Trường ĐH Giao thông Vận tải (UTC)
 */
(function () {
    'use strict';

    function initPortal() {
        var sidebar = document.getElementById('portal-sidebar');
        var scrim = document.querySelector('.sidebar-scrim');
        var toggleButtons = document.querySelectorAll('.sidebar-toggle');
        var isMobile = function () { return window.innerWidth <= 768; };

        // 1. Khởi tạo trạng thái Sidebar từ localStorage
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
        } catch (e) { }

        function updateAriaState(isOpen) {
            toggleButtons.forEach(function (btn) {
                btn.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
            });
        }

        // 2. Logic Toggle Sidebar kết hợp GSAP
        function toggleSidebar() {
            if (isMobile()) {
                var isOpen = document.body.classList.toggle('sidebar-mobile-open');
                updateAriaState(isOpen);
                if (window.gsap && sidebar) {
                    if (isOpen) {
                        gsap.fromTo(sidebar, 
                            { xPercent: -100, autoAlpha: 0 }, 
                            { xPercent: 0, autoAlpha: 1, duration: 0.35, ease: "power3.out" }
                        );
                        if (scrim) {
                            gsap.fromTo(scrim, { autoAlpha: 0 }, { autoAlpha: 1, duration: 0.25 });
                        }
                    }
                }
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
                if (window.gsap && sidebar) {
                    gsap.to(sidebar, {
                        xPercent: -100,
                        duration: 0.25,
                        ease: "power2.in",
                        onComplete: function () {
                            document.body.classList.remove('sidebar-mobile-open');
                            updateAriaState(false);
                        }
                    });
                    if (scrim) {
                        gsap.to(scrim, { autoAlpha: 0, duration: 0.2 });
                    }
                } else {
                    document.body.classList.remove('sidebar-mobile-open');
                    updateAriaState(false);
                }
            }
        }

        toggleButtons.forEach(function (btn) {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                toggleSidebar();
            });
        });

        if (scrim) {
            scrim.addEventListener('click', closeMobileSidebar);
        }

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                closeMobileSidebar();
            } else if ((e.ctrlKey && (e.key === 'b' || e.key === 'B')) || (e.altKey && (e.key === 'm' || e.key === 'M'))) {
                e.preventDefault();
                toggleSidebar();
            }
        });

        // 3. GSAP Animations: Choreography & Visual Enhancements
        if (window.gsap) {
            var mm = gsap.matchMedia();

            // Selector tập hợp các phần tử tham gia intro animation
            var animTargets = '.student-topbar, .student-welcome, .student-page-heading, .student-stat-card, .stat-card, .student-panel, .content-card, .chart-card, .student-table tbody tr, .custom-table tbody tr';

            function clearAnimatedProps() {
                try {
                    gsap.set(animTargets, { clearProps: "transform,opacity,visibility" });
                } catch (e) { }
            }

            mm.add("(prefers-reduced-motion: reduce)", function () {
                clearAnimatedProps();
            });

            mm.add("(prefers-reduced-motion: no-preference)", function () {
                var tl = null;
                var statCards = gsap.utils.toArray('.student-stat-card, .stat-card');

                try {
                    // Timeline mở màn trang Dashboard
                    tl = gsap.timeline({
                        defaults: { duration: 0.55, ease: "power3.out" },
                        onComplete: function () {
                            clearAnimatedProps();
                        }
                    });

                    // Topbar & Welcome
                    if (document.querySelector('.student-topbar')) {
                        tl.from('.student-topbar', { y: -15, autoAlpha: 0, duration: 0.45, clearProps: "transform,opacity,visibility" }, 0);
                    }

                    if (document.querySelector('.student-welcome')) {
                        tl.from('.student-welcome', { y: -20, autoAlpha: 0, duration: 0.6, clearProps: "transform,opacity,visibility" }, 0.1);
                    }

                    if (document.querySelector('.student-page-heading')) {
                        tl.from('.student-page-heading', { y: -20, autoAlpha: 0, duration: 0.55, clearProps: "transform,opacity,visibility" }, 0.1);
                    }

                    // Staggered Stat Cards
                    if (statCards.length > 0) {
                        tl.from(statCards, {
                            y: 16,
                            duration: 0.45,
                            stagger: 0.06,
                            ease: "power2.out",
                            clearProps: "transform,opacity,visibility"
                        }, 0.2);
                    }

                    // Staggered Panels
                    var panels = gsap.utils.toArray('.student-panel, .content-card, .chart-card');
                    if (panels.length > 0) {
                        tl.from(panels, {
                            y: 20,
                            duration: 0.5,
                            stagger: 0.1,
                            ease: "power2.out",
                            clearProps: "transform,opacity,visibility"
                        }, 0.35);
                    }

                    // Table rows stagger
                    var rows = gsap.utils.toArray('.student-table tbody tr, .custom-table tbody tr');
                    if (rows.length > 0) {
                        tl.from(rows, {
                            y: 15,
                            autoAlpha: 0,
                            duration: 0.35,
                            stagger: 0.03,
                            ease: "power1.out",
                            clearProps: "transform,opacity,visibility"
                        }, 0.45);
                    }
                } catch (err) {
                    clearAnimatedProps();
                }

                // Cơ chế chống kẹt:
                // (i) lắng nghe visibilitychange: khi trang hiển thị lại mà timeline chưa xong thì tl.progress(1) và xóa props
                var onVisibilityChange = function () {
                    if (!document.hidden && tl && tl.progress() < 1) {
                        tl.progress(1);
                        clearAnimatedProps();
                    }
                };
                document.addEventListener('visibilitychange', onVisibilityChange);

                // (ii) lắng nghe pageshow với event.persisted để đưa các phần tử về trạng thái cuối
                var onPageShow = function (event) {
                    if (event.persisted) {
                        if (tl && tl.progress() < 1) {
                            tl.progress(1);
                        }
                        clearAnimatedProps();
                    }
                };
                window.addEventListener('pageshow', onPageShow);

                // (iii) setTimeout an toàn khoảng 2 giây sau khi khởi tạo để ép tl.progress(1) nếu chưa xong
                var safeTimer = setTimeout(function () {
                    if (tl && tl.progress() < 1) {
                        tl.progress(1);
                        clearAnimatedProps();
                    }
                }, 2000);

                // Animated Number Counter cho GPA (chỉ khi có element riêng và là số thuần túy)
                statCards.forEach(function (card) {
                    var gpaEl = card.querySelector('.stat-value-gpa');
                    if (gpaEl && gpaEl.childNodes.length === 1 && gpaEl.childNodes[0].nodeType === 3) {
                        var text = gpaEl.innerText.trim();
                        var numMatch = text.match(/^(\d+(\.\d+)?)$/);
                        if (numMatch) {
                            var targetNum = parseFloat(numMatch[1]);
                            var counter = { val: 0 };
                            gsap.to(counter, {
                                val: targetNum,
                                duration: 1.0,
                                ease: "power2.out",
                                delay: 0.3,
                                onUpdate: function () {
                                    gpaEl.innerText = targetNum % 1 !== 0 ? counter.val.toFixed(2) : Math.round(counter.val);
                                }
                            });
                        }
                    }
                });

                // Micro-interactions trên Icon của Stat Cards (CSS đã xử lý hover card)
                statCards.forEach(function (card) {
                    var icon = card.querySelector('.student-stat-icon');
                    if (icon) {
                        card.addEventListener('mouseenter', function () {
                            gsap.to(icon, { scale: 1.12, duration: 0.25, ease: "back.out(2)" });
                        });
                        card.addEventListener('mouseleave', function () {
                            gsap.to(icon, { scale: 1, duration: 0.2, ease: "power2.out" });
                        });
                    }
                });

                // Micro-interactions trên Sidebar Nav Links
                var navLinks = gsap.utils.toArray('.student-nav a');
                navLinks.forEach(function (link) {
                    var symbol = link.querySelector('.nav-symbol');
                    link.addEventListener('mouseenter', function () {
                        if (symbol) {
                            gsap.to(symbol, { scale: 1.2, rotation: -6, duration: 0.25, ease: "back.out(2)" });
                        }
                    });
                    link.addEventListener('mouseleave', function () {
                        if (symbol) {
                            gsap.to(symbol, { scale: 1, rotation: 0, duration: 0.2, ease: "power1.out" });
                        }
                    });
                });

                // Logo hover effect
                var logoWrap = document.querySelector('.student-brand-logo-wrap');
                if (logoWrap) {
                    logoWrap.addEventListener('mouseenter', function () {
                        gsap.to(logoWrap, { rotation: 360, duration: 0.8, ease: "back.out(1.5)" });
                    });
                    logoWrap.addEventListener('mouseleave', function () {
                        gsap.to(logoWrap, { rotation: 0, duration: 0.4, ease: "power2.out" });
                    });
                }

                return function () {
                    document.removeEventListener('visibilitychange', onVisibilityChange);
                    window.removeEventListener('pageshow', onPageShow);
                    clearTimeout(safeTimer);
                };
            });
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initPortal);
    } else {
        initPortal();
    }
})();
