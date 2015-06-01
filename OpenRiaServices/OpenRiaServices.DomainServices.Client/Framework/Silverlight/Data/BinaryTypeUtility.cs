using System;
using System.Diagnostics;
using System.Reflection;

namespace OpenRiaServices.DomainServices
{
    internal class BinaryTypeUtility
    {
        private const string BinaryTypeFullName = "System.Data.Linq.Binary";
        private static Type binaryType;

        /// <summary>
        /// Returns whether the <paramref name="type"/> <c>is System.Data.Linq.Binary</c>.
        /// </summary>
        /// <remarks>
        /// We test Binary by Type Name so our client framework assembly can avoid taking an
        /// assembly reference to <c>System.Data.Linq</c>. If a type is determined to be
        /// binary, that type will be used to check reference equality for all subsequent
        /// invocations.
        /// </remarks>
        /// <param name="type">The type to check</param>
        /// <returns>Whether the <paramref name="type"/> is binary</returns>
        internal static bool IsTypeBinary(Type type)
        {
            if (type == null)
            {
                return false;
            }

            if (binaryType == type)
            {
                return true;
            }

            if (string.Compare(type.FullName, BinaryTypeFullName, StringComparison.Ordinal) == 0)
            {
                binaryType = type;
                return true;
            }
            return false;
        }

        private static MethodInfo binaryToArrayMethod;

        /// <summary>
        /// Returns the <paramref name="binary"/> converted to a <c>byte[]</c>.
        /// </summary>
        /// <param name="binary">The binary to convert</param>
        /// <returns>The byte[] form of the binary</returns>
        internal static byte[] GetByteArrayFromBinary(object binary)
        {
            Debug.Assert(binary != null, "The binary should never be null.");
            Debug.Assert(binaryType != null, "IsTypeBinary must return true for a type before this can be called.");

            // binary.ToArray();
            if (binaryToArrayMethod == null)
            {
                binaryToArrayMethod =
                    binaryType.GetMethod("ToArray");
            }
            return (byte[])binaryToArrayMethod.Invoke(binary, null);
        }

        private static ConstructorInfo binaryConstructor;

        /// <summary>
        /// Returns the <paramref name="bytes"/> converted to a <c>System.Data.Linq.Binary</c>.
        /// </summary>
        /// <param name="bytes">The bytes to convert</param>
        /// <returns>The binary form of the bytes</returns>
        internal static object GetBinaryFromByteArray(byte[] bytes)
        {
            Debug.Assert(bytes != null, "The byte array should never be null.");
            Debug.Assert(binaryType != null, "IsTypeBinary must return true for a type before this can be called.");

            // new Binary(bytes);
            if (binaryConstructor == null)
            {
                binaryConstructor =
                    binaryType.GetConstructor(new Type[] { typeof(byte[]) });
            }

            return binaryConstructor.Invoke(new object[] { bytes });
        }
    }
}
