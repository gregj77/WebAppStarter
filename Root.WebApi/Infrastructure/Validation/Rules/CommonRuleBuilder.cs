using FluentValidation;

namespace Validation.Rules
{
    public static class CommonRuleBuilder
    {
        public static IRuleBuilderOptions<T, TProperty> LanguageCode<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new LanguageCodeValidator());
        }
    }
}
