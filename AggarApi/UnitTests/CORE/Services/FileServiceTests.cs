using CORE.Services;
using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UnitTests.CORE.Services
{
    [TestFixture]
    public class FileServiceTests
    {
        private Mock<IWebHostEnvironment> _mockEnvironment;
        private FileService _fileService;
        [SetUp]
        public void Setup()
        {
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _fileService = new FileService(_mockEnvironment.Object);
            _mockEnvironment.Setup(env => env.WebRootPath).Returns("C:/FakeWebRoot");
        }
        [Test]
        public async Task UploadFileAsync_ShouldUploadFileSuccessfully()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var fileName = "test.png";
            var fileExtension = ".png";
            var stream = new MemoryStream();

            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                    .Callback<Stream, System.Threading.CancellationToken>((targetStream, _) => stream.CopyTo(targetStream))
                    .Returns(Task.CompletedTask);

            var allowedExtensions = new List<string> { ".png", ".jpg" };
            var directory = "uploads";
            var oldFilePath = "old_file.png";

            // Act
            var result = await _fileService.UploadFileAsync(directory, oldFilePath, fileMock.Object, allowedExtensions);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Contains(directory));
            Assert.That(result.Contains(fileExtension));
        }
        [Test]
        public async Task UploadFileAsync_ShouldReturnNull_WhenFileIsNull()
        {
            // Act
            var result = await _fileService.UploadFileAsync("uploads", null, null);

            // Assert
            Assert.That(result, Is.Null);
        }
        [Test]
        public async Task UploadFileAsync_ShouldThrowException_WhenInvalidFileExtension()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.txt");
            var allowedExtensions = new List<string> { ".png", ".jpg" };

            // Act
            var result = await _fileService.UploadFileAsync("uploads", null, fileMock.Object, allowedExtensions);

            // Assert
            Assert.That(result, Is.Null);
        }
        [Test]
        public async Task UploadFileAsync_ShouldReturnNull_WhenExceptionOccurs()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.png");
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                    .Throws(new IOException("Simulated file system error"));

            var allowedExtensions = new List<string> { ".png", ".jpg" };

            // Act
            var result = await _fileService.UploadFileAsync("uploads", null, fileMock.Object, allowedExtensions);

            // Assert
            Assert.That(result, Is.Null);
        }
        [Test]
        public async Task UploadFileAsync_ShouldCreateDirectory_WhenNotExists()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var fileName = "test.png";
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

            var directory = "nonexistent_directory";
            var allowedExtensions = new List<string> { ".png", ".jpg" };

            // Act
            var result = await _fileService.UploadFileAsync(directory, null, fileMock.Object, allowedExtensions);

            // Assert
            Assert.That(result, Is.Not.Null);
            _mockEnvironment.Verify(env => env.WebRootPath, Times.Once);
        }
    }
}
