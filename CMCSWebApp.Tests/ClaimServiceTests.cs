using Xunit;
using Moq;
using CMCSWebApp.Services;
using CMCSWebApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;

namespace CMCSWebApp.Tests
{
    public class ClaimServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly Mock<ILogger<ClaimService>> _mockLogger;
        private readonly ClaimService _claimService;

        public ClaimServiceTests()
        {
            // Setup mock UserManager
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockEnvironment.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());

            _mockLogger = new Mock<ILogger<ClaimService>>();

            _claimService = new ClaimService(
                _mockUserManager.Object,
                _mockEnvironment.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task CreateClaimAsync_WithValidData_ReturnsClaimId()
        {
            // Arrange
            var claim = new Claim
            {
                LecturerId = "user123",
                HoursWorked = 10,
                HourlyRate = 500
            };

            var mockFile = CreateMockFile("test.pdf", "application/pdf", 1024);

            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser
                {
                    FirstName = "John",
                    LastName = "Doe"
                });

            // Act
            var result = await _claimService.CreateClaimAsync(claim, mockFile);

            // Assert
            Assert.True(result > 0);
            Assert.Equal(Claim.ClaimStatus.PendingReview, claim.Status);
        }

        [Fact]
        public async Task CreateClaimAsync_WithNullDocument_ThrowsArgumentException()
        {
            // Arrange
            var claim = new Claim
            {
                LecturerId = "user123",
                HoursWorked = 10,
                HourlyRate = 500
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _claimService.CreateClaimAsync(claim, null));

            Assert.Contains("supporting document is required", exception.Message);
        }

        [Fact]
        public async Task CreateClaimAsync_WithOversizedFile_ThrowsArgumentException()
        {
            // Arrange
            var claim = new Claim
            {
                LecturerId = "user123",
                HoursWorked = 10,
                HourlyRate = 500
            };

            // Create a file larger than 5MB
            var mockFile = CreateMockFile("large.pdf", "application/pdf", 6 * 1024 * 1024);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _claimService.CreateClaimAsync(claim, mockFile));

            Assert.Contains("File size must not exceed 5MB", exception.Message);
        }

        [Fact]
        public async Task CreateClaimAsync_WithInvalidFileType_ThrowsArgumentException()
        {
            // Arrange
            var claim = new Claim
            {
                LecturerId = "user123",
                HoursWorked = 10,
                HourlyRate = 500
            };

            var mockFile = CreateMockFile("test.exe", "application/exe", 1024);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _claimService.CreateClaimAsync(claim, mockFile));

            Assert.Contains("Invalid file type", exception.Message);
        }

        [Fact]
        public async Task GetClaimByIdAsync_WithExistingId_ReturnsClaim()
        {
            // Arrange
            var claim = new Claim
            {
                LecturerId = "user123",
                HoursWorked = 10,
                HourlyRate = 500
            };

            var mockFile = CreateMockFile("test.pdf", "application/pdf", 1024);

            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser
                {
                    FirstName = "John",
                    LastName = "Doe"
                });

            var claimId = await _claimService.CreateClaimAsync(claim, mockFile);

            // Act
            var result = await _claimService.GetClaimByIdAsync(claimId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(claimId, result.ClaimId);
        }

        [Fact]
        public async Task UpdateClaimStatusAsync_WithValidId_UpdatesStatus()
        {
            // Arrange
            var claim = new Claim
            {
                LecturerId = "user123",
                HoursWorked = 10,
                HourlyRate = 500
            };

            var mockFile = CreateMockFile("test.pdf", "application/pdf", 1024);

            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser
                {
                    FirstName = "John",
                    LastName = "Doe"
                });

            var claimId = await _claimService.CreateClaimAsync(claim, mockFile);

            // Act
            await _claimService.UpdateClaimStatusAsync(claimId, Claim.ClaimStatus.VerifiedByCoordinator);

            var updatedClaim = await _claimService.GetClaimByIdAsync(claimId);

            // Assert
            Assert.Equal(Claim.ClaimStatus.VerifiedByCoordinator, updatedClaim.Status);
        }

        [Fact]
        public async Task GetPendingClaimsAsync_ForCoordinator_ReturnsOnlyPendingClaims()
        {
            // Arrange
            var claim1 = new Claim
            {
                LecturerId = "user123",
                HoursWorked = 10,
                HourlyRate = 500
            };

            var mockFile = CreateMockFile("test.pdf", "application/pdf", 1024);

            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser
                {
                    FirstName = "John",
                    LastName = "Doe"
                });

            await _claimService.CreateClaimAsync(claim1, mockFile);

            // Act
            var result = await _claimService.GetPendingClaimsAsync("Coordinator");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count > 0);
            Assert.All(result, c => Assert.Equal(Claim.ClaimStatus.PendingReview, c.Status));
        }

        [Fact]
        public async Task GetManagerDashboardDataAsync_ReturnsCorrectCounts()
        {
            // Arrange
            var claim = new Claim
            {
                LecturerId = "user123",
                HoursWorked = 10,
                HourlyRate = 500
            };

            var mockFile = CreateMockFile("test.pdf", "application/pdf", 1024);

            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser
                {
                    FirstName = "John",
                    LastName = "Doe"
                });

            await _claimService.CreateClaimAsync(claim, mockFile);

            // Act
            var result = await _claimService.GetManagerDashboardDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.TotalClaimsCount > 0);
        }

        // Helper method to create mock IFormFile
        private IFormFile CreateMockFile(string fileName, string contentType, long length)
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.ContentType).Returns(contentType);
            mockFile.Setup(f => f.Length).Returns(length);

            var stream = new MemoryStream(new byte[length]);
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.CompletedTask);

            return mockFile.Object;
        }
    }
}