/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"MainCameraShot.cs"
 * 
 *	A PlayableAsset that keeps track of which _Camera to cut to in the MainCameraMixer.  This is adapted from CinemachineShot.cs, published by Unity Technologies, and all credit goes to its respective authors.
 * 
 */

#if UNITY_2017_1_OR_NEWER

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace AC
{

	/**
	 * A PlayableAsset that keeps track of which _Camera to cut to in the MainCameraMixer.  This is adapted from CinemachineShot.cs, published by Unity Technologies, and all credit goes to its respective authors.
	 */
	public sealed class MainCameraShot : PlayableAsset
	{

		#region Variables

		public ExposedReference<_Camera> gameCamera;
		public float shakeIntensity;

		#endregion


		#region PublicFunctions

		public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
		{
			var playable = ScriptPlayable<MainCameraPlayableBehaviour>.Create (graph);
			playable.GetBehaviour ().gameCamera = gameCamera.Resolve (graph.GetResolver ());
			playable.GetBehaviour ().shakeIntensity = shakeIntensity;
			return playable;
		}

		#endregion

	}

}

#endif