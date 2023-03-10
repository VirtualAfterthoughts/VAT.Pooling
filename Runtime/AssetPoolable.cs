using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using VAT.Shared;
using VAT.Shared.Extensions;
using VAT.Shared.Utilities;

namespace VAT.Pooling {
    public delegate void AssetPoolableDelegate(AssetPoolable poolable);
    public delegate void AssetSpawnDelegate(AssetPoolable poolable, ulong id);

    public class AssetPoolable : CachedMonoBehaviour {
        public static ComponentCache<AssetPoolable> Cache { get; private set; } = new ComponentCache<AssetPoolable>();

        internal AssetPoolableDelegate Internal_PoolSpawnDelegate { get; set; }
        internal AssetPoolableDelegate Internal_PoolDespawnDelegate { get; set; }

        public AssetSpawnDelegate OnSpawnDelegate { get; set; }
        public AssetPoolableDelegate OnDespawnDelegate { get; set; }

        private bool _isLocked = false;
        public bool IsLocked => _isLocked;

        private Transform _initialParent;

        public virtual bool CanSpawn {
            get { return !_isLocked; }
        }

        public virtual bool CanDespawn {
            get { return true; }
        }

        private ulong _id;
        public ulong ID => _id;

        private void Awake() {
            Cache.Add(gameObject, this);
            _initialParent = Transform.parent;
        }

        private void OnDestroy() {
            Cache.Remove(gameObject);
        }

        internal virtual void OnSpawn(ulong id) {
            _id = id;
            OnSpawnDelegate?.Invoke(this, id);
            Internal_PoolSpawnDelegate?.Invoke(this);
        }

        /// <summary>
        /// Despawns the poolable.
        /// </summary>
        public void Despawn() {
            if (!CanDespawn)
                return;

            OnDespawn();
        }

        internal virtual void OnDespawn() {
            _id = 0;
            gameObject.SetActive(false);
            OnDespawnDelegate?.Invoke(this);
            Internal_PoolDespawnDelegate?.Invoke(this);

            Transform.EnsureParent(_initialParent);
        }

        /// <summary>
        /// Locks the poolable, preventing it from being reused.
        /// </summary>
        public void Lock() {
            _isLocked = true;
        }

        /// <summary>
        /// Unlocks the poolable and allows it to be reused.
        /// </summary>
        public void Unlock() {
            _isLocked = false;
        }

        /// <summary>
        /// Flags this poolable so that it is counted as "despawned" and may be reused.
        /// </summary>
        public void FlagForRespawning() {
            Internal_PoolDespawnDelegate?.Invoke(this);
        }

        /// <summary>
        /// Flags this poolable so that it is counted as "spawned".
        /// </summary>
        public void FlagForDespawning() {
            Internal_PoolSpawnDelegate?.Invoke(this);
        }
    }
}
