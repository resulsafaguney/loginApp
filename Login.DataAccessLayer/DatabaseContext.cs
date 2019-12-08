using Login.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Login.DataAccessLayer
{
    public class DatabaseContext : DbContext
    {
        //database oluşturulacak tablaloların tanımlamasının yapıldığı yer
        public DbSet<Customer> Customer { get; set; }
    }
}
