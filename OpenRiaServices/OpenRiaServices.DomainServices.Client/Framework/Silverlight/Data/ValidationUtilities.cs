using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using OpenRiaServices.DomainServices;
#if SERVERFX
using DataResource = OpenRiaServices.DomainServices.Server.Resource;
#else
using DataResource = OpenRiaServices.DomainServices.Client.Resource;

#endif

#if SERVERFX
namespace OpenRiaServices.DomainServices.Server
#else
namespace OpenRiaServices.DomainServices.Client
#endif
{
    internal static class ValidationUtilities
    {
        /// <summary>
        /// Creates a new <see cref="ValidationContext"/> for the current object instance.
        /// </summary>
        /// <param name="instance">The object instance being validated.</param>
        /// <param name="parentContext">Optional context to inherit from.  May be null.</param>
        /// <returns>A new validation context.</returns>
        internal static ValidationContext CreateValidationContext(object instance, ValidationContext parentContext)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            ValidationContext context = new ValidationContext(instance, parentContext, parentContext != null ? parentContext.Items : null);
            return context;
        }

        /// <summary>
        /// Internal helper method for getting a method from an object instance that matches
        /// the specified parameters.
        /// </summary>
        /// <param name="instance">Object instance on which the method will be called</param>
        /// <param name="methodName">The name of the method to be called</param>
        /// <param name="parameters">The parameter values to be passed to the method</param>
        /// <returns>A <see cref="MethodInfo"/> from an object instance that matches
        /// the specified parameters.</returns>
        internal static MethodInfo GetMethod(object instance, string methodName, object[] parameters)
        {
            Type instanceType = instance.GetType();
            MethodInfo[] candidates = instanceType.GetMethods()
                .Where(m => m.Name == methodName && IsBindable(m, parameters))
                .ToArray();

            if (candidates.Length == 0)
            {
                int parameterLength = (parameters == null) ? 0 : parameters.Length;
                if (parameterLength == 0)
                {
                    throw new MissingMethodException(string.Format(CultureInfo.CurrentCulture, DataResource.ValidationUtilities_MethodNotFound_ZeroParams, instanceType, methodName));
                }
                else
                {
                    // convert parameter types into a string of this format e.g. ('string', null, 'int')
                    string[] parameterTypes = parameters.Select(p => ((p == null) ? "null" : string.Format(CultureInfo.InvariantCulture, "'{0}'", p.GetType().ToString()))).ToArray();
                    throw new MissingMethodException(string.Format(CultureInfo.CurrentCulture, DataResource.ValidationUtilities_MethodNotFound, instanceType, methodName, parameterLength, string.Join(", ", parameterTypes)));
                }
            }

            if (candidates.Length > 1)
            {
                throw new AmbiguousMatchException(string.Format(CultureInfo.CurrentCulture, DataResource.ValidationUtilities_AmbiguousMatch, methodName));
            }
            return candidates[0];
        }

        internal static bool IsBindable(Type[] parameterTypes, object[] parameters)
        {
            int parameterLength = (parameters == null) ? 0 : parameters.Length;
            if (parameterTypes.Length != parameterLength)
            {
                return false;
            }

            for (int i = 0; i < parameterLength; i++)
            {
                if (parameters[i] == null)
                {
                    if (!TypeUtility.IsNullableType(parameterTypes[i]) && parameterTypes[i].IsValueType)
                    {
                        return false;
                    }
                }
                else if (!parameterTypes[i].IsAssignableFrom(parameters[i].GetType()))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks whether the specified set of parameters can be passed to the specified method.
        /// </summary>
        /// <param name="method">The method to validate the set of parameters against.</param>
        /// <param name="parameters">The set of parameters to check.</param>
        /// <returns><c>true</c> if the set of parameters can be passed to the specified method.</returns>
        internal static bool IsBindable(MethodInfo method, object[] parameters)
        {
            return IsBindable(method.GetParameters().Select(p => p.ParameterType).ToArray(), parameters);
        }

        /// <summary>
        /// Appends the specified memberPath to all member names in the validation results.
        /// </summary>
        /// <param name="validationResults">The validation results</param>
        /// <param name="memberPath">The member path to append</param>
        /// <returns>The updated validation results</returns>
        internal static IEnumerable<ValidationResult> ApplyMemberPath(IEnumerable<ValidationResult> validationResults, string memberPath)
        {
            if (string.IsNullOrEmpty(memberPath))
            {
                // no path to apply
                return validationResults;
            }

            return validationResults.Select(p => ApplyMemberPath(p, memberPath));
        }

        /// <summary>
        /// Appends the specified memberPath to all member names in the validation result.
        /// </summary>
        /// <param name="validationResult">The validation result</param>
        /// <param name="memberPath">The member path to append</param>
        /// <returns>The updated validation result</returns>
        internal static ValidationResult ApplyMemberPath(ValidationResult validationResult, string memberPath)
        {
            if (string.IsNullOrEmpty(memberPath))
            {
                // no path to apply
                return validationResult;
            }

            List<string> memberNames = new List<string>();
            foreach (string currMemberName in validationResult.MemberNames)
            {
                string transformedMemberName = memberPath + "." + currMemberName;
                memberNames.Add(transformedMemberName);
            }

            if (memberNames.Count == 0)
            {
                // If this is a type level validation error, there won't be any member names
                // so we need to add the member path. We use a terminating '.' to differentiate
                // between Type level and Property level errors. Otherwise, for an error with a
                // member name like "ContactInfo.Address" we wouldnt be able to determine if
                // the error applies to the Contact.Address property or the Contact.Address instance.
                memberNames.Add(memberPath + ".");
            }

            return new ValidationResult(validationResult.ErrorMessage, memberNames);
        }

        /// <summary>
        /// Validate the specified object an any complex members or collections recursively.
        /// </summary>
        /// <param name="instance">The instance to validate.</param>
        /// <param name="validationContext">The validation context</param>
        /// <param name="validationResults">The validation results</param>
        /// <returns>True if the object is valid, false otherwise.</returns>
        public static bool TryValidateObject(object instance, ValidationContext validationContext, List<ValidationResult> validationResults)
        {
            return ValidateObjectRecursive(instance, string.Empty, validationContext, validationResults);
        }

        /// <summary>
        /// This method recursively validates an object, first validating all properties, then
        /// validating the type. This method implements the classic Try pattern. However it serves
        /// as code sharing for the validation pattern where an exception is thrown on first error.
        /// </summary>
        /// <param name="instance">The object to validate.</param>
        /// <param name="memberPath">The dotted path of the member.</param>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="validationResults">The collection in which the validation results will be
        /// stored. The collection can be <c>null</c>.</param>
        /// <returns><c>True</c> if the object was successfully validated with no errors.</returns>
        /// <exception cref="ValidationException">When <paramref name="validationResults"/> is
        /// <c>null</c> and the object has a validation error.</exception>
        private static bool ValidateObjectRecursive(object instance, string memberPath,
            ValidationContext validationContext, List<ValidationResult> validationResults)
        {
            MetaType metaType = MetaType.GetMetaType(instance.GetType());
            if (!metaType.RequiresValidation)
            {
                return true;
            }

            // First validate all properties
            bool hasValidationErrors = false;
            foreach (MetaMember metaMember in metaType.Members.Where(m => m.RequiresValidation || m.IsComplex))
            {
                ValidationContext propertyValidationContext = ValidationUtilities.CreateValidationContext(instance, validationContext);
                propertyValidationContext.MemberName = metaMember.Member.Name;

                // Form the current member path, appending the current
                // member name if it is complex.
                string currMemberPath = memberPath;
                if (metaMember.IsComplex)
                {
                    if (currMemberPath.Length > 0)
                    {
                        currMemberPath += ".";
                    }
                    currMemberPath += metaMember.Member.Name;
                }

                object value = metaMember.GetValue(instance);

                // first validate the property itself
                if (metaMember.RequiresValidation)
                {
                    hasValidationErrors |= !ValidationUtilities.ValidateProperty(value, propertyValidationContext, validationResults, currMemberPath);
                }

                // for complex members, in addition to property level validation we need to
                // do deep validation recursively
                if (value != null && metaMember.IsComplex)
                {
                    if (!metaMember.IsCollection)
                    {
                        hasValidationErrors |= !ValidateObjectRecursive(value, currMemberPath, validationContext, validationResults);
                    }
                    else
                    {
                        hasValidationErrors |= !ValidateComplexCollection((IEnumerable)value, currMemberPath, validationContext, validationResults);
                    }
                }
            }

            // Only proceed to Type level validation if there are no property validation errors
            if (hasValidationErrors)
            {
                return false;
            }

            // Next perform Type level validation without validating properties, since we've already validated all properties.
            // Note that we can't use Validator.ValidateObject specifying 'validateAllProperties' since even when specifying false,
            // that API will validate RequiredAttribute.
            ValidationContext context = ValidationUtilities.CreateValidationContext(instance, validationContext);
            if (metaType.ValidationAttributes.Any())
            {
                hasValidationErrors |= !ValidationUtilities.ValidateValue(instance, context, validationResults, metaType.ValidationAttributes, memberPath);
            }

#if !SILVERLIGHT
            // Only proceed to IValidatableObject validation if there are no errors
            if (hasValidationErrors)
            {
                return false;
            }

            // Test for IValidatableObject implementation and run the validation if applicable
            // Note : this interface doesn't exist in Silverlight
            IValidatableObject validatable = instance as IValidatableObject;
            if (validatable != null)
            {
                IEnumerable<ValidationResult> results = validatable.Validate(context);

                if (!string.IsNullOrEmpty(memberPath))
                {
                    results = ValidationUtilities.ApplyMemberPath(results, memberPath);
                }

                foreach (ValidationResult result in results.Where(r => r != ValidationResult.Success))
                {
                    validationResults.Add(result);
                    hasValidationErrors = true;
                }
            }
#endif

            return !hasValidationErrors;
        }

        /// <summary>
        /// This method deeply validates all objects in a collection. This method implements the
        /// classic Try pattern. However it serves as code sharing for the validation pattern where
        /// an exception is thrown on first error.
        /// </summary>
        /// <param name="elements">The enumerable containing the objects to validate.</param>
        /// <param name="memberPath">The dotted path of the member.</param>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="validationResults">The collection in which the validation results will be
        /// stored. The collection can be <c>null</c>.</param>
        /// <returns><c>True</c> if the object was successfully validated with no errors.</returns>
        /// <exception cref="ValidationException">When <paramref name="validationResults"/> is
        /// <c>null</c> and the object has a validation error.</exception>
        private static bool ValidateComplexCollection(IEnumerable elements, string memberPath,
            ValidationContext validationContext, List<ValidationResult> validationResults)
        {
            bool hasValidationErrors = false;

            foreach (var element in elements)
            {
                if (element == null)
                {
                    continue;
                }

                hasValidationErrors |= !ValidateObjectRecursive(element, memberPath + "()", validationContext, validationResults);
            }

            return !hasValidationErrors;
        }

        private static bool ValidateProperty(object value, ValidationContext validationContext,
            List<ValidationResult> validationResults, string memberPath)
        {
            if (validationResults == null)
            {
                Validator.ValidateProperty(value, validationContext);
            }
            else
            {
                List<ValidationResult> currentResults = new List<ValidationResult>();
                if (!Validator.TryValidateProperty(value, validationContext, currentResults))
                {
                    // transform the validation results by applying the member path to the results
                    if (memberPath.Length > 0)
                    {
                        currentResults = ValidationUtilities.ApplyMemberPath(currentResults, memberPath).ToList();
                    }
                    validationResults.AddRange(currentResults);
                    return false;
                }
            }

            return true;
        }

        private static bool ValidateValue(object value, ValidationContext validationContext,
            List<ValidationResult> validationResults, IEnumerable<ValidationAttribute> validationAttributes,
            string memberPath)
        {
            if (validationResults == null)
            {
                Validator.ValidateValue(value, validationContext, validationAttributes);
            }
            else
            {
                // todo, needs to be array aware
                List<ValidationResult> currentResults = new List<ValidationResult>();
                if (!Validator.TryValidateValue(value, validationContext, currentResults, validationAttributes))
                {
                    // transform the validation results by applying the member path to the results
                    if (!string.IsNullOrEmpty(memberPath))
                    {
                        currentResults = ValidationUtilities.ApplyMemberPath(currentResults, memberPath).ToList();
                    }
                    validationResults.AddRange(currentResults);
                    return false;
                }
            }
            return true;
        }

#if SERVERFX
        internal static bool TryValidateMethodCall(DomainOperationEntry operationEntry, ValidationContext validationContext, object[] parameters, List<ValidationResult> validationResults)
        {
            bool breakOnFirstError = validationResults == null;

            ValidationContext methodContext = CreateValidationContext(validationContext.ObjectInstance, validationContext);
            methodContext.MemberName = operationEntry.Name;

            DisplayAttribute display = (DisplayAttribute)operationEntry.Attributes[typeof(DisplayAttribute)];

            if (display != null)
            {
                methodContext.DisplayName = display.GetName();
            }

            string methodPath = string.Empty;
            if (operationEntry.Operation == DomainOperation.Custom)
            {
                methodPath = operationEntry.Name + ".";
            }

            IEnumerable<ValidationAttribute> validationAttributes = operationEntry.Attributes.OfType<ValidationAttribute>();
            bool success = Validator.TryValidateValue(validationContext.ObjectInstance, methodContext, validationResults, validationAttributes);

            if (!breakOnFirstError || success)
            {
                for (int paramIndex = 0; paramIndex < operationEntry.Parameters.Count; paramIndex++)
                {
                    DomainOperationParameter methodParameter = operationEntry.Parameters[paramIndex];
                    object value = (parameters.Length > paramIndex ? parameters[paramIndex] : null);

                    ValidationContext parameterContext = ValidationUtilities.CreateValidationContext(validationContext.ObjectInstance, validationContext);
                    parameterContext.MemberName = methodParameter.Name;

                    string paramName = methodParameter.Name;

                    AttributeCollection parameterAttributes = methodParameter.Attributes;
                    display = (DisplayAttribute)parameterAttributes[typeof(DisplayAttribute)];

                    if (display != null)
                    {
                        paramName = display.GetName();
                    }

                    parameterContext.DisplayName = paramName;

                    string parameterPath = string.Empty;
                    if (!string.IsNullOrEmpty(methodPath) && paramIndex > 0)
                    {
                        parameterPath = methodPath + methodParameter.Name;
                    }

                    IEnumerable<ValidationAttribute> parameterValidationAttributes = parameterAttributes.OfType<ValidationAttribute>();
                    bool parameterSuccess = ValidationUtilities.ValidateValue(value, parameterContext, validationResults, parameterValidationAttributes,
                        ValidationUtilities.NormalizeMemberPath(parameterPath, methodParameter.ParameterType));
                    
                    // Custom methods run deep validation as well as parameter validation.
                    // If parameter validation has already failed, stop further validation.
                    if (parameterSuccess && operationEntry.Operation == DomainOperation.Custom && value != null)
                    {
                        Type parameterType = methodParameter.ParameterType;

                        if (TypeUtility.IsComplexType(parameterType))
                        {
                            parameterSuccess = ValidationUtilities.ValidateObjectRecursive(value, parameterPath, parameterContext, validationResults);
                        }
                        else if (TypeUtility.IsComplexTypeCollection(parameterType))
                        {
                            parameterSuccess = ValidationUtilities.ValidateComplexCollection(value as IEnumerable, parameterPath, parameterContext, validationResults);
                        }
                    }

                    success &= parameterSuccess;

                    if (breakOnFirstError && !success)
                    {
                        break;
                    }
                }
            }

            return success;
        }
#else //!SERVERFX
        /// <summary>
        /// Removes the specified member name prefix from the member names in the validation 
        /// results specified.
        /// </summary>
        /// <param name="validationResults">The validation results</param>
        /// <param name="memberName">The member name to remove</param>
        /// <returns>The updated validation results</returns>
        internal static IEnumerable<ValidationResult> RemoveMemberPrefix(IEnumerable<ValidationResult> validationResults, string memberName)
        {
            if (string.IsNullOrEmpty(memberName))
            {
                // no path to remove
                return validationResults;
            }
            List<ValidationResult> transformedResults = new List<ValidationResult>();
            foreach (ValidationResult result in validationResults)
            {
                List<string> memberNames = new List<string>();
                foreach (string currMemberName in result.MemberNames)
                {
                    if (!string.IsNullOrEmpty(currMemberName))
                    {
                        string transformedMemberName = currMemberName;
                        string searchString = memberName + ".";
                        if (transformedMemberName.StartsWith(searchString, StringComparison.Ordinal))
                        {
                            transformedMemberName = transformedMemberName.Substring(searchString.Length);
                        }

                        if (transformedMemberName.Length > 0)
                        {
                            memberNames.Add(transformedMemberName);
                        }
                    }
                }
                transformedResults.Add(new ValidationResult(result.ErrorMessage, memberNames));
            }

            return transformedResults;
        }

        /// <summary>
        /// This method recursively applies the specified validation errors to this instance
        /// and any nested instances (based on member paths specified in the validation results).
        /// Since this method correctly handles complex types, this method should be used rather
        /// than directly applying results to the collection.
        /// </summary>
        /// <param name="instance">The target entity or complex type to apply the errors to.</param>
        /// <param name="validationResults">The validation results to apply.</param>
        internal static void ApplyValidationErrors(object instance, IEnumerable<ValidationResult> validationResults)
        {
            // first apply the errors to this instance
            Entity entityInstance = instance as Entity;
            if (entityInstance != null)
            {
                entityInstance.ValidationResultCollection.ReplaceErrors(validationResults);
            }
            else
            {
                ((ComplexObject)instance).ValidationResultCollection.ReplaceErrors(validationResults);
            }

            // next enumerate all complex members and apply nested validation results
            MetaType metaType = MetaType.GetMetaType(instance.GetType());
            foreach (MetaMember complexMember in metaType.DataMembers.Where(p => p.IsComplex && !p.IsCollection))
            {
                // filter the results to only those with a member name that begins with a dotted path
                // starting at the current complex member
                IEnumerable<ValidationResult> results = validationResults.Where(p => p.MemberNames.Any(q => !string.IsNullOrEmpty(q) && q.StartsWith(complexMember.Member.Name + ".", StringComparison.Ordinal)));
                
                ComplexObject complexObject = (ComplexObject)complexMember.GetValue(instance);
                if (complexObject != null)
                {
                    results = ValidationUtilities.RemoveMemberPrefix(results, complexMember.Member.Name);
                    ApplyValidationErrors(complexObject, results);
                }
            }
        }

        /// <summary>
        /// Determines whether it is valid to call the specified method.
        /// </summary>
        /// <remarks>
        /// This method evaluates all <see cref="ValidationAttribute"/>s associated with the method and its parameters.  Any failure returns <c>false</c>.
        /// </remarks>
        /// <param name="methodName">The name of the method to be called</param>
        /// <param name="validationContext">Describes the method being tested</param>
        /// <param name="parameters">The parameter values to be passed to the method.  They will be validated.</param>
        /// <param name="validationResults">Optional collection to receive validation results for failed validations.</param>
        /// <returns><c>true</c> if the method is valid.</returns>
        internal static bool TryValidateCustomUpdateMethodCall(string methodName, ValidationContext validationContext, object[] parameters, List<ValidationResult> validationResults)
        {
            return ValidationUtilities.ValidateMethodCall(methodName, validationContext, parameters, validationResults, true);
        }

        internal static bool MethodRequiresValidation(MethodInfo method)
        {
            bool requiresValidation = method.GetCustomAttributes(true).OfType<ValidationAttribute>().Any();
            if (requiresValidation)
            {
                return true;
            }

            foreach (ParameterInfo paramInfo in method.GetParameters())
            {
                if (paramInfo.GetCustomAttributes(true).OfType<ValidationAttribute>().Any())
                {
                    return true;
                }
            }

            return false;
        }

        internal static void ValidateCustomUpdateMethodCall(string methodName, ValidationContext validationContext, object[] parameters)
        {
            ValidationUtilities.ValidateMethodCall(methodName, validationContext, parameters, null, true);
        }

        /// <summary>
        /// Determines whether it is valid to call the specified method.
        /// </summary>
        /// <remarks>This method evaluates all the <see cref="ValidationAttribute"/>s associated
        /// with the method and its parameters. It does not do deep validation of parameter
        /// instances. Any failure will cause the exception to be thrown.
        /// </remarks>
        /// <param name="methodName">The name of the method to be called</param>
        /// <param name="validationContext">Describes the method being called.</param>
        /// <param name="parameters">The parameter values to be passed to the method.  They will be
        /// validated.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="methodName"/> or
        /// <paramref name="validationContext"/> is null.</exception>
        /// <exception cref="ValidationException"> When it is not valid to call the specified
        /// method.</exception>
        internal static void ValidateMethodCall(string methodName, ValidationContext validationContext, object[] parameters)
        {
            ValidationUtilities.ValidateMethodCall(methodName, validationContext, parameters, null, false);
        }

        internal static bool ValidateMethodCall(string methodName,
            ValidationContext validationContext, object[] parameters,
            List<ValidationResult> validationResults, bool performTypeValidation)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                throw new ArgumentNullException("methodName");
            }

            if (validationContext == null)
            {
                throw new ArgumentNullException("validationContext");
            }

            if (validationContext.ObjectInstance == null)
            {
                throw new ArgumentException(DataResource.ValidationUtilities_ContextInstance_CannotBeNull, "validationContext");
            }

            MethodInfo method = GetMethod(validationContext.ObjectInstance, methodName, parameters);
            ValidationContext methodContext = CreateValidationContext(validationContext.ObjectInstance, validationContext);
            methodContext.MemberName = method.Name;

            DisplayAttribute display = GetDisplayAttribute(method);

            if (display != null)
            {
                methodContext.DisplayName = display.GetName();
            }

            string memberPath = string.Empty;
            if (performTypeValidation)
            {
                memberPath = method.Name + ".";
            }

            IEnumerable<ValidationAttribute> validationAttributes = method.GetCustomAttributes(typeof(ValidationAttribute), true).Cast<ValidationAttribute>();
            bool success = ValidationUtilities.ValidateValue(validationContext.ObjectInstance, methodContext, validationResults, validationAttributes, string.Empty);

            ParameterInfo[] parameterInfos = method.GetParameters();

            for (int paramIndex = 0; paramIndex < parameterInfos.Length; paramIndex++)
            {
                ParameterInfo methodParameter = parameterInfos[paramIndex];
                object value = (parameters.Length > paramIndex ? parameters[paramIndex] : null);

                ValidationContext parameterContext = ValidationUtilities.CreateValidationContext(validationContext.ObjectInstance, validationContext);
                parameterContext.MemberName = methodParameter.Name;

                string paramName = methodParameter.Name;

                display = GetDisplayAttribute(methodParameter);

                if (display != null)
                {
                    paramName = display.GetName();
                }

                parameterContext.DisplayName = paramName;

                string parameterPath = memberPath;
                if (performTypeValidation)
                {
                    parameterPath += methodParameter.Name;
                }

                IEnumerable<ValidationAttribute> parameterAttributes = methodParameter.GetCustomAttributes(typeof(ValidationAttribute), false).Cast<ValidationAttribute>();
                bool parameterSuccess = ValidationUtilities.ValidateValue(value, parameterContext, validationResults,
                    parameterAttributes, ValidationUtilities.NormalizeMemberPath(parameterPath, methodParameter.ParameterType));
               
                if (parameterSuccess && performTypeValidation && value != null)
                {
                    Type parameterType = methodParameter.ParameterType;

                    // If the user expects an exception we perform shallow validation only.
                    if (validationResults == null)
                    {
                        if (TypeUtility.IsComplexType(parameterType))
                        {
                            ValidationContext context = ValidationUtilities.CreateValidationContext(value, parameterContext);
                            context.DisplayName = paramName;
                            Validator.ValidateObject(value, context, /*validateAllProperties*/ true);
                        }
                    }
                    // Else we are able to report fully recursive validation errors to the user, perform deep validation.
                    else
                    {
                        if (TypeUtility.IsComplexType(parameterType))
                        {
                            parameterSuccess = ValidationUtilities.ValidateObjectRecursive(value, parameterPath, validationContext, validationResults);
                        }
                        else if (TypeUtility.IsComplexTypeCollection(parameterType))
                        {
                            parameterSuccess = ValidationUtilities.ValidateComplexCollection(value as IEnumerable, parameterPath, validationContext, validationResults);
                        }
                    }
                }

                success &= parameterSuccess;
            }

            return success;
        }

        /// <summary>
        /// Gets the DisplayAttribute that applies to a method, parameter, property, etc.
        /// </summary>
        /// <param name="member">A <see cref="ICustomAttributeProvider"/> member to query for <see cref="DisplayAttribute"/>s.</param>
        /// <returns>A <see cref="DisplayAttribute"/> found or <c>null</c> if none is found</returns>
        private static DisplayAttribute GetDisplayAttribute(ICustomAttributeProvider member)
        {
            return member.GetCustomAttributes(typeof(DisplayAttribute), true).SingleOrDefault() as DisplayAttribute;
        }
#endif
        /// <summary>
        /// Adds the collection token '()' to the <paramref name="memberPath"/> if the
        /// <paramref name="memberType"/> is an IEnumerable.
        /// </summary>
        /// <param name="memberPath">The path of the member.</param>
        /// <param name="memberType">The type of the member.</param>
        /// <returns>The correct member representation.</returns>
        private static string NormalizeMemberPath(string memberPath, Type memberType)
        {
            if (string.IsNullOrEmpty(memberPath))
            {
                return memberPath;
            }

            if (typeof(IEnumerable).IsAssignableFrom(memberType))
            {
                Debug.Assert(!memberPath.EndsWith("()", StringComparison.Ordinal), "The memberPath already contains a () suffix.");
                return memberPath + "()";
            }

            return memberPath;
        }
    }

#if !SERVERFX
    internal class ValidationResultEqualityComparer : EqualityComparer<ValidationResult>
    {
        public override bool Equals(ValidationResult left, ValidationResult right)
        {
            if (left.ErrorMessage.Equals(right.ErrorMessage, StringComparison.Ordinal) && left.MemberNames.SequenceEqual(right.MemberNames))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode(ValidationResult validationResult)
        {
            int hashCode = validationResult.ErrorMessage.GetHashCode();
            foreach (string memberName in validationResult.MemberNames)
            {
                hashCode ^= memberName.GetHashCode();
            }
            return hashCode;
        }
    }
#endif //!SERVERFX
}
