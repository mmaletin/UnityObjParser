
using UnityEngine;

public static class BoundsUtils {

	public static Bounds CalculateCombinedBounds(this GameObject gameObject, bool useRenderers = true, bool includeInactive = false)
    {
        Bounds bounds = new Bounds();
        bool boundsInitialized = false;

        if (useRenderers)
        {
            foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>())
            {
                if ((renderer.enabled && renderer.gameObject.activeInHierarchy) || includeInactive)
                {
                    if (!boundsInitialized)
                    {
                        bounds = renderer.bounds;
                        boundsInitialized = true;
                    }
                    else
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }
            }
        }
        else
        {
            foreach (var collider in gameObject.GetComponentsInChildren<Collider>())
            {
                if ((collider.enabled && collider.gameObject.activeInHierarchy) || includeInactive)
                {
                    if (!boundsInitialized)
                    {
                        bounds = collider.bounds;
                        boundsInitialized = true;
                    }
                    else
                    {
                        bounds.Encapsulate(collider.bounds);
                    }
                }
            }
        }

        return bounds;
    }
}