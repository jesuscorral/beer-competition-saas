using BeerCompetition.Competition.Application.Features.UserRegistration;
using BeerCompetition.Competition.Domain.Entities;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace BeerCompetition.Competition.UnitTests.Features.UserRegistration;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator;

    public RegisterUserCommandValidatorTests()
    {
        _validator = new RegisterUserCommandValidator();
    }

    [Fact]
    public void Validate_WithValidEntrantData_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "entrant@example.com",
            Password: "SecurePassword123!",
            FullName: "John Doe",
            Role: CompetitionUserRole.ENTRANT,
            CompetitionId: Guid.NewGuid(),
            BjcpRank: null,
            OrganizationName: null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyEmail_ShouldHaveValidationError()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "",
            Password: "SecurePassword123!",
            FullName: "John Doe",
            Role: CompetitionUserRole.ENTRANT,
            CompetitionId: Guid.NewGuid(),
            BjcpRank: null,
            OrganizationName: null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithInvalidEmailFormat_ShouldHaveValidationError()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "not-an-email",
            Password: "SecurePassword123!",
            FullName: "John Doe",
            Role: CompetitionUserRole.ENTRANT,
            CompetitionId: Guid.NewGuid(),
            BjcpRank: null,
            OrganizationName: null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithShortPassword_ShouldHaveValidationError()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            Password: "short",
            FullName: "John Doe",
            Role: CompetitionUserRole.ENTRANT,
            CompetitionId: Guid.NewGuid(),
            BjcpRank: null,
            OrganizationName: null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithEmptyFullName_ShouldHaveValidationError()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            Password: "SecurePassword123!",
            FullName: "",
            Role: CompetitionUserRole.ENTRANT,
            CompetitionId: Guid.NewGuid(),
            BjcpRank: null,
            OrganizationName: null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Validate_EntrantWithoutCompetitionId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            Password: "SecurePassword123!",
            FullName: "John Doe",
            Role: CompetitionUserRole.ENTRANT,
            CompetitionId: null,
            BjcpRank: null,
            OrganizationName: null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CompetitionId)
            .WithErrorMessage("Competition ID is required for this role");
    }

    [Fact]
    public void Validate_JudgeWithoutCompetitionId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            Password: "SecurePassword123!",
            FullName: "John Doe",
            Role: CompetitionUserRole.JUDGE,
            CompetitionId: null,
            BjcpRank: "Certified",
            OrganizationName: null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CompetitionId)
            .WithErrorMessage("Competition ID is required for this role");
    }

    [Fact]
    public void Validate_StewardWithoutCompetitionId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            Password: "SecurePassword123!",
            FullName: "John Doe",
            Role: CompetitionUserRole.STEWARD,
            CompetitionId: null,
            BjcpRank: null,
            OrganizationName: null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CompetitionId)
            .WithErrorMessage("Competition ID is required for this role");
    }

    [Fact]
    public void Validate_OrganizerWithCompetitionId_ShouldNotHaveValidationError()
    {
        // Arrange - Organizers don't need CompetitionId
        var command = new RegisterUserCommand(
            Email: "organizer@example.com",
            Password: "SecurePassword123!",
            FullName: "Jane Smith",
            Role: CompetitionUserRole.ORGANIZER,
            CompetitionId: null,
            BjcpRank: null,
            OrganizationName: "My Brew Club");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CompetitionId);
    }

    [Fact]
    public void Validate_OrganizerWithOrganizationName_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "organizer@example.com",
            Password: "SecurePassword123!",
            FullName: "Jane Smith",
            Role: CompetitionUserRole.ORGANIZER,
            CompetitionId: null,
            BjcpRank: null,
            OrganizationName: "Valid Organization Name");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(CompetitionUserRole.ENTRANT)]
    [InlineData(CompetitionUserRole.JUDGE)]
    [InlineData(CompetitionUserRole.STEWARD)]
    [InlineData(CompetitionUserRole.ORGANIZER)]
    public void Validate_WithValidRole_ShouldNotHaveRoleValidationError(CompetitionUserRole role)
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            Password: "SecurePassword123!",
            FullName: "John Doe",
            Role: role,
            CompetitionId: role != CompetitionUserRole.ORGANIZER ? Guid.NewGuid() : null,
            BjcpRank: null,
            OrganizationName: role == CompetitionUserRole.ORGANIZER ? "Test Org" : null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Role);
    }
}
