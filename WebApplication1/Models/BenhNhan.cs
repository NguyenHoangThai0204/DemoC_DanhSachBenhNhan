using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("BenhNhan")]
    public class BenhNhan
    {
        [Key]
        public int MaSoTang { get; set; }

        [BindNever]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string? MaBenhNhan { get; set; }

    


        public DateTime NgayNhapVien { get; set; }
        public DateTime? NgayXuatVien { get; set; }

        public int? SoNgayNhapVien { get; set; } // Nullable vì có thể chưa xuất viện

        [Column(TypeName = "decimal(22,2)")]
        public decimal? DonGia { get; set; } // Không dùng nullable
        [Column(TypeName = "decimal(22,2)")]
        public decimal? TongTien { get; set; }
        [ForeignKey("Nguoi")]
        public int MaNguoi { get; set; }
        public Nguoi Nguoi { get; set; } // Navigation
    }
}
