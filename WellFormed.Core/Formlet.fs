namespace WellFormed.Core

open System

open System.Windows
open System.Windows.Controls

type Result<'T> =
    | Success of 'T
    | Failure of string list


type Body =
    |   Empty
    |   Element         of FrameworkElement
    |   Label           of TextBlock*Body
    |   Group           of FrameworkElement*Grid*Body
    |   Join            of Body*Body

type IForm<'T> =
    abstract member Body    : unit -> Body
    abstract member Collect : unit -> Result<'T>
    inherit IDisposable

type Form<'T> =
    {
        Body    : unit -> Body
        Collect : unit -> Result<'T>
        Dispose : unit -> unit
    }
    interface IForm<'T> with
        member this.Body() = this.Body()
        member this.Collect() = this.Collect()
    interface IDisposable with
        member this.Dispose() = this.Dispose()
    static member New body collect dispose = {Body = body; Collect = collect; Dispose = dispose; }

type Formlet<'T> = 
    {
        Build : unit -> IForm<'T>
    }
    static member New (build : unit -> IForm<'T>) = { Build = build; }

module Formlet =

    let MapResult (m : Result<'T> -> Result<'U>) (f : Formlet<'T>) : Formlet<'U> = 
        let build () =
            let form = f.Build()

            let state : (Result<'T>*Result<'U>) option ref = ref None

            let getState() =
                state := 
                    match !state with
                        |   None            ->  let r = form.Collect()
                                                Some (r, (m r))
                        |   Some (r', mr')  ->  let r = form.Collect()
                                                if IsEqual r r'
                                                    then Some (r', mr')
                                                    else Some (r, (m r))

                (!state).Value
            {
                Body        = form.Body
                Dispose     = form.Dispose
                Collect     = fun () -> 
                                    let (r, mr) = getState()
                                    mr
            } :> IForm<'U>
        Formlet.New build

    let Map (m : 'T -> 'U) (f : Formlet<'T>) : Formlet<'U> = 
        let m' r =
            match r with 
                |   Success v   -> Success (m v)
                |   Failure s   -> Failure s
        MapResult m' f

    let Join (formlet: Formlet<Formlet<'T>>) : Formlet<'T> = 
        let build () =
            let form = formlet.Build()

            let state : (Result<Formlet<'T>>*IForm<'T> option) option ref = ref None

            let buildState collect =    match collect with
                                            |   Success formLet -> collect, Some (formLet.Build())
                                            |   f -> collect, None

            let getState() = 
                let innerCollect = form.Collect()
                state :=
                    match !state with
                        |   None -> Some (buildState innerCollect)
                        |   Some (innerCollect', None) -> 
                                if IsEqual innerCollect' innerCollect
                                    then Some (innerCollect', None)
                                    else Some (buildState innerCollect)
                        |   Some (innerCollect', Some innerForm') -> 
                                if IsEqual innerCollect' innerCollect
                                    then Some (innerCollect, Some innerForm')
                                    else 
                                            innerForm'.Dispose()
                                            Some (buildState innerCollect')
                (!state).Value

            {
                Body        = fun () -> 
                    let body = form.Body()

                    let i = getState()
                    match i with         
                        |   (_, Some innerForm) -> Join (body, innerForm.Body())
                        |   _                   -> Join (body, Empty)
                Dispose     = DoNothing
                Collect     = fun () -> 
                    let i = getState()
                    match i with         
                        |   (_, Some innerForm) -> innerForm.Collect()
                        |   (Failure f, _)      -> Failure f
            } :> IForm<'T>
        Formlet.New build

    let Bind<'T1, 'T2> (f : Formlet<'T1>) (b : 'T1 -> Formlet<'T2>) : Formlet<'T2> = 
        f |> Map b |> Join

                    
    let Return (x : 'T) : Formlet<'T> = 
        let state =          
                        {
                            Body        = fun () -> Empty
                            Dispose     = DoNothing
                            Collect     = fun () -> Success x
                        } :> IForm<'T>
        let build() = state
        Formlet.New build

    let Delay (f : unit -> Formlet<'T>) : Formlet<'T> = 
        let state = ref None
        let build () = 

            state := 
                match !state with
                    |   None -> Some (f().Build())
                    |   v -> v

            (!state).Value
        Formlet.New build

    let ReturnFrom (f : Formlet<'T>) = f

    type FormletBuilder() =
        member this.Return x = Return x
        member this.Bind(x, f) = Bind x f
        member this.Delay f = Delay f
        member this.ReturnFrom f = ReturnFrom f

    let Do = new FormletBuilder()


