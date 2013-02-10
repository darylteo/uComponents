﻿// --------------------------------------------------------------------------------------------------------------------
// <summary>
// 09.02.2013 - Created [Ove Andersen]
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace uComponents.DataTypes.DataTypeGrid.Interfaces
{
    using System.Web.UI;

    using uComponents.DataTypes.DataTypeGrid.Model;
    using uComponents.DataTypes.DataTypeGrid.ServiceLocators;

    using umbraco.interfaces;

    /// <summary>
    /// Interface for the <see cref="DataTypeFactoryServiceLocator"/>
    /// </summary>
    internal interface IDataTypeFactoryServiceLocator
    {
        /// <summary>
        /// Method for customizing the way the <see cref="IDataType"/> value is displayed in the grid.
        /// </summary>
        /// <remarks>Called when the grid displays the cell value for the specified <see cref="IDataType"/>.</remarks>
        /// <param name="dataType">The <see cref="IDataType"/> instance.</param>
        /// <returns>The display value.</returns>
        string GetDisplayValue(IDataType dataType);

        /// <summary>
        /// Method for getting the backing object for the specified <see cref="IDataType" />.
        /// </summary>
        /// <param name="dataType">The <see cref="IDataType" /> instance.</param>
        /// <returns>The backing object.</returns>
        /// <remarks>Called when the method <see cref="GridCell.GetObject{T}()" /> method is called on a <see cref="GridCell" />.</remarks>
        object GetObject(IDataType dataType);

        /// <summary>
        /// Method for getting the backing object for the specified <see cref="IDataType"/>.
        /// </summary>
        /// <remarks>Called when the method <see cref="GridCell.GetObject{T}()"/> method is called on a <see cref="GridCell"/>.</remarks>
        /// <typeparam name="TBackingObject">The backing type for the specified <see cref="IDataType"/>.</typeparam>
        /// <param name="dataType">The <see cref="IDataType"/> instance.</param>
        /// <returns>The backing object.</returns>
        TBackingObject GetObject<TBackingObject>(IDataType dataType);

        /// <summary>
        /// Method for performing special actions while creating the <see cref="IDataType"/> editor.
        /// </summary>
        /// <remarks>Called when the grid creates the editor controls for the specified <see cref="IDataType"/>.</remarks>
        /// <param name="dataType">The <see cref="IDataType"/> instance.</param>
        /// <param name="container">The editor control container.</param>
        void Configure(IDataType dataType, Control container);

        /// <summary>
        /// Method for executing special actions before saving the editor value to the database.
        /// </summary>
        /// <remarks>Called when the grid is saved for the specified <see cref="IDataType"/>.</remarks>
        /// <param name="dataType">The <see cref="IDataType"/> instance.</param>
        void Save(IDataType dataType);
    }
}