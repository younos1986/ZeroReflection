﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace ZeroReflection.Mapper
{
    public class MapperConfiguration
    {
        private readonly List<(Type Source, Type Destination)> _mappings = new();
        private readonly Dictionary<(Type Source, Type Destination), Delegate> _customMappings = new();

        private readonly Dictionary<(Type Source, Type Destination, string Property), Delegate>
            _customPropertyMappings = new();

        private readonly HashSet<(Type Source, Type Destination, string Property)> _ignoredProperties = new();
        

        public MappingBuilder<TSource, TDestination> CreateMap<TSource, TDestination>()
        {
            _mappings.Add((typeof(TSource), typeof(TDestination)));
            return new MappingBuilder<TSource, TDestination>(this);
        }

        public IEnumerable<(Type Source, Type Destination)> GetMappings() => _mappings;

        public void AddCustomMapping<TSource, TDestination>(Expression<Func<TSource, TDestination>> customMapperExpr)
        {
            // AOT-safe: Expression.Compile is not supported under NativeAOT. Use delegate overload instead.
            throw new NotSupportedException("Expression-based custom mappings are not supported in AOT. Use the Func<TSource,TDestination> overload.");
        }

        public void AddCustomMapping<TSource, TDestination>(Func<TSource, TDestination> customMapper)
        {
            _customMappings[(typeof(TSource), typeof(TDestination))] = customMapper;
        }

        public void AddPropertyMapping<TSource, TDestination, TProperty>(
            string propertyName,
            Func<TSource, TProperty> propertyMapper)
        {
            _customPropertyMappings[(typeof(TSource), typeof(TDestination), propertyName)] = propertyMapper;
        }

        public void IgnoreProperty<TSource, TDestination>(string propertyName)
        {
            _ignoredProperties.Add((typeof(TSource), typeof(TDestination), propertyName));
        }

        public bool HasCustomMapping<TSource, TDestination>()
        {
            return _customMappings.ContainsKey((typeof(TSource), typeof(TDestination)));
        }

        public Func<TSource, TDestination>? GetCustomMapping<TSource, TDestination>()
        {
            if (_customMappings.TryGetValue((typeof(TSource), typeof(TDestination)), out var mapping))
            {
                return (Func<TSource, TDestination>)mapping;
            }

            return null;
        }

        public bool HasCustomPropertyMapping<TSource, TDestination>(string propertyName)
        {
            return _customPropertyMappings.ContainsKey((typeof(TSource), typeof(TDestination), propertyName));
        }

        public Func<TSource, TProperty>? GetCustomPropertyMapping<TSource, TDestination, TProperty>(string propertyName)
        {
            if (_customPropertyMappings.TryGetValue((typeof(TSource), typeof(TDestination), propertyName),
                    out var mapping))
            {
                return (Func<TSource, TProperty>)mapping;
            }

            return null;
        }

        public bool IsPropertyIgnored<TSource, TDestination>(string propertyName)
        {
            return _ignoredProperties.Contains((typeof(TSource), typeof(TDestination), propertyName));
        }

        /// <summary>
        /// Builder for configuring property mappings and ignore rules between source and destination types.
        /// </summary>
        public class MappingBuilder<TSource, TDestination>
        {
            private readonly MapperConfiguration _config;
            // Tracks if any custom mapping, member mapping, or ignore rule has been configured
            private bool _hasCustomMappingOrMemberConfig;

            /// <summary>
            /// Initializes a new instance of the <see cref="MappingBuilder{TSource, TDestination}"/> class.
            /// </summary>
            /// <param name="config">The parent MapperConfiguration.</param>
            public MappingBuilder(MapperConfiguration config)
            {
                _config = config;
            }

            /// <summary>
            /// Configures a mapping for a destination property using a strongly-typed lambda expression.
            /// </summary>
            /// <typeparam name="TProperty">The property type.</typeparam>
            /// <param name="destinationProperty">Expression selecting the destination property.</param>
            /// <param name="sourceExpression">Function mapping from source to property value.</param>
            /// <returns>The current builder instance.</returns>
            public MappingBuilder<TSource, TDestination> ForMember<TProperty>(
                Expression<Func<TDestination, TProperty>> destinationProperty,
                Func<TSource, TProperty> sourceExpression)
            {
                var propertyName = GetPropertyName(destinationProperty);
                _config.AddPropertyMapping<TSource, TDestination, TProperty>(propertyName, sourceExpression);
                _hasCustomMappingOrMemberConfig = true;
                return this;
            }

            /// <summary>
            /// Configures a mapping for a destination property using its name (string-based, for backward compatibility).
            /// </summary>
            /// <typeparam name="TProperty">The property type.</typeparam>
            /// <param name="destinationProperty">The name of the destination property.</param>
            /// <param name="sourceExpression">Function mapping from source to property value.</param>
            /// <returns>The current builder instance.</returns>
            public MappingBuilder<TSource, TDestination> ForMember<TProperty>(
                string destinationProperty,
                Func<TSource, TProperty> sourceExpression)
            {
                _config.AddPropertyMapping<TSource, TDestination, TProperty>(destinationProperty, sourceExpression);
                _hasCustomMappingOrMemberConfig = true;
                return this;
            }

            /// <summary>
            /// Configures an ignore rule for a destination property using a strongly-typed lambda expression.
            /// </summary>
            /// <typeparam name="TProperty">The property type.</typeparam>
            /// <param name="destinationProperty">Expression selecting the destination property.</param>
            /// <returns>The current builder instance.</returns>
            public MappingBuilder<TSource, TDestination> Ignore<TProperty>(
                Expression<Func<TDestination, TProperty>> destinationProperty)
            {
                var propertyName = GetPropertyName(destinationProperty);
                _config.IgnoreProperty<TSource, TDestination>(propertyName);
                _hasCustomMappingOrMemberConfig = true;
                return this;
            }

            /// <summary>
            /// Configures an ignore rule for a destination property using its name (string-based, for backward compatibility).
            /// </summary>
            /// <param name="propertyName">The name of the destination property to ignore.</param>
            /// <returns>The current builder instance.</returns>
            public MappingBuilder<TSource, TDestination> Ignore(string propertyName)
            {
                _config.IgnoreProperty<TSource, TDestination>(propertyName);
                _hasCustomMappingOrMemberConfig = true;
                return this;
            }

            /// <summary>
            /// Registers a custom mapping using a compiled expression.
            /// </summary>
            /// <param name="customMapperExpr">The mapping expression.</param>
            /// <returns>The current builder instance.</returns>
            public MappingBuilder<TSource, TDestination> WithCustomMapping(Expression<Func<TSource, TDestination>> customMapperExpr)
            {
                // AOT-safe: Expression.Compile is not supported under NativeAOT. Use delegate overload instead.
                throw new NotSupportedException("Expression-based custom mappings are not supported in AOT. Use the Func<TSource,TDestination> overload.");
            }

            /// <summary>
            /// Registers a custom mapping using a delegate.
            /// </summary>
            /// <param name="customMapper">The mapping delegate.</param>
            /// <returns>The current builder instance.</returns>
            public MappingBuilder<TSource, TDestination> WithCustomMapping(Func<TSource, TDestination> customMapper)
            {
                _config.AddCustomMapping(customMapper);
                _hasCustomMappingOrMemberConfig = true;
                return this;
            }

            /// <summary>
            /// Prevents reversing the mapping if custom mapping, member mapping, or ignore rules are present.
            /// </summary>
            public void Reverse()
            {
                if (_hasCustomMappingOrMemberConfig)
                {
                    throw new InvalidOperationException("Reverse is not allowed when WithCustomMapping, ForMember, or Ignore have been used in this mapping configuration."+
                                                        " Define a separate mapping for the reverse direction if needed.");
                }
                _config._mappings.Add((typeof(TDestination), typeof(TSource)));
            }

            private string GetPropertyName<TProperty>(Expression<Func<TDestination, TProperty>> propertyExpression)
            {
                if (propertyExpression.Body is MemberExpression memberExpression)
                {
                    return memberExpression.Member.Name;
                }

                throw new ArgumentException("Expression must be a property access", nameof(propertyExpression));
            }
        }

        // These properties are part of the public API and are read by the source generator
        // via syntax/semantic analysis at compile time
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public bool EnableProjectionFunctions { get; set; }
        
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public bool UseSwitchDispatcher { get; set; } = true;
        
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public bool ThrowIfPropertyMissing { get; set; }
    }
}