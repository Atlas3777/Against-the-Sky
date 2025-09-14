#if PROTRACER_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace Andtech.ProTracer
{

	public partial class Bullet
	{

		/// <summary>
		/// Asynchronously draws the tracer with projectile motion.
		/// </summary>
		/// <param name="origin">The starting position.</param>
		/// <param name="direction">The initial direction of motion.</param>
		/// <param name="speed">How fast should the tracer move?</param>
		/// <param name="timeoutDistance">The maximum distance of the flight path.</param>
		/// <param name="strobeOffset">How much of the tracer should be shown initially? (This eliminates the Wagon-Wheel effect)</param>
		/// <param name="useGravity">Should the tracer obey gravity?</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
		/// <remarks>Control is returned to the caller once the tracer has completed the entire animation.</remarks>
		/// <returns>N/A</returns>
		public virtual async UniTask DrawRayAsync(Vector3 origin, Vector3 direction, float speed, float timeoutDistance = float.PositiveInfinity, float strobeOffset = 0.0F, bool useGravity = true, CancellationToken cancellationToken = default)
		{
			await AnimateRayAsync(origin, direction, speed, timeoutDistance: timeoutDistance, strobeOffset: strobeOffset, useGravity: useGravity, cancellationToken: cancellationToken);
			await ShrinkAsync(cancellationToken);
		}

		/// <summary>
		/// Asynchronously draws the tracer with projectile motion.
		/// </summary>
		/// <param name="origin">The starting position.</param>
		/// <param name="direction">The initial direction of motion.</param>
		/// <param name="speed">How fast should the tracer move?</param>
		/// <param name="timeoutDistance">The maximum distance of the flight path.</param>
		/// <param name="strobeOffset">How much of the tracer should be shown initially? (This eliminates the Wagon-Wheel effect)</param>
		/// <param name="useGravity">Should the tracer obey gravity?</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
		/// <remarks>Control is returned to the caller once the tracer has arrived at the destination.</remarks>
		/// <returns>N/A</returns>
		public async UniTask AnimateRayAsync(Vector3 origin, Vector3 direction, float speed, float timeoutDistance = float.PositiveInfinity, float strobeOffset = 0.0F, bool useGravity = true, CancellationToken cancellationToken = default)
		{
			DrawRay(origin, direction, speed: speed, timeoutDistance: timeoutDistance, strobeOffset: strobeOffset, useGravity: useGravity);

			await UniTask.WaitWhile(cachedIsAirborne, cancellationToken: cancellationToken);
		}
	}
}
#endif
