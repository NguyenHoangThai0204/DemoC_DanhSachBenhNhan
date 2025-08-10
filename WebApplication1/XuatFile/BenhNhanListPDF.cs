
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using WebApplication1.ModelSTOs;
public class BenhNhanListPDF : IDocument
{
    private readonly List<BenhNhanSTO> _data;
    //private readonly List<BenhNhan> _data;

    public BenhNhanListPDF(List<BenhNhanSTO> data)
    {
        _data = data;
    }
    //public BenhNhanListPDF(List<BenhNhan> data)
    //{
    //    _data = data;
    //}

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            // Thiết lập trang
            page.Size(PageSizes.A4);
            page.Margin(20);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(12).FontColor(Colors.Black));

            // Phần header
            page.Header()
                .Padding(10)
                .Column(headerColumn =>
                {
                    // Dòng 1: Thông tin bệnh viện và quốc hiệu
                    headerColumn.Item()
                        .Row(row =>
                        {
                            // Cột thông tin bệnh viện
                            row.RelativeColumn()
    .Column(infoColumn =>
    {
        infoColumn.Item().Text("BỆNH VIỆN A").Bold();
        infoColumn.Item().Text("Địa chỉ: 123 Đường ABC, Quận XYZ, TP.HCM");
        infoColumn.Item().Text("Điện thoại: (028) 1234 5678");
    });

                            // Cột quốc hiệu
                            row.RelativeColumn()
                                .Column(nationalColumn =>
                                {
                                    nationalColumn.Item()
                                        .AlignCenter()
                                        .Text("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM")
                                        .FontFamily("Times New Roman")
                                        .Bold();

                                    nationalColumn.Item()
                                        .AlignCenter()
                                        .Text("Độc lập - Tự do - Hạnh phúc")
                                        .FontFamily("Times New Roman")
                                        .Italic();

                                    nationalColumn.Item()
                                        .AlignCenter()
                                        .Text("---o0o---")
                                        .FontFamily("Times New Roman");
                                });
                        });

                    // Tiêu đề chính
                    headerColumn.Item()
                        .PaddingVertical(15)
                        .AlignCenter()
                        .Text("DANH SÁCH BỆNH NHÂN")
                        .Bold()
                        .FontSize(16)
                        .FontColor(Colors.Blue.Medium);

                    // Ngày xuất file
                    headerColumn.Item()
                        .PaddingBottom(15)
                        .AlignRight()
                        .Text($"Ngày xuất file: {DateTime.Now:dd-MM-yyyy HH:mm}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Medium);
                });

            // Phần nội dung chính
            //page.Content()
            //    .PaddingVertical(10)
            //    .Table(table =>
            //    {
            //        // Định nghĩa cột
            //        table.ColumnsDefinition(columns =>
            //        {
            //            columns.ConstantColumn(25); // STT
            //            columns.RelativeColumn(); // Mã BN
            //            columns.RelativeColumn(2); // Họ tên (rộng hơn)
            //            columns.RelativeColumn(); // Ngày sinh
            //            columns.RelativeColumn(); // Giới tính
            //            columns.RelativeColumn(); // Ngày nhập viện
            //            columns.RelativeColumn(); // Tổng tiền
            //        });

            //        // Tiêu đề bảng
            //        table.Header(header =>
            //        {
            //            header.Cell().Element(CellStyle).Text("STT");
            //            header.Cell().Element(CellStyle).Text("Mã BN");
            //            header.Cell().Element(CellStyle).Text("Họ tên");
            //            header.Cell().Element(CellStyle).Text("Ngày sinh");
            //            header.Cell().Element(CellStyle).Text("Giới tính");
            //            header.Cell().Element(CellStyle).Text("Ngày nhập viện");
            //            header.Cell().Element(CellStyle).Text("Tổng tiền");
            //        });

            //        // Dữ liệu bảng
            //        int stt = 1;
            //        foreach (var bn in _data)
            //        {
            //            table.Cell().Element(CellStyle).Text(stt++);
            //            table.Cell().Element(CellStyle).Text(bn.MaBenhNhan);
            //            table.Cell().Element(CellStyle).Text(bn.Nguoi?.HoTen ?? "");
            //            table.Cell().Element(CellStyle).Text(bn.Nguoi?.NgaySinh?.ToString("dd-MM-yyyy") ?? "");
            //            table.Cell().Element(CellStyle).Text(bn.Nguoi?.GioiTinh ?? "");
            //            table.Cell().Element(CellStyle).Text(bn.NgayNhapVien.ToString("dd-MM-yyyy HH:mm"));
            //            table.Cell().Element(CellStyle).AlignRight().Text($"{(bn.TongTien ?? 0):N0}");
            //        }
            //    });

            // tạo border cho table
            page.Content()
    .Table(table =>
    {
        // Định nghĩa cột (giữ nguyên)
        table.ColumnsDefinition(columns =>
        {
            columns.ConstantColumn(30); // STT
            columns.RelativeColumn(); // Mã BN
            columns.RelativeColumn(2); // Họ tên
            columns.RelativeColumn(); // Ngày sinh
            columns.RelativeColumn(); // Giới tính
            columns.RelativeColumn(); // Ngày nhập viện
            columns.RelativeColumn(); // Tổng tiền
        });

        // Header với border đậm hơn
        table.Header(header =>
        {
            void AddHeaderCell(string text)
            {
                //header.Cell()
                //    .Element(CellStyle)
                //    .Background(Colors.Grey.Lighten3) // Màu nền header
                //    .BorderBottom(2) // Border dưới dày hơn
                //    .BorderColor(Colors.Grey.Darken1)
                //    .Padding(1)
                //    .Text(text)
                //    .Bold();
                header.Cell()
                     .Border(1)
                     .BorderColor(Colors.Grey.Darken1)
                     .Background(Colors.Grey.Lighten3)
                     .PaddingVertical(2) // Giữ khoảng cách nhỏ
                     .PaddingHorizontal(3)
                     .AlignCenter() // Căn giữa ngang
                     .AlignMiddle() // QUAN TRỌNG: Căn giữa dọc
                     .Text(text)
                     .Bold()
                     .FontSize(11); // Giảm cỡ chữ

            }

            AddHeaderCell("STT");
            AddHeaderCell("Mã BN");
            AddHeaderCell("Họ và tên");
            AddHeaderCell("Ngày sinh");
            AddHeaderCell("Giới tính");
            AddHeaderCell("Ngày nhập viện");
            AddHeaderCell("Tổng tiền");
        });

        // Dữ liệu bảng
        int stt = 1;
        foreach (var bn in _data)
        {
            table.Cell().Element(CellStyle).Text(stt++);
            table.Cell().Element(CellStyle).Text(bn.MaBenhNhan);
            table.Cell().Element(CellStyle).Text(bn.HoTen ?? "");
            table.Cell().Element(CellStyle).Text(bn.NgaySinh?.ToString("dd-MM-yyyy") ?? "").AlignCenter();
            table.Cell().Element(CellStyle).Text(bn.GioiTinh ?? "");
            table.Cell().Element(CellStyle).Text(bn.NgayNhapVien.ToString("dd-MM-yyyy HH:mm"));
            table.Cell().Element(CellStyle).AlignRight().Text($"{(bn.TongTien ?? 0):N0}");
            //table.Cell().Element(CellStyle).Text(stt++);
            //table.Cell().Element(CellStyle).Text(bn.MaBenhNhan);
            //table.Cell().Element(CellStyle).Text(bn.Nguoi?.HoTen ?? "");
            //table.Cell().Element(CellStyle).Text(bn.Nguoi?.NgaySinh?.ToString("dd-MM-yyyy") ?? "");
            //table.Cell().Element(CellStyle).Text(bn.Nguoi?.GioiTinh ?? "");
            //table.Cell().Element(CellStyle).Text(bn.NgayNhapVien.ToString("dd-MM-yyyy HH:mm"));
            //table.Cell().Element(CellStyle).AlignRight().Text($"{(bn.TongTien ?? 0):N0}");
        }
    });


            // Phần footer
            page.Footer()
                .AlignCenter()
                .Text(x =>
                {
                    x.Span("Trang ");
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
        });
    }

    //private IContainer CellStyle(IContainer container)
    //{
    //    return container
    //        .PaddingVertical(5)
    //        .BorderBottom(1)
    //        .BorderColor(Colors.Grey.Lighten2);
    //}
    // có thêm border để dễ nhìn
    private IContainer CellStyle(IContainer container)
    {
        return container
            .Border(1) // Thêm border 1px cho mỗi ô
            .BorderColor(Colors.Grey.Medium) // Màu border
            .PaddingVertical(5) // Khoảng cách dọc trong ô
            .PaddingHorizontal(3) // Khoảng cách ngang trong ô
            .Background(Colors.White); // Nền trắng cho ô
    }

}

