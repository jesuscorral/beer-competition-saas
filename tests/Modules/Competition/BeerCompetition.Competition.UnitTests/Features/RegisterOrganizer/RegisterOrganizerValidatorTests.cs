using BeerCompetition.Competition.Application.Features.RegisterOrganizer;
using FluentValidation.TestHelper;

namespace BeerCompetition.Competition.UnitTests.Features.RegisterOrganizer;

public class RegisterOrganizerValidatorTests
{
    private readonly RegisterOrganizerValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new RegisterOrganizerCommand(
            Email: "organizer@example.com",
            Password: "SecurePass123",
            OrganizationName: "Awesome Homebrew Club",
            PlanName: "TRIAL"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyEmail_ShouldHaveValidationError(string? email)
    {
        // Arrange
        var command = new RegisterOrganizerCommand(
            Email: email!,
            Password: "SecurePass123",
            OrganizationName: "Test Org",
            PlanName: "TRIAL"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user @example.com")]
    public void Validate_InvalidEmailFormat_ShouldHaveValidationError(string email)
    {
        // Arrange
        var command = new RegisterOrganizerCommand(
            Email: email,
            Password: "SecurePass123",
            OrganizationName: "Test Org",
            PlanName: "TRIAL"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyPassword_ShouldHaveValidationError(string? password)
    {
        // Arrange
        var command = new RegisterOrganizerCommand(
            Email: "organizer@example.com",
            Password: password!,
            OrganizationName: "Test Org",
            PlanName: "TRIAL"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("short")]
    [InlineData("1234567")]
    public void Validate_PasswordTooShort_ShouldHaveValidationError(string password)
    {
        // Arrange
        var command = new RegisterOrganizerCommand(
            Email: "organizer@example.com",
            Password: password,
            OrganizationName: "Test Org",
            PlanName: "TRIAL"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 8 characters");
    }

    [Theory]
    [InlineData("nocapitals123")]
    [InlineData("alllowercase")]
    public void Validate_PasswordWithoutUppercase_ShouldHaveValidationError(string password)
    {
        // Arrange
        var command = new RegisterOrganizerCommand(
            Email: "organizer@example.com",
            Password: password,
            OrganizationName: "Test Org",
            PlanName: "TRIAL"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one uppercase letter");
    }

    [Theory]
    [InlineData("NOLOWERCASE123")]
    [InlineData("ALLUPPERCASE")]
    public void Validate_PasswordWithoutLowercase_ShouldHaveValidationError(string password)
    {
        // Arrange
        var command = new RegisterOrganizerCommand(
            Email: "organizer@example.com",
            Password: password,
            OrganizationName: "Test Org",
            PlanName: "TRIAL"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one lowercase letter");
    }

    [Theory]
    [InlineData("NoDigitsHere")]
    [InlineData("OnlyLetters")]
    public void Validate_PasswordWithoutDigit_ShouldHaveValidationError(string password)
    {
        // Arrange
        var command = new RegisterOrganizerCommand(
            Email: "organizer@example.com",
            Password: password,
            OrganizationName: "Test Org",
            PlanName: "TRIAL"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one digit");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyOrganizationName_ShouldHaveValidationError(string? organizationName)
    {
        // Arrange
        var command = new RegisterOrganizerCommand(
            Email: "organizer@example.com",
            Password: "SecurePass123",
            OrganizationName: organizationName!,
            PlanName: "TRIAL"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OrganizationName);
    }

    [Theory]
    [InlineData("TRIAL")]
    [InlineData("BASIC")]
    [InlineData("STANDARD")]
    [InlineData("PRO")]
    public void Validate_ValidPlanName_ShouldNotHaveValidationError(string planName)
    {
        // Arrange
        var command = new RegisterOrganizerCommand(
            Email: "organizer@example.com",
            Password: "SecurePass123",
            OrganizationName: "Test Org",
            PlanName: planName
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PlanName);
    }

    [Theory]
    [InlineData("INVALID")]
    [InlineData("FREE")]
    [InlineData("PREMIUM")]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_InvalidPlanName_ShouldHaveValidationError(string planName)
    {
        // Arrange
        var command = new RegisterOrganizerCommand(
            Email: "organizer@example.com",
            Password: "SecurePass123",
            OrganizationName: "Test Org",
            PlanName: planName
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PlanName)
            .WithErrorMessage("Plan name must be one of: TRIAL, BASIC, STANDARD, PRO");
    }
}
