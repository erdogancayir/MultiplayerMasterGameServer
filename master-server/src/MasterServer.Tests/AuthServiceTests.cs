using NUnit.Framework;
using Moq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System;
using MasterServer;
using MessagePack;
// Diğer gerekli using ifadeleri

namespace MasterServer.Tests
{
    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<PlayerManager> _mockPlayerManager;
        private Mock<LogManager> _mockLogManager;
        private Mock<ConnectionManager> _mockConnectionManager;
        private Mock<NetworkStream> _mockNetworkStream;
        private AuthService _authService;

        [SetUp]
        public void Setup()
        {
            var dbInterfaceInstance = new DbInterface("mongodb+srv://panteon_user:nnNkralzoYbFTpxH@panteondb.zyeh5mg.mongodb.net/?retryWrites=true&w=majority", "panteondb");
            var logManagerInstance = new LogManager(dbInterfaceInstance);
            var secret = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
            var tokenManagerInstance = new TokenManager(secret);

            var playerManagerMock = new Mock<PlayerManager>(dbInterfaceInstance, logManagerInstance, tokenManagerInstance);

            _mockLogManager = new Mock<LogManager>() ?? throw new ArgumentNullException(nameof(_mockLogManager));
            _mockConnectionManager = new Mock<ConnectionManager>() ?? throw new ArgumentNullException(nameof(_mockConnectionManager));
            _mockNetworkStream = new Mock<NetworkStream>() ?? throw new ArgumentNullException(nameof(_mockNetworkStream));

            _authService = new AuthService(playerManagerMock.Object, _mockLogManager.Object, _mockConnectionManager.Object) ?? throw new ArgumentNullException(nameof(_authService));
        }

        [Test]
        public async Task HandleSignUpRequest_WhenUserDoesNotExist_ShouldCreateUser()
        {
            var signUpRequest = new SignUpRequest
            {
                Username = "testUser",
                Password = "testPassword"
            };

            var dummyData = MessagePackSerializer.Serialize(signUpRequest);
            // Arrange
            _mockPlayerManager.Setup(m => m.IsUsernameAvailable(It.IsAny<string>())).ReturnsAsync(true);
            _mockPlayerManager.Setup(m => m.CreatePlayerAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            // Act
            //await _authService.HandleSignUpRequest(_mockNetworkStream.Object, dummyData, 0);

            // Assert
            _mockPlayerManager.Verify(m => m.CreatePlayerAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        // Diğer test senaryoları...
    }
}
