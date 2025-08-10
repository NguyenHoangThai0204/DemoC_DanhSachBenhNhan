use phongkham2 
go

-- Create TinhThanh table
CREATE TABLE TinhThanh (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MaTinh NVARCHAR(50) NOT NULL,
    TenTinh NVARCHAR(100) NOT NULL,
    VietTat NVARCHAR(MAX) NULL
);

ALTER TABLE TinhThanh
ADD Active BIT NOT NULL DEFAULT 1;

select * from TinhThanh

-- Create DanToc table
CREATE TABLE DanToc (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MaDanToc NVARCHAR(50) NOT NULL,
    TenDanToc NVARCHAR(100) NOT NULL,
    VietTat NVARCHAR(MAX) NULL
);

ALTER TABLE DanToc
ADD Active BIT NOT NULL DEFAULT 1;

select * from DanToc;

-- Create Nguoi table
CREATE TABLE Nguoi (
    MaNguoi INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    NgaySinh DATE NULL,
    GioiTinh NVARCHAR(10) NULL,
    DanTocId INT NULL,
    TinhThanhId INT NULL,
    CONSTRAINT FK_Nguoi_DanToc FOREIGN KEY (DanTocId) REFERENCES DanToc(Id),
    CONSTRAINT FK_Nguoi_TinhThanh FOREIGN KEY (TinhThanhId) REFERENCES TinhThanh(Id)
);

-- Create BenhNhan table with computed MaBenhNhan
CREATE TABLE BenhNhan (
    MaSoTang INT IDENTITY(1,1) PRIMARY KEY,
    MaBenhNhan AS ('BN' + RIGHT('000000' + CAST(MaSoTang AS VARCHAR(10)), 6)) PERSISTED,
    NgayNhapVien DATETIME NOT NULL,
    NgayXuatVien DATETIME NULL,
    SoNgayNhapVien INT NULL,
    DonGia DECIMAL(22,2) NULL,
    TongTien DECIMAL(22,2) NULL,
    MaNguoi INT NOT NULL,
    CONSTRAINT FK_BenhNhan_Nguoi FOREIGN KEY (MaNguoi) REFERENCES Nguoi(MaNguoi)
);

ALTER TABLE BenhNhan
ADD Active BIT NOT NULL DEFAULT 1;

select * from BenhNhan;

-- Create indexes for better performance
CREATE UNIQUE INDEX IX_TinhThanh_MaTinh ON TinhThanh(MaTinh);
CREATE UNIQUE INDEX IX_DanToc_MaDanToc ON DanToc(MaDanToc);
CREATE UNIQUE INDEX IX_BenhNhan_MaBenhNhan ON BenhNhan(MaBenhNhan);

INSERT INTO Dantoc (MaDanToc, TenDanToc, VietTat)
VALUES
(N'KIN', N'Kinh', N'["kin","k","vn"]'),
(N'TAY', N'Tày', N'["tay","t","ty"]'),
(N'THA', N'Thái', N'["thai","tha"]'),
(N'MNG', N'Mường', N'["muong","m","mn"]'),
(N'HMN', N'Hmong', N'["hmong","hm","hmo"]'),
(N'DAO', N'Dao', N'["dao","d","do"]'),
(N'HOA', N'Hoa', N'["hoa","h"]'),
(N'KHM', N'Khmer', N'["khmer","kh","km"]'),
(N'NUN', N'Nùng', N'["nung","nu","nn"]'),
(N'BAY', N'Bố Y', N'["boy","by"]'),
(N'CKR', N'Cờ Lao', N'["cola o"]'),
(N'CO', N'Cơ', N'["co"]'),
(N'CHO', N'Chơ Ro', N'["choro","cr"]'),
(N'CHU', N'Chu Ru', N'["churu","chu"]'),
(N'CHT', N'Chứt', N'["chut","cht"]'),
(N'CNQ', N'Cống', N'["cong","cq"]'),
(N'CTU', N'Cơ Tu', N'["cot u"]'),
(N'ED E', N'Ê Đê', N'["ede","ede"]'),
(N'GLR', N'Gia Rai', N'["giarai","glr"]'),
(N'GIAY', N'Giáy', N'["giay","gy"]'),
(N'GTR', N'Giẻ Triêng', N'["gietrieng","gtr"]'),
(N'HNI', N'Hà Nhì', N'["hanii","hni"]'),
(N'HRE', N'Hrê', N'["hre"]'),
(N'KHMU', N'Khơ Mú', N'["khomu","khmu"]'),
(N'KHA', N'Kháng', N'["khang","kh"]'),
(N'LCH', N'La Chí', N'["lach i"]'),
(N'LAH', N'La Ha', N'["laha"]'),
(N'LHU', N'La Hủ', N'["lahu"]'),
(N'LAO', N'Lào', N'["lao"]'),
(N'LLO', N'Lô Lô', N'["lolo","llo"]'),
(N'LU', N'Lự', N'["lu"]'),
(N'MA', N'Mạ', N'["ma"]'),
(N'MAN', N'Mảng', N'["mang"]'),
(N'MNONG', N'Mnông', N'["mnong"]'),
(N'NGAI', N'Ngái', N'["ngai"]'),
(N'ODU', N'Ơ Đu', N'["odu"]'),
(N'PA THEN', N'Pà Thẻn', N'["pathen"]'),
(N'PHULA', N'Phù Lá', N'["phula"]'),
(N'PUPEO', N'Pu Péo', N'["pupeo"]'),
(N'RAG', N'Ra Glai', N'["raglai"]'),
(N'ROMAM', N'Rơ Măm', N'["romam"]'),
(N'SACHAY', N'Sán Chay', N'["sanchay"]'),
(N'SANDIU', N'Sán Dìu', N'["sandiu"]'),
(N'SILA', N'Si La', N'["sila"]'),
(N'TAOI', N'Tà Ôi', N'["taoi"]'),
(N'THO', N'Thổ', N'["tho"]'),
(N'XINHMUN', N'Xinh Mun', N'["xinhmun"]'),
(N'XODANG', N'Xơ Đăng', N'["xodang"]'),
(N'XTIENG', N'Xtiêng', N'["xtieng"]');
/* ====================================================== Dùng STO ==================================================*/

/* dạng nâng cao của xuất danh sách theo trang*/
CREATE PROCEDURE sp_GetBenhNhanPaged
    @PageNumber INT,
    @PageSize INT
AS
BEGIN
SELECT 
    b.MaBenhNhan, 
    n.HoTen, 
    n.GioiTinh, 
    n.NgaySinh,
    d.TenDanToc,     -- ✅ Hiển thị tên dân tộc thay vì ID
    b.NgayNhapVien, 
    b.NgayXuatVien,
    b.DonGia,
    t.TenTinh,       -- ✅ Hiển thị tên tỉnh
    b.SoNgayNhapVien,
    b.TongTien
FROM BenhNhan b
JOIN Nguoi n ON b.MaNguoi = n.MaNguoi
LEFT JOIN DanToc d ON n.DanTocId = d.Id
LEFT JOIN TinhThanh t ON n.TinhThanhId = t.Id
WHERE b.Active = 1
    ORDER BY  b.MaBenhNhan asc
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY
END

/* procedure lấy danh sách bệnh nhân*/
CREATE PROCEDURE sp_GetBenhNhan
AS
BEGIN
SELECT 
    b.MaBenhNhan, 
    n.HoTen, 
    n.GioiTinh, 
    n.NgaySinh,
    d.TenDanToc,     -- ✅ Hiển thị tên dân tộc thay vì ID
    b.NgayNhapVien, 
    b.NgayXuatVien,
    b.DonGia,
    t.TenTinh,       -- ✅ Hiển thị tên tỉnh
    b.SoNgayNhapVien,
    b.TongTien
FROM BenhNhan b
JOIN Nguoi n ON b.MaNguoi = n.MaNguoi
LEFT JOIN DanToc d ON n.DanTocId = d.Id
LEFT JOIN TinhThanh t ON n.TinhThanhId = t.Id
    ORDER BY  n.HoTen DESC
END



CREATE PROCEDURE sp_GetBenhNhanTuNgayDenNgay
    @TuNgay nvarchar(10),  -- Thêm độ dài
    @DenNgay nvarchar(10)  -- Thêm độ dài
AS
BEGIN
SELECT 
    b.MaBenhNhan, 
    n.HoTen, 
    n.GioiTinh, 
    n.NgaySinh,
    d.TenDanToc,     -- ✅ Hiển thị tên dân tộc thay vì ID
    b.NgayNhapVien, 
    b.NgayXuatVien,
    b.DonGia,
    t.TenTinh,       -- ✅ Hiển thị tên tỉnh
    b.SoNgayNhapVien,
    b.TongTien
FROM BenhNhan b
JOIN Nguoi n ON b.MaNguoi = n.MaNguoi
LEFT JOIN DanToc d ON n.DanTocId = d.Id
LEFT JOIN TinhThanh t ON n.TinhThanhId = t.Id
WHERE CONVERT(Date, b.NgayNhapVien, 103) 
          BETWEEN CONVERT(date, @TuNgay, 103) 
              AND CONVERT(date, @DenNgay, 103)
ORDER BY  n.HoTen DESC
END

DROP PROCEDURE sp_DanhSachGoiKhamBenhTheoNgay;

/* ====================================================== Biểu mẫu báo cáo của thầy ==================================================*/

-- Create Nguoi table
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


select * from GoiKhamBenh
