use [phongkhamdakhoa]
go 

CREATE TABLE Nguoi (
    MaNguoi INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    NgaySinh DATE NOT NULL,
    GioiTinh NVARCHAR(10) NOT NULL,
    TinhThanh NVARCHAR(100),
    DanToc NVARCHAR(100)
);

CREATE TABLE BenhNhan (
    MaSoTang INT IDENTITY(1,1) PRIMARY KEY, -- không show ra, chỉ để tạo mã
    MaBenhNhan AS ('BN' + RIGHT('000' + CAST(MaSoTang AS VARCHAR(3)), 3)) PERSISTED,
    MaNguoi INT FOREIGN KEY REFERENCES Nguoi(MaNguoi),
    NgayNhapVien DATE,
    NgayXuatVien DATE
);
ALTER TABLE BenhNhan ADD MaBenhNhan AS ('BN' + RIGHT('000' + CAST(MaSoTang AS VARCHAR(10)), 3)) PERSISTED;
DROP TABLE [dbo].[BenhNhan];

ALTER TABLE BenhNhan 
ALTER COLUMN NgayNhapVien DATETIME;

ALTER TABLE BenhNhan ALTER COLUMN NgayNhapVien DATETIME2(0);

INSERT INTO Nguoi (HoTen, NgaySinh, GioiTinh, TinhThanh, DanToc)
VALUES 
(N'Trần Thị 5', '1995-06-20', N'Nữ', N'Hồ Chí Minh', N'Kinh');


INSERT INTO BenhNhan (MaNguoi, NgayNhapVien, NgayXuatVien)
VALUES 
(2, '2025-07-01', '2025-07-10');

SELECT 
    b.MaBenhNhan, 
    n.HoTen, 
    n.GioiTinh, 
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
LEFT JOIN TinhThanh t ON n.TinhThanhId = t.Id;


SELECT * FROM BenhNhan 

WHERE ISNUMERIC(DonGia) = 0 OR ISNUMERIC(TongTien) = 0;


ALTER TABLE BenhNhan
ALTER COLUMN TongTien decimal(22,2) NULL; 

select * from TinhThanh

delete from TinhThanh
where MaTinh = 'HB'

ALTER TABLE BenhNhan
ADD DonGia decimal(18,2) NULL;

CREATE TABLE Dantoc (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MaDanToc VARCHAR(20) COLLATE Vietnamese_CI_AS NOT NULL UNIQUE,
    TenDanToc NVARCHAR(100) COLLATE Vietnamese_CI_AS NOT NULL,
    VietTat NVARCHAR(MAX) COLLATE Vietnamese_CI_AS
);

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

drop table Dantoc

ALTER TABLE Nguoi
ADD DanTocId INT;

ALTER TABLE Nguoi
ADD CONSTRAINT FK_Nguoi_DanToc
FOREIGN KEY (DanTocId) REFERENCES DanToc(Id);

ALTER TABLE Nguoi
DROP CONSTRAINT FK_Nguoi_DanToc;

ALTER TABLE DanToc
ADD Active INT DEFAULT 1;

UPDATE DanToc
SET Active = 1
WHERE Active IS NULL;

select * from Dantoc

CREATE TABLE TinhThanh (
   Id INT IDENTITY(1,1) PRIMARY KEY,
    MaTinh VARCHAR(20) COLLATE Vietnamese_CI_AS NOT NULL UNIQUE,
    TenTinh NVARCHAR(100) COLLATE Vietnamese_CI_AS NOT NULL,
    VietTat NVARCHAR(MAX) COLLATE Vietnamese_CI_AS
);

ALTER TABLE Nguoi DROP COLUMN TinhThanh;

ALTER TABLE Nguoi
ADD TinhThanhId INT;

ALTER TABLE Nguoi
ADD CONSTRAINT FK_Nguoi_TinhThanh
FOREIGN KEY (TinhThanhId) REFERENCES TinhThanh(Id);

ALTER TABLE Nguoi
DROP CONSTRAINT FK_Nguoi_TinhThanh;

drop table TinhThanh

ALTER TABLE TinhThanh
ADD Active INT DEFAULT 1;

UPDATE TinhThanh
SET Active = 1
WHERE Active IS NULL;

select * from TinhThanh

select * from Nguoi


-- Bước 1: Xóa bảng BenhNhan trước (vì phụ thuộc vào Nguoi)
DELETE FROM BenhNhan
WHERE MaNguoi IN (
    SELECT n.MaNguoi
    FROM Nguoi n
    LEFT JOIN DanToc d ON n.DanTocId = d.ID
    WHERE d.ID IS NULL
);

-- Bước 2: Xóa người không có dân tộc
DELETE FROM Nguoi
WHERE MaNguoi = 1025;





CREATE TABLE TinhThanh (
    Id INT IDENTITY(1,1) PRIMARY KEY,      -- Tự động tăng
    MaTinh VARCHAR(20) NOT NULL UNIQUE,  -- Mã do người nhập
    TenTinh VARCHAR(100) NOT NULL,       -- Tên dân tộc
    VietTat NVARCHAR(MAX)                  -- Lưu mảng viết tắt dạng chuỗi JSON
);

select * from Nguoi

select * from Nguoi where CONVERT(date, NgaySinh, 103) = CONVERT(date, '15-05-2001', 103)

-- Thêm cột SoNgayNhapVien (cho phép NULL)
ALTER TABLE BenhNhan
ADD SoNgayNhapVien INT NULL;

ALTER TABLE BenhNhan
ADD TongTien DECIMAL(12,2) NULL;


ALTER TABLE Nguoi
ADD MaDanToc INT NULL;



select * from Nguoi 
select * from BenhNhan


ALTER AUTHORIZATION ON DATABASE::phongkhamdakhoa TO [LAPTOP-QIG4MMTG\Asus];




/* dạng nâng cao của xuất danh sách dùng STO*/
CREATE PROCEDURE sp_GetBenhNhanPaged
    @PageNumber INT,
    @PageSize INT
AS
BEGIN
SELECT 
    b.MaBenhNhan, 
    n.HoTen, 
    n.GioiTinh, 
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
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY
END

DROP PROCEDURE sp_GetBenhNhanPaged;