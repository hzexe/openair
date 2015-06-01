using System;
using System.Collections.Generic;

namespace OpenRiaServices.DomainServices.Client
{
    using System.Globalization;
    using System.Linq.Expressions;

    internal static class Evaluator
    {
        /// <summary>
        /// Performs evaluation and replacement of independent sub-trees
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <param name="funcCanBeEvaluated">A function that decides whether a given expression 
        /// node can be part of the local function.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        public static Expression PartialEval(Expression expression, Func<Expression, bool> funcCanBeEvaluated)
        {
            return new SubtreeEvaluator(new Nominator(funcCanBeEvaluated).Nominate(expression)).Eval(expression);
        }

        /// <summary>
        /// Performs evaluation and replacement of independent sub-trees
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        public static Expression PartialEval(Expression expression)
        {
            return PartialEval(expression, Evaluator.CanBeEvaluatedLocally);
        }

        private static bool CanBeEvaluatedLocally(Expression expression)
        {
            return expression.NodeType != ExpressionType.Parameter;
        }

        /// <summary>
        /// Evaluates and replaces sub-trees when first candidate is reached (top-down)
        /// </summary>
        private class SubtreeEvaluator : ExpressionVisitor
        {
            private Dictionary<Expression, bool> _candidates;

            internal SubtreeEvaluator(Dictionary<Expression, bool> candidates)
            {
                this._candidates = candidates;
            }

            internal Expression Eval(Expression exp)
            {
                return this.Visit(exp);
            }

            public override Expression Visit(Expression exp)
            {
                if (exp == null)
                {
                    return null;
                }

                if (this._candidates.ContainsKey(exp))
                {
                    return Evaluate(exp);
                }

                return base.Visit(exp);
            }

            protected override Expression VisitMemberInit(MemberInitExpression node)
            {
                Expression newExpr = this.Visit(node.NewExpression);
                if (newExpr.NodeType == ExpressionType.Constant)
                {
                    // We translated the NewExpression to a ConstantExpression, so by-pass 
                    // VisitMemberInit's validation and return the expression as-is.
                    return newExpr;
                }

                return base.VisitMemberInit(node);
            }

            private static Expression Evaluate(Expression e)
            {
                if (e.NodeType == ExpressionType.Constant)
                {
                    return e;
                }

                LambdaExpression lambda = Expression.Lambda(e);
                Delegate fn = lambda.Compile();
                return Expression.Constant(fn.DynamicInvoke(null), e.Type);
            }
        }

        /// <summary>
        /// Performs bottom-up analysis to determine which nodes can possibly
        /// be part of an evaluated sub-tree.
        /// </summary>
        private class Nominator : ExpressionVisitor
        {
            private Dictionary<Expression, bool> _candidates;
            private Func<Expression, bool> _funcCanBeEvaluated;
            private bool _cannotBeEvaluated;

            internal Nominator(Func<Expression, bool> funcCanBeEvaluated)
            {
                this._funcCanBeEvaluated = funcCanBeEvaluated;
            }

            internal Dictionary<Expression, bool> Nominate(Expression expression)
            {
                this._candidates = new Dictionary<Expression, bool>();
                this.Visit(expression);
                return this._candidates;
            }

            public override Expression Visit(Expression expression)
            {
                if (expression != null)
                {
                    bool saveCannotBeEvaluated = this._cannotBeEvaluated;
                    this._cannotBeEvaluated = false;
                    base.Visit(expression);
                    if (!this._cannotBeEvaluated)
                    {
                        if (this._funcCanBeEvaluated(expression))
                        {
                            this._candidates[expression] = true;
                        }
                        else
                        {
                            this._cannotBeEvaluated = true;
                        }
                    }
                    this._cannotBeEvaluated |= saveCannotBeEvaluated;
                }
                return expression;
            }
        }
    }
}
