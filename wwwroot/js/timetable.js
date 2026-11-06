/**
 * UTC Timetable & Teaching Schedule UI Controller
 * Quản lý modal chi tiết buổi học, xem danh sách sinh viên, điểm danh nhanh và in ấn
 */

(function () {
    'use strict';

    // Global Modal Instance
    let detailModalInstance = null;

    function getModalInstance() {
        const modalEl = document.getElementById('modalChiTietBuoiHoc');
        if (!modalEl) return null;
        if (!detailModalInstance && typeof bootstrap !== 'undefined') {
            detailModalInstance = new bootstrap.Modal(modalEl);
        }
        return detailModalInstance;
    }

    /**
     * Mở modal chi tiết buổi học
     * @param {number} lopHocPhanId 
     * @param {string} ngayHoc ISO date string 'yyyy-MM-dd'
     * @param {string} role 'giangvien' | 'sinhvien'
     */
    window.xemChiTietBuoiHoc = function (lopHocPhanId, ngayHoc, role) {
        if (!lopHocPhanId) return;

        const modalEl = document.getElementById('modalChiTietBuoiHoc');
        if (!modalEl) return;

        const modal = getModalInstance();
        if (!modal) return;

        // Reset & show loading state
        const loadingSpinner = document.getElementById('tkbModalLoading');
        const modalContent = document.getElementById('tkbModalContent');
        if (loadingSpinner) loadingSpinner.style.display = 'block';
        if (modalContent) modalContent.style.display = 'none';

        modal.show();

        const endpoint = role === 'giangvien'
            ? `/GiangVien/ChiTietBuoiHoc?lopHocPhanId=${lopHocPhanId}&ngay=${encodeURIComponent(ngayHoc || '')}`
            : `/SinhVien/ChiTietBuoiHoc?lopHocPhanId=${lopHocPhanId}&ngay=${encodeURIComponent(ngayHoc || '')}`;

        fetch(endpoint, {
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
        .then(response => {
            if (!response.ok) throw new Error('Không thể tải thông tin buổi học');
            return response.json();
        })
        .then(res => {
            if (!res.success || !res.data) throw new Error(res.message || 'Lỗi dữ liệu');
            renderModalData(res.data, role, ngayHoc);
        })
        .catch(err => {
            if (loadingSpinner) loadingSpinner.style.display = 'none';
            if (modalContent) {
                modalContent.style.display = 'block';
                modalContent.innerHTML = `
                    <div class="alert alert-danger mb-0">
                        <i class="bi bi-exclamation-octagon-fill me-2"></i> ${err.message || 'Lỗi tải dữ liệu buổi học'}
                    </div>`;
            }
        });
    };

    function renderModalData(data, role, ngayHoc) {
        const loadingSpinner = document.getElementById('tkbModalLoading');
        const modalContent = document.getElementById('tkbModalContent');

        if (loadingSpinner) loadingSpinner.style.display = 'none';
        if (!modalContent) return;
        modalContent.style.display = 'block';

        // Header Title
        const modalTitle = document.getElementById('tkbModalTitle');
        if (modalTitle) {
            modalTitle.innerHTML = `<i class="bi bi-journal-text me-2 text-primary"></i> ${data.tenMon || data.tenLopHocPhan}`;
        }

        // Action button (Điểm danh nếu là giảng viên)
        const btnDiemDanh = document.getElementById('btnModalDiemDanh');
        if (btnDiemDanh) {
            if (role === 'giangvien') {
                btnDiemDanh.style.display = 'inline-flex';
                btnDiemDanh.href = `/GiangVien/DiemDanh?lopHocPhanId=${data.lopHocPhanId}&ngay=${encodeURIComponent(ngayHoc || '')}`;
            } else {
                btnDiemDanh.style.display = 'none';
            }
        }

        // Format ngày học
        let ngayHocFormatted = 'Chưa xác định';
        if (data.ngayHoc) {
            const d = new Date(data.ngayHoc);
            ngayHocFormatted = d.toLocaleDateString('vi-VN', { weekday: 'long', year: 'numeric', month: '2-digit', day: '2-digit' });
        }

        let html = `
            <div class="tkb-modal-meta-grid">
                <div class="tkb-modal-meta-item">
                    <span class="text-muted small">Mã học phần:</span>
                    <strong class="font-monospace text-primary">${data.maMon}</strong>
                </div>
                <div class="tkb-modal-meta-item">
                    <span class="text-muted small">Lớp học phần:</span>
                    <strong>${data.maLopHocPhan} (Nhóm ${data.nhomLop || '01'})</strong>
                </div>
                <div class="tkb-modal-meta-item">
                    <span class="text-muted small">Số tín chỉ:</span>
                    <strong>${data.soTinChi} tín chỉ</strong>
                </div>
                <div class="tkb-modal-meta-item">
                    <span class="text-muted small">Phòng học:</span>
                    <strong class="text-success"><i class="bi bi-geo-alt-fill me-1"></i>${data.phongHoc || 'Chưa xếp phòng'}</strong>
                </div>
                <div class="tkb-modal-meta-item">
                    <span class="text-muted small">Thời gian & Ca học:</span>
                    <div>
                        <strong>${data.thu}, ${data.tenCaHoc} &bull; ${data.khungGio} (Tiết ${data.tietBatDau}–${data.tietKetThuc})</strong>
                        ${data.ghiChuGiaiLao ? `<div class="mt-1"><span class="badge bg-warning-subtle text-dark border"><i class="bi bi-cup-hot me-1"></i>${data.ghiChuGiaiLao}</span></div>` : ''}
                    </div>
                </div>
                <div class="tkb-modal-meta-item">
                    <span class="text-muted small">Ngày học:</span>
                    <strong>${ngayHocFormatted}</strong>
                </div>
                <div class="tkb-modal-meta-item">
                    <span class="text-muted small">Giảng viên phụ trách:</span>
                    <strong>${data.giangVien || 'Chưa phân công'}</strong>
                    ${data.emailGiangVien ? `<small class="text-muted d-block">${data.emailGiangVien}</small>` : ''}
                </div>
                <div class="tkb-modal-meta-item">
                    <span class="text-muted small">Trạng thái buổi học:</span>
                    <div><span class="badge ${data.badgeClass}">${data.statusText}</span></div>
                </div>
            </div>`;

        // Section đặc thù theo vai trò
        if (role === 'sinhvien') {
            html += `
                <div class="p-3 rounded-3 mb-3 border ${data.trangThaiDiemDanhCuaSinhVien === 'Có mặt' ? 'bg-success-subtle border-success-subtle' : 'bg-light'}">
                    <h6 class="fw-bold mb-1" style="font-size:0.9rem;">
                        <i class="bi bi-patch-check-fill me-1 text-primary"></i> Trạng thái chuyên cần của bạn:
                    </h6>
                    <div class="d-flex align-items-center gap-2 mt-2">
                        <span class="badge ${data.trangThaiDiemDanhCuaSinhVien === 'Có mặt' ? 'bg-success' : (data.trangThaiDiemDanhCuaSinhVien === 'Vắng' ? 'bg-danger' : 'bg-secondary')} px-3 py-2 fs-6">
                            ${data.trangThaiDiemDanhCuaSinhVien}
                        </span>
                        <span class="text-muted small">
                            ${data.daDiemDanh ? '(Đã có dữ liệu điểm danh từ giảng viên)' : '(Giảng viên chưa lưu điểm danh buổi này)'}
                        </span>
                    </div>
                </div>`;
        } else if (role === 'giangvien') {
            const students = data.danhSachSinhVien || [];
            html += `
                <div class="mb-3">
                    <div class="d-flex justify-content-between align-items-center mb-2">
                        <h6 class="fw-bold mb-0" style="font-size:0.92rem;">
                            <i class="bi bi-people-fill me-1 text-primary"></i> Danh sách sinh viên đăng ký (${students.length} SV)
                        </h6>
                        <span class="badge ${data.daDiemDanh ? 'bg-success' : 'bg-warning text-dark'}">
                            ${data.daDiemDanh ? '✓ Đã điểm danh buổi này' : 'Chưa điểm danh'}
                        </span>
                    </div>
                    <div class="tkb-student-table-wrap">
                        <table class="table table-sm table-hover mb-0" style="font-size:0.82rem;">
                            <thead class="table-light sticky-top">
                                <tr>
                                    <th style="width:40px;">STT</th>
                                    <th>Mã SV</th>
                                    <th>Họ và tên</th>
                                    <th>Lớp HC</th>
                                    <th>Trạng thái</th>
                                </tr>
                            </thead>
                            <tbody>
                                ${students.length === 0 ? '<tr><td colspan="5" class="text-center text-muted py-3">Chưa có sinh viên đăng ký</td></tr>' : ''}
                                ${students.map((sv, idx) => `
                                    <tr>
                                        <td>${idx + 1}</td>
                                        <td class="font-monospace fw-semibold text-primary">${sv.maSinhVien}</td>
                                        <td class="fw-bold">${sv.hoTen}</td>
                                        <td><span class="badge bg-light text-dark border">${sv.tenLopHanhChinh || '—'}</span></td>
                                        <td>
                                            <span class="badge ${sv.trangThaiDiemDanh === 'Có mặt' ? 'bg-success-subtle text-success' : (sv.trangThaiDiemDanh === 'Vắng' ? 'bg-danger-subtle text-danger' : 'bg-light text-muted')}">
                                                ${sv.trangThaiDiemDanh}
                                            </span>
                                        </td>
                                    </tr>
                                `).join('')}
                            </tbody>
                        </table>
                    </div>
                </div>`;
        }

        if (data.ghiChu) {
            html += `
                <div class="small text-muted p-2 rounded bg-light border">
                    <i class="bi bi-info-circle me-1"></i> Ghi chú: ${data.ghiChu}
                </div>`;
        }

        modalContent.innerHTML = html;
    }

    // In thời khóa biểu
    window.inThoiKhoaBieu = function () {
        window.print();
    };

})();
