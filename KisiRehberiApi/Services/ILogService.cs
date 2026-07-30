using System.Threading.Tasks;
public interface ILogService
{
    Task LogAsync(string userName, string actionType, string details);
}//log tablosu için arayüz oluşturma işlemini yapıyoruz