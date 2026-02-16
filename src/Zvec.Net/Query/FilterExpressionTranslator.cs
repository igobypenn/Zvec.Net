using System.Linq.Expressions;
using System.Text;
using Zvec.Net.Exceptions;
using Zvec.Net.Models;

namespace Zvec.Net.Query;

internal sealed class FilterExpressionTranslator<T> : ExpressionVisitor where T : IDocument
{
    private readonly StringBuilder _filter = new();
    private static readonly HashSet<string> SupportedMethods = new()
    {
        nameof(string.StartsWith),
        nameof(string.EndsWith),
        nameof(string.Contains)
    };

    public string Translate(Expression<Func<T, bool>> predicate)
    {
        _filter.Clear();
        Visit(predicate.Body);
        return _filter.ToString();
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        _filter.Append('(');

        Visit(node.Left);

        var op = node.NodeType switch
        {
            ExpressionType.Equal => node.Right is ConstantExpression { Value: null }
                ? " IS NULL"
                : " == ",
            ExpressionType.NotEqual => node.Right is ConstantExpression { Value: null }
                ? " IS NOT NULL"
                : " != ",
            ExpressionType.GreaterThan => " > ",
            ExpressionType.GreaterThanOrEqual => " >= ",
            ExpressionType.LessThan => " < ",
            ExpressionType.LessThanOrEqual => " <= ",
            ExpressionType.AndAlso => " && ",
            ExpressionType.OrElse => " || ",
            _ => throw new UnsupportedExpressionException(
                $"Binary operator '{node.NodeType}' is not supported in filter expressions. " +
                $"Supported operators: ==, !=, <, <=, >, >=, &&, ||")
        };

        _filter.Append(op);

        if (op is not (" IS NULL" or " IS NOT NULL"))
        {
            Visit(node.Right);
        }

        _filter.Append(')');
        return node;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        _filter.Append(node.Member.Name);
        return node;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        var value = node.Value;
        _filter.Append(value switch
        {
            string s => $"'{EscapeString(s)}'",
            bool b => b.ToString().ToLowerInvariant(),
            null => "null",
            float f => f.ToString("G", System.Globalization.CultureInfo.InvariantCulture),
            double d => d.ToString("G", System.Globalization.CultureInfo.InvariantCulture),
            decimal dec => dec.ToString("G", System.Globalization.CultureInfo.InvariantCulture),
            _ => value.ToString()
        });
        return node;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method.DeclaringType == typeof(string) && SupportedMethods.Contains(node.Method.Name))
        {
            Visit(node.Object!);

            switch (node.Method.Name)
            {
                case nameof(string.StartsWith):
                    _filter.Append(" HAS_PREFIX ");
                    break;
                case nameof(string.EndsWith):
                    _filter.Append(" HAS_SUFFIX ");
                    break;
                case nameof(string.Contains):
                    _filter.Append(" CONTAIN_ANY ");
                    break;
            }

            Visit(node.Arguments[0]);
            return node;
        }

        throw new UnsupportedExpressionException(
            $"Method '{node.Method.Name}' is not supported in filter expressions.\n" +
            $"Supported string methods: StartsWith, EndsWith, Contains\n" +
            $"Supported operators: ==, !=, <, <=, >, >=, &&, ||, null checks");
    }

    protected override Expression VisitUnary(UnaryExpression node)
    {
        if (node.NodeType == ExpressionType.Convert || node.NodeType == ExpressionType.ConvertChecked)
        {
            Visit(node.Operand);
            return node;
        }

        if (node.NodeType == ExpressionType.Not)
        {
            _filter.Append('!');
            Visit(node.Operand);
            return node;
        }

        throw new UnsupportedExpressionException(
            $"Unary operator '{node.NodeType}' is not supported in filter expressions.");
    }

    private static string EscapeString(string s) => s.Replace("'", "\\'");
}
