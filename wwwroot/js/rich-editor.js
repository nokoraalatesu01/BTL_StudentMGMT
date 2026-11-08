/**
 * Rich Text Editor Initialization Module (CKEditor 5)
 * Tự động tìm và biến đổi tất cả các thẻ <textarea class="rich-editor"> thành bộ soạn thảo WYSIWYG chuyên nghiệp.
 * Hỗ trợ: Định dạng văn bản, In đậm, In nghiêng, Danh sách, Bảng, Trích dẫn, Căn lề, Hoàn tác...
 */
(function () {
    'use strict';

    function initRichEditors(root) {
        root = root || document;

        // Nếu chưa nạp thư viện CKEditor 5 thì thoát an toàn
        if (typeof ClassicEditor === 'undefined') {
            console.warn('CKEditor 5 chưa được tải. Vui lòng kiểm tra script CDN của ClassicEditor.');
            return;
        }

        var textareas = root.querySelectorAll('textarea.rich-editor, textarea.ckeditor, textarea[data-rich-editor="true"]');

        textareas.forEach(function (textarea) {
            // Tránh khởi tạo lặp lại
            if (textarea.dataset.ckeditorInit === 'true') {
                return;
            }
            textarea.dataset.ckeditorInit = 'true';

            ClassicEditor
                .create(textarea, {
                    toolbar: {
                        items: [
                            'heading', '|',
                            'bold', 'italic', 'underline', 'strikethrough', '|',
                            'bulletedList', 'numberedList', '|',
                            'outdent', 'indent', '|',
                            'link', 'blockQuote', 'insertTable', '|',
                            'undo', 'redo'
                        ],
                        shouldNotGroupWhenFull: true
                    },
                    heading: {
                        options: [
                            { model: 'paragraph', title: 'Văn bản thường', class: 'ck-heading_paragraph' },
                            { model: 'heading2', view: 'h2', title: 'Tiêu đề lớn (H2)', class: 'ck-heading_heading2' },
                            { model: 'heading3', view: 'h3', title: 'Tiêu đề vừa (H3)', class: 'ck-heading_heading3' },
                            { model: 'heading4', view: 'h4', title: 'Tiêu đề nhỏ (H4)', class: 'ck-heading_heading4' }
                        ]
                    },
                    table: {
                        contentToolbar: ['tableColumn', 'tableRow', 'mergeTableCells']
                    },
                    placeholder: textarea.getAttribute('placeholder') || 'Nhập nội dung văn bản định dạng phong phú tại đây...'
                })
                .then(function (editor) {
                    textarea._ckeditorInstance = editor;

                    // Tự động đồng bộ nội dung từ editor về textarea khi có form submit
                    var form = textarea.closest('form');
                    if (form) {
                        form.addEventListener('submit', function () {
                            textarea.value = editor.getData();
                        });
                    }

                    // Đồng bộ khi nội dung editor thay đổi
                    editor.model.document.on('change:data', function () {
                        textarea.value = editor.getData();
                    });
                })
                .catch(function (error) {
                    console.error('Lỗi khi khởi tạo Rich Text Editor cho textarea:', error);
                });
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            initRichEditors(document);
        });
    } else {
        initRichEditors(document);
    }

    // Xuất hàm ra phạm vi toàn cục để có thể gọi lại sau khi mở Modal hoặc tải động AJAX
    window.initPortalRichEditors = initRichEditors;
})();

