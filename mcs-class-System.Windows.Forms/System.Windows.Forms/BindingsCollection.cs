//
// System.Windows.Forms.BindingsCollection.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002/3 Ximian, Inc
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents a collection of Binding objects for a control.
	///
	/// </summary>
	
	public class BindingsCollection : BaseCollection {

		#region Constructors
		protected internal BindingsCollection () 
		{
		}
		#endregion

		// --- public and protected Properties ---
		public override int Count {
			get {
				return base.Count;
			}
		}
		
		public Binding this[int index] {
			get {
				return (Binding)(base.List[index]);
			}
		}
		
		[MonoTODO]
		protected override ArrayList List {
			get {
				return base.List;
 }
		}
		
		// --- public Methods ---
		[MonoTODO]
		protected virtual void AddCore(Binding dataBinding) {
		}

		[MonoTODO]
		protected virtual void ClearCore(){
		}

		[MonoTODO]
		protected virtual void RemoveCore(Binding dataBinding){
		}

		// 
		// CollectionChanged event:
		// Though it was not documented, here methods Add and Remove 
		// cause the CollectionChanged event to occur, similarily as Clear.
		// Would be nice if someone checked the exact event behavior of .NET implementation.
		
		protected internal void Add(Binding binding) 
		{
			base.List.Add(binding);
			OnCollectionChanged(new CollectionChangeEventArgs(
				CollectionChangeAction.Add,
				base.List
			));
		}
		
		protected internal void Clear() 
		{
			base.List.Clear();
			OnCollectionChanged(new CollectionChangeEventArgs(
				CollectionChangeAction.Refresh,
				base.List
			));
		}

		protected virtual void OnCollectionChanged(CollectionChangeEventArgs ccevent) 
		{
			if (CollectionChanged != null)
				CollectionChanged(this, ccevent);
		}

		protected internal void Remove(Binding binding) 
		{
			base.List.Remove(binding);
			OnCollectionChanged(new CollectionChangeEventArgs(
				CollectionChangeAction.Remove,
				base.List
			));
		}

		protected internal void RemoveAt(int index) 
		{
			base.List.RemoveAt(index);
			OnCollectionChanged(new CollectionChangeEventArgs(
				CollectionChangeAction.Remove,
				base.List
			));
		}
		
		protected internal bool ShouldSerializeMyAll() 
		{
			if (this.Count>0) return true;
			else return false;
		}
		
		// public events
		public event CollectionChangeEventHandler CollectionChanged;
	}
}