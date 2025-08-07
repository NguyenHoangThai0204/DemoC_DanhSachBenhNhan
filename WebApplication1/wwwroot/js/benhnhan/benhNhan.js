
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

$(document).ready(function () {
    let tinhThanhData = [];
    const $inputTinh = $("#tinhThanhInput");
    const $dropdownTinh = $("#tinhThanhDropdown");
    $dropdownTinh.hide();

    // Hàm hiển thị dropdown
    function renderTinhThanhDropdown(data) {
        $dropdownTinh.empty();

        if (data.length === 0) {
            $dropdownTinh.append(`<div class="tinh-thanh-item">Không tìm thấy kết quả</div>`);
        } else {
            data.forEach(item => {
                $dropdownTinh.append(`
                    <div class="tinh-thanh-item"
                        data-matinh="${item.MaTinh}"
                        data-name="${item.TenTinh.toLowerCase()}"
                        data-viettat="${(item.VietTat || []).join(',').toLowerCase()}">
                        ${item.TenTinh}
                    </div>
                `);
            });
        }

        $dropdownTinh.show();
    }

    // Hàm lọc tỉnh thành theo từ khóa
    function filterTinhThanh(keyword) {
        const lowerKeyword = keyword.toLowerCase();

        const filtered = tinhThanhData
            .filter(item => {
                const name = item.TenTinh.toLowerCase();
                const ma = item.MaTinh.toLowerCase();
                const vietTat = (item.VietTat || []).map(v => v.toLowerCase());
                return !lowerKeyword || name.includes(lowerKeyword) || ma.includes(lowerKeyword) || vietTat.some(v => v.includes(lowerKeyword));
            })
            .sort((a, b) => {
                const aName = a.TenTinh.toLowerCase();
                const bName = b.TenTinh.toLowerCase();

                // Ưu tiên kết quả bắt đầu bằng keyword
                const aStartsWith = aName.startsWith(lowerKeyword);
                const bStartsWith = bName.startsWith(lowerKeyword);

                if (aStartsWith && !bStartsWith) return -1;
                if (!aStartsWith && bStartsWith) return 1;

                // Nếu cùng bắt đầu hoặc không bắt đầu thì sắp xếp ABC
                return aName.localeCompare(bName);
            });

        renderTinhThanhDropdown(filtered);
    }

    // Load dữ liệu JSON và sắp xếp ABC
    function loadTinhThanhData() {
        $.ajax({
            url: "/dulieu/tinhthanh.json",
            dataType: "json",
            success: function (data) {
                // Sắp xếp theo tên tỉnh
                tinhThanhData = data.sort((a, b) => a.TenTinh.localeCompare(b.TenTinh));
                console.log("Dữ liệu tỉnh/thành đã được tải:", tinhThanhData);
            },
            error: function () {
                tinhThanhData = [
                    { "MaTinh": "HNI", "TenTinh": "Hà Nội", "VietTat": ["hanoi", "hn"] },
                    { "MaTinh": "HCM", "TenTinh": "Hồ Chí Minh", "VietTat": ["hochiminh", "hcm", "tphcm", "sg"] }
                ];
                // Sắp xếp fallback data
                tinhThanhData.sort((a, b) => a.TenTinh.localeCompare(b.TenTinh));
                console.warn("Sử dụng dữ liệu mặc định");
            }
        });
    }

    // Input Tỉnh Thành: focus và input
    $inputTinh.on("focus input", function () {
        const keyword = $(this).val().trim();
        filterTinhThanh(keyword);
        $dropdownTinh.show();
    });

    // Chọn item
    $dropdownTinh.on("click", ".tinh-thanh-item", function () {
        if ($(this).text().trim() === "Không tìm thấy kết quả") return;

        const ten = $(this).text().trim();
        const ma = $(this).data("matinh");
        const tinhThanh = tinhThanhData.find(item => item.MaTinh === ma);

        if (tinhThanh) {
            $inputTinh.val(ten);
            $("#tinhThanhIdHidden").val('');
            $("#tinhThanhMaHidden").val(ma);
            $("#tinhThanhTenHidden").val(ten);
            $("#tinhThanhVietTatHidden").val(JSON.stringify(tinhThanh.VietTat));
            $dropdownTinh.hide();
        }
    });

    // Đóng khi click ra ngoài
    $(document).on("click", function (e) {
        if (!$(e.target).closest("#tinhThanhInput, #tinhThanhDropdown").length) {
            $dropdownTinh.hide();
        }
    });

    // Xử lý bàn phím (up, down, enter)
    let tinhIndex = -1;

    $inputTinh.on("keydown", function (e) {
        if (!$dropdownTinh.is(":visible")) return;

        const items = $dropdownTinh.find(".tinh-thanh-item:visible");
        if (items.length === 0) return;

        if (e.key === "ArrowDown") {
            e.preventDefault();
            tinhIndex = (tinhIndex + 1) % items.length;
            items.removeClass("active").eq(tinhIndex).addClass("active");
            items.eq(tinhIndex)[0].scrollIntoView({ block: "nearest" });
        } else if (e.key === "ArrowUp") {
            e.preventDefault();
            tinhIndex = (tinhIndex - 1 + items.length) % items.length;
            items.removeClass("active").eq(tinhIndex).addClass("active");
            items.eq(tinhIndex)[0].scrollIntoView({ block: "nearest" });
        } else if (e.key === "Enter") {
            e.preventDefault();
            if (tinhIndex >= 0 && tinhIndex < items.length) {
                items.eq(tinhIndex).click();
                tinhIndex = -1;
            }
        }
    });

    $inputTinh.on("input click", function () {
        tinhIndex = -1;
    });

    // Load dữ liệu khởi tạo
    loadTinhThanhData();

    // ==========================
    // XỬ LÝ DÂN TỘC
    // ==========================
    const $inputDanToc = $("#danTocInput");
    const $dropdownDanToc = $("#danTocDropdown");
    const $hiddenDanTocId = $("#danTocIdHidden");

    //function filterDanToc(keyword) {
    //    const lowerKeyword = keyword.toLowerCase();
    //    $("#danTocDropdown .dan-toc-item").each(function () {
    //        const name = $(this).data("name");
    //        const vietTat = $(this).data("viettat") || "";
    //        $(this).toggle(!lowerKeyword || name.includes(lowerKeyword) || vietTat.includes(lowerKeyword));
    //    });
    //}

    //$inputDanToc.on("focus input", function () {
    //    filterDanToc($(this).val().trim());
    //    $dropdownDanToc.show();
    //});
    function filterDanToc(keyword) {
        const lowerKeyword = keyword.toLowerCase();
        const $container = $("#danTocDropdown");
        const $items = $container.find(".dan-toc-item");

        // Sắp xếp các item
        const itemsArray = $items.get().sort((a, b) => {
            const aName = $(a).data("name").toLowerCase();
            const bName = $(b).data("name").toLowerCase();
            const aStartsWith = aName.startsWith(lowerKeyword);
            const bStartsWith = bName.startsWith(lowerKeyword);

            if (aStartsWith && !bStartsWith) return -1;
            if (!aStartsWith && bStartsWith) return 1;
            return aName.localeCompare(bName);
        });

        // Xóa tất cả items và thêm lại theo thứ tự mới
        $container.empty().append(itemsArray);

        // Lọc hiển thị
        $items.each(function () {
            const name = $(this).data("name").toLowerCase();
            const vietTat = ($(this).data("viettat") || "").toLowerCase();
            const isVisible = !lowerKeyword || name.includes(lowerKeyword) || vietTat.includes(lowerKeyword);
            $(this).toggle(isVisible);
        });
    }

    $inputDanToc.on("focus", function () {
        // Hiển thị dropdown trước khi filter
        $("#danTocDropdown").show();
        filterDanToc(""); // Hiển thị tất cả khi focus
    });

    $inputDanToc.on("input", function () {
        filterDanToc($(this).val().trim());
    });

    $dropdownDanToc.on("click", ".dan-toc-item", function () {
        $inputDanToc.val($(this).text().trim());
        $hiddenDanTocId.val($(this).data("id"));
        $dropdownDanToc.hide();
    });

    $(document).on("click", function (e) {
        if (!$(e.target).closest("#danTocInput, #danTocDropdown").length) {
            $dropdownDanToc.hide();
        }
    });

    let danTocIndex = -1;

    $inputDanToc.on("keydown", function (e) {
        if (!$dropdownDanToc.is(":visible")) return;

        const items = $dropdownDanToc.find(".dan-toc-item:visible");
        if (items.length === 0) return;

        if (e.key === "ArrowDown") {
            e.preventDefault();
            danTocIndex = (danTocIndex + 1) % items.length;
            items.removeClass("active").eq(danTocIndex).addClass("active");
            items.eq(danTocIndex)[0].scrollIntoView({ block: "nearest" });
        } else if (e.key === "ArrowUp") {
            e.preventDefault();
            danTocIndex = (danTocIndex - 1 + items.length) % items.length;
            items.removeClass("active").eq(danTocIndex).addClass("active");
            items.eq(danTocIndex)[0].scrollIntoView({ block: "nearest" });
        } else if (e.key === "Enter") {
            e.preventDefault();
            if (danTocIndex >= 0 && danTocIndex < items.length) {
                items.eq(danTocIndex).click();
                danTocIndex = -1;
            }
        }
    });

    $inputDanToc.on("input click", function () {
        danTocIndex = -1;
    });

    // Sắp xếp dropdown dân tộc theo ABC khi load
    function sortDanTocDropdown() {
        const items = $dropdownDanToc.find(".dan-toc-item").get();

        items.sort(function (a, b) {
            const nameA = $(a).text().trim().toLowerCase();
            const nameB = $(b).text().trim().toLowerCase();
            return nameA.localeCompare(nameB);
        });

        $dropdownDanToc.empty().append(items);
    }

    // Gọi khi document ready
    sortDanTocDropdown();
});

$(document).ready(function () {
    // Lấy giá trị pageSize từ URL hoặc mặc định là 5
    const urlParams = new URLSearchParams(window.location.search);
    let currentPageSize = urlParams.get('pageSize');

    // Nếu không có trong URL, kiểm tra dropdown
    if (!currentPageSize) {
        currentPageSize = $('#pageSizeSelect').val() || 5;
    }

    // Thiết lập giá trị selected cho dropdown
    $('#pageSizeSelect').val(currentPageSize);

    // Xử lý sự kiện thay đổi pageSize
    $('#pageSizeSelect').change(function () {
        const newPageSize = $(this).val();
        // Cập nhật cả pageSize trong URL khi chuyển trang
        window.location.href = '@Url.Action("DanhSach")' + `?page=1&pageSize=${newPageSize}`;
    });
});
$(document).ready(function () {
    // Hàm lưu trạng thái cột vào localStorage
    function saveColumnState() {
        const columnStates = {};
        $('.toggle-col').each(function () {
            const colIndex = $(this).data('col');
            columnStates[colIndex] = $(this).prop('checked');
        });
        localStorage.setItem('columnStates', JSON.stringify(columnStates));
    }

    // Hàm khôi phục trạng thái cột từ localStorage
    function restoreColumnState() {
        const savedStates = localStorage.getItem('columnStates');
        if (savedStates) {
            const columnStates = JSON.parse(savedStates);
            $('.toggle-col').each(function () {
                const colIndex = $(this).data('col');
                if (columnStates.hasOwnProperty(colIndex)) {
                    const isChecked = columnStates[colIndex];
                    $(this).prop('checked', isChecked);
                    toggleColumn(colIndex, isChecked);
                }
            });

            // Nếu tất cả các checkbox đều được chọn => chọn cả "toggle-all"
            const allChecked = $('.toggle-col').length === $('.toggle-col:checked').length;
            $('#toggle-all').prop('checked', allChecked);
        }
    }

    // Hàm ẩn/hiện cột
    function toggleColumn(colIndex, show) {
        const colNum = parseInt(colIndex) + 1; // Vì nth-child bắt đầu từ 1
        $('table th:nth-child(' + colNum + '), table td:nth-child(' + colNum + ')').css('display', show ? '' : 'none');
    }

    // Áp dụng lại trạng thái khi trang mới load
    restoreColumnState();

    // Xử lý sự kiện thay đổi checkbox
    $('.toggle-col').change(function () {
        const colIndex = $(this).data('col');
        const isChecked = $(this).prop('checked');
        toggleColumn(colIndex, isChecked);
        saveColumnState();

        // Đồng bộ toggle-all
        const allChecked = $('.toggle-col').length === $('.toggle-col:checked').length;
        $('#toggle-all').prop('checked', allChecked);
    });

    // Checkbox "Chọn tất cả"
    $('#toggle-all').change(function () {
        const isChecked = $(this).prop('checked');
        $('.toggle-col').prop('checked', isChecked).trigger('change');
    });

});
$(document).ready(function () {
    $('#searchInputTen').on('input', function () {
        var searchValue = $(this).val();
        var pageSize = $('#pageSizeSelect').val() || 5;

        $.ajax({
            url: '/BenhNhan/SearchBenhNhan',
            method: 'GET',
            data: { tenBenhNhan: searchValue, page: 1, pageSize: pageSize },
            success: function (res) {
                var tbody = $('tbody');
                tbody.empty(); // Xoá tbody hiện tại

                if (res.data && res.data.length > 0) {
                    let index = 1;
                    res.data.forEach(function (item) {
                        var row = `<tr>
                                    <td>${index++}</td>
                                    <td>${item.maBenhNhan}</td>
                                    <td style="text-align:left;">${item.hoTen}</td>
                                    <td>${item.ngaySinh}</td>
                                    <td>${item.gioiTinh}</td>
                                    <td style="text-align:left;">${item.danToc}</td>
                                    <td style="text-align:left;">${item.tinhThanh}</td>
                                    <td>${item.ngayNhapVien}</td>
                                    <td>${item.ngayXuatVien}</td>
                                    <td class="tdTien">${item.soNgay ? item.soNgay : ''}</td>
                                    <td class="tdTien">${item.donGia}</td>
                                    <td class="tdTien">${item.tongTien}</td>
                                    <td>
                                        <a href="#" class="btn btn-sm btn-info"
                                           data-bs-toggle="modal"
                                           data-bs-target="#addBenhNhanModal"
                                           data-url="/BenhNhan/Edit/${item.maBenhNhan}"
                                           title="Sửa">
                                            <i class="bi bi-pencil-square"></i>
                                        </a>
                                        <form action="/BenhNhan/Delete/${item.maBenhNhan}" method="post" style="display:inline;">
                                            <button type="submit" class="btn btn-sm btn-danger" onclick="return confirm('Bạn có chắc muốn xóa?');">
                                                <i class="bi bi-trash"></i>
                                            </button>
                                        </form>
                                    </td>
                                </tr>`;
                        tbody.append(row);
                    });
                } else {
                    tbody.append(`<tr><td colspan="13" style="text-align:center;">Không tìm thấy kết quả.</td></tr>`);
                }
            },
            error: function (xhr) {
                console.error("Lỗi khi tìm kiếm:", xhr.responseText);
            }
        });
    });
});
$(document).ready(function () {
    // Lấy giá trị pageSize từ URL hoặc mặc định là 5
    const urlParams = new URLSearchParams(window.location.search);
    let currentPageSize = urlParams.get('pageSize');

    // Nếu không có trong URL, kiểm tra dropdown
    if (!currentPageSize) {
        currentPageSize = $('#pageSizeSelect').val() || 5;
    }

    // Thiết lập giá trị selected cho dropdown
    $('#pageSizeSelect').val(currentPageSize);

    // Xử lý sự kiện thay đổi pageSize
    $('#pageSizeSelect').change(function () {
        const newPageSize = $(this).val();
        // Lấy URL hiện tại và cập nhật tham số pageSize
        const currentUrl = new URL(window.location.href);
        currentUrl.searchParams.set('pageSize', newPageSize);
        currentUrl.searchParams.set('page', '1'); // Reset về trang đầu tiên

        // Chuyển hướng đến URL mới
        window.location.href = currentUrl.toString();
    });
});
$(document).ready(function () {
    // Hàm lưu trạng thái cột vào localStorage
    function saveColumnState() {
        const columnStates = {};
        $('.toggle-col').each(function () {
            const colIndex = $(this).data('col');
            columnStates[colIndex] = $(this).prop('checked');
        });
        localStorage.setItem('columnStates', JSON.stringify(columnStates));
    }

    // Hàm khôi phục trạng thái cột từ localStorage
    function restoreColumnState() {
        const savedStates = localStorage.getItem('columnStates');
        if (savedStates) {
            const columnStates = JSON.parse(savedStates);
            $('.toggle-col').each(function () {
                const colIndex = $(this).data('col');
                if (columnStates.hasOwnProperty(colIndex)) {
                    const isChecked = columnStates[colIndex];
                    $(this).prop('checked', isChecked);
                    toggleColumn(colIndex, isChecked);
                }
            });

            // Nếu tất cả các checkbox đều được chọn => chọn cả "toggle-all"
            const allChecked = $('.toggle-col').length === $('.toggle-col:checked').length;
            $('#toggle-all').prop('checked', allChecked);
        }
    }

    // Hàm ẩn/hiện cột
    function toggleColumn(colIndex, show) {
        const colNum = parseInt(colIndex) + 1; // Vì nth-child bắt đầu từ 1
        $('table th:nth-child(' + colNum + '), table td:nth-child(' + colNum + ')').css('display', show ? '' : 'none');
    }

    // Áp dụng lại trạng thái khi trang mới load
    restoreColumnState();

    // Xử lý sự kiện thay đổi checkbox
    $('.toggle-col').change(function () {
        const colIndex = $(this).data('col');
        const isChecked = $(this).prop('checked');
        toggleColumn(colIndex, isChecked);
        saveColumnState();

        // Đồng bộ toggle-all
        const allChecked = $('.toggle-col').length === $('.toggle-col:checked').length;
        $('#toggle-all').prop('checked', allChecked);
    });

    // Checkbox "Chọn tất cả"
    $('#toggle-all').change(function () {
        const isChecked = $(this).prop('checked');
        $('.toggle-col').prop('checked', isChecked).trigger('change');
    });

});
$(document).ready(function () {
    $('#searchInputTen').on('input', function () {
        var searchValue = $(this).val();
        var pageSize = $('#pageSizeSelect').val() || 5;

        $.ajax({
            url: '/BenhNhan/SearchBenhNhan',
            method: 'GET',
            data: { tenBenhNhan: searchValue, page: 1, pageSize: pageSize },
            success: function (res) {
                var tbody = $('tbody');
                tbody.empty(); // Xoá tbody hiện tại

                if (res.data && res.data.length > 0) {
                    let index = 1;
                    res.data.forEach(function (item) {
                        var row = `<tr>
                                    <td>${index++}</td>
                                    <td>${item.maBenhNhan}</td>
                                    <td style="text-align:left;">${item.hoTen}</td>
                                    <td>${item.ngaySinh}</td>
                                    <td>${item.gioiTinh}</td>
                                    <td style="text-align:left;">${item.danToc}</td>
                                    <td style="text-align:left;">${item.tinhThanh}</td>
                                    <td>${item.ngayNhapVien}</td>
                                    <td>${item.ngayXuatVien}</td>
                                    <td class="tdTien">${item.soNgay ? item.soNgay : ''}</td>
                                    <td class="tdTien">${item.donGia}</td>
                                    <td class="tdTien">${item.tongTien}</td>
                                    <td>
                                        <a href="#" class="btn btn-sm btn-info"
                                           data-bs-toggle="modal"
                                           data-bs-target="#addBenhNhanModal"
                                           data-url="/BenhNhan/Edit/${item.maBenhNhan}"
                                           title="Sửa">
                                            <i class="bi bi-pencil-square"></i>
                                        </a>
                                        <form action="/BenhNhan/Delete/${item.maBenhNhan}" method="post" style="display:inline;">
                                            <button type="submit" class="btn btn-sm btn-danger" onclick="return confirm('Bạn có chắc muốn xóa?');">
                                                <i class="bi bi-trash"></i>
                                            </button>
                                        </form>
                                    </td>
                                </tr>`;
                        tbody.append(row);
                    });
                } else {
                    tbody.append(`<tr><td colspan="13" style="text-align:center;">Không tìm thấy kết quả.</td></tr>`);
                }
            },
            error: function (xhr) {
                console.error("Lỗi khi tìm kiếm:", xhr.responseText);
            }
        });
    });
});
