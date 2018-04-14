using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Utils
{
    public enum FilterOperators
    {
        Eq,
        Neq,
        Like,
        In,
        Between,
    }

    public class FilterHelper
    {
        private readonly ILogger _logger;

        private const string DefaultValidationMsg =
            @"Filter must match JSON object in form of {'Eq | Neq' : {'str_prop' : 'v1', 'num_prop' : 123 }, 'Like' : { 'str_prop' : 'xz'}, 'In' : { 'prop' : ['option1', 'option2'] } }, where allowed operators are: [OPERATORS] and allowed properties are: [PROPERTIES]";

        public delegate IEnumerable<string> OnNewFilterItem<in TContext>(FilterOperators filterOperator, FilterItem item, object value, bool shouldUseQuotes, TContext ctx);

        public FilterHelper(ILogger logger)
        {
            _logger = logger;
        }

        public ICollection<string> ProcessJsonFilter<TContext>(
            string filterAsJson,
            ICollection<FilterItem> properties,
            OnNewFilterItem<TContext> onNewFilterItem,
            TContext userContext)
        {
            if (string.IsNullOrWhiteSpace(filterAsJson))
            {
                _logger.Debug("Filter is an empty string. Nothing to do ...");
                return new string[0];
            }

            List<string> messages = new List<string>();
            Dictionary<FilterOperators, Dictionary<string, JToken>> filterObject;
            try
            {
                filterObject = JsonConvert.DeserializeObject<Dictionary<FilterOperators, Dictionary<string, JToken>>>(filterAsJson);
                if (null == filterObject) return new[] { $"Cannot deserialize invalid JSON content '{filterAsJson}'" };
            }
            catch (Exception err)
            {
                messages.Add(FromatErrorMessage(properties));

                string msg = $"Error processing filter - {err.Message} <{err.GetType().FullName}>";
                _logger.Warn(msg);

                messages.Add(msg);
                return messages;
            }

            foreach (var byOperator in filterObject.Where(kv => null != kv.Value && kv.Value.Count > 0))
            {
                var @operator = byOperator.Key;
                _logger.Debug("Processing filter definitions for operator {0}...", @operator);

                foreach (var filterPair in byOperator.Value)
                {
                    var item = properties.FirstOrDefault(p => string.Equals(p.FriendlyName, filterPair.Key, StringComparison.OrdinalIgnoreCase));
                    if (null == item)
                    {
                        var message = $"Could not find property '{filterPair.Key}' in the collection of filterable properties. Available properties are {string.Join(", ", properties.Select(p => p.FriendlyName))}";
                        _logger.Warn(message);
                        messages.Add(message);
                        continue;
                    }

                    Type targetType = item.Property.PropertyType;
                    if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        targetType = targetType.GetGenericArguments()[0];
                    }

                    bool shouldUseQuote = targetType == typeof(string) || targetType.IsEnum || targetType == typeof(DateTime) || targetType == typeof(DateTimeOffset);
                    object targetObject = null;

                    try
                    {
                        JValue singleValue = filterPair.Value as JValue;
                        JArray multipleValues = filterPair.Value as JArray;
                        if (null != singleValue)
                        {
                            targetObject = singleValue.ToObject(targetType);
                            if (targetType == typeof(string))
                            {
                                targetObject = ((string) targetObject)?.Replace("'", "''");
                            }
                        }
                        if (null != multipleValues)
                        {
                            var targetObjectArray = multipleValues
                                .Children()
                                .Select(t => t.ToObject(targetType))
                                .Select(p =>
                                {
                                    if (targetType == typeof(string) && null != p)
                                    {
                                        return ((string)p).Replace("'", "''");
                                    }
                                    return p;
                                })
                                .ToArray();

                            targetObject = targetObjectArray;

                            if (targetObjectArray.Length != 2 && @operator == FilterOperators.Between)
                            {
                                string message =
                                    $"Failed to process filter value for {item.FriendlyName} = '{filterPair.Value}' - between operator requires exactly 2 values";
                                _logger.Warn(message);
                                messages.Add(message);
                                continue;
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        string message =
                            $"Failed to process filter value for {item.FriendlyName} = '{filterPair.Value}' with error {err.Message} <{err.GetType().FullName}>";

                        _logger.Warn(message);
                        messages.Add(message);
                        continue;
                    }

                    foreach (var errorMessage in onNewFilterItem(@operator, item, targetObject, shouldUseQuote, userContext))
                    {
                        _logger.Warn(errorMessage);
                        messages.Add(errorMessage);
                    }
                }
            }

            return messages;
        }

        public string FilterOperatorToString(FilterOperators op)
        {
            switch (op)
            {
                case FilterOperators.Eq:
                    return " = ";

                case FilterOperators.Neq:
                    return " <> ";

                case FilterOperators.Like:
                    return " like ";

                case FilterOperators.In:
                    return " in  ";

                case FilterOperators.Between:
                    return " between ";
            }

            _logger.Warn("Operator {0} is not supported!", op);
            throw new NotSupportedException();
        }

        public string FormatFilterAsDbString(FilterOperators filterOpeator, string propertyName, object value, bool shouldUseQuotes)
        {
            var whereClause = new StringBuilder(propertyName);
            whereClause.Append(FilterOperatorToString(filterOpeator));
            whereClause.Append(filterOpeator == FilterOperators.In ? "(" : string.Empty);

            foreach (var formatted in FormatValue(value, shouldUseQuotes, filterOpeator == FilterOperators.Like, filterOpeator == FilterOperators.Between))
                whereClause.Append(formatted);

            whereClause.Append(filterOpeator == FilterOperators.In ? ")" : string.Empty);

            var result = whereClause.ToString();
            return result;
        }

        private IEnumerable<string> FormatValue(object value, bool shouldUseQuotes, bool useLike, bool useBetween)
        {
            object[] valueAsArray = value as object[];
            if (null != valueAsArray)
            {
                for (int i = 0; i < valueAsArray.Length; ++i)
                {
                    if (shouldUseQuotes) yield return "'";
                    yield return valueAsArray[i].ToString();
                    if (useLike) yield return "%";
                    if (shouldUseQuotes) yield return "'";
                    if (i != valueAsArray.Length - 1) yield return useBetween ? " and " : ",";
                }
            }
            else
            {
                if (shouldUseQuotes) yield return "'";
                yield return value.ToString();
                if (useLike) yield return "%";
                if (shouldUseQuotes) yield return "'";
            }
        }

        public string FromatErrorMessage(ICollection<FilterItem> properties)
        {
            return DefaultValidationMsg
                .Replace("[OPERATORS]", string.Join(", ", Enum.GetNames(typeof(FilterOperators))))
                .Replace("[PROPERTIES]", string.Join(", ", properties.Select(p => p.FriendlyName)));
        }

        [DebuggerDisplay("{FriendlyName} [{Property.Name} <{Property.PropertyType.FullName}>]", Name = "{FriendlyName}")]
        public class FilterItem
        {
            public FilterItem(PropertyInfo property) : this(property.Name, property)
            {
            }

            public FilterItem(string friendlyName, PropertyInfo property)
            {
                FriendlyName = friendlyName;
                Property = property;
            }

            public string FriendlyName { get; }
            public PropertyInfo Property { get; }
        }
    }
}
