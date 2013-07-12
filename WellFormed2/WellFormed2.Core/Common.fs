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


namespace WellFormed2.Core

[<AutoOpen>]
module internal InternalCommon =
    
    let ToForm (f : #IForm<'T>) = f :> IForm<'T>
    let ToFormlet (f : #IFormlet<'T>) = f :> IFormlet<'T>

    let EmptyArray = [||]

    let Success v = Collect.New v []

    let HardFail msg            = failwith msg

    let HardFail_InvalidCase () = HardFail "WellFormed2.ProgrammmingError: This case shouldn't be reached"

    let FailWithValue value (msg : string) = Collect.New value [{Context = []; Message = msg;}]

    let Fail<'T> (msg : string)   = Collect.New Unchecked.defaultof<'T> [{Context = []; Message = msg;}]

    let Fail_NeverBuiltUp ()= Fail "WellFormed2.ProgrammmingError: Never built up"



