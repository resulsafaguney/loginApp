using Login.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Login.BusinessLayer
{
    public class Repository<T> : RepositoryBase where T : class
    {
        private DbSet<T> _objectSet;
        public Repository()
        {
            //burası constructordır ve Repository desing patterni kullanılıp içine hangi class yapısı verilise(GENERİC OLDUĞU İÇİN) onun üzerinden CRUD işlemlerini gerçekleştirir
            _objectSet = _db.Set<T>();
        }
        public List<T> List()
        {
            //list methodu çağırıldığında verilen class(database tablosu) için bütün verileri çeker ve listeler
            return _objectSet.ToList();
        }
        public IQueryable<T> List(Expression<Func<T, bool>> where)
        {
            //belli bir expression yani tanıma göre verileri çeker
            return _objectSet.Where(where);
        }
        public int Insert(T obj)
        {
            //database e nesneyi ekler ve kayıdı çağırır
            _objectSet.Add(obj);
            return Save();
        }
        public int Update(T obj)
        {
            //database güncelleme işlemei sadece kayıdı çağırır
            return Save();
        }
        public int Delete(T obj)
        {
            //nesneyi siler ve kayıdı çağırır
            _objectSet.Remove(obj);
            return Save();
        }
        public int Save()
        {
            return _db.SaveChanges();
        }
        public T Find(Expression<Func<T, bool>> where)
        {
            //belli bir değere göre nesneyi çağırır
            return _objectSet.FirstOrDefault(where);
        }
    }
}
