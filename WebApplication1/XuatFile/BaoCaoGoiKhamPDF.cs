using DocumentFormat.OpenXml.VariantTypes;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApplication1.ModelSTOs;

public class BaoCaoGoiKhamPDF : IDocument
{
    private readonly List<GoiKhamSTO> _data;
    private readonly string _fromDate;
    private readonly string _toDate;

    public BaoCaoGoiKhamPDF(List<GoiKhamSTO> data, string fromDate, string toDate)
    {
        _data = data;

        // Nếu ngày tháng không được cung cấp, tự động tính toán
        if (string.IsNullOrEmpty(fromDate) || string.IsNullOrEmpty(toDate))
        {
            if (data.Any())
            {
                _fromDate = data.Min(x => x.NgayDangKy).ToString("dd-MM-yyyy");
                _toDate = data.Max(x => x.NgayDangKy).ToString("dd-MM-yyyy");
            }
            else
            {
                _fromDate = DateTime.Now.ToString("dd-MM-yyyy");
                _toDate = DateTime.Now.ToString("dd-MM-yyyy");
            }
        }
        else
        {
            _fromDate = fromDate;
            _toDate = toDate;
        }
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(20);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(12).FontColor(Colors.Black));

            page.Content()
                .Column(column =>
                {
                    // Logo và tiêu đề
                    column.Item()
                        .Row(row =>
                        {
                            row.RelativeColumn()
                                  .Row(innerRow =>
                                  {
                                      innerRow.ConstantColumn(61)
                                          .Column(logoColumn =>
                                          {
                                              logoColumn.Item()
                                                  .Width(60)
                                                  .Height(60)
                                                  .Image("wwwroot/dist/img/logo.png", ImageScaling.FitArea);
                                          });

                                      innerRow.RelativeColumn()
                                          .PaddingLeft(2)
                                          .Column(infoColumn =>
                                          {
                                              infoColumn.Item().Text("BỆNH VIỆN A").Bold().FontSize(12);
                                              infoColumn.Item().Text("Địa chỉ: 123 Đường ABC, Quận XYZ, TP.HCM").FontSize(10);
                                              infoColumn.Item().Text("Điện thoại: (028) 1234 5678").FontSize(10);
                                          });
                                  });

                            row.RelativeColumn()
                                .Column(nationalColumn =>
                                {
                                    nationalColumn.Item()
                                        .AlignRight()
                                        .Text("BÁO CÁO THỰC HIỆN THEO DÕI GÓI KHÁM BỆNH ")
                                        .FontFamily("Times New Roman")
                                        .FontSize(13)
                                        .Bold();

                                    nationalColumn.Item()
                                        .AlignRight()
                                        .Text("Đơn vị thống kê")
                                        .FontSize(11)
                                        .FontFamily("Times New Roman");

                                    nationalColumn.Item()
                                         .AlignRight()
                                         .Text(text =>
                                         {
                                             text.DefaultTextStyle(TextStyle.Default.FontSize(10).SemiBold());

                                             if (_fromDate == _toDate)
                                                 text.Span($"Ngày: {_fromDate}");
                                             else
                                                 text.Span($"Từ ngày: {_fromDate} đến ngày: {_toDate}");
                                         });
                                });
                        });

                    column.Item().PaddingTop(5);

                    // Bảng dữ liệu
                    column.Item()
                        .Table(table =>
                        {
                            // Định nghĩa cột
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(2f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(2f);
                            });

                            // Header
                            table.Header(header =>
                            {
                                void AddHeaderCell(string text)
                                {
                                    header.Cell()
                                        .Border(1)
                                        .BorderColor(Colors.Grey.Darken1)
                                        .Background(Colors.Grey.Lighten3)
                                        .PaddingVertical(2)
                                        .PaddingHorizontal(3)
                                        .AlignCenter()
                                        .AlignMiddle()
                                        .Text(text)
                                        .Bold()
                                        .FontSize(12);
                                }

                                AddHeaderCell("STT");
                                AddHeaderCell("Mã y tế");
                                AddHeaderCell("Họ và tên");
                                AddHeaderCell("Gói khám");
                                AddHeaderCell("Ngày đăng ký");
                                AddHeaderCell("Trạng thái");
                                AddHeaderCell("Chỉ định còn lại");
                                AddHeaderCell("Ghi chú");
                            });

                            // Body (tbody)
                            int stt = 1;
                            foreach (var item in _data)
                            {
                                table.Cell().Element(c => CellStyle(c)).AlignCenter().Text(stt++);
                                table.Cell().Element(c => CellStyle(c)).Text(item.MaYTe);
                                table.Cell().Element(c => CellStyle(c)).Text(item.HoTen);
                                table.Cell().Element(c => CellStyle(c)).Text(item.GoiKham);
                                table.Cell().Element(c => CellStyle(c)).AlignCenter().Text(item.NgayDangKy.ToString("dd-MM-yyyy"));
                                table.Cell().Element(c => CellStyle(c)).Text(item.TrangThaiThucHien);
                                table.Cell().Element(c => CellStyle(c)).Text(item.ChiDinhConLai);
                                table.Cell().Element(c => CellStyle(c)).Text(item.GhiChu);
                            }
                        });

                    column.Item().PaddingTop(15);

                    column.Item()
                        .AlignRight()
                        .PaddingRight(39)
                        .Text($"Ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}")
                        .FontSize(10)
                        .Italic();

                    // Bảng chữ ký
                    column.Item().PaddingTop(3)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            void AddSignCell(string title, string note)
                            {
                                table.Cell().Element(cell =>
                                    cell.Column(col =>
                                    {
                                        col.Item().AlignCenter().Text(title).Bold().FontSize(11);
                                        col.Item().AlignCenter().PaddingTop(5).Text(note).Italic().FontSize(9);
                                    })
                                );
                            }

                            AddSignCell("THỦ TRƯỞNG ĐƠN VỊ", "(Ký, họ tên, đóng dấu)");
                            AddSignCell("THỦ QUỸ", "(Ký, họ tên)");
                            AddSignCell("KẾ TOÁN", "(Ký, họ tên)");
                            AddSignCell("NGƯỜI LẬP BẢNG", "(Ký, họ tên)");
                        });

                    column.Item().PaddingTop(80);
                });

            // Footer
            page.Footer()
                .AlignRight()
                .Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
        });
    }

    // Style cho các ô dữ liệu (tbody)
    private IContainer CellStyle(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Medium)
            .PaddingVertical(5)
            .PaddingHorizontal(3)
            .Background(Colors.White)
            .AlignMiddle() // canh giữa theo chiều cao
            .DefaultTextStyle(TextStyle.Default.FontSize(10));
    }

}
