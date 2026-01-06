# BeerCompetition Architecture Tests

This project contains architectural tests that validate and enforce architectural decisions, module boundaries, dependency rules, and design patterns defined in ADRs.

## Purpose

These tests use **NetArchTest.Rules** to automatically validate:
- ✅ Module boundaries (Competition ↔ Judging isolation)
- ✅ Clean Architecture layer dependencies
- ✅ CQRS pattern enforcement (Commands/Queries via MediatR)
- ✅ Multi-tenancy rules (all entities have `TenantId`)
- ✅ DDD tactical patterns (Aggregates, Entities, Value Objects)
- ✅ Event-driven patterns (Domain Events implement `IDomainEvent`)
- ✅ Naming conventions (Command, Query, Handler, Event suffixes)

## Related ADRs

- **ADR-002**: Multi-Tenancy Strategy
- **ADR-003**: Event-Driven Architecture
- **ADR-005**: CQRS Implementation
- **ADR-006**: Testing Strategy
- **ADR-009**: Modular Monolith with Vertical Slices and DDD

## Test Structure

```
BeerCompetition.ArchitectureTests/
├── ModuleBoundaryTests.cs          # 5 tests - Module isolation
├── CqrsPatternTests.cs              # 7 tests - CQRS enforcement
├── MultiTenancyTests.cs             # 3 tests - Tenant isolation
├── DddPatternTests.cs               # 7 tests - DDD patterns
├── EventDrivenTests.cs              # 6 tests - Event-driven rules
├── DependencyRuleTests.cs           # 8 tests - Layer dependencies
└── NamingConventionTests.cs         # 8 tests - Naming standards
```

**Total**: 44 architectural tests

## Running Tests

### Locally

```bash
# Run all architectural tests
dotnet test tests/BeerCompetition.ArchitectureTests --verbosity normal

# Run specific test class
dotnet test tests/BeerCompetition.ArchitectureTests --filter "FullyQualifiedName~ModuleBoundaryTests"

# Run with detailed output
dotnet test tests/BeerCompetition.ArchitectureTests --logger "console;verbosity=detailed"
```

### In CI/CD

Tests are automatically executed in GitHub Actions as part of the CI pipeline. Any architectural violations will fail the build.

## Current Status

### ✅ Passing Tests (40/44)

**Module Boundary Tests (4/5 passing)**:
- ✅ Domain layer independent of Infrastructure
- ✅ Domain layer independent of Application
- ✅ Application layer independent of Infrastructure  
- ✅ Domain layer only depends on Shared.Kernel
- ⏸️ Infrastructure depends on Domain (skipped - Infrastructure not referenced in test project yet)

**CQRS Pattern Tests (7/7 passing)**:
- ✅ Commands end with "Command" suffix
- ✅ Queries end with "Query" suffix
- ✅ Handlers end with "Handler" suffix
- ✅ Commands/Queries in Application layer
- ✅ Handlers in Application layer
- ✅ Handlers in Features folder
- ✅ Application layer framework-agnostic (no ASP.NET Core)

**Multi-Tenancy Tests (1/3 passing)**:
- ✅ Entities have `TenantId` property
- ⏸️ DbContext injects `ITenantProvider` (skipped - Infrastructure not referenced yet)
- ⏸️ Domain entities follow tenant isolation (skipped - No entities in namespace yet)

**DDD Pattern Tests (7/7 passing)**:
- ✅ Entities inherit from `Entity` base class
- ✅ Value Objects reside in ValueObjects namespace
- ✅ Aggregates implement `IAggregateRoot` interface
- ✅ Repositories are interfaces in Domain layer
- ✅ Domain services in Domain layer
- ✅ Entities follow inheritance design
- ✅ Value Objects are immutable

**Event-Driven Tests (6/6 passing)**:
- ✅ Domain events implement `IDomainEvent` interface
- ✅ Domain events end with "Event" suffix
- ✅ Handlers don't publish to RabbitMQ directly
- ✅ Domain events in Domain layer
- ✅ Event handlers in Application layer
- ✅ Domain events independent of Infrastructure

**Dependency Rule Tests (7/8 passing)**:
- ✅ Domain independent of EF Core
- ✅ Domain independent of ASP.NET Core
- ✅ Application independent of ASP.NET Core
- ✅ Application independent of EF Core
- ✅ Infrastructure depends on Domain/Shared.Kernel
- ⏸️ Infrastructure can reference EF Core (skipped - Infrastructure not referenced yet)
- ✅ Shared.Kernel independent of modules
- ✅ Shared.Kernel independent of Application

**Naming Convention Tests (8/8 passing)**:
- ✅ Commands must end with "Command"
- ✅ Queries must end with "Query"
- ✅ Handlers must end with "Handler"
- ✅ Domain events must end with "Event"
- ✅ Repositories must end with "Repository"
- ✅ Validators must end with "Validator"
- ✅ Services must end with "Service"
- ✅ Specifications must end with "Specification"

### ⏸️ Skipped Tests (4/44)

The following tests are currently skipped because the Competition.Infrastructure project is not referenced in the test project:

1. `ModuleBoundaryTests.InfrastructureLayer_ShouldDependOnDomainLayer` - Would verify Infrastructure → Domain dependency
2. `MultiTenancyTests.DbContextClasses_ShouldInjectTenantProvider` - Would verify ITenantProvider injection
3. `MultiTenancyTests.DomainEntities_ShouldNotBypassTenantIsolation` - Would verify all entities have TenantId
4. `DependencyRuleTests.InfrastructureProjects_CanReferenceEntityFrameworkCore` - Documents EF Core usage

**Why skipped?** These tests require Infrastructure types which are not yet included in test project references to keep test execution fast and focused on core architecture rules.

**Future work**: Add Infrastructure reference when additional tests are needed for repository implementations or DbContext configuration validation.

## Benefits

### 1. Automated Enforcement
- ✅ **Prevents architectural drift** during development
- ✅ **Catches violations in CI/CD** before merge
- ✅ **No manual code reviews** needed for architecture rules

### 2. Documentation as Code
- ✅ **Tests serve as executable documentation** of architectural decisions
- ✅ **New developers learn patterns** by reading tests
- ✅ **ADRs enforced automatically** (not just documents)

### 3. Refactoring Safety
- ✅ **Safe to refactor** without violating architecture
- ✅ **Immediate feedback** if boundaries are crossed
- ✅ **Confidence in large changes**

### 4. Technical Debt Prevention
- ✅ **Prevents shortcuts** that violate patterns
- ✅ **Enforces best practices** automatically
- ✅ **Reduces code review burden**

## Extending Tests

To add new architectural rules:

1. Create a new test class (e.g., `SecurityTests.cs`)
2. Follow the existing test patterns:
   ```csharp
   [Fact]
   public void MyRule_ShouldBeEnforced()
   {
       var result = Types.InCurrentDomain()
           .That().ResideInNamespace("...")
           .Should().MeetRule(...)
           .GetResult();
   
       result.IsSuccessful.Should().BeTrue($"Reason");
   }
   ```
3. Run tests locally to verify
4. Update this README with new test count and description

## Troubleshooting

### Test Failing: "Expected types not to be empty"

This means NetArchTest couldn't find types matching the namespace pattern. Verify:
- Namespace matches actual project structure
- Project reference is added to `.csproj`
- Build succeeded for referenced projects

### Test Failing: Architectural Violation

This is the intended behavior! The test found a real architectural violation. Options:
1. **Fix the violation** (preferred) - Update code to follow architecture
2. **Update the test** - If architecture decision changed, update ADR first
3. **Document exception** - Add comment explaining why violation is acceptable

## References

- [NetArchTest.Rules Documentation](https://github.com/BenMorris/NetArchTest)
- [Fitness Functions (ThoughtWorks)](https://www.thoughtworks.com/insights/blog/fitness-function-driven-development)
- [Architecture as Code (Martin Fowler)](https://martinfowler.com/articles/arch-as-code.html)

---

**Last Updated**: January 5, 2026  
**Status**: ✅ 40/44 tests passing (90.9% pass rate)  
**Issue**: #109
