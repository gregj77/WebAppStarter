using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentValidation.Validators;

namespace Validation.Rules
{
    internal class LanguageCodeValidator : PropertyValidator
    {
        private HashSet<string> _languageCodes;

        public LanguageCodeValidator(string errorMessageResourceName, Type errorMessageResourceType) : base(errorMessageResourceName, errorMessageResourceType)
        {
            Initialize();
        }

        public LanguageCodeValidator(string errorMessage) : base(errorMessage)
        {
            Initialize();
        }

        public LanguageCodeValidator() : base("{PropertyName} doesn't match any valid 2 letter language code")
        {
            Initialize();
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null) return true;

            var asString = context.PropertyValue.ToString();
            return _languageCodes.Contains(asString);

        }

        private void Initialize()
        {
            _languageCodes = new HashSet<string>(CultureInfo
                .GetCultures(CultureTypes.SpecificCultures)
                .Select(c => c.TwoLetterISOLanguageName), StringComparer.OrdinalIgnoreCase);
        }
    }
}
