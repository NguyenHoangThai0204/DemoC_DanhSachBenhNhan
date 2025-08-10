
// goiKham.js - Xử lý ngày tháng cho module Gói Khám
function initDateInputFormatting() {
    const dateInputIds = ["ngayTuNgay", "ngayDenNgay"];

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

function initDatePicker() {
    $('[id="ngayTuNgay"], [id="ngayDenNgay"]').datepicker({
        format: 'dd-mm-yyyy',
        autoclose: true,
        language: 'vi',
        todayHighlight: true,
        orientation: 'bottom auto',
    });
}

function updateTable(data) {
    const tbody = $('tbody');
    tbody.empty();

    if (data && data.length > 0) {
        data.forEach((item, index) => {
            const row = `
                    <tr>
                        <td>${index + 1}</td>
                        <td>${item.maYTe || ''}</td>
                        <td style="text-align:left;">${item.hoTen || 'Không rõ'}</td>
                        <td style="text-align:left;">${item.goiKham || 'Không rõ'}</td>
                        <td>${item.ngayDangKy ? formatDate(item.ngayDangKy) : ''}</td>
                        <td style="text-align:left;">${item.trangThaiThucHien || 'Không rõ'}</td>
                        <td style="text-align:left;">${item.chiDinhConLai || 'Không rõ'}</td>
                        <td style="text-align:left;">${item.ghiChu || 'Không rõ'}</td>
                    </tr>
                `;
            tbody.append(row);
        });
    } else {
        tbody.append('<tr><td colspan="8" class="text-center">Không có dữ liệu</td></tr>');
    }
}

// Hàm định dạng ngày
function formatDate(dateString) {
    if (!dateString) return '';
    const date = new Date(dateString);
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    return `${day}-${month}-${year}`;
}
$(document).ready(function () {
    // Khởi tạo datepicker và định dạng nhập liệu
    initDatePicker();
    initDateInputFormatting();

  
    $('.datepicker-trigger').click(function () {
        console.log("Icon calendar clicked!"); // 🔍 Test bắt sự kiện

        const inputId = $(this).closest('.input-group').find('.date-input').attr('id');
        console.log("Target input:", inputId); // 🔍 Xem input đang show datepicker

        $('#' + inputId).datepicker('show');
    });
    $('#btnUpload').click(function (e) {
        window.location.reload();
    });

    $('#btnExportPDFGoiKham').off('click').on('click', function (e) {
        e.preventDefault();

        const btn = this;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Đang tạo PDF...';
        btn.disabled = true;

        // Kiểm tra nếu có dữ liệu đã lọc thì gửi đi, không thì gửi request rỗng
        const requestData = window.filteredData
            ? {
                data: window.filteredData,
                fromDate: $('#ngayTuNgay').val(),
                toDate: $('#ngayDenNgay').val()
            }
            : { data: [], fromDate: '', toDate: '' }; // Gửi request rỗng để trigger lấy toàn bộ dữ liệu

        fetch("/goikham/export/pdf", {
            method: "POST",
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/pdf'
            },
            body: JSON.stringify(requestData)
        })
            .then(res => res.blob())
            .then(blob => {
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement("a");
                a.href = url;
                a.download = "BaoCaoGoiKham.pdf";
                a.click();
                window.URL.revokeObjectURL(url);
            })
            .catch(error => {
                console.error("Error:", error);
                toastr.error("Có lỗi khi tạo PDF");
            })
            .finally(() => {
                btn.innerHTML = '<i class="bi bi-file-earmark-pdf"></i> Xuất PDF';
                btn.disabled = false;
            });
    });
    $('#btnExportExcelGoiKham').off('click').on('click', function (e) {
        e.preventDefault();

        const btn = $(this);
        const originalHtml = btn.html();
        btn.html('<span class="spinner-border spinner-border-sm"></span> Đang tạo Excel...');
        btn.prop('disabled', true);

        const requestData = window.filteredData
            ? {
                data: window.filteredData,
                fromDate: $('#ngayTuNgay').val(),
                toDate: $('#ngayDenNgay').val()
            }
            : { data: [], fromDate: '', toDate: '' };

        // Thêm loading state
        $.ajax({
            url: '/goikham/export/excel',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(requestData),
            xhrFields: {
                responseType: 'blob'
            },
            success: function (data, status, xhr) {
                // Kiểm tra content type
                const contentType = xhr.getResponseHeader('content-type');
                if (contentType !== 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet') {
                    throw new Error('Invalid file type');
                }

                // Tạo URL tải file
                const blob = new Blob([data], { type: contentType });
                const url = window.URL.createObjectURL(blob);

                // Tạo thẻ a để tải xuống
                const a = document.createElement('a');
                a.href = url;
                a.download = `BaoCaoGoiKham_${requestData.fromDate || 'all'}_den_${requestData.toDate || 'now'}.xlsx`;
                document.body.appendChild(a);
                a.click();

                // Dọn dẹp
                window.URL.revokeObjectURL(url);
                document.body.removeChild(a);
            },
            error: function (xhr, status, error) {
                console.error('Error:', error);
                try {
                    const errMsg = xhr.responseJSON?.error || xhr.statusText || 'Lỗi không xác định';
                    toastr.error(`Lỗi khi tạo file Excel: ${errMsg}`);
                } catch (e) {
                    toastr.error('Lỗi khi xử lý phản hồi từ server');
                }
            },
            complete: function () {
                btn.html(originalHtml);
                btn.prop('disabled', false);
            }
        });
    });
    // Xử lý sự kiện click cho nút lọc gói khám theo ngày
    $('#btnFilter').click(function (e) {

        e.preventDefault(); // Ngăn chặn hành động mặc định của nút
        console.log("Filter button clicked!"); // 🔍 Test bắt sự kiện

        const tuNgay = $('#ngayTuNgay').val();
        const denNgay = $('#ngayDenNgay').val();
        //console.log("From date:", tuNgay, "To date:", denNgay); // 🔍 Xem giá trị ngày tháng

        if (tuNgay === "" || denNgay === "" || (tuNgay === "" && denNgay === "")) {
            // gửi thông báo chọn ngày
            return;
        }

        // Gửi yêu cầu AJAX để lọc gói khám
        $.ajax({
            url: '/GoiKham/FilterByDay',
            type: 'POST',
            data: {
                tuNgay: tuNgay,
                denNgay: denNgay
            },
            success: function (response) {
                console.log("Lọc thành công", response);

                if (response.success) {
                    toastr.info(response.message); // 🔔 Thông báo kết quả
                    updateTable(response.data);
                    window.filteredData = response.data;
                } else {
                    toastr.error("Có lỗi khi lọc dữ liệu");
                }
            },
            error: function (xhr, status, error) {
                console.error("Lỗi khi lọc dữ liệu:", error);
                alert('Có lỗi xảy ra khi lọc dữ liệu');
            }
        });
    }); 




});