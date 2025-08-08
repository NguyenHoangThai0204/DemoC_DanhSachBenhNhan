namespace WebApplication1.Models
{
    public class BenhNhanSTO
    {
        public string MaBenhNhan { get; set; }
        public string HoTen { get; set; }
        public string GioiTinh { get; set; }
        public string TenDanToc { get; set; }
        public DateTime? NgaySinh { get; set; }
        public DateTime NgayNhapVien { get; set; }
        public DateTime? NgayXuatVien { get; set; }
        public decimal? DonGia { get; set; }
        public string TenTinh { get; set; }
        public int? SoNgayNhapVien { get; set; }
        public decimal? TongTien { get; set; }
    }
}
