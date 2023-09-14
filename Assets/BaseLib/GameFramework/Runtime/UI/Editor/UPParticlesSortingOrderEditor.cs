
using UnityEditor;

namespace BaseLib.GameFramework.Runtime.UI.Editor
{
    [CustomEditor (typeof (UPParticlesSortingOrder))]
    public class UPParticlesSortingOrderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI ()
        {
            
            base.OnInspectorGUI ();
            var myTarget = (UPParticlesSortingOrder)target;
            
            myTarget.DrawButtons ();

        }
    }
}