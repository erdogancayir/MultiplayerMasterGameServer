public class AuthService
{
    private readonly DbInterface _dbInterface;

    public AuthService(DbInterface dbInterface)
    {
        _dbInterface = dbInterface;
    }

    public bool Authenticate(string username, string password)
    {
        // Veritabanında kullanıcı adı ve şifre doğrulaması yapın
        // Örnek: dbInterface.GetUserByUsernameAndPassword(username, password);
        return true; // veya false, doğrulamaya bağlı
    }
}
