﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NHibernate.Linq;
using NHibernate.Linq.Visitors;
using NHibernate.Util;
using NUnit.Framework;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;

namespace NHibernate.Test.Linq
{
	using System.Threading.Tasks;
	[TestFixture]
	public class CustomPreTransformRegistrarTestsAsync : LinqTestCase
	{
		protected override void Configure(Cfg.Configuration configuration)
		{
			configuration.Properties[Cfg.Environment.PreTransformerRegistrar] = typeof(PreTransformerRegistrar).AssemblyQualifiedName;
		}

		[Test]
		public async Task RewriteLikeAsync()
		{
			// This example shows how to use the pre-transformer registrar to rewrite the
			// query so that StartsWith, EndsWith and Contains methods will generate the same sql.
			var queryPlanCache = GetQueryPlanCache();
			queryPlanCache.Clear();
			await (db.Customers.Where(o => o.ContactName.StartsWith("A")).ToListAsync());
			await (db.Customers.Where(o => o.ContactName.EndsWith("A")).ToListAsync());
			await (db.Customers.Where(o => o.ContactName.Contains("A")).ToListAsync());

			Assert.That(queryPlanCache.Count, Is.EqualTo(1));
		}

		[Serializable]
		public class PreTransformerRegistrar : IExpressionTransformerRegistrar
		{
			public void Register(ExpressionTransformerRegistry expressionTransformerRegistry)
			{
				expressionTransformerRegistry.Register(new LikeTransformer());
			}
		}

		private class LikeTransformer : IExpressionTransformer<MethodCallExpression>
		{
			private static readonly MethodInfo Like = ReflectHelper.GetMethodDefinition(() => SqlMethods.Like(null, null));
			private static readonly MethodInfo EndsWith = ReflectHelper.GetMethodDefinition<string>(x => x.EndsWith(null));
			private static readonly MethodInfo StartsWith = ReflectHelper.GetMethodDefinition<string>(x => x.StartsWith(null));
			private static readonly MethodInfo Contains = ReflectHelper.GetMethodDefinition<string>(x => x.Contains(null));
			private static readonly Dictionary<MethodInfo, Func<object, string>> ValueTransformers =
				new Dictionary<MethodInfo, Func<object, string>>
				{
					{StartsWith, s => $"{s}%"},
					{EndsWith, s => $"%{s}"},
					{Contains, s => $"%{s}%"},
				};

			public Expression Transform(MethodCallExpression expression)
			{
				if (ValueTransformers.TryGetValue(expression.Method, out var valueTransformer) &&
				    expression.Arguments[0] is ConstantExpression constantExpression)
				{
					return Expression.Call(
						Like,
						expression.Object,
						Expression.Constant(valueTransformer(constantExpression.Value))
					);
				}

				return expression;
			}

			public ExpressionType[] SupportedExpressionTypes { get; } = {ExpressionType.Call};
		}
	}
}
