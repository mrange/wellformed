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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using WellFormed2.WPF.Internal.Source.Extensions;

namespace WellFormed2.WPF.Internal
{
    partial class DispatchQueue<TEventName>
        where TEventName : class
    {
        struct NamedEvent
        {
            public TEventName   Name        ;
            public Action       Action      ;
            
            public static NamedEvent Create (TEventName name, Action action)
            {
                return new NamedEvent
                {
                    Name    = name  ,
                    Action  = action    ,
                };
            }
        }

        readonly Dispatcher m_dispatcher;
        readonly List<NamedEvent> m_actions = new List<NamedEvent>();

        int m_isDispatching;

        public DispatchQueue (Dispatcher dispatcher)
        {
            m_dispatcher = dispatcher;
        }

        public void Async_Dispatch (TEventName eventName, Action action)
        {
            m_dispatcher.VerifyAccess ();

            if (action == null)
            {
                return;
            }

            m_actions.Add (NamedEvent.Create (eventName, action));

            StartDispatch ();
        }

        void StartDispatch ()
        {
            if (Interlocked.Exchange (ref m_isDispatching, 1) == 0)
            {
                m_dispatcher.Async_Invoke ("DispatchQueue", OnDispatched);
            }
        }

        void OnDispatched ()
        {
            try
            {
                if (m_actions.Count > 0)
                {
                    var seenNames = new Dictionary<TEventName, int> ();

                    for (var iter = m_actions.Count - 1; iter >= 0; --iter)
                    {
                        var evt = m_actions[iter];

                        if (!seenNames.ContainsKey (evt.Name))
                        {
                            seenNames[evt.Name] = iter;
                        }
                    }

                    var actions = seenNames
                        .Select (kv => kv.Value)
                        .OrderBy (v => v)
                        .Select (i => m_actions[i].Action)
                        .Where (a => a != null)
                        .ToArray ()
                        ;

                    m_actions.Clear ();

                    foreach (var act in actions)
                    {
                        act ();
                    }
                }
            }
            finally
            {
                Interlocked.Exchange (ref m_isDispatching, 0);
            }
        }
    }
}