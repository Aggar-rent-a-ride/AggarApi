using AutoMapper;
using CORE.Constants;
using CORE.DTOs.Message;
using CORE.Services;
using CORE.Services.IServices;
using DATA.DataAccess.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Hosting;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.CORE.Services
{
    [TestFixture]
    public class ChatServiceTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IMapper> _mapper;
        private Mock<IUserService> _userService;
        private Mock<IFileService> _fileService;
        private ChatService _messageService;

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mapper = new Mock<IMapper>();
            _userService = new Mock<IUserService>();
            _fileService = new Mock<IFileService>();
            _messageService = new ChatService(_mockUnitOfWork.Object, _mapper.Object, _userService.Object, _fileService.Object);
        }
        [Test]
        public async Task CreateMessageAsync_NullDto_ReturnsErrorMessage()
        {
            // Act
            var result = await _messageService.CreateMessageAsync<CreateMessageDto, GetMessageDto>(null, 1);

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.BadRequest));
        }
        [Test]
        public async Task CreateMessageAsync_EmptyFilePath_ReturnsErrorMessage()
        {
            // Act
            var result = await _messageService.CreateMessageAsync<CreateFileMessageDto, GetFileMessageDto>(new CreateFileMessageDto { }, 1);

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.BadRequest));
        }
        [Test]
        public async Task CreateMessageAsync_ReceiverDoesNotExist_ReturnsErrorMessage()
        {
            //Arrange
            _fileService.Setup(x => x.HashFile(It.IsAny<string>())).Returns("test");
            _userService.Setup(x => x.CheckAllUsersExist(It.IsAny<List<int>>())).ReturnsAsync(false);
            var createFileMessageDto = new CreateFileMessageDto { FilePath = "test", Checksum = "test", ReceiverId = 2000, ClientMessageId = "test"  };

            // Act
            var result = await _messageService.CreateMessageAsync<CreateFileMessageDto, GetFileMessageDto>(createFileMessageDto, 1);

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.BadRequest));
            Assert.That(result.Message, Is.EqualTo("Users do not exist"));
        }
        [Test]
        public async Task CreateMessageAsync_ValidationFails_DeletesFile()
        {
            //Arrange
            _fileService.Setup(x => x.HashFile(It.IsAny<string>())).Returns("test");
            _userService.Setup(x => x.CheckAllUsersExist(It.IsAny<List<int>>())).ReturnsAsync(false);
            var createFileMessageDto = new CreateFileMessageDto { FilePath = "test"};

            // Act
            var result = await _messageService.CreateMessageAsync<CreateFileMessageDto, GetFileMessageDto>(createFileMessageDto, 1);

            // Assert
            _fileService.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Once);
        }

    }
}
