﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Parser;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using TheCommons.Core.Configuration;
using TheCommons.Forge.Exceptions;
using TheCommons.Forge.ManagedTypes.Features;
using TheCommons.Traits.Attributes;
using Expression = System.Linq.Expressions.Expression;

namespace TheCommons.Forge.ManagedTypes.Resolvers
{
    /// <summary>
    /// Type resolver that can lookup a type based on a configuration and the result of an expression.
    /// </summary>
    public class ExpressionTypeResolver : ITypeResolver
    {
        [Autowire] private IConfigurationService configurationService;
        
        private Dictionary<Func<bool>, Type> expressionDictionary =
            new Dictionary<Func<bool>, Type>();

        /// <summary>
        /// Get the type that matches should return based on the current configuration.
        /// If there are multiple this will return the first match.
        /// </summary>
        /// <returns></returns>
        public Type ResolveType()
        {
      
            var matches = expressionDictionary
                .Where(keyPair => keyPair.Key.Invoke())
                .Select(kvp => kvp.Value)
                .ToList();

            if (matches.Count > 1)
            {
                var typeNames = string.Join(",", this.expressionDictionary.Values.Select(o => o.Name));

                throw new ContainerException($"Multiple matches during ResolveType. The conditions are not exclusive. Types: {typeNames}");
            }

            return matches.FirstOrDefault();
            
        }

        class CustomExpressionPromoter : IExpressionPromoter
        {
            public Expression Promote(Expression expr, Type type, bool exact, bool convertExpr)
            {
                return expr;
            }
        }

        /// <summary>
        /// Add an expression that would allow a valid type to be returned.
        /// </summary>
        /// <param name="expressionString"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public ExpressionTypeResolver AddType(string expressionString, Type type)
        {
            // only allow variable names that
            // are valid as configuration variables could be in the form of Category:Key:Something

            Regex validVariableName = new Regex("[a-zA-Z][A-Za-z0-9]*");
            // load all configuration variables as an expression

            ParameterExpression[] pe = configurationService
                .GetKeys()
                .Where(k => validVariableName.IsMatch(k))
                .Select(o => Expression.Parameter(configurationService.Get(o).GetType(), o))
                .ToArray();


            // use an Expression parser to parse the expression string. This is a bit of a hack using
            // a class I'm calling an "Infinite Dictionary" it allows expressions to use Constants as
            // their string value (e.g. Color == Blue means configService.Get("Color") == "Blue")
            // The Infinite Dictionary returns dictionary["Key"] == "Key" for any Key.

            var parserParams = new object[] { new InfiniteDictionary() };

            var parser = new ExpressionParser(pe, expressionString, parserParams, ParsingConfig.Default);
            
            var lambda = Expression.Lambda(parser.Parse(typeof(bool)), pe.ToArray()).Compile();

            // Add a predicate to the dictionary that will test to see if the current configuration
            // matches the parsed expression.

            expressionDictionary.Add(() =>
            {
                // get the latest configuration
                object[] valueList = pe
                    .Select(o => configurationService.Get(o.Name))
                    .ToArray();
                // run the lambda
                return (bool) lambda.DynamicInvoke(valueList);
            }, type);

            return this;
        }
    }
}