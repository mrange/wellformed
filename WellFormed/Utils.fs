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

namespace WellFormed

open System
open System.Windows

open System.Text.RegularExpressions

open WellFormed.Core


[<AutoOpen>]
module Utils = 

    let Multiply (mp : int array) (s : string) = 
        s.ToCharArray()
        |> Array.zip mp
        |> Array.map (fun (l,r) -> l*(int r - int '0'))

    let MultiplyAndAccumulateWithFlatten (mp : int array) (s : string) = 
        Multiply mp s
        |> Array.map (fun v -> v / 10 + v % 10)
        |> Array.sum

    let MultiplyAndAccumulate (mp : int array) (s : string) = 
        Multiply mp s
        |> Array.sum

    let SwedenRegexRegNoPattern = Regex (@"^\d{6}-\d{4}$", RegexOptions.Compiled)
    let SwedenRegNoMultiplyPattern = [|2;1;2;1;2;1;0;2;1;2;1;|]

    let SwedenRegNo regNo = 
        if not  <| SwedenRegexRegNoPattern.IsMatch (regNo) then
            Some "Registration number needs the form: YYMMDD-CCCC"
        else
            let maa = (MultiplyAndAccumulateWithFlatten SwedenRegNoMultiplyPattern regNo) % 10

            if maa <> 0 then
                Some "Registration number checksum not correct"
            else 
                None

    let NorwayRegexRegNoPattern = Regex (@"^\d{6}-\d{5}$", RegexOptions.Compiled)
    let NorwayRegNoMultiplyPattern1 = [|3;7;6;1;8;9;0;4;5;2;1;0;|]
    let NorwayRegNoMultiplyPattern2 = [|5;4;3;2;7;6;0;5;4;3;2;1;|]


    let NorwayRegNo regNo = 
        if not <| NorwayRegexRegNoPattern.IsMatch (regNo) then
            Some "Registration number needs the form: DDMMYY-CCCCC"
        else
            let first   = (MultiplyAndAccumulate NorwayRegNoMultiplyPattern1 regNo) % 11
            let second  = (MultiplyAndAccumulate NorwayRegNoMultiplyPattern2 regNo) % 11

            if first <> 0 || second <> 0 then
                Some "Registration number checksum not correct"
            else 
                None


    let Validated t validator = 
        Input.Text "" 
        |> Enhance.WithValidation validator
        |> Enhance.WithErrorVisual
        |> Enhance.WithLabel t

    let NonEmpty t = 
        Input.Text "" 
        |> Enhance.WithValidation_NonEmpty
        |> Enhance.WithErrorVisual
        |> Enhance.WithLabel t

    let AllowEmpty t = 
        Input.Text "" 
        |> Enhance.WithLabel t

    let Int t v = 
        Input.Integer v
        |> Enhance.WithErrorVisual
        |> Enhance.WithLabel t

    let Date t = 
        Input.DateTime None
        |> Enhance.WithErrorVisual
        |> Enhance.WithLabel t

