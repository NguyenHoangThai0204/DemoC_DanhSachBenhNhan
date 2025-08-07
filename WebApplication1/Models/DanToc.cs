using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("Dantoc")]
    public class DanToc
    {
        // id của dân tộc
        [Key]
        public int Id { get; set; }
        // mã dân tộc
        public string MaDanToc { get; set; }
        // tên dân tộc
        public string TenDanToc { get; set; }
        // mảng các chữ viết tắt của dân tộc

        //public string[]? VietTat { get; set; }    
        // ⚠️ Đây là chuỗi JSON được lưu trong DB
        public string? VietTat { get; set; }
        [NotMapped]
        public string[] VietTatArray
        {
            get => string.IsNullOrEmpty(VietTat)
                ? Array.Empty<string>()
                : System.Text.Json.JsonSerializer.Deserialize<string[]>(VietTat);
            set => VietTat = System.Text.Json.JsonSerializer.Serialize(value);
        }

    }
}
