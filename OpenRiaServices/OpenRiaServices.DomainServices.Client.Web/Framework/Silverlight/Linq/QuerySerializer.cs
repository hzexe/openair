using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace OpenRiaServices.DomainServices.Client
{
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// This serializer supports Where, OrderBy, Skip and Take.
    /// </summary>
    internal static class QuerySerializer
    {
        public static List<ServiceQueryPart> Serialize(IQueryable query)
        {
            List<ServiceQueryPart> queryParts;

            Expression expr = query.Expression;
            MethodCallConverter converter = new MethodCallConverter();
            expr = converter.Visit(expr);

            Visitor visitor = new Visitor();
            visitor.Visit(expr, out queryParts);

            return queryParts;
        }

        internal class Visitor : ExpressionVisitor
        {
            private ServiceQueryPart _currPart;
            private StringBuilder _currPartBuilder;
            private List<ServiceQueryPart> _queryParts;
            private Expression _queryRoot;
            private bool _inLambdaExpression;

            public void Visit(Expression expr, out List<ServiceQueryPart> queryPartsList)
            {
                // determine the root of the query to ensure that we don't
                // evaluate it locally
                this._queryRoot = GetQueryRoot(expr);
                Debug.Assert(this._queryRoot.NodeType == ExpressionType.Constant, "query root should be an expression with a constant value");

                expr = Evaluator.PartialEval(expr, this.CanBeEvaluated);

                this._queryParts = new List<ServiceQueryPart>();
                this.Visit(expr);

                this._queryParts.Reverse();
                this.FlattenOrderExpressions();

                queryPartsList = this._queryParts;
            }

            protected override Expression VisitConditional(ConditionalExpression c)
            {
                // format conditional : iff(Test, IfTrue, IfFalse)
                this._currPartBuilder.Append("iif(");
                Expression test = this.Visit(c.Test);
                this._currPartBuilder.Append(",");
                Expression ifTrue = this.Visit(c.IfTrue);
                this._currPartBuilder.Append(",");
                Expression ifFalse = this.Visit(c.IfFalse);
                this._currPartBuilder.Append(")");

                if (test != c.Test || ifTrue != c.IfTrue || ifFalse != c.IfFalse)
                {
                    return Expression.Condition(test, ifTrue, ifFalse);
                }
                return c;
            }

            protected override Expression VisitBinary(BinaryExpression b)
            {
                this._currPartBuilder.Append("(");

                Expression left = this.Visit(b.Left);

                switch (b.NodeType)
                {
                    case ExpressionType.Equal:
                        this._currPartBuilder.Append("==");
                        break;
                    case ExpressionType.NotEqual:
                        this._currPartBuilder.Append("!=");
                        break;
                    case ExpressionType.AndAlso:
                        this._currPartBuilder.Append("&&");
                        break;
                    case ExpressionType.OrElse:
                        this._currPartBuilder.Append("||");
                        break;
                    case ExpressionType.GreaterThan:
                        this._currPartBuilder.Append(">");
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        this._currPartBuilder.Append(">=");
                        break;
                    case ExpressionType.LessThan:
                        this._currPartBuilder.Append("<");
                        break;
                    case ExpressionType.LessThanOrEqual:
                        this._currPartBuilder.Append("<=");
                        break;
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                        this._currPartBuilder.Append("*");
                        break;
                    case ExpressionType.Modulo:
                        this._currPartBuilder.Append("%");
                        break;
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                        this._currPartBuilder.Append("-");
                        break;
                    case ExpressionType.Divide:
                        this._currPartBuilder.Append("/");
                        break;
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                        this._currPartBuilder.Append("+");
                        break;
                    case ExpressionType.ArrayIndex:
                        // handled below - the right expression
                        // is the index
                        break;
                    case ExpressionType.And:
                    case ExpressionType.Or:
                    case ExpressionType.ExclusiveOr:
                    case ExpressionType.LeftShift:
                    case ExpressionType.RightShift:
                        throw new NotSupportedException(Resources.QuerySerialization_BitwiseOperatorsNotSupported);
                    default:
                        throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Resources.QuerySerialization_UnsupportedBinaryOp, b.NodeType.ToString()));
                }

                if (b.NodeType == ExpressionType.ArrayIndex)
                {
                    this._currPartBuilder.Append("[");
                }

                Expression right = this.Visit(b.Right);

                if (b.NodeType == ExpressionType.ArrayIndex)
                {
                    this._currPartBuilder.Append("]");
                }

                this._currPartBuilder.Append(")");

                Expression conversion = this.Visit(b.Conversion);
                if (left != b.Left || right != b.Right || conversion != b.Conversion)
                {
                    if (b.NodeType == ExpressionType.Coalesce && b.Conversion != null)
                    {
                        return Expression.Coalesce(left, right, conversion as LambdaExpression);
                    }
                    else
                    {
                        return Expression.MakeBinary(b.NodeType, left, right, b.IsLiftedToNull, b.Method);
                    }
                }
                return b;
            }

            protected override Expression VisitUnary(UnaryExpression u)
            {
                switch (u.NodeType)
                {
                    case ExpressionType.Not:
                        if (u.Operand.Type == typeof(bool) || u.Operand.Type == typeof(bool?))
                        {
                            this._currPartBuilder.Append("!");
                        }
                        else
                        {
                            throw new NotSupportedException(Resources.QuerySerialization_BitwiseOperatorsNotSupported);
                        }
                        break;
                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                        this._currPartBuilder.Append("-");
                        break;
                    case ExpressionType.ArrayLength:
                        this.Visit(u.Operand);
                        this._currPartBuilder.Append(".Length");
                        return u;
                    case ExpressionType.Quote:
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                        break;
                    default:
                        throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Resources.QuerySerialization_UnsupportedUnaryOp, u.NodeType.ToString()));
                }

                return base.VisitUnary(u);
            }

            protected override Expression VisitMethodCall(MethodCallExpression m)
            {
                ServiceQueryPart holdPart = this._currPart;
                StringBuilder holdPartBuilder = this._currPartBuilder;

                string methodName = m.Method.Name.ToLower(CultureInfo.InvariantCulture);
                if (IsSequenceOperatorCall(m))
                {
                    if (methodName == "where" || methodName == "orderby" || methodName == "orderbydescending" || methodName == "skip"
                        || methodName == "take" || methodName == "thenby" || methodName == "thenbydescending")
                    {
                        this._currPart = new ServiceQueryPart();
                        this._currPartBuilder = new StringBuilder();
                        this._currPart.QueryOperator = methodName;
                        this._queryParts.Add(this._currPart);
                    }
                    else if (methodName == "select")
                    {
                        // if the selection specified is anything other than an
                        // empty selector, throw an exception. I.e. only Select(p => p)
                        // is supported.
                        bool isEmptySelection = false;
                        Expression selector = m.Arguments[1];
                        if (selector.NodeType == ExpressionType.Quote)
                        {
                            LambdaExpression lex = (LambdaExpression)((UnaryExpression)m.Arguments[1]).Operand;
                            isEmptySelection = (lex.Body.NodeType == ExpressionType.Parameter) &&
                                               (lex.Parameters.Single() == lex.Body);
                        }
                        if (!isEmptySelection)
                        {
                            throw new NotSupportedException(Resources.QuerySerialization_ProjectionsNotSupported);
                        }
                    }
                    else
                    {
                        throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Resources.QuerySerialization_UnsupportedQueryOperator, m.Method.Name));
                    }

                    Expression obj = this.Visit(m.Object);
                    IEnumerable<Expression> args = this.Visit(m.Arguments);
                    if (obj != m.Object || args != m.Arguments)
                    {
                        return Expression.Call(obj, m.Method, args);
                    }

                    // post processing
                    if (methodName == "orderbydescending" || methodName == "thenbydescending")
                    {
                        if (methodName == "orderbydescending")
                        {
                            this._currPart.QueryOperator = "orderby";
                        }
                        else
                        {
                            this._currPart.QueryOperator = "thenby";
                        }

                        this._currPartBuilder.Append(" desc");
                    }
                }
                else if (m.Method.DeclaringType == typeof(Enum) && m.Method.Name == "HasFlag")
                {
                    // Serialize (p => p.enumProp.HasFlag( EnumType.A)) into "(it.enumProp has EnumType.A)"

                    this._currPartBuilder.Append("(");
                    this.Visit(m.Object);

                    // We could convert it to an int here if possible, otherwise we will do it anyway on the server
                    //this.Visit(Expression.Convert(m.Object, typeof(int)));
                    this._currPartBuilder.Append(" has ");

                    // We could convert it to an int here if possible, otherwise we will do it anyway on the server
                    //this.Visit(Expression.Convert(m.Arguments[0], typeof(int)));
                    this.Visit(m.Arguments[0]);
                   
                    this._currPartBuilder.Append(")");
                }
                else
                {
                    // Ensure that the method is accessible
                    // Only methods on supported predefined types are allowed
                    VerifyMethodAccessibility(m);

                    if (m.Method.IsStatic)
                    {
                        this._currPartBuilder.Append(m.Method.DeclaringType.Name);
                    }
                    else
                    {
                        this.Visit(m.Object);
                    }
                    if (m.Object == null || m.Object.NodeType != ExpressionType.Parameter)
                    {
                        // for all member accesses other than those directly off of
                        // our query parameter, we need to append a dot
                        this._currPartBuilder.Append(".");
                    }
                    this._currPartBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}(", m.Method.Name);
                    this.VisitMethodParameters(m.Arguments);
                    this._currPartBuilder.Append(")");
                }

                if (this._currPart != null)
                {
                    this._currPart.Expression = this._currPartBuilder.ToString();
                }
                this._currPart = holdPart;
                this._currPartBuilder = holdPartBuilder;

                return m;
            }

            protected override Expression VisitNew(NewExpression nex)
            {
                // REVIEW: When does this code get hit?
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Resources.QuerySerialization_NewExpressionsNotSupported));
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                if (this._inLambdaExpression)
                {
                    throw new NotSupportedException(Resources.QuerySerialization_NestedQueriesNotSupported);
                }

                this._inLambdaExpression = true;

                Expression ret = base.VisitLambda<T>(node);

                this._inLambdaExpression = false;

                return ret;
            }

            protected override Expression VisitConstant(ConstantExpression c)
            {
                if (c.Value != null && c.Value is LambdaExpression)
                {
                    // if after local evaluation we have a constant lambda expression,
                    // we unwrap it's body and evaluate
                    LambdaExpression lex = Expression.Lambda(((LambdaExpression)c.Value).Body);
                    Delegate fn = lex.Compile();
                    object value = fn.DynamicInvoke(null);
                    this.FormatConstant(value);
                }
                else if (!typeof(IQueryable).IsAssignableFrom(c.Type))
                {
                    this.FormatConstant(c.Value);
                }

                return base.VisitConstant(c);
            }

            protected override Expression VisitMember(MemberExpression m)
            {
                Expression ret = base.VisitMember(m);

                if (m.Expression == null)
                {
                    // static member access
                    this._currPartBuilder.Append(m.Member.DeclaringType.Name);
                }

                if (m.Expression == null || m.Expression.NodeType != ExpressionType.Parameter)
                {
                    // for all member accesses other than those directly off of
                    // our query parameter, we need to append a dot
                    this._currPartBuilder.Append(".");
                }
                else
                {
                    // prepend "it." for all parameter references to disambiguate calls to properties with names 
                    // that are also reserved keywords.
                    this._currPartBuilder.Append("it.");
                }

                this._currPartBuilder.Append(m.Member.Name);

                return ret;
            }

            /// <summary>
            /// Drill through MethodCallExpression chain searching for the root of
            /// the query.
            /// </summary>
            /// <param name="expr">The <see cref="Expression"/> to search.</param>
            /// <returns>root of the query</returns>
            private static Expression GetQueryRoot(Expression expr)
            {
                Expression queryRoot = expr;
                while (queryRoot.NodeType == ExpressionType.Call)
                {
                    queryRoot = ((MethodCallExpression)queryRoot).Arguments[0];
                }
                return queryRoot;
            }

            private static bool IsSequenceOperatorCall(MethodCallExpression mc)
            {
                Type declType = mc.Method.DeclaringType;
                if (declType == typeof(Enumerable) || declType == typeof(Queryable))
                {
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Verify that the declaring type of the method is one of the supported types.
            /// Note that this list of types corresponds to the list of accessible types
            /// defined by the server query parser.
            /// </summary>
            /// <param name="mce">The method call expression.</param>
            private static void VerifyMethodAccessibility(MethodCallExpression mce)
            {
                Type declaringType = mce.Method.DeclaringType;
                bool isSupportedMethod = false;

                // methods on our supported types are supported
                if (TypeUtility.IsPredefinedType(declaringType) || (declaringType == typeof(Math))
                    || (declaringType == typeof(Convert)) || (declaringType == typeof(object)))
                {
                    isSupportedMethod = true;
                }

                if (!isSupportedMethod)
                {
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Resources.QuerySerialization_MethodNotAccessible, mce.Method.Name, declaringType));
                }
            }

            /// <summary>
            /// Ensure that the specified constant value is of a Type
            /// supported by the server query serializer - we don't want
            /// to serialize a value that the server can't handle.
            /// </summary>
            /// <param name="value">The constant value</param>
            private static void ValidateConstant(object value)
            {
                if (value == null)
                {
                    return;
                }

                if (!TypeUtility.IsPredefinedSimpleType(value.GetType()))
                {
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Resources.QuerySerialization_UnsupportedType, value.GetType()));
                }
            }

            private bool CanBeEvaluated(Expression expression)
            {
                if (expression.NodeType == ExpressionType.MemberAccess)
                {
                    MemberExpression mex = (MemberExpression)expression;
                    if ((mex.Member.DeclaringType == typeof(DateTime) || mex.Member.DeclaringType == typeof(DateTimeOffset)) &&
                        mex.Member.MemberType == System.Reflection.MemberTypes.Property &&
                        mex.Expression == null)
                    {
                        // DateTime.Now, Today, etc. static members are remoted
                        return false;
                    }
                }
                return (expression.NodeType != ExpressionType.Parameter && !(expression == this._queryRoot));
            }

            /// <summary>
            /// Unify all ordering expressions into a single order expression
            /// </summary>
            private void FlattenOrderExpressions()
            {
                List<ServiceQueryPart> newList = new List<ServiceQueryPart>();
                ServiceQueryPart orderPart = null;
                foreach (ServiceQueryPart part in this._queryParts)
                {
                    if (part.QueryOperator.StartsWith("orderby", StringComparison.Ordinal))
                    {
                        orderPart = part;
                    }

                    if (part.QueryOperator.StartsWith("thenby", StringComparison.Ordinal))
                    {
                        // accumulate secondary ordering expressions
                        orderPart.Expression += ", " + part.Expression;
                    }
                    else
                    {
                        newList.Add(part);
                    }
                }

                this._queryParts = newList;
            }

            private void VisitMethodParameters(ReadOnlyCollection<Expression> parameters)
            {
                for (int i = 0, n = parameters.Count; i < n; i++)
                {
                    if (i > 0)
                    {
                        this._currPartBuilder.Append(", ");
                    }
                    this.Visit(parameters[i]);
                }
            }

            private void FormatConstant(object c)
            {
                ValidateConstant(c);

                string value = string.Empty;
                Type valueType = c != null ? c.GetType() : null;
                if (c == null)
                {
                    value = "null";
                }
                else if (valueType == typeof(string))
                {
                    // Wrap quotes and backslashes.
                    value = "\"" + Convert.ToString(c, CultureInfo.InvariantCulture).Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
                }
                else if (valueType == typeof(char))
                {
                    // Wrap quotes and backslashes.
                    value = "'" + Convert.ToString(c, CultureInfo.InvariantCulture).Replace("\\", "\\\\").Replace("'", "\\'") + "'";
                }
                else if (valueType.IsEnum)
                {
                    // SL doesn't support Enum.Format
                    value = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", valueType.Name, c);
                }
                else if (valueType == typeof(DateTime))
                {
                    // DateTime constants are always formatted as a construction expression
                    // using the number of ticks and the kind.
                    DateTime dt = (DateTime)c;
                    this._currPartBuilder.AppendFormat(CultureInfo.InvariantCulture, "DateTime({0},{1})", dt.Ticks, "\"" + dt.Kind.ToString() + "\"");
                }
                else if (valueType == typeof(DateTimeOffset))
                {
                    // DateTimeOffset constants are always formatted as a construction expression 
                    // using the number of ticks and the offset in the form of TimeSpan
                    DateTimeOffset dto = (DateTimeOffset)c;
                    this._currPartBuilder.AppendFormat(CultureInfo.InvariantCulture, "DateTimeOffset({0},TimeSpan({1}))", dto.Ticks, dto.Offset.Ticks);
                }
                else if (valueType == typeof(TimeSpan))
                {
                    // TimeSpan constants are always formatted as a construction expression
                    // using the number of ticks
                    TimeSpan ts = (TimeSpan)c;
                    this._currPartBuilder.AppendFormat(CultureInfo.InvariantCulture, "TimeSpan({0})", ts.Ticks);
                }
                else if (valueType == typeof(Uri))
                {
                    Uri uri = (Uri)c;
                    this._currPartBuilder.AppendFormat(CultureInfo.InvariantCulture, "Uri(\"{0}\")", uri.ToString());
                }
                else if (valueType == typeof(Guid))
                {
                    // the server query parser doen't support the normal string representation
                    // of Guids, so we have to serialize them as construction statements
                    byte[] bytes = ((Guid)c).ToByteArray();
                    byte[] ba1 = new byte[] { bytes[0], bytes[1], bytes[2], bytes[3] };
                    byte[] ba2 = new byte[] { bytes[4], bytes[5] };
                    byte[] ba3 = new byte[] { bytes[6], bytes[7] };
                    this._currPartBuilder.AppendFormat(
                            CultureInfo.InvariantCulture, "Guid({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10})",
                            BitConverter.ToInt32(ba1, 0),
                            (short)BitConverter.ToUInt16(ba2, 0),
                            (short)BitConverter.ToUInt16(ba3, 0),
                            bytes[8], bytes[9], bytes[10], bytes[11], bytes[12], bytes[13], bytes[14], bytes[15]);
                }
                else if (valueType == typeof(Single))
                {
                    value = Convert.ToString(c, CultureInfo.InvariantCulture) + "F";
                }
                else if (valueType == typeof(Decimal))
                {
                    value = Convert.ToString(c, CultureInfo.InvariantCulture) + "M";
                }
                else if (valueType == typeof(Double))
                {
                    value = Convert.ToString(c, CultureInfo.InvariantCulture) + "D";
                }
                else
                {
                    value = Convert.ToString(c, CultureInfo.InvariantCulture);
                }

                this._currPartBuilder.Append(value);
            }
        }

        /// <summary>
        /// This visitor is used to make any required method call translations.
        /// </summary>
        internal class MethodCallConverter : ExpressionVisitor
        {
            protected override Expression VisitUnary(UnaryExpression u)
            {
                if (u.NodeType == ExpressionType.Convert)
                {
                    Expression operand = this.Visit(u.Operand);
                    if (u.Type == typeof(object))
                    {
                        // Remove the conversion.
                        return operand;
                    }
                    else if (u.Type == typeof(bool) && (operand.NodeType == ExpressionType.Conditional || MethodCallConverter.IsComparison(operand)))
                    {
                        // We don't want to try and convert the conditional/comparison to a boolean because 
                        // its type may no longer be an object/string (the supported types for Conversions.ToBoolean).
                        return operand;
                    }
                }
                
                return base.VisitUnary(u);
            }

            protected override Expression VisitMethodCall(MethodCallExpression m)
            {
                if (m.Method.DeclaringType.FullName == "Microsoft.VisualBasic.CompilerServices.Operators" && m.Arguments.Count == 3)
                {
                    ExpressionType comparisonType;
                    if (MethodCallConverter.GetVBObjectComparisonType(m, out comparisonType))
                    {
                        // Translate CompareObject[operator](x, y) to x [operator] y.
                        ReadOnlyCollection<Expression> args = this.Visit(m.Arguments);
                        return Expression.MakeBinary(comparisonType, args[0], args[1]);
                    }
                }
                else if (m.Method.DeclaringType.FullName == "Microsoft.VisualBasic.Interaction"
                    && m.Method.Name == "IIf"
                    && m.Arguments.Count == 3)
                {
                    // Translate VB's "iif(test, truePart, falsePart)" into "test ? truePart : falsePart".
                    ReadOnlyCollection<Expression> args = this.Visit(m.Arguments);
                    return Expression.Condition(args[0], args[1], args[2]);
                }

                return base.VisitMethodCall(m);
            }

            protected override Expression VisitBinary(BinaryExpression b)
            {
                BinaryExpression binaryExpression = b.Left as BinaryExpression;
                ConstantExpression constant = b.Right as ConstantExpression;

                // Ignore VB coalesce operator.
                if (binaryExpression != null && b.NodeType == ExpressionType.Coalesce)
                {
                    // VB translates comparisons against nullable values to the following expression:
                    //     (comparison) ?? false // E.g. (nullableInt > 5) ?? false
                    // Because we don't support coalesce on the server, we will convert this expression to:
                    //     (comparison) // E.g. (nullableInt > 5)
                    if (constant != null && constant.Type == typeof(bool) && ((bool)constant.Value == false) && 
                        MethodCallConverter.IsComparison(b.Left))
                    {
                        return Expression.MakeBinary(binaryExpression.NodeType, binaryExpression.Left, binaryExpression.Right);
                    }
                }

                MethodCallExpression mce = b.Left as MethodCallExpression;

                // Translate VB equality/inequality comparisons into normal string comparisons
                bool isVbCompareString =
                    mce != null && mce.Method.DeclaringType.FullName == "Microsoft.VisualBasic.CompilerServices.Operators" &&
                    mce.Method.Name == "CompareString" && mce.Arguments.Count == 3;
                if (isVbCompareString && constant != null && constant.Type == typeof(int) && (int)constant.Value == 0)
                {
                    Expression left = mce.Arguments[0];
                    Expression right = mce.Arguments[1];
                    bool caseSensitive = true;
                    ConstantExpression textCompareParam = mce.Arguments[2] as ConstantExpression;
                    if (textCompareParam != null && textCompareParam.Type == typeof(bool) && (bool)textCompareParam.Value == true)
                    {
                        // the CompareString param is means ignore case, so we invert here
                        caseSensitive = false;
                    }

                    if ((b.NodeType == ExpressionType.Equal || b.NodeType == ExpressionType.NotEqual) && caseSensitive)
                    {
                        // Case sensitive equality/inequality comparisons translate to simple equality/inequality operators
                        // CompareString(expr1, expr2, false) == 0 translates to expr1 == expr2
                        // CompareString(expr1, expr2, false) != 0 translates to expr1 != expr2
                        return Expression.MakeBinary(b.NodeType, left, right);
                    }
                    else
                    {
                        // for all other comparisons, we make an equivalent translation to string.Compare
                        return Expression.MakeBinary(b.NodeType, MakeVBCompare(left, right, caseSensitive), Expression.Constant(0));
                    }
                }

                return base.VisitBinary(b);
            }

            /// <summary>
            /// For the specified arguments and case sensitivity, return a expression
            /// representing an equivalent call to string.Compare.
            /// </summary>
            /// <param name="left">The expression on the left-hand side.</param>
            /// <param name="right">The expression on the right-hand side.</param>
            /// <param name="caseSensitive">Indicates whether to do a case-sensitive comparison.</param>
            /// <returns>Returns the expression.</returns>
            private static MethodCallExpression MakeVBCompare(Expression left, Expression right, bool caseSensitive)
            {
                MethodCallExpression mce = null;
                if (caseSensitive)
                {
                    // Case sensitive comparisons are the default, so are translated to
                    // string.Compare(expr1, expr2)
                    MethodInfo strCompareMethodInfo = typeof(string)
                        .GetMethod("Compare", BindingFlags.Static | BindingFlags.Public, null,
                        new Type[] { typeof(string), typeof(string) }, null);
                    mce = Expression.Call(null, strCompareMethodInfo, left, right);
                }
                else
                {
                    // Case insensitive comparisons result in a call to
                    // string.Compare(expr1, expr2, StringComparison.OrdinalIgnoreCase)
                    MethodInfo strCompareMethodInfo = typeof(string)
                        .GetMethod("Compare", BindingFlags.Static | BindingFlags.Public, null,
                        new Type[] { typeof(string), typeof(string), typeof(StringComparison) }, null);
                    mce = Expression.Call(null, strCompareMethodInfo, left, right, Expression.Constant(StringComparison.OrdinalIgnoreCase));
                }

                return mce;
            }

            private static bool IsComparison(Expression expression)
            {
                MethodCallExpression mce = expression as MethodCallExpression;
                if (mce == null)
                {
                    switch (expression.NodeType)
                    {
                        case ExpressionType.Equal:
                        case ExpressionType.LessThan:
                        case ExpressionType.LessThanOrEqual:
                        case ExpressionType.GreaterThan:
                        case ExpressionType.GreaterThanOrEqual:
                        case ExpressionType.NotEqual:
                            return true;
                        default:
                            return false;
                    }
                }

                ExpressionType ignored;
                return MethodCallConverter.GetVBObjectComparisonType(mce, out ignored);
            }

            private static bool GetVBObjectComparisonType(MethodCallExpression m, out ExpressionType comparisonType)
            {
                comparisonType = (ExpressionType)(-1);

                if (!String.Equals(m.Method.DeclaringType.FullName, "Microsoft.VisualBasic.CompilerServices.Operators", StringComparison.Ordinal))
                {
                    return false;
                }

                // We ignore the 3rd argument (comparison type), since we don't support comparing strings anyway.
                switch (m.Method.Name)
                {
                    case "CompareObjectEqual":
                        comparisonType = ExpressionType.Equal;
                        return true;
                    case "CompareObjectNotEqual":
                        comparisonType = ExpressionType.NotEqual;
                        return true;
                    case "CompareObjectGreater":
                        comparisonType = ExpressionType.GreaterThan;
                        return true;
                    case "CompareObjectGreaterEqual":
                        comparisonType = ExpressionType.GreaterThanOrEqual;
                        return true;
                    case "CompareObjectLess":
                        comparisonType = ExpressionType.LessThan;
                        return true;
                    case "CompareObjectLessEqual":
                        comparisonType = ExpressionType.LessThanOrEqual;
                        return true;
                }
                return false;
            }
        }
    }
}
