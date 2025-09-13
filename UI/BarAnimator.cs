using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.UI
{
    /// <summary>
    /// Smoothly animates ProgressBar value (0..100) and adds status classes "ok", "warn", "danger"
    /// based on configured thresholds. Can optionally pulse when in critical (danger) state.
    /// </summary>
    public class BarAnimator : MonoBehaviour
    {
        [SerializeField] private float warnThreshold = 0.3f;
        [SerializeField] private float dangerThreshold = 0.15f;
        [SerializeField] private float defaultDuration = 0.25f;
        [SerializeField] private bool pulseWhenCritical = true;

        private Coroutine _running;

        /// <summary>
        /// Animates a ProgressBar to the specified target in [0..1] over the given duration (seconds).
        /// </summary>
        public void AnimateTo(ProgressBar bar, float targetValue, float? duration = null)
        {
            if (bar == null) return;
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(Animate(bar, targetValue, duration ?? defaultDuration));
        }

        private IEnumerator Animate(ProgressBar bar, float targetValue, float duration)
        {
            targetValue = Mathf.Clamp01(targetValue);

            float start = Mathf.Clamp01(bar.value / 100f);
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duration);
                float v01 = Mathf.Lerp(start, targetValue, EaseOutQuad(k));
                bar.value = v01 * 100f; // ProgressBar expects 0..100
                UpdateStatusClasses(bar, v01);
                yield return null;
            }
            bar.value = targetValue * 100f;
            UpdateStatusClasses(bar, targetValue);

            if (pulseWhenCritical && targetValue <= dangerThreshold)
                yield return Pulse(bar, 0.92f, 1.06f, 0.4f);

            _running = null;
        }

        private void UpdateStatusClasses(ProgressBar bar, float v01)
        {
            bar.RemoveFromClassList("ok");
            bar.RemoveFromClassList("warn");
            bar.RemoveFromClassList("danger");
            if (v01 <= dangerThreshold) bar.AddToClassList("danger");
            else if (v01 <= warnThreshold) bar.AddToClassList("warn");
            else bar.AddToClassList("ok");
        }

        private static float EaseOutQuad(float x) => 1f - (1f - x) * (1f - x);

        private IEnumerator Pulse(VisualElement ve, float minScale, float maxScale, float time)
        {
            float t = 0;
            while (t < time)
            {
                t += Time.deltaTime;
                float k = Mathf.PingPong(t * 2f, 1f);
                float s = Mathf.Lerp(minScale, maxScale, k);
                ve.style.scale = new Scale(new Vector3(s, s, 1f));
                yield return null;
            }
            ve.style.scale = new Scale(Vector3.one);
        }
    }
}