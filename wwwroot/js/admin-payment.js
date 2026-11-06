/**
 * UTC Admin Payment Management - GSAP Animation & UI Controller
 * Follows GSAP Animation Rules: Lifecycle teardown, gsap.context(),
 * GPU transforms, autoAlpha, timeline positioning, and prefers-reduced-motion.
 */

(function () {
    'use strict';

    // Global cleanup reference
    let paymentCtx = null;

    function initPaymentAnimations() {
        if (typeof gsap === 'undefined') {
            console.warn('[AdminPayment] GSAP not found, skipping animations.');
            return;
        }

        // Register ScrollTrigger if available
        if (typeof ScrollTrigger !== 'undefined') {
            gsap.registerPlugin(ScrollTrigger);
        }

        const rootContainer = document.querySelector('.payment-dashboard-wrap');
        if (!rootContainer) return;

        // Clean up any existing context before re-initializing
        if (paymentCtx) {
            paymentCtx.revert();
        }

        // Wrap everything inside gsap.context() scoped to rootContainer
        paymentCtx = gsap.context(() => {
            const mm = gsap.matchMedia();

            // Accessibility: Handle reduced motion
            mm.add("(prefers-reduced-motion: reduce)", () => {
                // If user prefers reduced motion, set elements visible immediately without movement
                gsap.set('.payment-header-anim, .payment-tabs-container, .payment-filter-card, .payment-stat-card, .payment-card, .payment-table tbody tr', {
                    autoAlpha: 1,
                    y: 0,
                    scale: 1
                });
            });

            // Normal motion enabled
            mm.add("(prefers-reduced-motion: no-preference)", () => {
                const tl = gsap.timeline({
                    defaults: {
                        duration: 0.45,
                        ease: "power2.out"
                    }
                });

                // 1. Header & Tabs Entrance
                tl.fromTo('.payment-header-anim', 
                    { autoAlpha: 0, y: -12 }, 
                    { autoAlpha: 1, y: 0, duration: 0.4 }
                )
                .fromTo('.payment-tabs-container', 
                    { autoAlpha: 0, y: -10 }, 
                    { autoAlpha: 1, y: 0, duration: 0.35 }, 
                    "-=0.2"
                )
                // 2. Filter Bar
                .fromTo('.payment-filter-card', 
                    { autoAlpha: 0, y: 12 }, 
                    { autoAlpha: 1, y: 0, duration: 0.4 }, 
                    "-=0.2"
                );

                // 3. Stat Cards Entrance (Stagger with back.out)
                const statCards = gsap.utils.toArray('.payment-stat-card');
                if (statCards.length > 0) {
                    tl.fromTo(statCards, 
                        { autoAlpha: 0, y: 18, scale: 0.96 }, 
                        { 
                            autoAlpha: 1, 
                            y: 0, 
                            scale: 1, 
                            stagger: 0.06, 
                            ease: "back.out(1.3)",
                            duration: 0.5 
                        }, 
                        "-=0.2"
                    );

                    // Animated Counter for Stat Values
                    statCards.forEach(card => {
                        const counterEl = card.querySelector('[data-counter-target]');
                        if (counterEl) {
                            const rawTarget = parseFloat(counterEl.getAttribute('data-counter-target')) || 0;
                            const isCurrency = counterEl.getAttribute('data-is-currency') === 'true';
                            const counterObj = { val: 0 };

                            gsap.to(counterObj, {
                                val: rawTarget,
                                duration: 1.2,
                                ease: "power2.out",
                                delay: 0.25,
                                onUpdate: () => {
                                    if (isCurrency) {
                                        counterEl.innerHTML = Math.round(counterObj.val).toLocaleString('vi-VN') + '<span class="stat-currency">₫</span>';
                                    } else {
                                        counterEl.textContent = Math.round(counterObj.val).toLocaleString('vi-VN');
                                    }
                                },
                                onComplete: () => {
                                    if (isCurrency) {
                                        counterEl.innerHTML = Math.round(rawTarget).toLocaleString('vi-VN') + '<span class="stat-currency">₫</span>';
                                    } else {
                                        counterEl.textContent = Math.round(rawTarget).toLocaleString('vi-VN');
                                    }
                                }
                            });
                        }
                    });
                }

                // 4. Chart / Main Content Cards Entrance
                const contentCards = gsap.utils.toArray('.payment-card');
                if (contentCards.length > 0) {
                    tl.fromTo(contentCards, 
                        { autoAlpha: 0, y: 16, scale: 0.98 }, 
                        { 
                            autoAlpha: 1, 
                            y: 0, 
                            scale: 1, 
                            stagger: 0.08, 
                            duration: 0.45 
                        }, 
                        "-=0.25"
                    );
                }

                // 5. Table Rows Stagger Reveal
                const tableRows = gsap.utils.toArray('.payment-table tbody tr');
                if (tableRows.length > 0) {
                    tl.fromTo(tableRows, 
                        { autoAlpha: 0, y: 10 }, 
                        { 
                            autoAlpha: 1, 
                            y: 0, 
                            stagger: 0.025, 
                            duration: 0.35 
                        }, 
                        "-=0.2"
                    );
                }

                // Micro-interactions: Card Hover Effects
                statCards.forEach(card => {
                    card.addEventListener('mouseenter', () => {
                        gsap.to(card, { y: -4, duration: 0.2, ease: "power1.out" });
                    });
                    card.addEventListener('mouseleave', () => {
                        gsap.to(card, { y: 0, duration: 0.25, ease: "power1.out" });
                    });
                });

                // Micro-interactions: VCB Ticket Voucher entrance (if present)
                const vcbTicket = document.querySelector('.vcb-ticket-container');
                if (vcbTicket) {
                    gsap.fromTo(vcbTicket,
                        { autoAlpha: 0, scale: 0.92, y: 20 },
                        { autoAlpha: 1, scale: 1, y: 0, duration: 0.6, ease: "back.out(1.4)", delay: 0.2 }
                    );
                }
            });

        }, rootContainer);
    }

    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initPaymentAnimations);
    } else {
        initPaymentAnimations();
    }

    // Expose for external calls (e.g. after AJAX or filter refresh)
    window.AdminPayment = {
        refresh: initPaymentAnimations,
        revert: () => {
            if (paymentCtx) paymentCtx.revert();
        }
    };
})();
