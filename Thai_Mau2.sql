CREATE TABLE Thai_Mau2 (
    MaGoiKham INT IDENTITY(1,1) PRIMARY KEY,
    MaYTe NVARCHAR(20),
    HoTen NVARCHAR(100) NOT NULL,
    GoiKham NVARCHAR(50) NULL, 
    NgayDangKy DATE NULL,
    TrangThaiThucHien NVARCHAR(100) NOT NULL,
    ChiDinhConLai NVARCHAR(MAX) NULL,
    GhiChu NVARCHAR(MAX) NULL,
    IdChiNhanh INT,
    Active BIT,
    NgayTao DATETIME DEFAULT GETDATE()
);

INSERT INTO Thai_Mau2 (MaYTe, HoTen, GoiKham, NgayDangKy, TrangThaiThucHien, ChiDinhConLai, GhiChu, IdChiNhanh, Active)
VALUES 
('BN001', N'Nguyễn Văn A', N'Gói khám tổng quát', '2025-07-05', N'Đã hoàn thành', N'Không', N'Bệnh nhân khỏe mạnh', 1, 1),
('BN002', N'Trần Thị B', N'Gói khám sức khỏe định kỳ', '2025-07-10', N'Đang thực hiện', N'Xét nghiệm máu, siêu âm', N'Cần kiểm tra lại sau 1 tuần', 1, 1),
('BN003', N'Lê Văn C', N'Gói khám chuyên sâu tim mạch', '2025-07-15', N'Chưa thực hiện', N'Tất cả các chỉ định', N'Hẹn ngày 20/7/2025', 1, 1),
('BN004', N'Phạm Thị D', N'Gói khám phụ khoa', '2025-07-16', N'Đã hoàn thành', N'Không', N'Kê đơn thuốc 7 ngày', 1, 1),
('BN005', N'Hoàng Văn E', N'Gói khám tiền hôn nhân', '2025-07-20', N'Đang thực hiện', N'Xét nghiệm nước tiểu', N'Chờ kết quả xét nghiệm', 1, 1),
('BN006', N'Vũ Thị F', N'Gói khám tổng quát', '2025-07-20', N'Đã hoàn thành', N'Không', N'Bệnh nhân cần tái khám sau 6 tháng', 1, 1),
('BN007', N'Đặng Văn G', N'Gói khám sức khỏe lái xe', '2025-07-20', N'Chưa thực hiện', N'Tất cả các chỉ định', N'Chưa đóng phí', 1, 1),
('BN008', N'Bùi Thị H', N'Gói khám thai sản', '2025-08-01', N'Đang thực hiện', N'Siêu âm thai', N'Thai 12 tuần tuổi', 1, 1),
('BN009', N'Mai Văn I', N'Gói khám tổng quát', '2025-08-02', N'Đã hoàn thành', N'Không', N'Bệnh nhân huyết áp cao', 1, 1),
('BN010', N'Lý Thị K', N'Gói khám sức khỏe xin việc', '2025-08-03', N'Đã hoàn thành', N'Không', N'Đủ điều kiện sức khỏe', 1, 1),
('BN011', N'Nguyễn A', N'Gói khám tổng quát', '2025-07-05', N'Đã hoàn thành', N'Không', N'Bệnh nhân khỏe mạnh', 1, 1),
('BN012', N'Trần B', N'Gói khám sức khỏe định kỳ', '2025-07-10', N'Đang thực hiện', N'Xét nghiệm máu, siêu âm', N'Cần kiểm tra lại sau 1 tuần', 1, 1),
('BN013', N'Lê C', N'Gói khám chuyên sâu tim mạch', '2025-07-15', N'Chưa thực hiện', N'Tất cả các chỉ định', N'Hẹn ngày 20/7/2025', 1, 1),
('BN014', N'Phạm D', N'Gói khám phụ khoa', '2025-07-16', N'Đã hoàn thành', N'Không', N'Kê đơn thuốc 7 ngày', 1, 1),
('BN015', N'Hoàng E', N'Gói khám tiền hôn nhân', '2025-07-20', N'Đang thực hiện', N'Xét nghiệm nước tiểu', N'Chờ kết quả xét nghiệm', 1, 1),
('BN016', N'Vũ F', N'Gói khám tổng quát', '2025-07-20', N'Đã hoàn thành', N'Không', N'Bệnh nhân cần tái khám sau 6 tháng', 1, 1),
('BN017', N'Đặng G', N'Gói khám sức khỏe lái xe', '2025-07-20', N'Chưa thực hiện', N'Tất cả các chỉ định', N'Chưa đóng phí', 1, 1);

CREATE PROCEDURE sp_DanhSachGoiKhamBenhTheoNgay
@IdChiNhanh INT
AS
BEGIN
    SELECT 
        MaYTe,
        HoTen,
        GoiKham, 
        NgayDangKy,
        TrangThaiThucHien,
        ChiDinhConLai,
        GhiChu 
    FROM Thai_Mau2
    WHERE IdChiNhanh = @IdChiNhanh
    ORDER BY NgayDangKy DESC;
END

select * from Thai_Mau2 where CONVERT(date, NgayDangKy, 103) = CONVERT(date, '01-08-2025', 103)

CREATE PROCEDURE sp_GoiKhamBenhLocTheoNgay
    @TuNgay NVARCHAR(10), 
    @DenNgay NVARCHAR(10),
    @IdChiNhanh INT
AS
BEGIN
    SELECT 
        MaYTe,
        HoTen,
        GoiKham, 
        NgayDangKy,
        TrangThaiThucHien,
        ChiDinhConLai,
        GhiChu 
    FROM Thai_Mau2
    WHERE CONVERT(DATE, NgayDangKy, 103) 
          BETWEEN CONVERT(DATE, @TuNgay, 103) 
              AND CONVERT(DATE, @DenNgay, 103)
      AND IdChiNhanh = @IdChiNhanh
    ORDER BY NgayDangKy asc;
END