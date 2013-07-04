wellformed
==========

Hi there.

I stumbled upon WebSharper for F# which was amazing. One of the most interesting aspects of WebSharper was how you defined webforms using F# computation expressions.

That gave me the idea for WellFormed - Use F# computation expressions to define WPF forms

A simple form using WellFormed could then be

```

let IndividualFormlet regNoValidator = 
    Formlet.Do
        {
            let!    firstName   = NonEmpty "First name"
            let!    lastName    = NonEmpty "Last name"
            let!    regno       = Validated "Registration no" regNoValidator
            let!    dayOfBirth  = Date "Birth date"

            return Individual {FirstName = firstName; LastName = lastName; RegNo = regno; DayOfBirth = dayOfBirth; }
        }
        |> Enhance.WithLegend "Individual Information"


```

This will be rendered as three textboxes with labels all wrapped in a group.

It's and type-safe, composable and declarative. The problem with declarative coding is that it's great for what's intended to do but often hard to hook in custom logic.

Not so with WebSharper (and in extension WellFormed)

This how to use a combobox to show either the formlet to collect information about a individual or information about a company. 

```

let EntityFormlet individualRegNoValidator companyRegNoValidator =  
        let options  =  Input.Option [|"Individual", IndividualFormlet individualRegNoValidator; "Company",  CompanyFormlet companyRegNoValidator|]
                        |> Enhance.WithLabel "Type"
        Formlet.Do
            {
                let! option = options  
                return! option
            }

```

First time it takes a while to see what it does but that comes more from having preconceived notions on how develop forms instead of looking at what the code is trying to tell you.

This sold me on using computation expressions for forms.

The current state of wellformed is that it works pretty well and should be good enough to be considered if you are looking for something like this for WPF and F#.

TODO
====

1. Implement Enhance.Many - In order to create formlets that supports create list of objects
1.1. Begun implement Enhance.Many, the UI is functional but a bit rough
1. Tweak the UI to be more appealing
1. Support Visual Styling of controls.
1. Refactor Formlet<T> - In order to support advance layout, visual tree collapsing and smarter preserving of user state they way formlets are implemented will need a major overhaul. How you compose formlets will not be affected but custom implementations of formlets will.

Feedback appreciated

Mårten
