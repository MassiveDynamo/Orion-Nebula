using System.ComponentModel.DataAnnotations;

namespace Data.Models
{
    public class EDSystemName
    {
        [Key]
        public long Id64 { get; set; }
   
        [MaxLength(200)]
        public string Name { get; set; }
        
        public EDSystemName()
        {
            Id64 = 0;
            Name = string.Empty;
        }
        public EDSystemName(long id64, string name)
        {
            Id64 = id64;
            Name = name;
        }
    }
}
