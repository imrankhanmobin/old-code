//
// System.Windows.Forms.Design.ComponentEditorPage.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms.Design
{
	public abstract class ComponentEditorPage : Panel
	{
		private bool commitOnDeactivate = false;
		private IComponent component;
		private bool firstActivate = true;
		private Icon icon;
		private int loading = 0;
		private bool loadRequired = false;
		private IComponentEditorPageSite pageSite;

		public ComponentEditorPage ()
		{
		}

		public bool CommitOnDeactivate {
			get { return commitOnDeactivate; }
			set { commitOnDeactivate = value; }
		}

		protected IComponent Component {
			get { return component; }
			set { component = value; }
		}

		[MonoTODO ("Find out what this does.")]
		protected override CreateParams CreateParams {
			get {
				throw new NotImplementedException ();
			}
		}

		protected bool FirstActivate {
			get { return firstActivate; }
			set { firstActivate = value; }
		}

		public Icon Icon {
			get { return icon; }
			set { icon = value; }
		}

		protected int Loading {
			get { return loading; }
			set { loading = value; }
		}

		protected bool LoadRequired {
			get { return loadRequired; }
			set { loadRequired = value; }
		}

		protected IComponentEditorPageSite PageSite {
			get { return pageSite; }
			set { pageSite = value; }
		}

		public virtual string Title {
			get { return base.Text; }
		}

		public virtual void Activate ()
		{
			Visible = true;
			firstActivate = false;
			if (loadRequired) {
				EnterLoadingMode ();
				LoadComponent ();
				ExitLoadingMode ();
			}
		}

		public virtual void ApplyChanges ()
		{
			SaveComponent ();
		}

		public virtual void Deactivate ()
		{
			Visible = false;
		}

		protected void EnterLoadingMode ()
		{
			loading++;
		}

		protected void ExitLoadingMode ()
		{
			loading--;
		}

		public virtual Control GetControl ()
		{
			return this;
		}

		protected IComponent GetSelectedComponent ()
		{
			return component;
		}

		protected bool IsFirstActivate ()
		{
			return firstActivate;
		}

		protected bool IsLoading ()
		{
			return (loading != 0);
		}

		public virtual bool IsPageMessage (ref Message msg)
		{
			return PreProcessMessage (ref msg);
		}

		protected abstract void LoadComponent ();

		[MonoTODO ("Find out what this does.")]
		public virtual void OnApplyComplete ()
		{
		}

		protected virtual void ReloadComponent ()
		{
			loadRequired = true;
		}

		protected abstract void SaveComponent ();

		public virtual void SetComponent (IComponent component)
		{
			this.component = component;
			ReloadComponent ();
		}

		[MonoTODO ("Find out what this does.")]
		protected virtual void SetDirty ()
		{
		}

		public virtual void SetSite (IComponentEditorPageSite site)
		{
			pageSite = site;
			pageSite.GetControl ().Controls.Add (this);

		}

		public virtual void ShowHelp ()
		{
		}

		public virtual bool SupportsHelp ()
		{
			return false;
		}
	}
}