using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace backend.Validators
{
    public static class ValidationHelper
    {
        public static async Task<IResult?> ValidateAsync<T>(T request, IValidator<T> validator)
        {
            var result = await validator.ValidateAsync(request);
            if (!result.IsValid)
            {
                var errors = result.ToDictionary();
                return Results.ValidationProblem(errors);
            }
            return null;
        }
    }
}