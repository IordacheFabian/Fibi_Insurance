// using Application.Policies.DTOs.Command;
// using Application.Policies.DTOs.Requests;
// using Application.Validators;
// using FluentValidation.TestHelper;

// namespace Application.Tests.Validators;

// public class CancelPolicyValidatorTests
// {
//     private readonly CancelPolicyValidator _validator = new();

//     [Fact]
//     public void Validator_ShouldFail_WhenPolicyIdMissing()
//     {
//         var command = BuildCommand(c => c.PolicyId = Guid.Empty);

//         var result = _validator.TestValidate(command);

//         result.ShouldHaveValidationErrorFor(x => x.PolicyId);
//     }

//     [Fact]
//     public void Validator_ShouldFail_WhenCancellationDtoMissing()
//     {
//         var command = new CancelPolicy.Command { PolicyId = Guid.NewGuid(), CancelPolicyDto = null! };

//         var result = _validator.TestValidate(command);

//         result.ShouldHaveValidationErrorFor(x => x.CancelPolicyDto);
//     }

//     [Fact]
//     public void Validator_ShouldFail_WhenReasonTooLong()
//     {
//         var command = BuildCommand(c => c.CancelPolicyDto.CancellationReason = new string('a', 501));

//         var result = _validator.TestValidate(command);

//         result.ShouldHaveValidationErrorFor(x => x.CancelPolicyDto.CancellationReason);
//     }

//     private static CancelPolicy.Command BuildCommand(Action<CancelPolicy.Command>? configure = null)
//     {
//         var command = new CancelPolicy.Command
//         {
//             PolicyId = Guid.NewGuid(),
//             CancelPolicyDto = new CancelPolicyDto { CancellationReason = "Valid" }
//         };
//         configure?.Invoke(command);
//         return command;
//     }
// }
