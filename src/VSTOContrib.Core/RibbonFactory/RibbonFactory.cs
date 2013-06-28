﻿using System;
using System.Runtime.InteropServices;
using Microsoft.Office.Core;
using Microsoft.Office.Tools;
using VSTOContrib.Core.RibbonFactory.Interfaces;
using stdole;

namespace VSTOContrib.Core.RibbonFactory
{
    /// <summary>
    /// Simplifies adding custom Ribbon's to Office. 
    /// Allows the custom Ribbon xml to be wired up to IRibbonViewModel's
    /// by convention. Simply name the Ribbon.xml the same as the ribbon view model class
    /// in the same assembly
    /// </summary>
    [ComVisible(true)]
    public abstract class RibbonFactory : IRibbonFactory
    {
        internal const string CommonCallbacks = "CommonCallbacks";
        readonly IRibbonFactoryController ribbonFactoryController;
        static readonly object InstanceLock = new object();

        bool initialsed;

        /// <summary>
        /// Initializes a new instance of the <see cref="RibbonFactory"/> class.
        /// </summary>
        /// <param name="ribbonFactoryController"></param>
        protected RibbonFactory(IRibbonFactoryController ribbonFactoryController)
        {
            lock (InstanceLock)
            {
                if (Current != null)
                    throw new InvalidOperationException("You can only create a single ribbon factory");
                Current = this;
            }

            this.ribbonFactoryController = ribbonFactoryController;
        }

        /// <summary>
        /// Initialises and builds up the ribbon factory
        /// </summary>
        /// <param name="customTaskPaneCollection">The custom task pane collection.</param>
        /// <returns>
        /// Disposible object to call on outlook shutdown
        /// </returns>
        /// <exception cref="ViewNotFoundException">If the view cannot be located for a view model</exception>
        public abstract IDisposable InitialiseFactory(
            CustomTaskPaneCollection customTaskPaneCollection);

        /// <summary>
        /// Initialises the factory internal.
        /// </summary>
        /// <typeparam name="TRibbonTypes">The type of the ribbon types.</typeparam>
        /// <param name="viewProvider">The view provider.</param>
        /// <returns></returns>
        protected IDisposable InitialiseFactoryInternal<TRibbonTypes>(
            IViewProvider<TRibbonTypes> viewProvider) where TRibbonTypes : struct
        {
            if (initialsed)
                throw new InvalidOperationException("Ribbon Factory already Initialised");

            initialsed = true;

            return ribbonFactoryController.Initialise(
                viewProvider);
        }

        ///<summary>
        /// Gets or Sets the strategy that fetches the Ribbon XML for a given view
        ///</summary>
        public IViewLocationStrategy LocateViewStrategy
        {
            get { return ribbonFactoryController.LocateViewStrategy; }
            set
            {
                if (value == null) return;

                ribbonFactoryController.LocateViewStrategy = value;
            }
        }

        /// <summary>
        /// Current instance of RibbonFactory
        /// </summary>
        public static IRibbonFactory Current { get; protected set; }

        /// <summary>
        /// Ribbon_s the load.
        /// </summary>
        /// <param name="ribbonUi">The ribbon UI.</param>
        // ReSharper disable InconsistentNaming
        public virtual void Ribbon_Load(IRibbonUI ribbonUi)
        {
            ribbonFactoryController.RibbonLoaded(ribbonUi);
        }
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Gets the custom UI.
        /// </summary>
        /// <param name="ribbonId">The ribbon id.</param>
        /// <returns></returns>
        public string GetCustomUI(string ribbonId)
        {
            return ribbonFactoryController.GetCustomUI(ribbonId);
        }


        /***************************************************/
        /*                                                 */
        /*                   Callbacks                     */
        /*                                                 */
        /***************************************************/

        /// <summary>
        /// button onAction callback
        /// </summary>
        /// <param name="control"></param>
        public void OnAction(IRibbonControl control)
        {
            ribbonFactoryController.Invoke(control, () => OnAction(null));
        }

        /// <summary>
        /// dropDown and gallery onAction callback
        /// </summary>
        /// <param name="control"></param>
        /// <param name="selectedId"></param>
        /// <param name="selectedIndex"></param>
        public void SelectionOnAction(IRibbonControl control, string selectedId, int selectedIndex)
        {
            ribbonFactoryController.Invoke(control, () => SelectionOnAction(null, null, 0), selectedId, selectedIndex);
        }

        /// <summary>
        /// checkBox and togglebutton onAction callback
        /// </summary>
        /// <param name="control"></param>
        /// <param name="pressed"></param>
        public void PressedOnAction(IRibbonControl control, bool pressed)
        {
            ribbonFactoryController.Invoke(control, () => PressedOnAction(null, true), pressed);
        }

        /// <summary>
        /// GetDescription callback
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public string GetDescription(IRibbonControl control)
        {
            return (string)ribbonFactoryController.InvokeGet(control, () => GetDescription(null));
        }

        /// <summary>
        /// GetEnabled callback
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public bool GetEnabled(IRibbonControl control)
        {
            return (bool)ribbonFactoryController.InvokeGet(control, () => GetEnabled(null));
        }

        /// <summary>
        /// GetImageMso callback
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public string GetImageMso(IRibbonControl control)
        {
            return (string)ribbonFactoryController.InvokeGet(control, () => GetImageMso(null));
        }

        /// <summary>
        /// GetLabel callback
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public string GetLabel(IRibbonControl control)
        {
            return (string)ribbonFactoryController.InvokeGet(control, () => GetLabel(null));
        }

        /// <summary>
        /// GetKeyTip callback
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public string GetKeyTip(IRibbonControl control)
        {
            return (string)ribbonFactoryController.InvokeGet(control, () => GetKeyTip(null));
        }

        /// <summary>
        /// GetScreenTip
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public string GetScreenTip(IRibbonControl control)
        {
            return (string)ribbonFactoryController.InvokeGet(control, () => GetScreenTip(null));
        }

        /// <summary>
        /// GetSuperTip
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public string GetSuperTip(IRibbonControl control)
        {
            return (string)ribbonFactoryController.InvokeGet(control, () => GetSuperTip(null));
        }

        /// <summary>
        /// GetVisible callback
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public bool GetVisible(IRibbonControl control)
        {
            return (bool)ribbonFactoryController.InvokeGet(control, () => GetVisible(null));
        }

        /// <summary>
        /// GetShowImage callback
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public bool GetShowImage(IRibbonControl control)
        {
            return (bool)ribbonFactoryController.InvokeGet(control, () => GetShowImage(null));
        }

        /// <summary>
        /// GetShowLabel
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public bool GetShowLabel(IRibbonControl control)
        {
            return (bool)ribbonFactoryController.InvokeGet(control, () => GetShowLabel(null));
        }

        /// <summary>
        /// GetItemCount callback
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public int GetItemCount(IRibbonControl control)
        {
            return (int)ribbonFactoryController.InvokeGet(control, () => GetItemCount(null));
        }

        /// <summary>
        /// GetItemId callback
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public string GetItemId(IRibbonControl control, int index)
        {
            return (string)ribbonFactoryController.InvokeGet(control, () => GetItemId(null, 0));
        }

        /// <summary>
        /// GetItemLabel callback
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public string GetItemLabel(IRibbonControl control, int index)
        {
            return (string)ribbonFactoryController.InvokeGet(control, () => GetItemLabel(null, 0));
        }

        /// <summary>
        /// GetItemScreenTip callback
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public string GetItemScreenTip(IRibbonControl control, int index)
        {
            return (string)ribbonFactoryController.InvokeGet(control, () => GetItemScreenTip(null, 0));
        }

        /// <summary>
        /// GetItemSuperTip callback
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public string GetItemSuperTip(IRibbonControl control, int index)
        {
            return (string)ribbonFactoryController.InvokeGet(control, () => GetItemSuperTip(null, 0));
        }

        /// <summary>
        /// GetSelectedItemId callback
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public string GetSelectedItemId(IRibbonControl control)
        {
            return (string)ribbonFactoryController.InvokeGet(control, () => GetSelectedItemId(null));
        }

        /// <summary>
        /// GetSelectedItemIndex callback
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public int GetSelectedItemIndex(IRibbonControl control)
        {
            return (int)ribbonFactoryController.InvokeGet(control, () => GetSelectedItemIndex(null));
        }

        /// <summary>
        /// GetContent callback
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public string GetContent(IRibbonControl control)
        {
            return (string)ribbonFactoryController.InvokeGet(control, () => GetContent(null));
        }

        /// <summary>
        /// GetText callback
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public string GetText(IRibbonControl control)
        {
            return (string)ribbonFactoryController.InvokeGet(control, () => GetText(null));
        }

        /// <summary>
        /// GetTitle callback
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public string GetTitle(IRibbonControl control)
        {
            return (string)ribbonFactoryController.InvokeGet(control, () => GetTitle(null));
        }

        /// <summary>
        /// GetPressed callback
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public bool GetPressed(IRibbonControl control)
        {
            return (bool)ribbonFactoryController.InvokeGet(control, () => GetPressed(null));
        }

        /// <summary>
        /// GetSize callback
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public RibbonControlSize GetSize(IRibbonControl control)
        {
            return (RibbonControlSize)ribbonFactoryController.InvokeGet(control, () => GetSize(null));
        }

        /// <summary>
        /// GetItemHeight
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public int GetItemHeight(IRibbonControl control)
        {
            return (int)ribbonFactoryController.InvokeGet(control, () => GetItemHeight(control));
        }

        /// <summary>
        /// GetImage
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public IPictureDisp GetImage(IRibbonControl control)
        {
            return (IPictureDisp)ribbonFactoryController.InvokeGet(control, () => GetImage(null));
        }

        /// <summary>
        /// GetItemImage
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public IPictureDisp GetItemImage(IRibbonControl control, int index)
        {
            return (IPictureDisp)ribbonFactoryController.InvokeGet(control, () => GetItemImage(null, 0), index);
        }

        /// <summary>
        /// OnTextChanged callback
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="text">The text.</param>
        public void OnTextChanged(IRibbonControl control, string text)
        {
            ribbonFactoryController.Invoke(control, () => OnTextChanged(null, null), text);
        }
    }
}
