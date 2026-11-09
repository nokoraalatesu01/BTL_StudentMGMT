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
            var animTargets = '.student-topbar, .student-welcome, .page-header, .student-page-heading, .student-stat-card, .stat-card, .payment-stat-card, .tkb-stat-card, .student-panel, .content-card, .chart-card, .gv-action-row, .student-table tbody tr, .custom-table tbody tr, .gv-table tbody tr, .admin-content .table tbody tr';

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
                var statCards = gsap.utils.toArray('.student-stat-card, .stat-card, .payment-stat-card, .tkb-stat-card');

                try {
                    // Timeline mở màn trang Dashboard
                    tl = gsap.timeline({
                        defaults: { duration: 0.55, ease: "power3.out" },
                        onComplete: function () {
                            clearAnimatedProps();
                        }
                    });

                    // Topbar & Welcome / Page Header
                    if (document.querySelector('.student-topbar')) {
                        tl.from('.student-topbar', { y: -15, autoAlpha: 0, duration: 0.45, clearProps: "transform,opacity,visibility" }, 0);
                    }

                    var headerEl = document.querySelector('.student-welcome, .student-page-heading, .page-header');
                    if (headerEl) {
                        tl.from(headerEl, { y: -18, autoAlpha: 0, duration: 0.55, clearProps: "transform,opacity,visibility" }, 0.08);
                    }

                    // Staggered Stat Cards
                    if (statCards.length > 0) {
                        tl.from(statCards, {
                            y: 18,
                            autoAlpha: 0,
                            duration: 0.48,
                            stagger: 0.05,
                            ease: "power2.out",
                            clearProps: "transform,opacity,visibility"
                        }, 0.15);
                    }

                    // Staggered Action Rows (Giảng viên)
                    var actionRows = gsap.utils.toArray('.gv-action-row');
                    if (actionRows.length > 0) {
                        tl.from(actionRows, {
                            y: 14,
                            autoAlpha: 0,
                            duration: 0.45,
                            stagger: 0.05,
                            ease: "power2.out",
                            clearProps: "transform,opacity,visibility"
                        }, 0.28);
                    }

                    // Staggered Panels & Content Cards
                    var panels = gsap.utils.toArray('.student-panel, .content-card, .chart-card');
                    if (panels.length > 0) {
                        tl.from(panels, {
                            y: 20,
                            autoAlpha: 0,
                            duration: 0.5,
                            stagger: 0.08,
                            ease: "power2.out",
                            clearProps: "transform,opacity,visibility"
                        }, 0.25);
                    }

                    // Horizontal Progress Chart Bars (Admin Dashboard)
                    var chartBars = gsap.utils.toArray('.admin-content .bar-row .bar, .chart-card .bar-row .bar');
                    if (chartBars.length > 0) {
                        tl.from(chartBars, {
                            scaleX: 0,
                            transformOrigin: "left center",
                            duration: 0.85,
                            stagger: 0.05,
                            ease: "power3.out",
                            clearProps: "transform"
                        }, 0.35);
                    }

                    // Table rows stagger (giới hạn 15 dòng đầu để cực kỳ mượt mà)
                    var rows = gsap.utils.toArray('.student-table tbody tr, .custom-table tbody tr, .gv-table tbody tr, .admin-content .table tbody tr');
                    if (rows.length > 0) {
                        var firstRows = rows.slice(0, 15);
                        tl.from(firstRows, {
                            y: 12,
                            autoAlpha: 0,
                            duration: 0.35,
                            stagger: 0.025,
                            ease: "power1.out",
                            clearProps: "transform,opacity,visibility"
                        }, 0.38);
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

                // (iii) setTimeout an toàn khoảng 1.8 giây sau khi khởi tạo để ép tl.progress(1) nếu chưa xong
                var safeTimer = setTimeout(function () {
                    if (tl && tl.progress() < 1) {
                        tl.progress(1);
                        clearAnimatedProps();
                    }
                }, 1800);

                // Universal Counter Function (GSAP rolling number)
                function animateNumber(el, targetVal, isDecimal) {
                    var counter = { val: 0 };
                    gsap.to(counter, {
                        val: targetVal,
                        duration: 1.1,
                        ease: "power2.out",
                        delay: 0.22,
                        onUpdate: function () {
                            if (isDecimal) {
                                el.innerText = counter.val.toFixed(2);
                            } else {
                                el.innerText = Math.round(counter.val).toLocaleString('vi-VN');
                            }
                        }
                    });
                }

                // 1. Animated GPA Counter (Sinh viên)
                statCards.forEach(function (card) {
                    var gpaEl = card.querySelector('.stat-value-gpa');
                    if (gpaEl && gpaEl.childNodes.length === 1 && gpaEl.childNodes[0].nodeType === 3) {
                        var text = gpaEl.innerText.trim();
                        var numMatch = text.match(/^(\d+(\.\d+)?)$/);
                        if (numMatch) {
                            animateNumber(gpaEl, parseFloat(numMatch[1]), true);
                        }
                    }
                });

                // 2. Animated Metric Counters (Giảng viên: [data-count])
                document.querySelectorAll('[data-count]').forEach(function (el) {
                    var raw = el.getAttribute('data-count');
                    var count = parseInt(raw, 10);
                    if (!isNaN(count) && count > 0) {
                        animateNumber(el, count, false);
                    }
                });

                // 3. Animated Metric Counters (Admin: .stat-card .num)
                document.querySelectorAll('.stat-card .num').forEach(function (el) {
                    if (el.childNodes.length === 1 && el.childNodes[0].nodeType === 3) {
                        var rawText = el.innerText.trim().replace(/[.,]/g, '');
                        var count = parseInt(rawText, 10);
                        if (!isNaN(count) && count > 0) {
                            animateNumber(el, count, false);
                        }
                    }
                });

                // Micro-interactions trên Icon của Stat Cards
                statCards.forEach(function (card) {
                    var icon = card.querySelector('.student-stat-icon, .stat-ico');
                    if (icon) {
                        card.addEventListener('mouseenter', function () {
                            gsap.to(icon, { scale: 1.15, rotation: 5, duration: 0.28, ease: "back.out(2)", overwrite: "auto" });
                        });
                        card.addEventListener('mouseleave', function () {
                            gsap.to(icon, { scale: 1, rotation: 0, duration: 0.22, ease: "power2.out", clearProps: "transform", overwrite: "auto" });
                        });
                    }
                });

                // Micro-interactions trên Action Rows (Giảng viên)
                actionRows.forEach(function (row) {
                    var arrow = row.querySelector('.gv-action-arrow');
                    if (arrow) {
                        row.addEventListener('mouseenter', function () {
                            gsap.to(arrow, { x: 5, duration: 0.22, ease: "power2.out", overwrite: "auto" });
                        });
                        row.addEventListener('mouseleave', function () {
                            gsap.to(arrow, { x: 0, duration: 0.2, ease: "power2.in", clearProps: "transform", overwrite: "auto" });
                        });
                    }
                });

                // Micro-interactions trên Sidebar Nav Links
                var navLinks = gsap.utils.toArray('.student-nav a');
                navLinks.forEach(function (link) {
                    var symbol = link.querySelector('.nav-symbol');
                    link.addEventListener('mouseenter', function () {
                        if (symbol) {
                            gsap.to(symbol, { scale: 1.2, rotation: -6, duration: 0.25, ease: "back.out(2)", overwrite: "auto" });
                        }
                    });
                    link.addEventListener('mouseleave', function () {
                        if (symbol) {
                            gsap.to(symbol, { scale: 1, rotation: 0, duration: 0.2, ease: "power1.out", clearProps: "transform", overwrite: "auto" });
                        }
                    });
                });

                // Logo hover effect
                var logoWrap = document.querySelector('.student-brand-logo-wrap');
                if (logoWrap) {
                    logoWrap.addEventListener('mouseenter', function () {
                        gsap.to(logoWrap, { rotation: 360, duration: 0.8, ease: "back.out(1.5)", overwrite: "auto" });
                    });
                    logoWrap.addEventListener('mouseleave', function () {
                        gsap.to(logoWrap, { rotation: 0, duration: 0.4, ease: "power2.out", clearProps: "transform", overwrite: "auto" });
                    });
                }

                // In-View Entrance cho các phần tử xuất hiện khi cuộn trang (Scroll Triggering via IntersectionObserver)
                if ('IntersectionObserver' in window) {
                    var offscreenTargets = document.querySelectorAll('.student-panel, .content-card, .custom-table-container, .student-table-wrap, .gv-table-responsive');
                    var scrollObserver = new IntersectionObserver(function (entries) {
                        entries.forEach(function (entry) {
                            if (entry.isIntersecting) {
                                var target = entry.target;
                                scrollObserver.unobserve(target);
                                gsap.from(target, {
                                    y: 20,
                                    autoAlpha: 0,
                                    duration: 0.5,
                                    ease: "power2.out",
                                    clearProps: "transform,opacity,visibility"
                                });
                            }
                        });
                    }, { threshold: 0.08 });

                    offscreenTargets.forEach(function (target) {
                        var rect = target.getBoundingClientRect();
                        if (rect.top > window.innerHeight) {
                            scrollObserver.observe(target);
                        }
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
