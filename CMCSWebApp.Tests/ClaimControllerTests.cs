using Xunit;
using Moq;
using CMCSWebApp.Controllers;
using CMCSWebApp.Services;
using CMCSWebApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Collections.Generic;

namespace CMCSWebApp.Tests
{
    public class ClaimsControllerTests
    {
        private readonly Mock<IClaimService> _mockClaimService;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly ClaimsController _controller;

        public ClaimsControllerTests()
        {
            _mockClaimService = new Mock<IClaimService>();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _controller = new ClaimsController(
                _mockClaimService.Object,
                _mockUserManager.Object);

            // Setup TempData to prevent NullReferenceException
            var tempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;

            // Setup controller context with authenticated user
            SetupControllerContext("user123", "Lecturer");
        }

        private void SetupControllerContext(string userId, string role)
        {
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, userId),
                new System.Security.Claims.Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            _mockUserManager.Setup(x => x.GetUserId(claimsPrincipal))
                .Returns(userId);
        }

        [Fact]
        public async Task Index_ReturnsViewWithClaims()
        {
            // Arrange
            var claims = new List<CMCSWebApp.Models.Claim>
            {
                new CMCSWebApp.Models.Claim { ClaimId = 1, LecturerId = "user123" }
            };

            _mockClaimService.Setup(x => x.GetClaimsForUserAsync("user123"))
                .ReturnsAsync(claims);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<CMCSWebApp.Models.Claim>>(viewResult.Model);
            Assert.Single(model);
        }

        [Fact]
        public void Create_Get_ReturnsView()
        {
            // Act
            var result = _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Claim/Create.cshtml", viewResult.ViewName);
        }

        [Fact]
        public async Task Create_Post_WithValidModel_RedirectsToIndex()
        {
            // Arrange
            var claim = new CMCSWebApp.Models.Claim
            {
                HoursWorked = 10,
                HourlyRate = 500,
                LecturerId = "user123"
            };

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("test.pdf");
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.ContentType).Returns("application/pdf");

            // Setup the service to succeed
            _mockClaimService.Setup(x => x.CreateClaimAsync(
                It.IsAny<CMCSWebApp.Models.Claim>(),
                It.IsAny<IFormFile>()))
                .ReturnsAsync(1);

            // CRITICAL FIX: Clear ModelState to ensure validation passes
            _controller.ModelState.Clear();

            // Act
            var result = await _controller.Create(claim, mockFile.Object);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            _mockClaimService.Verify(x => x.CreateClaimAsync(
                It.Is<CMCSWebApp.Models.Claim>(c => c.LecturerId == "user123"),
                mockFile.Object), Times.Once);
        }

        [Fact]
        public async Task ViewDetails_WithValidId_ReturnsView()
        {
            // Arrange
            var claim = new CMCSWebApp.Models.Claim
            {
                ClaimId = 1,
                LecturerId = "user123"
            };

            SetupControllerContext("user123", "Lecturer");

            _mockClaimService.Setup(x => x.GetClaimByIdAsync(1))
                .ReturnsAsync(claim);

            // Act
            var result = await _controller.ViewDetails(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Claim/ViewDetails.cshtml", viewResult.ViewName);
            Assert.IsType<CMCSWebApp.Models.Claim>(viewResult.Model);
        }

        [Fact]
        public async Task ViewDetails_WithInvalidId_RedirectsToHome()
        {
            // Arrange
            _mockClaimService.Setup(x => x.GetClaimByIdAsync(999))
                .ReturnsAsync((CMCSWebApp.Models.Claim)null);

            // Act
            var result = await _controller.ViewDetails(999);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);

            // Verify TempData was set
            Assert.True(_controller.TempData.ContainsKey("ErrorMessage"));
        }

        [Fact]
        public async Task Verify_WithValidId_UpdatesStatusAndRedirects()
        {
            // Arrange
            SetupControllerContext("coordinator123", "Coordinator");

            _mockClaimService.Setup(x => x.UpdateClaimStatusAsync(
                1,
                CMCSWebApp.Models.Claim.ClaimStatus.VerifiedByCoordinator))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Verify(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CoordinatorDashboard", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);

            _mockClaimService.Verify(x => x.UpdateClaimStatusAsync(
                1,
                CMCSWebApp.Models.Claim.ClaimStatus.VerifiedByCoordinator), Times.Once);

            // Verify TempData was set
            Assert.True(_controller.TempData.ContainsKey("SuccessMessage"));
        }

        [Fact]
        public async Task Approve_WithValidId_UpdatesStatusAndRedirects()
        {
            // Arrange
            SetupControllerContext("manager123", "Manager");

            _mockClaimService.Setup(x => x.UpdateClaimStatusAsync(
                1,
                CMCSWebApp.Models.Claim.ClaimStatus.ApprovedByManager))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Approve(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ManagerDashboard", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);

            _mockClaimService.Verify(x => x.UpdateClaimStatusAsync(
                1,
                CMCSWebApp.Models.Claim.ClaimStatus.ApprovedByManager), Times.Once);

            Assert.True(_controller.TempData.ContainsKey("SuccessMessage"));
        }

        [Fact]
        public async Task Reject_AsCoordinator_UpdatesStatusAndRedirects()
        {
            // Arrange
            SetupControllerContext("coordinator123", "Coordinator");

            _mockClaimService.Setup(x => x.UpdateClaimStatusAsync(
                1,
                CMCSWebApp.Models.Claim.ClaimStatus.Rejected))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Reject(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CoordinatorDashboard", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);

            Assert.True(_controller.TempData.ContainsKey("SuccessMessage"));
        }

        [Fact]
        public async Task Reject_AsManager_UpdatesStatusAndRedirects()
        {
            // Arrange
            SetupControllerContext("manager123", "Manager");

            _mockClaimService.Setup(x => x.UpdateClaimStatusAsync(
                1,
                CMCSWebApp.Models.Claim.ClaimStatus.Rejected))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Reject(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ManagerDashboard", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);

            Assert.True(_controller.TempData.ContainsKey("SuccessMessage"));
        }
    }
}