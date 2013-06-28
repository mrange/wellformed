wellformed
==========

Hi there.

I stumbled upon WebSharper for F# which was amazing. One of the most interesting aspects of WebSharper was how you defined webforms using F# computation expressions.

That gave me the idea for WellFormed - Use F# computation expressions to define WPF forms

A simple form using WellFormed could then be

```

let IndividualFormlet = 
    Formlet.Do
        {
            let!    firstName   = LabelInput "First name"
            let!    lastName    = LabelInput "Last name"
            let!    regno       = LabelInput "Registration no"

            return Individual {FirstName = firstName; LastName = lastName; RegNo = regno;}
        }
        |> Enchance.WithGroup "Individual Information"


```

This will be rendered as three textboxes with labels all wrapped in a group.

It's and type-safe, composable and declarative. The problem with declarative coding is that it's great for what's intended to do but often hard to hook in custom logic.

Not so with WebSharper (and in extension WellFormed)

This how to use a combobox to show either the formlet to collect information about a individual or information about a company. 

```

let EntityFormlet = 
    Formlet.Do
        {
            let! select = Select 0 ["Individual", IndividualFormlet; "Company",  CompanyFormlet]

            return! select
        }

```

First time it takes a while to see what it does but that comes more from having preconceived notions on how develop forms instead of looking at what the code is trying to tell you.

This sold me on using computation expressions for forms.

The currently state of WellFormed is honestly a bag of ideas but the code is slowly transforming into something more and more useful.

Feedback appreciated

Mårten
