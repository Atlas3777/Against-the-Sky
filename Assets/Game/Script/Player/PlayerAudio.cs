using UnityEngine;

    public class PlayerAudio : MonoBehaviour
    {
        [Header("Audio Settings")]
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;

        private CharacterController _controller;
        private PlayerAnimation _animationController;

        public void Setup(CharacterController controller)
        {
            _controller = controller;
        }

        public void UpdateAudio()
        {
            // Аудио обновляется через Animation Events
        }

        public void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center),
                        FootstepAudioVolume);
                }
            }
        }

        public void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center),
                    FootstepAudioVolume);
            }
        }
    }
