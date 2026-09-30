using System.ComponentModel.DataAnnotations;

namespace Data.Models
{
    public class EDSystemName
    {
        [Key]
        [MaxLength(200)]
        public string Name { get; set; }
        
        public EDSystemName()
        {
            Name = string.Empty;
        }
        public EDSystemName(string name)
        {
            Name = name;
        }
    }
}
