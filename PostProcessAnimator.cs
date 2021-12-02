using System;
using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace BlueTomato {
    public class PostProcessAnimator : MonoBehaviour {
        public class Effect<T> where T : PostProcessEffectSettings {
            struct TweenProperties {
                public Action<object> setter;
                public Func<object> getter;

                public TweenProperties(FieldInfo valueField, object parameter) {
                    setter = x => valueField.SetValue(parameter, x);
                    getter = () => valueField.GetValue(parameter);
                }
            }

            public T settings;
            public T originalSettings;

            Dictionary<string, Tween> tweens;

            public void Init(PostProcessVolume volume) {
                settings = volume.profile.GetSetting<T>();
                originalSettings = Instantiate(volume.profile.GetSetting<T>());
                tweens = new Dictionary<string, Tween>();
            }

            public void Tween(Tween tween, bool disableOnComplete = false, string id = "") {
                ManageTweenForID(tween, id, disableOnComplete);
            }

            public void Tween(string property, float endValue, float duration, bool disableOnComplete = false) {
                TweenProperties tweenProps = GetTweenProperties<FloatParameter>(property);
                Tween tween = DOTween.To(() => (float)tweenProps.getter(), x => tweenProps.setter(x), endValue, duration);

                ManageTweenForID(tween, property, disableOnComplete);
            }
			
			public void Tween(string property, int endValue, float duration, bool disableOnComplete = false) {
                TweenProperties tweenProps = GetTweenProperties<IntParameter>(property);
                Tween tween = DOTween.To(() => (int)tweenProps.getter(), x => tweenProps.setter(x), endValue, duration);

                ManageTweenForID(tween, property, disableOnComplete);
            }

            public void Tween(string property, Color endValue, float duration, bool disableOnComplete = false) {
                TweenProperties tweenProps = GetTweenProperties<ColorParameter>(property);
                Tween tween = DOTween.To(() => (Color)tweenProps.getter(), x => tweenProps.setter(x), endValue, duration);

                ManageTweenForID(tween, property, disableOnComplete);
            }

            void ManageTweenForID(Tween tween, string id, bool disableOnComplete) {
                if(id != string.Empty) {
                    if(tweens.ContainsKey(id)) {
                        tweens[id]?.Kill();
                        tweens[id] = tween;
                    }
                    else {
                        tweens.Add(id, tween);
                    }
                }

                if(disableOnComplete) {
                    TweenCallback oldOnComplete = tween.onComplete;

                    tween.OnComplete(() => {
                        oldOnComplete?.Invoke();
                        settings.active = false;
                    });
                }
            }

            TweenProperties GetTweenProperties<U>(string fieldName) {
                var parameterField = typeof(T).GetField(fieldName);
                var parameterObject = parameterField.GetValue(settings);

                return new TweenProperties(typeof(U).GetField("value"), parameterObject);
            }
        }

        PostProcessVolume volume;
        Dictionary<System.Type, object> effects;

        void Awake() {
            volume = GetComponent<PostProcessVolume>();
            volume.profile = Instantiate(volume.profile);
            effects = new Dictionary<System.Type, object>();

            foreach(var setting in volume.profile.settings) {
                System.Type TSetting = setting.GetType();
                System.Type TEffect = typeof(Effect<>).MakeGenericType(TSetting);
                dynamic effect = Activator.CreateInstance(TEffect);

                effect.Init(volume);
                effects.Add(TSetting, effect);
            }
        }

        public Effect<T> GetEffect<T>() where T : PostProcessEffectSettings {
            return effects[typeof(T)] as Effect<T>;
        }
    }
}
