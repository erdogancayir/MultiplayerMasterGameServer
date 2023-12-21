using NUnit.Framework;
using MasterServer;
using Moq;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using MessagePack;
using NUnit.Framework;

namespace MasterServer.Tests
{
    [TestFixture]
    public class AuthServiceTests
    {
        private AuthService _authService;
        private Mock<PlayerManager> _mockPlayerManager;
        private Mock<LogManager> _mockLogManager;
        private Mock<ConnectionManager> _mockConnectionManager;

        [SetUp]
        public void Setup()
        {
            _mockPlayerManager = new Mock<PlayerManager>();
            _mockLogManager = new Mock<LogManager>();
            _mockConnectionManager = new Mock<ConnectionManager>();
            _authService = new AuthService(_mockPlayerManager.Object, _mockLogManager.Object, _mockConnectionManager.Object);
        }

        [Test]
        public async Task SignUpRequest_ReturnsExpectedResponse()
        {
            // Arrange
            var signUpRequest = new SignUpRequest
            {
                OperationTypeId = (int)OperationType.SignUpRequest,
                Username = "testuser",
                Password = "testpassword"
            };
            var serializedRequest = MessagePackSerializer.Serialize(signUpRequest);
            var mockStream = new MemoryStream();

            _mockPlayerManager.Setup(x => x.IsUsernameAvailable("testuser")).ReturnsAsync(true);
            _mockPlayerManager.Setup(x => x.CreatePlayerAsync("testuser", "testpassword")).ReturnsAsync(true);

            // Act
            var mockSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _authService.HandleSignUpRequest(new NetworkStream(mockSocket), serializedRequest, "testConnectionId");

            // Assert
            mockStream.Seek(0, SeekOrigin.Begin);
            var response = MessagePackSerializer.Deserialize<SignUpResponse>(mockStream.ToArray());
            Assert.IsTrue(response.Success);
            Assert.AreEqual("Player successfully created.", response.Message);
        }
    }
}