using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Media;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public abstract partial class HandlerTestBase<THandler, TStub>
	{
		[Fact(DisplayName = "Automation Id is set correctly")]
		[InlineData()]
		public async Task SetAutomationId()
		{
			var view = new TStub
			{
				AutomationId = "TestId"
			};
			var id = await GetValueAsync(view, handler => GetAutomationId(handler));
			Assert.Equal(view.AutomationId, id);
		}

		[Theory(DisplayName = "FlowDirection is set correctly")]
		[InlineData(FlowDirection.LeftToRight)]
		[InlineData(FlowDirection.RightToLeft)]
		public async Task SetFlowDirection(FlowDirection flowDirection)
		{
			var view = new TStub
			{
				FlowDirection = flowDirection
			};

			var expectedFlowDirection = flowDirection;

#if WINDOWS
			if (view is LayoutStub)
			{
				// On Windows, we deliberately _do not_ set the platform FlowDirection
				// for Layouts, because the cross-platform layout code is already taking
				// flow direction into account. The result of setting the cross-platform
				// flow direction will always be Microsoft.UI.Xaml.FlowDirection.LeftToRight
				expectedFlowDirection = FlowDirection.LeftToRight;
			}
#endif

			var id = await GetValueAsync(view, handler => GetFlowDirection(handler));
			Assert.Equal(expectedFlowDirection, id);
		}

		[Theory(DisplayName = "Opacity is set correctly")]
		[InlineData(0)]
		[InlineData(0.25)]
		[InlineData(0.5)]
		[InlineData(0.75)]
		[InlineData(1)]
		public async Task SetOpacity(double opacity)
		{
			var view = new TStub
			{
				Opacity = opacity
			};
			var id = await GetValueAsync(view, handler => GetOpacity(handler));
			Assert.Equal(view.Opacity, id);
		}

		[Theory(DisplayName = "Visibility is set correctly")]
		[InlineData(Visibility.Collapsed)]
		[InlineData(Visibility.Hidden)]
		public virtual async Task SetVisibility(Visibility visibility)
		{
			var view = new TStub
			{
				Visibility = visibility
			};

			var id = await GetValueAsync(view, handler => GetVisibility(handler));
			Assert.Equal(view.Visibility, id);
		}

		[Fact(DisplayName = "Setting Semantic Description makes element accessible")]
		public async Task SettingSemanticDescriptionMakesElementAccessible()
		{
			var view = new TStub();
			view.Semantics.Description = "Test";
			var important = await GetValueAsync(view, handler => GetIsAccessibilityElement(handler));
			Assert.True(important);
		}

		[Fact(DisplayName = "Setting Semantic Hint makes element accessible")]
		public async Task SettingSemanticHintMakesElementAccessible()
		{
			var view = new TStub();
			view.Semantics.Hint = "Test";
			var important = await GetValueAsync(view, handler => GetIsAccessibilityElement(handler));
			Assert.True(important);
		}

		[Fact(DisplayName = "Semantic Description is set correctly"
#if __ANDROID__
			, Skip = "This value can't be validated through automated tests"
#endif
		)]
		public async Task SetSemanticDescription()
		{
			var view = new TStub();
			view.Semantics.Description = "Test";
			var id = await GetValueAsync(view, handler => GetSemanticDescription(handler));
			Assert.Equal(view.Semantics.Description, id);
		}

		[Fact(DisplayName = "Semantic Hint is set correctly"
#if __ANDROID__
			, Skip = "This value can't be validated through automated tests"
#endif
		)]
		public async Task SetSemanticHint()
		{
			var view = new TStub();
			view.Semantics.Hint = "Test";
			var id = await GetValueAsync(view, handler => GetSemanticHint(handler));
			Assert.Equal(view.Semantics.Hint, id);
		}

		[Fact(DisplayName = "Semantic Heading is set correctly")]
		public async Task SetSemanticHeading()
		{
			var view = new TStub();
			view.Semantics.HeadingLevel = SemanticHeadingLevel.Level1;
			var id = await GetValueAsync(view, handler => GetSemanticHeading(handler));
			Assert.Equal(view.Semantics.HeadingLevel, id);
		}

		[Fact(DisplayName = "Null Semantics Doesnt throw exception")]
		public async Task NullSemanticsClass()
		{
			var view = new TStub
			{
				Semantics = null,
				AutomationId = "CreationFailed"
			};
			var id = await GetValueAsync(view, handler => GetAutomationId(handler));
			Assert.Equal(view.AutomationId, id);
		}

		[Fact(DisplayName = "Clip Initializes ContainerView Correctly")]
		public async Task ContainerViewInitializesCorrectly()
		{
			var view = new TStub
			{
				Height = 100,
				Width = 100,
				Background = new SolidPaintStub(Colors.Red),
				Clip = new EllipseGeometryStub(new Graphics.Point(50, 50), 50, 50)
			};

			var handler = await CreateHandlerAsync(view);

			Assert.NotNull(handler.ContainerView);
		}

		[Theory(DisplayName = "Native View Bounds are not empty")]
		[InlineData(1)]
		[InlineData(100)]
		[InlineData(1000)]
		public async Task ReturnsNonEmptyPlatformViewBounds(int size)
		{
			var view = new TStub()
			{
				Height = size,
				Width = size,
			};

			var platformViewBounds = await GetValueAsync(view, handler => GetPlatformViewBounds(handler));
			Assert.NotEqual(platformViewBounds, new Graphics.Rect());
		}

		[Theory(DisplayName = "Native View Bounding Box are not empty")]
		[InlineData(1)]
		[InlineData(100)]
		[InlineData(1000)]
		public async Task ReturnsNonEmptyNativeBoundingBounds(int size)
		{
			var view = new TStub()
			{
				Height = size,
				Width = size,
			};

			var nativeBoundingBox = await GetValueAsync(view, handler => GetBoundingBox(handler));
			Assert.NotEqual(nativeBoundingBox, new Graphics.Rect());
		}


		[Theory(DisplayName = "Native View Transforms are not empty"
#if __IOS__
			, Skip = "https://github.com/dotnet/maui/issues/3600"
#endif
			)]
		[InlineData(1)]
		[InlineData(100)]
		[InlineData(1000)]
		public async Task ReturnsNonEmptyPlatformViewTransforms(int size)
		{
			var view = new TStub()
			{
				Height = size,
				Width = size,
				Scale = .5,
				Rotation = size,
			};

			var platformViewTransform = await GetValueAsync(view, handler => GetViewTransform(handler));
			Assert.NotEqual(platformViewTransform, new System.Numerics.Matrix4x4());
		}

		[Theory(DisplayName = "View Renders To Image"
#if !__ANDROID__
			, Skip = "iOS and Windows can't render elements to images from test runner. It's missing the required root windows."
#endif
			)]
		[InlineData(ScreenshotFormat.Jpeg)]
		[InlineData(ScreenshotFormat.Png)]
		public async Task RendersAsImage(ScreenshotFormat type)
		{
			var view = new TStub()
			{
				Height = 100,
				Width = 100,
			};

			var result = await GetValueAsync(view, handler => handler.VirtualView.CaptureAsync());
			Assert.NotNull(result);

			using var stream = await result.OpenReadAsync(type);

			Assert.True(stream.Length > 0);
		}
	}
}
