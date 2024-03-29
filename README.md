# Post Process Animator
Post Process Animator is a utility class to easily animate Post-processing parameters using DOTween and reflection.

# Motivation
While this solution is not as "safe" as manipulating these parameters directly (by virtue of working with string) and might seem like overkill, it is super fast and easy to use. If you find yourself animating Post-processing values through code a lot, this class can help make your life a little easier by keeping track of existing tweens, simplifying your syntax, removing boilerplate and consolidating your dependencies.

*The "typical" way of animating a Lens Distortion's intensity:*
```csharp
public class SomeBehaviour : MonoBehaviour {
    [SerializeField] PostProcessVolume volume;
    [SerializeField] float ldTargetIntensity;
    [SerializeField] float ldTweenDuration;
    
    LensDistortion ld;
    float originalLdIntensity;
    Tween ldIntensityTween;
    
    void Awake() {
        // Make sure the volume's profile is not the original instance if you don't want to modify the Scriptable Object stored in your project.
        // Multiple scripts might be touching it, so you'd need a static or shared bool somewhere.
        if(Globals.OriginalPostProcessProfile == volume.profile) {
            volume.profile = Instantiate(volume.profile);
        }
        
        ld = volume.profile.GetSetting<LensDistortion>();
        
        // Potentially store the original values for later resets
        originalLdIntensity = ld.intensity;
    }
    
    void AnimateLDIntensity() {
        ldTween?.Kill();
        ldTween = DOVirtual.Float(ld.intensity.value, ldTargetIntensity, ldTweenDuration, x => ld.intensity.value = x);
    }
}
```

This doesn't look *too* bad, until you also want to do the same for its scale, requiring an added `float originalLdScale` and ` Tween ldScaleTween`, as well as killing and reassigning this new tween.

*Using Post Process Animator:*
```csharp
public class SomeBehaviour : MonoBehaviour {
    using BlueTomato;
    
    [SerializeField] PostProcessAnimator postProcessAnimator;
    [SerializeField] float ldTargetIntensity;
    [SerializeField] float ldTargetScale;
    [SerializeField] float ldTweenDuration;
    
    void AnimateLD() {
        var ld = postProcessAnimator.GetEffect<LensDistortion>();
        ld.Tween("intensity", ldTargetIntensity, ldTweenDuration);
        ld.Tween("scale", ldTargetScale, ldTweenDuration);
    }
}
```

Or if you want more control over your tweens:

```csharp
void AnimateLDIntensity() {
    Tween tween = DOVirtual.Float(0f, ldTargetIntensity, ldTweenSpeed, x => ld.settings.intensity.value = x).SetSpeedBased();

    var ld = postProcessAnimator.GetEffect<LensDistortion>();
    ld.Tween(tween, id: "intensity");
}
```

# How to use
Simply add `PostProcessAnimator.cs` to your project, and attach the `PostProcessAnimator` component to the same object as your Post Processing Volume. In your controlling script, make sure to add the `using BlueTomato` directive, and just start using the methods as shown above!

Each Post Process Animator Effect (retrieved through `postProcessAnimator.GetEffect<>()`) stores both its current settings, as well as it's original ones for easy access. For example, to reset the lens distortion intensity from the above example:

```csharp
void ResetLDIntensity() {
    var ld = postProcessAnimator.GetEffect<LensDistortion>();
    ld.Tween("intensity", ld.originalSettings.intensity.value, ldTweenDuration);
}
```

Starting a tween on a property automatically kills the existing one for that same property, if one exists. When passing in your own tween the `id` parameter can be used to accomplish this same behaviour (it is recommended to use the animated property's name as the id).
