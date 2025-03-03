﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class VisualStateManagerTests
	{
		const string NormalStateName = "Normal";
		const string InvalidStateName = "Invalid";
		const string FocusedStateName = "Focused";
		const string DisabledStateName = "Disabled";
		const string CommonStatesName = "CommonStates";

		static VisualStateGroupList CreateTestStateGroups()
		{
			var stateGroups = new VisualStateGroupList();
			var visualStateGroup = new VisualStateGroup { Name = CommonStatesName };
			var normalState = new VisualState { Name = NormalStateName };
			var invalidState = new VisualState { Name = InvalidStateName };
			var focusedState = new VisualState { Name = FocusedStateName };
			var disabledState = new VisualState { Name = DisabledStateName };

			visualStateGroup.States.Add(normalState);
			visualStateGroup.States.Add(invalidState);
			visualStateGroup.States.Add(focusedState);
			visualStateGroup.States.Add(disabledState);

			stateGroups.Add(visualStateGroup);

			return stateGroups;
		}

		static VisualStateGroupList CreateStateGroupsWithoutNormalState()
		{
			var stateGroups = new VisualStateGroupList();
			var visualStateGroup = new VisualStateGroup { Name = CommonStatesName };
			var invalidState = new VisualState { Name = InvalidStateName };

			visualStateGroup.States.Add(invalidState);

			stateGroups.Add(visualStateGroup);

			return stateGroups;
		}

		[Test]
		public void InitialStateIsNormalIfAvailable()
		{
			var label1 = new Label();

			VisualStateManager.SetVisualStateGroups(label1, CreateTestStateGroups());

			var groups1 = VisualStateManager.GetVisualStateGroups(label1);

			Assert.That(groups1[0].CurrentState.Name, Is.EqualTo(NormalStateName));
		}

		[Test]
		public void InitialStateIsNullIfNormalNotAvailable()
		{
			var label1 = new Label();

			VisualStateManager.SetVisualStateGroups(label1, CreateStateGroupsWithoutNormalState());

			var groups1 = VisualStateManager.GetVisualStateGroups(label1);

			Assert.Null(groups1[0].CurrentState);
		}

		[Test]
		public void VisualElementsStateGroupsAreDistinct()
		{
			var label1 = new Label();
			var label2 = new Label();

			VisualStateManager.SetVisualStateGroups(label1, CreateTestStateGroups());
			VisualStateManager.SetVisualStateGroups(label2, CreateTestStateGroups());

			var groups1 = VisualStateManager.GetVisualStateGroups(label1);
			var groups2 = VisualStateManager.GetVisualStateGroups(label2);

			Assert.AreNotSame(groups1, groups2);

			Assert.That(groups1[0].CurrentState.Name, Is.EqualTo(NormalStateName));
			Assert.That(groups2[0].CurrentState.Name, Is.EqualTo(NormalStateName));

			VisualStateManager.GoToState(label1, InvalidStateName);

			Assert.That(groups1[0].CurrentState.Name, Is.EqualTo(InvalidStateName));
			Assert.That(groups2[0].CurrentState.Name, Is.EqualTo(NormalStateName));
		}

		[Test]
		public void VisualStateGroupsFromSettersAreDistinct()
		{
			var x = new Setter();
			x.Property = VisualStateManager.VisualStateGroupsProperty;
			x.Value = CreateTestStateGroups();

			var label1 = new Label();
			var label2 = new Label();

			x.Apply(label1);
			x.Apply(label2);

			var groups1 = VisualStateManager.GetVisualStateGroups(label1);
			var groups2 = VisualStateManager.GetVisualStateGroups(label2);

			Assert.NotNull(groups1);
			Assert.NotNull(groups2);

			Assert.AreNotSame(groups1, groups2);

			Assert.That(groups1[0].CurrentState.Name, Is.EqualTo(NormalStateName));
			Assert.That(groups2[0].CurrentState.Name, Is.EqualTo(NormalStateName));

			VisualStateManager.GoToState(label1, InvalidStateName);

			Assert.That(groups1[0].CurrentState.Name, Is.EqualTo(InvalidStateName));
			Assert.That(groups2[0].CurrentState.Name, Is.EqualTo(NormalStateName));
		}

		[Test]
		public void ElementsDoNotHaveVisualStateGroupsCollectionByDefault()
		{
			var label1 = new Label();
			Assert.False(label1.HasVisualStateGroups());
			var vsg = VisualStateManager.GetVisualStateGroups(label1);
			Assert.False(label1.HasVisualStateGroups());
			vsg.Add(new VisualStateGroup());
			Assert.True(label1.HasVisualStateGroups());
		}

		[Test]
		public void StateNamesMustBeUniqueWithinGroup()
		{
			IList<VisualStateGroup> vsgs = CreateTestStateGroups();

			var duplicate = new VisualState { Name = NormalStateName };

			Assert.Throws<InvalidOperationException>(() => vsgs[0].States.Add(duplicate));
		}

		[Test]
		public void StateNamesMustBeUniqueWithinGroupList()
		{
			IList<VisualStateGroup> vsgs = CreateTestStateGroups();

			// Create and add a second VisualStateGroup
			var secondGroup = new VisualStateGroup { Name = "Foo" };
			vsgs.Add(secondGroup);

			// Create a VisualState with the same name as one in another group in this list
			var duplicate = new VisualState { Name = NormalStateName };

			Assert.Throws<InvalidOperationException>(() => secondGroup.States.Add(duplicate));
		}

		[Test]
		public void StateNamesMustBeUniqueWithinGroupListWhenAddingGroup()
		{
			IList<VisualStateGroup> vsgs = CreateTestStateGroups();

			// Create and add a second VisualStateGroup
			var secondGroup = new VisualStateGroup { Name = "Foo" };

			// Create a VisualState with the same name as one in another group in the list
			var duplicate = new VisualState { Name = NormalStateName };
			secondGroup.States.Add(duplicate);

			Assert.Throws<InvalidOperationException>(() => vsgs.Add(secondGroup));
		}

		[Test]
		public void GroupNamesMustBeUniqueWithinGroupList()
		{
			IList<VisualStateGroup> vsgs = CreateTestStateGroups();
			var secondGroup = new VisualStateGroup { Name = CommonStatesName };

			Assert.Throws<InvalidOperationException>(() => vsgs.Add(secondGroup));
		}

		[Test]
		public void StateNamesInGroupMayNotBeNull()
		{
			IList<VisualStateGroup> vsgs = CreateTestStateGroups();

			var nullStateName = new VisualState();

			Assert.Throws<InvalidOperationException>(() => vsgs[0].States.Add(nullStateName));
		}

		[Test]
		public void StateNamesInGroupMayNotBeEmpty()
		{
			IList<VisualStateGroup> vsgs = CreateTestStateGroups();

			var emptyStateName = new VisualState { Name = "" };

			Assert.Throws<InvalidOperationException>(() => vsgs[0].States.Add(emptyStateName));
		}

		[Test]
		public void VerifyVisualStateChanges()
		{
			var label1 = new Label();
			VisualStateManager.SetVisualStateGroups(label1, CreateTestStateGroups());

			var groups1 = VisualStateManager.GetVisualStateGroups(label1);
			Assert.That(groups1[0].CurrentState.Name, Is.EqualTo(NormalStateName));

			label1.IsEnabled = false;

			groups1 = VisualStateManager.GetVisualStateGroups(label1);
			Assert.That(groups1[0].CurrentState.Name, Is.EqualTo(DisabledStateName));


			label1.SetValue(VisualElement.IsFocusedPropertyKey, true);
			groups1 = VisualStateManager.GetVisualStateGroups(label1);
			Assert.That(groups1[0].CurrentState.Name, Is.EqualTo(DisabledStateName));

			label1.IsEnabled = true;
			groups1 = VisualStateManager.GetVisualStateGroups(label1);
			Assert.That(groups1[0].CurrentState.Name, Is.EqualTo(FocusedStateName));


			label1.SetValue(VisualElement.IsFocusedPropertyKey, false);
			groups1 = VisualStateManager.GetVisualStateGroups(label1);
			Assert.That(groups1[0].CurrentState.Name, Is.EqualTo(NormalStateName));

		}

		[Test]
		public void VisualElementGoesToCorrectStateWhenAvailable()
		{
			var label = new Label();
			double targetBottomMargin = 1.5;

			var group = new VisualStateGroup();
			var list = new VisualStateGroupList();

			var normalState = new VisualState { Name = NormalStateName };
			normalState.Setters.Add(new Setter { Property = View.MarginBottomProperty, Value = targetBottomMargin });

			list.Add(group);
			group.States.Add(normalState);

			VisualStateManager.SetVisualStateGroups(label, list);

			Assert.That(label.Margin.Bottom, Is.EqualTo(targetBottomMargin));
		}

		[Test]
		public void VisualElementGoesToCorrectStateWhenAvailableFromSetter()
		{
			double targetBottomMargin = 1.5;

			var group = new VisualStateGroup();
			var list = new VisualStateGroupList();

			var normalState = new VisualState { Name = NormalStateName };
			normalState.Setters.Add(new Setter { Property = View.MarginBottomProperty, Value = targetBottomMargin });

			var x = new Setter
			{
				Property = VisualStateManager.VisualStateGroupsProperty,
				Value = list
			};

			list.Add(group);
			group.States.Add(normalState);

			var label1 = new Label();
			var label2 = new Label();

			x.Apply(label1);
			x.Apply(label2);

			Assert.That(label1.Margin.Bottom, Is.EqualTo(targetBottomMargin));
			Assert.That(label2.Margin.Bottom, Is.EqualTo(targetBottomMargin));
		}

		[Test]
		public void VisualElementGoesToCorrectStateWhenSetterHasTarget()
		{
			double defaultMargin = default(double);
			double targetMargin = 1.5;

			var label1 = new Label();
			var label2 = new Label();
			INameScope nameScope = new NameScope();
			NameScope.SetNameScope(label1, nameScope);
			nameScope.RegisterName("Label1", label1);
			NameScope.SetNameScope(label2, nameScope);
			nameScope.RegisterName("Label2", label2);

			var list = new VisualStateGroupList
			{
				new VisualStateGroup
				{
					States =
					{
						new VisualState
						{
							Name = NormalStateName,
							Setters =
							{
								new Setter { Property = View.MarginBottomProperty, Value = targetMargin },
								new Setter { TargetName = "Label2", Property = View.MarginTopProperty, Value = targetMargin }
							}
						}
					}
				}
			};

			VisualStateManager.SetVisualStateGroups(label1, list);

			Assert.That(label1.Margin.Top, Is.EqualTo(defaultMargin));
			Assert.That(label1.Margin.Bottom, Is.EqualTo(targetMargin));
			Assert.That(label1.Margin.Left, Is.EqualTo(defaultMargin));

			Assert.That(label2.Margin.Top, Is.EqualTo(targetMargin));
			Assert.That(label2.Margin.Bottom, Is.EqualTo(defaultMargin));
		}

		[Test]
		public void CanRemoveAStateAndAddANewStateWithTheSameName()
		{
			var stateGroups = new VisualStateGroupList();
			var visualStateGroup = new VisualStateGroup { Name = CommonStatesName };
			var normalState = new VisualState { Name = NormalStateName };
			var invalidState = new VisualState { Name = InvalidStateName };

			stateGroups.Add(visualStateGroup);
			visualStateGroup.States.Add(normalState);
			visualStateGroup.States.Add(invalidState);

			var name = visualStateGroup.States[0].Name;

			visualStateGroup.States.Remove(visualStateGroup.States[0]);

			visualStateGroup.States.Add(new VisualState { Name = name });
		}

		[Test]
		public void CanRemoveAGroupAndAddANewGroupWithTheSameName()
		{
			var stateGroups = new VisualStateGroupList();
			var visualStateGroup = new VisualStateGroup { Name = CommonStatesName };
			var secondVisualStateGroup = new VisualStateGroup { Name = "Whatevs" };
			var normalState = new VisualState { Name = NormalStateName };
			var invalidState = new VisualState { Name = InvalidStateName };

			stateGroups.Add(visualStateGroup);
			visualStateGroup.States.Add(normalState);
			visualStateGroup.States.Add(invalidState);

			stateGroups.Add(secondVisualStateGroup);

			var name = stateGroups[0].Name;

			stateGroups.Remove(stateGroups[0]);

			stateGroups.Add(new VisualStateGroup { Name = name });
		}

		[SetUp]
		public void Setup()
		{
			AppInfo.SetCurrent(new MockAppInfo() { RequestedTheme = AppTheme.Light });
			Application.Current = new Application();
		}

		[TearDown]
		public void TearDown()
		{
			Application.Current = null;
		}

		[Test]
		//https://github.com/dotnet/maui/issues/6251
		public void AppThemeBindingInVSM()
		{

			var label = new Label() { BackgroundColor = Colors.Red };
			var list = new VisualStateGroupList
			{
				new VisualStateGroup
				{
					States =
					{
						new VisualState { Name = NormalStateName},
						new VisualState
						{
							Name = DisabledStateName,
							Setters =
							{
								new Setter { Property = View.BackgroundColorProperty, Value = new AppThemeBinding{ Light=Colors.Purple, Dark=Colors.Purple, Default=Colors.Purple } },
							}
						}
					}
				}
			};

			VisualStateManager.SetVisualStateGroups(label, list);

			Assert.That(label.BackgroundColor, Is.EqualTo(Colors.Red));
			VisualStateManager.GoToState(label, DisabledStateName);
			Assert.That(label.BackgroundColor, Is.EqualTo(Colors.Purple));
			VisualStateManager.GoToState(label, NormalStateName);
			Assert.That(label.BackgroundColor, Is.EqualTo(Colors.Red));
		}

		[Test]
		//https://github.com/dotnet/maui/issues/4139
		public void ChangingStyleContainingVSMShouldResetStateValue()
		{
			var label = new Label();
			var SelectedStateName = "Selected";

			var style0 = new Style(typeof(Label))
			{
				Setters = {
					new Setter { Property = Label.TextColorProperty, Value = Colors.Black},
					new Setter {
						Property = VisualStateManager.VisualStateGroupsProperty,
						Value = new VisualStateGroupList {
							new VisualStateGroup {
								States = {
									new VisualState { Name = NormalStateName },
									new VisualState {
										Name = SelectedStateName,
										Setters = { new Setter { Property = Label.TextColorProperty, Value=Colors.Red } }
									}
								}
							}
						}
					},
				}
			};

			var style1 = new Style(typeof(Label))
			{
				Setters = {
					new Setter { Property = Label.TextColorProperty, Value = Colors.Black},
					new Setter {
						Property = VisualStateManager.VisualStateGroupsProperty,
						Value = new VisualStateGroupList {
							new VisualStateGroup {
								States = {
									new VisualState { Name = NormalStateName },
									new VisualState {
										Name = SelectedStateName,
										Setters = { new Setter { Property = Label.TextColorProperty, Value=Colors.Cyan } }
									}
								}
							}
						}
					},
				}
			};

			label.Style = style0;
			Assert.That(label.TextColor, Is.EqualTo(Colors.Black));
			VisualStateManager.GoToState(label, SelectedStateName);
			Assert.That(label.TextColor, Is.EqualTo(Colors.Red));
			label.Style = style1;
			Assert.That(label.TextColor, Is.EqualTo(Colors.Black));
		}

		[Test]
		//https://github.com/dotnet/maui/issues/6857
		public void VSMFromStyleAreUnApplied()
		{
			var label = new Label
			{
				Style = new Style(typeof(Label))
				{
					Setters = {
						new Setter {
							Property = VisualStateManager.VisualStateGroupsProperty,
							Value = new VisualStateGroupList {
								new VisualStateGroup {
									States = {
										new VisualState { Name = NormalStateName },
									}
								}
							}
						},
					}
				}
			};

			Assert.That(VisualStateManager.GetVisualStateGroups(label), Is.Not.Null);
			Assert.That(VisualStateManager.GetVisualStateGroups(label).Count, Is.EqualTo(1)); //the list applied by style has one group			
			label.ClearValue(Label.StyleProperty); //clear the style
			Assert.That(VisualStateManager.GetVisualStateGroups(label).Count, Is.EqualTo(0)); //default style (created by defaultValueCreator) has no groups
		}

		[Test]
		//https://github.com/dotnet/maui/issues/6885
		public void UnapplyingVSMShouldUnapplySetters()
		{
			var label = new Label();
			var SelectedStateName = "Selected";

			VisualStateManager.SetVisualStateGroups(label, new VisualStateGroupList {
				new VisualStateGroup {
					States = {
						new VisualState {
							Name = SelectedStateName,
							Setters = { new Setter { Property=Label.TextColorProperty, Value=Colors.HotPink} }
						}
					}
				}
			});

			VisualStateManager.GoToState(label, SelectedStateName);
			Assert.That(label.TextColor, Is.EqualTo(Colors.HotPink));
			label.ClearValue(VisualStateManager.VisualStateGroupsProperty);
			Assert.That(VisualStateManager.GetVisualStateGroups(label).Count, Is.EqualTo(0)); //default style (created by defaultValueCreator) has no groups
			Assert.That(label.TextColor, Is.Not.EqualTo(Colors.HotPink)); //setter should be unapplied
		}

		[Test]
		//https://github.com/dotnet/maui/issues/6856
		public void VSMInStyleShouldHaveStylePriority()
		{
			var label = new Label { TextColor = Colors.HotPink };//Setting the color manually should prevents style override
			var SelectedStateName = "Selected";

			Assert.That(label.TextColor, Is.EqualTo(Colors.HotPink));

			label.Style = new Style(typeof(Label))
			{
				Setters = {
					new Setter { Property = Label.TextColorProperty, Value = Colors.AliceBlue },
					new Setter {
						Property = VisualStateManager.VisualStateGroupsProperty,
						Value = new VisualStateGroupList {
							new VisualStateGroup {
								States = {
									new VisualState {
										Name = SelectedStateName,
										Setters = { new Setter { Property = Label.TextColorProperty, Value=Colors.OrangeRed} }
									},
								}
							}
						}
					},
				}
			};

			Assert.That(label.TextColor, Is.EqualTo(Colors.HotPink)); //textcolor from Style isn't applied
			VisualStateManager.GoToState(label, SelectedStateName);
			Assert.That(label.TextColor, Is.EqualTo(Colors.HotPink)); //textcolor Style's VSM isn't applied
		}

		[Test]
		[Explicit("This test was created to check performance characteristics; leaving it in because it may be useful again.")]
		[TestCase(1, 10)]
		[TestCase(1, 10000)]
		[TestCase(10, 100)]
		[TestCase(10, 10000)]
		public void ValidatePerformance(int groups, int states)
		{
			IList<VisualStateGroup> vsgs = new VisualStateGroupList();

			var groupList = new List<VisualStateGroup>();

			for (int n = 0; n < groups; n++)
			{
				groupList.Add(new VisualStateGroup { Name = n.ToString() });
			}

			var watch = new Stopwatch();

			watch.Start();

			foreach (var group in groupList)
			{
				vsgs.Add(group);
			}

			watch.Stop();

			double iterations = states;
			var random = new Random();

			for (int n = 0; n < iterations; n++)
			{
				var state = new VisualState { Name = n.ToString() };
				var group = groupList[random.Next(0, groups - 1)];
				watch.Start();
				group.States.Add(state);
				watch.Stop();
			}

			var average = watch.ElapsedMilliseconds / iterations;

			Debug.WriteLine($">>>>> VisualStateManagerTests ValidatePerformance: {watch.ElapsedMilliseconds}ms over {iterations} iterations; average of {average}ms");

		}
	}
}