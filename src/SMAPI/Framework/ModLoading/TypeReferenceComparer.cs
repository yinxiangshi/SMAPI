using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mono.Cecil;

namespace StardewModdingAPI.Framework.ModLoading
{
    /// <summary>Performs heuristic equality checks for <see cref="TypeReference"/> instances.</summary>
    internal class TypeReferenceComparer : IEqualityComparer<TypeReference>
    {
        /*********
        ** Properties
        *********/
        /// <summary>A pattern matching type name substrings to strip for display.</summary>
        private readonly Regex StripTypeNamePattern = new Regex(@"`\d+(?=<)", RegexOptions.Compiled);

        private List<char> symbolBoundaries = new List<char> { '<', '>', ',' };


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the specified objects are equal.</summary>
        /// <param name="a">The first object to compare.</param>
        /// <param name="b">The second object to compare.</param>
        public bool Equals(TypeReference a, TypeReference b)
        {
            string typeA = this.GetComparableTypeID(a);
            string typeB = this.GetComparableTypeID(b);

            string placeholderType = "", actualType = "";

            if (this.HasPlaceholder(typeA))
            {
                placeholderType = typeA;
                actualType = typeB;
            }
            else if (this.HasPlaceholder(typeB))
            {
                placeholderType = typeB;
                actualType = typeA;
            }
            else
                return typeA == typeB;

            return this.PlaceholderTypeValidates(placeholderType, actualType);
        }

        /// <summary>Get a hash code for the specified object.</summary>
        /// <param name="obj">The object for which a hash code is to be returned.</param>
        /// <exception cref="T:System.ArgumentNullException">The object type is a reference type and <paramref name="obj" /> is null.</exception>
        public int GetHashCode(TypeReference obj)
        {
            return obj.GetHashCode();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a unique string representation of a type.</summary>
        /// <param name="type">The type reference.</param>
        private string GetComparableTypeID(TypeReference type)
        {
            return this.StripTypeNamePattern.Replace(type.FullName, "");
        }

        /// <summary>Determine whether this type ID has a placeholder such as !0.</summary>
        /// <param name="typeID">The type to check.</param>
        /// <returns>true if the type ID contains a placeholder, false if not.</returns>
        private bool HasPlaceholder(string typeID)
        {
            return typeID.Contains("!0");
        }

        /// <summary> returns whether this type ID is a placeholder, i.e., it begins with "!".</summary>
        /// <param name="symbol">The symbol to validate.</param>
        /// <returns>true if the symbol is a placeholder, false if not</returns>
        private bool IsPlaceholder(string symbol)
        {
            return symbol.StartsWith("!");
        }

        /// <summary> Traverses and parses out symbols from a type which does not contain placeholder values.</summary>
        /// <param name="type">The type to traverse.</param>
        /// <param name="typeSymbols">A List in which to store the parsed symbols.</param>
        private void TraverseActualType(string type, List<SymbolLocation> typeSymbols)
        {
            int depth = 0;
            string symbol = "";

            foreach (char c in type)
            {
                if (this.symbolBoundaries.Contains(c))
                {
                    typeSymbols.Add(new SymbolLocation(symbol, depth));
                    symbol = "";
                    switch (c)
                    {
                        case '<':
                            depth++;
                            break;
                        case '>':
                            depth--;
                            break;
                        default:
                            break;
                    }
                }
                else
                    symbol += c;
            }
        }

        /// <summary> Determines whether two symbols in a type ID match, accounting for placeholders such as !0.</summary>
        /// <param name="symbolA">A symbol in a typename which contains placeholders.</param>
        /// <param name="symbolB">A symbol in a typename which does not contain placeholders.</param>
        /// <param name="placeholderMap">A dictionary containing a mapping of placeholders to concrete types.</param>
        /// <returns>true if the symbols match, false if not.</returns>
        private bool SymbolsMatch(SymbolLocation symbolA, SymbolLocation symbolB, Dictionary<string, string> placeholderMap)
        {
            if (symbolA.depth != symbolB.depth)
                return false;

            if (!this.IsPlaceholder(symbolA.symbol))
            {
                return symbolA.symbol == symbolB.symbol;
            }

            if (placeholderMap.ContainsKey(symbolA.symbol))
            {
                return placeholderMap[symbolA.symbol] == symbolB.symbol;
            }

            placeholderMap[symbolA.symbol] = symbolB.symbol;

            return true;
        }

        /// <summary> Determines whether a type which has placeholders correctly resolves to the concrete type provided. </summary>
        /// <param name="type">A type containing placeholders such as !0.</param>
        /// <param name="typeSymbols">The list of symbols extracted from the concrete type.</param>
        /// <returns>true if the type resolves correctly, false if not.</returns>
        private bool PlaceholderTypeResolvesToActualType(string type, List<SymbolLocation> typeSymbols)
        {
            Dictionary<string, string> placeholderMap = new Dictionary<string, string>();

            int depth = 0, symbolCount = 0;
            string symbol = "";

            foreach (char c in type)
            {
                if (this.symbolBoundaries.Contains(c))
                {
                    bool match = this.SymbolsMatch(new SymbolLocation(symbol, depth), typeSymbols[symbolCount], placeholderMap);
                    if (typeSymbols.Count <= symbolCount ||
                        !match)
                        return false;

                    symbolCount++;
                    symbol = "";
                    switch (c)
                    {
                        case '<':
                            depth++;
                            break;
                        case '>':
                            depth--;
                            break;
                        default:
                            break;
                    }
                }
                else
                    symbol += c;
            }

            return true;
        }

        /// <summary>Determines whether a type with placeholders in it matches a type without placeholders.</summary>
        /// <param name="placeholderType">The type with placeholders in it.</param>
        /// <param name="actualType">The type without placeholders.</param>
        /// <returns>true if the placeholder type can resolve to the actual type, false if not.</returns>
        private bool PlaceholderTypeValidates(string placeholderType, string actualType)
        {
            List<SymbolLocation> typeSymbols = new List<SymbolLocation>();

            this.TraverseActualType(actualType, typeSymbols);
            return PlaceholderTypeResolvesToActualType(placeholderType, typeSymbols);
        }



        /*********
        ** Inner classes
        *********/
        protected class SymbolLocation
        {
            public string symbol;
            public int depth;

            public SymbolLocation(string symbol, int depth)
            {
                this.symbol = symbol;
                this.depth = depth;
            }
        }
    }
}
