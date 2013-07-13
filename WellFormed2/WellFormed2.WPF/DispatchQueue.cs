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

using System.Threading;
using System.Windows.Threading;
using WellFormed2.WPF.Source.Extensions;

namespace WellFormed2.WPF
{
    partial class DispatchQueue<TEventName>
    {
        readonly Dispatcher m_dispatcher;

        int m_isDispatching;

        public DispatchQueue(Dispatcher dispatcher)
        {
            m_dispatcher = dispatcher;
        }

        void StartDispatch ()
        {
            if (Interlocked.Exchange (ref m_isDispatching, 1) == 0)
            {
                m_dispatcher.Async_Invoke ("DispatchQueue", OnDispatched);
            }
        }

        void OnDispatched()
        {
            try
            {
                    
            }
            finally
            {
                m_isDispatching = 0;
            }
        }
    }
}