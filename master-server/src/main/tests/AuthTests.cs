using NUnit.Framework;
using Moq;
using System.Net.Sockets;
using System.Threading.Tasks;
using MessagePack;

[TestFixture]
public class AuthServiceTests
{
    private AuthService _authService;
    private Mock<PlayerManager> _mockPlayerManager;
    private Mock<LogManager> _mockLogManager;
    private Mock<ConnectionManager> _mockConnectionManager;
    private Mock<NetworkStream> _mockNetworkStream;

    [SetUp]
    public void Setup()
    {
        _mockPlayerManager = new Mock<PlayerManager>();
        _mockLogManager = new Mock<LogManager>();
        _mockConnectionManager = new Mock<ConnectionManager>();
        _mockNetworkStream = new Mock<NetworkStream>();

        _authService = new AuthService(_mockPlayerManager.Object, _mockLogManager.Object, _mockConnectionManager.Object);
    }

    [Test]
    public async Task HandleSignUpRequest_WhenCalled_InvokesCreatePlayerAsync()
    {
        // Arrange
        byte[] fakeData = MessagePackSerializer.Serialize(new SignUpRequest { Username = "testUser", Password = "testPass" });
        string fakeConnectionId = "123";
        
        _mockPlayerManager.Setup(x => x.IsUsernameAvailable(It.IsAny<string>())).ReturnsAsync(true);
        _mockPlayerManager.Setup(x => x.CreatePlayerAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        // Act
        await Task.Run(() => _authService.HandleSignUpRequest(_mockNetworkStream.Object, fakeData, fakeConnectionId));

        // Assert
        _mockPlayerManager.Verify(x => x.CreatePlayerAsync("testUser", "testPass"), Times.Once);
    }

}
