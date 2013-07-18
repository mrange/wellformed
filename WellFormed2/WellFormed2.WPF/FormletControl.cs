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
using System.Windows.Controls;
using WellFormed2.Core;

namespace WellFormed2.WPF
{
    public partial class ResetFormEventArgs
    {
        
    }

    public partial class SubmitFormEventArgs
    {
        
    }

    public partial class RebuildFormEventArgs
    {
        
    }

    public partial class FormletControl : Control
    {

        FormletRebuildContext m_context;
        IFormletHolder m_holder;

        partial void Constructed__FormletControl()
        {
            m_context = new FormletRebuildContext ();

            this.AddHandler_RebuildForm (OnRebuildForm);
            this.AddHandler_SubmitForm  (OnSubmitForm);
            this.AddHandler_ResetForm   (OnResetForm);
        }

        void OnResetForm(object sender, ResetFormEventArgs eventargs)
        {
            throw new NotImplementedException();
        }

        void OnSubmitForm(object sender, SubmitFormEventArgs eventargs)
        {
            throw new NotImplementedException();
        }

        void OnRebuildForm(object sender, RebuildFormEventArgs eventargs)
        {
            throw new NotImplementedException();
        }

        public void ShowFormlet<T> (Types.IFormlet<T> formlet, Action<T> submitter)
        {
            if (formlet == null)
            {
                return;
            }

            m_holder = new FormletHolder<T> (m_context, formlet, submitter);

            var ftv = m_holder.Rebuild ();

        }

    }
}
