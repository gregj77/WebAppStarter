using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;
using Utils;
using Utils.Data;

namespace Validation
{
    public abstract class QueryArgumentsValidator<T> : BaselineValidator<T> where T : QueryArguments
    {
        private readonly FilterHelper _filterHelper;

        protected QueryArgumentsValidator(FilterHelper helper)
        {
            _filterHelper = helper;

            InitializeRuleset();
        }

        protected abstract IEnumerable<FilterHelper.FilterItem> FilterProperties { get; }

        protected abstract IEnumerable<string> SortProperties { get; }


        protected void InitializeRuleset()
        {
            When(x => x.RecordsPerPage.HasValue,
                () => RuleFor(x => x.RecordsPerPage).GreaterThan((uint)0));

            When(x => x.StartRow.HasValue,
                () => RuleFor(x => x.StartRow).GreaterThan((uint)0));

            var filters = FilterProperties.ToArray();
            RuleFor(x => x).Custom((target, context) => ValidateAndPrefillFilterObject(target, context, filters));

            var allowed = SortProperties.SelectMany(sort => new[] { sort, $"-{sort}" }).ToArray();
            var allowedAsString = string.Join(", ", allowed);
            var expr = string.Join("|", allowed);
            var regExpr = new Regex(expr, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            RuleFor(x => x.OrderBy)
                .Matches(regExpr)
                .WithMessage(x => $"{{PropertyName}} must must match one of the following sort expressions {allowedAsString}");
        }

        private void ValidateAndPrefillFilterObject(T data, CustomContext ctx, ICollection<FilterHelper.FilterItem> properties)
        {
            var filterExpressions = new List<string>();
            data.FilterProcessed = filterExpressions;

            var result = _filterHelper.ProcessJsonFilter(data.Filter, properties, ProcessFitlerValue, filterExpressions);

            result.Aggregate(ctx, (c, error) =>
            {
                c.AddFailure(nameof(data.Filter), error);
                return c;
            });
        }

        private IEnumerable<string> ProcessFitlerValue(
            FilterOperators @operator,
            FilterHelper.FilterItem item,
            object value,
            bool useQuote,
            List<string> ctx)
        {
            ctx.Add(_filterHelper.FormatFilterAsDbString(@operator, item.FriendlyName, value, useQuote));
            yield break;
        }
    }

}
