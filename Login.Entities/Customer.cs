using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Login.Entities
{
    [Table("Customer")]
    public class Customer
    {
        //Database de oluşturulacak tablonın özelliklerinin ve sutun isimlerinin tanımlamasının yapıldığı yer
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [StringLength(50)]
        public string customerId { get; set; }
        [StringLength(50)]
        public string Password { get; set; }
        public DateTime lastLoginTime { get; set; }
        public int Stat { get; set; }
        public DateTime lastUpdateDate { get; set; }
        public int recordStat { get; set; }
        [StringLength(50)]
        public string HashType { get; set; }
        public int wrongPass { get; set; }
    }
}
