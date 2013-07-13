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

using Microsoft.FSharp.Core;

namespace WellFormed2.WPF
{
    static partial class Extensions
    {

        public static FSharpOption<T> ToOption<T> (this T value)
            where T : class
        {
            return value != null ? FSharpOption<T>.Some (value) : FSharpOption<T>.None;
        }
    }
}
