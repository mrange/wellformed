
// ############################################################################
// #                                                                          #
// #        ---==>  T H I S  F I L E  I S   G E N E R A T E D  <==---         #
// #                                                                          #
// # This means that any edits to the .cs file will be lost when its          #
// # regenerated. Changes should instead be applied to the corresponding      #
// # text template file (.tt)                                                 #
// ############################################################################



// ############################################################################
// @@@ INCLUDING: https://raw.github.com/mrange/T4Include/master/Extensions/BasicExtensions.cs
// @@@ INCLUDE_FOUND: ../Common/Array.cs
// @@@ INCLUDE_FOUND: ../Common/Config.cs
// @@@ INCLUDE_FOUND: ../Common/Log.cs
// @@@ INCLUDING: https://raw.github.com/mrange/T4Include/master/Extensions/WpfExtensions.cs
// @@@ INCLUDE_FOUND: ../Common/Array.cs
// @@@ INCLUDE_FOUND: ../Common/Log.cs
// @@@ INCLUDING: https://raw.github.com/mrange/T4Include/master/Common/Array.cs
// @@@ INCLUDING: https://raw.github.com/mrange/T4Include/master/Common/Config.cs
// @@@ INCLUDING: https://raw.github.com/mrange/T4Include/master/Common/Log.cs
// @@@ INCLUDE_FOUND: Config.cs
// @@@ INCLUDE_FOUND: Generated_Log.cs
// @@@ SKIPPING (Already seen): https://raw.github.com/mrange/T4Include/master/Common/Array.cs
// @@@ SKIPPING (Already seen): https://raw.github.com/mrange/T4Include/master/Common/Log.cs
// @@@ SKIPPING (Already seen): https://raw.github.com/mrange/T4Include/master/Common/Config.cs
// @@@ INCLUDING: https://raw.github.com/mrange/T4Include/master/Common/Generated_Log.cs
// ############################################################################
// Certains directives such as #define and // Resharper comments has to be 
// moved to top in order to work properly    
// ############################################################################
// ReSharper disable InconsistentNaming
// ReSharper disable PartialMethodWithSinglePart
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantNameQualifier
// ############################################################################

// ############################################################################
// @@@ BEGIN_INCLUDE: https://raw.github.com/mrange/T4Include/master/Extensions/BasicExtensions.cs
namespace WellFormed2.WPF.Internal
{
    // ----------------------------------------------------------------------------------------------
    // Copyright (c) Mårten Rånge.
    // ----------------------------------------------------------------------------------------------
    // This source code is subject to terms and conditions of the Microsoft Public License. A 
    // copy of the license can be found in the License.html file at the root of this distribution. 
    // If you cannot locate the  Microsoft Public License, please send an email to 
    // dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
    //  by the terms of the Microsoft Public License.
    // ----------------------------------------------------------------------------------------------
    // You must not remove this notice, or any other, from this software.
    // ----------------------------------------------------------------------------------------------
    
    
    
    namespace Source.Extensions
    {
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using System.IO;
        using System.Reflection;
    
        using Source.Common;
    
        static partial class BasicExtensions
        {
            public static bool IsNullOrWhiteSpace (this string v)
            {
                return string.IsNullOrWhiteSpace (v);
            }
    
            public static bool IsNullOrEmpty (this string v)
            {
                return string.IsNullOrEmpty (v);
            }
    
            public static T FirstOrReturn<T>(this T[] values, T defaultValue)
            {
                if (values == null)
                {
                    return defaultValue;
                }
    
                if (values.Length == 0)
                {
                    return defaultValue;
                }
    
                return values[0];
            }
    
            public static T FirstOrReturn<T>(this IEnumerable<T> values, T defaultValue)
            {
                if (values == null)
                {
                    return defaultValue;
                }
    
                foreach (var value in values)
                {
                    return value;
                }
    
                return defaultValue;
            }
    
            public static string DefaultTo (this string v, string defaultValue = null)
            {
                return !v.IsNullOrEmpty () ? v : (defaultValue ?? "");
            }
    
            public static IEnumerable<T> DefaultTo<T>(
                this IEnumerable<T> values, 
                IEnumerable<T> defaultValue = null
                )
            {
                return values ?? defaultValue ?? Array<T>.Empty;
            }
    
            public static T[] DefaultTo<T>(this T[] values, T[] defaultValue = null)
            {
                return values ?? defaultValue ?? Array<T>.Empty;
            }
    
            public static T DefaultTo<T>(this T v, T defaultValue = default (T))
                where T : struct, IEquatable<T>
            {
                return !v.Equals (default (T)) ? v : defaultValue;
            }
    
            public static string FormatWith (this string format, CultureInfo cultureInfo, params object[] args)
            {
                return string.Format (cultureInfo, format ?? "", args.DefaultTo ());
            }
    
            public static string FormatWith (this string format, params object[] args)
            {
                return format.FormatWith (Config.DefaultCulture, args);
            }
    
            public static TValue Lookup<TKey, TValue>(
                this IDictionary<TKey, TValue> dictionary, 
                TKey key, 
                TValue defaultValue = default (TValue))
            {
                if (dictionary == null)
                {
                    return defaultValue;
                }
    
                TValue value;
                return dictionary.TryGetValue (key, out value) ? value : defaultValue;
            }
    
            public static TValue GetOrAdd<TKey, TValue>(
                this IDictionary<TKey, TValue> dictionary, 
                TKey key, 
                TValue defaultValue = default (TValue))
            {
                if (dictionary == null)
                {
                    return defaultValue;
                }
    
                TValue value;
                if (!dictionary.TryGetValue (key, out value))
                {
                    value = defaultValue;
                    dictionary[key] = value;
                }
    
                return value;
            }
    
            public static TValue GetOrAdd<TKey, TValue>(
                this IDictionary<TKey, TValue> dictionary,
                TKey key,
                Func<TValue> valueCreator
                )
            {
                if (dictionary == null)
                {
                    return valueCreator ();
                }
    
                TValue value;
                if (!dictionary.TryGetValue (key, out value))
                {
                    value = valueCreator ();
                    dictionary[key] = value;
                }
    
                return value;
            }
    
            public static void DisposeNoThrow (this IDisposable disposable)
            {
                try
                {
                    if (disposable != null)
                    {
                        disposable.Dispose ();
                    }
                }
                catch (Exception exc)
                {
                    Log.Exception ("DisposeNoThrow: Dispose threw: {0}", exc);
                }
            }
    
            public static TTo CastTo<TTo> (this object value, TTo defaultValue)
            {
                return value is TTo ? (TTo) value : defaultValue;
            }
    
            public static string Concatenate (this IEnumerable<string> values, string delimiter = null, int capacity = 16)
            {
                values = values ?? Array<string>.Empty;
                delimiter = delimiter ?? ", ";
    
                return string.Join (delimiter, values);
            }
    
            public static string GetResourceString (this Assembly assembly, string name, string defaultValue = null)
            {
                defaultValue = defaultValue ?? "";
    
                if (assembly == null)
                {
                    return defaultValue;
                }
    
                var stream = assembly.GetManifestResourceStream (name ?? "");
                if (stream == null)
                {
                    return defaultValue;
                }
    
                using (stream)
                using (var streamReader = new StreamReader (stream))
                {
                    return streamReader.ReadToEnd ();
                }
            }
    
            public static IEnumerable<string> ReadLines (this TextReader textReader)
            {
                if (textReader == null)
                {
                    yield break;
                }
    
                string line;
    
                while ((line = textReader.ReadLine ()) != null)
                {
                    yield return line;
                }
            }
    
    #if !NETFX_CORE
            public static IEnumerable<Type> GetInheritanceChain (this Type type)
            {
                while (type != null)
                {
                    yield return type;
                    type = type.BaseType;
                }
            }
    #endif
        }
    }
}
// @@@ END_INCLUDE: https://raw.github.com/mrange/T4Include/master/Extensions/BasicExtensions.cs
// ############################################################################

// ############################################################################
// @@@ BEGIN_INCLUDE: https://raw.github.com/mrange/T4Include/master/Extensions/WpfExtensions.cs
namespace WellFormed2.WPF.Internal
{
    // ----------------------------------------------------------------------------------------------
    // Copyright (c) Mårten Rånge.
    // ----------------------------------------------------------------------------------------------
    // This source code is subject to terms and conditions of the Microsoft Public License. A 
    // copy of the license can be found in the License.html file at the root of this distribution. 
    // If you cannot locate the  Microsoft Public License, please send an email to 
    // dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
    //  by the terms of the Microsoft Public License.
    // ----------------------------------------------------------------------------------------------
    // You must not remove this notice, or any other, from this software.
    // ----------------------------------------------------------------------------------------------
    
    
    
    
    namespace Source.Extensions
    {
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using System.Linq;
        using System.Reflection;
        using System.Text;
        using System.Windows;
        using System.Windows.Data;
        using System.Windows.Media;
        using System.Windows.Threading;
        using System.Xml;
    
        using Source.Common;
    
        static partial class WpfExtensions
        {
            public static void Async_Invoke (
                this Dispatcher dispatcher, 
                string actionName, 
                Action action
                )
            {
                if (action == null)
                {
                    return;
                }
    
                Action act = () =>
                                 {
    #if DEBUG
                                     Log.Info ("Async_Invoke: {0}", actionName ?? "Unknown");
    #endif
    
                                     try
                                     {
                                         action ();
                                     }
                                     catch (Exception exc)
                                     {
                                         Log.Exception ("Async_Invoke: Caught exception: {0}", exc);
                                     }
                                 };
    
                dispatcher = dispatcher ?? Dispatcher.CurrentDispatcher;
                dispatcher.BeginInvoke (DispatcherPriority.ApplicationIdle, act);
            }
    
            public static void Async_Invoke (
                this DependencyObject dependencyObject, 
                string actionName, 
                Action action
                )
            {
                var dispatcher = dependencyObject == null ? Dispatcher.CurrentDispatcher : dependencyObject.Dispatcher;
    
                dispatcher.Async_Invoke (actionName, action);
            }
    
            public static BindingBase GetBindingOf (
                this DependencyObject dependencyObject, 
                DependencyProperty dependencyProperty 
                )
            {
                if (dependencyObject == null)
                {
                    return null;
                }
    
                if (dependencyProperty == null)
                {
                    return null;
                }
    
                return BindingOperations.GetBindingBase (dependencyObject, dependencyProperty);
            }
    
            public static bool IsBoundTo (
                this DependencyObject dependencyObject, 
                DependencyProperty dependencyProperty 
                )
            {
                if (dependencyObject == null)
                {
                    return false;
                }
    
                if (dependencyProperty == null)
                {
                    return false;
                }
    
                return BindingOperations.IsDataBound (dependencyObject, dependencyProperty);
            }
    
            public static void ResetBindingOf (
                this DependencyObject dependencyObject, 
                DependencyProperty dependencyProperty, 
                BindingBase binding = null
                )
            {
                if (dependencyObject == null)
                {
                    return;
                }
    
                if (dependencyProperty == null)
                {
                    return;
                }
    
                BindingOperations.ClearBinding (dependencyObject, dependencyProperty);
    
                if (binding != null)
                {
                    BindingOperations.SetBinding (dependencyObject, dependencyProperty, binding);
                }
            }
    
            public static TFreezable FreezeObject<TFreezable> (this TFreezable freezable)
                where TFreezable : Freezable
            {
                if (freezable == null)
                {
                    return null;
                }
    
                if (!freezable.IsFrozen && freezable.CanFreeze)
                {
                    freezable.Freeze ();
                }
    
                return freezable;
            }
    
            public static TranslateTransform ToTranslateTransform (this Vector v)
            {
                return new TranslateTransform (v.X, v.Y);
            }
    
            public static TranslateTransform UpdateFromVector (this TranslateTransform tt, Vector v)
            {
                if (tt == null)
                {
                    return null;
                }
    
                tt.X = v.X;
                tt.Y = v.Y;
    
                return tt;
            }
    
            public static bool IsNear (this double v, double c)
            {
                return Math.Abs(v - c) < double.Epsilon * 100;            
            }
    
            public static double Interpolate (this double t, double from, double to)
            {
                if (t < 0)
                {
                    return from;
                }
    
                if (t > 1)
                {
                    return to;
                }
    
                return t*(to - from) + from;
            }
            
            public static Vector Interpolate (this double t, Vector from, Vector to)
            {
                if (t < 0)
                {
                    return from;
                }
    
                if (t > 1)
                {
                    return to;
                }
    
                return new Vector (
                    t*(to.X - from.X) + from.X,
                    t*(to.Y - from.Y) + from.Y
                    );
            }
    
            public static Rect ToRect (this Size size)
            {
                return new Rect(0,0,size.Width, size.Height);
            }
        
            public static string GetName (this DependencyObject dependencyObject)
            {
                var frameworkElement = dependencyObject as FrameworkElement;
                if (frameworkElement != null)
                {
                    return frameworkElement.Name ?? "";
                }
    
                var frameworkContentElement = dependencyObject as FrameworkContentElement;
                if (frameworkContentElement != null)
                {
                    return frameworkContentElement.Name ?? "";                
                }
    
                return "";
            }
    
            public static IEnumerable<DependencyProperty> GetLocalDependencyProperties (this DependencyObject dependencyObject)
            {
                if (dependencyObject == null)
                {
                    yield break;
                }
    
                var enumerator = dependencyObject.GetLocalValueEnumerator ();
                while (enumerator.MoveNext ())
                {
                    var current = enumerator.Current;
                    yield return current.Property;
                }
            }
    
            public static IEnumerable<DependencyProperty> GetClassDependencyProperties (this Type type)
            {
                if (type == null)
                {
                    return Array<DependencyProperty>.Empty;
                }
    
                if (!typeof(DependencyObject).IsAssignableFrom(type))
                {
                    return Array<DependencyProperty>.Empty;
                }
    
                return type
                        .GetFields (BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                        .Where (fi => fi.FieldType == typeof (DependencyProperty))
                        .Select (fi => fi.GetValue (null) as DependencyProperty)
                        .Where (dp => dp != null)
                        ;
            }
    
            public static IEnumerable<DependencyProperty> GetClassDependencyProperties (this DependencyObject dependencyObject)
            {
                if (dependencyObject == null)
                {
                    return Array<DependencyProperty>.Empty;
                }
    
                return dependencyObject.GetType ().GetClassDependencyProperties ();
            }
    
            public static IEnumerable<DependencyProperty> GetDependencyProperties (this DependencyObject dependencyObject)
            {
                if (dependencyObject == null)
                {
                    return Array<DependencyProperty>.Empty;
                }
    
                return dependencyObject
                    .GetClassDependencyProperties ()
                    .Union (dependencyObject.GetLocalDependencyProperties ())
                    ;
            }
    
            public static IEnumerable<DependencyObject> GetVisualChildren (this DependencyObject dependencyObject)
            {
                if (dependencyObject == null)
                {
                    yield break;
                }
    
                var count = VisualTreeHelper.GetChildrenCount (dependencyObject);
                for (var iter = 0; iter < count; ++iter)
                {
                    yield return VisualTreeHelper.GetChild (dependencyObject, iter);
                }
            }
    
            public static IEnumerable<DependencyObject> GetLogicalChildren (this DependencyObject dependencyObject)
            {
                if (dependencyObject == null)
                {
                    return Array<DependencyObject>.Empty;    
                }
    
                return LogicalTreeHelper.GetChildren (dependencyObject).OfType<DependencyObject> ();
            }
    
            static string GetValueAsString (object obj)
            {
                var formattable = obj as IFormattable;
                if (formattable != null)
                {
                    return formattable.ToString ("", CultureInfo.InvariantCulture);
                }
    
                if (obj != null)
                {
                    return obj.ToString ();
                }
    
                return "";
            }
    
            static void GetTree_AsString_Impl (
                this DependencyObject dependencyObject, 
                XmlWriter xmlWriter,
                Func<DependencyObject, IEnumerable<DependencyObject>> childrenGetter 
                )
            {
                if (dependencyObject == null)
                {
                    return;
                }
                
                xmlWriter.WriteStartElement (dependencyObject.GetType().Name);
    
                var enumerator = dependencyObject.GetLocalValueEnumerator ();
                while (enumerator.MoveNext ())
                {
                    var current = enumerator.Current;
    
                    xmlWriter.WriteAttributeString (
                        current.Property.Name, 
                        GetValueAsString (current.Value)
                        );
                }
    
                foreach (var child in childrenGetter (dependencyObject))
                {
                    child.GetTree_AsString_Impl (xmlWriter, childrenGetter);
                }
    
                xmlWriter.WriteEndElement ();
    
            }
    
            static string GetTree_AsString (
                this DependencyObject dependencyObject,             
                Func<DependencyObject, IEnumerable<DependencyObject>> childrenGetter 
                )
            {
                var settings =  new XmlWriterSettings
                                    {
                                        Indent  = true  ,
                                    };
    
                var sb = new StringBuilder (128);
    
                using (var xmlWriter = XmlWriter.Create (sb, settings))
                {
                    xmlWriter.WriteStartDocument ();
    
                    dependencyObject.GetTree_AsString_Impl (xmlWriter, childrenGetter);
    
                    xmlWriter.WriteEndDocument ();
                }
    
                return sb.ToString ();            
            }
    
            public static string GetLogicalTree_AsString (this DependencyObject dependencyObject)
            {
                return dependencyObject.GetTree_AsString (d => d.GetLogicalChildren ());
            }
    
            public static string GetVisualTree_AsString (this DependencyObject dependencyObject)
            {
                return dependencyObject.GetTree_AsString (d => d.GetVisualChildren ());
            }
    
            public static IEnumerable<DependencyObject> GetVisualTree_BreadthFirst (this DependencyObject dependencyObject)
            {
                if (dependencyObject == null)
                {
                    yield break;
                }
    
                var queue = new Queue<DependencyObject> ();
                queue.Enqueue (dependencyObject);
    
                while (queue.Count > 0)
                {
                    var obj = queue.Dequeue ();
    
                    foreach (var child in obj.GetVisualChildren ())
                    {
                        if (child != null)
                        {
                            queue.Enqueue (child);
                            yield return child;                        
                        }
                    }                
                }
            }
    
            public static IEnumerable<DependencyObject> GetLogicalTree_BreadthFirst (this DependencyObject dependencyObject)
            {
                if (dependencyObject == null)
                {
                    yield break;
                }
    
                var queue = new Queue<DependencyObject> ();
                queue.Enqueue (dependencyObject);
    
                while (queue.Count > 0)
                {
                    var obj = queue.Dequeue ();
    
                    foreach (var child in obj.GetLogicalChildren ())
                    {
                        if (child != null)
                        {
                            queue.Enqueue (child);
                            yield return child;                        
                        }
                    }                
                }
            }
    
            public static double LimitBy (this double size, double constraint)
            {
                constraint = Math.Max (0, constraint);
    
                if (double.IsNaN (size))
                {
                    return size;
                }
    
                if (size < 0)
                {
                    return 0;
                }
    
                if (size > constraint)
                {
                    return constraint;
                }
    
                return size;
            }
    
            public static Size LimitBy (this Size size, Size constraint)
            {
                return new Size(
                    size.Width.LimitBy(constraint.Width), 
                    size.Height.LimitBy(constraint.Height)
                    );    
            }
    
            static byte HexColor (this char ch)
            {
                switch (ch)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        return (byte) (ch - '0');
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                        return (byte) (ch - 'a' + 10);
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                        return (byte) (ch - 'A' + 10);
                    default:
                        return 0;
                }       
            }
    
            static byte ExpandNibble (this byte b)
            {
                var n = b & 0xF;
                return (byte) (n | (n << 4));
            }
    
            public static Color ParseColor (this string color, Color defaultTo)
            {
                if (string.IsNullOrEmpty (color))
                {
                    return defaultTo;                
                }
    
                if (color[0] != '#')
                {                   
                    return defaultTo;
                }
    
                switch (color.Length)
                {
                    default:
                        return defaultTo;
                    case 4:
                        // #FFF
                        return Color.FromRgb (
                            color[1].HexColor().ExpandNibble(),
                            color[2].HexColor().ExpandNibble(),
                            color[3].HexColor().ExpandNibble()
                            );
                    case 5:
                        // #FFFF
                        return Color.FromArgb (
                            color[1].HexColor().ExpandNibble(),
                            color[2].HexColor().ExpandNibble(),
                            color[3].HexColor().ExpandNibble(),
                            color[4].HexColor().ExpandNibble()
                            );
                    case 7:
                        // #FFFFFF
                        return Color.FromRgb (
                            (byte) ((color[1].HexColor() << 4) + color[2].HexColor()),
                            (byte) ((color[3].HexColor() << 4) + color[4].HexColor()),
                            (byte) ((color[5].HexColor() << 4) + color[6].HexColor())
                            );
                    case 9:
                        // #FFFFFFFF
                        return Color.FromArgb (
                            (byte) ((color[1].HexColor() << 4) + color[2].HexColor()),
                            (byte) ((color[3].HexColor() << 4) + color[4].HexColor()),
                            (byte) ((color[5].HexColor() << 4) + color[6].HexColor()),
                            (byte) ((color[7].HexColor() << 4) + color[8].HexColor())
                            );
                }
    
            }
    
        }
    }
}
// @@@ END_INCLUDE: https://raw.github.com/mrange/T4Include/master/Extensions/WpfExtensions.cs
// ############################################################################

// ############################################################################
// @@@ BEGIN_INCLUDE: https://raw.github.com/mrange/T4Include/master/Common/Array.cs
namespace WellFormed2.WPF.Internal
{
    // ----------------------------------------------------------------------------------------------
    // Copyright (c) Mårten Rånge.
    // ----------------------------------------------------------------------------------------------
    // This source code is subject to terms and conditions of the Microsoft Public License. A 
    // copy of the license can be found in the License.html file at the root of this distribution. 
    // If you cannot locate the  Microsoft Public License, please send an email to 
    // dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
    //  by the terms of the Microsoft Public License.
    // ----------------------------------------------------------------------------------------------
    // You must not remove this notice, or any other, from this software.
    // ----------------------------------------------------------------------------------------------
    
    namespace Source.Common
    {
        static class Array<T>
        {
            public static readonly T[] Empty = new T[0];
        }
    }
}
// @@@ END_INCLUDE: https://raw.github.com/mrange/T4Include/master/Common/Array.cs
// ############################################################################

// ############################################################################
// @@@ BEGIN_INCLUDE: https://raw.github.com/mrange/T4Include/master/Common/Config.cs
namespace WellFormed2.WPF.Internal
{
    // ----------------------------------------------------------------------------------------------
    // Copyright (c) Mårten Rånge.
    // ----------------------------------------------------------------------------------------------
    // This source code is subject to terms and conditions of the Microsoft Public License. A 
    // copy of the license can be found in the License.html file at the root of this distribution. 
    // If you cannot locate the  Microsoft Public License, please send an email to 
    // dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
    //  by the terms of the Microsoft Public License.
    // ----------------------------------------------------------------------------------------------
    // You must not remove this notice, or any other, from this software.
    // ----------------------------------------------------------------------------------------------
    
    
    namespace Source.Common
    {
        using System.Globalization;
    
        sealed partial class InitConfig
        {
            public CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
        }
    
        static partial class Config
        {
            static partial void Partial_Constructed(ref InitConfig initConfig);
    
            public readonly static CultureInfo DefaultCulture;
    
            static Config ()
            {
                var initConfig = new InitConfig();
    
                Partial_Constructed (ref initConfig);
    
                initConfig = initConfig ?? new InitConfig();
    
                DefaultCulture = initConfig.DefaultCulture;
            }
        }
    }
}
// @@@ END_INCLUDE: https://raw.github.com/mrange/T4Include/master/Common/Config.cs
// ############################################################################

// ############################################################################
// @@@ BEGIN_INCLUDE: https://raw.github.com/mrange/T4Include/master/Common/Log.cs
namespace WellFormed2.WPF.Internal
{
    // ----------------------------------------------------------------------------------------------
    // Copyright (c) Mårten Rånge.
    // ----------------------------------------------------------------------------------------------
    // This source code is subject to terms and conditions of the Microsoft Public License. A 
    // copy of the license can be found in the License.html file at the root of this distribution. 
    // If you cannot locate the  Microsoft Public License, please send an email to 
    // dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
    //  by the terms of the Microsoft Public License.
    // ----------------------------------------------------------------------------------------------
    // You must not remove this notice, or any other, from this software.
    // ----------------------------------------------------------------------------------------------
    
    
    
    namespace Source.Common
    {
        using System;
        using System.Globalization;
    
        static partial class Log
        {
            static partial void Partial_LogLevel (Level level);
            static partial void Partial_LogMessage (Level level, string message);
            static partial void Partial_ExceptionOnLog (Level level, string format, object[] args, Exception exc);
    
            public static void LogMessage (Level level, string format, params object[] args)
            {
                try
                {
                    Partial_LogLevel (level);
                    Partial_LogMessage (level, GetMessage (format, args));
                }
                catch (Exception exc)
                {
                    Partial_ExceptionOnLog (level, format, args, exc);
                }
                
            }
    
            static string GetMessage (string format, object[] args)
            {
                format = format ?? "";
                try
                {
                    return (args == null || args.Length == 0)
                               ? format
                               : string.Format (Config.DefaultCulture, format, args)
                        ;
                }
                catch (FormatException)
                {
    
                    return format;
                }
            }
        }
    }
}
// @@@ END_INCLUDE: https://raw.github.com/mrange/T4Include/master/Common/Log.cs
// ############################################################################

// ############################################################################
// @@@ BEGIN_INCLUDE: https://raw.github.com/mrange/T4Include/master/Common/Generated_Log.cs
namespace WellFormed2.WPF.Internal
{
    // ############################################################################
    // #                                                                          #
    // #        ---==>  T H I S  F I L E  I S   G E N E R A T E D  <==---         #
    // #                                                                          #
    // # This means that any edits to the .cs file will be lost when its          #
    // # regenerated. Changes should instead be applied to the corresponding      #
    // # template file (.tt)                                                      #
    // ############################################################################
    
    
    
    
    
    
    namespace Source.Common
    {
        using System;
    
        partial class Log
        {
            public enum Level
            {
                Success = 1000,
                HighLight = 2000,
                Info = 3000,
                Warning = 10000,
                Error = 20000,
                Exception = 21000,
            }
    
            public static void Success (string format, params object[] args)
            {
                LogMessage (Level.Success, format, args);
            }
            public static void HighLight (string format, params object[] args)
            {
                LogMessage (Level.HighLight, format, args);
            }
            public static void Info (string format, params object[] args)
            {
                LogMessage (Level.Info, format, args);
            }
            public static void Warning (string format, params object[] args)
            {
                LogMessage (Level.Warning, format, args);
            }
            public static void Error (string format, params object[] args)
            {
                LogMessage (Level.Error, format, args);
            }
            public static void Exception (string format, params object[] args)
            {
                LogMessage (Level.Exception, format, args);
            }
    #if !NETFX_CORE && !SILVERLIGHT && !WINDOWS_PHONE
            static ConsoleColor GetLevelColor (Level level)
            {
                switch (level)
                {
                    case Level.Success:
                        return ConsoleColor.Green;
                    case Level.HighLight:
                        return ConsoleColor.White;
                    case Level.Info:
                        return ConsoleColor.Gray;
                    case Level.Warning:
                        return ConsoleColor.Yellow;
                    case Level.Error:
                        return ConsoleColor.Red;
                    case Level.Exception:
                        return ConsoleColor.Red;
                    default:
                        return ConsoleColor.Magenta;
                }
            }
    #endif
            static string GetLevelMessage (Level level)
            {
                switch (level)
                {
                    case Level.Success:
                        return "SUCCESS  ";
                    case Level.HighLight:
                        return "HIGHLIGHT";
                    case Level.Info:
                        return "INFO     ";
                    case Level.Warning:
                        return "WARNING  ";
                    case Level.Error:
                        return "ERROR    ";
                    case Level.Exception:
                        return "EXCEPTION";
                    default:
                        return "UNKNOWN  ";
                }
            }
    
        }
    }
    
}
// @@@ END_INCLUDE: https://raw.github.com/mrange/T4Include/master/Common/Generated_Log.cs
// ############################################################################

// ############################################################################
namespace WellFormed2.WPF.Internal.Include
{
    static partial class MetaData
    {
        public const string RootPath        = @"https://raw.github.com/";
        public const string IncludeDate     = @"2013-07-27T20:20:04";

        public const string Include_0       = @"https://raw.github.com/mrange/T4Include/master/Extensions/BasicExtensions.cs";
        public const string Include_1       = @"https://raw.github.com/mrange/T4Include/master/Extensions/WpfExtensions.cs";
        public const string Include_2       = @"https://raw.github.com/mrange/T4Include/master/Common/Array.cs";
        public const string Include_3       = @"https://raw.github.com/mrange/T4Include/master/Common/Config.cs";
        public const string Include_4       = @"https://raw.github.com/mrange/T4Include/master/Common/Log.cs";
        public const string Include_5       = @"https://raw.github.com/mrange/T4Include/master/Common/Generated_Log.cs";
    }
}
// ############################################################################


