namespace WellFormed.Core

open System

open System.Collections.Generic

open System.Windows
open System.Windows.Controls

type DelayControl() =
    inherit ContentControl()

    member this.Value 
        with    get ()                          = this.Content :?> FrameworkElement
        and     set (value : FrameworkElement)  = this.Content <- value


type JoinControl<'T>() =
    inherit FrameworkElement()

    member val Left = null :> FrameworkElement with get, set
    member val Right = null :> FrameworkElement with get, set

    member val Formlet : 'T option = None with get, set
