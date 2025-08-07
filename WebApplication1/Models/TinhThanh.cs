using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("TinhThanh")]
    public class TinhThanh
    {
        // id của dân tộc
        [Key]
        public int Id { get; set; }
        // mã dân tộc
        public string MaTinh { get; set; }
        // tên tỉnh
        public string TenTinh { get; set; }
        public string? VietTat { get; set; }
        [NotMapped]
        public List<string> VietTatArray
        {
            get
            {
                if (string.IsNullOrWhiteSpace(VietTat))
                    return new List<string>();

                try
                {
                    return JsonConvert.DeserializeObject<List<string>>(VietTat) ?? new List<string>();
                }
                catch
                {
                    return new List<string>(); // hoặc throw nếu bạn muốn bắt lỗi cụ thể
                }
            }
            set
            {
                VietTat = JsonConvert.SerializeObject(value ?? new List<string>());
            }
        }
    }
}
