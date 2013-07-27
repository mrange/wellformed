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
using System.Windows.Controls;
using WellFormed2.Core;
using WellFormed2.WPF.Internal.Source.Extensions;

namespace WellFormed2.WPF.Internal
{
    sealed partial class VisualText : Visuals.IText
    {
        public readonly TextBox TextBox;

        public VisualText()
        {
            TextBox = new TextBox ();
        }

        public string Text
        {
            get { return TextBox.Text; }
            set { TextBox.Text = value ?? ""; }
        }

        public object Visual
        {
            get
            {
                return TextBox;
            }
        }
    }


    sealed partial class FormletRebuildContext : Types.IFormletRebuildContext
    {
        readonly Dictionary<Type, Func<object>> m_creators = new Dictionary<Type, Func<object>> ();

        void AddCreator<T> (Func<object> creator)
            where T : class
        {
            if (creator == null)
            {
                return;
            }

            m_creators[typeof(T)] = creator;
        }

        void AddCreator<T, TToCreate> ()
            where T : class
            where TToCreate : T, new 
        {
            AddCreator<T> (() => new TToCreate ());
        }

        public FormletRebuildContext ()
        {
            AddCreator<Visuals.IText, VisualText> ();
            
        }

        public T CreateInstance<T>()
        {
            var creator = m_creators.Lookup (typeof(T));

            if (creator == null)
            {
                return default (T);
            }

            return (T) creator ();
        }
    }
}