#if PROTRACER_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace Andtech.ProTracer
{

	public partial class TracerObject
	{

		/// <summary>
		/// Asychronously draws the tracer between two points.
		/// </summary>
		/// <param name="from">The starting position.</param>
		/// <param name="to">The end position.</param>
		/// <param name="speed">How fast should the tracer move?</param>
		/// <param name="strobeOffset">How much of the tracer should be shown initially? (This eliminates the Wagon-Wheel effect)</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
		/// <remarks>Control is returned to the caller once the tracer has completed the entire animation.</remarks>
		/// <returns>N/A</returns>
		/// <seealso cref="AnimateLineAsync(Vector3, Vector3, float, float, CancellationToken)"/>
		/// <seealso cref="ShrinkAsync(CancellationToken)"/>
		public async UniTask DrawLineAsync(Vector3 from, Vector3 to, float speed, float strobeOffset = 0.0F, CancellationToken cancellationToken = default)
		{
			await AnimateLineAsync(from, to, speed, strobeOffset: strobeOffset, cancellationToken: cancellationToken);
			await ShrinkAsync();
		}

		/// <summary>
		/// Asynchronously draws the tracer with projectile motion.
		/// </summary>
		/// <param name="origin">The starting position.</param>
		/// <param name="direction">The initial direction of motion.</param>
		/// <param name="speed">How fast should the tracer move?</param>
		/// <param name="timeoutDistance">The maximum distance of the flight path.</param>
		/// <param name="strobeOffset">How much of the tracer should be shown initially? (This eliminates the Wagon-Wheel effect)</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
		/// <remarks>Control is returned to the caller once the tracer has completed the entire animation.</remarks>
		/// <returns>N/A</returns>
		/// <seealso cref="AnimateRayAsync(Vector3, Vector3, float, float, float, CancellationToken)"/>
		/// <seealso cref="ShrinkAsync(CancellationToken)"/>
		public async UniTask DrawRayAsync(Vector3 origin, Vector3 direction, float speed, float timeoutDistance = Mathf.Infinity, float strobeOffset = 0.0F, CancellationToken cancellationToken = default)
		{
			await AnimateRayAsync(origin, direction, speed, timeoutDistance: timeoutDistance, strobeOffset: strobeOffset, cancellationToken: cancellationToken);
			await ShrinkAsync();
		}

		/// <summary>
		/// Asynchronously draws the tracer by following (tracking) a point.
		/// </summary>
		/// <param name="readPosition">How should we obtain the position?</param>
		/// <param name="timeoutDistance">How far can the tracer travel before timing out?</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
		/// <remarks>Control is returned to the caller once the tracer has completed the entire animation.</remarks>
		/// <returns>N/A</returns>
		/// <seealso cref="AnimateTrackAsync(Func{Vector3}, float, CancellationToken)"/>
		/// <seealso cref="ShrinkAsync(CancellationToken)"/>
		public async UniTask TrackAsync(Func<Vector3> readPosition, float timeoutDistance = Mathf.Infinity, CancellationToken cancellationToken = default)
		{
			await AnimateTrackAsync(readPosition, timeoutDistance, cancellationToken: cancellationToken);
			await ShrinkAsync();
		}

		/// <summary>
		/// Asychronously draws the tracer between two points.
		/// </summary>
		/// <param name="from">The starting position.</param>
		/// <param name="to">The end position.</param>
		/// <param name="speed">How fast should the tracer move?</param>
		/// <param name="strobeOffset">How much of the tracer should be shown initially? (This eliminates the Wagon-Wheel effect)</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
		/// <remarks>Control is returned to the caller once the tracer has arrived at the destination.</remarks>
		/// <returns>N/A</returns>
		public async UniTask AnimateLineAsync(Vector3 from, Vector3 to, float speed, float strobeOffset = 0.0F, CancellationToken cancellationToken = default)
		{
			DrawLine(from, to, speed, strobeOffset: strobeOffset);

			await UniTask.WaitWhile(cachedIsAirborne, cancellationToken: cancellationToken);
		}

		/// <summary>
		/// Asynchronously draws the tracer with projectile motion.
		/// </summary>
		/// <param name="origin">The starting position.</param>
		/// <param name="direction">The initial direction of motion.</param>
		/// <param name="speed">How fast should the tracer move?</param>
		/// <param name="timeoutDistance">The maximum distance of the flight path.</param>
		/// <param name="strobeOffset">How much of the tracer should be shown initially? (This eliminates the Wagon-Wheel effect)</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
		/// <remarks>Control is returned to the caller once the tracer has arrived at the destination.</remarks>
		/// <returns>N/A</returns>
		public async UniTask AnimateRayAsync(Vector3 origin, Vector3 direction, float speed, float timeoutDistance = Mathf.Infinity, float strobeOffset = 0.0F, CancellationToken cancellationToken = default)
		{
			DrawRay(origin, direction, speed: speed, timeoutDistance: timeoutDistance, strobeOffset: strobeOffset);

			await UniTask.WaitWhile(cachedIsAirborne, cancellationToken: cancellationToken);
		}

		/// <summary>
		/// Asynchronously draws the tracer by following (tracking) a point.
		/// </summary>
		/// <param name="readPosition">How should we obtain the position?</param>
		/// <param name="timeoutDistance">How far can the tracer travel before timing out?</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
		/// <remarks>Control is returned to the caller once the tracer has arrived at the destination.</remarks>
		/// <returns>N/A</returns>
		public async UniTask AnimateTrackAsync(Func<Vector3> readPosition, float timeoutDistance = Mathf.Infinity, CancellationToken cancellationToken = default)
		{
			Track(readPosition, timeoutDistance: timeoutDistance);

			await UniTask.WaitWhile(cachedIsAirborne, cancellationToken: cancellationToken);
		}

		/// <summary>
		/// Forces the tracer to begin shrinking.
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <remarks>Control is returned to the caller once the tracer completely shrinks.</remarks>
		/// <returns>N/A</returns>
		public async UniTask ShrinkAsync(CancellationToken cancellationToken = default)
		{
			Shrink();

			await UniTask.WaitUntil(cachedIsDone, cancellationToken: cancellationToken);
		}
	}
}
#endif
