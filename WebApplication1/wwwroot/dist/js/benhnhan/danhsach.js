document.getElementById("btnExportPDF").addEventListener("click", function () {
    // Show loading indicator
    const btn = this;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Đang tạo PDF...';
    btn.disabled = true;

    fetch("/export/pdf", {
        method: "GET",
        headers: {
            'Accept': 'application/pdf'
        }
    })
        .then(response => {
            if (!response.ok) {
                return response.text().then(text => { throw new Error(text || "Không thể tải file PDF") });
            }
            return response.blob();
        })
        .then(blob => {
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement("a");
            a.href = url;
            a.download = "DanhSachBenhNhan.pdf";
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);
        })
        .catch(error => {
            console.error("Error:", error);
            alert("Lỗi khi xuất PDF: " + error.message);
        })
        .finally(() => {
            btn.innerHTML = '<i class="bi bi-file-earmark-pdf"></i> Xuất PDF';
            btn.disabled = false;
        });
});
// xử lí sự kiện tìm theo tên và load vào tbody
$(document).ready(function () {
    $('#searchInputTen').on('input', function () {
        var searchValue = $(this).val().toLowerCase();
        $('tbody tr').each(function () {
            var tenBenhNhan = $(this).find('td:nth-child(2)').text().toLowerCase();
            if (tenBenhNhan.includes(searchValue)) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    });
});
$(document).ready(function () {
    $('#addBenhNhanModal').on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget); // Nút được click
        var url = button.data('url'); // Lấy URL từ data-url (đã chứa currentPage)
        var modal = $(this);

        // Load form bằng AJAX
        $.get(url, function (data) {
            modal.find('.modal-body').html(data);
        });
    });
});
document.addEventListener("DOMContentLoaded", function () {
    const toggleBtn = document.querySelector('.column-toggle-dropdown button');
    const dropdownMenu = document.querySelector('.column-toggle-dropdown .dropdown-menu');

    toggleBtn.addEventListener('click', function (e) {
        e.stopPropagation();
        dropdownMenu.style.display = dropdownMenu.style.display === 'block' ? 'none' : 'block';
    });

    // KHÔNG ẩn nếu click bên trong dropdown menu
    document.addEventListener('click', function (e) {
        if (!dropdownMenu.contains(e.target) && e.target !== toggleBtn) {
            dropdownMenu.style.display = 'none';
        }
    });

    // Ngăn click checkbox làm tắt dropdown
    dropdownMenu.addEventListener('click', function (e) {
        e.stopPropagation();
    });
});