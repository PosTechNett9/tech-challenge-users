using FIAP.CloudGames.Users.Application.Dtos;
using FIAP.CloudGames.Users.Application.Services;
using FIAP.CloudGames.Users.Domain.Entities;
using FIAP.CloudGames.Users.Domain.Enums;
using FIAP.CloudGames.Users.Domain.Interfaces.Repositories;
using FIAP.CloudGames.Users.Domain.ValueObjects;
using Moq;

namespace FIAP.CloudGames.Users.Tests.Application.UseCases.Users
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userService = new UserService(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddUserAndReturnCreatedUser()
        {
            var dto = new CreateUserDto
            {
                Name = "John Doe",
                Email = "john@example.com",
                Password = "Password!123",
                Role = UserRolesEnum.Admin,
            };

            _userRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            var result = await _userService.CreateAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.Role, result.Role);
            _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnListOfUsers()
        {
            var users = new List<User>
            {
                new("Test", Email.Create("test@test.com"), Password.FromPlainText("Password!123"), UserRolesEnum.User)
            };

            _userRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

            var result = await _userService.GetAllAsync();

            Assert.Single(result);
            Assert.Equal("Test", ((List<User>)result)[0].Name);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUser_WhenFound()
        {
            var id = Guid.NewGuid();
            var user = new User("Test", Email.Create("test@test.com"), Password.FromPlainText("Password!123"), UserRolesEnum.Admin);
            _userRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(user);

            var result = await _userService.GetByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal(user.Name, result!.Name);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnUpdatedUser()
        {
            var id = Guid.NewGuid();
            var existingUser = new User("Old", Email.Create("old@test.com"), Password.FromPlainText("Password!123"), UserRolesEnum.User);
            var dto = new UpdateUserDto
            {
                Id = id,
                Name = "New",
                Email = "new@test.com",
                Password = "Password!123",
                Role = UserRolesEnum.Admin
            };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingUser);
            _userRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(existingUser);

            var result = await _userService.UpdateAsync(dto);

            Assert.NotNull(result);
            Assert.Equal("New", result!.Name);
            Assert.Equal(UserRolesEnum.Admin, result.Role);
            _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnTrue_WhenUserExists()
        {
            var id = Guid.NewGuid();
            var user = new User(
                "Test",
                Email.Create("test@test.com"),
                Password.FromPlainText("Password!123"),
                UserRolesEnum.User
            );

            _userRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(user);
            _userRepositoryMock.Setup(r => r.DeleteAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);

            var result = await _userService.DeleteAsync(id);

            Assert.True(result);
            _userRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            var id = Guid.NewGuid();
            _userRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((User?)null);

            var result = await _userService.DeleteAsync(id);

            Assert.False(result);
            _userRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }
    }

}
