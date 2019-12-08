using Login.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Login.BusinessLayer
{
    public class RepositoryBase
    {
        protected static DatabaseContext _db;
        private static object _lockSync = new object();
        protected RepositoryBase()
        {
            //repository base oluşturulurken singleton desing pattenini kullandık çünkü databasecontext program boyunca sadece 1 adet new lenmelidir.
            //RepositoryBase class ile aynı isimli oluşturulmuş method constructordır repository içinde extend edip yani miras alındığında ilk olurak buraya gelir
            CreateContext();
        }
        public static void CreateContext()
        {
            if (_db == null)
            {
                lock (_lockSync)
                {
                    //locksync signleton desing patteninin bir yapısıdır ekstra koruma sağlamak için kullanılır daha önce oluşturulmuşsa buraya giriş yapmaz
                    //bu sayede program boyunca databasecontext nesnesi sadece 1 kere new lenmiş olduğu kesinleşir
                    if (_db == null)
                    {
                        _db = new DatabaseContext();
                    }
                }
            }
        }
    }
}
