using System.Threading.Tasks;
namespace KisiRehberiApi.Services;

public interface ILogService
{
    Task LogAsync(string userName, string actionType, string details);
}//log tablosu için arayüz oluşturma işlemini yapıyoruz