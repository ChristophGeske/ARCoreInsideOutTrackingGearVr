using System.Collections;
using UnityEngine;

namespace DaydreamElements.Common.IconMenu {

  /// Describes the icon's current status
  public enum IconState {
    // icon is shown
    Shown,
    // icon greyed out
    Hidden,
    // icon not shown. It was just created or about to be destroyed.
    Closed,
    // icon is open and is highlighted
    Hovering,
  }

  /// Describes how two icons are connected.
  public enum IconRelationship {
    DescendantOfRhs,
    AncestorOfRhs,
    UnkownRelationship,
  };

  /// Wraps data used to gradually change values.
  public struct IconValue {
    /// Specify how quickly the value changes.
    public enum EasingFunction {
      Linear,
      EaseOutCubic
    };

    /// Construct and set to an initial value
    public IconValue(float initialValue, EasingFunction easingFunction =
                    EasingFunction.EaseOutCubic) {
      duration = 0.1f; //inital duration doesn't matter since the value is constant.
      finalValue = initialValue;
      startValue = initialValue;
      startTime = 0.0f;
      this.easingFunction = easingFunction;
    }

    /// Starts a fade from value at startTime to finalValue if needed.
    public void FadeTo(float finalValue, float duration, float startTime) {
      if (Mathf.Approximately(this.finalValue, finalValue)) {
        return;
      }
      this.startValue = ValueAtTime(startTime);
      this.startTime = startTime;
      this.finalValue = finalValue;
      this.duration = duration;
    }

    /// Returns the value at a given time.
    public float ValueAtTime(float atTime) {
      switch (easingFunction) {
      case EasingFunction.Linear:
        return Mathf.Lerp(startValue, finalValue, (atTime - startTime) / duration);
      case EasingFunction.EaseOutCubic:
      default:
        return CubicEaseOut(startValue, finalValue, (atTime - startTime) / duration);
      }
    }

    /// Helper function to calculate a cubic ease out.
    private static float CubicEaseOut(float initialValue, float finalValue, float progress)
    {
      return initialValue + (finalValue - initialValue) * (Mathf.Pow(Mathf.Clamp01(progress) -
                                                                        1.0f, 3) + 1.0f);
    }

    /// Compares atTime to startTime and duration, returns a value from 0 to 1 inclusive.
    public float GetProgress(float atTime) {
      return Mathf.Clamp01((atTime - startTime) / duration);
    }

    public float startTime;
    public float startValue;
    public float finalValue;
    public float duration;
    public EasingFunction easingFunction;
  };
}
