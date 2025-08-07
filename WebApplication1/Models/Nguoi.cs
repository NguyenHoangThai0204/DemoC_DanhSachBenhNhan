using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("Nguoi")]
    public class Nguoi
    {
        [Key]
        public int MaNguoi { get; set; }

        public string HoTen { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string GioiTinh { get; set; }

        // Đây là khóa ngoại đến DanToc
        public int? DanTocId { get; set; }

        public DanToc? DanToc { get; set; }

        // Đây là khóa ngoại đến TinhThanh
        public int? TinhThanhId { get; set; }
        public TinhThanh? TinhThanh { get; set; }


        //public string TinhThanh { get; set; }
    }
}
