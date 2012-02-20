﻿using System;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;

namespace uComponents.Core.DataTypes.EnumDropDownList
{
	/// <summary>
	/// Enum DropDownList Data Type
	/// </summary>
	public class EnumDropDownListDataEditor : CompositeControl, IDataEditor
	{
		/// <summary>
		/// Field for the data.
		/// </summary>
		private IData data;

		/// <summary>
		/// Field for the options.
		/// </summary>
		private EnumDropDownListOptions options;

		/// <summary>
		/// Field for the CustomValidator.
		/// </summary>
		private CustomValidator customValidator = new CustomValidator();

		/// <summary>
		/// Field for the DropDownList.
		/// </summary>
		private DropDownList dropDownList = new DropDownList();

		/// <summary>
		/// Gets a value indicating whether [treat as rich text editor].
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [treat as rich text editor]; otherwise, <c>false</c>.
		/// </value>
		public virtual bool TreatAsRichTextEditor
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether [show label].
		/// </summary>
		/// <value><c>true</c> if [show label]; otherwise, <c>false</c>.</value>
		public virtual bool ShowLabel
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Gets the editor.
		/// </summary>
		/// <value>The editor.</value>
		public Control Editor
		{
			get
			{
				return this;
			}
		}

		/// <summary>
		/// Initializes a new instance of EnumCheckBoxListDataEditor
		/// </summary>
		/// <param name="data"></param>
		/// <param name="options"></param>
		internal EnumDropDownListDataEditor(IData data, EnumDropDownListOptions options)
		{
			this.data = data;
			this.options = options;
		}

		/// <summary>
		/// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
		/// </summary>
		protected override void CreateChildControls()
		{
			AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(this.CurrentDomain_ReflectionOnlyAssemblyResolve);

			FieldInfo fieldInfo;
			ListItem dropDownListItem;

			try
			{
				Assembly assembly = Assembly.ReflectionOnlyLoadFrom(MapPathSecure("~/bin/" + this.options.Assembly));
				Type type = assembly.GetType(this.options.Enum);

				// Loop though enum to create drop down list items
				foreach (string name in Enum.GetNames(type))
				{
					dropDownListItem = new ListItem(name, name); // Default to the enum item name

					fieldInfo = type.GetField(name);

					// Loop though any custom attributes that may have been applied the the curent enum item
					foreach (CustomAttributeData customAttributeData in CustomAttributeData.GetCustomAttributes(fieldInfo))
					{
						if (customAttributeData.Constructor.DeclaringType.Name == "EnumDropDownListAttribute")
						{
							// Loop though each property on the EnumDropDownListAttribute
							foreach (CustomAttributeNamedArgument customAttributeNamedArguement in customAttributeData.NamedArguments)
							{
								switch (customAttributeNamedArguement.MemberInfo.Name)
								{
									case "Text":
										dropDownListItem.Text = customAttributeNamedArguement.TypedValue.Value.ToString();
										break;

									case "Value":
										dropDownListItem.Value = customAttributeNamedArguement.TypedValue.Value.ToString();
										break;

									case "Enabled":
										dropDownListItem.Enabled = (bool)customAttributeNamedArguement.TypedValue.Value;
										break;
								}
							}
						}
					}

					this.dropDownList.Items.Add(dropDownListItem);
				}
			}
			catch
			{
			}

			// Add a default please select value
			this.dropDownList.Items.Insert(0, new ListItem(string.Concat(ui.Text("choose"), "..."), "-1"));

			this.Controls.Add(this.customValidator);
			this.Controls.Add(this.dropDownList);
		}

		private Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
		{
			return Assembly.ReflectionOnlyLoad(args.Name);
		}

		/// <summary>
		/// Raises the <see cref="E:System.Web.UI.Control.Load"/> event.
		/// </summary>
		/// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.EnsureChildControls();

			if (!this.Page.IsPostBack)
			{
				// Get selected items from Node Name or Node Id
				var dropDownListItem = this.dropDownList.Items.FindByValue(this.data.Value.ToString());
				if (dropDownListItem != null)
				{
					dropDownListItem.Selected = true;
				}
			}
		}

		/// <summary>
		/// Called by Umbraco when saving the node
		/// </summary>
		public void Save()
		{
			Property property = new Property(((DefaultData)this.data).PropertyId);
			if (property.PropertyType.Mandatory && this.dropDownList.SelectedValue == "-1")
			{
				// Property is mandatory, but no value selected in the DropDownList
				this.customValidator.IsValid = false;

				DocumentType documentType = new DocumentType(property.PropertyType.ContentTypeId);
				ContentType.TabI tab = documentType.getVirtualTabs.Where(x => x.Id == property.PropertyType.TabId).FirstOrDefault();

				if (tab != null)
				{
					this.customValidator.ErrorMessage = ui.Text("errorHandling", "errorMandatory", new string[] { property.PropertyType.Alias, tab.Caption }, User.GetCurrent());
				}
			}

			this.data.Value = this.dropDownList.SelectedValue;
		}
	}
}