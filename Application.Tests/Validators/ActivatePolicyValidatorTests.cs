using Application.Policies.Command;
using Application.Validators;
using FluentValidation.TestHelper;

namespace Application.Tests.Validators;

public class ActivatePolicyValidatorTests
{
    private readonly ActivatePolicyValidator _validator = new();

    [Fact]
    public void Validator_ShouldFail_WhenPolicyIdMissing()
    {
        var command = new ActivatePolicy.Command { PolicyId = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PolicyId);
    }

    [Fact]
    public void Validator_ShouldPass_ForValidCommand()
    {
        var command = new ActivatePolicy.Command { PolicyId = Guid.NewGuid() };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
