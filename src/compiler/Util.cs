﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace HappyCspp.Compiler
{
    static class Util
    {
        internal static Exception NewSyntaxNotSupportedException<T>(SeparatedSyntaxList<T> syntaxList)
            where T : SyntaxNode
        {
            return new NotSupportedException();
        }

        internal static Exception NewSyntaxNotSupportedException(SyntaxToken syntaxToken)
        {
            return new NotSupportedException();
        }

        internal static Exception NewSyntaxNotSupportedException(SyntaxNode syntaxNode)
        {
            return new NotSupportedException();
        }

        internal static Exception NewTypeNotFoundException(string typeName)
        {
            return new System.IO.FileNotFoundException();
        }

        internal static Exception NewTokenNotFoundException(string tokenName)
        {
            return new System.IO.FileNotFoundException();
        }

        internal static string RemoveLastChar(this string str)
        {
            return str.Substring(0, str.Length - 1);
        }

        internal static T GetAttributeValue<T>(SeparatedSyntaxList<AttributeArgumentSyntax> attrArgs, int index)
        {
            var expr = attrArgs[index].Expression;

            if (expr is LiteralExpressionSyntax)
            {
                var lt = attrArgs[index].Expression as LiteralExpressionSyntax;
                return (T)lt.Token.Value;
            }
            else if (expr is TypeOfExpressionSyntax)
            {
                var t = expr as TypeOfExpressionSyntax;
                return (T)(object)t.Type;
            }
            else
            {
                // Only literals are supported here
                throw Util.NewSyntaxNotSupportedException(attrArgs[index]);
            }
        }

        internal static void GetAliases(SeparatedSyntaxList<AttributeArgumentSyntax> attrArgs, out string alias, out string altAlias)
        {
            alias = GetAttributeValue<string>(attrArgs, 0);
            if (attrArgs.Count > 1)
            {
                altAlias = GetAttributeValue<string>(attrArgs, 1);
            }
            else
            {
                altAlias = null;
            }
        }

        internal static string GetSymbolAlias(bool preferWideChar, ImmutableArray<AttributeData> attributes)
        {
            foreach (var attr in attributes)
            {
                if (attr.AttributeClass.Name != "AliasAttribute")
                    continue;

                var aliases = attr.ConstructorArguments.ToArray();
                if (preferWideChar && aliases.Length > 1)
                {
                    return (string)aliases[1].Value;
                }
                else
                {
                    return (string)aliases[0].Value;
                }
            }

            return null;
        }


        internal static bool IsAttributeDefined(IEnumerable<AttributeData> attributes, string attributeName)
        {
            foreach (var attr in attributes)
            {
                if (attr.AttributeClass.Name == attributeName)
                    return true;
            }

            return false;
        }

        internal static bool IsAttributeDefined(IEnumerable<AttributeListSyntax> attributes, string attributeShortName)
        {
            foreach (var attrList in attributes)
            {
                foreach (var attr in attrList.Attributes)
                {
                    string name = attr.Name.ToString();
                    if (name == attributeShortName || name == attributeShortName + "Attribute")
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal static void RunCommand(string program, string arguments)
        {
            Console.WriteLine("{0} {1}", program, arguments);

            Process p = Process.Start(program, arguments);
            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                throw new Exception(string.Format("{0} {1} failed with exit code {2}.", program, arguments, p.ExitCode));
            }
        }

        internal static T GetJsonValue<T>(this JObject jObject, params string[] path)
        {
            if (path == null || path.Length == 0)
            {
                return default(T);
            }

            JToken jToken = jObject.Root;

            for(int i = 0; i < path.Length - 1; i++)
            {
                jToken = string.IsNullOrEmpty(path[i]) ? jToken.First() : jToken[path[i]];
                if (jToken == null)
                {
                    return default(T);
                }
            }

            return jToken.Value<T>(path.Last());
        }

        internal static IEnumerable<T> GetJsonValues<T>(this JObject jObject, params string[] path)
        {
            if (path == null || path.Length == 0)
            {
                return null;
            }

            JToken jToken = jObject.Root;

            for(int i = 0; i < path.Length - 1; i++)
            {
                jToken = string.IsNullOrEmpty(path[i]) ? jToken.First() : jToken[path[i]];
                if (jToken == null)
                {
                    return null;
                }
            }

            return jToken.Values<T>(path.Last());
        }
    }
}
