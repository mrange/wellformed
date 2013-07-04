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

namespace WellFormed.Core

type LayoutOrientation = 
    |   TopToBottom
    |   LeftToRight

type StretchBehavior = 
    |   NoStretch
    |   RightStretches

[<StructuralEquality>]
[<StructuralComparison>]
type Failure =
    {
        Context : string list
        Message : string
    }
    static member New (context : string list) (message : string) = { Context = context; Message = message;}

[<StructuralEquality>]
[<StructuralComparison>]
type Collect<'T> =
    {
        Value       : 'T
        Failures    : Failure list
    }
    static member New (value : 'T) (failures : Failure list) = { Value = value; Failures = failures;}

