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

open System.Collections.Generic

[<AutoOpen>]
module Common =
    
    let ToForm (f : #IForm<'T>) = f :> IForm<'T>
    let ToFormlet (f : #IFormlet<'T>) = f :> IFormlet<'T>

    let Success v = Collect.New v []

    let HardFail msg            = failwith msg

    let FailWithValue value (msg : string) = Collect.New value [{Context = []; Message = msg;}]

    let Fail<'T> (msg : string)   = Collect.New Unchecked.defaultof<'T> [{Context = []; Message = msg;}]

[<AutoOpen>]
module internal InternalCommon =
    
    let HardFail_InvalidCase () = HardFail "WellFormed2.ProgrammmingError: This case shouldn't be reached"

    let Fail_NeverBuiltUp ()= Fail "WellFormed2.ProgrammmingError: Never built up"

    type MergedOrientation =
        |   AllSame
        |   CurrentAndLeftSame
        |   CurrentAndRightSame
        |   LeftAndRightSame
        |   AllDifferent

    let FlattenTree (co : LayoutOrientation) (vt : VisualTree) =
        let GetOrientation (o : LayoutOrientation) (vt : VisualTree) =
            match vt with
            | Empty         -> o
            | Leaf _        -> o
            | Fork (fo,_,_) -> fo

        let rec AppendToList (fvt : List<FlatVisualTree>) (vt : VisualTree) =
            match vt with
            | Empty         ->  ()
            | Leaf v        ->  fvt.Add (Visual v)
            | Fork (o,l,r)  ->
                let lo = GetOrientation o l
                let ro = GetOrientation o r  

                if o = lo && o = ro then
                    AppendToList fvt l
                    AppendToList fvt r
                elif o = lo then
                    AppendToList fvt l
                    fvt.Add <| MakeFlatTree ro r
                elif o = ro then
                    fvt.Add <| MakeFlatTree lo l
                    AppendToList fvt r
                elif lo = ro then
                    let ifvt = List<FlatVisualTree> ()
                    AppendToList ifvt l
                    AppendToList ifvt r
                    fvt.Add <| Layout (lo, ifvt.ToArray ())
                else
                    fvt.Add <| MakeFlatTree lo l
                    fvt.Add <| MakeFlatTree ro r

        and MakeFlatTree (o : LayoutOrientation) (vt : VisualTree) =
            let fvt = List<FlatVisualTree> ()
            AppendToList fvt vt
            Layout (o, fvt
            .ToArray ())

        MakeFlatTree co vt

