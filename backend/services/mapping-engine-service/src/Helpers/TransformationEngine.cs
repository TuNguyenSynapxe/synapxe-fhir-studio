using Newtonsoft.Json.Linq;

namespace MappingEngineService.Helpers;

public class TransformationEngine : ITransformationEngine
{
    public string? Transform(string? inputValue, string? transformationExpression)
    {
        if (string.IsNullOrWhiteSpace(transformationExpression))
            return inputValue;

        if (inputValue == null)
            return null;

        try
        {
            // Phase 1: Simple string-based transformations
            var expression = transformationExpression;

            // UPPERCASE
            if (expression.Equals("UPPERCASE", StringComparison.OrdinalIgnoreCase) || 
                expression.Equals("UPPER", StringComparison.OrdinalIgnoreCase))
            {
                return inputValue.ToUpperInvariant();
            }

            // LOWERCASE
            if (expression.Equals("LOWERCASE", StringComparison.OrdinalIgnoreCase) || 
                expression.Equals("LOWER", StringComparison.OrdinalIgnoreCase))
            {
                return inputValue.ToLowerInvariant();
            }

            // TRIM
            if (expression.Equals("TRIM", StringComparison.OrdinalIgnoreCase))
            {
                return inputValue.Trim();
            }

            // PREFIX:<value>
            if (expression.StartsWith("PREFIX:", StringComparison.OrdinalIgnoreCase))
            {
                var prefix = expression.Substring(7);
                return prefix + inputValue;
            }

            // SUFFIX:<value>
            if (expression.StartsWith("SUFFIX:", StringComparison.OrdinalIgnoreCase))
            {
                var suffix = expression.Substring(7);
                return inputValue + suffix;
            }

            // CONCAT:<value1>,<value2>,...
            if (expression.StartsWith("CONCAT:", StringComparison.OrdinalIgnoreCase))
            {
                var parts = expression.Substring(7).Split(',');
                var result = inputValue;
                foreach (var part in parts)
                {
                    result += part.Trim();
                }
                return result;
            }

            // REPLACE:<old>,<new>
            if (expression.StartsWith("REPLACE:", StringComparison.OrdinalIgnoreCase))
            {
                var parts = expression.Substring(8).Split(',');
                if (parts.Length >= 2)
                {
                    var oldValue = parts[0].Trim();
                    var newValue = parts[1].Trim();
                    return inputValue.Replace(oldValue, newValue);
                }
            }

            // CONSTANT:<value> - returns the constant value regardless of input
            if (expression.StartsWith("CONSTANT:", StringComparison.OrdinalIgnoreCase))
            {
                return expression.Substring(9).Trim();
            }

            // DEFAULT:<value> - returns value if input is null/empty, otherwise returns input
            if (expression.StartsWith("DEFAULT:", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(inputValue))
                {
                    return expression.Substring(8).Trim();
                }
                return inputValue;
            }

            // SUBSTRING:<start>,<length>
            if (expression.StartsWith("SUBSTRING:", StringComparison.OrdinalIgnoreCase))
            {
                var parts = expression.Substring(10).Split(',');
                if (parts.Length >= 1 && int.TryParse(parts[0].Trim(), out var start))
                {
                    if (start < 0 || start >= inputValue.Length)
                        return inputValue;

                    if (parts.Length >= 2 && int.TryParse(parts[1].Trim(), out var length))
                    {
                        if (start + length > inputValue.Length)
                            length = inputValue.Length - start;
                        return inputValue.Substring(start, length);
                    }
                    return inputValue.Substring(start);
                }
            }

            // If no transformation matches, return original value
            return inputValue;
        }
        catch
        {
            // On transformation failure, return original value
            return inputValue;
        }
    }
}
