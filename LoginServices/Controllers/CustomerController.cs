using Login.BusinessLayer;
using Login.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace LoginServices.Controllers
{
    //web configden connnectionstring sql servera göre ayarlanmalıdır...
    public class CustomerController : ApiController
    {
        Repository<Customer> repo_customer = new Repository<Customer>();
        public string decrtpass;
        public HttpResponseMessage Get(string customerId,string Password)
        {
            //kullanıcının login olup olmadığının kontrolü burada yapılacaktır


            Customer findCustomerId = new Customer();//customer nesnesi oluşturulur

            findCustomerId= repo_customer.Find(x => x.customerId == customerId);//repo_customerdan gönderilen customerid li bir kullanıcı olup olmadığına bakılır

            if (findCustomerId == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Kullanıcı Adı Bulunamadı");//oluşturulan kullanıcın null olup olmadığına bakıldı
            }
            var hashtext = "albaraka"; //eşleştirme codu(şifreleme için) belirtildi

            try
            {
                var bytesToBeDecrypted = Convert.FromBase64String(findCustomerId.Password);
                var passwordBytes = Encoding.UTF8.GetBytes(Password);
                passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
                var bytesDecrypted = CustomerController.Decrypt(bytesToBeDecrypted, passwordBytes);
                decrtpass = Encoding.UTF8.GetString(bytesDecrypted);
            }
            catch (Exception)
            {
                decrtpass = null;
            }
            

            if(findCustomerId.recordStat == 2 )
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Kullanıcı İptal Edilmiş");
                //databaseden gelen oluşturduğumuz nesnenin recordstat propertiesi kontrol edilir iptal olup olmadığına bakılır recordstat =2 durumu kullanıcının iptal edildiğini söyler
            }
            else if (findCustomerId.Stat == 2 && decrtpass == hashtext)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Kullanıcı Bloke");
                //stat = 2 durumu kullanıcının bloke olduğunu söyler ama aynı zamanda passwordünde doğru olduğu kontrol edilmelidir aksi halde password yanlış olsa bile kullanıcı adı doğru olduğu takdirde kullanıcı bloke döner
            }
            else if(findCustomerId.Stat == 2)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Kullanıcı Şifresi Hatalı");
                //üst tarafta stat durumu şifre ile kontrol edildiği için kullanıcı adı doğru bile olsa şifre yanlış olduğu durumda bu seçeneğin içine girecektir.
            }
            else if(decrtpass == hashtext)
            {
                //kullanıcının login olduğu durumdur
                findCustomerId.wrongPass = 0;//kullanıcının şifreyi yanlış girmesi sıfırlanır
                findCustomerId.lastLoginTime = DateTime.Now; //last logini güncellenir
                repo_customer.Update(findCustomerId); //kullanıcı bilgileri update edilir
                return Request.CreateErrorResponse(HttpStatusCode.OK, "Kullanıcı Girişi Başarılı"); //kullanıcı girişinin başarılı olduğu dönülür
            }
            else
            {
                if(findCustomerId.wrongPass==2)
                {
                    //bu kullanıcının 3 kere hatalı şifre girdiğini gösterir
                    findCustomerId.Stat = 2; //kullanıcı bloke edilir
                    findCustomerId.lastUpdateDate = DateTime.Now; //bloke durumunun gerçekleştiği tarih değiştirilir
                    repo_customer.Update(findCustomerId); //kullanıcı güncellendiği kısım
                }
                else
                {
                    findCustomerId.wrongPass += 1; //kullanıcı hatalı şifre girdiği halde kaç kere girdiğinin bulunup 1 arttırıldığı kısım
                    repo_customer.Update(findCustomerId); //kullanıcı güncellendiği kısım
                }
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Kullanıcı Şifresi Hatalı"); //şifrenin hatalı olduğu dönülür
            }
        }
        public HttpResponseMessage Post(Customer customer)
        {
            //kullanıcı oluşturmasının yapıldığı yerdir


            Customer existing = new Customer(); // yeni bir kullanıcı nesnesi oluşturulur
            existing = repo_customer.Find(x => x.customerId == customer.customerId); //kullanıcı adının daha önce databasede bulunup bulunmadığını araştırır
            if(existing==null)
            {
                try
                {
                    var hashtext = "albaraka"; //eşleştirme codu(şifreleme için) belirtildi
                    HashAlgorithm algorithm = SHA256.Create();
                    var bytesToBeEncrypted = Encoding.UTF8.GetBytes(hashtext);
                    var passwordBytes = Encoding.UTF8.GetBytes(customer.Password);
                    passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
                    var bytesEncrypted = CustomerController.Encrypt(bytesToBeEncrypted, passwordBytes);

                    Customer newcostumer = new Customer();  //customer nesnesi oluşturur
                    newcostumer.customerId = customer.customerId;   //yeni oluşturduğun nesnenin customerıd özelliğini girersin(gelen http isteğinin body kısmından aldığın)
                    newcostumer.Password = Convert.ToBase64String(bytesEncrypted);  //yeni oluşturduğun nesnenin Password özelliğini girersin(gelen http isteğinin body kısmından aldığın)
                    newcostumer.lastLoginTime = DateTime.Now;  
                    newcostumer.Stat = 1; //kulanıcı aktif edilir
                    newcostumer.lastUpdateDate = DateTime.Now;  //son güncellemeyi kayıt zamaanı yapar
                    newcostumer.recordStat = 1;  //kullanıcı aktif edilir
                    newcostumer.HashType = "SHA256,hashtext=albaraka"; //hash algoritması ve hashtexti girilir
                    newcostumer.wrongPass = 0;  //şifreyi yanlış girme sayısı verilir

                    repo_customer.Insert(newcostumer); //kullanıcı oluşturulur
                    
                    return Request.CreateErrorResponse(HttpStatusCode.Created, "Kullanıcı Kaydı Başarılı"+" "+"Kullanıcı Adı:"+newcostumer.customerId);  //kullanıcının başarılı şekilde oluştuğu söylenir ve adı dönülür
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);     //kullanıcı oluşturulurken server tarafında hata olduğu söylenir
                }
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Kullanıcı Mevcut");  //kullanıcının mevcut olduğu söylenir
            }
        }
        public HttpResponseMessage Put(Customer customer)
        {
            //programa güncelleme seçeniği ileride eklenebilir şuan kullanılmamakta
            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Veri güncelleme yapılamadı");  
        }
        private static byte[] Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);

                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }

                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }
        private static byte[] Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);

                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }

                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }
    }
}
