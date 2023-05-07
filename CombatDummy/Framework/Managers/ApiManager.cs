using CombatDummy.Framework.Interfaces;
using StardewModdingAPI;

namespace CombatDummy.Framework.Managers
{
    internal class ApiManager
    {
        private IMonitor _monitor;
        private IDynamicGameAssetsApi _dynamicGameAssetsApi;

        public ApiManager(IMonitor monitor)
        {
            _monitor = monitor;
        }

        internal bool HookIntoDynamicGameAssets(IModHelper helper)
        {
            _dynamicGameAssetsApi = helper.ModRegistry.GetApi<IDynamicGameAssetsApi>("spacechase0.DynamicGameAssets");

            if (_dynamicGameAssetsApi is null)
            {
                _monitor.Log("Failed to hook into spacechase0.DynamicGameAssets.", LogLevel.Error);
                return false;
            }

            _monitor.Log("Successfully hooked into spacechase0.DynamicGameAssets.", LogLevel.Debug);
            return true;
        }

        public IDynamicGameAssetsApi GetDynamicGameAssetsApi()
        {
            return _dynamicGameAssetsApi;
        }
    }
}