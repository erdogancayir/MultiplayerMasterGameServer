public class GameServerManager
{
    private List<GameServer> _gameServers; // Oyun sunucularınızın listesi

    public GameServerManager()
    {
        _gameServers = new List<GameServer>();
        // Burada, mevcut oyun sunucularınızı yükleyin veya yapılandırın
    }

    // Oyuncuyu uygun bir oyun sunucusuna atama
    public GameServer AllocatePlayerToServer(string playerId)
    {
        // Burada, oyuncuyu uygun bir oyun sunucusuna atayacak algoritmanızı uygulayın
        // Örnek: Yük dengesi, oyun türüne göre seçim vb.
        // Şimdilik basit bir örnek:
        return _gameServers.FirstOrDefault(); // İlk sunucuyu döndür
    }

    // Sunucu listesini güncelleme
    public void UpdateGameServers(List<GameServer> servers)
    {
        _gameServers = servers;
    }

    // Diğer yönetim işlevleri...
}
