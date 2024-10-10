using NUnit.Framework;
using Ethik.Utility.Security.Jwt.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ethik.Utility.Security.Jwt.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Moq;

namespace Ethik.Utility.Security.Jwt.Services.Tests;

[TestFixture]
public class JwtTokenServiceTests
{
    private Mock<IOptions<JwtSettings>> _mockJwtSettings = null!;
    private JwtSettings _jwtSettings = null!;
    private JwtTokenService _sut = null!;

    [SetUp]
    public void Setup()
    {
        _jwtSettings = new JwtSettings
        {
            Issuer = "testIssuer",
            Audience = "testAudience",
            ExpiryMinutes = 60,
            SecretKey = "SuperSecretKey12345678909876543211234567890"
        };

        _mockJwtSettings = new Mock<IOptions<JwtSettings>>();
        _mockJwtSettings.Setup(s => s.Value).Returns(_jwtSettings);

        _sut = new JwtTokenService(_mockJwtSettings.Object);
    }

    [Test]
    public void GenerateToken_ShouldReturnToken_WithCorrectClaims()
    {
        // Arrange
        var userId = "12345";
        var email = "test@example.com";
        var role = "Admin";

        // Act
        var token = _sut.GenerateToken(userId, email, role);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        Assert.That(jwtToken.Issuer, Is.EqualTo(_jwtSettings.Issuer));
        Assert.That(jwtToken.Audiences.First(), Is.EqualTo(_jwtSettings.Audience));
        Assert.That(jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value, Is.EqualTo(userId));
        Assert.That(jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.UniqueName).Value, Is.EqualTo(email));
        Assert.That(jwtToken.Claims.First(x => x.Type == ClaimTypes.Role).Value, Is.EqualTo(role));
    }

    [Test]
    public void GetTokenDetails_ShouldReturnCorrectDetails_ForValidToken()
    {
        // Arrange
        var userId = "12345";
        var email = "test@example.com";
        var role = "Admin";
        var token = _sut.GenerateToken(userId, email, role);

        // Act
        var result = _sut.GetTokenDetails(token);

        // Assert
        Assert.That(result.Token, Is.EqualTo(token));
        Assert.That(result.TokenType, Is.EqualTo("Bearer"));
        Assert.IsTrue(result.Expiration > DateTime.UtcNow);
    }

    [Test]
    public void GetTokenDetails_SecurityTokenMalformedException_ForInvalidToken()
    {
        // Arrange
        var invalidToken = "invalidTokenString";

        // Act & Assert
        Assert.Throws<SecurityTokenMalformedException>(() => _sut.GetTokenDetails(invalidToken));
    }
}