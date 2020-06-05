using Projet_Forum.Services.Services;
using Xunit;

namespace Projet_Forum.Tests
{
    public class TokenReaderServiceTest
    {
        #region GetTokenUsername()

        [Fact(DisplayName = "GetTokenUsername returns null response -> null accesstoken")]
        public void GetTokenUsernameWithNullArg()
        {
            // Arrange
            TokenReaderService service = new TokenReaderService();

            // Act
            var result = service.GetTokenUsername(null);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetTokenRole()

        [Fact(DisplayName = "GetTokenRole returns null response -> null accesstoken")]
        public void GetTokenRoleWithNullArg()
        {
            // Arrange
            TokenReaderService service = new TokenReaderService();

            // Act
            var result = service.GetTokenRole(null);

            // Assert
            Assert.Null(result);
        }
        #endregion
    }
}
