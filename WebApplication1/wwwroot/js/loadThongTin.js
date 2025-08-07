
document.querySelectorAll('.toggle-col').forEach(cb => {
    cb.addEventListener('change', function () {
        const colIndex = parseInt(this.dataset.col) + 1; // vì nth-child bắt đầu từ 1
        const isChecked = this.checked;

        document.querySelectorAll('table th:nth-child(' + colIndex + '), table td:nth-child(' + colIndex + ')')
            .forEach(cell => {
                cell.style.display = isChecked ? '' : 'none';
            });
    });
});
document.getElementById('toggle-all').addEventListener('change', function () {
    const allChecked = this.checked;
    const checkboxes = document.querySelectorAll('.toggle-col');

    checkboxes.forEach(cb => {
        cb.checked = allChecked;
        cb.dispatchEvent(new Event('change')); // Gọi lại sự kiện thay đổi nếu bạn có xử lý ẩn/hiện cột
    });
});
$(document).ready(function () {
    console.log("Document ready - search script loaded");

    $('#searchInputTen').on('input', function () {
        console.log("Search input changed");
        var searchValue = $(this).val().toLowerCase().trim();
        console.log("Search value:", searchValue);

        $('tbody tr').each(function () {
            // Cột Họ tên là cột thứ 3 (STT=1, Mã BN=2, Họ tên=3)
            var hoTen = $(this).find('td:nth-child(3)').text().toLowerCase();
            console.log("Checking row with name:", hoTen);

            if (hoTen.includes(searchValue) || searchValue === "") {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    });
});

function initDateInputFormatting() {
    const dateInputIds = ["ngaySinh", "ngayNhapVien", "ngayXuatVien"];

    dateInputIds.forEach(function (id) {
        const input = document.getElementById(id);
        if (!input) return;

        input.addEventListener("input", function (e) {
            let value = input.value.replace(/\D/g, "");
            let formatted = "";
            let selectionStart = input.selectionStart;

            if (value.length > 0) formatted += value.substring(0, 2);
            if (value.length >= 3) formatted += "-" + value.substring(2, 4);
            if (value.length >= 5) formatted += "-" + value.substring(4, 8);

            if (formatted !== input.value) {
                const prevLength = input.value.length;
                input.value = formatted;
                const newLength = formatted.length;
                const diff = newLength - prevLength;
                input.setSelectionRange(selectionStart + diff, selectionStart + diff);
            }
        });

        input.addEventListener("click", function () {
            const pos = input.selectionStart;
            if (pos <= 2) input.setSelectionRange(0, 2);
            else if (pos <= 5) input.setSelectionRange(3, 5);
            else input.setSelectionRange(6, 10);
        });

        input.addEventListener("keydown", function (e) {
            const pos = input.selectionStart;
            let val = input.value;

            if (e.key === "Backspace" && (pos === 3 || pos === 6)) {
                e.preventDefault();
                input.value = val.slice(0, pos - 1) + val.slice(pos);
                input.setSelectionRange(pos - 1, pos - 1);
            }
            if (e.key === "Delete" && (pos === 2 || pos === 5)) {
                e.preventDefault();
                input.value = val.slice(0, pos) + val.slice(pos + 1);
                input.setSelectionRange(pos, pos);
            }
        });
    });
}

function initNgayNhapVienDisplay() {
    var ngayNhapVienInput = document.getElementById('ngayNhapVienInput');
    var ngayNhapVienDisplay = document.getElementById('ngayNhapVienDisplay');

    if (ngayNhapVienInput && ngayNhapVienInput.value) {
        var formattedDate = moment(ngayNhapVienInput.value).format('DD-MM-YYYY hh:mm A');
        ngayNhapVienDisplay.value = formattedDate;
    }
}

function initDatePicker() {
    $('[id^="ngaySinh"], [id^="ngayNhapVien"], [id^="ngayXuatVien"]').datepicker({
        format: 'dd-mm-yyyy',
        autoclose: true,
        language: 'vi',
        todayHighlight: true,
        orientation: 'bottom auto',
    });
}

$('#addBenhNhanModal').on('hidden.bs.modal', function () {
    $(this).removeData('loaded'); // reset flag
    $('#addBenhNhanFormContainer').html(''); // xoá form cũ
});
$(document).ready(function () {
    initDateInputFormatting(); // chạy lần đầu khi trang load
});

function initAutoCalculation() {
    // Hàm định dạng số cải tiến
    function formatNumber(num) {
        num = num.toString().replace(/\./g, '');
        return isNaN(num) ? '0' : parseInt(num).toLocaleString('vi-VN');
    }

    // Hàm parse số chắc chắn hơn
    function parseNumber(str) {
        return parseInt(str.toString().replace(/\./g, '')) || 0;
    }

    // Gắn sự kiện cho DonGia
    $('#DonGia').off('input change').on('input change', function () {
        let raw = $(this).val().replace(/[^\d]/g, '');
        $(this).val(formatNumber(raw));
        calculateAll();
    });

    // Hàm tính toán cải tiến
    function calculateAll() {
        const soNgay = parseInt($('#SoNgayNhapVien').val()) || 0;
        const donGia = parseNumber($('#DonGia').val());
        const tongTien = soNgay * donGia;

        $('#TongTien').val(formatNumber(tongTien));
        $('#TongTienRaw').val(tongTien);

        console.log('Tính toán:', {
            soNgay: soNgay,
            donGia: donGia,
            tongTien: tongTien
        });
    }

    // Gắn sự kiện cho các trường liên quan
    $('#ngayNhapVien, #ngayXuatVien').off('change').on('change', function () {
        const nhapVien = $('#ngayNhapVien').val();
        const xuatVien = $('#ngayXuatVien').val();

        if (nhapVien && xuatVien) {
            const date1 = new Date(nhapVien.split(' ')[0].split('-').reverse().join('-'));
            const date2 = new Date(xuatVien.split(' ')[0].split('-').reverse().join('-'));
            const diffDays = Math.floor((date2 - date1) / (1000 * 60 * 60 * 24)) + 1;

            $('#SoNgayNhapVien').val(diffDays > 0 ? diffDays : '');
        } else {
            $('#SoNgayNhapVien').val('');
        }

        calculateAll();
    });

    // Tính toán ngay khi load
    calculateAll();
}
$(document).ready(function () {
    $('#danTocId').select2({
        theme: 'bootstrap-5',
        width: '100%',
        dropdownAutoWidth: true
    });
});
function updateQueryStringParameter(uri, key, value) {
    var re = new RegExp("([?&])" + key + "=.*?(&|$)", "i");
    var separator = uri.indexOf('?') !== -1 ? "&" : "?";
    if (uri.match(re)) {
        return uri.replace(re, '$1' + key + "=" + value + '$2');
    }
    return uri + separator + key + "=" + value;
}

$(document).ready(function () {
    // Khi mở modal thêm
    $('#addBenhNhanModal').on('show.bs.modal', function (e) {
        var button = $(e.relatedTarget);
        var url = button.data('url');
        var modal = $(this);

        // Lấy currentPage và pageSize từ URL hiện tại hoặc dropdown
        var urlParams = new URLSearchParams(window.location.search);
        var currentPage = urlParams.get('page') || 1;
        var pageSize = $('#pageSizeSelect').val() || urlParams.get('pageSize') || 5;

        // Thêm currentPage và pageSize vào URL
        url = updateQueryStringParameter(url, 'currentPage', currentPage);
        url = updateQueryStringParameter(url, 'pageSize', pageSize);

        // Load form vào modal
        modal.find('#addBenhNhanFormContainer').load(url, function () {
            modal.find('.modal-title').text(
                url.includes('/Edit/') ? 'Sửa thông tin bệnh nhân' : 'Thêm mới bệnh nhân'
            );
        });
        if (url) {


            $.get(url, function (html) {
                $('#addBenhNhanFormContainer').html(html);

                initDatePicker();
                initNgayNhapVienDisplay();
                //initSelect2();
                initDateInputFormatting();

                // CHỜ DOM cập nhật xong rồi mới gọi initAutoCalculation
                setTimeout(() => {
                    initAutoCalculation();
                }, 0);
            })

        }
    });
   
    // Khi modal ẩn đi thì xóa nội dung để lần sau luôn load mới
    $('#addBenhNhanModal').on('hidden.bs.modal', function () {
        $('#addBenhNhanFormContainer').html('');
        $(this).removeData('bs.modal');
    });
});
