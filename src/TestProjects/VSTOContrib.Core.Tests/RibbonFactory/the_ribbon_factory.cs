﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Office.Core;
using Microsoft.Office.Tools;
using NSubstitute;
using VSTOContrib.Core.RibbonFactory;
using VSTOContrib.Core.RibbonFactory.Interfaces;
using VSTOContrib.Core.Tests.RibbonFactory.TestStubs;
using Xunit;
using VSTOContrib.Testing;

namespace VSTOContrib.Core.Tests.RibbonFactory
{
    public class the_ribbon_factory : IDisposable
    {
        private readonly IViewProvider<TestRibbonTypes> viewProvider;
        private readonly TestRibbonFactory ribbonFactoryUnderTest;
        TestRibbonViewModel viewModel;
        public the_ribbon_factory()
        {
            viewProvider = Substitute.For<IViewProvider<TestRibbonTypes>>();
            ribbonFactoryUnderTest = new TestRibbonFactory(
                t => viewModel = (TestRibbonViewModel)Activator.CreateInstance(t),
                new Lazy<CustomTaskPaneCollection>(() => Substitute.For<CustomTaskPaneCollection>()),
                viewProvider,
                new TestContextProvider(),
                Assembly.GetExecutingAssembly());
        }

        [Fact]
        public void cannot_create_multiple_instances()
        {
            Assert.Throws<InvalidOperationException>(() => new TestRibbonFactory(
                t => (IRibbonViewModel)Activator.CreateInstance(t),
                new Lazy<CustomTaskPaneCollection>(() => Substitute.For<CustomTaskPaneCollection>()),
                viewProvider, new TestContextProvider()));
        }

        [Fact]
        public void cannot_initialise_twice()
        {
            ribbonFactoryUnderTest.InitialiseFactory(OfficeObjectMother.CreateCustomTaskPaneCollection());

            Assert.Throws<InvalidOperationException>(() => ribbonFactoryUnderTest.InitialiseFactory(
                OfficeObjectMother.CreateCustomTaskPaneCollection()));
        }

        [Fact]
        public void default_constructor_uses_default_view_model_locator()
        {
            Assert.IsType<DefaultViewLocationStrategy>(ribbonFactoryUnderTest.LocateViewStrategy);
        }

        [Fact]
        public void initialise_throws_when_no_assemblies_specified_to_scan()
        {
            Assert.Throws<InvalidOperationException>(() => new TestRibbonFactory(t => (IRibbonViewModel)Activator.CreateInstance(t),
                new Lazy<CustomTaskPaneCollection>(() => Substitute.For<CustomTaskPaneCollection>()),
                viewProvider, new TestContextProvider()));
        }

        [Fact]
        public void resolves_associated_view_for_viewmodel()
        {
            ribbonFactoryUnderTest.InitialiseFactory(
                OfficeObjectMother.CreateCustomTaskPaneCollection());

            var customUI1 = ribbonFactoryUnderTest.GetCustomUI(TestRibbonTypes.RibbonType1.GetEnumDescription());
            var customUI2 = ribbonFactoryUnderTest.GetCustomUI(TestRibbonTypes.RibbonType2.GetEnumDescription());
            Assert.Contains("view1", customUI1);
            Assert.Contains("view2", customUI2);
        }

        [Fact]
        public void ribbon_xml_callbacks_modified_to_ribbon_factory_callbacks_for_toggle_button()
        {
            // arrange
            ribbonFactoryUnderTest.InitialiseFactory(
                OfficeObjectMother.CreateCustomTaskPaneCollection());

            // act
            var processedRibbon = ribbonFactoryUnderTest.GetCustomUI(TestRibbonTypes.RibbonType1.GetEnumDescription());

            // assert
            Assert.Contains("onAction=\"PressedOnAction\"", processedRibbon);
            Assert.Contains("getPressed=\"GetPressed\"", processedRibbon);
        }

        [Fact]
        public void ribbon_xml_callbacks_modified_to_ribbon_factory_callbacks_for_button()
        {
            // arrange
            ribbonFactoryUnderTest.InitialiseFactory(
                OfficeObjectMother.CreateCustomTaskPaneCollection());

            // act
            var processedRibbon = ribbonFactoryUnderTest.GetCustomUI(TestRibbonTypes.RibbonType1.GetEnumDescription());

            // assert
            Assert.Contains("onAction=\"OnAction\"", processedRibbon);
            Assert.Contains("getEnabled=\"GetEnabled\"", processedRibbon);
        }

        [Fact]
        public void toggle_button_is_bound_to_property_get()
        {
            // arrange
            ribbonFactoryUnderTest.InitialiseFactory(
                OfficeObjectMother.CreateCustomTaskPaneCollection());
            var processedRibbon = ribbonFactoryUnderTest.GetCustomUI(TestRibbonTypes.RibbonType1.GetEnumDescription());
            //Open new view to create a viewmodel for view
            var viewInstance = new TestWindow { Context = new TestWindowContext() };
            viewProvider.NewView += Raise.EventWith(viewProvider, new NewViewEventArgs<TestRibbonTypes>(
                viewInstance, viewInstance.Context, TestRibbonTypes.RibbonType1));
            viewModel.PanelShown = true;
            var toggleButtonTag = GetTag(processedRibbon, "testTogglePanelButton");

            // act
            var ribbonControl = GetRibbonControl("testTogglePanelButton", toggleButtonTag, viewInstance);
            var isPressed = ribbonFactoryUnderTest.GetPressed(ribbonControl);

            // assert
            Assert.True(isPressed);
        }

        [Fact]
        public void toggle_button_is_bound_to_property_set()
        {
            // arrange
            ribbonFactoryUnderTest.InitialiseFactory(
                OfficeObjectMother.CreateCustomTaskPaneCollection());
            var processedRibbon = ribbonFactoryUnderTest.GetCustomUI(TestRibbonTypes.RibbonType1.GetEnumDescription());
            //Open new view to create a viewmodel for view
            var viewInstance = new TestWindow { Context = new TestWindowContext() };
            viewProvider.NewView += Raise.EventWith(viewProvider, new NewViewEventArgs<TestRibbonTypes>(
                viewInstance, viewInstance.Context, TestRibbonTypes.RibbonType1));
            viewModel.PanelShown = true;
            var toggleButtonTag = GetTag(processedRibbon, "testTogglePanelButton");

            // act
            var ribbonControl = GetRibbonControl("testTogglePanelButton", toggleButtonTag, viewInstance);
            ribbonFactoryUnderTest.PressedOnAction(ribbonControl, false);

            // assert
            Assert.False(viewModel.PanelShown);
        }

        [Fact]
        public void toggle_button_is_bound_to_property_listens_to_property_changed_events()
        {
            // arrange
            ribbonFactoryUnderTest.InitialiseFactory(
                OfficeObjectMother.CreateCustomTaskPaneCollection());
            //Open new view to create a viewmodel for view
            var viewInstance = new TestWindow { Context = new TestWindowContext() };
            viewProvider.NewView += Raise.EventWith(viewProvider, new NewViewEventArgs<TestRibbonTypes>(
                viewInstance, viewInstance.Context, TestRibbonTypes.RibbonType1));
            var ribbon = Substitute.For<IRibbonUI>();
            ribbonFactoryUnderTest.Ribbon_Load(ribbon);

            // act
            viewModel.OnPropertyChanged(new PropertyChangedEventArgs("PanelShown"));

            // assert
            ribbon.Received().InvalidateControl("testTogglePanelButton");
        }

        [Fact]
        public void ribbon_xml_getenabled_can_bind_to_method()
        {
            // arrange
            ribbonFactoryUnderTest.InitialiseFactory(
                OfficeObjectMother.CreateCustomTaskPaneCollection());
            var processedRibbon = ribbonFactoryUnderTest.GetCustomUI(TestRibbonTypes.RibbonType1.GetEnumDescription());
            //Open new view to create a viewmodel for view
            var viewInstance = new TestWindow
                                   {
                                       Context = new TestWindowContext()
                                   };
            viewProvider.NewView += Raise.EventWith(viewProvider, new NewViewEventArgs<TestRibbonTypes>(
                viewInstance, viewInstance.Context, TestRibbonTypes.RibbonType1));
            viewModel.PanelShown = true;
            var buttonTag = GetTag(processedRibbon, "actionButton");

            // act
            var ribbonControl = GetRibbonControl("actionButton", buttonTag, viewInstance);
            var isEnabled = ribbonFactoryUnderTest.GetEnabled(ribbonControl);

            // assert
            Assert.True(isEnabled);
        }

        [Fact]
        public void ribbon_factory_calls_back_to_correct_view_model()
        {
            // arrange
            var viewModels = new List<TestRibbonViewModel>();
            ribbonFactoryUnderTest.ClearCurrent();
            var ribbonFactory = new TestRibbonFactory(
                t =>
                {
                    var testRibbon = (TestRibbonViewModel)Activator.CreateInstance(t);
                    viewModels.Add(testRibbon);
                    return testRibbon;
                },
                new Lazy<CustomTaskPaneCollection>(() => Substitute.For<CustomTaskPaneCollection>()),
                viewProvider,
                new TestContextProvider(), Assembly.GetExecutingAssembly());
            ribbonFactory.InitialiseFactory(OfficeObjectMother.CreateCustomTaskPaneCollection());
            var processedRibbon = ribbonFactory.GetCustomUI(TestRibbonTypes.RibbonType1.GetEnumDescription());
            //Open new view to create a viewmodel for view
            var viewInstance = new TestWindow { Context = new TestWindowContext() };
            var view2Instance = new TestWindow { Context = new TestWindowContext() };
            viewProvider.NewView += Raise.EventWith(viewProvider, new NewViewEventArgs<TestRibbonTypes>(
                viewInstance, viewInstance.Context, TestRibbonTypes.RibbonType1));
            viewProvider.NewView += Raise.EventWith(viewProvider, new NewViewEventArgs<TestRibbonTypes>(
                view2Instance, view2Instance.Context, TestRibbonTypes.RibbonType1));
            var buttonTag = GetTag(processedRibbon, "testTogglePanelButton");

            // act
            viewModels[1].PanelShown = true;
            var ribbonControl = GetRibbonControl("testTogglePanelButton", buttonTag, viewInstance);
            var ribbon2Control = GetRibbonControl("testTogglePanelButton", buttonTag, view2Instance);
            var isPressed = ribbonFactory.GetPressed(ribbonControl);
            var is2Pressed = ribbonFactory.GetPressed(ribbon2Control);

            // assert
            Assert.False(isPressed);
            Assert.True(is2Pressed);
        }

        [Fact]
        public void new_window_with_same_context_does_not_create_new_viewmodel()
        {
            // arrange
            var viewModels = new List<TestRibbonViewModel>();
            ribbonFactoryUnderTest.ClearCurrent();
            var ribbonFactory = new TestRibbonFactory(
                t =>
                {
                    var testRibbon = (TestRibbonViewModel)Activator.CreateInstance(t);
                    viewModels.Add(testRibbon);
                    return testRibbon;
                },
                new Lazy<CustomTaskPaneCollection>(() => Substitute.For<CustomTaskPaneCollection>()),
                viewProvider, new TestContextProvider(), Assembly.GetExecutingAssembly());
            ribbonFactory.InitialiseFactory(OfficeObjectMother.CreateCustomTaskPaneCollection());
            //Open new view to create a viewmodel for view
            var viewInstance = new TestWindow { Context = new TestWindowContext() };
            var view2Instance = new TestWindow { Context = new TestWindowContext() };

            // act

            viewProvider.NewView += Raise.EventWith(viewProvider, new NewViewEventArgs<TestRibbonTypes>(
                                                                        viewInstance, viewInstance.Context,
                                                                        TestRibbonTypes.RibbonType1));
            viewProvider.NewView += Raise.EventWith(viewProvider, new NewViewEventArgs<TestRibbonTypes>(
                                                                        view2Instance, viewInstance.Context,
                                                                        TestRibbonTypes.RibbonType1));

            // assert
            Assert.Equal(1, viewModels.Count);
        }

        [Fact]
        public void new_window_with_different_context_does_not_create_new_viewmodel()
        {
            // arrange
            ribbonFactoryUnderTest.ClearCurrent();
            var viewModels = new List<TestRibbonViewModel>();
            var ribbonFactory = new TestRibbonFactory(
                t =>
                {
                    var testRibbon = (TestRibbonViewModel)Activator.CreateInstance(t);
                    viewModels.Add(testRibbon);
                    return testRibbon;
                },
                new Lazy<CustomTaskPaneCollection>(() => Substitute.For<CustomTaskPaneCollection>()),
                viewProvider, new TestContextProvider(), Assembly.GetExecutingAssembly());
            ribbonFactory.InitialiseFactory(OfficeObjectMother.CreateCustomTaskPaneCollection());
            //Open new view to create a viewmodel for view
            var viewInstance = new TestWindow { Context = new TestWindowContext() };
            var view2Instance = new TestWindow { Context = new TestWindowContext() };

            // act

            viewProvider.NewView += Raise.EventWith(viewProvider, new NewViewEventArgs<TestRibbonTypes>(
                                                                        viewInstance, viewInstance.Context,
                                                                        TestRibbonTypes.RibbonType1));
            viewProvider.NewView += Raise.EventWith(viewProvider, new NewViewEventArgs<TestRibbonTypes>(
                                                                        view2Instance, view2Instance.Context,
                                                                        TestRibbonTypes.RibbonType1));

            // assert
            Assert.Equal(2, viewModels.Count);
        }

        private static string GetTag(string ribbonXml, string controlId)
        {
            var tagExpression = new Regex("\\<.*? id=\\\"" + controlId + "\\\".*?tag=\\\"(.*?)\\\"");
            return tagExpression.Match(ribbonXml).Groups[1].Value;
        }

        private static IRibbonControl GetRibbonControl(string id, string tag, object view)
        {
            var ribbonControl = Substitute.For<IRibbonControl>();
            ribbonControl.Id.Returns(id);
            ribbonControl.Tag.Returns(tag);
            ribbonControl.Context.Returns(view);
            return ribbonControl;
        }

        public void Dispose()
        {
            ribbonFactoryUnderTest.ClearCurrent();
        }
    }

    public class TestContextProvider : IViewContextProvider
    {
        public object GetContextForView(object view)
        {
            return ((TestWindow)view).Context;
        }

        public TRibbonType GetRibbonTypeForView<TRibbonType>(object view)
        {
            return (TRibbonType)(object)TestRibbonTypes.RibbonType1;
        }
    }

    public class TestWindowContext
    {
    }

    public class TestWindow
    {
        public TestWindowContext Context { get; set; }
    }
}
