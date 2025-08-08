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

DROP PROCEDURE sp_GetBenhNhanPaged;