// ============================================================================
// SYSTEM 1b — INTERCOM STORE ANNOUNCEMENT SYSTEM
// A store-wide, position-less audio channel. Plays queued announcements with
// a mandatory static/chime intro, then rebroadcasts them onto the event bus
// so gameplay systems can react to WHAT was said, not just that audio played.
//
// Canonical lines:
//   StoreOpening : "The store is now open. Welcome, valued customers!"
//   StoreClosing : "The store is now closed. Please make your way to the
//                   exits immediately."
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using InfiniteWalmart.Core;

namespace InfiniteWalmart.Environment
{
    [RequireComponent(typeof(AudioSource))]
    public class IntercomSystem : MonoBehaviour
    {
        [Header("Audio Bank — Opus populates clip table")]
        [SerializeField] private AudioClip staticIntroChime;
        [SerializeField] private List<AnnouncementClipEntry> clipBank = new();

        [System.Serializable]
        public struct AnnouncementClipEntry
        {
            public string ClipKey;
            public AudioClip Clip;
            [Range(0f, 1f)] public float DistortionAmount; // Night lines play warped.
        }

        private readonly Queue<IntercomAnnouncement> _queue = new();
        private AudioSource _source;
        private bool _isPlaying;

        private void Awake() => _source = GetComponent<AudioSource>();

        private void OnEnable() => GameEventBus.AnnouncementPlayed += Enqueue;
        private void OnDisable() => GameEventBus.AnnouncementPlayed -= Enqueue;

        private void Enqueue(IntercomAnnouncement announcement)
        {
            _queue.Enqueue(announcement);
            if (!_isPlaying)
                StartCoroutine(DrainQueue());
        }

        private System.Collections.IEnumerator DrainQueue()
        {
            _isPlaying = true;
            while (_queue.Count > 0)
            {
                var announcement = _queue.Dequeue();

                // 1. Static chime intro — the dread cue.
                if (staticIntroChime != null)
                {
                    _source.PlayOneShot(staticIntroChime);
                    yield return new WaitForSeconds(staticIntroChime.length);
                }

                // 2. The announcement body.
                var clip = ResolveClip(announcement.ClipKey);
                if (clip != null)
                {
                    // TODO(Opus): route through distortion filter chain using
                    // the entry's DistortionAmount during NightShift.
                    _source.PlayOneShot(clip);
                    yield return new WaitForSeconds(clip.length);
                }
            }
            _isPlaying = false;
        }

        private AudioClip ResolveClip(string key)
        {
            foreach (var entry in clipBank)
                if (entry.ClipKey == key)
                    return entry.Clip;
            return null;
        }
    }
}
