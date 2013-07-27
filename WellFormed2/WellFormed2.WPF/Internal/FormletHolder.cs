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
using WellFormed2.Core;

namespace WellFormed2.WPF.Internal
{
    partial interface IFormletHolder
    {
        Types.FlatVisualTree Rebuild ();
        bool Submit ();
    }

    sealed partial class FormletHolder<T> : IFormletHolder
    {
        public readonly Types.IFormletRebuildContext    Context     ;
        public readonly Types.IFormlet<T>               Formlet     ;
        public readonly Action<T>                       Submitter   ;

        public          Types.IForm<T>                  Form        ;

        static readonly Types.LayoutOrientation DefaultOrientation = Types.LayoutOrientation.LeftToRight;

        public FormletHolder(Types.IFormletRebuildContext context, Types.IFormlet<T> formlet, Action<T> submitter)
        {
            Context     = context;
            Formlet     = formlet;
            Submitter   = submitter ?? (v => {});
        }

        public Types.FlatVisualTree Rebuild()
        {
            if (Formlet == null)
            {
                return Types.FlatVisualTree.NewLayout(DefaultOrientation, new Types.FlatVisualTree[0]);
            }

            Form = Formlet.Rebuild (Context, Form.ToOption ());

            var ctx = Types.FormRenderContext.New (DefaultOrientation);

            var vt = Form.Render (ctx);

            return Common.FlattenTree (DefaultOrientation, vt);
        }

        public bool Submit()
        {
            if (Form == null)
            {
                return false;
            }

            var collect = Form.Collect ();
            if (collect.Failures.Length > 0)
            {
                return false;
            }

            Submitter (collect.Value);

            return true;
        }
    }
}