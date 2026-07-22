using System.Collections.Generic;
using System.Threading.Tasks;
using HelloDev.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = HelloDev.Logging.Logger;

namespace KingdomLike.Utils
{
    public class IDDatabaseBootstrap : MonoBehaviour, IBootstrapInitializable
    {
        private const string LogId = "IDs.Database";

        #region Data

        [FoldoutGroup("Database")] [ListDrawerSettings(ShowFoldout = true, DraggableItems = true)] [SerializeField]
        private List<ScriptableObjectWithID> _entries = new();

        #endregion

        #region Bootstrap

        [FoldoutGroup("Bootstrap")] [SerializeField]
        private bool _selfInitialize = true;

        private bool _isInitialized;
        private GameContext _context;

        #endregion

        #region Properties

        public bool SelfInitialize
        {
            get => _selfInitialize;
            set => _selfInitialize = value;
        }

        public bool IsInitialized => _isInitialized;

        #endregion

        #region Initialization

        private void OnEnable()
        {
            if (SelfInitialize && !IsInitialized)
                InitializeAsync();
        }

        public void ReceiveContext(GameContext context)
        {
            _context = context;
        }

        public Task InitializeAsync()
        {
            if (IsInitialized)
                return Task.CompletedTask;

            IDDatabase.Initialize(_entries);

            _isInitialized = true;

            Logger.LogVerbose(
                LogId,
                $"Initialized ID database with {IDDatabase.Count} entries.",
                this);

            return Task.CompletedTask;
        }

        #endregion

        #region Shutdown

        public void Shutdown()
        {
            IDDatabase.Clear();

            _isInitialized = false;

            Logger.LogVerbose(
                LogId,
                "ID database shut down.",
                this);
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            Shutdown();
        }

        #endregion
    }
}