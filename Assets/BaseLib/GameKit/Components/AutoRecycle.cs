using UnityEngine;
using System.Collections;
using Shelter.Scripts.Utils;

namespace GameKit.Base
{
    [DisallowMultipleComponent]
    public class AutoRecycle : MonoBehaviour
    {
        public delegate void RecycleEvent();

        public float delay = 10;
        public bool autoDestroy = true;
        public RecycleEvent onRecycle;

        protected virtual IEnumerator DelayAction()
        {
            yield return YieldUtils.WaitForSeconds(delay);
            gameObject.Recycle();
        }

        // Use this for initialization
        protected void OnRecycle()
        {
            if (onRecycle != null)
            {
                onRecycle.Invoke();
                onRecycle = null;
            }

            gameObject.Recycle();
        }

        public void DelayRecycle(float delay)
        {
            this.delay = delay;
            Reset();
        }

        public void DelayDestroy(float delay)
        {
            CancelInvoke();
            Destroy(gameObject, delay);
        }

        public virtual void Reset()
        {
            CancelInvoke();
            autoDestroy = false;

            if (delay > 0f)
            {
                Invoke(nameof(OnRecycle), delay);
            }
            else
            {
                OnRecycle();
            }
        }

        private void OnEnable()
        {
            Reset();
        }

        private void OnDisable()
        {
            if (autoDestroy) Destroy(this);
        }
    }
}

